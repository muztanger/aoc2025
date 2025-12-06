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

            long BankValue(List<int> bankIndexes) => bankIndexes.Aggregate(0L, (v, i) => v * 10 + bank[i]);

            List<int> first = [0];
            
            var stack = new Stack<List<int>>();
            
            {
                var maxIndexes = new List<int>();
                var maxValue = bank[0];
                for (int i = 0; i < bank.Count - 12 + 1; i++)
                {
                    if (bank[i] > maxValue)
                    {
                        maxIndexes.Clear();
                        maxIndexes.Add(i);
                        maxValue = bank[i];
                    }
                    else if (bank[i] == maxValue)
                    {
                        maxIndexes.Add(i);
                    }
                }
                foreach (var index in maxIndexes)
                {
                    stack.Push([index]);
                }
            }

            while (stack.Count > 0)
            {
                var indexes = stack.Pop();
                if (indexes.Count == 12)
                {
                    var value = BankValue(indexes);
                    max = Math.Max(max, value);
                    continue;
                }

                var last = indexes.Last();
                var firstIndex = last + 1;
                if (firstIndex < bank.Count && indexes.Count < 12)
                {
                    var maxValue = bank[firstIndex];
                    var maxIndexes = new List<int>();
                    for (int i = firstIndex; i < bank.Count - 12 + indexes.Count + 1; i++)
                    {
                        if (bank[i] > maxValue)
                        {
                            maxIndexes.Clear();
                            maxIndexes.Add(i);
                            maxValue = bank[i];
                        }
                        else if (bank[i] == maxValue)
                        {
                            maxIndexes.Add(i);
                        }
                    }
                    foreach (var index in maxIndexes)
                    {
                        var newIndexes = new List<int>(indexes) { index };
                        stack.Push(newIndexes);
                    }
                }
            }
            Console.WriteLine(max);
            result += max;
        }
        return result.ToString();
    }

    private string example = """
            987654321111111
            811111111111119
            234234234234278
            818181911112111
            """;

    [TestMethod]
    public void Day03_Part1_Example01()
    {
        var result = Part1(Common.GetLines(example));
        Assert.AreEqual("357", result);
    }
    
    [TestMethod]
    public void Day03_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day03), "2025"));
        Assert.AreEqual("17113", result);
    }
    
    [TestMethod]
    public void Day03_Part2_Example01()
    {
        var result = Part2(Common.GetLines(example));
        Assert.AreEqual("3121910778619", result);
    }
    
  
    [TestMethod]
    public void Day03_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day03), "2025"));
        Assert.AreEqual("169709990062889", result);
    }
    
}
