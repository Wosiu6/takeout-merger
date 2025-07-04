﻿using Microsoft.Extensions.Logging;
using System.Collections.Frozen;
using TakeoutMerger.src.Common.Utils;

namespace TakeoutMerger.src.Core.Services
{
    public interface IFileService
    {
        List<string> GetFilesByExtensions(string directoryPath, ICollection<string> extensions, SearchOption searchOption = SearchOption.AllDirectories, bool excludeExtensions = false);
        Dictionary<string, string> GetFileDataMatches(string directoryPath, List<string> foundFiles);
    }

    public class FileService(ILogger logger) : LoggableBase(logger), IFileService
    {
        public List<string> GetFilesByExtensions(string directoryPath, ICollection<string> extensions, SearchOption searchOption = SearchOption.AllDirectories, bool excludeExtensions = false)
        {
            Logger.LogInformation($"Searching in: {directoryPath}");

            List<string> filesList = [];
            IEnumerable<string> allFiles = Directory.EnumerateFiles(directoryPath, "*.*", searchOption);

            ICollection<string> lowerCaseExtensions = extensions.Select(ext => ext.ToLowerInvariant()).ToFrozenSet();

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

            Logger.LogInformation($"Found {filesList.Count} files by {string.Join(",", extensions)}");
            return filesList;
        }

        public Dictionary<string, string> GetFileDataMatches(string directoryPath, List<string> foundFiles)
        {
            Logger.LogInformation($"Matching files with JSONs in: {directoryPath}");

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
                Logger.LogInformation($"Matching JSON: {++progress}/{totalFiles}: {foundFile}");

                // try starts with
                var match = potentialJsonFiles.Where(x => !usedJsonFiles.Contains(x) && x.StartsWith(foundFile)).FirstOrDefault();

                if (match != null)
                {
                    fileJsonMap[foundFile] = match;
                    Logger.LogInformation($"Found match for {foundFile}: {match}");
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
                    usedJsonFiles.Add(exactMatch); // Mark as used
                    Logger.LogInformation($"Found exact match for {foundFile}: {exactMatch}");
                    continue; // Move to the next foundFile
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
                    Logger.LogInformation($"Found closest match for {foundFile}: {closestMatch}");
                }
            }

            Logger.LogInformation($"Matched {fileJsonMap.Count} files in {directoryPath}");
            return fileJsonMap;
        }
    }
}
