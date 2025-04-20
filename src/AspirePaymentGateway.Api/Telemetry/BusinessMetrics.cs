using System.Diagnostics.Metrics;

namespace AspirePaymentGateway.Api.Telemetry
{
    public class BusinessMetrics
    {
        public const string Name = "BinaryMash.AspirePaymentGateway";
        public const string PaymentFateCountName = "payment.fate.count";
        public const string PaymentRequestedCountName = "payment.requested.count";

        private readonly Counter<long> _paymentFateCount;
        private readonly Counter<long> _paymentRequestedCount;

        public BusinessMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create(Name);

            _paymentFateCount = meter.CreateCounter<long>(PaymentFateCountName);
            _paymentRequestedCount = meter.CreateCounter<long>(PaymentRequestedCountName);
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
            KeyValuePair<string, object?> fateTag = new("Fate", fate);
            KeyValuePair<string, object?> reasonTag = new("Reason", reason);

            _paymentFateCount.Add(1, fateTag, reasonTag);
        }

        private void RecordPaymentRequest(bool accepted)
        {
            KeyValuePair<string, object?> validTag = new("Valid", accepted);

            _paymentRequestedCount.Add(1, validTag);
        }
    }
}
