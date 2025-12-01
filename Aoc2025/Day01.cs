namespace Advent_of_Code_2025;

[TestClass]
public class Day01
{
    private static string Part1(IEnumerable<string> input)
    {
        var dial = 50;
        Console.WriteLine(dial);
        var result = 0;
        foreach (var line in input)
        {
            if (line.StartsWith("L"))
            {
                dial -= int.Parse(line[1..]);
            }
            else if (line.StartsWith("R"))
            {
                dial += int.Parse(line[1..]);
            }
            dial = (dial + 100) % 100;
            Console.WriteLine(dial);
            if (dial == 0)
            {
                result++;
            }
        }
        return result.ToString();
    }
    
    private static string Part2(IEnumerable<string> input)
    {
        var dial = 50;
        var result = 0;
        foreach (var line in input)
        {
            var start = dial;
            var steps = 0;
            var count = 0;
            if (line.StartsWith("L"))
            {
                steps = -int.Parse(line[1..]);
            }
            else if (line.StartsWith("R"))
            {
                steps = int.Parse(line[1..]);
            }

            var step = Math.Sign(steps);
            
            while (steps != 0)
            {
                steps -= step;
                dial += step;

                dial = (dial + 100) % 100;
                if (dial == 0)
                {
                    result++;
                    count++;
                }
            }

            Console.WriteLine($"{line}: {start} -> {dial}: {count} {result}");
        }
        return result.ToString();
    }
    
    [TestMethod]
    public void Day01_Part1_Example01()
    {
        var input = """
            L68
            L30
            R48
            L5
            R60
            L55
            L1
            L99
            R14
            L82
            """;
        var result = Part1(Common.GetLines(input));
        Assert.AreEqual("3", result);
    }
    
    [TestMethod]
    public void Day01_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day01), "2025"));
        Assert.AreEqual("995", result);
    }
    
    [TestMethod]
    public void Day01_Part2_Example01()
    {
        var input = """
            L68
            L30
            R48
            L5
            R60
            L55
            L1
            L99
            R14
            L82
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("6", result);
    }
    
    [TestMethod]
    public void Day01_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day01), "2025"));
        Assert.AreEqual("5847", result);
    }
    
}
