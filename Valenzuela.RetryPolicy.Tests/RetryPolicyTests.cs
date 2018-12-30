using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Valenzuela.RetryPolicy.Tests
{
    public class RetryPolicyTests
    {
        [Fact]
        [Trait("Owner", "CValenzuela")]
        public void Retry_Success()
        {
            const int expectedRetryLimit = 3;
            const int expectedRetryDelay = 500;
            const string expectedValue = "success";

            ILoggerFactory loggerFactory = new LoggerFactory();
            var retryPolicy = new RetryPolicy(loggerFactory, expectedRetryLimit, expectedRetryDelay);
            var actualValue = string.Empty;
            int executionCount = 0;
            var actualException = Record.Exception(() =>
                retryPolicy.Execute(() =>
                {
                    if (executionCount == 0)
                    {
                        executionCount++;
                        throw new InvalidOperationException();
                    }
                    actualValue = expectedValue;
                }));

            Assert.Null(actualException);
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        [Trait("Owner", "CValenzuela")]
        public virtual async Task RetryAsync_Success()
        {
            const int expectedRetryLimit = 3;
            const int expectedRetryDelay = 500;
            const string expectedValue = "success";

            ILoggerFactory loggerFactory = new LoggerFactory();
            var retryPolicy = new RetryPolicy(loggerFactory, expectedRetryLimit, expectedRetryDelay);
            var actualValue = string.Empty;
            int executionCount = 0;

            var actualException = await Record.ExceptionAsync(async () =>
                await retryPolicy.ExecuteAsync(async (token) =>
                {
                    if (executionCount == 0)
                    {
                        executionCount++;
                        throw new InvalidOperationException();
                    }
                    actualValue = expectedValue;
                    await Task.Yield();
                }));

            Assert.Null(actualException);
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        [Trait("Owner", "CValenzuela")]
        public void Retry_Failed()
        {
            const int expectedRetryLimit = 3;
            const int expectedRetryDelay = 500;
            int actualRetryLimit = 0;
            ILoggerFactory loggerFactory = new LoggerFactory();
            var retryPolicy = new RetryPolicy(loggerFactory, expectedRetryLimit, expectedRetryDelay);


            var actualException = Record.Exception(() =>
                retryPolicy.Execute(() =>
                {
                    actualRetryLimit++;
                    throw new InvalidOperationException();
                }));

            Assert.NotNull(actualException);
            Assert.IsType<RetryPolicyException>(actualException);
            Assert.Equal($"Attempted to re-execute {expectedRetryLimit} times", actualException.Message);
            Assert.Equal(expectedRetryLimit, actualRetryLimit - 1);
        }

        [Fact]
        [Trait("Owner", "CValenzuela")]
        public async Task RetryAsync_Failed()
        {
            const int expectedRetryLimit = 3;
            const int expectedRetryDelay = 500;
            int actualRetryLimit = 0;
            ILoggerFactory loggerFactory = new LoggerFactory();
            var retryPolicy = new RetryPolicy(loggerFactory, expectedRetryLimit, expectedRetryDelay);

            var actualException = await Record.ExceptionAsync(async () =>
            {
                await retryPolicy.ExecuteAsync(async (token) =>
                {
                    actualRetryLimit++;
                    await Task.Yield();
                    throw new InvalidOperationException(actualRetryLimit.ToString());
                });

            });

            Assert.NotNull(actualException);
            Assert.IsType<RetryPolicyException>(actualException);
            Assert.Equal($"Attempted to re-execute {expectedRetryLimit} times", actualException.Message);
            Assert.Equal(expectedRetryLimit, actualRetryLimit - 1);
        }
    }
}