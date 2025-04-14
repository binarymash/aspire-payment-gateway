using AspirePaymentGateway.Api.Extensions.DateTime;
using AspirePaymentGateway.Api.Features.Payments;
using AspirePaymentGateway.Api.Telemetry;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Moq;
using Moq.AutoMock;
using System.Diagnostics.Metrics;
using System.Globalization;
using static AspirePaymentGateway.Api.Features.Payments.Contracts;

namespace AspirePaymentGateway.Api.Tests.Features.Payments.CreatePayment
{
    public class CreatePaymentHandlerTests : IDisposable
    {
        private readonly MetricCollector<int> _collector;
        readonly AutoMocker _mocker;
        readonly CreatePaymentHandler _handler;
        private bool disposedValue;

        public CreatePaymentHandlerTests()
        {
            var serviceCollection = new ServiceCollection()
                .AddMetrics()
                .AddSingleton<BusinessMetrics>();

            var services = serviceCollection.BuildServiceProvider();

            var metrics = services.GetRequiredService<BusinessMetrics>();
            var meterFactory = services.GetRequiredService<IMeterFactory>();
            _collector = new MetricCollector<int>(meterFactory, "HatCo.Store", "hatco.store.hats_sold");

            _mocker = new AutoMocker();
            _mocker.Use<BusinessMetrics>(metrics);
            _mocker.Use<IDateTimeProvider>(new StubbedDateTimeProvider(DateTime.Parse("2022-01-01T00:00:00Z", CultureInfo.InvariantCulture)));

            _handler = _mocker.CreateInstance<CreatePaymentHandler>();
        }

        [Fact(Skip ="WIP")]
        public async Task InvalidRequest()
        {
            ValidationResult failedResult = new([new ValidationFailure("someProperty", "an error message")]);

            _mocker.GetMock<IValidator<PaymentRequest>>()
                .Setup(val => val.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);

            PaymentRequest request = new(
                new CardDetails(
                    "4444333322221111", 
                    "Philip Wood", 
                    new CardExpiry(05, 2029),
                    123),
                new PaymentDetails(4567L, "GBP"));

            var result = await _handler.PostPaymentAsync(request, CancellationToken.None);

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _collector.Dispose();
                }

                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CreatePaymentHandlerTests()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
