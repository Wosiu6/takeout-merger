using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Utils;
using ZLogger;

namespace TakeoutMerger.Core.Services;

public interface IFileService
{
    List<string> GetFilesByExtensions(string directoryPath, ICollection<string> extensions, SearchOption searchOption = SearchOption.AllDirectories, bool excludeExtensions = false);
    Dictionary<string, string> GetFileDataMatches(string directoryPath, List<string> foundFiles);
}

public class FileService(ILogger<FileService> logger) : IFileService
{
    private readonly ILogger _logger = logger;
    
    public List<string> GetFilesByExtensions(string directoryPath, ICollection<string> extensions, SearchOption searchOption = SearchOption.AllDirectories, bool excludeExtensions = false)
    {
        _logger.ZLogInformation($"Searching in: {directoryPath}");

        List<string> filesList = [];
        IEnumerable<string> allFiles = Directory.EnumerateFiles(directoryPath, "*.*", searchOption);

        FrozenSet<string> lowerCaseExtensions = extensions.Select(ext => ext.ToLowerInvariant()).ToFrozenSet();

        foreach (string file in allFiles)
        {
            string? fileExtension = Path.GetExtension(file)?.ToLowerInvariant();

            bool shouldAddFile;

            if (string.IsNullOrEmpty(fileExtension))
            {
                shouldAddFile = excludeExtensions;
            }
            else
            {
                bool containsExtension = lowerCaseExtensions.Contains(fileExtension);
                shouldAddFile = excludeExtensions == !containsExtension;
            }

            if (shouldAddFile)
            {
                filesList.Add(file);
            }
        }

        _logger.ZLogInformation($"Found {filesList.Count} files; extensions: {string.Join(",", extensions)} in {directoryPath}");
        return filesList;
    }

    public Dictionary<string, string> GetFileDataMatches(string directoryPath, List<string> foundFiles)
    {
        _logger.ZLogInformation($"Matching files with JSONs in: {directoryPath}");

        Dictionary<string, string> fileJsonMap = [];
        HashSet<string> usedJsonFiles = [];

        List<string> potentialJsonFiles = Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.AllDirectories).ToList();

        if (potentialJsonFiles.Count == 0)
        {
            return [];
        }

        int progress = 0;
        int totalFiles = foundFiles.Count;

        foreach (string foundFile in foundFiles)
        {
            _logger.ZLogInformation($"Matching JSON: {++progress}/{totalFiles}: {foundFile}");

            // try starts with
            var match = potentialJsonFiles.Where(x => !usedJsonFiles.Contains(x) && x.StartsWith(foundFile)).FirstOrDefault();

            if (match != null)
            {
                fileJsonMap[foundFile] = match;
                _logger.ZLogInformation($"Found match for {foundFile}: {match}");
                continue;
            }

            string foundFileNameWithoutExtension = Path.GetFileNameWithoutExtension(foundFile);
            string foundFileName = Path.GetFileName(foundFile);

            string? exactMatch = potentialJsonFiles
                .FirstOrDefault(jsonFile =>
                    !usedJsonFiles.Contains(jsonFile) &&
                    (Path.GetFileNameWithoutExtension(jsonFile).Equals(foundFileNameWithoutExtension, StringComparison.OrdinalIgnoreCase) ||
                     Path.GetFileName(jsonFile).Equals(foundFileName, StringComparison.OrdinalIgnoreCase)));

            if (exactMatch != null)
            {
                fileJsonMap[foundFile] = exactMatch;
                usedJsonFiles.Add(exactMatch);
                _logger.ZLogInformation($"Found exact match for {foundFile}: {exactMatch}");
                continue;
            }

            // Try Levenshtein
            string? closestMatch = null;
            int minDistance = int.MaxValue;

            foreach (string jsonFile in potentialJsonFiles)
            {
                int distance = MathUtils.CalcLevenshteinDistance(foundFile, jsonFile);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestMatch = jsonFile;
                }
            }

            if (closestMatch != null)
            {
                fileJsonMap[foundFile] = closestMatch;
                _logger.ZLogInformation($"Found closest match for {foundFile}: {closestMatch}");
            }
        }

        _logger.ZLogInformation($"Matched {fileJsonMap.Count} files in {directoryPath}");
        return fileJsonMap;
    }
}