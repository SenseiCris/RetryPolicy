using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Valenzuela.RetryPolicy
{
    /// <summary>
    /// 
    /// </summary>    
    public class RetryPolicy
    {
        /// <summary>
        /// The default retry limit
        /// </summary>
        public const int DEFAULT_RETRY_LIMIT = 3;

        /// <summary>
        /// The default retry delay in milliseconds
        /// </summary>
        public const int DEFAULT_RETRY_DELAY = 500;

        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Gets or sets the maximum amount of execution attempts before quitting.
        /// </summary>
        /// <value>
        /// The retry limit.
        /// </value>
        public int RetryLimit { get; protected set; }

        /// <summary>
        /// Gets or sets the amount of time to wait in milliseconds before executing another attempt
        /// </summary>
        /// <value>
        /// The retry delay.
        /// </value>
        public int RetryDelay { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryPolicy"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="retryLimit">The maximum amount of execution attempts before quitting.</param>
        /// <param name="retryDelay">The amount of time to wait in milliseconds before executing another attempt.</param>
        public RetryPolicy(ILoggerFactory loggerFactory, int retryLimit = DEFAULT_RETRY_LIMIT, int retryDelay = DEFAULT_RETRY_DELAY)
        {
            Logger = loggerFactory.CreateLogger(GetType());
            RetryLimit = retryLimit;
            RetryDelay = retryDelay;
        }

        /// <summary>
        /// Executes the specified function.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="RetryPolicyException">Thrown when all attempts to execute the given function fails</exception>
        public async Task ExecuteAsync(Func<CancellationToken, Task> func, CancellationToken token = default)
        {
            Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.ExecuteAsync)}\"");
            await ExecuteAsync(func, 0, token);
        }

        /// <summary>
        /// Executes the specified function.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <param name="exitCondition">The exit condition.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="RetryPolicyException">Thrown when all attempts to execute the given function fails</exception>
        public async Task ExecuteAsync(Func<CancellationToken, Task> func, Func<CancellationToken, Task<bool>> exitCondition, CancellationToken token = default)
        {
            Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.ExecuteAsync)}\"");
            await ExecuteAsync(func, exitCondition, 0, token);
        }

        /// <summary>
        /// Executes the specified function.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="RetryPolicyException">Thrown when all attempts to execute the given action fails</exception>
        public void Execute(Action action)
        {
            Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.Execute)}\"");
            Execute(action, 0, null);
        }

        /// <summary>
        /// Executes the specified function.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="exitCondition">The exit condition.</param>
        /// <exception cref="RetryPolicyException">Thrown when all attempts to execute the given action fails</exception>
        public void Execute(Action action, Func<bool> exitCondition)
        {
            Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.Execute)}\"");
            Execute(action, exitCondition, 0, null);
        }

        /// <summary>
        /// Executes the specified function.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="token">The token.</param>
        /// <param name="lastException">The last exception.</param>
        /// <returns></returns>
        /// <exception cref="RetryPolicyException">Thrown when all attempts to execute the given function fails</exception>
        private async Task ExecuteAsync(Func<CancellationToken, Task> func, int retryCount, CancellationToken token = default, Exception lastException = null)
        {
            Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.ExecuteAsync)}_2\",  \"retryCount\": \"{retryCount}\"");
            var funcHandle = func;
            if (retryCount <= RetryLimit)
            {
                try
                {
                    if (funcHandle != null && !token.IsCancellationRequested)
                    {
                        await funcHandle(token);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Execution Failed. Retry number {retryCount}. Retry again in {RetryDelay} milliseconds");
                    retryCount++;
                    await Task.Delay(RetryDelay);
                    await ExecuteAsync(funcHandle, retryCount, token, ex);
                }
            }
            else
            {
                Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.ExecuteAsync)}_2\",  \"retryCount\": \"{retryCount}\"");

                var retryPolicyException = new RetryPolicyException($"Attempted to re-execute { retryCount - 1 } times", func, lastException);
                Logger.LogError(retryPolicyException, "Unable to execute function");
                throw retryPolicyException;
            }
        }

        /// <summary>
        /// Executes the specified function.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <param name="exitCondition">The exit condition.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="token">The token.</param>
        /// <param name="lastException">The last exception.</param>
        /// <returns></returns>
        /// <exception cref="RetryPolicyException">Thrown when all attempts to execute the given function fails</exception>
        private async Task ExecuteAsync(Func<CancellationToken, Task> func, Func<CancellationToken, Task<bool>> exitCondition, int retryCount, CancellationToken token = default, Exception lastException = null)
        {
            Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.ExecuteAsync)}_3\", \"retryCount\": \"{retryCount}\"");
            var funcHandle = func;
            var exitConditionHandle = exitCondition;

            if (retryCount <= RetryLimit)
            {
                try
                {
                    if (funcHandle != null && !token.IsCancellationRequested && exitConditionHandle != null)
                    {
                        await funcHandle(token);

                        if (await exitConditionHandle(token))
                        {
                            Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.ExecuteAsync)}_3\", \"message\": \"Exit Condition met at Retry number {retryCount}\"");
                            await Task.Yield();
                            return;
                        }
                        else
                        {
                            retryCount++;
                            Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.ExecuteAsync)}_3\", \"message\": \"Exit Condition not met. Retry number {retryCount}. Retry again in {RetryDelay} milliseconds\"");                            
                            await Task.Delay(RetryDelay);
                            await ExecuteAsync(funcHandle, exitConditionHandle, retryCount, token, lastException);
                        }
                    }
                }
                catch (Exception ex)
                {
                    retryCount++;

                    Logger.LogError(ex, $"Execution Failed. Retry number {retryCount}. Retry again in {RetryDelay} milliseconds");
                    await Task.Delay(RetryDelay);
                    await ExecuteAsync(funcHandle, exitConditionHandle, retryCount, token, ex);

                }
            }
            else
            {
                Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.ExecuteAsync)}_3\",  \"retryCount\": \"{retryCount}\"");

                var retryPolicyException = new RetryPolicyException($"Attempted to re-execute { retryCount - 1 } times", func, lastException);
                Logger.LogError(retryPolicyException, "Unable to execute function");
                throw retryPolicyException;
            }
        }

        /// <summary>
        /// Executes the specified function.
        /// </summary>
        /// <param name="action">The function.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="lastException">The last exception.</param>
        /// <exception cref="RetryPolicyException">Thrown when all attempts to execute the given action fails</exception>
        private void Execute(Action action, int retryCount, Exception lastException = null)
        {
            Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.Execute)}_2\",  \"retryCount\": \"{retryCount}\"");
            var actionHandle = action;
            if (retryCount <= RetryLimit && actionHandle != null)
            {
                try
                {
                    actionHandle();
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Logger.LogError(ex, $"Execution Failed. Retry number {retryCount}. Retry again in {RetryDelay} milliseconds");
                    Thread.Sleep(RetryDelay);
                    Execute(actionHandle, retryCount, ex);
                }
            }
            else
            {
                Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.Execute)}_2\",  \"retryCount\": \"{retryCount}\"");

                var retryPolicyException = new RetryPolicyException($"Attempted to re-execute { retryCount - 1 } times", action, lastException);
                Logger.LogError(retryPolicyException, "Unable to execute action");
                throw retryPolicyException;
            }
        }

        /// <summary>
        /// Executes the specified function.
        /// </summary>
        /// <param name="action">The function.</param>
        /// <param name="exitCondition">The exit condition.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="lastException">The last exception.</param>
        /// <exception cref="RetryPolicyException">Thrown when all attempts to execute the given action fails</exception>
        private void Execute(Action action, Func<bool> exitCondition, int retryCount, Exception lastException = null)
        {
            Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.Execute)}_3\",  \"retryCount\": \"{retryCount}\"");
            var actionHandle = action;
            var exitConditionHandle = exitCondition;
            if (retryCount <= RetryLimit && actionHandle != null && exitCondition != null)
            {
                try
                {
                    actionHandle();
                    if (exitConditionHandle())
                    {
                        Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.Execute)}_3\", \"message\": \"Exit Condition met at Retry number {retryCount}\"");
                        return;
                    }
                    else
                    {
                        retryCount++;
                        Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.Execute)}_3\", \"message\": \"Exit Condition not met. Retry number {retryCount}. Retry again in {RetryDelay} milliseconds\"");
                        Thread.Sleep(RetryDelay);
                        Execute(actionHandle, exitConditionHandle, retryCount, lastException);

                    }
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Logger.LogError(ex, $"Execution Failed. Retry number {retryCount}. Retry again in {RetryDelay} milliseconds");
                    Thread.Sleep(RetryDelay);
                    Execute(actionHandle, exitConditionHandle, retryCount, ex);
                }
            }
            else
            {
                Logger.LogInformation($"\"Class\": \"{nameof(RetryPolicy)}\", \"Operation\" : \"{nameof(RetryPolicy.Execute)}_3\",  \"retryCount\": \"{retryCount}\"");

                var retryPolicyException = new RetryPolicyException($"Attempted to re-execute { retryCount - 1 } times", action, lastException);
                Logger.LogError(retryPolicyException, "Unable to execute action");
                throw retryPolicyException;
            }
        }

    }


}
