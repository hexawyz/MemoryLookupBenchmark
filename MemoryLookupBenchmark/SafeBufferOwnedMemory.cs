using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace MemoryLookupBenchmark
{
	/// <summary>Wrapper around <see cref="SafeBuffer"/> usable together with <see cref="ReadOnlyBuffer{T}"/>.</summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class SafeBufferOwnedMemory<T> : OwnedMemory<T>, IMemoryList<T>
	{
		public static ReadOnlyBuffer<T> CreateReadOnlyBuffer(SafeBuffer buffer, long offset, long length)
		{
			int count = (int)(length & 0x3FFF_FFFF);
			length -= count;
			offset += length;
			var last = new SafeBufferOwnedMemory<T>(buffer, offset, count, null);
			var first = last;

			while (length > 0)
			{
				length -= 0x4000_0000;
				offset -= 0x4000_0000;
				first = new SafeBufferOwnedMemory<T>(buffer, offset, 0x4000_0000, first);
			}

			return new ReadOnlyBuffer<T>(first, 0, last, count);
		}

		private readonly SafeBuffer _buffer;
		private readonly long _offset;
		private readonly int _length;
		private readonly SafeBufferOwnedMemory<T> _next;

		private SafeBufferOwnedMemory(SafeBuffer buffer, long offset, int length, SafeBufferOwnedMemory<T> next)
		{
			_buffer = buffer;
			_offset = offset;
			_length = length;
			_next = next;
		}

		public long RunningIndex => _offset;
		public override int Length => _length;
		public IMemoryList<T> Next => _next;

		protected override void Dispose(bool disposing) { }

		public override bool IsDisposed => _buffer.IsClosed;
		protected override bool IsRetained => !_buffer.IsClosed;

		public override unsafe Span<T> Span => new Span<T>((byte*)_buffer.DangerousGetHandle() + _offset, _length);
		
		public override void Retain()
		{
			bool success = false;
			_buffer.DangerousAddRef(ref success);
		}

		public override bool Release()
		{
			_buffer.DangerousRelease();
			return _buffer.IsClosed;
		}

		public override unsafe MemoryHandle Pin(int byteOffset = 0) => new MemoryHandle(this, (byte*)_buffer.DangerousGetHandle() + _offset);

		protected override bool TryGetArray(out ArraySegment<T> arraySegment)
		{
			arraySegment = null;
			return false;
		}
	}
}
