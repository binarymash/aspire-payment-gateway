using System.Runtime.CompilerServices;

namespace AspirePaymentGateway.Tests
{
    public static class ModuleInitializer
    {
        [ModuleInitializer]
        public static void Initialize() =>
            VerifierSettings.InitializePlugins();
    }
}
