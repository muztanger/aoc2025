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

        for (int i = 0; i < positions.Count - 1; i++)
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
        var result = 0L;
        var lines = new List<Line<long>>();
        var positions = new List<Pos<long>>();
        
        foreach (var line in input)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var parts = line.Split(',').Select(long.Parse).ToArray();
            positions.Add(new Pos<long>(parts[0], parts[1]));
        }
        
        // Create lines forming the polygon
        for (int i = 0; i < positions.Count; i++)
        {
            var nextIndex = (i + 1) % positions.Count;
            lines.Add(new Line<long>(positions[i], positions[nextIndex]));
        }

        for (int i = 0; i < positions.Count - 1; i++)
        {
            for (int j = i + 1; j < positions.Count; j++)
            {
                var box = new Box<long>(positions[i], positions[j]);
                var ixMin = box.Width > 1 ? box.Min.x + 1 : box.Min.x;
                var ixMax = box.Width > 1 ? box.Max.x - 1 : box.Max.x;
                var iyMin = box.Height > 1 ? box.Min.y + 1 : box.Min.y;
                var iyMax = box.Height > 1 ? box.Max.y - 1 : box.Max.y;
                var inside = new Box<long>(new Pos<long>(ixMin, iyMin), new Pos<long>(ixMax, iyMax));
                
                // Check that no line intersects inside
                var intersects = false;
                foreach (var line in lines)
                {
                    if (inside.Intersects(line))
                    {
                        intersects = true;
                        break;
                    }
                }
                if (intersects)
                    continue;
                result = Math.Max(result, box.Area);
            }
        }

        return result.ToString();
    }
    
    string example = """
        7,1
        11,1
        11,7
        9,7
        9,5
        2,5
        2,3
        7,3
        """;
    [TestMethod]
    public void Day09_Part1_Example01()
    {
        var result = Part1(Common.GetLines(example));
        Assert.AreEqual("50", result);
    }
    
    [TestMethod]
    public void Day09_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day09), "2025"));
        Assert.AreEqual("4738108384", result);
    }
    
    [TestMethod]
    public void Day09_Part2_Example01()
    {
        var result = Part2(Common.GetLines(example));
        Assert.AreEqual("24", result);
    }
    
    [TestMethod]
    public void Day09_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day09), "2025"));
        Assert.AreEqual("1513792010", result);
    }
    
}
