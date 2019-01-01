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
            var retryPolicy = new RetryPolicy(loggerFactory, 7, 500);
            var executionAttemptCount = 0;
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


            //Execute async Example with delegate condition
            executionAttemptCount = 0;
            await retryPolicy.ExecuteAsync(async (token) =>
            {
                var randomNumber = random.Next(1, 3);
                executionAttemptCount++;
                logger.LogInformation("Executing Async with custom exit condition");
                if (randomNumber % 2 == 1)
                {
                    logger.LogInformation("Simulating Random Transient Exception");
                    throw new Exception("Random Transient Exception");
                }
                logger.LogInformation("Execution Async Complete with custom exit condition");
                await Task.Yield();
            }, async (token) =>
            {
                if (executionAttemptCount % 2 == 1)
                {
                    return await Task.FromResult(false);
                }
                else
                {
                    return await Task.FromResult(true);
                }
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


            //Execute sync Example with delegate condition
            executionAttemptCount = 0;
            retryPolicy.Execute(() =>
            {
                var randomNumber = random.Next(1, 3);
                executionAttemptCount++;
                logger.LogInformation("Executing with custom exit condition");
                if (randomNumber % 2 == 1)
                {
                    logger.LogInformation("Simulating Random Transient Exception");
                    throw new Exception("Random Transient Exception");
                }
                logger.LogInformation("Execution Complete with custom exit condition");
            }, () =>
             {
                 if (executionAttemptCount % 2 == 1)
                 {
                     return false;
                 }
                 else
                 {
                     return true;
                 }
             });


            System.Console.ReadLine();
        }
    }
}
