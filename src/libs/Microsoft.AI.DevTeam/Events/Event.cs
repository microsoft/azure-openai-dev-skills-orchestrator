
[GenerateSerializer]
public class Event
{
    [Id(0)]
    public string Message { get; set; }
    [Id(1)]
    public Dictionary<string,string> Data { get; set; }
}

[GenerateSerializer]
public class GithubFlowEvent : Event
{
    [Id(0)]
    public EventType Type { get; set; }
}

public enum EventType
{
    NewAsk,
    ReadmeChainClosed,
    CodeChainClosed,
    CodeGenerationRequested,
    DevPlanRequested,
    ReadmeGenerated,
    DevPlanGenerated,
    CodeGenerated,
    DevPlanChainClosed,
    ReadmeRequested,
    ReadmeStored,
    SandboxRunFinished,
    ReadmeCreated,
    CodeCreated,
    DevPlanCreated,
    SandboxRunCreated
}