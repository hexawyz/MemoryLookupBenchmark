namespace MemoryLookupBenchmark
{
	internal struct XorShiftRandom
	{
		public static XorShiftRandom Create(ulong seed) => new XorShiftRandom(seed);

		private ulong _state;

		private XorShiftRandom(ulong seed)
		{
			_state = seed;
		}

		public ulong Next()
		{
			ulong x = _state;
			x ^= x >> 12;
			x ^= x << 25;
			x ^= x >> 27;
			_state = x;
			return x * 0x2545F4914F6CDD1D;
		}
	}
}
