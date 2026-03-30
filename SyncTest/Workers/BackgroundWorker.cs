using Microsoft.Extensions.Hosting;
using SyncTest.Services;

namespace SyncTest.Workers;

public class BackgroundWorker : BackgroundService
{
    private readonly DataService _service;

    public BackgroundWorker(DataService service)
    {
        _service = service;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Background worker started...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 🔹 Parallel execution
                var userTask = _service.GetSecondAsync();
                var ordersTask = _service.GetThirdAsync();
                var paymentsTask = _service.GetPaymentsAsync();

                await Task.WhenAll(userTask, ordersTask, paymentsTask);

                Console.WriteLine($"[Worker] User: {await userTask}");
                Console.WriteLine($"[Worker] Orders: {await ordersTask}");
                Console.WriteLine($"[Worker] Payments: {await paymentsTask}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Worker error: {ex.Message}");
            }

            // 🔹 Respect cancellation
            await Task.Delay(5000, stoppingToken);
        }
    }
}