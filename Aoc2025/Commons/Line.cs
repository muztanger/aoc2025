namespace Advent_of_Code_2025.Commons;

public class Line<T> where T : INumber<T>
{
    private readonly Pos<T> mP1;
    private readonly Pos<T> mP2;
    private bool mIsVertical;
    private readonly T mDx;
    private readonly T mDy;
    private readonly T mSlope = T.Zero;

    public Line(Pos<T> p1, Pos<T> p2)
    {
        mP1 = p1;
        mP2 = p2;
        mIsVertical = p1.x == p2.x;
        mDx = p2.x - p1.x;
        mDy = p2.y - p1.y;
        if (!mIsVertical)
        {
            mSlope = mDy / mDx;
        }
    }

    public bool OnLine(Pos<T> pos)
    {
        return mDx * (pos.y - mP1.y) - mDy * (pos.x - mP1.x) == T.Zero;
    }
}
