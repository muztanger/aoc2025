namespace Advent_of_Code_2025;

[TestClass]
public class Day11
{
    class Nod
    {
        public string Name { get; }
        public List<Nod> Connections { get; set; } = [];
        public Nod(string name)
        {
            Name = name;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (Nod)obj;

            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        override public string ToString()
        {
            return $"{Name}({string.Join(", ", Connections.Select(c => c.Name))})";
        }
    }
    private static string Part1(IEnumerable<string> input)
    {
        var result = new StringBuilder();
        var nodes = new HashSet<Nod>();
        foreach (var line in input)
        {
            var split = line.Replace(":", "").Split(' ');
            foreach (var name in split)
            {
                nodes.Add(new Nod(name));
            }
        }
        foreach (var line in input)
        {
            var name = line.Split(':')[0];
            var node = nodes.First(n => n.Name == name);
            var connections = line.Split(':')[1].Split(',').Select(c => nodes.First(n => n.Name == c.Trim())).ToList();
            node.Connections.AddRange(connections);
        }
        Console.WriteLine(nodes.First(n => n.Name == "you"));
        return result.ToString();
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
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day11_Part1_Example02()
    {
        var input = """
            <TODO>
            """;
        var result = Part1(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day11_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day11), "2025"));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day11_Part2_Example01()
    {
        var input = """
            <TODO>
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day11_Part2_Example02()
    {
        var input = """
            <TODO>
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day11_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day11), "2025"));
        Assert.AreEqual("", result);
    }
    
}
