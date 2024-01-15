using agg_store.Domain;

namespace agg_store.Commands;

public class WithdrawCommand
{
    private readonly EventStoreService _eventStoreService;

    public WithdrawCommand(EventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    public async Task Execute(BankAccount aggregate, decimal amount)
    {
        aggregate.Withdraw(amount);

        await _eventStoreService.AppendEventsAsync(aggregate.Id, aggregate.Events);
    }
}