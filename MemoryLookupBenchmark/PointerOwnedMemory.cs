using System;
using System.Buffers;
using System.Buffers.Current;

namespace MemoryLookupBenchmark
{
    public unsafe sealed class PointerOwnedMemory<T> : OwnedMemory<T>
    {
        private readonly byte* _pointer;
        private readonly long _offset;
        private readonly int _length;

        public unsafe PointerOwnedMemory(byte* pointer, long offset, int length)
        {
            _pointer = pointer;
            _offset = offset;
            _length = length;
        }

        public override int Length => _length;

        protected override void Dispose(bool disposing) { }

        public override bool IsDisposed => false;
        protected override bool IsRetained => true;

        public override unsafe Span<T> Span => new Span<T>(_pointer + _offset, _length);

        public override void Retain()
        {
        }

        public override bool Release()
        {
            return false;
        }

        public override unsafe MemoryHandle Pin(int byteOffset = 0) => new MemoryHandle(this, _pointer + _offset);

        protected override bool TryGetArray(out ArraySegment<T> arraySegment)
        {
            arraySegment = null;
            return false;
        }
    }
}