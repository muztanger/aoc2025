namespace Advent_of_Code_2025;

[TestClass]
public class Day10
{
    class Machine
    {
        class Lights
        {

            public List<bool> State;
            public int Count => State.Count;
            
            public Lights(List<bool> state)
            {
                State = state;
            }

            public Lights(int count)
            {
                State = [.. new bool[count]];
            }

            public Lights(Lights other)
            {
                State = [.. other.State];
            }

            public override bool Equals(object? obj)
            {
                if (obj is not Lights other)
                {
                    return false;
                }
                if (State.Count != other.State.Count)
                {
                    return false;
                }
                for (int i = 0; i < State.Count; i++)
                {
                    if (State[i] != other.State[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                int hash = 17;
                foreach (var light in State)
                {
                    hash = hash * 31 + (light ? 1 : 0);
                }
                return hash;
            }

            public override string ToString()
            {
                return string.Concat(State.Select(b => b ? '#' : '.'));
            }

            internal void Toggle(List<int> wirings)
            {
                foreach (var index in wirings)
                {
                    State[index] = !State[index];
                }
            }
        }

        class JoltageLevels
        {
            internal List<int> Values;
            public JoltageLevels(List<int> values)
            {
                Values = values;
            }

            public JoltageLevels(int count)
            {
                Values = [.. new int[count]];
            }

            public JoltageLevels(JoltageLevels other)
            {
                Values = [.. other.Values];
            }

            public override bool Equals(object? obj)
            {
                if (obj is not JoltageLevels other)
                {
                    return false;
                }
                if (Values.Count != other.Values.Count)
                {
                    return false;
                }
                for (int i = 0; i < Values.Count; i++)
                {
                    if (Values[i] != other.Values[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                int hash = 17;
                foreach (var value in Values)
                {
                    hash = hash * 31 + value;
                }
                return hash;
            }

            public override string ToString()
            {
                return string.Join(",", Values);
            }

            internal void Press(List<int> list)
            {
                foreach (var index in list)
                {
                    Values[index]++;
                }
            }

            internal bool IsTooHighLevel(JoltageLevels requirements)
            {
                for (var i = 0; i < Values.Count; i++)
                {
                    if (Values[i] > requirements.Values[i])
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private const int MEMORY_LIMIT = 10_000_000;
        Lights _lights;
        readonly Lights _diagram;
        List<List<int>> _wirings;
        JoltageLevels _requirements;
        JoltageLevels _levels;

        public Machine(List<bool> diagram, List<List<int>> wirings, List<int> requirements)
        {
            _diagram = new Lights(diagram);
            _lights = new Lights(_diagram.Count);
            _wirings = wirings;
            _requirements = new JoltageLevels(requirements);
            _levels = new JoltageLevels(requirements.Count);
        }

        public static Machine Parse(string input)
        {
            var lightDiagramMatch = Regex.Match(input, @"\[([^\]]+)\]");
            Assert.IsTrue(lightDiagramMatch.Success);
            Assert.IsTrue(lightDiagramMatch.Groups[1].Success);
            List<bool> lights = [.. lightDiagramMatch.Groups[1].Value.Select(c => c == '#')];

            var wiringsMatches = Regex.Matches(input, @"\(([^)]+)\)");
            Assert.IsNotEmpty(wiringsMatches);
            var wirings = new List<List<int>>();
            foreach (Match match in wiringsMatches)
            {
                wirings.Add([.. match.Groups[1].Value.Split(",").Select(int.Parse)]);
            }

            var requirementsMatch = Regex.Match(input, @"{([^}]+)}");
            Assert.IsTrue(requirementsMatch.Success);
            Assert.IsTrue(requirementsMatch.Groups[1].Success);
            List<int> requirements = [.. requirementsMatch.Groups[1].Value.Split(",").Select(int.Parse)];

            return new Machine(lights, wirings, requirements);
        }

        public int FewestStepsToLight()
        {
            var initialState = (_lights, 0);
            var visited = new Dictionary<(Lights lights, int step), int>();
            var queue = new Queue<((Lights lights, int step) state, int steps)>();
            queue.Enqueue((initialState, 0));
            visited[initialState] = 0;
            while (queue.Count > 0)
            {
                var (currentState, steps) = queue.Dequeue();
                var (currentLights, currentWiringIndex) = currentState;
                if (currentLights.Equals(_diagram))
                {
                    return steps - 1;
                }
                Assert.IsNotNull(currentLights);
                // Toggle lights based on current wiring
                var newLights = new Lights(currentLights);
                newLights.Toggle(_wirings[currentWiringIndex]);

                for (var nextWiringIndex = 0; nextWiringIndex < _wirings.Count; nextWiringIndex++)
                {
                    var newState = (newLights, nextWiringIndex);
                    if (!visited.TryGetValue(newState, out var visitedCount))
                    {
                        visited[newState] = steps + 1;
                        queue.Enqueue((newState, steps + 1));
                    }
                    else if (visitedCount > steps + 1)
                    {
                        visited[newState] = steps + 1;
                        queue.Enqueue((newState, steps + 1));
                    }
                }
            }
            return -1; // No solution found
        }

        internal int FewestPressesToJoltage()
        {
            var target = _requirements.Values.ToArray();
            
            // Build wiring matrix
            var wiringMatrix = new int[_wirings.Count][];
            for (int w = 0; w < _wirings.Count; w++)
            {
                wiringMatrix[w] = new int[target.Length];
                foreach (var idx in _wirings[w])
                {
                    wiringMatrix[w][idx]++;
                }
            }
            
            return SolveWithDivideAndConquer(wiringMatrix, target);
        }

        private static int SolveWithDivideAndConquer(int[][] wiringMatrix, int[] target)
        {
            int targetLen = target.Length;
            int numButtons = wiringMatrix.Length;
            
            Console.WriteLine("Using divide-and-conquer approach");
            
            // Step 1: Search BACKWARD from target to find first equal joltage state
            Console.WriteLine("Searching backward from target to find equal state...");
            var (equalHeight, pressesFromEqual) = SearchBackwardToEqualState(wiringMatrix, target);
            
            if (equalHeight < 0)
            {
                Console.WriteLine("No equal state found, using direct search");
                return SolveWithButtonCounts(wiringMatrix, target);
            }
            
            Console.WriteLine($"Found equal state at height {equalHeight}, {pressesFromEqual} presses from equal to target");
            
            // Step 2: Find equal-to-equal patterns for building up from 0 to equalHeight
            Console.WriteLine($"Finding equal-to-equal patterns up to height {equalHeight}...");
            var patterns = FindEqualToEqualPatterns(wiringMatrix, maxHeight: equalHeight, maxPresses: 15);
            Console.WriteLine($"Found {patterns.Count} patterns");
            
            if (patterns.Count == 0)
            {
                Console.WriteLine("No patterns found, using direct search");
                return SolveWithButtonCounts(wiringMatrix, target);
            }
            
            // Step 3: Use patterns to go from 0 to the equal state
            Console.WriteLine($"Finding pattern combination to reach height {equalHeight}...");
            int pressesToEqual = FindPatternCombination(patterns, equalHeight);
            
            if (pressesToEqual < 0)
            {
                Console.WriteLine("Cannot reach equal state using patterns, using direct search");
                return SolveWithButtonCounts(wiringMatrix, target);
            }
            
            int total = pressesToEqual + pressesFromEqual;
            Console.WriteLine($"Total: {pressesToEqual} (0 to equal) + {pressesFromEqual} (equal to target) = {total}");
            
            return total;
        }

        private static (int equalHeight, int pressesFromEqual) SearchBackwardToEqualState(int[][] wiringMatrix, int[] target)
        {
            int targetLen = target.Length;
            int numButtons = wiringMatrix.Length;
            
            // BFS backward from target, "removing" button presses
            var pq = new PriorityQueue<(int[] state, int presses), int>();
            var visited = new HashSet<long>();
            
            var targetHash = ComputeStateHash(target);
            pq.Enqueue((target, 0), 0);
            visited.Add(targetHash);
            
            const int maxPresses = 100;
            const int maxStates = 1000000;
            
            while (pq.Count > 0 && visited.Count < maxStates)
            {
                var (state, presses) = pq.Dequeue();
                
                if (presses > maxPresses) continue;
                
                // Check if current state is all equal
                bool allEqual = true;
                int firstValue = state[0];
                for (int i = 1; i < targetLen; i++)
                {
                    if (state[i] != firstValue)
                    {
                        allEqual = false;
                        break;
                    }
                }
                
                if (allEqual)
                {
                    Console.WriteLine($"  Found equal state at height {firstValue} after {presses} presses backward");
                    return (firstValue, presses);
                }
                
                // Try "un-pressing" each button (subtract its effects)
                for (int w = 0; w < numButtons; w++)
                {
                    bool valid = true;
                    
                    // Check if we can subtract this button's effects (all values stay >= 0)
                    for (int i = 0; i < targetLen; i++)
                    {
                        if (state[i] - wiringMatrix[w][i] < 0)
                        {
                            valid = false;
                            break;
                        }
                    }
                    
                    if (!valid) continue;
                    
                    // Create new state by subtracting button effects
                    var prevState = new int[targetLen];
                    for (int i = 0; i < targetLen; i++)
                    {
                        prevState[i] = state[i] - wiringMatrix[w][i];
                    }
                    
                    var prevHash = ComputeStateHash(prevState);
                    
                    if (visited.Add(prevHash))
                    {
                        // Heuristic: encourage moving toward equal values
                        var variance = 0.0;
                        var avg = prevState.Sum() / targetLen;
                        for (int i = 0; i < targetLen; i++)
                        {
                            var diff = prevState[i] - avg;
                            variance += diff * diff;
                        }

                        pq.Enqueue((prevState, presses + 1), (int)Math.Round(presses + 1.0 + variance));
                    }
                }
            }
            
            Console.WriteLine($"  No equal state found after exploring {visited.Count} states");
            return (-1, -1);
        }

        private static List<(int presses, int heightChange, int[] buttonCounts)> FindEqualToEqualPatterns(
            int[][] wiringMatrix, int maxHeight, int maxPresses)
        {
            int targetLen = wiringMatrix[0].Length;
            int numButtons = wiringMatrix.Length;
            
            var patterns = new List<(int presses, int heightChange, int[] buttonCounts)>();
            var queue = new Queue<(int[] buttonCounts, int presses)>();
            var visited = new HashSet<long>();
            
            var start = new int[numButtons];
            queue.Enqueue((start, 0));
            visited.Add(ComputeButtonHash(start));
            
            while (queue.Count > 0)
            {
                var (buttonCounts, presses) = queue.Dequeue();
                
                if (presses > 0 && presses <= maxPresses)
                {
                    var levels = new int[targetLen];
                    for (int w = 0; w < numButtons; w++)
                    {
                        for (int i = 0; i < targetLen; i++)
                        {
                            levels[i] += buttonCounts[w] * wiringMatrix[w][i];
                        }
                    }
                    
                    // Check if all equal
                    bool allEqual = true;
                    int firstLevel = levels[0];
                    for (int i = 1; i < targetLen; i++)
                    {
                        if (levels[i] != firstLevel)
                        {
                            allEqual = false;
                            break;
                        }
                    }
                    
                    // Only add patterns that don't exceed our target equal height
                    if (allEqual && firstLevel > 0 && firstLevel <= maxHeight)
                    {
                        patterns.Add((presses, firstLevel, buttonCounts));
                    }
                    
                    // Don't explore beyond maxHeight - prune this branch
                    if (allEqual && firstLevel > maxHeight)
                    {
                        continue; // Skip exploring from this state
                    }
                }
                
                if (presses >= maxPresses) continue;
                
                // Try pressing each button
                for (int w = 0; w < numButtons; w++)
                {
                    var nextCounts = new int[numButtons];
                    for (int b = 0; b < numButtons; b++)
                    {
                        nextCounts[b] = buttonCounts[b];
                    }
                    nextCounts[w]++;
                    
                    var hash = ComputeButtonHash(nextCounts);
                    if (visited.Add(hash))
                    {
                        // Quick check: don't explore if current joltages already exceed maxHeight
                        var levels = new int[targetLen];
                        bool exceedsMax = false;
                        for (int w2 = 0; w2 < numButtons; w2++)
                        {
                            for (int i = 0; i < targetLen; i++)
                            {
                                levels[i] += nextCounts[w2] * wiringMatrix[w2][i];
                                if (levels[i] > maxHeight)
                                {
                                    exceedsMax = true;
                                    break;
                                }
                            }
                            if (exceedsMax) break;
                        }
                        
                        if (!exceedsMax)
                        {
                            queue.Enqueue((nextCounts, presses + 1));
                        }
                    }
                }
            }
            
            return patterns;
        }

        private static int FindPatternCombination(
            List<(int presses, int heightChange, int[] buttonCounts)> patterns,
            int targetHeight)
        {
            if (targetHeight == 0) return 0;
            
            var dp = new Dictionary<int, int> { [0] = 0 };
            var queue = new PriorityQueue<int, int>();
            queue.Enqueue(0, 0);
            
            while (queue.Count > 0)
            {
                int currentHeight = queue.Dequeue();
                int currentPresses = dp[currentHeight];
                
                if (currentHeight == targetHeight)
                {
                    return currentPresses;
                }
                
                foreach (var (presses, heightChange, _) in patterns)
                {
                    int nextHeight = currentHeight + heightChange;
                    
                    if (nextHeight > targetHeight) continue;
                    if (nextHeight < 0) continue;
                    
                    int nextPresses = currentPresses + presses;
                    
                    if (!dp.TryGetValue(nextHeight, out var existingPresses) || nextPresses < existingPresses)
                    {
                        dp[nextHeight] = nextPresses;
                        queue.Enqueue(nextHeight, nextPresses);
                    }
                }
            }
            
            return -1;
        }

        private static int SolveWithButtonCounts(int[][] wiringMatrix, int[] target)
        {
            int numButtons = wiringMatrix.Length;
            int targetLen = target.Length;
            
            var pq = new PriorityQueue<(int[] buttonCounts, int totalPresses), int>();
            var visited = new HashSet<long>();
            
            var start = new int[numButtons];
            var startHash = ComputeButtonHash(start);
            
            pq.Enqueue((start, 0), 0);
            visited.Add(startHash);
            
            while (pq.Count > 0 && visited.Count < 10000000)
            {
                var (buttonCounts, totalPresses) = pq.Dequeue();
                
                var currentLevels = new int[targetLen];
                for (int w = 0; w < numButtons; w++)
                {
                    for (int i = 0; i < targetLen; i++)
                    {
                        currentLevels[i] += buttonCounts[w] * wiringMatrix[w][i];
                    }
                }
                
                bool isTarget = true;
                for (int i = 0; i < targetLen; i++)
                {
                    if (currentLevels[i] != target[i])
                    {
                        isTarget = false;
                        break;
                    }
                }
                if (isTarget) return totalPresses;
                
                for (int w = 0; w < numButtons; w++)
                {
                    bool valid = true;
                    for (int i = 0; i < targetLen; i++)
                    {
                        if (currentLevels[i] + wiringMatrix[w][i] > target[i])
                        {
                            valid = false;
                            break;
                        }
                    }
                    
                    if (!valid) continue;
                    
                    var nextCounts = new int[numButtons];
                    for (int b = 0; b < numButtons; b++)
                    {
                        nextCounts[b] = buttonCounts[b];
                    }
                    nextCounts[w]++;
                    
                    var nextHash = ComputeButtonHash(nextCounts);
                    
                    if (visited.Add(nextHash))
                    {
                        int h = 0;
                        for (int i = 0; i < targetLen; i++)
                        {
                            int newLevel = currentLevels[i] + wiringMatrix[w][i];
                            h += target[i] - newLevel;
                        }
                        
                    pq.Enqueue((nextCounts, totalPresses + 1), totalPresses + 1 + (h / 10));
                    }
                }
            }
            
            Console.WriteLine($"Failed after exploring {visited.Count} button combination states");
            return -1;
        }

        private static long ComputeButtonHash(int[] buttonCounts)
        {
            long hash = 17;
            for (int i = 0; i < buttonCounts.Length; i++)
            {
                hash = hash * 31 + buttonCounts[i];
            }
            return hash;
        }

        private static long ComputeStateHash(int[] state)
        {
            long hash = 17;
            for (int i = 0; i < state.Length; i++)
            {
                hash = hash * 31 + state[i];
            }
            return hash;
        }
    }

    private static string Part1(IEnumerable<string> input)
    {
        var result = 0;
        foreach (var line in input)
        {
            result += Machine.Parse(line).FewestStepsToLight();
        }
        return result.ToString();
    }
    
    private static string Part2(IEnumerable<string> input)
    {
        var result = 0;
        foreach (var line in input)
        {
            var presses = Machine.Parse(line).FewestPressesToJoltage();
            Console.WriteLine($"Presses: {presses}");
            result += presses;
        }
        return result.ToString();
    }
    
    string example = """
        [.##.] (3) (1,3) (2) (2,3) (0,2) (0,1) {3,5,4,7}
        [...#.] (0,2,3,4) (2,3) (0,4) (0,1,2) (1,2,3,4) {7,5,12,7,2}
        [.###.#] (0,1,2,3,4) (0,3,4) (0,1,2,4,5) (1,2) {10,11,11,5,10,5}
        """;

    [TestMethod]
    public void Day10_Part1_Example01()
    {
        var result = Part1(Common.GetLines(example));
        Assert.AreEqual("7", result);
    }
    
    [TestMethod]
    public void Day10_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day10), "2025"));
        Assert.AreEqual("477", result);
    }
    
    [TestMethod]
    public void Day10_Part2_Example01()
    {
        var profiler = new Profiler();
        profiler.Start();
        var result = Part2(Common.GetLines(example));
        profiler.Stop();
        Assert.AreEqual("33", result);
        profiler.Print();
    }
    
    [TestMethod]
    public void Day10_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day10), "2025"));
        Assert.IsGreaterThan(4576, int.Parse(result));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day10_Part2_RandomLine()
    {
        var allLines = Common.DayInput(nameof(Day10), "2025").ToList();
        var random = new Random(42); // Fixed seed for reproducibility
        var randomIndex = random.Next(allLines.Count);
        
        Console.WriteLine($"Testing line {randomIndex + 1} of {allLines.Count}");
        Console.WriteLine($"Input: {allLines[randomIndex]}");
        
        var profiler = new Profiler();
        profiler.Start();
        var presses = Machine.Parse(allLines[randomIndex]).FewestPressesToJoltage();
        profiler.Stop();
        
        Console.WriteLine($"Presses: {presses}");
        profiler.Print();
        
        Assert.IsGreaterThanOrEqualTo(0, presses, $"Line {randomIndex + 1} returned {presses}");
    }

    [TestMethod]
    public void Day10_Part2_FailingCases()
    {
        // Cases that failed in the full run - let's test them individually
        var allLines = Common.DayInput(nameof(Day10), "2025").ToList();
        
        // First failing case (line 0): {36,63,29,56,28,48,43,52,23}
        Console.WriteLine("\n=== Testing Line 0 ===");
        var result0 = Machine.Parse(allLines[0]).FewestPressesToJoltage();
        Console.WriteLine($"Line 0 result: {result0}");
        
        // Second failing case (line 1): {171,166,199,75,30,65,170,64}
        Console.WriteLine("\n=== Testing Line 1 ===");
        var result1 = Machine.Parse(allLines[1]).FewestPressesToJoltage();
        Console.WriteLine($"Line 1 result: {result1}");
        
        // Line that found equal state but no patterns (line 4): {46,44,15,39,72,45,31}
        Console.WriteLine("\n=== Testing Line 4 ===");
        var result4 = Machine.Parse(allLines[4]).FewestPressesToJoltage();
        Console.WriteLine($"Line 4 result: {result4}");
        
        // Line that found equal state and patterns but pattern combo failed (line 5): {23,40,20,64}
        Console.WriteLine("\n=== Testing Line 5 ===");
        var result5 = Machine.Parse(allLines[5]).FewestPressesToJoltage();
        Console.WriteLine($"Line 5 result: {result5}");
    }

    [TestMethod]
    public void Day10_Part2_AnalyzePatternFailures()
    {
        // Test cases where patterns were found but couldn't reach the target height
        var testCases = new[]
        {
            ("[.#..###] (2,5,6) (0,1,2,3,4,5) (0,2,3,5,6) (1,4,5,6) (1,2,4,6) {19,33,40,19,33,25,31}", 7), // Found 0 patterns
            ("[#.##] (0,1) (1,3) (1,2) (0,2) (0,1,3) (2,3) {35,37,27,16}", 0), // Small case
        };
        
        foreach (var (input, expectedHeight) in testCases)
        {
            Console.WriteLine($"\nTesting: {input}");
            var result = Machine.Parse(input).FewestPressesToJoltage();
            Console.WriteLine($"Result: {result}");
        }
    }

    [TestMethod]
    public void Day10_Part2_DirectSearchComparison()
    {
        // Compare divide-and-conquer vs direct search on a few cases
        var allLines = Common.DayInput(nameof(Day10), "2025").ToList();
        
        Console.WriteLine("Testing if direct search alone would work better for failing cases");
        
        // Test line 6 which succeeded with direct search fallback: result 206
        Console.WriteLine("\n=== Line 6 (succeeded with direct fallback) ===");
        var result6 = Machine.Parse(allLines[6]).FewestPressesToJoltage();
        Console.WriteLine($"Result: {result6}");
        Assert.AreEqual(206, result6, "Line 6 should return 206");
    }

    [TestMethod]
    public void Day10_Part2_BackwardSearchDepth()
    {
        // Test if increasing backward search limits helps
        var allLines = Common.DayInput(nameof(Day10), "2025").ToList();
        
        // Line 0 failed with "No equal state found after exploring 1000001 states"
        Console.WriteLine("Testing line 0 with current limits:");
        Console.WriteLine($"Input: {allLines[0]}");
        var result = Machine.Parse(allLines[0]).FewestPressesToJoltage();
        Console.WriteLine($"Result: {result}");
        
        // This test documents that backward search needs higher limits or different strategy
        Assert.AreNotEqual(-1, result, "Should find a solution (may need algorithm improvements)");
    }

    [TestMethod]
    public void Day10_Part2_SuccessfulCases()
    {
        // Document what works to understand the pattern
        var successfulCases = new (string input, int expected)[]
        {
            ("[#.#.] (0,2) (1,3) {3,14,3,14}", 17), // Line 2: worked
            ("[#.#.] (0,3) (3) (2,3) (0,1) (1,2,3) (2) {16,25,49,48}", 26), // Line 3: worked
            ("..#.#] (2,5) (0,1) (0,3,4) (3,5) (4) (1,4) (0,1,2,3) (0,1,2,5) {43,48,41,28,198,42}", 32), // Line 8: worked
        };
        
        foreach (var (input, expected) in successfulCases)
        {
            Console.WriteLine($"\nTesting successful case: {input.Substring(0, Math.Min(50, input.Length))}...");
            var result = Machine.Parse(input).FewestPressesToJoltage();
            Console.WriteLine($"Expected: {expected}, Got: {result}");
            Assert.AreEqual(expected, result);
        }
    }
}
