using TakeoutMerger.Core.Common.Interfaces;

namespace TakeoutMerger.Core.Handlers;

public abstract class BaseFileHandler(string fileToHandlePath, string outputPath) : IHandler
{
    private readonly string _fileToHandlePath = fileToHandlePath;
    private readonly string _outputPath = outputPath;

    public virtual Task HandleAsync()
    {
        throw new NotImplementedException("Base File Handler Handle Async Not Implemented");
    }

    public virtual void Handle()
    {
        throw new NotImplementedException("Base File Handler Handle Not Implemented");
    }
}