using FlowWire.Core;
using FlowWire.Engine.Exceptions;
using FlowWire.Engine.Internals;
using FlowWire.Engine.IO;
using System.Text.Json;

namespace FlowWire.Engine.Runtime;

using Microsoft.Extensions.ObjectPool;

internal sealed class WorkflowContextImpl : IWorkflowContext, IResettable
{
    private long _activeGeneration;
    private long _currentGeneration;

    private readonly Arena _arena;
    private readonly CommandQueue _commands;
    private readonly ArenaBufferWriter _bufferWriter;

    internal CommandQueue Commands => _commands;

    private ReadOnlyMemory<byte> _history;
    private int _historyOffset;
    private CancellationToken _cancellationToken;

    private readonly DeterministicRandomAdapter _random;
    private DateTime _now;

    public DateTime Now
    {
        get
        {
            EnsureValid();
            return _now;
        }
    }

    public Random Random
    {
        get
        {
            EnsureValid();
            return _random;
        }
    }

    public CancellationToken CancellationToken => _cancellationToken;

    public WorkflowContextImpl()
    {
        // Allocate once per instance life
        _arena = new Arena();
        _commands = new CommandQueue();
        _bufferWriter = new ArenaBufferWriter(_arena);
        _random = new DeterministicRandomAdapter(0); // Seeded later

        _activeGeneration = 0;
        _currentGeneration = 0;
    }

    public void Initialize(ReadOnlyMemory<byte> history, long seed, DateTime now, CancellationToken cancellationToken)
    {
        _history = history;
        _historyOffset = 0;
        _now = now;
        _random.Reset((ulong)seed);

        _cancellationToken = cancellationToken;

        _currentGeneration++;
        _activeGeneration = _currentGeneration;
    }

    public bool TryReset()
    {
        _activeGeneration = -1;

        _arena.Reset(shrink: true);
        _commands.Clear();

        _history = default;
        _cancellationToken = default;

        return true;
    }

    private void EnsureValid()
    {
        if (_currentGeneration != _activeGeneration)
        {
            throw new InvalidWorkflowContextException();
        }
    }

    public Guid NewGuid()
    {
        EnsureValid();
        var buffer = new byte[16];
        _random.NextBytes(buffer);
        return new Guid(buffer);
    }

    public async Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        EnsureValid();
        cancellationToken.ThrowIfCancellationRequested();

        // Replay Check
        if (TryGetNextEvent(out var view))
        {
            if (view.Type == EventType.TimerFired)
            {
                ConsumeEvent(view);
                return;
            }
            if (view.Type == EventType.TimerStarted)
            {
                ConsumeEvent(view);
            }
        }

        // New Command
        int startPos = _arena.Position;

        var writer = new Utf8JsonWriter(_bufferWriter);
        JsonSerializer.Serialize(writer, delay);
        writer.Flush();

        int endPos = _arena.Position;
        int length = endPos - startPos;

        // Enqueue
        _commands.Enqueue(new Command(EventType.TimerStarted, startPos, length));

        await Task.Yield();
        var tcs = new TaskCompletionSource<bool>();
        await tcs.Task;
    }

    public async Task<TResult> CallActivityAsync<TInput, TResult>(string name, TInput input, ActivityOptions? options = null)
    {
        EnsureValid();

        // Replay Check
        if (TryGetNextEvent(out var view))
        {
            if (view.Type == EventType.ActivityCompleted)
            {
                ConsumeEvent(view);
                // Deserialize Result
                return JsonSerializer.Deserialize<TResult>(view.Payload)!;
            }
        }

        // Schedule
        int startPos = _arena.Position;

        var writer = new Utf8JsonWriter(_bufferWriter);
        JsonSerializer.Serialize(writer, input);
        writer.Flush();

        int endPos = _arena.Position;
        int length = endPos - startPos;

        _commands.Enqueue(new Command(EventType.ActivityScheduled, startPos, length));

        // Suspend
        var tcs = new TaskCompletionSource<TResult>();
        return await tcs.Task;
    }

    public Task CallActivityAsync<TInput>(string name, TInput input, ActivityOptions? options = null)
    {
        return CallActivityAsync<TInput, bool>(name, input, options);
    }

    private bool TryGetNextEvent(out EventView view)
    {
        var slice = _history.Span.Slice(_historyOffset);
        if (slice.Length < 13)
        {
            view = default;
            return false;
        }
        view = new EventView(slice);
        return true;
    }

    private void ConsumeEvent(EventView view)
    {
        _historyOffset += view.TotalSize;
    }
}
