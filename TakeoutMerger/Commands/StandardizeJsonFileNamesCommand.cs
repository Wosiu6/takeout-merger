using ConsoleAppFramework;
using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Handlers.Files;
using ZLogger;

namespace TakeoutMerger.Commands;

[RegisterCommands]
public class TakeoutMergerCommands(
    ILogger<TakeoutMergerCommands> logger,
    IJsonNameStandardizationHandler jsonNameStandardizationHandler)
{
    private readonly ILogger<TakeoutMergerCommands> _logger = logger;
    private readonly IJsonNameStandardizationHandler _jsonNameStandardizationHandler = jsonNameStandardizationHandler;

    /// <summary>
    /// Test Root
    /// </summary>
    /// <param name="msg">-m, Message to show</param>
    [Command("")]
    public async Task RootCommandTest(string msg)
    {
        _logger.ZLogInformation($"Start {msg}");
        
        await Task.CompletedTask;
        //await _jsonNameStandardizationHandler.HandleAsync();
        
        _logger.ZLogInformation($"End {msg}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="msg">-m, Message to show</param>
    [Command("StandardizeJsonFileNames")]
    public void StandardizeJsonFileNames(string msg)
    {
        _logger.ZLogInformation($"Message is {msg}");
    }
}