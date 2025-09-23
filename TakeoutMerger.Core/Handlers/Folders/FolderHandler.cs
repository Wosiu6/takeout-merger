using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Interfaces;
using TakeoutMerger.Core.Common.Logging;

namespace TakeoutMerger.Core.Handlers.Folders;

public interface IFolderHandler : IHandler;

public class FolderHandler(string outputPath, ILogger logger) : LoggableBase(logger), IFolderHandler
{
    private readonly string _outputPath = outputPath;

    public async Task HandleAsync()
    {
        // rename all json files and build cache
        // handle all the files, pass cache in

        JsonFileNameHandler jsonFileFileNameHandler;

        await Task.CompletedTask;
    }

    public void Handle()
    {
        throw new NotImplementedException();
    }
}