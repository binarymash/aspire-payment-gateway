using System.Diagnostics.Metrics;

namespace AspirePaymentGateway.Api.Telemetry
{
    public class BusinessMetrics
    {
        public const string Name = "BinaryMash.AspirePaymentGateway";

        private readonly Meter _meter;
        private readonly Counter<long> _paymentFateCount;
        private readonly Counter<long> _paymentRequestedCount;

        public BusinessMetrics(IMeterFactory meterFactory)
        {
            _meter = meterFactory.Create(Name);

            _paymentFateCount = _meter.CreateCounter<long>("payment.fate.count");
            _paymentRequestedCount = _meter.CreateCounter<long>("payment.requested.count");
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
