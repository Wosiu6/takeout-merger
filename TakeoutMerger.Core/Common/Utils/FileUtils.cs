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
}