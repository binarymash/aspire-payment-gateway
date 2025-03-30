using System.Diagnostics.Metrics;

namespace AspirePaymentGateway.Api.Telemetry
{
    public class BusinessMetrics
    {
        public const string Name = "BinaryMash.AspirePaymentGateway";
        public const string PaymentFateCountName = "payment.fate.count";
        public const string PaymentRequestedCountName = "payment.requested.count";

        private readonly Meter _meter;
        private readonly Counter<long> _paymentFateCount;
        private readonly Counter<long> _paymentRequestedCount;

        public BusinessMetrics(IMeterFactory meterFactory)
        {
            _meter = meterFactory.Create(Name);

            _paymentFateCount = _meter.CreateCounter<long>(PaymentFateCountName);
            _paymentRequestedCount = _meter.CreateCounter<long>(PaymentRequestedCountName);
        }

        public void RecordPaymentRequestAccepted()
        {
            RecordPaymentRequest(true);
        }

        public void RecordPaymentRequestRejected()
        {
            RecordPaymentRequest(false);
        }

        public void RecordPaymentFate(string fate, string? reason = null)
        {
            _paymentFateCount.Add(1, [new("Fate", fate), new("Reason", reason)]);
        }

        private void RecordPaymentRequest(bool accepted)
        {
            _paymentRequestedCount.Add(1, [new("Valid", accepted)]);
        }

    }
}
