using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Utils;
using TakeoutMerger.Core.Services;

namespace TakeoutMerger.Core.Handlers;

public interface IFileMetadataMatcher
{
    Dictionary<string, string> GetFileDataMatches(string directoryPath);
}

public class FileMetadataMetadataMatcher(ILogger<MetaDataApplier> logger) : IFileMetadataMatcher
{
    private readonly ILogger _logger = logger;
    
    public Dictionary<string, string> GetFileDataMatches(string directoryPath)
    {
        _logger.LogInformation("Matching files with JSONs in: {DirectoryPath}", directoryPath);

        var fileJsonMap = new Dictionary<string, string>();
        var mediaFiles = FileUtils.GetFilesExceptExtensions(directoryPath, [".json"]);
        var metadataFiles = Directory.EnumerateFiles(directoryPath, "*.json", 
            SearchOption.TopDirectoryOnly).ToFrozenSet();

        if (metadataFiles.Count == 0)
        {
            _logger.LogWarning("No JSON metadata files found in {DirectoryPath}", directoryPath);
            return fileJsonMap;
        }

        foreach (var mediaFile in mediaFiles)
        {
            var match = FindMatchingJsonFile(mediaFile, metadataFiles);
            if (match != null)
            {
                fileJsonMap[mediaFile] = match;
                _logger.LogInformation("Found match for {MediaFile}: {JsonFile}", mediaFile, match);
            }
        }

        _logger.LogInformation("Matched {FileCount} files in {DirectoryPath}", 
            fileJsonMap.Count, directoryPath);
        return fileJsonMap;
    }

    private string? FindMatchingJsonFile(string mediaFile, 
        IReadOnlySet<string> metadataFiles)
    {
        var prefixMatch = metadataFiles.FirstOrDefault(x => 
            x.StartsWith(mediaFile, StringComparison.OrdinalIgnoreCase));

        if (prefixMatch != null)
        {
            return prefixMatch;
        }

        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mediaFile);
        string fileName = Path.GetFileName(mediaFile);

        var exactMatch = metadataFiles.FirstOrDefault(jsonFile =>
            (Path.GetFileNameWithoutExtension(jsonFile)
                .Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase) ||
             Path.GetFileName(jsonFile)
                .Equals(fileName, StringComparison.OrdinalIgnoreCase)));

        if (exactMatch != null)
        {
            return exactMatch;
        }

        string? closestMatch = null;
        int minDistance = int.MaxValue;

        foreach (var jsonFile in metadataFiles)
        {
            int distance = MathUtils.CalcLevenshteinDistance(mediaFile, jsonFile);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestMatch = jsonFile;
            }
        }

        return closestMatch;
    }
}