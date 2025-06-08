using takeout_merger_p;
using System.Collections.Concurrent;

if (args.Length < 1)
{
    Console.WriteLine("Usage: takeout_merger_p <directoryPath> <outputPath>]");
    Console.WriteLine("Example: takeout_merger_p C:\\Downloads\\Takeout C:\\Downloads\\MyOutputFolder");
    return;
}

string directoryPath = args[0];
string outputPath = args[1];
int currentProgress = 0;

ValidateWorkingDirectory();
SetupHandlers();

List<string> foundTagTypesPaths = FileHandler.GetFilesByExtensions(directoryPath, [".tiff", ".jpg", ".jpeg"]);
List<string> foundPngPaths = FileHandler.GetFilesByExtensions(directoryPath, [".png"]);
List<string> foundUnsuportedPaths = FileHandler.GetFilesByExtensions(directoryPath, [".tiff", ".jpg", ".jpeg", ".png", ".json"], exclude: true);


// PNGS
Dictionary<string, string>? pngTakeoutPairs = FileHandler.MatchFilesWithJsonsFuzzy(directoryPath, foundPngPaths);
ConcurrentDictionary<string, string> concurrentFizzyPng = new(pngTakeoutPairs);

foreach (var pngTakeoutPair in pngTakeoutPairs)
{

    var newPath = PngToTiffConverter.Convert(pngTakeoutPair.Key, CompressionMode.None);

    var newNameNoExtension = Path.GetFileNameWithoutExtension(newPath);

    ImageMetaHandler.ApplyJsonDataToImage(newPath, pngTakeoutPair.Value);

    Console.WriteLine("Applying Json to PNGs {0}/{1}: {2}",
                    ++currentProgress, pngTakeoutPairs.Count, pngTakeoutPair.Key);
}

// JPGs/JPEGs/TIFFs
Dictionary<string, string>? foundTagTypesTakeoutPairs = FileHandler.MatchFilesWithJsonsFuzzy(directoryPath, foundTagTypesPaths);
currentProgress = 0;

foreach (var foundTagTypesTakeoutPair in foundTagTypesTakeoutPairs)
{
    var fileNameNoExt = Path.GetFileNameWithoutExtension(foundTagTypesTakeoutPair.Key);
    var fileExtension = Path.GetExtension(foundTagTypesTakeoutPair.Key);

    var newPath = FileHandler.GetUniqueFileName($"{outputPath}\\{fileNameNoExt}{fileExtension}");
    File.Copy(foundTagTypesTakeoutPair.Key, newPath);

    ImageMetaHandler.ApplyJsonDataToTagImage(newPath, foundTagTypesTakeoutPair.Value);

    Console.WriteLine("Applying Json to Tags {0}/{1}: {2}",
                    ++currentProgress, foundTagTypesTakeoutPairs.Count, foundTagTypesTakeoutPair.Key);
}

// simple copy
currentProgress = 0;

foreach (var foundVideo in foundVideos)
{
    var fileNameNoExt = Path.GetFileNameWithoutExtension(foundVideo);
    var fileExtension = Path.GetExtension(foundVideo);

    var newPath = FileHandler.GetUniqueFileName($"{outputPath}\\{fileNameNoExt}{fileExtension}");

    File.Copy(foundVideo, newPath);

    Console.WriteLine("Copying movies {0}/{1}: {2}",
                    ++currentProgress, foundVideos.Count, foundVideo);
}

void ValidateWorkingDirectory()
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
    PngToTiffConverter.OutputPath = outputPath;
    ImageMetaHandler.OutputPath = outputPath;
}
