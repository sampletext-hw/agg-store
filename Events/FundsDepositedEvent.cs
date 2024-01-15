using agg_store.Domain;

namespace agg_store.Events;

public class FundsDepositedEvent : AggregateEvent<BankAccount>
{
    public Guid AccountId { get; }

    public FundsDepositedEvent(Guid accountId, decimal amount)
    {
        AccountId = accountId;
        Amount = amount;
    }

    public override void Apply(BankAccount aggregate, bool store = false)
    {
        aggregate.Apply(this);

        if (store)
        {
            aggregate.RegisterEvent(this);
        }
    }

    public override AggregateEvent<BankAccount> Invert()
    {
        return new FundsWithdrawnEvent(AccountId, Amount);
    }
}