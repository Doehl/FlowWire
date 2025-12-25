using System;

namespace FlowWire.Engine.Runtime;

/// <summary>
/// Adapts the struct-based DeterministicRandom to the System.Random class API.
/// This allows us to use the zero-alloc RNG with APIs that expect System.Random,
/// including the IWorkflowContext interface.
/// </summary>
public sealed class DeterministicRandomAdapter(ulong seed) : Random
{
    private DeterministicRandom _rng = new(seed);

    public void Reset(ulong seed)
    {
        _rng.Reset(seed);
    }

    public override int Next()
    {
        return _rng.Next();
    }

    public override int Next(int maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxValue);

        return _rng.Next(maxValue);
    }

    public override int Next(int minValue, int maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue);

        long range = (long)maxValue - minValue;
        return (int)(minValue + (uint)_rng.Next((int)range));
    }

    public override double NextDouble()
    {
        return (double)(_rng.Next()) / int.MaxValue;
    }

    public override void NextBytes(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        _rng.NextBytes(buffer);
    }
    
    public override void NextBytes(Span<byte> buffer)
    {
        _rng.NextBytes(buffer);
    }
}
