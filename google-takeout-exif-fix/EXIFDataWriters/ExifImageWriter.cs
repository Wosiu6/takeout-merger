using takeout_merger_p.FileHandlers;

namespace takeout_merger_p.EXIFDataWriters;

public static class ExifImageWriter
{
    public static string OutputPath = "";

    public static async Task WriteImageMetadata(IDictionary<string, string> pairs)
    {
        Console.WriteLine("Writing image metadata...");

        int index = 0;

        foreach (var item in pairs)
        {
            await WriteImageMetadata(item.Key, item.Value);
        }

        Console.WriteLine("Writing image metadata completed.");
    }

    private static async Task WriteImageMetadata(string jsonFilePath, string filePath)
    {
        var newPath = $"{OutputPath}\\" + Path.GetFileName(filePath);
        var extension = Path.GetExtension(filePath);

        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                //await ExifHandler.Handle(jsonFilePath, filePath, newPath);
                break;
            default:
                //await ExifHandler.Handle(jsonFilePath, filePath, newPath);
                return;
        }

        
    }

    
}
