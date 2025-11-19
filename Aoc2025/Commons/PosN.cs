namespace Advent_of_Code_2025.Commons;

public class PosN<T>
    where T : INumber<T>, IEquatable<T>
{
    private readonly ReadOnlyMemory<T> values;
    public int Count => values.Length;

    public PosN(params T[] w)
    {
        values = new ReadOnlyMemory<T>(w);
    }

    public PosN(PosN<T> other)
    {
        values = new ReadOnlyMemory<T>(other.values.ToArray());
    }

    public PosN(IEnumerable<T> v)
    {
        values = new ReadOnlyMemory<T>(v.ToArray());
    }

    public T this[int i]
    {
        get => values.Span[i];
    }

    public static PosN<T> operator *(PosN<T> p1, T n)
    {
        var result = new T[p1.Count];
        var span = p1.values.Span;
        for (int i = 0; i < span.Length; i++)
        {
            result[i] = n * span[i];
        }
        return new PosN<T>(result);
    }

    public static PosN<T> operator +(PosN<T> p1, PosN<T> p2)
    {
        var result = new T[p1.Count];
        var span1 = p1.values.Span;
        var span2 = p2.values.Span;
        for (int i = 0; i < span1.Length; i++)
        {
            result[i] = span1[i] + span2[i];
        }
        return new PosN<T>(result);
    }

    public static PosN<T> operator -(PosN<T> p)
    {
        var result = new T[p.Count];
        var span = p.values.Span;
        for (int i = 0; i < span.Length; i++)
        {
            result[i] = -span[i];
        }
        return new PosN<T>(result);
    }

    public static PosN<T> operator -(PosN<T> p1, PosN<T> p2) => p1 + (-p2);

    public override string ToString()
    {
        return $"({string.Join(",", values.Span.ToArray())})";
    }

    internal T Manhattan(PosN<T> inter)
    {
        T sum = T.Zero;
        var span1 = values.Span;
        var span2 = inter.values.Span;
        for (int i = 0; i < span1.Length; i++)
        {
            sum += T.Abs(span1[i] - span2[i]);
        }
        return sum;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as PosN<T>);
    }

    public bool Equals([AllowNull] PosN<T> other)
    {
        if (other == null || other.Count != this.Count)
            return false;

        var span1 = this.values.Span;
        var span2 = other.values.Span;
        for (int i = 0; i < span1.Length; i++)
        {
            if (!span1[i].Equals(span2[i]))
                return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        int hash = 43;
        var span = values.Span;
        for (int i = 0; i < span.Length; i++)
        {
            hash = hash * 2621 + span[i].GetHashCode();
        }
        return hash;
    }

    internal TResult Dist<TResult>(PosN<T> p1)
        where TResult : IFloatingPoint<TResult>, IRootFunctions<TResult>
    {
        var delta = p1 - this;
        TResult squareSum = TResult.Zero;
        var span = delta.values.Span;
        for (int i = 0; i < span.Length; i++)
        {
            squareSum += TResult.CreateChecked(span[i] * span[i]);
        }
        return TResult.Sqrt(squareSum);
    }
}
