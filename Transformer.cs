using Google.Cloud.AIPlatform.V1;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Text;

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
    public async Task<string> GenerateAsync(TransformRequest request)
    {
        string fileContents;

        switch (request.SourceType)
        {
            case SourceType.File:
                fileContents = await GetFileContents(request.SourceUrl);
                break;
            case SourceType.Repository:
                fileContents = await GetFileContentsFromGit(request.SourceUrl);
                break;
            default:
                throw new NotImplementedException();
        }       

        var enrichedPrompt = $"{request.Prompt}\n\n<code>\n{fileContents}\n</code>";

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

    private async Task<string> GetFileContentsFromGit(string url)
    {
        //var url = $"https://api.github.com/repos/{owner}/{repo}/zipball/{reference}";

        var extractionDir = $"./bin/extracted"; // {owner}/{repo}/{reference}";
        var sb = new StringBuilder();

        try
        {
             // Required by github otherwise a 403 is returned
            _httpClient.DefaultRequestHeaders.Add("User-Agent", _options.GithubUser);

            using var response = await _httpClient.GetStreamAsync(url);

            ZipFile.ExtractToDirectory(response, extractionDir);

            foreach (var file in GetFilesRecursive(extractionDir, ["*.cs*", "*.config"]))
            {
                string code = await File.ReadAllTextAsync(file);

                string text = $"<filename>{file}</filename>\n<code>\n{code}\n</code>";
                sb.Append(text);
            }

            return sb.ToString();
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file contents from {url}", url);
            throw new ApplicationException("Error getting file contents", ex);
        }
        finally
        {
            if (Directory.Exists(extractionDir))
            {
                Directory.Delete(extractionDir, true);
            }
        }
    }

    private static IEnumerable<string> GetFilesRecursive(string directory, string[] filePattern)
    {
        var files = new List<string>();

        filePattern.ToList().ForEach(pattern =>
        {
            files.AddRange(Directory.GetFiles(directory, pattern));
        });

        foreach (var dir in Directory.GetDirectories(directory))
        {
            files.AddRange(GetFilesRecursive(dir, filePattern));
        }

        return files;
    }
}
