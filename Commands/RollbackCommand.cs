using System.Text.Json;
using agg_store.Domain;
using agg_store.Events;
using EventStore.Client;

namespace agg_store.Commands;

public class RollbackCommand
{
    private readonly EventStoreService _eventStoreService;
    private readonly EventStoreClient _eventStoreClient;

    public RollbackCommand(EventStoreService eventStoreService, EventStoreClient eventStoreClient)
    {
        _eventStoreService = eventStoreService;
        _eventStoreClient = eventStoreClient;
    }

    public async Task<bool> Execute(BankAccount aggregate, Guid id)
    {
        var streamName = $"bankAccount-{aggregate.Id}";

        var foundEvent = await FindEvent(id, streamName);

        if (foundEvent is not null)
        {
            foundEvent.Invert()
                .Apply(aggregate, store: true);

            await _eventStoreService.AppendEventsAsync(aggregate.Id, aggregate.Events);
            return true;
        }

        return false;
    }

    private async Task<AggregateEvent<BankAccount>?> FindEvent(Guid id, string streamName)
    {
        var streamResult = _eventStoreClient.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            StreamPosition.Start,
            100
        );

        var readState = await streamResult.ReadState;

        if (readState == ReadState.StreamNotFound)
        {
            return null;
        }

        AggregateEvent<BankAccount>? foundEvent = null;

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

            if (message.Event.EventId.ToGuid() == id)
            {
                foundEvent = ev;
                break;
            }
        }

        return foundEvent;
    }
}