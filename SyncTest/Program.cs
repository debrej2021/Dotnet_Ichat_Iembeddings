using SyncTest.Services;
using SyncTest.Workers;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Register services
builder.Services.AddSingleton<DataService>();

// 🔹 Background worker
builder.Services.AddHostedService<BackgroundWorker>();

var app = builder.Build();

// 🔹 API endpoint
app.MapGet("/dashboard", async (DataService service, CancellationToken ct) =>
{
    var userTask = service.GetSecondAsync();
    var ordersTask = service.GetThirdAsync();
    var paymentsTask = service.GetPaymentsAsync();

    await Task.WhenAll(userTask, ordersTask, paymentsTask);

    return Results.Ok(new
    {
        user = await userTask,
        orders = await ordersTask,
        payments = await paymentsTask
    });
});

app.Run();