namespace Advent_of_Code_2025;

[TestClass]
public class Day09
{
    private static string Part1(IEnumerable<string> input)
    {
        var result = 0L;
        var positions = new List<Pos<long>>();
        foreach (var line in input)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var (x, y) = line.Split(',').Select(long.Parse).ToArray();
            positions.Add(new Pos<long>(x, y));
        }

        for (int i = 0; i < positions.Count -1; i++)
        {
            for (int j = i + 1; j < positions.Count; j++)
            {
                var box = new Box<long>(positions[i], positions[j]);
                result = Math.Max(result, box.Area);
            }
        }

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
    public void Day09_Part1_Example01()
    {
        var input = """
            7,1
            11,1
            11,7
            9,7
            9,5
            2,5
            2,3
            7,3
            """;
        var result = Part1(Common.GetLines(input));
        Assert.AreEqual("50", result);
    }
    
    [TestMethod]
    public void Day09_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day09), "2025"));
        Assert.AreNotEqual("2147268864", result);
        Assert.AreEqual("4738108384", result);
    }
    
    [TestMethod]
    public void Day09_Part2_Example01()
    {
        var input = """
            <TODO>
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day09_Part2_Example02()
    {
        var input = """
            <TODO>
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day09_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day09), "2025"));
        Assert.AreEqual("", result);
    }
    
}
