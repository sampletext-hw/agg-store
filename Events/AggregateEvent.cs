using agg_store.Domain;

namespace agg_store.Events;

public abstract class AggregateEvent<T> where T : Aggregate<T>
{
    public abstract void Apply(T aggregate, bool store = false);
    public decimal Amount { get; set; }

    public abstract AggregateEvent<T> Invert();
}