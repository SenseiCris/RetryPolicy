using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;


namespace Valenzuela.RetryPolicy.Console
{
    internal class Program
    {
        /// <summary>
        /// Example Usage for Retry Policy
        /// </summary>        
        /// <returns></returns>
        private static async Task Main(string[] args)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole(LogLevel.Debug);

            var logger = loggerFactory.CreateLogger(typeof(Program));
            var random = new Random();
            var retryPolicy = new RetryPolicy(loggerFactory, 3, 500);

            //Execute async Example
            await retryPolicy.ExecuteAsync(async (token) =>
            {
                var randomNumber = random.Next(1, 3);
                logger.LogInformation("Executing Async");
                if (randomNumber % 2 == 1)
                {
                    logger.LogInformation("Simulating Random Transient Exception");
                    throw new Exception("Random Transient Exception");
                }
                logger.LogInformation("Execution Async Complete");
                await Task.Yield();
            });

            //Execute sync example
            retryPolicy.Execute(() =>
            {
                var randomNumber = random.Next(1, 3);
                logger.LogInformation("Executing");
                if (randomNumber % 2 == 1)
                {
                    logger.LogInformation("Simulating Random Transient Exception");
                    throw new Exception("Random Transient Exception");
                }
                logger.LogInformation("Execution Complete");
            });

            System.Console.ReadLine();
        }
    }
}
