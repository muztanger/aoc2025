using System.ComponentModel;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace Advent_of_Code_2025;

[TestClass]
public class Day07
{
    class Tachyon
    {
        public Pos<int> Pos {  get; set; }
    
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
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

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
        var box = new Box<int>(input.First().Length, input.Count());
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
                        if (!next.Contains(split.t1)) next.Add(split.t1);
                        if (!next.Contains(split.t2)) next.Add(split.t2);
                        
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
    
    private static string Part2(IEnumerable<string> input)
    {
        var result = 0L;
        var box = new Box<int>(input.First().Length - 1, input.Count() - 1);
        var manifold = input.ToList();

        var stack = new Stack<(Tachyon tachyon, List<Tachyon> path, long count)>();
        stack.Push((new Tachyon() { Pos = new(manifold.First().IndexOf('S'), 0) }, [], 0));

        var mem = new Dictionary<string, long>();
        while (stack.Count > 0)
        {
            var (tachyon, path, count) = stack.Pop();
            var key = String.Concat(path);
            if (mem.TryGetValue(key, out var cached))
            {
                result += cached;
                continue;
            }
            else
            {
                mem[key] = count;
            }

            var below = tachyon.Pos + Pos<int>.South;
            if (box.Contains(below))
            {
                var newPath = new List<Tachyon>(path)
                {
                    tachyon
                };
                if (manifold[below.y][below.x] == '^')
                {
                    var split = tachyon.Split();
                    stack.Push((split.t2, newPath, count + 1));
                    stack.Push((split.t1, newPath, count + 1));
                }
                else
                {
                    stack.Push((new Tachyon() { Pos = below }, newPath, count));
                }
            }
            else
            {
               result += count; // Correct?
            }
        }

        return result.ToString();
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
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day07_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day07), "2025"));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day07_Part2_Example01()
    {
        var result = Part2(Common.GetLines(example));
        Assert.AreEqual("", result);
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
