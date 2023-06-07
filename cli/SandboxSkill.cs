using DotNet.Testcontainers.Builders;
using Microsoft.SemanticKernel.SkillDefinition;

public class SandboxSkill
{
    [SKFunction("Run a script in Alpine sandbox")]
    [SKFunctionInput(Description = "The script to be executed")]
    [SKFunctionName("RunInAlpine")]
    public async Task<string> RunInAlpineAsync(string input)
    {
        return await RunInContainer(input, "alpine");
    }

    [SKFunction("Run a script in dotnet alpine sandbox")]
    [SKFunctionInput(Description = "The script to be executed")]
    [SKFunctionName("RunInDotnetAlpine")]
    public async Task<string> RunInDotnetAlpineAsync(string input)
    {
        return await RunInContainer(input, "mcr.microsoft.com/dotnet/sdk:7.0");
    }

    private async Task<string> RunInContainer(string input, string image)
    {
        var tempScriptFile = $"{Guid.NewGuid().ToString()}.sh";
        var tempScriptPath = $"./tmp/{tempScriptFile}";
        await File.WriteAllTextAsync(tempScriptPath, input);
        var dotnetContainer = new ContainerBuilder()
                            .WithName(Guid.NewGuid().ToString("D"))
                            .WithImage(image)
                            .WithBindMount(Path.Combine(Directory.GetCurrentDirectory(), "src"), "/src")
                            .WithBindMount(Path.Combine(Directory.GetCurrentDirectory(), tempScriptPath), $"/src/{tempScriptFile}")
                            .WithWorkingDirectory("/src")
                            .WithCommand("sh", tempScriptFile)
                            .Build();

        await dotnetContainer.StartAsync()
                            .ConfigureAwait(false);
        // Cleanup
        File.Delete(tempScriptPath);
        File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "src", tempScriptFile));
        return "";
    }
}