using System.Collections;

namespace FlowWire.SourceGenerators;

/// <summary>
/// A wrapper for arrays that implements value equality.
/// </summary>
internal sealed class EquatableArray<T>(T[] array) : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    private readonly T[] _array = array;

    public bool Equals(EquatableArray<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (_array.Length != other._array.Length) return false;

        for (int i = 0; i < _array.Length; i++)
        {
            if (!_array[i].Equals(other._array[i])) return false;
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        int hash = 17;
        foreach (var item in _array)
        {
            hash = hash * 31 + item.GetHashCode();
        }
        return hash;
    }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _array.GetEnumerator();

    public static implicit operator EquatableArray<T>(T[] array) => new(array);
}
