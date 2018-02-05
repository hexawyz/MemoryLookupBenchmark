using BenchmarkDotNet.Attributes;
using System;
using System.Buffers;

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
		
		public unsafe MemoryAccessBenchmark()
		{
			// Create a memory mapped file and fill it with random bytes
			_memory = new MemoryMappedFileMemory(ItemLength * ItemCount);
			GenerateRandomBytes(_memory.DataPointer, _memory.Length);
			_readOnlyBuffer = _memory.CreateReadOnlyBuffer();

			// Create the same RNG for both benchmarks
			_random = XorShiftRandom.Create(0x6780867534);
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

		[Benchmark]
		public void Custom()
		{
			Span<byte> destination = stackalloc byte[ItemLength];
			_memory.Slice((long)(_random.Next() & ItemCountMask), ItemLength).Span.CopyTo(destination);
		}

		[Benchmark]
		public void ReadOnlyBuffer()
		{
			Span<byte> destination = stackalloc byte[ItemLength];
			_readOnlyBuffer.Slice((long)(_random.Next() & ItemCountMask), ItemLength).CopyTo(destination);
		}
	}
}
