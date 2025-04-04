using AspirePaymentGateway.Api.Extensions.DateTime;
using AspirePaymentGateway.Api.Storage.InMemory;
using AspirePaymentGateway.Api.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Moq;
using System.Diagnostics.Metrics;
using static AspirePaymentGateway.Api.Features.Payments.Contracts;
using AspirePaymentGateway.Api.Features.Payments;
using AspirePaymentGateway.Api.Features.Payments.Validation;
using AspirePaymentGateway.Api.Features.Payments.Services.FraudApi;
using AspirePaymentGateway.Api.Features.Payments.Services.BankApi;

namespace AspirePaymentGateway.Api.Tests.Features.Payments.CreatePayment
{
    public abstract class ComponentTests
    {
        protected readonly CreatePaymentHandler _handler;
        protected readonly PaymentRequestValidator _validator;
        protected readonly InMemoryPaymentEventRepository _repository;
        protected readonly Mock<IFraudApi> _fraudApi;
        protected readonly Mock<IBankApi> _bankApi;
        protected readonly StubbedDateTimeProvider _dateTimeProvider;

        protected readonly MetricCollector<long> _paymentRequestedCountCollector;
        protected readonly MetricCollector<long> _paymentFateCountCollector;

        public ComponentTests()
        {
            _validator = new();
            _repository = new();
            _fraudApi = new();
            _bankApi = new();
            _dateTimeProvider = new(DateTime.Parse("2022-01-01T00:00:00Z"));

            (var businessMetrics, var meterFactory) = SetUpBusinessMetrics();

            _paymentRequestedCountCollector = new MetricCollector<long>(meterFactory, BusinessMetrics.Name, BusinessMetrics.PaymentRequestedCountName);
            _paymentFateCountCollector = new MetricCollector<long>(meterFactory, BusinessMetrics.Name, BusinessMetrics.PaymentFateCountName);

            _handler = new CreatePaymentHandler(
                _validator,
                _repository,
                _fraudApi.Object,
                _bankApi.Object,
                businessMetrics,
                _dateTimeProvider);
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

        protected static PaymentRequest NominalRequest => new(
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
