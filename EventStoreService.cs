using System.Text.Json;
using agg_store.Domain;
using agg_store.Events;
using EventStore.Client;

namespace agg_store;

public class EventStoreService
{
    private readonly EventStoreClient _eventStoreClient;

    public EventStoreService(EventStoreClient eventStoreClient)
    {
        _eventStoreClient = eventStoreClient;
    }

    public async Task AppendEventsAsync<T>(Guid aggregateId, IEnumerable<AggregateEvent<T>> events)
        where T : Aggregate<T>
    {
        var eventData = events.Select(
            e => new EventData(
                Uuid.NewUuid(),
                e.GetType().Name,
                JsonSerializer.SerializeToUtf8Bytes(e)
            )
        );

        await _eventStoreClient.AppendToStreamAsync(
            $"bankAccount-{aggregateId}",
            StreamState.Any,
            eventData
        );
    }
}