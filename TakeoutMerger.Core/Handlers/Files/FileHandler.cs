using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Converters;
using TakeoutMerger.Core.Handlers.Metadata;

namespace TakeoutMerger.Core.Handlers.Files;

public interface IFileHandler
{
    Task HandleAsync(string mediaFilePath, string metadataFilePath, string outputFolder);
}

public class FileHandler(ILogger<FileHandler> logger, IPngToTiffConverter pngToTiffConverter, IMetaDataApplier metaDataApplier)
    : IFileHandler
{
    private readonly ILogger _logger = logger;
    private readonly IPngToTiffConverter _pngToTiffConverter = pngToTiffConverter;
    private readonly IMetaDataApplier _metaDataApplier = metaDataApplier;

    private static readonly string[] _tagTypeExtensions = [".tiff", ".jpg", ".jpeg"];
    private static readonly string[] _bitmapConvertibleExtensions = [".png"];

    public async Task HandleAsync(string mediaFilePath, string metadataFilePath, string outputFolder)
    {
        var fileExtension = Path.GetExtension(mediaFilePath);
        bool shouldDelete = false;
        
        if (_bitmapConvertibleExtensions.Contains(fileExtension))
        {
            _metaDataApplier.ApplyJsonMetaDataToNonExifFile(mediaFilePath, metadataFilePath, outputFolder);
            
            mediaFilePath = _pngToTiffConverter.Convert(mediaFilePath, Path.GetTempPath());
            fileExtension = Path.GetExtension(mediaFilePath);
            shouldDelete = true;
        }

        if (_tagTypeExtensions.Contains(fileExtension))
        {
            _metaDataApplier.ApplyJsonMetaDataToTagImage(mediaFilePath, metadataFilePath, outputFolder);
        }
        else
        {
            _metaDataApplier.ApplyJsonMetaDataToNonExifFile(mediaFilePath, metadataFilePath, outputFolder);
        }

        if (shouldDelete)
        {
            File.Delete(mediaFilePath);
        }
        
        await Task.CompletedTask;
    }
}