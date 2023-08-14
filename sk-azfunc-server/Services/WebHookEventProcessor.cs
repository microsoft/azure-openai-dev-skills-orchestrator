﻿using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Newtonsoft.Json;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.IssueComment;
using Octokit.Webhooks.Events.Issues;
using Octokit.Webhooks.Models;
using Microsoft.SKDevTeam;

public class SKWebHookEventProcessor : WebhookEventProcessor
{
    private readonly IKernel _kernel;
    private readonly ILogger<SKWebHookEventProcessor> _logger;
    private static HttpClient httpClient = new HttpClient();
    private readonly GithubService _ghService;

    public SKWebHookEventProcessor(IKernel kernel, ILogger<SKWebHookEventProcessor> logger, GithubService ghService)
    {
        _kernel = kernel;
        _logger = logger;
        _ghService = ghService;
    }
    protected override async Task ProcessIssuesWebhookAsync(WebhookHeaders headers, IssuesEvent issuesEvent, IssuesAction action)
    {
        var ghClient = await _ghService.GetGitHubClient();
        var org = issuesEvent.Organization.Login;
        var repo = issuesEvent.Repository.Name;
        var issueNumber = issuesEvent.Issue.Number;
        var input = issuesEvent.Issue.Body;
        if (issuesEvent.Action == IssuesAction.Opened)
        {
            // Assumes the label follows the following convention: Skill.Function example: PM.Readme
            var labels = issuesEvent.Issue.Labels.First().Name.Split(".");
            var skillName = labels[0];
            var functionName = labels[1];
            if (skillName == "Do" && functionName == "It")
            {
                var issueOrchestrationRequest = new IssueOrchestrationRequest
                {
                    Number = issueNumber,
                    Org = org,
                    Repo = repo,
                    Input = input
                };
                var content = new StringContent(JsonConvert.SerializeObject(issueOrchestrationRequest), Encoding.UTF8, "application/json");
                // TODO: remove hardcoded url, read from config
                _ = await httpClient.PostAsync("http://localhost:7071/api/doit", content);
            }
            else
            {
                var result = await RunSkill(skillName, functionName, input);
                await ghClient.Issue.Comment.Create(org, repo, (int)issueNumber, result);
            }
        }
        else if (issuesEvent.Action == IssuesAction.Closed && issuesEvent.Issue.User.Type.Value == UserType.Bot)
        {
            // TODO: remove hardcoded url, read from config
            var metadata = await httpClient.GetFromJsonAsync<IssueMetadata>($"http://localhost:7071/api/metadata/{issueNumber}");
            var closeIssueRequest = new CloseIssueRequest { InstanceId = metadata.InstanceId, CommentId = metadata.CommentId, Org = org, Repo = repo };
            var content = new StringContent(JsonConvert.SerializeObject(closeIssueRequest), Encoding.UTF8, "application/json");
            // TODO: remove hardcoded url, read from config
            _ = await httpClient.PostAsync("http://localhost:7071/api/close", content).ConfigureAwait(false);
        }
    }

    protected override async Task ProcessIssueCommentWebhookAsync(
        WebhookHeaders headers,
        IssueCommentEvent issueCommentEvent,
        IssueCommentAction action)
    {
        // we only resond to non-bot comments
        if (issueCommentEvent.Sender.Type.Value != UserType.Bot)
        {
            var ghClient = await _ghService.GetGitHubClient();
            var org = issueCommentEvent.Organization.Login;
            var repo = issueCommentEvent.Repository.Name;
            var issueId = issueCommentEvent.Issue.Number;


            // Assumes the label follows the following convention: Skill.Function example: PM.Readme
            var labels = issueCommentEvent.Issue.Labels.First().Name.Split(".");
            var skillName = labels[0];
            var functionName = labels[1];
            var input = issueCommentEvent.Comment.Body;
            var result = await RunSkill(skillName, functionName, input);

            await ghClient.Issue.Comment.Create(org, repo, (int)issueId, result);
        }
    }

    private async Task<string> RunSkill(string skillName, string functionName, string input)
    {
        var skillConfig = SemanticFunctionConfig.ForSkillAndFunction(skillName, functionName);
        var function = _kernel.CreateSemanticFunction(skillConfig.PromptTemplate, skillConfig.Name, skillConfig.SkillName,
                                                   skillConfig.Description, skillConfig.MaxTokens, skillConfig.Temperature,
                                                   skillConfig.TopP, skillConfig.PPenalty, skillConfig.FPenalty);

        var interestingMemories = _kernel.Memory.SearchAsync("waf-pages", input, 2);
        var wafContext = "Consider the following architectural guidelines:";
        await foreach (var memory in interestingMemories)
        {
            wafContext += $"\n {memory.Metadata.Text}";
        }

        var context = new ContextVariables();
        context.Set("input", input);
        context.Set("wafContext", wafContext);

        var result = await _kernel.RunAsync(context, function).ConfigureAwait(false);
        return result.ToString();
    }
}