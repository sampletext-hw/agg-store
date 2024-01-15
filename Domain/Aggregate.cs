using agg_store.Events;

namespace agg_store.Domain;

public class Aggregate<T> where T : Aggregate<T>
{
    private readonly List<AggregateEvent<T>> _events; 
    public IReadOnlyList<AggregateEvent<T>> Events => _events;

    public Aggregate()
    {
        _events = new List<AggregateEvent<T>>();
    }

    protected void OnEvent(AggregateEvent<T> ev)
    {
        _events.Add(ev);
    }
}