using Microsoft.Extensions.Compliance.Classification;

namespace AspirePaymentGateway.Api.Extensions.Redaction
{
    public static class DataTaxonomy
    {
        public static string TaxonomyName { get; } = typeof(DataTaxonomy).FullName!;

        public static DataClassification SensitiveData { get; } = new(TaxonomyName, nameof(SensitiveData));

        public static DataClassification PiiData { get; } = new(TaxonomyName, nameof(PiiData));
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SensitiveDataAttribute : DataClassificationAttribute
    {
        public SensitiveDataAttribute() : base(DataTaxonomy.SensitiveData)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PiiDataAttribute : DataClassificationAttribute
    {
        public PiiDataAttribute() : base(DataTaxonomy.PiiData)
        {
        }
    }
}
