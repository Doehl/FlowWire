namespace FlowWire.Engine.Runtime;

/// <summary>
/// A pre-allocated memory segment for managing workflow state.
/// Implements a bump-pointer allocator to avoid GC overhead.
/// </summary>
public sealed class Arena(int size = 65536)
{
    private byte[] _buffer = new byte[size];
    private readonly int _initialSize = size;
    private int _head = 0;

    public int Position => _head;

    /// <summary>
    /// Gets the number of bytes currently used in the arena.
    /// </summary>
    public int Used => _head;

    /// <summary>
    /// Gets the total capacity of the arena.
    /// </summary>
    public int Capacity => _buffer.Length;

    /// <summary>
    /// Gets the remaining space in the arena.
    /// </summary>
    public int Free => _buffer.Length - _head;

    /// <summary>
    /// Reserves a span of memory without advancing the head immediately.
    /// MUST be followed by Advance() or Allocate().
    /// </summary>
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsSpan(_head);
    }
    
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsMemory(_head);
    }

    /// <summary>
    /// Advances the head by count bytes. Used after writing to GetSpan().
    /// </summary>
    public void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (_head + count > _buffer.Length) throw new OutOfMemoryException("Cannot advance beyond buffer size. Did you call GetSpan with enough size?");
        _head += count;
    }

    /// <summary>
    /// Resets the arena pointer to the beginning, effectively clearing the state.
    /// Does not zero out memory for performance reasons.
    /// </summary>
    public void Reset(bool shrink = false)
    {
        _head = 0;
        if (shrink && _buffer.Length > _initialSize)
        {
            _buffer = new byte[_initialSize];
        }
    }

    /// <summary>
    /// Allocates a slice of memory from the arena.
    /// </summary>
    /// <param name="size">Number of bytes to allocate.</param>
    /// <returns>A span representing the allocated memory.</returns>
    /// <exception cref="OutOfMemoryException">Thrown if the arena is full.</exception>
    public Span<byte> Allocate(int size)
    {
        EnsureCapacity(size);
        var slice = _buffer.AsSpan(_head, size);
        _head += size;
        return slice;
    }

    /// <summary>
    /// Gets the entire valid data segment.
    /// </summary>
    public ReadOnlySpan<byte> GetUsedMemory()
    {
        return _buffer.AsSpan(0, _head);
    }

    private void EnsureCapacity(int sizeHint)
    {
        if (sizeHint == 0) sizeHint = 1;
        int required = _head + sizeHint;
        if (required > _buffer.Length)
        {
            int newSize = Math.Max(_buffer.Length * 2, required);
            var newBuffer = new byte[newSize];
            Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _head);
            _buffer = newBuffer;
        }
    }
}
