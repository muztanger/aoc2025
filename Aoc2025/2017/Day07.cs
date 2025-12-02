namespace Advent_of_Code_2017;

[TestClass]
public class Day07
{
    public class Program(string name, int weight)
    {
        public string Name => name;
        public int Weight => weight;
        public List<Program> Disc { get; set; } = [];
        public override bool Equals(object? obj)
        {
            return Name.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return $"Tower({name}, {weight}, [{string.Join(",", Disc)}])";
        }

        public int Count()
        {
            return 1 + Disc.Sum(x => x.Count());
        }

        public int CheckDisc()
        {
            if (Disc.Count == 0) return -1;
            foreach (var d in Disc)
            {
                if (d.CheckDisc() > 0)
                {
                    return d.CheckDisc();
                }
            }

            var check = Disc.First().WeightSum();
            var foundError = false;
            foreach (var p in Disc.Skip(1))
            {
                if (p.WeightSum() != check)
                {
                    foundError = true;
                    break;
                }
            }
            if (foundError)
            {
                var weightSums = new Dictionary<int, int>();
                var oddSum = 0;
                foreach (var p in Disc)
                {
                    Assert.IsFalse(weightSums.ContainsKey(p.Weight)); // Trying naive implementaion
                    weightSums[p.Weight] = p.WeightSum();
                    oddSum ^= p.WeightSum(); // Find odd weightsum with xor trick
                }
                var commonSum = weightSums.First(kv => kv.Value != oddSum).Value;
                var wOdd = weightSums.First(kv => kv.Value == oddSum).Key;
                return wOdd + commonSum - oddSum;
            }
            
            return -1;
        }

        public int WeightSum()
        {
            return Weight + Disc.Sum(x => x.WeightSum());
        }

        public static Program? Bottom(IEnumerable<string> input)
        {
            var result = new StringBuilder();
            var programs = new List<Program>();
            foreach (var line in input)
            {
                var split = line.Split(' ');
                if (split.Length > 0)
                {
                    var name = split[0].Trim();
                    var value = split[1].Trim().Replace("(", "").Replace(")", "");
                    var program = new Program(name, int.Parse(value));
                    programs.Add(program);
                }
            }
            foreach (var line in input)
            {
                if (line.Contains("->"))
                {
                    var name = line.Split(' ')[0].Trim();
                    var program = programs.Find(s => s.Name.Equals(name));
                    var aboves = line.Split("->")[1].Split(',').Select(s => s.Trim());
                    foreach (var aboveName in aboves)
                    {
                        var above = programs.Find(s => s.Name.Equals(aboveName));
                        if (above != null)
                        {
                            program?.Disc.Add(above);
                        }
                    }
                }
            }
            Program? bottom = null;
            foreach (var program in programs)
            {
                if (bottom is null || program.Count() > bottom.Count())
                {
                    bottom = program;
                }
            }
            return bottom;
        }
    }


    private static string Part1(IEnumerable<string> input)
    {
        return Program.Bottom(input)?.Name ?? "";
    }
    
    private static string Part2(IEnumerable<string> input)
    {
        var bottom = Program.Bottom(input);
        if (bottom is null)
        {
            throw new Exception("no bottom");
        }
        
        return bottom.CheckDisc().ToString();
    }
    
    [TestMethod]
    public void Day07_Part1_Example01()
    {
        var input = """
            pbga (66)
            xhth (57)
            ebii (61)
            havc (66)
            ktlj (57)
            fwft (72) -> ktlj, cntj, xhth
            qoyq (66)
            padx (45) -> pbga, havc, qoyq
            tknk (41) -> ugml, padx, fwft
            jptl (61)
            ugml (68) -> gyxo, ebii, jptl
            gyxo (61)
            cntj (57)
            """;
        var result = Part1(Common.GetLines(input));
        Assert.AreEqual("tknk", result);
    }
    
    [TestMethod]
    public void Day07_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day07), "2017"));
        Assert.AreEqual("mwzaxaj", result);
    }
    
    [TestMethod]
    public void Day07_Part2_Example01()
    {
        var input = """
            pbga (66)
            xhth (57)
            ebii (61)
            havc (66)
            ktlj (57)
            fwft (72) -> ktlj, cntj, xhth
            qoyq (66)
            padx (45) -> pbga, havc, qoyq
            tknk (41) -> ugml, padx, fwft
            jptl (61)
            ugml (68) -> gyxo, ebii, jptl
            gyxo (61)
            cntj (57)
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("60", result);
    }
    
    [TestMethod]
    public void Day07_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day07), "2017"));
        Assert.IsLessThan(21932, int.Parse(result));
        Assert.AreEqual("1219", result);
    }
    
}
