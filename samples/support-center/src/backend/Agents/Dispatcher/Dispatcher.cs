using Microsoft.AI.Agents.Abstractions;
using Microsoft.AI.Agents.Orleans;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Orleans.Runtime;
using SupportCenter.Attributes;
using SupportCenter.Events;
using SupportCenter.Extensions;
using SupportCenter.Options;
using System.Reflection;

namespace SupportCenter.Agents;

[ImplicitStreamSubscription(Consts.OrleansNamespace)]
[DispatcherChoice("QnA", "The customer is asking a question. When the request is generic or can't be classified differently, use this choice.", EventType.QnARequested)]
[DispatcherChoice("Discount", "The customer is asking for a discount about a product or service.", EventType.DiscountRequested)]
[DispatcherChoice("Invoice", "The customer is asking for an invoice.", EventType.InvoiceRequested)]
[DispatcherChoice("Customer Info", "The customer is asking for reading or updating his or her information or profile.", EventType.CustomerInfoRequested)]
public class Dispatcher : AiAgent<DispatcherState>
{
    protected override string Namespace => Consts.OrleansNamespace;
    private readonly ILogger<Dispatcher> _logger;

    public Dispatcher(
        [PersistentState("state", "messages")] IPersistentState<AgentState<DispatcherState>> state,
        Kernel kernel,
        ISemanticTextMemory memory,
        ILogger<Dispatcher> logger)
    : base(state, memory, kernel)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async override Task HandleEvent(Event item)
    {
        _logger.LogInformation("[{Dispatcher}] Event {EventType}. Data: {EventData}", nameof(Dispatcher), item.Type, item.Data);

        string? messageId = item.Data.GetValueOrDefault<string>("id");
        string? userId = item.Data.GetValueOrDefault<string>("userId");
        string? userMessage = item.Data.GetValueOrDefault<string>("userMessage");
        string? intent;

        switch (item.Type)
        {
            case nameof(EventType.UserConnected):
                // The user reconnected, let's send the last message if we have one.
                string? lastMessage = _state.State.History.LastOrDefault()?.Message;
                if (lastMessage == null)
                {
                    _logger.LogInformation("[{Dispatcher}] Event {EventType}. Data: {EventData}. Last message is missing.", nameof(Dispatcher), item.Type, item.Data);
                    return;
                }
                if (userId == null)
                {
                    _logger.LogError("[{Dispatcher}] Event {EventType}. Data: {EventData}. User ID is missing.", nameof(Dispatcher), item.Type, item.Data);
                    return;
                }
                intent = await ExtractIntentAsync(lastMessage);
                await SendDispatcherEvent(messageId, userId, intent, lastMessage);
                break;
            case nameof(EventType.UserChatInput):

                intent = await ExtractIntentAsync(userMessage);
                await SendDispatcherEvent(messageId, userId, intent, userMessage);
                break;
            case nameof(EventType.QnARetrieved):
            case nameof(EventType.DiscountRetrieved):
            case nameof(EventType.InvoiceRetrieved):
            case nameof(EventType.CustomerInfoRetrieved):
                var answer = item.Data.GetValueOrDefault<string>("answer");
                _logger.LogInformation($"[{nameof(Dispatcher)}] Event {nameof(EventType.QnARetrieved)}. Answer: {item.Data["answer"]}");
                AddToHistory(answer, ChatUserType.Agent);
                break;
            default:
                break;
        }
    }


    private async Task<string> ExtractIntentAsync(string message)
    {
        var input = AppendChatHistory(message);

        var context = new KernelArguments { ["input"] = input };
        context.Add("choices", GetAndSerializeChoices());
        return await CallFunction(DispatcherPrompts.GetIntent, context);
    }

    private string GetAndSerializeChoices()
    {
        var choices = this.GetType().GetCustomAttributes<DispatcherChoice>()
            .Select(attr => new Choice(attr.Name, attr.Description))
            .ToArray();

        return string.Join("\n", choices.Select(c => $"- {c.Name}: {c.Description}")); ;
    }

    private async Task SendDispatcherEvent(string id, string userId, string intent, string userMessage)
    {
        var type = this.GetType()
            .GetCustomAttributes<DispatcherChoice>()
            .FirstOrDefault(attr => attr.Name == intent)?.DispatchToEvent.ToString() ?? EventType.Unknown.ToString();

        await PublishEvent(Consts.OrleansNamespace, this.GetPrimaryKeyString(), new Event
        {
            Type = type,
            Data = new Dictionary<string, string>
            {
                { nameof(id), id },
                { nameof(userId), userId },
                { nameof(userMessage), userMessage },
            }
        });
    }
}