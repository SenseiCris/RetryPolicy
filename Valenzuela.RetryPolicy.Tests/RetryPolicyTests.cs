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
        public virtual void Retry()
        {
            int expectedRetryLimit = 3;
            int expectedRetryDelay = 500;
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
        public virtual async Task RetryAsync()
        {
            int expectedRetryLimit = 3;
            int expectedRetryDelay = 500;
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