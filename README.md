# RetryPolicy
A simple *Retry Policy* for dotnet core


## Overview

A Retry Policy is a general purpose utility class for dotnet core written in CSharp (C#) that will execute a block of code and attempt to re-execute that code in the event of a failure. It is designed to work with async code as well as synchronous code. 


## Features

- Configurable number of retry attempts
- Configurable delay between attempts
- Works with synchronous code
- Works with asynchronous code
- Logs attempts and errors

## Usage


Take a look at the included unit tests and **Console** application for an example usage. 

1. Instantiate the **RetryPolicy** class with the desired configuration
2. Call the **Execute** or **ExecuteAsync** methods with a delegate expression you wish to run and retry in the event of a failure

```csharp
private static async Task Main(string[] args)
{
    var loggerFactory = new LoggerFactory();
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
}
```

**Example Output**
```console

info: Valenzuela.RetryPolicy.RetryPolicy[0]
      "Class": "RetryPolicy", "Operation" : "ExecuteAsync", "func": " "
info: Valenzuela.RetryPolicy.RetryPolicy[0]
      "Class": "RetryPolicy", "Operation" : "ExecuteAsync_2", "func": " ", "retryCount": "0"
info: Valenzuela.RetryPolicy.Console.Program[0]
      Executing Async
info: Valenzuela.RetryPolicy.Console.Program[0]
      Simulating Random Transient Exception
fail: Valenzuela.RetryPolicy.RetryPolicy[0]
      Execution Failed. Retry number 0. Retry again in 500 milliseconds
System.Exception: Random Transient Exception
   at Valenzuela.RetryPolicy.Console.Program.<>c__DisplayClass0_0.<<Main>b__0>d.MoveNext() in D:\Git\RetryPolicy\Valenzuela.RetryPolicy.Console\Program.cs:line 33
--- End of stack trace from previous location where exception was thrown ---
   at Valenzuela.RetryPolicy.RetryPolicy.ExecuteAsync(Func`2 func, Int32 retryCount, CancellationToken token, Exception lastException) in D:\Git\RetryPolicy\Valenzuela.RetryPolicy\RetryPolicy.cs:line 100
info: Valenzuela.RetryPolicy.RetryPolicy[0]
      "Class": "RetryPolicy", "Operation" : "ExecuteAsync_2", "func": " ", "retryCount": "1"
info: Valenzuela.RetryPolicy.Console.Program[0]
      Executing Async
info: Valenzuela.RetryPolicy.Console.Program[0]
      Simulating Random Transient Exception
fail: Valenzuela.RetryPolicy.RetryPolicy[0]
      Execution Failed. Retry number 1. Retry again in 500 milliseconds
System.Exception: Random Transient Exception
   at Valenzuela.RetryPolicy.Console.Program.<>c__DisplayClass0_0.<<Main>b__0>d.MoveNext() in D:\Git\RetryPolicy\Valenzuela.RetryPolicy.Console\Program.cs:line 33
--- End of stack trace from previous location where exception was thrown ---
   at Valenzuela.RetryPolicy.RetryPolicy.ExecuteAsync(Func`2 func, Int32 retryCount, CancellationToken token, Exception lastException) in D:\Git\RetryPolicy\Valenzuela.RetryPolicy\RetryPolicy.cs:line 100
info: Valenzuela.RetryPolicy.RetryPolicy[0]
      "Class": "RetryPolicy", "Operation" : "ExecuteAsync_2", "func": " ", "retryCount": "2"
info: Valenzuela.RetryPolicy.Console.Program[0]
      Executing Async
info: Valenzuela.RetryPolicy.Console.Program[0]
      Execution Async Complete
info: Valenzuela.RetryPolicy.RetryPolicy[0]
      "Class": "RetryPolicy", "Operation" : "Execute", "action": " "
info: Valenzuela.RetryPolicy.RetryPolicy[0]
      "Class": "RetryPolicy", "Operation" : "Execute_2", "action": " ", "retryCount": "0"
info: Valenzuela.RetryPolicy.Console.Program[0]
      Executing
info: Valenzuela.RetryPolicy.Console.Program[0]
      Execution Complete

```
