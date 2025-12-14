using System.Runtime.CompilerServices;

namespace Advent_of_Code_2025;

[TestClass]
public class Day12
{
    public readonly struct ShapeData
    {
        public readonly bool[] Cells;
        public readonly int Height;
        public readonly int Width;
        public readonly int CellCount;

        public ShapeData(bool[,] cells)
        {
            Height = cells.GetLength(0);
            Width = cells.GetLength(1);
            Cells = new bool[Height * Width];
            CellCount = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int idx = y * Width + x;
                    Cells[idx] = cells[y, x];
                    if (cells[y, x]) CellCount++;
                }
            }
        }

        public ShapeData(List<List<bool>> listShape)
        {
            Height = listShape.Count;
            Width = listShape[0].Count;
            Cells = new bool[Height * Width];
            CellCount = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int idx = y * Width + x;
                    Cells[idx] = listShape[y][x];
                    if (listShape[y][x]) CellCount++;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetCell(int x, int y)
        {
            return Cells[y * Width + x];
        }
    }

    public class Region
    {
        public static List<Present> Presents = [];
        public static List<int> PresentSizeOrder { get; set; } = [];
        public static List<RectanglePattern> RectanglePatterns { get; set; } = [];

        readonly Box<int> _size;
        readonly List<int> _shapeCounts;

        public record RectanglePattern(int Width, int Height, Dictionary<int, int> Recipe)
        {
            public int TotalCells => Width * Height;
            public int TotalShapes => Recipe.Values.Sum();
            
            public override string ToString() => 
                $"{Width}x{Height} ({TotalCells} cells) = {string.Join(" + ", Recipe.Select(kvp => $"{kvp.Value}×Present{kvp.Key}"))}";
        }

        public Region(Box<int> size, List<int> shapeCounts)
        {
            _size = size;
            _shapeCounts = shapeCounts;
        }

        public static Region Parse(string line)
        {
            var split = line.Split(':');
            
            var (width, height) = split[0].Split('x').Select(int.Parse).ToArray();

            Box<int> size = new(width, height);
            List<int> shapeCounts = [.. split[1].Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => int.Parse(s.Trim()))];

            return new Region(size, shapeCounts);
        }

        public override string ToString()
        {
            return $"Region({_size}, {string.Join(",", _shapeCounts)}";
        }

        public bool CanFitPresents()
        {
            var width = _size.Width;
            var height = _size.Height;
            var board = new bool[height * width];
            var counts = _shapeCounts.ToArray();
            
            // OPTIMIZATION: Check if this region exactly matches a known rectangle pattern
            foreach (var pattern in RectanglePatterns)
            {
                if ((pattern.Width == width && pattern.Height == height) ||
                    (pattern.Width == height && pattern.Height == width))
                {
                    // Check if our counts exactly match the pattern recipe
                    bool exactMatch = true;
                    for (int i = 0; i < counts.Length; i++)
                    {
                        int needed = pattern.Recipe.TryGetValue(i, out int n) ? n : 0;
                        if (counts[i] != needed)
                        {
                            exactMatch = false;
                            break;
                        }
                    }
                    
                    if (exactMatch)
                    {
                        return true; // This region exactly matches a known fillable pattern!
                    }
                }
            }
            
            // Fall back to full backtracking search
            var memo = new Dictionary<long, bool>();
            return TryPlace(0, counts, board, width, height, memo);
        }

        private static long ComputeBoardHash(bool[] board, int[] counts)
        {
            long hash = 17;
            
            // Hash the board state using ulong chunks for efficiency
            int fullUlongs = board.Length / 64;
            int i = 0;
            
            // Process 64 bits at a time
            for (int u = 0; u < fullUlongs; u++)
            {
                ulong chunk = 0;
                for (int bit = 0; bit < 64; bit++, i++)
                {
                    if (board[i])
                        chunk |= (1UL << bit);
                }
                hash = hash * 31 + (long)chunk;
            }
            
            // Process remaining bits
            if (i < board.Length)
            {
                ulong chunk = 0;
                for (int bit = 0; i < board.Length; bit++, i++)
                {
                    if (board[i])
                        chunk |= (1UL << bit);
                }
                hash = hash * 31 + (long)chunk;
            }
            
            // Hash the remaining counts
            for (int j = 0; j < counts.Length; j++)
            {
                hash = hash * 31 + counts[j];
            }
            
            return hash;
        }

        private bool TryPlace(int presentType, int[] counts, bool[] board, int width, int height, Dictionary<long, bool> memo)
        {
            if (presentType == Presents.Count)
                return counts.All(c => c == 0);

            var presentIndex = Region.PresentSizeOrder[presentType];
            if (counts[presentIndex] == 0)
            {
                return TryPlace(presentType + 1, counts, board, width, height, memo);
            }

            // Check memoization cache
            long stateHash = ComputeBoardHash(board, counts);
            if (memo.TryGetValue(stateHash, out bool cachedResult))
            {
                return cachedResult;
            }

            var shapes = Presents[presentIndex].Shapes;
            bool result = false;

            for (int shapeIdx = 0; shapeIdx < shapes.Count; shapeIdx++)
            {
                var shape = shapes[shapeIdx];
                int sh = shape.Height;
                int sw = shape.Width;
                int maxY = height - sh;
                int maxX = width - sw;
                
                for (int y = 0; y <= maxY; y++)
                {
                    for (int x = 0; x <= maxX; x++)
                    {
                        if (CanPlaceShape(shape, board, width, x, y))
                        {
                            PlaceShape(shape, board, width, x, y, true);
                            counts[presentIndex]--;
                            if (TryPlace(presentType, counts, board, width, height, memo))
                            {
                                result = true;
                                counts[presentIndex]++;
                                PlaceShape(shape, board, width, x, y, false);
                                memo[stateHash] = result;
                                return result;
                            }
                            counts[presentIndex]++;
                            PlaceShape(shape, board, width, x, y, false);
                        }
                    }
                }
            }
            
            memo[stateHash] = result;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CanPlaceShape(ShapeData shape, bool[] board, int boardWidth, int x, int y)
        {
            int sh = shape.Height;
            int sw = shape.Width;
            var cells = shape.Cells;
            int shapeWidth = shape.Width;
            
            for (int dy = 0; dy < sh; dy++)
            {
                int boardRowStart = (y + dy) * boardWidth;
                int shapeRowStart = dy * shapeWidth;
                for (int dx = 0; dx < sw; dx++)
                {
                    if (cells[shapeRowStart + dx] && board[boardRowStart + x + dx])
                        return false;
                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PlaceShape(ShapeData shape, bool[] board, int boardWidth, int x, int y, bool value)
        {
            int sh = shape.Height;
            int sw = shape.Width;
            var cells = shape.Cells;
            int shapeWidth = shape.Width;
            
            for (int dy = 0; dy < sh; dy++)
            {
                int boardRowStart = (y + dy) * boardWidth;
                int shapeRowStart = dy * shapeWidth;
                for (int dx = 0; dx < sw; dx++)
                {
                    if (cells[shapeRowStart + dx])
                        board[boardRowStart + x + dx] = value;
                }
            }
        }

        public static void FindRectanglePatterns(int maxShapes = 6)
        {
            RectanglePatterns.Clear();
            
            Console.WriteLine($"Searching for rectangle patterns (2-{maxShapes} shapes)...");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            
            for (int totalShapes = 2; totalShapes <= maxShapes; totalShapes++)
            {
                FindPatternsWithNShapes(totalShapes);
            }
            
            sw.Stop();
            Console.WriteLine($"Found {RectanglePatterns.Count} unique rectangle patterns in {sw.ElapsedMilliseconds}ms");
            
            // Show some examples
            var bySize = RectanglePatterns.GroupBy(p => p.TotalCells).OrderBy(g => g.Key).Take(5);
            foreach (var group in bySize)
            {
                Console.WriteLine($"  {group.Key} cells: {string.Join(", ", group.Take(2))}");
            }
        }

        private static void FindPatternsWithNShapes(int n)
        {
            GenerateDistributions(n, 0, new int[Presents.Count], (distribution) =>
            {
                int totalCells = 0;
                for (int i = 0; i < distribution.Length; i++)
                {
                    totalCells += distribution[i] * Presents[i].Shapes[0].CellCount;
                }
                
                if (totalCells == 0) return;
                
                for (int w = 1; w <= totalCells; w++)
                {
                    if (totalCells % w == 0)
                    {
                        int h = totalCells / w;
                        
                        if (TryFillRectangle(w, h, distribution))
                        {
                            var recipe = new Dictionary<int, int>();
                            for (int i = 0; i < distribution.Length; i++)
                            {
                                if (distribution[i] > 0)
                                    recipe[i] = distribution[i];
                            }
                            
                            bool isDup = RectanglePatterns.Any(p => 
                                ((p.Width == w && p.Height == h) || (p.Width == h && p.Height == w)) &&
                                p.Recipe.Count == recipe.Count &&
                                p.Recipe.All(kvp => recipe.TryGetValue(kvp.Key, out int v) && v == kvp.Value));
                            
                            if (!isDup)
                            {
                                RectanglePatterns.Add(new RectanglePattern(w, h, recipe));
                            }
                            
                            return;
                        }
                    }
                }
            });
        }

        private static void GenerateDistributions(int remaining, int presentIdx, int[] counts, Action<int[]> action)
        {
            if (remaining == 0)
            {
                action(counts);
                return;
            }
            
            if (presentIdx >= counts.Length)
                return;
            
            for (int use = 0; use <= remaining; use++)
            {
                counts[presentIdx] = use;
                GenerateDistributions(remaining - use, presentIdx + 1, counts, action);
            }
            counts[presentIdx] = 0;
        }

        private static bool TryFillRectangle(int width, int height, int[] targetCounts)
        {
            var board = new bool[height * width];
            var counts = new int[Presents.Count];
            int targetCells = width * height;
            
            var cts = new System.Threading.CancellationTokenSource();
            cts.CancelAfter(500);
            
            try
            {
                return TryPlaceShapesForPattern(board, width, height, 0, counts, targetCounts, targetCells, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        private static bool TryPlaceShapesForPattern(bool[] board, int width, int height, int presentIdx, int[] counts, 
            int[] targetCounts, int targetCells, System.Threading.CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            
            int filledCells = board.Count(c => c);
            if (filledCells == targetCells)
            {
                for (int i = 0; i < counts.Length; i++)
                {
                    if (counts[i] != targetCounts[i])
                        return false;
                }
                return true;
            }
            
            if (presentIdx >= Presents.Count)
                return false;
            
            if (counts[presentIdx] >= targetCounts[presentIdx])
                return TryPlaceShapesForPattern(board, width, height, presentIdx + 1, counts, targetCounts, targetCells, ct);
            
            foreach (var shape in Presents[presentIdx].Shapes)
            {
                int sh = shape.Height;
                int sw = shape.Width;
                
                for (int y = 0; y <= height - sh; y++)
                {
                    for (int x = 0; x <= width - sw; x++)
                    {
                        if (CanPlaceShape(shape, board, width, x, y))
                        {
                            PlaceShape(shape, board, width, x, y, true);
                            counts[presentIdx]++;
                            
                            if (TryPlaceShapesForPattern(board, width, height, presentIdx, counts, targetCounts, targetCells, ct))
                                return true;
                            
                            counts[presentIdx]--;
                            PlaceShape(shape, board, width, x, y, false);
                        }
                    }
                }
            }
            
            return TryPlaceShapesForPattern(board, width, height, presentIdx + 1, counts, targetCounts, targetCells, ct);
        }
    }

    public class Present
    {
        required public List<ShapeData> Shapes { get; init; }

        public static Present Parse(string part)
        {
            
            List<List<bool>> shape = [];
            foreach (var line in part.Split('\n').Select(s => s.Trim()))
            {
                if (string.IsNullOrWhiteSpace(line) || line.Contains(':')) continue;
                shape.Add([.. line.Select(c => c == '#')]);
            }
            var result = new Present()
            {
                Shapes = GenerateUniqueShapes(shape)
            };

            return result;
        }


        /// <summary>
        /// Generates all unique rotations and flips of the shape.
        /// </summary>
        internal static List<ShapeData> GenerateUniqueShapes(List<List<bool>> shape)
        {
            var unique = new HashSet<string>();
            var result = new List<ShapeData>();

            List<List<bool>> current = shape;
            for (int i = 0; i < 4; i++)
            {
                if (i > 0)
                {
                    current = Rotate90(current);
                }
                AddIfUnique(current, unique, result);

                var flipped = FlipHorizontal(current);
                AddIfUnique(flipped, unique, result);
            }
            return result;
        }

        private static void AddIfUnique(List<List<bool>> shape, HashSet<string> unique, List<ShapeData> result)
        {
            var key = ShapeToString(shape);
            if (unique.Add(key))
            {
                result.Add(new ShapeData(shape));
            }
        }

        private static string ShapeToString(List<List<bool>> shape) =>
            string.Join("\n", shape.Select(row => string.Concat(row.Select(b => b ? '#' : '.'))));

        private static List<List<bool>> Rotate90(List<List<bool>> shape)
        {
            var rows = shape.Count;
            var cols = shape[0].Count;
            var rotated = new List<List<bool>>();
            for (int c = 0; c < cols; c++)
            {
                rotated.Add([.. Enumerable.Range(0, rows).Select(r => shape[rows - r - 1][c])]);
            }
            return rotated;
        }

        private static List<List<bool>> FlipHorizontal(List<List<bool>> shape) =>
            [.. shape.Select(row => row.AsEnumerable().Reverse().ToList())];
    }

    private static string Part1(IEnumerable<string> input)
    {
        var result = 0;
        var parts = string.Join("\n", input).Split("\n\n");
        var presents = new List<Present>();
        var n = 6;
        foreach (var part in parts)
        {
            if (presents.Count == n && Region.Presents.Count == 0)
            {
                Region.Presents = presents;
                Region.PresentSizeOrder = [.. Enumerable.Range(0, presents.Count).OrderBy(x => -Region.Presents[x].Shapes[0].CellCount)];
                
                // Find all rectangle patterns (2-6 shapes)
                Region.FindRectanglePatterns(6);
            }

            if (presents.Count < n)
            {
                presents.Add(Present.Parse(part));
            }
            else
            {
                foreach (var line in part.Split('\n'))
                {
                    var region = Region.Parse(line);
                    if (region.CanFitPresents())
                    {
                        result++;
                    }
                }
            }
        }
        return result.ToString();
    }
    
    private static string Part2(IEnumerable<string> input)
    {
        var result = new StringBuilder();
        foreach (var line in input)
        {
        }
        return result.ToString();
    }
    
    [TestMethod]
    public void Day12_Part1_Example01()
    {
        var input = """
            0:
            ###
            ##.
            ##.

            1:
            ###
            ##.
            .##

            2:
            .##
            ###
            ##.

            3:
            ##.
            ###
            ##.

            4:
            ###
            #..
            ###

            5:
            ###
            .#.
            ###

            4x4: 0 0 0 0 2 0
            12x5: 1 0 1 0 2 2
            12x5: 1 0 1 0 3 2
            """;
        var result = Part1(Common.GetLines(input));
        Assert.AreEqual("2", result);
    }

    [TestMethod]
    public void Day12_Part1_Example01_Performance()
    {
        var input = """
            0:
            ###
            ##.
            ##.

            1:
            ###
            ##.
            .##

            2:
            .##
            ###
            ##.

            3:
            ##.
            ###
            ##.

            4:
            ###
            #..
            ###

            5:
            ###
            .#.
            ###

            4x4: 0 0 0 0 2 0
            12x5: 1 0 1 0 2 2
            12x5: 1 0 1 0 3 2
            """;

        // Single run measurement
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = Part1(Common.GetLines(input));
        sw.Stop();

        Console.WriteLine($"Single execution time: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"Result: {result}");
    }
    
    [TestMethod]
    public void Day12_Part1_Example02()
    {
        var input = """
            <TODO>
            """;
        var result = Part1(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day12_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day12), "2025"));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day12_Part2_Example01()
    {
        var input = """
            <TODO>
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day12_Part2_Example02()
    {
        var input = """
            <TODO>
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day12_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day12), "2025"));
        Assert.AreEqual("", result);
    }
    
}
