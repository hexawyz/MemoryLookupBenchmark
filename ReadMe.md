# Memory Lookup Benchmark

This project tries to benchmark the performance of methods of accessing large blocks of memory (e.g. memory-mapped files) with ````ReadOnlySequence<T>```` versus a more direct approach at indexing memory.

# What does the benchmark measure

The benchmark works on a 3,75GB memory mapped file, composed of many 5 bytes items filled with random data.
It compares the performance of copying data from the memory mapped file to a stack-based Span&lt;byte&gt;, between various ````ReadOnlySequence<T>```` implementations and a custom class.

# Results

``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.192)
Intel Core i7-3720QM CPU 2.60GHz (Ivy Bridge), 1 CPU, 8 logical cores and 4 physical cores
Frequency=2533319 Hz, Resolution=394.7391 ns, Timer=TSC
.NET Core SDK=2.1.300-preview1-008174
  [Host]     : .NET Core 2.1.0-preview1-26216-03 (Framework 4.6.26216.04), 64bit RyuJIT
  Job-KYWOSJ : .NET Core 2.1.0-preview1-26216-03 (Framework 4.6.26216.04), 64bit RyuJIT

RemoveOutliers=False  Runtime=Core  Server=True  
LaunchCount=3  RunStrategy=Throughput  TargetCount=10  
WarmupCount=5  

```
|                                         Method |    Categories |        Mean |      Error |    StdDev |         Op/s | Scaled | ScaledSD | Allocated |
|----------------------------------------------- |-------------- |------------:|-----------:|----------:|-------------:|-------:|---------:|----------:|
|                &#39;ReadOnlySequence&lt;T&gt; (current)&#39; |     1 segment |   136.78 ns |  0.7901 ns |  1.183 ns |  7,311,039.5 |   1.00 |     0.00 |       0 B |
| &#39;ReadOnlySequence&lt;T&gt; (PR dotnet/corefx#27455)&#39; |     1 segment |   111.76 ns |  1.4777 ns |  2.212 ns |  8,947,805.1 |   0.82 |     0.02 |       0 B |
| &#39;ReadOnlySequence&lt;T&gt; (PR dotnet/corefx#27499)&#39; |     1 segment |    99.55 ns |  0.7943 ns |  1.189 ns | 10,044,753.2 |   0.73 |     0.01 |       0 B |
|                                                |               |             |            |           |              |        |          |           |
|                &#39;ReadOnlySequence&lt;T&gt; (current)&#39; |  100 segments | 1,582.65 ns |  6.2641 ns |  9.376 ns |    631,852.0 |   1.00 |     0.00 |       0 B |
| &#39;ReadOnlySequence&lt;T&gt; (PR dotnet/corefx#27455)&#39; |  100 segments | 1,091.69 ns |  4.5283 ns |  6.778 ns |    916,013.8 |   0.69 |     0.01 |       0 B |
| &#39;ReadOnlySequence&lt;T&gt; (PR dotnet/corefx#27499)&#39; |  100 segments |   286.55 ns |  2.0150 ns |  3.016 ns |  3,489,793.6 |   0.18 |     0.00 |       0 B |
|                                                |               |             |            |           |              |        |          |           |
|                &#39;ReadOnlySequence&lt;T&gt; (current)&#39; | 1000 segments | 1,628.14 ns | 10.5124 ns | 15.735 ns |    614,198.1 |   1.00 |     0.00 |       0 B |
| &#39;ReadOnlySequence&lt;T&gt; (PR dotnet/corefx#27455)&#39; | 1000 segments | 1,251.71 ns |  6.7948 ns | 10.170 ns |    798,906.5 |   0.77 |     0.01 |       0 B |
| &#39;ReadOnlySequence&lt;T&gt; (PR dotnet/corefx#27499)&#39; | 1000 segments |   359.72 ns |  1.5800 ns |  2.365 ns |  2,779,974.5 |   0.22 |     0.00 |       0 B |
|                                                |               |             |            |           |              |        |          |           |
|                                        Span&lt;T&gt; |       MM item |   166.36 ns |  3.6417 ns |  5.451 ns |  6,011,103.4 |   0.54 |     0.02 |       0 B |
|                                 BufferSlice&lt;T&gt; |       MM item |   167.92 ns |  1.4966 ns |  2.240 ns |  5,955,217.6 |   0.54 |     0.01 |       0 B |
|                &#39;ReadOnlySequence&lt;T&gt; (current)&#39; |       MM item |   308.96 ns |  2.3315 ns |  3.490 ns |  3,236,672.7 |   1.00 |     0.00 |       0 B |
| &#39;ReadOnlySequence&lt;T&gt; (PR dotnet/corefx#27455)&#39; |       MM item |   284.28 ns |  3.9080 ns |  5.849 ns |  3,517,663.2 |   0.92 |     0.02 |       0 B |
| &#39;ReadOnlySequence&lt;T&gt; (PR dotnet/corefx#27499)&#39; |       MM item |   240.50 ns |  2.7957 ns |  4.185 ns |  4,157,924.2 |   0.78 |     0.02 |       0 B |
