using agg_store;
using agg_store.Commands;
using agg_store.Queries;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("EventStore");

if (connectionString is null)
{
    Console.WriteLine("connection string was null");
    return;
}

builder.Services.AddEventStoreClient(connectionString);

builder.Services.AddSingleton<EventStoreService>();

builder.Services.AddSingleton<GetActualBalanceQuery>();
builder.Services.AddSingleton<WithdrawCommand>();
builder.Services.AddSingleton<DepositCommand>();
builder.Services.AddSingleton<GetHistoryQuery>();
builder.Services.AddSingleton<RollbackCommand>();

var app = builder.Build();

app.MapGet(
    "/get-balance/{id:guid}",
    async ([FromServices] GetActualBalanceQuery query, [FromRoute] Guid id) =>
    {
        var account = await query.Execute(id);

        return Results.Ok(account.Balance);
    }
);

app.MapGet(
    "/withdraw/{id:guid}/{amount:decimal}",
    async ([FromServices] GetActualBalanceQuery query, [FromServices] WithdrawCommand command, [FromRoute] Guid id, decimal amount) =>
    {
        var account = await query.Execute(id);

        await command.Execute(account, amount);

        return Results.Ok(account.Balance);
    }
);

app.MapGet(
    "/deposit/{id:guid}/{amount:decimal}",
    async ([FromServices] GetActualBalanceQuery query, [FromServices] DepositCommand command, [FromRoute] Guid id, decimal amount) =>
    {
        var account = await query.Execute(id);

        await command.Execute(account, amount);

        return Results.Ok(account.Balance);
    }
);

app.MapGet(
    "/history/{id:guid}",
    async ([FromServices] GetHistoryQuery query, [FromRoute] Guid id) =>
    {
        var result = await query.Execute(id);

        return Results.Ok(result);
    }
);

app.MapGet(
    "/rollback/{id:guid}/{transaction:guid}",
    async ([FromServices] GetActualBalanceQuery query, [FromServices] RollbackCommand command, [FromRoute] Guid id, [FromRoute] Guid transaction) =>
    {
        var account = await query.Execute(id);

        var result = await command.Execute(account, transaction);

        return Results.Ok(result ? "Success" : "Fail");
    }
);

app.Run();