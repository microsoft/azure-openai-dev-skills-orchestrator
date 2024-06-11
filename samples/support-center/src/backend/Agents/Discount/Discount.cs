using Microsoft.AI.Agents.Abstractions;
using Microsoft.AI.Agents.Orleans;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Orleans.Runtime;
using SupportCenter.Agents;
using SupportCenter.Events;
using SupportCenter.Options;
using static Microsoft.AI.Agents.Orleans.Resolvers;

namespace Marketing.Agents;

[ImplicitStreamSubscription(Consts.OrleansNamespace)]
public class Discount : AiAgent<DiscountState>
{
    protected override string Namespace => Consts.OrleansNamespace;
    protected override string Name => nameof(Discount);

    private readonly ILogger<Discount> _logger;

    public Discount([PersistentState("state", "messages")] IPersistentState<AgentState<DiscountState>> state,
        ILogger<Discount> logger,
        KernelResolver kernelResolver,
        SemanticTextMemoryResolver memoryResolver)
    : base(state, kernelResolver, memoryResolver)
    {
        _logger = logger;
    }

    public async override Task HandleEvent(Event item)
    {
        switch (item.Type)
        {
            case nameof(EventType.UserConnected):
                // The user reconnected, let's send the last message if we have one
                string? lastMessage = _state.State.History.LastOrDefault()?.Message;
                if (lastMessage == null)
                {
                    return;
                }
                //await SendDispatcherEvent(userId, lastMessage, item.Data["userId"]);
                break;
            default:
                break;
        }
    }
}