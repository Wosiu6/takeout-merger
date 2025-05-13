using google_takeout_exif_fix;
using google_takeout_exif_fix.EXIFDataWriters;

if (args.Length < 2)
{
    Console.WriteLine("Usage: YourAppName.exe <directoryPath> <extension1> [extension2 ...]");
    Console.WriteLine("Example: YourAppName.exe C:\\MyFolder .txt .log .csv");
    return;
}

string directoryPath = args[0];

if (!Directory.Exists(directoryPath))
{
    Console.WriteLine($"Error: Directory not found at '{directoryPath}'");
    return;
}

List<string> extensionsToFind = [];
for (int i = 1; i < args.Length; i++)
{
    string ext = args[i];
    if (!ext.StartsWith("."))
    {
        ext = "." + ext;
    }
    extensionsToFind.Add(ext.ToLowerInvariant());
}

if (extensionsToFind.Count == 0)
{
    Console.WriteLine("Error: No file extensions specified.");
    return;
}

Console.WriteLine($"Searching in: {directoryPath}");
Console.WriteLine($"For extensions: {string.Join(", ", extensionsToFind)}");

List<string> foundPngs = FileHandler.GetFilesByExtensions(directoryPath, [ ".png" ]);
Console.WriteLine($"Found {foundPngs.Count} pngs to duplicate");

foreach (var pngsFile in foundPngs)
{
    // PngConverter.ConvertPngToJpeg(pngsFile);
}

List<string> foundJpegs = FileHandler.GetFilesByExtensions(directoryPath, [".jpg", ".jpeg"]);

Console.WriteLine($"Found {foundJpegs.Count} jpgs to fix");

var jpgJsonPairs = FileHandler.MatchFilesWithJsonsFuzzy(directoryPath, foundJpegs);

foreach (var jpgJsonPair in jpgJsonPairs)
{
    ExifJpegWriter.WritePngMetadata(jpgJsonPair.Value, jpgJsonPair.Key);
}
