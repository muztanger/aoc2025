using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Advent_of_Code_2025;

[TestClass]
public class Day06
{
    private static Dictionary<string, Func<long, long, long>> operations = new ()
    {
        { "+", (a, b) => a + b },
        { "-", (a, b) => a - b },
        { "*", (a, b) => a * b },
        { "/", (a, b) => a / b },
    };

    private static string Part1(IEnumerable<string> input)
    {
        var result = new StringBuilder();
        string[]? operationsLine = null;
        var numbersLines = new List<List<long>>();
        foreach (var line in input)
        {
            var split = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (long.TryParse(split[0], out _))
            {
                numbersLines.Add(split.Select(s => long.Parse(s)).ToList());
            }
            else if (line.ContainsAny(['+', '-', '*', '/']))
            {
                operationsLine = split;
            }
        }
        Assert.IsNotNull(operationsLine);
        var results = new List<long>();
        for (var x = 0; x < numbersLines[0].Count; x++)
        {
            var aggregate = 0L;
            for (var y = 0; y < numbersLines.Count; y++)
            {
                var number = numbersLines[y][x];
                var op = operationsLine[x];
                if (y == 0)
                {
                    aggregate = number;
                }
                else
                {
                    aggregate = operations[op](aggregate, number);
                }
            }
            results.Add(aggregate);
        }

        return results.Sum().ToString();
    }
    
    private static string Part2(IEnumerable<string> input)
    {
        var lines = input.ToList();
        var columns = new List<string>();
        int nx = lines[0].Count();
        var ny = lines.Count();
        for (var x = 0; x < nx;  x++)
        {
            var colStr = new StringBuilder();
            for (var y = 0; y < ny; y++)
            {
                Assert.AreEqual(nx, lines[y].Count());
                colStr.Append(lines[y][x]);
            }
            columns.Add(colStr.ToString());
        }

        var result = 0L;
        string? op = null;
        var numbers = new List<long>();

        foreach (var colStr in columns)
        {
            var column = colStr.Trim();
            if (colStr.ContainsAny(['+', '-', '*', '/']))
            {
                op = Regex.Match(colStr, @"[+*/-]").Value;
                column = Regex.Replace(colStr, @"[+*/ -]", "").Trim();
            }

            if (long.TryParse(column, out var number))
            {
                numbers.Add(number);
            }
            else
            {
                Assert.IsNotNull(op);

                var aggregate = numbers[0];
                foreach (var x in numbers.Skip(1))
                {
                    aggregate = operations[op](aggregate, x);
                }
                result += aggregate;

                numbers.Clear();
                op = null;
            }
        }

        {
            var aggregate = numbers[0];
            foreach (var x in numbers.Skip(1))
            {
                aggregate = operations[op](aggregate, x);
            }
            result += aggregate;
        }

        return result.ToString();
    }

    private string example = """
            123 328  51 64 
             45 64  387 23 
              6 98  215 314
            *   +   *   +  
            """;

    [TestMethod]
    public void Day06_Part1_Example01()
    {
        var result = Part1(Common.GetLines(example));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day06_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day06), "2025"));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day06_Part2_Example01()
    {
        var result = Part2(Common.GetLines(example));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day06_Part2_Example02()
    {
        var input = """
            <TODO>
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day06_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day06), "2025"));
        Assert.AreEqual("", result);
    }
    
}
