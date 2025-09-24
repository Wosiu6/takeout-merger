using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Interfaces;
using TakeoutMerger.Core.Services;

namespace TakeoutMerger.Core.Handlers.Files;

public interface IJsonNameStandardizationHandler
{
    public Task HandleAsync(string inputFolder);
}

public class JsonNameStandardizationHandler(ILogger<JsonNameStandardizationHandler> logger, IFileService fileService) : IJsonNameStandardizationHandler
{
    private readonly ILogger _logger = logger;
    private readonly IFileService _fileService = fileService;
    
    private const string _supplementedMetadataRegexString =
        @"^*.sup(p(l(e(m(e(n(t(a(l(-(m(e(t(a(d(a(t(a)?)?)?)?)?)?)?)?)?)?)?)?)?)?)?)?)?)?.*\.json$";

    private Regex _supplementedMetadataRegex = new Regex(_supplementedMetadataRegexString);
    
    private const string _jsonExtension = ".json";
    
    public async Task HandleAsync(string inputFolder)
    {
        List<string> foundJsons = fileService.GetFilesByExtensions(inputFolder, [".json"]);

        foreach (var foundJson in foundJsons)
        {
            var newJsonPath = _supplementedMetadataRegex.Replace(foundJson, _jsonExtension);

            if (!File.Exists(newJsonPath))
            {
                File.Copy(foundJson, newJsonPath, true);
            }
            
            File.Delete(foundJson);
        }
    }
}

