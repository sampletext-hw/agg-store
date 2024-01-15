using agg_store.Domain;

namespace agg_store.Commands;

public class DepositCommand
{
    private readonly EventStoreService _eventStoreService;

    public DepositCommand(EventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    public async Task Execute(BankAccount aggregate, decimal amount)
    {
        aggregate.Deposit(amount);

        await _eventStoreService.AppendEventsAsync(aggregate.Id, aggregate.Events);
    }
}