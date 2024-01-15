using System.Text.Json;
using agg_store.Domain;
using agg_store.Events;
using EventStore.Client;

namespace agg_store.Queries;

public class GetActualBalanceQuery
{
    private readonly EventStoreClient _eventStoreClient;

    public GetActualBalanceQuery(EventStoreClient eventStoreClient)
    {
        _eventStoreClient = eventStoreClient;
    }

    public async Task<BankAccount> Execute(Guid aggregateId)
    {
        var streamName = $"bankAccount-{aggregateId}";
        
        var streamResult = _eventStoreClient.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start);
        var aggregate = new BankAccount(aggregateId);

        var readState = await streamResult.ReadState;

        if (readState == ReadState.StreamNotFound)
        {
            return aggregate;
        }

        await foreach (var message in streamResult)
        {
            var eventType = Type.GetType($"agg_store.Events.{message.Event.EventType}, agg-store, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            if (eventType is null)
            {
                throw new InvalidOperationException($"Unknown event in stream: {eventType}");
            }
            
            var ev = JsonSerializer.Deserialize(message.Event.Data.Span, eventType) as AggregateEvent<BankAccount>;

            if (ev is null)
            {
                throw new InvalidOperationException($"Couldn't parse event");
            }
            
            ev.Apply(aggregate);
        }

        return aggregate;
    }
}