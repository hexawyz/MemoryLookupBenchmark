using System;
using System.Runtime.InteropServices;

namespace MemoryLookupBenchmark
{
    /// <summary>Thin wrapper around <see cref="SafeBuffer"/> for slicing large memory blocks.</summary>
    /// <typeparam name="T"></typeparam>
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct SafeBufferSlice<T>
    {
        private readonly SafeBuffer _buffer;
        private readonly long _offset;
        private readonly int _length;

        internal SafeBufferSlice(SafeBuffer buffer, long offset, int length)
        {
            _buffer = buffer;
            _offset = offset;
            _length = length;
        }

        public unsafe ReadOnlySpan<T> Span => new ReadOnlySpan<T>((byte*)_buffer.DangerousGetHandle() + _offset, _length);
    }
}
