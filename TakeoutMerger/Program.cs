using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Handlers;
using TakeoutMerger.Core.Handlers.Directories;
using TakeoutMerger.Core.Handlers.Files;
using TakeoutMerger.Core.Handlers.Metadata;
using TakeoutMerger.Core.Services;
using TakeoutMerger.Logging;
using ZLogger;

var app = ConsoleApp.Create();

app.ConfigureServices(x =>
{
    x.AddScoped<ITakeoutMergerService, TakeoutMergerService>();
    x.AddScoped<IDirectoryHandler, DirectoryHandler>();
    x.AddScoped<IFileHandler, FileHandler>();
    x.AddScoped<IFileService, FileService>();
    x.AddScoped<IFileMetadataMatcher, FileMetadataMetadataMatcher>();
    x.AddScoped<IJsonNameStandardizationHandler, JsonNameStandardizationHandler>();
    x.AddScoped<IPngToTiffConverter, PngToTiffConverter>();
    x.AddScoped<IMetaDataApplier, MetadataApplier>();
});
app.ConfigureLogging(x =>
{
    x.ClearProviders();
    x.SetMinimumLevel(LogLevel.Trace);
    //x.AddZLoggerConsole(options => { options.UseCustomPlainTextFormatter(); });
    x.AddZLoggerFile((options, _) =>
    {
        options.UseCustomPlainTextFormatter();
        return "log.txt";
    });
});

app.Run(args);