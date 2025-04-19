using System.Diagnostics;
using System.Globalization;
using AspirePaymentGateway.Api.Extensions.DateTime;
using AspirePaymentGateway.Api.Features.Payments;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using AspirePaymentGateway.Api.Features.Payments.Validation;
using AspirePaymentGateway.Api.Storage.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace AspirePaymentGateway.Api.Tests.Features.Payments.GetPayment
{
    public class GetPaymentFixture
    {
        public GetPaymentHandler GetPaymentHandler { get; init; }
        public PaymentSession Session { get; init; }
        public InMemoryPaymentEventRepository Repository { get; init; }
        public StubbedDateTimeProvider DateTimeProvider { get; init; }
        public ActivitySource ActivitySource { get; init; }

        public Mock<ILogger<GetPaymentHandler>> Logger { get; init; }

        public GetPaymentFixture()
        {
            ServiceCollection services = new();
            services.AddDomainServices();

            var serviceProvider = services.BuildServiceProvider();

            Repository = new InMemoryPaymentEventRepository();
            Session = new(Repository);
            Logger = new();
            DateTimeProvider = new(DateTime.Parse("2022-01-01T00:00:00Z", CultureInfo.InvariantCulture));
            ActivitySource = ActivitySourceHelper.ActivitySource;

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
