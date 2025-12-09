namespace Advent_of_Code_2025.Commons;

public class Pos3<T> : IEquatable<Pos3<T>> where T : INumber<T>
{
    public T x;
    public T y;
    public T z;

    public Pos<T> XY => new(x, y);
    public Pos<T> YZ => new(y, z);

    public Pos3(T x, T y, T z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Pos3(Pos3<T> other)
    {
        this.x = other.x;
        this.y = other.y;
        this.z = other.z;
    }

    public static Pos3<T> operator *(Pos3<T> p1, T n)
    {
        return new Pos3<T>(p1.x * n, p1.y * n, p1.z * n);
    }

    public static Pos3<T> operator +(Pos3<T> p1, Pos3<T> p2)
    {
        return new Pos3<T>(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);
    }
    public static Pos3<T> operator -(Pos3<T> p) => new Pos3<T>(-p.x, -p.y, -p.z);

    public static Pos3<T> operator -(Pos3<T> p1, Pos3<T> p2) => p1 + (-p2);

    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }

    public T Manhattan(Pos3<T> inter)
    {
        return T.Abs(x - inter.x) + T.Abs(y - inter.y) + T.Abs(z - inter.z);
    }

    public TResult Dist<TResult>(Pos3<T> p1)
        where TResult : IFloatingPoint<TResult>, IRootFunctions<TResult>
    {
        Pos3<T> delta = p1 - this;
        var dx = TResult.CreateChecked(delta.x);
        var dy = TResult.CreateChecked(delta.y);
        var dz = TResult.CreateChecked(delta.z);
        return TResult.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        Pos3<T>? posObj = obj as Pos3<T>;
        if (posObj == null)
            return false;
        else
            return Equals(posObj);
    }

    public bool Equals(Pos3<T>? other)
    {
        return other != null &&
               x == other.x &&
               y == other.y &&
               z == other.z;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = (int)199933;
            hash = (hash * 993319) ^ x.GetHashCode();
            hash = (hash * 993319) ^ y.GetHashCode();
            hash = (hash * 993319) ^ z.GetHashCode();
            return hash;
        }
    }
}
