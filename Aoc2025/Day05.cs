namespace Advent_of_Code_2025;

[TestClass]
public class Day05
{
    private static string Part1(IEnumerable<string> input)
    {
        var ranges = new List<(long, long)>();
        var result = 0;
        foreach (var line in input)
        {
            if (string.IsNullOrEmpty(line)) continue;
            if (line.Contains('-'))
            {
                var split = line.Split('-');
                ranges.Add((long.Parse(split[0].Trim()), long.Parse(split[1].Trim())));
            }
            else if (long.TryParse(line.Trim(), out var x))
            {
                foreach (var range in ranges)
                {
                    if (x >= range.Item1 && x <= range.Item2)
                    {
                        result++;
                        break;
                    }
                }
            }
        }
        return result.ToString();
    }
    
    private static string Part2(IEnumerable<string> input)
    {
        var ranges = new List<(long, long)>();
        var result = 0L;
        foreach (var line in input)
        {
            if (string.IsNullOrEmpty(line)) continue;
            if (line.Contains('-'))
            {
                var split = line.Split('-');
                ranges.Add((long.Parse(split[0].Trim()), long.Parse(split[1].Trim())));
            }
        }
        var lastCount = -1;
        while (lastCount != ranges.Count)
        {
            lastCount = ranges.Count;
            var newRanges = new List<(long, long)>();
            for (int i = 0; i < ranges.Count - 1; i++)
            {
                var r1 = ranges[i];
                for (int j = i + 1; j < ranges.Count; j++)
                {
                    var r2 = ranges[j];
                    // check if they overlap
                    if ((r1.Item1 >= r2.Item1 && r1.Item1 <= r2.Item2)
                        || (r1.Item2 >= r2.Item1 && r1.Item2 <= r2.Item2)
                        || (r2.Item1 >= r1.Item1 && r2.Item2 <= r1.Item2)
                        || (r2.Item2 >= r1.Item1 && r2.Item2 <= r1.Item2))
                    {
                        var min = Math.Min(r1.Item1, r2.Item1);
                        var max = Math.Max(r1.Item2, r2.Item2);
                        newRanges.Add((min, max));
                        ranges.Remove(r1);
                        ranges.Remove(r2);
                        break;
                    }
                }
            }
            ranges = ranges.Concat(newRanges).ToList();
        }
        foreach (var range in ranges)
        {
            result += range.Item2 - range.Item1 + 1;
            Console.WriteLine($"{range}: {result}");
        }
        return result.ToString();
    }
    
    [TestMethod]
    public void Day05_Part1_Example01()
    {
        var input = """
                3-5
                10-14
                16-20
                12-18

                1
                5
                8
                11
                17
                32
                """;
        var result = Part1(Common.GetLines(input));
        Assert.AreEqual("3", result);
    }
    
    [TestMethod]
    public void Day05_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day05), "2025"));
        Assert.AreEqual("770", result);
    }
    
    [TestMethod]
    public void Day05_Part2_Example01()
    {
        var input = """
            3-5
            10-14
            16-20
            12-18
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("14", result);
    }
    
    [TestMethod]
    public void Day05_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day05), "2025"));
        Assert.AreNotEqual("360642626971493", result);
        Assert.AreEqual("357674099117260", result);
    }
    
}
