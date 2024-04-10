﻿using System.Text.Json;
using CloudNative.CloudEvents;
using Dapr.Actors;
using Dapr.Actors.Runtime;
using Dapr.Client;
using Microsoft.AI.Agents.Dapr;
using Microsoft.AI.DevTeam.Dapr.Events;
using Newtonsoft.Json.Linq;

namespace Microsoft.AI.DevTeam.Dapr;

public class Hubber : Agent, IDoStuffWithGithub
{
    private readonly IManageGithub _ghService;

    public Hubber(ActorHost host, DaprClient client, IManageGithub ghService) :base(host, client)
    {
        _ghService = ghService;
    }

    public override async Task HandleEvent(CloudEvent item)
    {
        switch (item.Type)
        {
            case nameof(GithubFlowEventType.NewAsk):
                {
                    var data = (JObject)item.Data;
                    var parentNumber = long.Parse(data["issueNumber"].ToString());
                    var org = data["org"].ToString();
                    var repo = data["repo"].ToString();
                    var input = data["input"].ToString();
                    var pmIssue = await CreateIssue(org, repo , input, "PM.Readme", parentNumber);
                    var devLeadIssue = await CreateIssue(org, repo , input, "DevLead.Plan", parentNumber);
                    await PostComment(org, repo, parentNumber, $" - #{pmIssue} - tracks PM.Readme");
                    await PostComment(org, repo, parentNumber, $" - #{devLeadIssue} - tracks DevLead.Plan");   
                    await CreateBranch(org, repo, $"sk-{parentNumber}");
                }
                break;
            case nameof(GithubFlowEventType.ReadmeGenerated):
            case nameof(GithubFlowEventType.DevPlanGenerated):
            case nameof(GithubFlowEventType.CodeGenerated):
                {
                    var data = (JObject)item.Data;
                    var result = data["result"].ToString();
                    var org = data["org"].ToString();
                    var repo = data["repo"].ToString();
                    var issueNumber = long.Parse(data["issueNumber"].ToString());
                    var contents = string.IsNullOrEmpty(result)? "Sorry, I got tired, can you try again please? ": result;
                    await PostComment(org,repo, issueNumber, contents);
                }
               
                break;
            case nameof(GithubFlowEventType.DevPlanCreated):
                {
                    var data = (JObject)item.Data;
                    var parentNumber = long.Parse(data["parentNumber"].ToString());
                    var org = data["org"].ToString();
                    var repo = data["repo"].ToString();
                    var plan = JsonSerializer.Deserialize<DevLeadPlanResponse>(data["plan"].ToString());
                    var prompts = plan.steps.SelectMany(s => s.subtasks.Select(st => st.prompt));
                    
                    foreach (var prompt in prompts)
                    {
                        var functionName = "Developer.Implement";
                        var issue = await CreateIssue(org, repo, prompt, functionName, parentNumber);
                        var commentBody = $" - #{issue} - tracks {functionName}";
                        await PostComment(org, repo, parentNumber, commentBody);
                    }
                }
                break;
            case nameof(GithubFlowEventType.ReadmeStored):
                {
                    var data = (JObject)item.Data;
                    var parentNumber = long.Parse(data["parentNumber"].ToString());
                    var issueNumber = long.Parse(data["issueNumber"].ToString());
                    var org = data["org"].ToString();
                    var repo = data["repo"].ToString();
                    var branch = $"sk-{parentNumber}";
                    await CommitToBranch(org, repo, parentNumber, issueNumber, "output", branch);
                    await CreatePullRequest(org, repo, parentNumber, branch);
                }
                break;
            case nameof(GithubFlowEventType.SandboxRunFinished):
                {
                    var data = (JObject)item.Data;
                    var org = data["org"].ToString();
                    var repo = data["repo"].ToString();
                    var parentNumber = long.Parse(data["parentNumber"].ToString());
                    var issueNumber = long.Parse(data["issueNumber"].ToString());
                    var branch = $"sk-{parentNumber}";
                    await CommitToBranch(org, repo, parentNumber, issueNumber, "output", branch);
                }
                break;
            default:
                break;
        }
    }

    public async Task<int> CreateIssue(string org, string repo, string input, string function, long parentNumber)
    {
        return await _ghService.CreateIssue(org, repo, input, function, parentNumber);
    }
    public async Task PostComment(string org, string repo, long issueNumber, string comment)
    {
        await _ghService.PostComment(org, repo, issueNumber, comment);
    }
    public async Task CreateBranch(string org, string repo, string branch)
    {
        await _ghService.CreateBranch(org, repo, branch);
    }
    public async Task CreatePullRequest(string org, string repo, long issueNumber, string branch)
    {
        await _ghService.CreatePR(org, repo, issueNumber, branch);
    }
    public async Task CommitToBranch(string org, string repo, long parentNumber, long issueNumber, string rootDir, string branch)
    {
        await _ghService.CommitToBranch(org, repo, parentNumber, issueNumber, rootDir, branch);
    }
}

public interface IDoStuffWithGithub : IActor
{
    Task<int> CreateIssue(string org, string repo, string input, string function, long parentNumber);
    Task PostComment(string org, string repo, long issueNumber, string comment);
    Task CreateBranch(string org, string repo, string branch);
    Task CreatePullRequest(string org, string repo, long issueNumber, string branch);
    Task CommitToBranch(string org, string repo, long parentNumber, long issueNumber, string rootDir, string branch);
}