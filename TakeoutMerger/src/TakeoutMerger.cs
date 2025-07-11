﻿using Microsoft.Extensions.Logging;
using TakeoutMerger.src.Common.Logging;
using TakeoutMerger.src.Common.Utils;
using TakeoutMerger.src.Core.Services;

namespace TakeoutMerger.src
{
    public class TakeoutMerger
    {
        private int _counter = 0;
        private int _amountOfFolders = 0;

        public void Start(string[] args)
        {
            string inputPath = args[0];
            string outputPath = args[1];

            if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
            {
                var errorMessage = $"Input and output paths must be provided.\nUsage: TakeoutMerger <directoryPath> <outputPath>\nExample: TakeoutMerger C:\\Downloads\\Takeout C:\\Downloads\\MyOutputFolder";
                throw new ArgumentException(errorMessage);
            }

            DirectoryUtils.EnsureWorkingDirectoryExists(inputPath, outputPath);

#if DEBUG
            string logFilePath = $"{outputPath}\\console_log.txt";
            using StreamWriter logFileWriter = new(logFilePath, append: true);
            ILogger logger = SetupLogger(logFileWriter);
#else
            ILogger logger = SetupLogger();
#endif

            var subDirectories = Directory.GetDirectories(inputPath, "*", SearchOption.AllDirectories);
            _amountOfFolders = subDirectories.Count() + 1; // +1 for the top directory itself

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

                    Interlocked.Increment(ref _counter);
                    logger.LogCritical($"Progress: {_counter}/{_amountOfFolders}.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error processing folder: {inputPath}");
                }
            });
        }

        private ILogger SetupLogger(StreamWriter? logFileWriter = null)
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                builder
#if DEBUG
                .AddProvider(new CustomFileLoggerProvider(logFileWriter!))
#endif
                .AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                }));

            return loggerFactory.CreateLogger("TakeoutMerger");
        }
    }
}
