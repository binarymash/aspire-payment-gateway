using System.Diagnostics;

namespace AspirePaymentGateway.Api
{
    public static class ActivitySourceHelper
    {
        public static readonly ActivitySource ActivitySource = new(
            name: "AspirePaymentGateway.Api", 
            version: System.Reflection.Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString());
    }
}
