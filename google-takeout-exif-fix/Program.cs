using google_takeout_exif_fix;
using google_takeout_exif_fix.EXIFDataWriters;

if (args.Length < 1)
{
    Console.WriteLine("Usage: YourAppName.exe <directoryPath> <extension1> [extension2 ...]");
    Console.WriteLine("Example: YourAppName.exe C:\\MyFolder .txt .log .csv");
    return;
}

string directoryPath = args[0];
string outputPath = args[1];

if (!Directory.Exists(directoryPath))
{
    Console.WriteLine($"Error: Directory not found at '{directoryPath}'");
    return;
}

Console.WriteLine($"Searching in: {directoryPath}");

List<string> foundPngs = FileHandler.GetFilesByExtensions(directoryPath, [ ".png" ]);
Console.WriteLine($"Found {foundPngs.Count} pngs to duplicate");

foreach (var pngsFile in foundPngs)
{
    PngConverter.ConvertPngToJpeg(pngsFile);
}

List<string> foundFiles = FileHandler.GetFilesByExtensions(directoryPath, [".json"], exclude: true);

Console.WriteLine($"Found {foundFiles.Count} files to fix");

var jpgJsonPairs = FileHandler.MatchFilesWithJsonsFuzzy(directoryPath, foundFiles);

if (jpgJsonPairs == null)
{
    Console.WriteLine("No JSON files found to match with JPEGs.");
    return;
}

ExifImageWriter.OutputPath = outputPath;

foreach (var jpgJsonPair in jpgJsonPairs)
{
    ExifImageWriter.WriteImageMetadata(jpgJsonPair.Value, jpgJsonPair.Key);
}
