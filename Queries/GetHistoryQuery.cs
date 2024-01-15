using System.Text.Json;
using agg_store.Domain;
using agg_store.Events;
using EventStore.Client;

namespace agg_store.Queries;

public class GetHistoryQuery
{
    public record Item(Guid Id, string Type, decimal Amount);
    
    private readonly EventStoreClient _eventStoreClient;

    public GetHistoryQuery(EventStoreClient eventStoreClient)
    {
        _eventStoreClient = eventStoreClient;
    }

    public async Task<IList<Item>> Execute(Guid aggregateId)
    {
        var streamName = $"bankAccount-{aggregateId}";
        
        var streamResult = _eventStoreClient.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, 100);

        var readState = await streamResult.ReadState;

        if (readState == ReadState.StreamNotFound)
        {
            return new List<Item>();
        }
        var list = new List<Item>();
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
            
            list.Add(new Item(message.Event.EventId.ToGuid(), message.Event.EventType, ev.Amount));
        }

        return list;
    }
}