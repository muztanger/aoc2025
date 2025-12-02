namespace Advent_of_Code_2025;

[TestClass]
public class Day02
{
    private static string Part1(IEnumerable<string> input)
    {
        var ranges = new List<(long Start, long End)>();
        foreach (var line in input)
        {
            var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                var rangeParts = part.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var start = long.Parse(rangeParts[0]);
                var end = long.Parse(rangeParts[1]);
                ranges.Add((start, end));
            }
        }
        bool IsInvalidId(long id)
        {
            if (id < 10)
            {
                return false;
            }
            var idStr = id.ToString();
            if (idStr.Length % 2 != 0)
            {
                return false;
            }
            var mid = idStr.Length / 2;
            for (var i = 0; i < idStr.Length / 2; i++)
            {
                if (idStr[i] != idStr[mid + i])
                {
                    return false;
                }
            }
            return true;
        }
        var result = 0L;
        foreach (var (start, end) in ranges)
        {
            for (var id = start; id <= end; id++)
            {
                if (IsInvalidId(id))
                {
                    result += id;
                }
            }
        }
        return result.ToString();
    }
    
    private static string Part2(IEnumerable<string> input)
    {
        var ranges = new List<(long Start, long End)>();
        foreach (var line in input)
        {
            var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                var rangeParts = part.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var start = long.Parse(rangeParts[0]);
                var end = long.Parse(rangeParts[1]);
                ranges.Add((start, end));
            }
        }
        var result = 0L;
        foreach (var (start, end) in ranges)
        {
            var found = new List<long>();
            for (var id = start; id <= end; id++)
            {
                if (IsInvalidId(id))
                {
                    found.Add(id);
                    result += id;
                }
            }
            Console.WriteLine($"{start}-{end}: {string.Join(", ", found)}");
        }
        return result.ToString();
    }

    private static bool IsInvalidId(long id)
    {
        if (id < 10)
        {
            return false;
        }

        var idStr = id.ToString();

        for (var length = idStr.Length / 2; length > 0; length--)
        {
            if (idStr.Length % length != 0)
            {
                continue;
            }

            var isMatch = true;
            for (var i = 0; i < length && isMatch; i++)
            {
                char check = idStr[i];
                for (var k = i + length; k < idStr.Length; k += length)
                {
                    if (idStr[k] != check)
                    {
                        isMatch = false;
                        break;
                    }
                }
            }

            if (isMatch)
            {
                return true;
            }
        }

        return false;
    }

    [TestMethod]
    [DataRow("999", true)]
    [DataRow("824824824", true)]
    [DataRow("1010", true)]
    [DataRow("1111", true)]
    [DataRow("1212", true)]
    [DataRow("123123", true)]
    [DataRow("12341234", true)]
    [DataRow("1188511885", true)]
    [DataRow("1234", false)]
    public void Day02_IsInvalidId_Tests(string idStr, bool expected)
    {
        var id = long.Parse(idStr);
        var result = IsInvalidId(id);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void Day02_Part1_Example01()
    {
        var input = """
            11-22,95-115,998-1012,1188511880-1188511890,222220-222224,1698522-1698528,446443-446449,38593856-38593862,565653-565659,824824821-824824827,2121212118-2121212124
            """;
        var result = Part1(Common.GetLines(input));
        Assert.AreEqual("1227775554", result);
    }
        
    [TestMethod]
    public void Day02_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day02), "2025"));
        Assert.AreEqual("28844599675", result);
    }
    
    [TestMethod]
    public void Day02_Part2_Example01()
    {
        var input = """
            11-22,95-115,998-1012,1188511880-1188511890,222220-222224,1698522-1698528,446443-446449,38593856-38593862,565653-565659,824824821-824824827,2121212118-2121212124
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("4174379265", result);
    }
    
    
    [TestMethod]
    public void Day02_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day02), "2025"));
        Assert.AreEqual("48778605167", result);
    }
    
}
