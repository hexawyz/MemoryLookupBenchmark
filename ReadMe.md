# Memory Lookup Benchmark

This project tries to benchmark the performance of methods of accessing large blocks of memory (e.g. memory-mapped files) with ````ReadOnlyBuffer<T>```` versus a more direct approach at indexing memory.

# What does the benchmark measure

The benchmark works on a 3,75GB memory mapped file, composed of many 5 bytes items filled with random data.
It compares the performance of copying data from the memory mapped file to a stack-based Span&lt;byte&gt;, between ````ReadOnlyBuffer<T>```` and a custom implementation.

# Results

````
// * Detailed results *
MemoryAccessBenchmark.Custom: DefaultJob
Runtime = .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT; GC = Concurrent Workstation
Mean = 142.7912 ns, StdErr = 0.1426 ns (0.10%); N = 13, StdDev = 0.5141 ns
Min = 141.7743 ns, Q1 = 142.5432 ns, Median = 142.7881 ns, Q3 = 142.9924 ns, Max = 144.0505 ns
IQR = 0.4492 ns, LowerFence = 141.8693 ns, UpperFence = 143.6662 ns
ConfidenceInterval = [142.1754 ns; 143.4069 ns] (CI 99.9%), Margin = 0.6157 ns (0.43% of Mean)
Skewness = 0.54, Kurtosis = 4


MemoryAccessBenchmark.ReadOnlyBuffer: DefaultJob
Runtime = .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT; GC = Concurrent Workstation
Mean = 294.7868 ns, StdErr = 0.8890 ns (0.30%); N = 13, StdDev = 3.2054 ns
Min = 290.1037 ns, Q1 = 293.1232 ns, Median = 294.3099 ns, Q3 = 295.5389 ns, Max = 303.0835 ns
IQR = 2.4157 ns, LowerFence = 289.4996 ns, UpperFence = 299.1624 ns
ConfidenceInterval = [290.9482 ns; 298.6254 ns] (CI 99.9%), Margin = 3.8386 ns (1.30% of Mean)
Skewness = 1.04, Kurtosis = 4.03


Total time: 00:00:43 (43.36 sec)

// * Summary *

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.192)
Intel Core i7-3720QM CPU 2.60GHz (Ivy Bridge), 1 CPU, 8 logical cores and 4 physical cores
Frequency=2533317 Hz, Resolution=394.7394 ns, Timer=TSC
.NET Core SDK=2.1.4
  [Host]     : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT


         Method |     Mean |     Error |    StdDev |
--------------- |---------:|----------:|----------:|
         Custom | 142.8 ns | 0.6157 ns | 0.5141 ns |
 ReadOnlyBuffer | 294.8 ns | 3.8386 ns | 3.2054 ns |
````