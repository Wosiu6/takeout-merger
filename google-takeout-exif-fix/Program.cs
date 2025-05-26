using takeout_merger_p;
using takeout_merger_p.EXIFDataWriters;

if (args.Length < 1)
{
    Console.WriteLine("Usage: YourAppName.exe <directoryPath> <extension1> [extension2 ...]");
    Console.WriteLine("Example: YourAppName.exe C:\\MyFolder .txt .log .csv");
    return;
}

string directoryPath = args[0];
string outputPath = args[1];

ValidateUserInput();
SetupHandlers();


List<string> foundPngs = FileHandler.GetFilesByExtensions(directoryPath, [".png"]);

var pngTakeoutPairs = FileHandler.MatchFilesWithJsonsFuzzy(directoryPath, foundPngs);

foreach (var pngsFile in foundPngs)
{
    PngConverter.ConvertPngToJpeg(pngsFile);
}

return;

//List<string> foundFiles = FileHandler.GetFilesByExtensions(directoryPath, [".json", ".png"], exclude: true);
List<string> foundFiles = FileHandler.GetFilesByExtensions(directoryPath, [".json", ".png"], exclude: true);

var takoutPairs = FileHandler.MatchFilesWithJsonsFuzzy(directoryPath, foundFiles);

TakeoutDuplicator.DuplicatePairs(takoutPairs);

await ExifImageWriter.WriteImageMetadata(takoutPairs);

void ValidateUserInput()
{
    if (!Directory.Exists(directoryPath))
    {
        Console.WriteLine($"Error: Directory not found at '{directoryPath}'");
        return;
    }

    if (!Directory.Exists(outputPath))
    {
        Console.WriteLine($"Output directory not found at '{outputPath}'. Creating it now.");
        Directory.CreateDirectory(outputPath);
    }
}

void SetupHandlers()
{
    TakeoutDuplicator.OutputPath = outputPath;
    PngConverter.OutputPath = outputPath;
    ExifImageWriter.OutputPath = outputPath;
}
