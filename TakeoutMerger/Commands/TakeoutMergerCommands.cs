using ConsoleAppFramework;
using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Utils;
using TakeoutMerger.Core.Services;
using ZLogger;

namespace TakeoutMerger.Commands;

[RegisterCommands]
public class TakeoutMergerCommands(
    ILogger<TakeoutMergerCommands> logger,
    ITakeoutMergerService takeoutMergerService)
{
    private readonly ILogger<TakeoutMergerCommands> _logger = logger;
    private readonly ITakeoutMergerService _takeoutMergerService = takeoutMergerService;

    /// <summary>
    /// Executes the application as is. Renames all the json files and processes the files using the renamed files, saves them to the output folder.
    /// </summary>
    /// <param name="inputFolder">-if, Google Takeout original folder.</param>
    /// <param name="outputFolder">-of, Destination folder to which all the application output is going to go.</param>
    [Command("")]
    public async Task MergeTakeoutMetadata([Argument]string inputFolder, [Argument]string outputFolder)
    {
        _logger.ZLogInformation($"Takeout Merger starting: Full merge");

        _takeoutMergerService.EnsureWorkingDirectoryExists(inputFolder, outputFolder);
        
        await _takeoutMergerService.StandardizeJsonNamesAsync(inputFolder);
        
        await _takeoutMergerService.ApplyTakeoutMetadata(inputFolder, outputFolder);
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