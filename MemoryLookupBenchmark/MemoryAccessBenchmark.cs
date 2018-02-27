using BenchmarkDotNet.Attributes;
using System;
using System.Buffers.ReadOnlySequenceSegment;
using System.Buffers.Current;
using System.Buffers.IMemoryList;
using System.Runtime.InteropServices;
using System.Buffers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Attributes.Columns;

namespace MemoryLookupBenchmark
{
    [Config(typeof(DefaultCoreConfig))]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class MemoryAccessBenchmark
    {
        public const int ItemLength = 5;
        public const long ItemCount = 0x3000_0000;
        public const long ItemCountMask = ItemCount - 1;

        private readonly MemoryMappedFileMemory _mmMemory;

        private readonly System.Buffers.ReadOnlySequenceSegment.ReadOnlySequence<byte> _singleReadOnlyBufferROSS;
        private readonly System.Buffers.IMemoryList.ReadOnlySequence<byte> _singleReadOnlyBufferIMemoryList;
        private readonly System.Buffers.Current.ReadOnlySequence<byte> _singleReadOnlyBufferCurrent;

        private readonly System.Buffers.ReadOnlySequenceSegment.ReadOnlySequence<byte> _multiReadOnlyBufferROSS;
        private readonly System.Buffers.IMemoryList.ReadOnlySequence<byte> _multiReadOnlyBufferIMemoryList;
        private readonly System.Buffers.Current.ReadOnlySequence<byte> _multiReadOnlyBufferCurrent;

        private readonly System.Buffers.ReadOnlySequenceSegment.ReadOnlySequence<byte> _readOnlyBufferROSS;
        private readonly System.Buffers.IMemoryList.ReadOnlySequence<byte> _readOnlyBufferIMemoryList;
        private readonly System.Buffers.Current.ReadOnlySequence<byte> _readOnlyBufferCurrent;
        private XorShiftRandom _random;
        private readonly IntPtr _constantMemoryPointer;

        public unsafe MemoryAccessBenchmark()
        {
            // Create a memory mapped file and fill it with random bytes
            _mmMemory = new MemoryMappedFileMemory(ItemLength * ItemCount);
            GenerateRandomBytes(_mmMemory.DataPointer, _mmMemory.Length);
            _readOnlyBufferROSS = _mmMemory.CreateReadOnlyBufferROSS();
            _readOnlyBufferIMemoryList = _mmMemory.CreateReadOnlyBufferIMemoryList();
            _readOnlyBufferCurrent = _mmMemory.CreateReadOnlyBufferCurrent();

            // Create the same RNG for both benchmarks
            _random = XorShiftRandom.Create(0x6780867534);

            // Allocate 5 bytes for the 
            _constantMemoryPointer = Marshal.AllocHGlobal(5);
            var ownedMemory = new PointerOwnedMemory<byte>((byte*)_constantMemoryPointer, 0, 5);
            _singleReadOnlyBufferROSS = ReadOnlySequenceSegment.OwnedMemorySegment<byte>.CreateSingleSequence(ownedMemory);
            _singleReadOnlyBufferIMemoryList = IMemoryList.OwnedMemorySegment<byte>.CreateSingleSequence(ownedMemory);
            _singleReadOnlyBufferCurrent = Current.OwnedMemorySegment<byte>.CreateSingleSequence(ownedMemory);

            _multiReadOnlyBufferROSS = ReadOnlySequenceSegment.OwnedMemorySegment<byte>.CreateMultiSequence(ownedMemory);
            _multiReadOnlyBufferIMemoryList = IMemoryList.OwnedMemorySegment<byte>.CreateMultiSequence(ownedMemory);
            _multiReadOnlyBufferCurrent = Current.OwnedMemorySegment<byte>.CreateMultiSequence(ownedMemory);
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

        //[Benchmark(Description = "Generate random index")]
        //public long RandomOnly() => (long)(_random.Next() & ItemCountMask) * ItemLength;

        //[BenchmarkCategory("1 segment"), Benchmark(Description = "Copy static data. Local Span")]
        //public unsafe long NoRandomAccess()
        //{
        //    Span<byte> destination = stackalloc byte[ItemLength];
        //    long index = (long)(_random.Next() & ItemCountMask) * ItemLength;
        //    new ReadOnlySpan<byte>((void*)_constantMemoryPointer, ItemLength).CopyTo(destination);
        //    return index;
        //}


        [BenchmarkCategory("1 segment"), Benchmark(Baseline = true, Description = "ReadOnlySequence<T> (current)")]
        public unsafe long SingleSegmentCurrent()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            long index = (long)(_random.Next() & ItemCountMask) * ItemLength;
            _singleReadOnlyBufferCurrent.Slice(0, ItemLength).CopyTo(destination);
            return index;
        }

        [BenchmarkCategory("1 segment"), Benchmark(Description = "ReadOnlySequence<T> (PR dotnet/corefx#27455)")]
        public unsafe long SingleSegmentIMemoryList()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            long index = (long)(_random.Next() & ItemCountMask) * ItemLength;
            _singleReadOnlyBufferIMemoryList.Slice(0, ItemLength).CopyTo(destination);
            return index;
        }

        [BenchmarkCategory("1 segment"), Benchmark(Description = "ReadOnlySequence<T> (PR dotnet/corefx#27499)")]
        public unsafe long SingleSegmentROSS()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            long index = (long)(_random.Next() & ItemCountMask) * ItemLength;
            _singleReadOnlyBufferROSS.Slice(0, ItemLength).CopyTo(destination);
            return index;
        }

        [BenchmarkCategory("100 segments"), Benchmark(Baseline = true, Description = "ReadOnlySequence<T> (current)")]
        public unsafe long MultiSegmentCurrent()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            long index = (long)(_random.Next() & ItemCountMask) * ItemLength;
            _multiReadOnlyBufferCurrent.Slice(ItemLength * 99, ItemLength).CopyTo(destination);
            return index;
        }

        [BenchmarkCategory("100 segments"), Benchmark(Description = "ReadOnlySequence<T> (PR dotnet/corefx#27455)")]
        public unsafe long MultiSegmentIMemoryList()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            long index = (long)(_random.Next() & ItemCountMask) * ItemLength;
            _multiReadOnlyBufferIMemoryList.Slice(ItemLength * 99, ItemLength).CopyTo(destination);
            return index;
        }

        [BenchmarkCategory("100 segments"), Benchmark(Description = "ReadOnlySequence<T> (PR dotnet/corefx#27499)")]
        public unsafe long MultiSegmentROSS()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            long index = (long)(_random.Next() & ItemCountMask) * ItemLength;
            _multiReadOnlyBufferROSS.Slice(ItemLength * 99, ItemLength).CopyTo(destination);
            return index;
        }

        [BenchmarkCategory("MM item"), Benchmark(Description = "Span<T>")]
        public unsafe void Direct()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            new ReadOnlySpan<byte>(_mmMemory.DataPointer + (long)(_random.Next() & ItemCountMask) * ItemLength, ItemLength).CopyTo(destination);
        }

        [BenchmarkCategory("MM item"), Benchmark(Description = "BufferSlice<T>")]
        public void Custom()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            _mmMemory.Slice((long)(_random.Next() & ItemCountMask) * ItemLength, ItemLength).Span.CopyTo(destination);
        }

        [BenchmarkCategory("MM item"), Benchmark(Baseline = true, Description = "ReadOnlySequence<T> (current)")]
        public void ReadOnlyBufferCurrent()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            _readOnlyBufferCurrent.Slice((long)(_random.Next() & ItemCountMask) * ItemLength, ItemLength).CopyTo(destination);
        }

        [BenchmarkCategory("MM item"), Benchmark(Description = "ReadOnlySequence<T> (PR dotnet/corefx#27455)")]
        public void ReadOnlyBufferIMemoryList()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            _readOnlyBufferIMemoryList.Slice((long)(_random.Next() & ItemCountMask) * ItemLength, ItemLength).CopyTo(destination);
        }

        [BenchmarkCategory("MM item"), Benchmark(Description = "ReadOnlySequence<T> (PR dotnet/corefx#27499)")]
        public void ReadOnlyBufferROSS()
        {
            Span<byte> destination = stackalloc byte[ItemLength];
            _readOnlyBufferROSS.Slice((long)(_random.Next() & ItemCountMask) * ItemLength, ItemLength).CopyTo(destination);
        }
    }
}
