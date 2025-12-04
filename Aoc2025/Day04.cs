namespace Advent_of_Code_2025;

[TestClass]
public class Day04
{
    private static string Part1(IEnumerable<string> input)
    {
        var result = 0;
        var width = input.First().Length;
        var height = input.Count();
        var box = new Box<int>(width, height);

        var grid = new List<List<char>>();
        foreach (var line in input)
        {
            grid.Add(line.ToList());
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[y][x] != '@')
                {
                    continue;
                }

                var count = 0;
                var pos = new Pos<int>(x, y);
                foreach (var dir in Pos<int>.CompassDirections)
                {
                    var other = pos + dir;
                    if (box.Contains(other) && grid[other.y][other.x] == '@')
                    {
                        count++;
                    }
                }
                if (count < 4)
                {
                    result++;
                }
            }
        }

        return result.ToString();
    }
    
    private static string Part2(IEnumerable<string> input)
    {
        var width = input.First().Length;
        var height = input.Count();
        var box = new Box<int>(width, height);


        var grid = new List<List<char>>();
        foreach (var line in input)
        {
            grid.Add(line.ToList());
        }

        var removed = new HashSet<Pos<int>>();
        var lastRemoveCount = -1;
        while (lastRemoveCount != removed.Count)
        {
            lastRemoveCount = removed.Count;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pos = new Pos<int>(x, y);
                    if (removed.Contains(pos) || grid[y][x] != '@')
                    {
                        continue;
                    }
                    var count = 0;
                    foreach (var dir in Pos<int>.CompassDirections)
                    {
                        var other = pos + dir;
                        if (box.Contains(other) && grid[other.y][other.x] == '@' && !removed.Contains(other))
                        {
                            count++;
                        }
                    }
                    if (count < 4)
                    {
                        removed.Add(pos);
                    }
                }
            }
        }

        return removed.Count.ToString();
    }

    private string example = """
            ..@@.@@@@.
            @@@.@.@.@@
            @@@@@.@.@@
            @.@@@@..@.
            @@.@@@@.@@
            .@@@@@@@.@
            .@.@.@.@@@
            @.@@@.@@@@
            .@@@@@@@@.
            @.@.@@@.@.
            """;
    
    [TestMethod]
    public void Day04_Part1_Example01()
    {
        var result = Part1(Common.GetLines(example));
        Assert.AreEqual("13", result);
    }
    
    [TestMethod]
    public void Day04_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day04), "2025"));
        Assert.AreEqual("1505", result);
    }
    
    [TestMethod]
    public void Day04_Part2_Example01()
    {
        var result = Part2(Common.GetLines(example));
        Assert.AreEqual("43", result);
    }
    
    [TestMethod]
    public void Day04_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day04), "2025"));
        Assert.AreEqual("9182", result);
    }
    
}
