using agg_store.Events;

namespace agg_store.Domain;

public class BankAccount : Aggregate<BankAccount>
{
    public Guid Id { get; private set; }
    public decimal Balance { get; private set; }

    public BankAccount(Guid id)
    {
        Id = id;
        Balance = 0;
    }

    public void Deposit(decimal amount)
    {
        var ev = new FundsDepositedEvent(Id, amount);
        Apply(ev);
        RegisterEvent(ev);
    }

    public void Withdraw(decimal amount)
    {
        var ev = new FundsWithdrawnEvent(Id, amount);
        Apply(ev);
        RegisterEvent(ev);
    }

    public void Apply(FundsDepositedEvent @event)
    {
        Balance += @event.Amount;
    }

    public void Apply(FundsWithdrawnEvent @event)
    {
        Balance -= @event.Amount;
    }

    public void RegisterEvent(AggregateEvent<BankAccount> ev)
    {
        OnEvent(ev);
    }
}