namespace Advent_of_Code_2025.Commons;

[TestClass]
public class TestCommon
{
    [TestMethod]
    public void TestPos()
    {
        Assert.AreEqual(new Pos<int>(1, 1), new Pos<int>(1, 1));
    }

    [TestMethod]
    public void TestBox()
    {
        // Test Constructor
        //Assert.ThrowsException<ArgumentException>(() => new Box<int>(new Pos<int>(3, 3), new Pos<int>(0, 0)));
        //Assert.ThrowsException<ArgumentException>(() => new Box<int>(new Pos<int>(3, 0), new Pos<int>(2, 3)));
        //Assert.ThrowsException<ArgumentException>(() => new Box<int>(new Pos<int>(0, 3), new Pos<int>(3, 2)));

        var window = new Box<int>(new Pos<int>(0, 0), new Pos<int>(3, 3));

        // Test IsInside
        Assert.IsTrue(window.Contains(new Pos<int>(1, 1)));

        for (int x = window.Min.x; x <= window.Max.x; x++)
        {
            for (int y = window.Min.y; y <= window.Max.y; y++)
            {
                Assert.IsTrue(window.Contains(new Pos<int>(x, y)));
            }
        }

        Assert.IsFalse(window.Contains(new Pos<int>(-1, 1)));
        Assert.IsFalse(window.Contains(new Pos<int>(4, 1)));
        Assert.IsFalse(window.Contains(new Pos<int>(1, -1)));
        Assert.IsFalse(window.Contains(new Pos<int>(1, 4)));

        // Test Width
        Assert.AreEqual(4, new Box<int>(new Pos<int>(-1, 0), new Pos<int>(2, 0)).Width);
        Assert.AreEqual(3, new Box<int>(new Pos<int>(1, 0), new Pos<int>(3, 0)).Width);
        Assert.AreEqual(7, new Box<int>(7, 1).Width);
        Assert.AreEqual(8, new Box<int>(8, 7).Width);

        // Test Height
        Assert.AreEqual(4, new Box<int>(new Pos<int>(0, -1), new Pos<int>(0, 2)).Height);
        Assert.AreEqual(3, new Box<int>(new Pos<int>(0, 1), new Pos<int>(0, 3)).Height);

        Box<int> B() => new Box<int>(5, 5);
        Assert.IsTrue(B().Contains(new Box<int>(5, 5)));
        Assert.IsFalse(B().Contains(new Box<int>(6, 5)));
        Assert.IsFalse(B().Contains(new Box<int>(5, 6)));
        {
            var c = B();
            c.IncreaseToPoint(new Pos<int>(0, -1));
            Assert.IsFalse(B().Contains(c));
        }
        {
            var c = B();
            c.IncreaseToPoint(new Pos<int>(-1, 0));
            Assert.IsFalse(B().Contains(c));
        }

        {
            var a = B().Translate(new Pos<int>(-1, -1));
            var b = B().Translate(new Pos<int>(2, 2));
            Assert.AreEqual(new Box<int>(new Pos<int>(2, 2), new Pos<int>(3, 3)), a.Intersection(b));

            Assert.AreEqual(new Box<int>(5, 2), B().Intersection(B().Translate(new Pos<int>(0, -3))));
            Assert.AreEqual(new Box<int>(2, 5), B().Intersection(B().Translate(new Pos<int>(-3, 0))));
        }
    }

    [TestMethod]
    public void TestPos3()
    {
        var p = new Pos3<int>(0, 0, 0);
        var actual = p.Dist<double>(new Pos3<int>(2, 3, 6));
        Assert.AreEqual(7.0, actual, double.Epsilon);
    }

    [TestMethod]
    public void TestPosN()
    {
        var p1 = new PosN<long>(1, 2, 3, 4);
        var p2 = new PosN<long>(4, 5, 6, 7);
        var actual = p1.Dist<double>(p2);
        Assert.AreEqual(6.0, actual, double.Epsilon);

        Assert.AreEqual(new PosN<long>(5, 7, 9, 11), p1 + p2);
        Assert.AreEqual(new PosN<long>(-3, -3, -3, -3), p1 - p2);
    }

    [TestMethod]
    public void TestBenchmarkPosN()
    {
        var timer = new System.Diagnostics.Stopwatch();

        var random = new Random();

        // Trigger garbage collection and wait for pending finalizers
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Capture initial garbage collection counts
        int initialGen0Collections = GC.CollectionCount(0);
        int initialGen1Collections = GC.CollectionCount(1);
        int initialGen2Collections = GC.CollectionCount(2);


        timer.Start();
        for (int i = 0; i < 1_000_000; i++)
        {
            var p1 = new PosN<long>(random.Next(), random.Next(), random.Next(), random.Next());
            var p2 = new PosN<long>(random.Next(), random.Next(), random.Next(), random.Next());
            var p3 = p1 + p2;
            Assert.AreEqual(new PosN<long>(
                p1[0] + p2[0],
                p1[1] + p2[1],
                p1[2] + p2[2],
                p1[3] + p2[3]), p3);
        }
        timer.Stop();

        // Capture final garbage collection counts
        int finalGen0Collections = GC.CollectionCount(0);
        int finalGen1Collections = GC.CollectionCount(1);
        int finalGen2Collections = GC.CollectionCount(2);

        // Calculate the number of garbage collections that occurred
        int gen0Collections = finalGen0Collections - initialGen0Collections;
        int gen1Collections = finalGen1Collections - initialGen1Collections;
        int gen2Collections = finalGen2Collections - initialGen2Collections;

        Console.WriteLine($"Elapsed time: {timer.ElapsedMilliseconds} ms");
        Console.WriteLine($"Gen 0 collections: {gen0Collections}");
        Console.WriteLine($"Gen 1 collections: {gen1Collections}");
        Console.WriteLine($"Gen 2 collections: {gen2Collections}");

        // 2024-11-30: 1_000_000 iterations with 4 elements in PosN<long> and List<long> values
        //     Elapsed time: 610 ms
        //     Gen 0 collections: 126
        //     Gen 1 collections: 1
        //     Gen 2 collections: 0

        // 2024-11-30: 1_000_000 iterations with 4 elements in PosN<long> and ReadOnlyMemory<long> values
        //     Elapsed time: 367 ms
        //     Gen 0 collections: 42
        //     Gen 1 collections: 1
        //     Gen 2 collections: 0
    }

    [TestMethod]
    public void TestAllCombos()
    {
        var expected = new List<List<int>>
        {
            ([1]),
            ([2]),
            ([3]),
            ([1, 2]),
            ([1, 3]),
            ([2, 3]),
            ([1, 2, 3])
        };

        var permutationCount = 0;
        foreach (var list in Common.GetAllCombos([1, 2, 3]))
        {
            Assert.IsTrue(Exist(list), string.Join(", ", list));
            permutationCount++;
        }

        bool Exist(List<int> x)
        {
            foreach (var list in expected)
            {
                var success = true;
                if (x.Count != list.Count) continue;
                for (int i = 0; i < x.Count; i++)
                {
                    if (x[i] != list[i])
                    {
                        success = false;
                        break;
                    }
                }
                if (success) return true;
            }
            return false;
        }

        Assert.AreEqual(expected.Count, permutationCount);
    }


    [TestMethod]
    [DataRow("", " ", new string[] { })]
    [DataRow("a", " ", new string[] { "a" })]
    [DataRow(" ", " ", new string[] { })]
    [DataRow("a", ",", new string[] { "a" })]
    [DataRow(" , , , ", ",", new string[] { })]
    [DataRow("a b c", " ", new string[] { "a", "b", "c" })]
    [DataRow("  a   b c             ", " ", new string[] { "a", "b", "c" })]
    [DataRow("a, b, c", ",", new string[] { "a", "b", "c" })]
    [DataRow("    a,   b , c    ,         ", ",", new string[] { "a", "b", "c" })]
    [DataRow("a,,b,,c", ",", new string[] { "a", "b", "c" })]
    [DataRow(",,a,,b,,c,,", ",", new string[] { "a", "b", "c" })]
    public void TestSplitTrimWithString(string input, string separator, string[] expected)
    {
        var actual = input.SplitTrim(separator);
        Assert.HasCount(expected.Length, actual);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i]);
        }
    }

    [TestMethod]
    [DataRow("", ' ', new string[] { })]
    [DataRow("a", ' ', new string[] { "a" })]
    [DataRow(" ", ' ', new string[] { })]
    [DataRow("a", ',', new string[] { "a" })]
    [DataRow(" , , , ", ',', new string[] { })]
    [DataRow("a b c", ' ', new string[] { "a", "b", "c" })]
    [DataRow("  a   b c             ", ' ', new string[] { "a", "b", "c" })]
    [DataRow("a, b, c", ',', new string[] { "a", "b", "c" })]
    [DataRow("    a,   b , c    ,         ", ',', new string[] { "a", "b", "c" })]
    [DataRow("a,,b,,c", ',', new string[] { "a", "b", "c" })]
    [DataRow(",,a,,b,,c,,", ',', new string[] { "a", "b", "c" })]
    public void TestSplitTrimWithChar(string input, char separator, string[] expected)
    {
        var actual = input.SplitTrim(separator);
        Assert.HasCount(expected.Length, actual);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i]);
        }
    }
}
