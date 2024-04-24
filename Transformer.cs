using Google.Cloud.AIPlatform.V1;
using Microsoft.Extensions.Options;

namespace CodeTransform;

public class Transformer
{
    private readonly TransformerOptions _options;
    private readonly string _model;
    private readonly PredictionServiceClient _predictionServiceClient;
    private readonly HttpClient _httpClient;
    private readonly ILogger<Transformer> _logger;


    public Transformer(IOptions<TransformerOptions> options, 
        PredictionServiceClient predictionServiceClient,
        HttpClient httpClient,
        ILogger<Transformer> logger)
    {
        _options = options.Value;
        
        if (string.IsNullOrEmpty(_options.ProjectId))
            throw new Exception("Missing configuration variable: projectId");

        _model = $"projects/{_options.ProjectId}/locations/{_options.LocationId}/publishers/google/models/{_options.ModelId}";
        
        _predictionServiceClient = predictionServiceClient;

        _httpClient = httpClient;

        _logger = logger;
    }

    /// <summary>
    /// Executes a code transformation given a prompt and the url of a file.
    /// </summary>
    /// <example>"Generate a random quote from a fictional person."</example>
    public async Task<string> GenerateAsync(string prompt, string filename)
    {
        var fileContents = await GetFileContents(filename);

        var enrichedPrompt = $"{prompt}\n\n<code>\n{fileContents}\n</code>";

        var result = await GenerateTextAsync(enrichedPrompt);

        return result;
    }

    /// <summary>
    /// Invokes the Vertex AI Model to generate text.
    /// </summary>
    /// <param name="textPrompt">Your prompt</param>
    /// <param name="temperature">0.0 - 1.0 how creative the model should be</param>
    /// <returns></returns>
    private async Task<string> GenerateTextAsync(string textPrompt)
    {
        var generationConfig = new GenerationConfig() 
        { 
            CandidateCount = 1, 
            MaxOutputTokens = 8192, 
            Temperature = 0.2f, 
            TopP = 1
        };

        var content = new Content() { Role = "USER" };
        content.Parts.Add(new Part() { Text = textPrompt });

        var request = new GenerateContentRequest
        {
            Contents = { content, },
            GenerationConfig = generationConfig,
            Model = _model,
        };

        var response = await _predictionServiceClient.GenerateContentAsync(request);
        var text = response.Candidates.First().Content.Parts.First().Text;

        return text;
    }

    private async Task<string> GetFileContents(string url)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(url);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file contents from {url}", url);
            throw new ApplicationException("Error getting file contents", ex);
        }
    }
}
