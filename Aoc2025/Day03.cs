namespace Advent_of_Code_2025;

[TestClass]
public class Day03
{
    private static string Part1(IEnumerable<string> input)
    {
        var result = 0;
        foreach (var line in input)
        {
            var bank = line.Trim().Select(c => int.Parse(c.ToString())).ToList();
            var max = int.MinValue;
            for (int i = 0; i < bank.Count - 1; i++)
            {
                var y = bank[i] * 10;
                for (int j = i + 1; j < bank.Count; j++)
                {
                    max = Math.Max(max, y + bank[j]);
                }
            }
            result += max;
        }
        return result.ToString();
    }
    
    private static string Part2(IEnumerable<string> input)
    {
        var result = 0L;
        foreach (var line in input)
        {
            var bank = line.Trim().Select(c => long.Parse(c.ToString())).ToList();
            
            var max = long.MinValue;

            int[] first = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
            
            var stack = new Stack<int[]>();
            stack.Push(first);

            var visited = new HashSet<int[]>();
            visited.Add(first);

            while (stack.Count > 0)
            {
                var indexes = stack.Pop();
                var value = indexes.Aggregate(0L, (v, i) => v * 10 + bank[i]);
                max = Math.Max(max, value);

                for (int i = indexes.Length - 1; i >= 0; i--)
                {
                    if (indexes[i] < bank.Count - 1 && !indexes.Contains(indexes[i] + 1))
                    {
                        var newIndexes = (int[])indexes.Clone();
                        newIndexes[i]++;

                        if (!visited.Contains(newIndexes) && newIndexes[i] < bank.Count)
                        {
                            var newValue = newIndexes.Aggregate(0L, (v, i) => v * 10 + bank[i]);
                            if (newValue > max)
                            {
                                visited.Add(newIndexes);
                                stack.Push(newIndexes);
                            }
                        }
                    }
                }
            }
            Console.WriteLine(max);
            result += max;
        }
        return result.ToString();
    }
    
    [TestMethod]
    public void Day03_Part1_Example01()
    {
        var input = """
            987654321111111
            811111111111119
            234234234234278
            818181911112111
            """;
        var result = Part1(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day03_Part1_Example02()
    {
        var input = """
            <TODO>
            """;
        var result = Part1(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day03_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day03), "2025"));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day03_Part2_Example01()
    {
        var input = """
            987654321111111
            811111111111119
            234234234234278
            818181911112111
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day03_Part2_Example02()
    {
        var input = """
            <TODO>
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day03_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day03), "2025"));
        Assert.AreEqual("", result);
    }
    
}
