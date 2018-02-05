# Memory Lookup Benchmark

This project tries to benchmark the performance of methods of accessing large blocks of memory (e.g. memory-mapped files) with ````ReadOnlyBuffer<T>```` versus a more direct approach at indexing memory.

# What does the benchmark measure

The benchmark works on a 3,75GB memory mapped file, composed of many 5 bytes items filled with random data.
It compares the performance of copying data from the memory mapped file to a stack-based Span&lt;byte&gt;, between ````ReadOnlyBuffer<T>```` and a custom implementation.

# Results

``` ini
BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.192)
Intel Core i7-4578U CPU 3.00GHz (Haswell), 1 CPU, 4 logical cores and 2 physical cores
Frequency=2929690 Hz, Resolution=341.3330 ns, Timer=TSC
.NET Core SDK=2.1.4
  [Host]     : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT
```

|                                                                                    Method |       Mean |     Error |    StdDev | Scaled | ScaledSD |
|------------------------------------------------------------------------------------------ |-----------:|----------:|----------:|-------:|---------:|
|                                                    &#39;Only generate the random item index.&#39; |   1.774 ns | 0.0295 ns | 0.0262 ns |   0.01 |     0.00 |
|               &#39;Generate the random index, and copy static data to the stack using Spans.&#39; |  39.803 ns | 0.0751 ns | 0.0702 ns |   0.25 |     0.00 |
|                         &#39;Copy a random item to the stack using a locally generated Span.&#39; | 160.455 ns | 1.7740 ns | 1.6594 ns |   1.00 |     0.00 |
| &#39;Copy a random item to the stack using the custom implemented SafeBufferSlice&lt;T&gt; struct.&#39; | 168.540 ns | 3.3838 ns | 4.5172 ns |   1.05 |     0.03 |
|                     &#39;Copy a random item to the stack using the ReadOnlyBuffer&lt;T&gt; struct.&#39; | 329.546 ns | 3.3078 ns | 3.0941 ns |   2.05 |     0.03 |
