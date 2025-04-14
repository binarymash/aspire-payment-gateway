using AspirePaymentGateway.Api.Extensions.DateTime;
using AspirePaymentGateway.Api.Features.Payments.Services.BankApi;
using AspirePaymentGateway.Api.Features.Payments.Services.FraudApi;
using AspirePaymentGateway.Api.Features.Payments.Validation;
using AspirePaymentGateway.Api.Features.Payments;
using AspirePaymentGateway.Api.Storage.InMemory;
using AspirePaymentGateway.Api.Telemetry;
using MELT;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;
using AspirePaymentGateway.Api.Features.Payments.Services.Storage;
using Moq;
using static AspirePaymentGateway.Api.Features.Payments.Contracts;
using System.Diagnostics.Metrics;

namespace AspirePaymentGateway.Api.Tests.Features.Payments.CreatePayment
{
    public class CreatePaymentFixture
    {
        public ITestLoggerFactory LoggerFactory { get; init; }
        public CreatePaymentHandler CreatePaymentHandler { get; init; }
        public PaymentSession Session { get; init; }
        public InMemoryPaymentEventRepository Repository { get; init; }
        public Mock<IFraudApi> FraudApi { get; init; }
        public Mock<IBankApi> BankApi { get; init; }
        public StubbedDateTimeProvider DateTimeProvider { get; init; }
        public ActivitySource ActivitySource { get; init; }
        public MetricCollector<long> PaymentRequestedCountCollector { get; init; }
        public MetricCollector<long> PaymentFateCountCollector { get; init; }

        public CreatePaymentFixture()
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
        }

        public CreatePaymentFixture Reset()
        {
            LoggerFactory.Sink.Clear();
            FraudApi.Reset();
            BankApi.Reset();
            //Repository
            PaymentRequestedCountCollector.Clear();
            PaymentFateCountCollector.Clear();
            return this;
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
    }
}
