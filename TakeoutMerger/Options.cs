namespace TakeoutMerger;

public class RootCommandOptions(string message)
{
    public string Message { get; } = message;
}

public class Options(string inputPath, string outputPath)
{
    public string InputPath { get; } = inputPath;
    public string OutputPath { get; } = outputPath;
}