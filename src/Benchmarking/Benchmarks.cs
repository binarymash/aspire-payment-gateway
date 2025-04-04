using BenchmarkDotNet.Attributes;

namespace Benchmarking
{
    [MemoryDiagnoser(false)]
    [DisassemblyDiagnoser(2)]
    public class Benchmarks
    {

        [GlobalSetup]
        public void Setup()
        {
            // This method is called once before all benchmarks
            // You can use it to set up any shared resources or data
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            // This method is called once after all benchmarks
            // You can use it to clean up any shared resources or data
        }

        [Benchmark]
        public void BenchmarkMethod1()
        {
            // This is the first benchmark method
            // Replace with your actual code to benchmark
            for (int i = 0; i < 1000000; i++)
            {
                var result = i * i;
            }
        }
    }
}
