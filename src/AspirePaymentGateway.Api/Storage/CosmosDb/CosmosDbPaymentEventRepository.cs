using System.Text.Json;
using AspirePaymentGateway.Api.Features.Payments.Domain.Events;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using BinaryMash.Extensions.Results;
using Microsoft.Azure.Cosmos;
using static AspirePaymentGateway.Api.Features.Payments.Domain.Errors;

namespace AspirePaymentGateway.Api.Storage.CosmosDb
{
    public partial class CosmosDbPaymentEventRepository(ILogger<CosmosDbPaymentEventRepository> logger, CosmosClient cosmos, PaymentEventMapper mapper) : IPaymentEventsRepository
    {
        private readonly Container _container = cosmos.GetContainer("PaymentsDb", "Payments");

        public async Task<Result<IList<PaymentEvent>>> GetAsync(string paymentId, CancellationToken cancellationToken)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.paymentId = @paymentId ORDER BY c.occurredAt")
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
                        var map = mapper.MapToPaymentEvent((JsonElement)doc);

                        if (map.TryPickT0(out PaymentEvent paymentEvent, out ErrorDetail error))
                        {
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

        [LoggerMessage(Level = LogLevel.Information, Message = "Tried to save a payment with no changes")]
        partial void LogEmptySaveRequest();
    }
}
