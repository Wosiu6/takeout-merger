using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Utils;
using ZLogger;

namespace TakeoutMerger.Core.Handlers.Metadata;

public interface IFileMetadataMatcher
{
    Dictionary<string, string> GetFileDataMatches(string directoryPath);
}

public class FileMetadataMetadataMatcher(ILogger<MetadataApplier> logger) : IFileMetadataMatcher
{
    private readonly ILogger _logger = logger;
    
    public Dictionary<string, string> GetFileDataMatches(string directoryPath)
    {
        _logger.ZLogInformation($"Matching files with JSONs in: {directoryPath}");

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
            if (match == null) continue;
            fileJsonMap[mediaFile] = match;
            _logger.ZLogInformation($"Found match for {mediaFile}: {match}");
        }

        _logger.ZLogInformation($"Matched {fileJsonMap.Count} files in {directoryPath}");
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
            if (distance >= minDistance) continue;
            
            minDistance = distance;
            closestMatch = jsonFile;
        }

        return closestMatch;
    }
}