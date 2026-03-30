using Polly;
using Polly.CircuitBreaker;

namespace SyncTest.Services;

public class DataService
{
    private readonly AsyncPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreaker;

    public DataService()
    {
        // 🔹 Retry: 3 attempts with incremental delay
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, attempt =>
                TimeSpan.FromSeconds(attempt),
                (ex, time) =>
                {
                    Console.WriteLine($"🔁 Retry after {time.TotalSeconds}s due to: {ex.Message}");
                });

        // 🔹 Circuit Breaker: open after 3 failures
        _circuitBreaker = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(10),
                onBreak: (ex, ts) =>
                {
                    Console.WriteLine($"⛔ Circuit OPEN for {ts.TotalSeconds}s");
                },
                onReset: () =>
                {
                    Console.WriteLine("✅ Circuit CLOSED");
                });
    }

    // 🔹 Simulates User call
    public async Task<string> GetSecondAsync()
    {
        return await ExecuteWithPolicies(async () =>
        {
            await Task.Delay(1000);

            SimulateFailure("Second");

            return "Second async result received!";
        });
    }

    // 🔹 Simulates Orders call
    public async Task<string> GetThirdAsync()
    {
        return await ExecuteWithPolicies(async () =>
        {
            await Task.Delay(1500);

            SimulateFailure("Third");

            return "Third async result received!";
        });
    }

    // 🔹 Simulates Payments call
    public async Task<string> GetPaymentsAsync()
    {
        return await ExecuteWithPolicies(async () =>
        {
            await Task.Delay(2000);

            SimulateFailure("Payments");

            return "Payments data received!";
        });
    }

    // 🔥 Common wrapper for Polly
    private async Task<string> ExecuteWithPolicies(Func<Task<string>> action)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _circuitBreaker.ExecuteAsync(action));
        }
        catch (BrokenCircuitException)
        {
            return "⚠️ Circuit OPEN - request skipped";
        }
        catch (Exception ex)
        {
            return $"❌ Failed after retries: {ex.Message}";
        }
    }

    // 🔥 Random failure simulator
    private void SimulateFailure(string source)
    {
        if (Random.Shared.Next(1, 4) == 1)
        {
            Console.WriteLine($"❌ {source} call failed");
            throw new Exception($"{source} failure");
        }

        Console.WriteLine($"✅ {source} call succeeded");
    }
}