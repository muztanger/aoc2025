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

            long BankValue(int[] bankIndexes) => bankIndexes.Aggregate(0L, (v, i) => v * 10 + bank[i]);

            int[] first = Enumerable.Range(bank.Count - 12, 12).ToArray();
            
            var queue = new PriorityQueue<int[], long>();
            queue.Enqueue(first, -BankValue(first));

            var visited = new HashSet<string>
            {
                string.Join(",", first)
            };

            while (queue.Count > 0)
            {
                var indexes = queue.Dequeue();
                var value = BankValue(indexes);
                max = Math.Max(max, value);

                for (var i = 0; i < indexes.Length; i++)
                {
                    if (indexes[i] > 0 && !indexes.Contains(indexes[i] - 1))
                    {
                        var newIndexes = (int[])indexes.Clone();
                        newIndexes[i]--;

                        var newValue = BankValue(newIndexes);
                        var newKey = string.Join(",", newIndexes);
                        if (!visited.Contains(newKey)) // Need something like Dijkstra's algorithm to avoid revisiting
                        {
                            visited.Add(newKey);
                            queue.Enqueue(newIndexes, -newValue);
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
        Assert.AreEqual("3121910778619", result);
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
