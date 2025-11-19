namespace Advent_of_Code_2025.Commons;

public class Pos<T> : IEquatable<Pos<T>>
    where T : INumber<T>
{
    public T x;
    public T y;
    public Pos(T x, T y)
    {
        this.x = x;
        this.y = y;
    }

    public Pos((T, T) z)
    {
        x = z.Item1;
        y = z.Item2;
    }

    public Pos(Pos<T> other)
    {
        this.x = other.x;
        this.y = other.y;
    }

    public T Dist() => T.Abs(y - x);

    public static Pos<T> operator *(Pos<T> p1, T n) => new(p1.x * n, p1.y * n);
    public static Pos<T> operator *(T n, Pos<T> p1) => p1 * n;
    public static Pos<T> operator %(Pos<T> p1, T n) => new(p1.x % n, p1.y % n);
    public static Pos<T> operator %(Pos<T> p1, Pos<T> p2) => new(p1.x % p2.x, p1.y % p2.y);
    public static Pos<T> operator +(Pos<T> p1, Pos<T> p2) => new(p1.x + p2.x, p1.y + p2.y);
    public static Pos<T> operator -(Pos<T> p) => new(-p.x, -p.y);
    public static Pos<T> operator -(Pos<T> p1, Pos<T> p2) => p1 + (-p2);

    public static readonly Pos<T> Zero = new(T.Zero, T.Zero);
    public static readonly Pos<T> One = new(T.One, T.One);

    public static readonly Pos<T> East = new(T.One, T.Zero);
    public static readonly Pos<T> South = new(T.Zero, T.One);
    public static readonly Pos<T> West = new(-T.One, T.Zero);
    public static readonly Pos<T> North = new(T.Zero, -T.One);

    public static readonly Pos<T> SouthEast = South + East;
    public static readonly Pos<T> SouthWest = South + West;
    public static readonly Pos<T> NorthWest = North + West;
    public static readonly Pos<T> NorthEast = North + East;

    public static readonly List<Pos<T>> CardinalDirections =
    [
        East,
        South,
        West,
        North
    ];

    public static readonly List<Pos<T>> CompassDirections =
    [
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest,
        North,
        NorthEast
    ];

    public static readonly List<Pos<T>> DiagonalDirections =
    [
        SouthEast,
        SouthWest,
        NorthWest,
        NorthEast
    ];

    public void Set(Pos<T> other)
    {
        this.x = other.x;
        this.y = other.y;
    }

    public override string ToString()
    {
        return $"({x}, {y})";
    }

    internal T Manhattan(Pos<T> inter)
    {
        return T.Abs(x - inter.x) + T.Abs(y - inter.y);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is not Pos<T> posObj)
            return false;
        else
            return Equals(posObj);
    }

    public bool Equals(Pos<T>? other)
    {
        return other is not null &&
               x == other.x &&
               y == other.y;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() * 7919 + y.GetHashCode();
    }

    public static bool operator ==(Pos<T> pos1, Pos<T> pos2)
    {
        if (((object)pos1) == null || ((object)pos2) == null)
            return Object.Equals(pos1, pos2);

        return pos1.Equals(pos2);
    }

    public static bool operator !=(Pos<T> pos1, Pos<T> pos2)
    {
        if (((object)pos1) == null || ((object)pos2) == null)
            return !Object.Equals(pos1, pos2);

        return !(pos1.Equals(pos2));
    }

    public bool BetweenXY(T z)
    {
        return z >= x && z <= y;
    }

    internal bool Between(Pos<T> p1, Pos<T> p2)
    {
        if (p1.x == p2.x && p2.x == this.x)
        {
            return (p1.y < y && y < p2.y) || (p2.y < y && y < p1.y);
        }
        if (!(new Line<T>(this, p1).OnLine(p2))) return false;

        if (p1.x < this.x) return p2.x > this.x;
        if (p1.x > this.x) return p2.x < this.x;

        return false;
    }

    internal T Dist2(Pos<T> p1)
    {
        var delta = p1 - this;
        return delta.x * delta.x + delta.y * delta.y;
    }

    internal bool Adjacent(Pos<T> other)
    {
        var dp = this - other;
        var x = T.Abs(dp.x);
        var y = T.Abs(dp.y);
        return y <= T.One && x <= T.One;
    }

    internal Pos<T> Sign(Pos<T> other)
    {
        var dp = other - this;
        dp.x = T.Sign(dp.x) switch { -1 => -T.One, 1 => T.One, _ => T.Zero };
        dp.y = T.Sign(dp.y) switch { -1 => -T.One, 1 => T.One, _ => T.Zero };
        return new Pos<T>(dp);
    }

    internal Pos<T> Abs()
    {
        return new Pos<T>(T.Abs(x), T.Abs(y));
    }




}
