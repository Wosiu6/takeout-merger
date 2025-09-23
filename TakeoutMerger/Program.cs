using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Handlers.Files;
using TakeoutMerger.Logging;
using ZLogger;

var app = ConsoleApp.Create();

app.ConfigureServices(x =>
{
    x.AddScoped<IJsonNameStandardizationHandler, JsonNameStandardizationHandler>();
});

app.ConfigureLogging(x =>
{
    x.ClearProviders();
    x.SetMinimumLevel(LogLevel.Trace);
    x.AddZLoggerConsole(options =>
    {
        options.UseCustomPlainTextFormatter();
    });
    x.AddZLoggerFile((options, _) =>
    {
        options.UseCustomPlainTextFormatter();
        return "log.txt";
    });
});

app.Run(args);

