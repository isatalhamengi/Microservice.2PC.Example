using Coordinator.Models.Contexts;
using Coordinator.Services;
using Coordinator.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TwoPhaseCommitContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

builder.Services.AddHttpClient("OrderAPI",client => client.BaseAddress = new("https://localhost:7034/"));
builder.Services.AddHttpClient("StockAPI", client => client.BaseAddress = new("https://localhost:7035/"));
builder.Services.AddHttpClient("PaymentAPI", client => client.BaseAddress = new("https://localhost:7202/"));


builder.Services.AddTransient<ITransactionService, TransactionService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/create-order-transaction", async (ITransactionService transactionService) =>
{
    //Phase 1 - Prepare
    var transactionId = await transactionService.CreateTransactionAsync();
    await transactionService.PreapareServicesAsync(transactionId);
    bool transactionState = await transactionService.CheckTransactionStateServicesAsync(transactionId);

    if (transactionState)
    {
        //Phase 2 - Commit
        await transactionService.CommitAsync(transactionId);
        transactionState = await transactionService.CheckTransactionStateServicesAsync(transactionId);
    }
    if (!transactionState)
        await transactionService.RollBackAsync(transactionId);
});

app.Run();
