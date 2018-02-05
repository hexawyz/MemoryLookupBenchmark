using BenchmarkDotNet.Attributes;
using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace MemoryLookupBenchmark
{
    public class MemoryAccessBenchmark
    {
        public const int ItemLength = 5;
        public const long ItemCount = 0x3000_0000;
        public const long ItemCountMask = ItemCount - 1;

        private readonly MemoryMappedFileMemory _memory;
        private readonly ReadOnlyBuffer<byte> _readOnlyBuffer;
        private XorShiftRandom _random;
        private readonly IntPtr _constantMemoryPointer;

        public unsafe MemoryAccessBenchmark()
        {
            // Create a memory mapped file and fill it with random bytes
            _memory = new MemoryMappedFileMemory(ItemLength * ItemCount);
            GenerateRandomBytes(_memory.DataPointer, _memory.Length);
            _readOnlyBuffer = _memory.CreateReadOnlyBuffer();

            // Create the same RNG for both benchmarks
            _random = XorShiftRandom.Create(0x6780867534);

            // Allocate 5 bytes for the 
            _constantMemoryPointer = Marshal.AllocHGlobal(5);
        }

        private static unsafe void GenerateRandomBytes(byte* pointer, long count)
        {
            // Forget about alignment since this test will run on x64
            ulong* p = (ulong*)pointer;
            long c = count >> 3;

            var random = XorShiftRandom.Create(0x2985194649313);

            for (int i = 0; i < c; i++)
            {
                *p++ = random.Next();
            }

            int remainingBytes = (int)(count & 0x7);

            if (remainingBytes > 0)
            {
                ulong n = random.Next();

                new Span<byte>(&n, remainingBytes).CopyTo(new Span<byte>(p, remainingBytes));
            }
        }

        [Benchmark(Description = "Only generate the random item index.")]
        public long RandomOnly() => (long)(_random.Next() & ItemCountMask) * ItemLength;

        [Benchmark(Description = "Generate the random index, and copy static data to the stack using Spans.")]
        public unsafe long NoRandomAccess()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            long index = (long)(_random.Next() & ItemCountMask) * ItemLength;
            new ReadOnlySpan<byte>((void*)_constantMemoryPointer, ItemLength).CopyTo(destination);
            return index;
        }

        [Benchmark(Baseline = true, Description = "Copy a random item to the stack using a locally generated Span.")]
        public unsafe void Direct()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            new ReadOnlySpan<byte>(_memory.DataPointer + (long)(_random.Next() & ItemCountMask) * ItemLength, ItemLength).CopyTo(destination);
        }

        [Benchmark(Description = "Copy a random item to the stack using the custom implemented SafeBufferSlice<T> struct.")]
        public void Custom()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            _memory.Slice((long)(_random.Next() & ItemCountMask) * ItemLength, ItemLength).Span.CopyTo(destination);
        }

        [Benchmark(Description = "Copy a random item to the stack using the ReadOnlyBuffer<T> struct.")]
        public void ReadOnlyBuffer()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            _readOnlyBuffer.Slice((long)(_random.Next() & ItemCountMask) * ItemLength, ItemLength).CopyTo(destination);
        }
    }
}
