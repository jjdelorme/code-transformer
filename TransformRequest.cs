namespace CodeTransform;

public enum SourceType
{
    File,
    Repository
}

public record TransformRequest(string Prompt, string SourceUrl, SourceType SourceType);
