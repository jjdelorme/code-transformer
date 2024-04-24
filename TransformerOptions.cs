namespace CodeTransform;

public class TransformerOptions
{
    public const string Transformer = "Transformer";
    
    public string? ProjectId { get; set; }
    public string? ModelId { get; set; }
    public string? LocationId { get; set; }

    public static TransformerOptions FromConfiguration(ConfigurationManager config)
    {
        return config.GetSection(Transformer).Get<TransformerOptions>();
    }
}
