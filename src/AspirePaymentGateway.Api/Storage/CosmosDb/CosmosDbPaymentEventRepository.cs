using System.Text.Json;
using AspirePaymentGateway.Api.Features.Payments.Domain.Events;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using BinaryMash.Extensions.Results;
using Microsoft.Azure.Cosmos;
using OneOf;
using static AspirePaymentGateway.Api.Features.Payments.Domain.Errors;

namespace AspirePaymentGateway.Api.Storage.CosmosDb
{
    public partial class CosmosDbPaymentEventRepository(ILogger<CosmosDbPaymentEventRepository> logger, CosmosClient cosmos) : IPaymentEventsRepository
    {
        private readonly Container _container = cosmos.GetContainer("PaymentsDb", "Payments");
        private readonly ILogger<CosmosDbPaymentEventRepository> _logger;

        JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task<Result<IList<PaymentEvent>>> GetAsync(string paymentId, CancellationToken cancellationToken)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.paymentId = @paymentId")
                    .WithParameter("@paymentId", paymentId);

                var events = new List<PaymentEvent>();

                using var iterator = _container.GetItemQueryIterator<dynamic>(
                    query,
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(paymentId)
                    });

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync(cancellationToken);
                    foreach (var doc in response)
                    {
                        var eventType = ((System.Text.Json.JsonElement)doc).GetProperty("eventType").GetString();
                        string json = doc.ToString();

                        // Use a factory or reflection to get the correct .NET type
                        var map = EventTypeToType(eventType);

                        if (map.TryPickT0(out var type, out var error))
                        {
                            PaymentEvent paymentEvent = (PaymentEvent)System.Text.Json.JsonSerializer.Deserialize(json, type, serializerOptions)!;
                            events.Add(paymentEvent);
                        }
                        else
                        {
                            return Result.Error<IList<PaymentEvent>>(error);
                        }
                    }
                }

                return events;
            }
            catch (Exception ex)
            {
                return Result.Error<IList<PaymentEvent>>(new StorageReadExceptionError(ex));
            }
        }

        public async Task<Result> SaveAsync(IList<PaymentEvent> paymentEvents, CancellationToken cancellationToken)
        {
            try
            {
                if (paymentEvents.Count == 0)
                {
                    LogEmptySaveRequest();
                    return Result.Ok;
                }

                var tasks = new List<Task>();

                foreach (var paymentEvent in paymentEvents)
                {
                    // Upsert each event (insert or replace)
                    tasks.Add(_container.CreateItemAsync(
                        paymentEvent,
                        new PartitionKey(paymentEvent.PaymentId),
                        cancellationToken: cancellationToken));
                }

                await Task.WhenAll(tasks);

                return Result.Ok;
            }
            catch (Exception ex)
            {
                return Result.Error<PaymentEvent>(new StorageWriteExceptionError(ex));
            }
        }

        private static OneOf<Type, UnknownEventTypeError> EventTypeToType(string eventType) => eventType switch
        {
            nameof(PaymentRequestedEvent) => typeof(PaymentRequestedEvent),
            nameof(PaymentScreenedEvent) => typeof(PaymentScreenedEvent),
            nameof(PaymentAuthorisedEvent) => typeof(PaymentAuthorisedEvent),
            nameof(PaymentDeclinedEvent) => typeof(PaymentDeclinedEvent),
            _ => new UnknownEventTypeError(eventType)
        };

        [LoggerMessage(Level = LogLevel.Information, Message = "Tried to save a payment with no changes")]
        partial void LogEmptySaveRequest();
    }
}
