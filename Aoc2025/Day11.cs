namespace Advent_of_Code_2025;

[TestClass]
public class Day11
{
    class Node
    {
        public string Name { get; }
        public List<Node> Connections { get; set; } = [];
        public Node(string name)
        {
            Name = name;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (Node)obj;

            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        override public string ToString()
        {
            return $"{Name}({string.Join(", ", Connections)})";
        }

        public int PathsToOut()
        {
            if (Name == "out")
            {
                return 1;
            }
            return Connections.Sum(c => c.PathsToOut());
        }

        public long PathsToOutViaDacAndFft(bool hasDac = false, bool hasFft = false, Dictionary<(string, bool, bool), long> memo = null)
        {
            memo ??= new Dictionary<(string, bool, bool), long>();
            
            var key = (Name, hasDac, hasFft);
            if (memo.ContainsKey(key))
            {
                return memo[key];
            }

            if (Name == "out")
            {
                return (hasDac && hasFft) ? 1 : 0;
            }

            bool newHasDac = hasDac || Name == "dac";
            bool newHasFft = hasFft || Name == "fft";

            long result = Connections.Sum(c => c.PathsToOutViaDacAndFft(newHasDac, newHasFft, memo));
            memo[key] = result;
            return result;
        }
    }
    private static string Part1(IEnumerable<string> input)
    {
        var nodes = new HashSet<Node>();
        foreach (var line in input)
        {
            var split = line.Replace(":", "").Split(' ');
            foreach (var name in split)
            {
                nodes.Add(new Node(name));
            }
        }
        foreach (var line in input)
        {
            var name = line.Split(':')[0];
            var node = nodes.First(n => n.Name == name);
            var connections = line.Split(':')[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(c => nodes.First(n => n.Name == c.Trim())).ToList();
            node.Connections.AddRange(connections);
        }
        //Console.WriteLine(nodes.First(n => n.Name == "you"));
        return nodes.First(n => n.Name == "you").PathsToOut().ToString();
    }
    
    private static string Part2(IEnumerable<string> input)
    {
        var nodes = new HashSet<Node>();
        foreach (var line in input)
        {
            var split = line.Replace(":", "").Split(' ');
            foreach (var name in split)
            {
                nodes.Add(new Node(name));
            }
        }
        foreach (var line in input)
        {
            var name = line.Split(':')[0];
            var node = nodes.First(n => n.Name == name);
            var connections = line.Split(':')[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(c => nodes.First(n => n.Name == c.Trim())).ToList();
            node.Connections.AddRange(connections);
        }
        return nodes.First(n => n.Name == "svr").PathsToOutViaDacAndFft().ToString();
    }

    [TestMethod]
    public void Day11_Part1_Example01()
    {
        var input = """
            aaa: you hhh
            you: bbb ccc
            bbb: ddd eee
            ccc: ddd eee fff
            ddd: ggg
            eee: out
            fff: out
            ggg: out
            hhh: ccc fff iii
            iii: out
            """;
        var result = Part1(Common.GetLines(input));
        Assert.AreEqual("5", result);
    }
    
  
    
    [TestMethod]
    public void Day11_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day11), "2025"));
        Assert.AreEqual("494", result);
    }
    
    [TestMethod]
    public void Day11_Part2_Example01()
    {
        var input = """
            svr: aaa bbb
            aaa: fft
            fft: ccc
            bbb: tty
            tty: ccc
            ccc: ddd eee
            ddd: hub
            hub: fff
            eee: dac
            dac: fff
            fff: ggg hhh
            ggg: out
            hhh: out
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("2", result);
    }
    
    [TestMethod]
    public void Day11_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day11), "2025"));
        Assert.AreEqual("", result);
    }
    
}
