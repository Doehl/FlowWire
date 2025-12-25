namespace FlowWire.Engine.Internals;

/// <summary>
/// A zero-allocation list/queue for Commands.
/// </summary>
public sealed class CommandQueue(int initialCapacity = 32)
{
    private Command[] _buffer = new Command[initialCapacity];
    private int _count = 0;

    public int Count => _count;

    public void Enqueue(Command command)
    {
        if (_count == _buffer.Length)
        {
            Array.Resize(ref _buffer, _buffer.Length * 2);
        }
        _buffer[_count++] = command;
    }

    public void EnqueueRange(ReadOnlySpan<Command> commands)
    {
        int required = _count + commands.Length;
        if (required > _buffer.Length)
        {
            int newSize = Math.Max(_buffer.Length * 2, required);
            Array.Resize(ref _buffer, newSize);
        }

        commands.CopyTo(_buffer.AsSpan(_count));
        _count += commands.Length;
    }

    public ReadOnlySpan<Command> GetCommands()
    {
        return _buffer.AsSpan(0, _count);
    }

    public void Clear()
    {
        _count = 0;
    }
}
