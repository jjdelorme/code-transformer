using System.Text;

public class CodeContextBuilder
{
    public static async Task<string> GetFileContents(string directory)
    {
        var stats = new FileStats();
        var sb = new StringBuilder();

        foreach (var file in GetFilesRecursive(directory, "*.cs*"))
        {
            string code = await File.ReadAllTextAsync(file);
            stats.Add(code.Length);

            string text = $"<filename>{file}</filename>\n<code>{code}</code>";
            sb.Append(text);
        }        

        Console.WriteLine(stats);

        return sb.ToString();
    }

    private static string[] GetFilesRecursive(string directory, string filePattern)
    {
        var files = new List<string>();

        foreach (var file in Directory.GetFiles(directory, filePattern))
        {
            files.Add(file);
        }

        foreach (var dir in Directory.GetDirectories(directory))
        {
            files.AddRange(GetFilesRecursive(dir, filePattern));
        }

        return files.ToArray();
    }

    internal class FileStats
    {
        public int Files { get; private set; }
        public double TotalCharacters { get; private set; }
        public double AverageCharactersPerFile { get; private set; }
        public int EstimatedTokens => TotalCharacters > 0 ? 
            (int)Math.Ceiling(TotalCharacters / 4) : 0;
                
        internal void Add(int chars)
        {
            TotalCharacters += chars;
            Files++;
            AverageCharactersPerFile = Math.Ceiling(TotalCharacters / Files);
        }

        public override string ToString()
        {
            return $"Files: {Files}\nTotal Characters: {TotalCharacters}\nAverage Characters Per File: {AverageCharactersPerFile}\nEstimated Tokens: {EstimatedTokens}";
        }
    }
}
