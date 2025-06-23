using Microsoft.Extensions.Logging;
using TakeoutMerger.src.Common.Utils;
using TakeoutMerger.src.Core.Services;

namespace TakeoutMerger.src
{
    public class TakeoutMerger
    {
        public void Start(string[] args)
        {
            ILogger logger = SetupLogger();

            if (args.Length < 1)
            {
                logger.LogError("Insufficient parameters supplied.");
                logger.LogInformation("Usage: TakeoutMerger <directoryPath> <outputPath>");
                logger.LogInformation("Example: TakeoutMerger C:\\Downloads\\Takeout C:\\Downloads\\MyOutputFolder");
                return;
            }

            string inputPath = args[0];
            string outputPath = args[1];

            DirectoryUtils.EnsureWorkingDirectoryExists(inputPath, outputPath, logger);
            
            var subDirectories = Directory.GetDirectories(inputPath, "*", SearchOption.AllDirectories);

            List<Task> subDirectoryTasks = [];

            foreach (var subDirectory in subDirectories)
            {
                var task = ProcessFolder(logger, subDirectory, outputPath);

                subDirectoryTasks.Add(task);
            }

            var topDirectoryTask = ProcessFolder(logger, inputPath, outputPath, searchOption: SearchOption.TopDirectoryOnly);
            subDirectoryTasks.Add(topDirectoryTask);

            Task.WaitAll(subDirectoryTasks);
        }

        private Task ProcessFolder(ILogger logger, string inputPath, string outputPath, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Task.Run(() =>
            {
                try
                {
                    logger.LogInformation($"Processing folder: {inputPath}");

                    JsonService jsonService = new(logger, inputPath, outputPath, searchOption);
                    jsonService.Process();

                    PngService pngService = new(logger, inputPath, outputPath, searchOption);
                    pngService.Process();

                    TagImageService tagImageService = new(logger, inputPath, outputPath, searchOption);
                    tagImageService.Process();

                    UnsuportedFilesService unsuportedFilesService = new(logger, inputPath, outputPath, searchOption);
                    unsuportedFilesService.Process();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error processing folder: {inputPath}");
                }
            });
        }

        private ILogger SetupLogger()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                }));

            return loggerFactory.CreateLogger("TakeoutMerger");
        }
    }
}
