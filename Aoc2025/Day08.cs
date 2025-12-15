using System.Linq;
using System.Runtime.InteropServices;

namespace Advent_of_Code_2025;

[TestClass]
public class Day08
{
    private static string Part1(IEnumerable<string> input, int n)
    {
        var boxes = new List<Pos3<int>>();
        foreach (var line in input)
        {
            if (string.IsNullOrEmpty(line)) continue;
            var (x, y, z) = line.Trim().Split(',').Select(s => int.Parse(s)).Take(3).ToArray();
            boxes.Add(new Pos3<int>(x, y, z));
        }
        Console.WriteLine($"Boxes: {string.Join(",", boxes)}");
        var circuits = new List<HashSet<Pos3<int>>>();
        var connected = new HashSet<(Pos3<int>, Pos3<int>)>();
        var connections = 0;
        while (true)
        {
            var minDist = double.MaxValue;
            Pos3<int>? minP1 = null;
            Pos3<int>? minP2 = null;
            var isFound = false;
            for (int i = 0; i < boxes.Count - 1; i++)
            {
                var p1 = boxes[i];
                for (int j = i + 1; j < boxes.Count; j++)
                {
                    var p2 = boxes[j];
                    if (connected.Contains((p1, p2)) || connected.Contains((p2, p1)))
                    {
                        continue;
                    }
                    isFound = true;
               
                    var dp = p1.Dist<double>(p2);
                    if (dp < minDist)
                    {
                        minDist = dp;
                        minP1 = new Pos3<int>(p1);
                        minP2 = new Pos3<int>(p2);
                    }
                }
            }
            if (!isFound) break;
            if (connections == n) break;
            connections++;

            Assert.IsNotNull(minP1);
            Assert.IsNotNull(minP2);
            connected.Add((minP1, minP2));

            {
                HashSet<Pos3<int>>? c1 = null;
                HashSet<Pos3<int>>? c2 = null;
                foreach (var circuit in circuits)
                {
                    if (circuit.Contains(minP1))
                    {
                        c1 = circuit;
                    }
                    if (circuit.Contains(minP2))
                    {
                        c2 = circuit;
                    }
                }
                if (c1 is null && c2 is null)
                {
                    circuits.Add(new HashSet<Pos3<int>>()
                    {
                        minP1,
                        minP2
                    });
                }
                else if (c1 is not null && c2 is null)
                {
                    c1.Add(minP2);
                }
                else if (c1 is null && c2 is not null)
                {
                    c2.Add(minP1);
                }
                else if (c1 is not null && c2 is not null)
                {
                    if (c1 == c2) continue;
                    circuits.Remove(c1);
                    circuits.Remove(c2);
                    circuits.Add([.. c1.Union(c2)]);
                }
            }
        }

        Console.WriteLine(string.Join("\n", circuits.Select(c => string.Join(",", c))));
        Console.WriteLine(string.Join(",", circuits.Select(x => x.Count)));
        return circuits.Select(x => x.Count).OrderDescending().Take(3).Aggregate((a, x) => (a == 0 ? 1 : a) * x).ToString();
    }
    
    private static string Part2(IEnumerable<string> input)
    {
        var result = new StringBuilder();
        foreach (var line in input)
        {
        }
        return result.ToString();
    }
    
    [TestMethod]
    public void Day08_Part1_Example01()
    {
        var input = """
            162,817,812
            57,618,57
            906,360,560
            592,479,940
            352,342,300
            466,668,158
            542,29,236
            431,825,988
            739,650,466
            52,470,668
            216,146,977
            819,987,18
            117,168,530
            805,96,715
            346,949,466
            970,615,88
            941,993,340
            862,61,35
            984,92,344
            425,690,689
            """;
        var result = Part1(Common.GetLines(input), 10);
        Assert.AreEqual("40", result);
    }
    
    [TestMethod]
    public void Day08_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day08), "2025"), 1000);
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day08_Part2_Example01()
    {
        var input = """
            <TODO>
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day08_Part2_Example02()
    {
        var input = """
            <TODO>
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day08_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day08), "2025"));
        Assert.AreEqual("", result);
    }
    
}
