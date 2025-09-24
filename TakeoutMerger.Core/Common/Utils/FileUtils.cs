using System.Collections.Frozen;

namespace TakeoutMerger.Core.Common.Utils;

public static class FileUtils
{
    public static string GetUniqueFilePath(string filePath, int counter = 0)
    {
        string newFilePath = counter == 0 ? filePath : GenerateNumberedFileName(filePath, counter);

        if (!File.Exists(newFilePath))
        {
            return newFilePath;
        }

        return GetUniqueFilePath(filePath, counter + 1);
    }

    private static string GenerateNumberedFileName(string originalPath, int number)
    {
        string directory = Path.GetDirectoryName(originalPath) ?? "";
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalPath);
        string extension = Path.GetExtension(originalPath);

        string numberedFileName = $"{fileNameWithoutExtension}_{number}{extension}";

        return Path.Combine(directory, numberedFileName);
    }
    
    public static List<string> GetFilesByExtensions(string directoryPath, ICollection<string> extensions, SearchOption searchOption = SearchOption.TopDirectoryOnly, bool excludeExtensions = false)
    {
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

        return filesList;
    }

    public static List<string> GetFilesExceptExtensions(string directoryPath, ICollection<string> extensions, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        return GetFilesByExtensions(directoryPath, extensions, searchOption, true);
    }
}