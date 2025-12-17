using Advent_of_Code_2025.Commons;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace Advent_of_Code_2025;

[TestClass]
public class Day07
{
    class Tachyon
    {
        public Pos<int> Pos {  get; init; } = new(0,0);

        public (Tachyon t1,Tachyon t2) Split()
        {
            return (new Tachyon { Pos = new(Pos.x - 1, Pos.y + 1) },
                new Tachyon { Pos = new(Pos.x + 1, Pos.y + 1) });
        }

        public override string ToString()
        {
            return Pos.ToString();
        }

        // override object.Equals
        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (Tachyon)obj;

            return Pos.Equals(other.Pos);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Pos.GetHashCode();
        }
    }

    private static string Part1(IEnumerable<string> input)
    {
        var result = 0;
        var box = new Box<int>(input.First().Length - 1, input.Count() - 1);
        var manifold = input.ToList();
        var start = new Tachyon() { Pos = new (manifold.First().IndexOf('S'), 0) };
        var tachyons = new HashSet<Tachyon>() { start };
        
        for (int y = 0; y < box.Height; y++)
        {
            var next = new HashSet<Tachyon>();
            foreach (Tachyon tachyon in tachyons)
            {
                var below = tachyon.Pos + Pos<int>.South;
                if (box.Contains(below))
                {
                    if (manifold[below.y][below.x] == '^')
                    {
                        var split = tachyon.Split();
                        next.Add(split.t1);
                        next.Add(split.t2);
                        
                        result++;
                    }
                    else
                    {
                        next.Add(new Tachyon() { Pos = below });
                    }
                }
            }
            tachyons = next;
        }
        return result.ToString();
    }

    class Node
    {
        public Pos<int> Pos { get; init;}
        public Node? SouthWest { get; set; } = null;
        public Node? SouthEast { get; set; } = null;
        public Node? South { get; set; } = null;

        public Node(Pos<int> pos)
        {
            Pos = pos;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (Node)obj;

            return Pos.Equals(other.Pos);
        }

        public override int GetHashCode()
        {
            return Pos.GetHashCode();
        }

        public long Count()
        {
            if (South is not null)
            {
                return South.Count();
            }
            else if (SouthWest is not null || SouthEast is not null)
            {
                return (SouthWest?.Count() ?? 0) + (SouthEast?.Count() ?? 0);
            }
            return 1;
        }
    }

    private static string Part2(IEnumerable<string> input)
    {
        var box = new Box<int>(input.First().Length - 1, input.Count() - 1);
        var manifold = input.ToList();

        var start = new Node(new(manifold.First().IndexOf('S'), 0));

        var stack = new Stack<Node>();
        stack.Push(start);
        var nodeSet = new HashSet<Node>();
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            var below = node.Pos + Pos<int>.South;
            if (box.Contains(below))
            {
                if (manifold[below.y][below.x] == '^')
                {
                    var se = new Node(below + Pos<int>.East);
                    if (nodeSet.TryGetValue(se, out var southEast))
                    {
                        node.SouthEast = southEast;
                    }
                    else
                    {
                        node.SouthEast = se;
                        nodeSet.Add(se);
                        stack.Push(se);
                    }

                    var sw = new Node(below + Pos<int>.West);
                    if (nodeSet.TryGetValue(sw, out var southWest))
                    {
                        node.SouthWest = southWest;
                    }
                    else
                    {
                        node.SouthWest = sw;
                        nodeSet.Add(sw);
                        stack.Push(sw);
                    }
                }
                else
                {
                    var s = new Node(below);
                    if (nodeSet.TryGetValue(s, out var south))
                    {
                        node.South = south;
                    }
                    else
                    {
                        node.South = s;
                        nodeSet.Add(s);
                        stack.Push(s);
                    }
                }
            }
        }

        return start.Count().ToString();
    }
    
    private string example = """
        .......S.......
        ...............
        .......^.......
        ...............
        ......^.^......
        ...............
        .....^.^.^.....
        ...............
        ....^.^...^....
        ...............
        ...^.^...^.^...
        ...............
        ..^...^.....^..
        ...............
        .^.^.^.^.^...^.
        ...............
            
        """;

    [TestMethod]
    public void Day07_Part1_Example01()
    {
        var result = Part1(Common.GetLines(example));
        Assert.AreEqual("21", result);
    }
    
    [TestMethod]
    public void Day07_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day07), "2025"));
        Assert.AreEqual("1585", result);
    }
    
    [TestMethod]
    public void Day07_Part2_Example01()
    {
        var result = Part2(Common.GetLines(example));
        Assert.AreEqual("40", result);
    }
    
    [TestMethod]
    public void Day07_Part2_Example02()
    {
        var input = """
            <TODO>
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day07_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day07), "2025"));
        Assert.AreEqual("", result);
    }
    
}
