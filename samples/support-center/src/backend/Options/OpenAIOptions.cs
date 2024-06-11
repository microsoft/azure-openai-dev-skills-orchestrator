using System.ComponentModel.DataAnnotations;

namespace SupportCenter.Options;

public class OpenAIOptions
{
    // Embeddings
    [Required]
    public string EmbeddingsEndpoint { get; set; } = string.Empty;
    [Required]
    public string EmbeddingsApiKey { get; set; } = string.Empty;
    [Required]
    public string EmbeddingsDeploymentOrModelId { get; set; } = string.Empty;

    // Chat
    [Required]
    public string ChatEndpoint { get; set; } = string.Empty;
    [Required]
    public string ChatApiKey { get; set; } = string.Empty;
    [Required]
    public string ChatDeploymentOrModelId { get; set; } = string.Empty;

    // TextToImage
    /*[Required]
    public string? ImageEndpoint { get; set; }
    [Required]
    public string? ImageApiKey { get; set; }
    // When using OpenAI, this is not required.
    public string? ImageDeploymentOrModelId { get; set; }*/
}