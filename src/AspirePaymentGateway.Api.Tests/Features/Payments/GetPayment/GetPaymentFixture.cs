using AspirePaymentGateway.Api.Features.Payments;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using AspirePaymentGateway.Api.Features.Payments.Validation;
using AspirePaymentGateway.Api.Storage.InMemory;
using Microsoft.Extensions.Logging;
using Moq;

namespace AspirePaymentGateway.Api.Tests.Features.Payments.GetPayment
{
    public class GetPaymentFixture
    {
        public GetPaymentHandler GetPaymentHandler { get; init; }
        public PaymentSession Session { get; init; }
        public InMemoryPaymentEventRepository Repository { get; init; }
        public Mock<ILogger<GetPaymentHandler>> Logger { get; init; }

        public GetPaymentFixture()
        {
            Repository = new InMemoryPaymentEventRepository();
            Session = new(Repository);
            Logger = new();

            GetPaymentHandler = new GetPaymentHandler(
                session: Session,
                logger: Logger.Object,
                validator: new PaymentIdValidator());
        }

        public GetPaymentFixture Reset()
        {
            Repository.Clear();
            return this;
        }
    }
}
