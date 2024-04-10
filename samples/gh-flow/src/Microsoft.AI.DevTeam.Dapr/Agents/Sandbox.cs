﻿using CloudNative.CloudEvents;
using Dapr.Actors.Runtime;
using Microsoft.AI.Agents.Dapr;
using Microsoft.AI.DevTeam.Events;

namespace Microsoft.AI.DevTeam;
public class Sandbox : Agent
{
     protected override string Namespace => Consts.MainNamespace;
    private const string ReminderName = "SandboxRunReminder";
    private readonly IManageAzure _azService;
   // private readonly IReminderRegistry _reminderRegistry;
   // private IGrainReminder? _reminder;

    //protected readonly IPersistentState<SandboxMetadata> _state;

    public Sandbox(ActorHost host,IManageAzure azService) : base(host)
    {
        _azService = azService;
    }

    public override async Task HandleEvent(CloudEvent item)
    {
        switch(item.Type)
       {
           case nameof(GithubFlowEventType.SandboxRunCreated):
            {

            }
                break;
            //    var org = item.Data["org"];
            //    var repo = item.Data["repo"];
            //    var parentIssueNumber = long.Parse(item.Data["parentNumber"]);
            //    var issueNumber = long.Parse(item.Data["issueNumber"]);
            //    await ScheduleCommitSandboxRun(org, repo, parentIssueNumber, issueNumber);
           
           default:
               break;
       }
    }

    public async Task ScheduleCommitSandboxRun(string org, string repo, long parentIssueNumber, long issueNumber)
    {
        // await StoreState(org, repo, parentIssueNumber, issueNumber);
        // _reminder = await _reminderRegistry.RegisterOrUpdateReminder(
        //     callingGrainId: this.GetGrainId(),
        //     reminderName: ReminderName,
        //     dueTime: TimeSpan.Zero,
        //     period: TimeSpan.FromMinutes(1));
    }

    // async Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
    // {
    //     // if (!_state.State.IsCompleted)
    //     // {
    //     //     var sandboxId =  $"sk-sandbox-{_state.State.Org}-{_state.State.Repo}-{_state.State.ParentIssueNumber}-{_state.State.IssueNumber}";
    //     //     if (await _azService.IsSandboxCompleted(sandboxId))
    //     //     {
    //     //         await _azService.DeleteSandbox(sandboxId);
    //     //         await PublishEvent(Consts.MainNamespace, this.GetPrimaryKeyString(), new Event
    //     //         {
    //     //             Type = nameof(GithubFlowEventType.SandboxRunFinished),
    //     //             Data = new Dictionary<string, string> {
    //     //                 { "org", _state.State.Org },
    //     //                 { "repo", _state.State.Repo },
    //     //                 { "issueNumber", _state.State.IssueNumber.ToString() },
    //     //                 { "parentNumber", _state.State.ParentIssueNumber.ToString() }
    //     //             }
    //     //         });
    //     //         await Cleanup();
    //     //     }
    //     // }
    //     // else
    //     // {
    //     //     await Cleanup();
    //     // }
    // }

    private async Task StoreState(string org, string repo, long parentIssueNumber, long issueNumber)
    {
        // _state.State.Org = org;
        // _state.State.Repo = repo;
        // _state.State.ParentIssueNumber = parentIssueNumber;
        // _state.State.IssueNumber = issueNumber;
        // _state.State.IsCompleted = false;
        // await _state.WriteStateAsync();
    }

   

    // private async Task Cleanup()
    // {
    //     _state.State.IsCompleted = true;
    //     await _reminderRegistry.UnregisterReminder(
    //         this.GetGrainId(), _reminder);
    //     await _state.WriteStateAsync();
    // }
}


public class SandboxMetadata
{
    public string Org { get; set; }
    public string Repo { get; set; }
    public long ParentIssueNumber { get; set; }
    public long IssueNumber { get; set; }
    public bool IsCompleted { get; set; }
}