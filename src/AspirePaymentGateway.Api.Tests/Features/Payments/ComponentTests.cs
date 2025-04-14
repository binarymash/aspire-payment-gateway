using System.Diagnostics;
using System.Diagnostics.Metrics;
using AspirePaymentGateway.Api.Extensions.DateTime;
using AspirePaymentGateway.Api.Features.Payments;
using AspirePaymentGateway.Api.Features.Payments.Validation;
using AspirePaymentGateway.Api.Features.Payments.Services.BankApi;
using AspirePaymentGateway.Api.Features.Payments.Services.FraudApi;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using AspirePaymentGateway.Api.Storage.InMemory;
using AspirePaymentGateway.Api.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Microsoft.Extensions.Logging;
using MELT;
using Moq;
using static AspirePaymentGateway.Api.Features.Payments.Contracts;
using System.Globalization;

namespace AspirePaymentGateway.Api.Tests.Features.Payments
{
    public abstract class ComponentTests
    {
        protected ITestLoggerFactory LoggerFactory { get; init; }
        protected CreatePaymentHandler CreatePaymentHandler { get; init; }
        protected GetPaymentHandler GetPaymentHandler { get; init; }
        protected PaymentSession Session { get; init; }
        protected InMemoryPaymentEventRepository Repository { get; init; }
        protected Mock<IFraudApi> FraudApi { get; init; }
        protected Mock<IBankApi> BankApi { get; init; }
        protected StubbedDateTimeProvider DateTimeProvider { get; init; }
        protected ActivitySource ActivitySource { get; init; }
        protected MetricCollector<long> PaymentRequestedCountCollector { get; init; }
        protected MetricCollector<long> PaymentFateCountCollector { get; init; }

        public ComponentTests()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddDomainServices();

            var serviceProvider = services.BuildServiceProvider();

            LoggerFactory = TestLoggerFactory.Create();
            Repository = new InMemoryPaymentEventRepository();
            Session = new(Repository);
            FraudApi = new();
            BankApi = new();
            DateTimeProvider = new(DateTime.Parse("2022-01-01T00:00:00Z", CultureInfo.InvariantCulture));
            ActivitySource = ActivitySourceHelper.ActivitySource;

            (var businessMetrics, var meterFactory) = SetUpBusinessMetrics();

            PaymentRequestedCountCollector = new MetricCollector<long>(meterFactory, BusinessMetrics.Name, BusinessMetrics.PaymentRequestedCountName);
            PaymentFateCountCollector = new MetricCollector<long>(meterFactory, BusinessMetrics.Name, BusinessMetrics.PaymentFateCountName);

            CreatePaymentHandler = new CreatePaymentHandler(
                logger: LoggerFactory.CreateLogger<CreatePaymentHandler>(),
                validator: new PaymentRequestValidator(),
                session: Session,
                fraudApi: FraudApi.Object,
                bankApi: BankApi.Object,
                businessMetrics,
                dateTimeProvider: DateTimeProvider,
                activitySource: ActivitySource);

            GetPaymentHandler = new GetPaymentHandler(
                session: Session, 
                logger: LoggerFactory.CreateLogger<GetPaymentHandler>(),
                validator: new PaymentIdValidator());
        }

        private static (BusinessMetrics, IMeterFactory) SetUpBusinessMetrics()
        {
            var services = new ServiceCollection()
                .AddMetrics()
                .AddSingleton<BusinessMetrics>()
                .BuildServiceProvider();

            var businessMetrics = services.GetRequiredService<BusinessMetrics>();
            var meterFactory = services.GetRequiredService<IMeterFactory>();

            return (businessMetrics, meterFactory);
        }

        protected static PaymentRequest NominalPaymentRequest => new(
            new CardDetails(
                CardNumber: "4444333322221111",
                CardHolderName: "Philip Wood",
                Expiry: new CardExpiry(
                    Month: 5,
                    Year: 2029),
                CVV: 123),
            new PaymentDetails(
                Amount: 4567L,
                CurrencyCode: "GBP"));
    }
}
