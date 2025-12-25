using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace FlowWire.Engine.Runtime;

/// <summary>
/// A lightweight, stack-allocated, deterministic RNG.
/// Implements a PCG-variant or SplitMix64 for speed and statistical quality.
/// </summary>
public struct DeterministicRandom
{
    // The internal state
    private ulong _state;

    // 64-bit version of Golden Ratio
    private ulong _goldenRatio = 0x9E3779B97f4A7C15;

    // First mixing Constant of the SplitMix64 (Avalanche Step A)
    private ulong _avalancheA = 0xBF58476D1CE4E5B9;

    // Second mixing Constant of the SplitMix64 (Avalanche Step B)
    private ulong _avalancheB = 0x94D049BB133111EB;

    // Constructor seeds the state immediately
    public DeterministicRandom(ulong seed)
    {
        Reset(seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset(ulong seed)
    {
        // Mix the seed slightly to avoid correlation if seed is low
        _state = seed + _goldenRatio;
        Next();
    }

    /// <summary>
    /// Returns a non-negative random integer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Next()
    {
        return (int)(NextUInt64() >> 33);
    }

    /// <summary>
    /// Returns a random integer within [0, maxValue).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Next(int maxValue)
    {
        return (int)(NextUInt64() % (ulong)maxValue);
    }

    /// <summary>
    /// Fills the buffer with random bytes.
    /// </summary>
    public void NextBytes(Span<byte> buffer)
    {
        int i = 0;
        while (i <= buffer.Length - 8)
        {
            ulong next = NextUInt64();
            BinaryPrimitives.WriteUInt64LittleEndian(buffer[i..], next);
            i += 8;
        }

        if (i < buffer.Length)
        {
            ulong next = NextUInt64();
            while (i < buffer.Length)
            {
                buffer[i] = (byte)next;
                next >>= 8;
                i++;
            }
        }
    }

    /// <summary>
    /// Core SplitMix64 implementation: Fast, passes BigCrush, low overhead.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NextUInt64()
    {
        ulong z = (_state += _goldenRatio);
        z = (z ^ (z >> 30)) * _avalancheA;
        z = (z ^ (z >> 27)) * _avalancheB;
        return z ^ (z >> 31);
    }
}
