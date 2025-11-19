using Aoc2024.Commons;
using System.Security.Cryptography;

namespace Advent_of_Code_2025.Commons;
public static class Common
{
    public static IEnumerable<string> DayInput(string day, string year = "2024")
    {
        var profiler = new Profiler();
        profiler.Start();

        var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", year == "2024" ? "" : year);
        var fileName = Path.Combine(baseDir, "Input", $"{day}.input");

        if (!File.Exists(fileName))
        {
            using var fs = File.Create(fileName);
        }

        Console.WriteLine($"Read from {Path.GetFullPath(fileName)}");

        int counter = 0;
        string? line;

        // Read the file and display it line by line.  
        using var file = new StreamReader(fileName);
        while (file != null && (line = file.ReadLine()) != null)
        {
            yield return line;
            counter++;
        }

        profiler.Stop();

        Console.WriteLine("There were {0} lines.", counter);
        profiler.Print("Reading file stats");
    }

    public static IEnumerable<string> GetLines(string input)
    {
        using StringReader reader = new(input);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            yield return line;
        }
    }

    public static string ComputeHash(string rawData)
    {
        using (var hash = SHA1.Create())
        {
            byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }


    // https://stackoverflow.com/questions/228038/best-way-to-reverse-a-string?page=1&tab=votes#tab-top
    public static string Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    // Found on https://stackoverflow.com/questions/955982/tuples-or-arrays-as-dictionary-keys-in-c-sharp
    /// <summary>
    /// EO: 2016-04-14
    /// Generator of all permutations of an array of anything.
    /// Base on Heap's Algorithm. See: https://en.wikipedia.org/wiki/Heap%27s_algorithm#cite_note-3
    /// </summary>
    //public static class Permutations
    //{
    /// <summary>
    /// Heap's algorithm to find all pmermutations. Non recursive, more efficient.
    /// </summary>
    /// <param name="items">Items to permute in each possible ways</param>
    /// <param name="funcExecuteAndTellIfShouldStop"></param>
    /// <returns>Return true if cancelled</returns> 
    public static bool ForAllPermutation<T>(T[] items, Func<T[], bool> funcExecuteAndTellIfShouldStop)
    {
        int countOfItem = items.Length;

        if (countOfItem <= 1)
        {
            return funcExecuteAndTellIfShouldStop(items);
        }

        var indexes = new int[countOfItem];
        for (int i = 0; i < countOfItem; i++)
        {
            indexes[i] = 0;
        }

        if (funcExecuteAndTellIfShouldStop(items))
        {
            return true;
        }

        for (int i = 1; i < countOfItem;)
        {
            if (indexes[i] < i)
            { // On the web there is an implementation with a multiplication which should be less efficient.
                if ((i & 1) == 1) // if (i % 2 == 1)  ... more efficient ??? At least the same.
                {
                    Swap(ref items[i], ref items[indexes[i]]);
                }
                else
                {
                    Swap(ref items[i], ref items[0]);
                }

                if (funcExecuteAndTellIfShouldStop(items))
                {
                    return true;
                }

                indexes[i]++;
                i = 1;
            }
            else
            {
                indexes[i++] = 0;
            }
        }

        return false;
    }

    /// <summary>
    /// This function is to show a linq way but is far less efficient
    /// From: StackOverflow user: Pengyang : http://stackoverflow.com/questions/756055/listing-all-permutations-of-a-string-integer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
    {
        if (length == 1) return list.Select(t => new T[] { t });

        return GetPermutations(list, length - 1)
            .SelectMany(t => list.Where(e => !t.Contains(e)),
                (t1, t2) => t1.Concat(new T[] { t2 }));
    }

    public static void Swap<T>(ref T a, ref T b)
    {
        (b, a) = (a, b);
    }

    public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
    {
        (list[indexB], list[indexA]) = (list[indexA], list[indexB]);
        return list;
    }

    // https://stackoverflow.com/questions/7802822/all-possible-combinations-of-a-list-of-values
    public static List<List<T>> GetAllCombos<T>(List<T> list)
    {
        int comboCount = (int)Math.Pow(2, list.Count) - 1;
        List<List<T>> result = new();
        for (int i = 1; i < comboCount + 1; i++)
        {
            // make each combo here
            result.Add(new List<T>());
            for (int j = 0; j < list.Count; j++)
            {
                if ((i >> j) % 2 != 0)
                    result.Last().Add(list[j]);
            }
        }
        return result;
    }

    public static int GetSequenceHashCode<T>(this IEnumerable<T> sequence)
    {
        const int seed = 743;
        const int modifier = 71;

        unchecked
        {
            var result = seed;
            foreach (var item in sequence)
            {
                if (item != null)
                {
                    result += (result * modifier) + item.GetHashCode();
                }
            }
            return result;
        }
    }

    public static int GetSequenceHashCodeFromString<T>(this IEnumerable<T> sequence)
    {
        return string.Join(",", sequence).GetHashCode();
    }

}

public class GeneralizedComparer<T> : IComparer<T> where T : IComparable
{
    int IComparer<T>.Compare(T? x, T? y)
    {
        if (x == null)
        {
            return y == null ? 0 : 1;
        }

        int comp = x.CompareTo(y);

        if (comp != 0)
            return comp;
        else
            return 1;
    }
}

//public sealed class ReverseComparer<T> : IComparer<T>
//{
//    private readonly IComparer<T> inner;
//    public ReverseComparer() : this(inner: null) { }
//    public ReverseComparer(IComparer<T> inner)
//    {
//        this.inner = inner ?? Comparer<T>.Default;
//    }
//    int IComparer<T>.Compare(T? x, T? y) { return inner.Compare(y, x); }
//}

public static class StringExtensions
{
    public static string[] SplitTrim(this string src, string separator)
    {
        return src.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    public static string[] SplitTrim(this string src, char separator)
    {
        return src.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}

public static class ArrayExtensions
{
    public static void Deconstruct<T>(this T[] srcArray, out T out0)
    {
        if (srcArray == null || !(srcArray.Length == 1))
        {
            throw new ArgumentException($"null or wrong array size {srcArray}", nameof(srcArray));
        }
        out0 = srcArray[0];
    }

    public static void Deconstruct<T>(this T[] srcArray, out T out0, out T out1)
    {
        if (srcArray == null || !(srcArray.Length == 2))
        {
            throw new ArgumentException($"null or wrong array size {srcArray}", nameof(srcArray));
        }
        out0 = srcArray[0];
        out1 = srcArray[1];
    }

    public static void Deconstruct<T>(this T[] srcArray, out T out0, out T out1, out T out2)
    {
        if (srcArray == null || !(srcArray.Length == 3))
        {
            throw new ArgumentException($"null or wrong array size {srcArray}", nameof(srcArray));
        }
        out0 = srcArray[0];
        out1 = srcArray[1];
        out2 = srcArray[2];
    }

    public static void Deconstruct<T>(this T[] srcArray, out T out0, out T out1, out T out2, out T out3)
    {
        if (srcArray == null || !(srcArray.Length == 4))
        {
            throw new ArgumentException($"null or wrong array size {srcArray}", nameof(srcArray));
        }
        out0 = srcArray[0];
        out1 = srcArray[1];
        out2 = srcArray[2];
        out3 = srcArray[3];
    }

    public static void Deconstruct<T>(this T[] srcArray, out T out0, out T out1, out T out2, out T out3, out T out4)
    {
        if (srcArray == null || !(srcArray.Length == 5))
        {
            throw new ArgumentException($"null or wrong array size {srcArray}", nameof(srcArray));
        }
        out0 = srcArray[0];
        out1 = srcArray[1];
        out2 = srcArray[2];
        out3 = srcArray[3];
        out4 = srcArray[4];
    }

    public static void Deconstruct<T>(this T[] srcArray, out T out0, out T out1, out T out2, out T out3, out T out4, out T out5)
    {
        if (srcArray == null || !(srcArray.Length == 6))
        {
            throw new ArgumentException($"null or wrong array size {srcArray}", nameof(srcArray));
        }
        out0 = srcArray[0];
        out1 = srcArray[1];
        out2 = srcArray[2];
        out3 = srcArray[3];
        out4 = srcArray[4];
        out5 = srcArray[5];
    }

    public static void Deconstruct<T>(this T[] srcArray, out T out0, out T out1, out T out2, out T out3, out T out4, out T out5, out T out6)
    {
        if (srcArray == null || !(srcArray.Length == 7))
        {
            throw new ArgumentException($"null or wrong array size {srcArray}", nameof(srcArray));
        }
        out0 = srcArray[0];
        out1 = srcArray[1];
        out2 = srcArray[2];
        out3 = srcArray[3];
        out4 = srcArray[4];
        out5 = srcArray[5];
        out6 = srcArray[6];
    }

    public static void Deconstruct<T>(this T[] srcArray, out T out0, out T out1, out T out2, out T out3, out T out4, out T out5, out T out6, out T out7)
    {
        if (srcArray == null || !(srcArray.Length == 8))
        {
            throw new ArgumentException($"null or wrong array size {srcArray}", nameof(srcArray));
        }
        out0 = srcArray[0];
        out1 = srcArray[1];
        out2 = srcArray[2];
        out3 = srcArray[3];
        out4 = srcArray[4];
        out5 = srcArray[5];
        out6 = srcArray[6];
        out7 = srcArray[7];
    }
}
