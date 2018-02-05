using BenchmarkDotNet.Running;

namespace MemoryLookupBenchmark
{
	internal static class Program
    {
		private static void Main(string[] args) => BenchmarkRunner.Run<MemoryAccessBenchmark>();
	}
}
