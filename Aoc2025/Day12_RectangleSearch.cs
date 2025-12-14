using System.Runtime.CompilerServices;

namespace Advent_of_Code_2025;

[TestClass]
public class Day12_RectangleSearch
{
    public record RectanglePattern(int Width, int Height, Dictionary<int, int> Recipe)
    {
        public int TotalCells => Width * Height;
        public int TotalShapes => Recipe.Values.Sum();
        
        public override string ToString() => 
            $"{Width}x{Height} ({TotalCells} cells) = {string.Join(" + ", Recipe.Select(kvp => $"{kvp.Value}×Present{kvp.Key}"))}";
    }

    [TestMethod]
    public void FindAllRectanglePatterns_2to6_Shapes()
    {
        var input = """
            0:
            #.#
            ###
            ##.

            1:
            ..#
            .##
            ##.

            2:
            ###
            ###
            #..

            3:
            #..
            ##.
            ###

            4:
            ###
            #.#
            #.#

            5:
            ###
            .#.
            ###
            """;

        var presents = new List<Day12.Present>();
        var parts = input.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.None);
        foreach (var part in parts)
        {
            presents.Add(Day12.Present.Parse(part));
        }

        Day12.Region.Presents = presents;
        
        Console.WriteLine("Present cell counts:");
        for (int i = 0; i < presents.Count; i++)
        {
            Console.WriteLine($"  Present {i}: {presents[i].Shapes[0].CellCount} cells");
        }
        
        var allPatterns = new List<RectanglePattern>();
        
        // Search for rectangles using 2-6 shapes
        for (int totalShapes = 2; totalShapes <= 6; totalShapes++)
        {
            Console.WriteLine($"\n=== Searching with {totalShapes} shapes ===");
            var patterns = FindPatternsWithNShapes(presents, totalShapes);
            allPatterns.AddRange(patterns);
            Console.WriteLine($"Found {patterns.Count} patterns with {totalShapes} shapes");
        }
        
        Console.WriteLine($"\n=== SUMMARY ===");
        Console.WriteLine($"Total patterns found: {allPatterns.Count}");
        
        // Group by size
        var bySize = allPatterns.GroupBy(p => p.TotalCells).OrderBy(g => g.Key);
        foreach (var group in bySize)
        {
            Console.WriteLine($"\n{group.Key} cells ({group.Count()} patterns):");
            foreach (var pattern in group.Take(3))
            {
                Console.WriteLine($"  {pattern}");
            }
            if (group.Count() > 3)
                Console.WriteLine($"  ... and {group.Count() - 3} more");
        }
    }

    private List<RectanglePattern> FindPatternsWithNShapes(List<Day12.Present> presents, int n)
    {
        var found = new List<RectanglePattern>();
        
        // Generate all possible distributions of n shapes among 6 present types
        GenerateDistributions(n, 0, new int[presents.Count], (distribution) =>
        {
            // Calculate total cells
            int totalCells = 0;
            for (int i = 0; i < distribution.Length; i++)
            {
                totalCells += distribution[i] * presents[i].Shapes[0].CellCount;
            }
            
            if (totalCells == 0) return;
            
            // Try all possible rectangle dimensions
            for (int w = 1; w <= totalCells; w++)
            {
                if (totalCells % w == 0)
                {
                    int h = totalCells / w;
                    
                    // Try to fill this rectangle
                    if (TryFillRectangle(w, h, distribution, presents))
                    {
                        var recipe = new Dictionary<int, int>();
                        for (int i = 0; i < distribution.Length; i++)
                        {
                            if (distribution[i] > 0)
                                recipe[i] = distribution[i];
                        }
                        
                        // Check for duplicate (same or rotated)
                        bool isDup = found.Any(p => 
                            ((p.Width == w && p.Height == h) || (p.Width == h && p.Height == w)) &&
                            p.Recipe.Count == recipe.Count &&
                            p.Recipe.All(kvp => recipe.TryGetValue(kvp.Key, out int v) && v == kvp.Value));
                        
                        if (!isDup)
                        {
                            var pattern = new RectanglePattern(w, h, recipe);
                            found.Add(pattern);
                            Console.WriteLine($"  Found: {pattern}");
                        }
                        
                        return; // Found one for this distribution, move to next
                    }
                }
            }
        });
        
        return found;
    }

    private void GenerateDistributions(int remaining, int presentIdx, int[] counts, Action<int[]> action)
    {
        if (remaining == 0)
        {
            action(counts);
            return;
        }
        
        if (presentIdx >= counts.Length)
            return;
        
        // Try using 0 to remaining of this present type
        for (int use = 0; use <= remaining; use++)
        {
            counts[presentIdx] = use;
            GenerateDistributions(remaining - use, presentIdx + 1, counts, action);
        }
        counts[presentIdx] = 0;
    }

    private bool TryFillRectangle(int width, int height, int[] targetCounts, List<Day12.Present> presents)
    {
        var board = new bool[height * width];
        var counts = new int[presents.Count];
        int targetCells = width * height;
        
        // Timeout after 500ms
        var cts = new System.Threading.CancellationTokenSource();
        cts.CancelAfter(500);
        
        try
        {
            return TryPlaceShapes(board, width, height, 0, counts, targetCounts, targetCells, presents, cts.Token);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private bool TryPlaceShapes(bool[] board, int width, int height, int presentIdx, int[] counts, 
        int[] targetCounts, int targetCells, List<Day12.Present> presents, System.Threading.CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        
        // Check if filled
        int filledCells = board.Count(c => c);
        if (filledCells == targetCells)
        {
            // Verify we used the right counts
            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] != targetCounts[i])
                    return false;
            }
            return true;
        }
        
        if (presentIdx >= presents.Count)
            return false;
        
        // If we've already placed the target for this present, move to next
        if (counts[presentIdx] >= targetCounts[presentIdx])
            return TryPlaceShapes(board, width, height, presentIdx + 1, counts, targetCounts, targetCells, presents, ct);
        
        // Try placing this present's shapes
        foreach (var shape in presents[presentIdx].Shapes)
        {
            int sh = shape.Height;
            int sw = shape.Width;
            
            for (int y = 0; y <= height - sh; y++)
            {
                for (int x = 0; x <= width - sw; x++)
                {
                    if (CanPlace(shape, board, width, x, y))
                    {
                        Place(shape, board, width, x, y, true);
                        counts[presentIdx]++;
                        
                        if (TryPlaceShapes(board, width, height, presentIdx, counts, targetCounts, targetCells, presents, ct))
                            return true;
                        
                        counts[presentIdx]--;
                        Place(shape, board, width, x, y, false);
                    }
                }
            }
        }
        
        // Try next present type
        return TryPlaceShapes(board, width, height, presentIdx + 1, counts, targetCounts, targetCells, presents, ct);
    }

    private bool CanPlace(Day12.ShapeData shape, bool[] board, int boardWidth, int x, int y)
    {
        int sw = shape.Width;
        var cells = shape.Cells;
        
        for (int dy = 0; dy < shape.Height; dy++)
        {
            int boardRowStart = (y + dy) * boardWidth;
            int shapeRowStart = dy * sw;
            for (int dx = 0; dx < sw; dx++)
            {
                if (cells[shapeRowStart + dx] && board[boardRowStart + x + dx])
                    return false;
            }
        }
        return true;
    }

    private void Place(Day12.ShapeData shape, bool[] board, int boardWidth, int x, int y, bool value)
    {
        int sw = shape.Width;
        var cells = shape.Cells;
        
        for (int dy = 0; dy < shape.Height; dy++)
        {
            int boardRowStart = (y + dy) * boardWidth;
            int shapeRowStart = dy * sw;
            for (int dx = 0; dx < sw; dx++)
            {
                if (cells[shapeRowStart + dx])
                    board[boardRowStart + x + dx] = value;
            }
        }
    }
}
