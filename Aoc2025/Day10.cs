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
            
            // Search BACKWARD from target, "un-pressing" buttons
            var pq = new PriorityQueue<(int[] state, int totalPresses), int>();
            var visited = new HashSet<long>();
            
            var targetHash = ComputeStateHash(target);
            pq.Enqueue((target, 0), 0);
            visited.Add(targetHash);
            
            // Check if target is all zeros (already at goal)
            bool isZero = true;
            for (int i = 0; i < targetLen; i++)
            {
                if (target[i] != 0)
                {
                    isZero = false;
                    break;
                }
            }
            if (isZero) return 0;
            
            while (pq.Count > 0 && visited.Count < 10000000)
            {
                var (state, totalPresses) = pq.Dequeue();
                
                // Check if we reached all zeros
                bool reachedZero = true;
                for (int i = 0; i < targetLen; i++)
                {
                    if (state[i] != 0)
                    {
                        reachedZero = false;
                        break;
                    }
                }
                if (reachedZero) return totalPresses;
                
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
                        // Same heuristic as SearchBackwardToEqualState: minimize variance
                        var variance = 0.0;
                        var avg = prevState.Sum() / (double)targetLen;
                        for (int i = 0; i < targetLen; i++)
                        {
                            var diff = prevState[i] - avg;
                            variance += diff * diff;
                        }
                        
                        int priority = (int)Math.Round(totalPresses + 1.0 + variance);
                        pq.Enqueue((prevState, totalPresses + 1), priority);
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

    [TestMethod]
    [DataRow("[.##......] (0,1,3,4,6,7,8) (1,2,3,5,6,8) (0,1) (3,5,6,7) (2,5,7) (1,2,3,4,5,7,8) (7) (0,1,3) (0,3,7) (1,4,6) {36,63,29,56,28,48,43,52,23}")]
    [DataRow("[.##....#] (0,1,2,3,4,5,7) (0,2,3,7) (1,3,5,6) (0,1,2,6) (2,3,5,6,7) (2,4) (0,1,2,3,7) (0,2,3,5,6,7) {171,166,199,75,30,65,170,64}")]
    [DataRow("[.#.#.] (0,2,3,4) (1,3) (0,2) (1,4) {1,27,1,7,20}")]
    [DataRow("[#...#] (1,4) (2,3) (0,2,3) (1,2,3) (0,1,4) {7,22,11,11,15}")]
    [DataRow("[##.#.#.] (1,3,4,6) (4,5) (0,1,3,4) (0,4,6) (1,2,3,4) (1,2,4,5) (0,1,4,5,6) (0,3) (0,2,4,5) {46,44,15,39,72,45,31}")]
    [DataRow("[.##.] (1,3) (1) (0,3) (0,1,3) (1,2,3) (3) {23,40,20,64}")]
    [DataRow("[...#] (0,2) (0,1,2) (3) (2,3) (1,2) {27,177,206,19}")]
    [DataRow("[.###.] (2,3,4) (0,3,4) (0,4) (3) (0,2,3) (0,1,3) {24,3,19,41,20}")]
    [DataRow("[##.#] (0,1,3) (0,1,2) (2,3) {19,19,14,31}")]
    [DataRow("[#..#] (0,3) (1,2,3) (0,1,3) {139,13,6,145}")]
    [DataRow("[.#..##] (0,2,3) (1,4) (1,2,4) (5) (0,1,2,4,5) (4,5) (2) (2,3) {34,30,58,25,36,30}")]
    [DataRow("[##..#] (0,2,3) (2) (0,1,2,4) {25,5,45,20,5}")]
    [DataRow("[..#..] (0,3) (0,1) (3) (1,2,4) (0,2) (0,2,4) (1,2,3,4) {35,10,22,23,20}")]
    [DataRow("[#.#..###..] (3) (3,5,6) (0,1,2,3,4,9) (0,4,5,6) (5,6) (0,1,2,3,5,6,7,9) (1,2,3,4,5,7,8,9) (1,2,3,6,7,8,9) (9) (0,6) (2,3,4,5,6,7,9) (1,2,3,4,5,6,9) (8) {28,40,43,64,44,35,39,5,21,58}")]
    [DataRow("[.....#.#.] (3,4) (1,4) (0,2,3,5,6,8) (1,4,5,7) (0,3,4,6,7,8) (0,1,2,3,5,8) (2,4,5) (2,5) (2,3,5,8) (6) {24,29,22,43,58,28,19,16,24}")]
    [DataRow("[#.#.] (0,2) (1,3) {3,14,3,14}")]
    [DataRow("[#.#...] (1) (0,2,3,4) (1,2,3,5) (3,4) (0,2) (3) (0,1,2,3,5) {31,28,36,57,23,17}")]
    [DataRow("[#.#...#] (0,2,4,5) (0,1,2,3,4,6) (0,6) (2,3,4,5,6) (0,2,6) (0,3,4,6) {42,11,33,34,42,21,47}")]
    [DataRow("[.###...#.#] (1,2,3,4,5,7,8) (0,1,2,4,5,6,7,9) (3,5) (0,1,2,3,4,6,7,8,9) (1,2,9) (0,1,2,4,6,7,9) (7) (0,2,3,4,5,6,8,9) {59,58,77,52,67,61,59,60,34,69}")]
    [DataRow("[##......#.] (1,3,5,6,7,8) (1,2,6,7) (0,6) (0,3,4,5,6,8) (4,9) (0,1,2,3,4,5,6,7,8) (0,3,4,6,8,9) (1,4,6,7) (0,4) (1,2,4,5,8) (1,2,5) {38,41,30,18,50,30,44,27,31,4}")]
    [DataRow("[.####.#.] (0,4,6,7) (1,2,3) (1,2,3,4,6) (1,5,7) (4,5) (0,1,2,4,5,6) {21,165,163,162,40,4,39,22}")]
    [DataRow("[.###] (0,3) (1,2) (0,1) (2) {5,5,1,1}")]
    [DataRow("[...#.#..#] (0,3,4,5,6,7,8) (1,4,6,8) (1,6) (1,2,3,4,5,6,8) (1,2,3,4,5,7,8) (2,3,4,5,8) (0,1,3,7) (0,1,4,5,6) {27,78,35,46,74,54,64,20,58}")]
    [DataRow("[####..#] (2,4) (3,4,5,6) (2,3,5,6) (0,1,3) (1,2,4,5,6) (0,1,3,5) (0,2,5,6) (1,2,3,4) {33,56,57,48,53,46,36}")]
    [DataRow("[.##.#] (0,2,3) (0,3,4) (0,1,2,3) (2,4) (0,1) {132,20,31,121,118}")]
    [DataRow("[#.#.#.#..#] (1,2) (5,7,8) (0,2,3,5,9) (3,4,5,7,8) (1,2,3,7,9) (0,2,9) (2,4,5) (0,2,3,4,5,6,7,9) (0,1,4,6,7,8,9) (1,3,4,6) (0,1,2,4,5,7,8,9) (0,2,3,4,6,7,9) {185,26,193,179,185,183,154,188,37,194}")]
    [DataRow("[...#..] (2,3,4,5) (2,3) (0,1,3,4) (1,2,5) (5) (0,1) {23,42,42,29,15,33}")]
    [DataRow("[...#.#] (2,5) (0,1) (0,3,4) (3,5) (4) (1,4) (0,1,2,3) (0,1,2,5) {43,48,41,28,198,42}")]
    [DataRow("[#####.#] (0,1,6) (0,3,4,5,6) (0,1,5) (1,4,6) (0,2,4,5) (0,1,2,3) (1,2,3,6) (0,1,4,5,6) (0,2,3,4) {96,62,44,46,75,71,60}")]
    [DataRow("[.#..###..] (0,2,3,5,8) (2,3,5,6) (6,7) (0,3,4,8) (1,2,3,4,8) (4,5,8) (1,2,4,5,6,7,8) (2,3,4,5,6,7,8) (1,2,4,5,8) (0,6) (0,2,7) {41,46,237,193,211,214,195,187,219}")]
    [DataRow("[#..#.##.#.] (5,6) (8) (1,3) (0,1,3,5,6,8,9) (6,9) (0,2,4,5,6,7,8,9) (1,2,3,6,8) (2,3,4,5,6,8,9) (1,2,4,5,7) {21,47,49,49,42,64,61,24,56,45}")]
    [DataRow("[#.##....#] (0,1,4,5,6) (0,2,4,5,6) (1,2,3,4,5,6) (0,1,2,5,7,8) (3,6,7) (0,2,4,5) (1,3,5,6,7,8) (0,4,6,7) (2,4,8) (2,5,6,7,8) {37,37,58,24,51,69,68,43,51}")]
    [DataRow("[##..] (1,2) (3) (1,3) (0,3) (0,2,3) (0,1) {140,133,7,34}")]
    [DataRow("[.#..#] (1,2,4) (3,4) (2,3,4) (0,4) {20,19,25,11,50}")]
    [DataRow("[##..####.] (0,2,6,8) (1,2,3,5,6,7) (4,6,7,8) (0,2,3,4,5,6,7,8) (2,4,6,8) (1,2,3,4,5,6,7,8) (3,8) (5) (0,1,6,8) (2,4,7,8) {15,25,37,25,48,22,58,43,63}")]
    [DataRow("[.#.#####] (0,1,3,4,6) (2,3,5,6,7) (0,5,6,7) (4,7) (0,2,3,4,6,7) (0,1,2,3,5,6,7) (0,1,5) (0,2,6,7) {48,36,47,56,33,43,60,61}")]
    [DataRow("[##..#.#] (0,1,2,4,6) (2,6) (2,4,6) (0,1,5) (1,3,4,5) (0,2,3,4,6) (1,3,4,5,6) (2,3,5) (3,4) {44,46,150,42,44,44,151}")]
    [DataRow("[##.#.] (1,3,4) (0,2,4) (0,2,3) (0,1,4) (0,1,3) (1,3) {41,36,30,44,33}")]
    [DataRow("[..##.#....] (0,1,2,4,5,7,8,9) (0,3,4,8) (0,1,2,3,4,5,8,9) (2,7,8,9) (0,1,2,3,5,8,9) (0,2,4,5,7,8,9) (3,4,8) (0,1,2,3,5,6) (1,6) (0,1,3,4,9) (0,2,4,7) {265,241,259,62,261,233,13,231,262,264}")]
    [DataRow("[#.....#.] (3,4,5,7) (2,4,6,7) (0,1,4,5,6,7) (1,5,6) (4,6) (0,5,7) (0,3) (1,3,5,7) (0,3,4,5,6,7) {59,31,5,34,49,65,68,51}")]
    [DataRow("[##.#] (0,1) (0,1,3) (1,2) (1,3) {28,52,16,20}")]
    [DataRow("[.#.#] (0,3) (3) (2,3) (0,1) (1,2,3) (2) {16,25,49,48}")]
    [DataRow("[.#......#] (0,3,4,5,6,7,8) (2,4,5) (0,1,2,4,6,7,8) (0,2,3,4,5,6,8) (1,2,4,8) (3) (0,1,5) (0,1,2,5,6,7) (3,5,6) (1,8) {49,64,37,31,34,51,39,29,43}")]
    [DataRow("[..#.#..] (2,4) (2,5,6) (4,5,6) (2,3,4,6) (0,1,3,4,5,6) (0,3,4) (1,2,4,5,6) (0,2,3,4) (0,2,3,4,5,6) {34,7,54,50,73,26,42}")]
    [DataRow("[#...#] (1,2,3,4) (0,1,3) (0,1,2,3) {35,43,25,43,8}")]
    [DataRow("[..#.#..#..] (0,2,4,6) (2,4) (5,6,7,9) (0,1,2,3,4,6,7,8) (1,2,3,8,9) (0,1,4,5,6,8,9) (3,4,7,9) (0,2,5,7,8,9) (3,4,5,6,7,8) (2,3,5,6,7,8,9) (1,2,3,4,5,6,7,9) (4,9) {46,48,71,47,69,77,71,62,66,95}")]
    [DataRow("[.#.#.###.] (0,6) (0,2) (5,8) (4,7,8) (1,2,3,5,8) (0,2,7) (1,3,5,6,7) {41,28,44,28,5,48,25,20,45}")]
    [DataRow("[#.###.##] (1,2,3,5,6,7) (2,4,6,7) (5,6,7) (0,1,2,4,5,6) (0,2,3,4,6,7) (3,4,5,6) (1,2,3,4) {12,28,55,47,62,27,54,38}")]
    [DataRow("[...#.#####] (0,4,5) (0,1,8,9) (6,8,9) (0,1,3,4,5,6,7,8) (0,3,7) (0,5,7) (3) (4,6,9) (5,7) (1,2,5,9) (0,1,2,5,6,7,8,9) (3,4,5,6,8) (0,1,2,3,4,5,6,7,8) {43,42,29,38,32,67,33,46,36,30}")]
    [DataRow("[..#.#####] (2,3,5,6,7) (3,7) (0,5,7,8) (0,1,2,4,5,6,7) (0,1,3,4,5,6,7,8) (0,1,2,3,4,5,7,8) (0,2,3,4,5,6,7) {58,23,30,51,38,59,30,76,38}")]
    [DataRow("[.##.....#.] (0,3,5) (3,6) (1,3,4,5,6,7,9) (0,1,2,3,4,6,7,8) (0,2,4,5,6,7,8) (0,2,4,5,7,8) (1,2,3,5,6,7,9) (0,3,9) (0,5,6) (0,1,2,3,6,8,9) {81,59,50,114,49,58,87,57,42,61}")]
    [DataRow("[##....] (0,1,2,3,5) (0,1,2,4) (2,4) (1,3) (0,1,3) {26,42,18,40,7,11}")]
    [DataRow("[.##.#...#.] (4,5,9) (0,1,4,7,8) (3,5,9) (0,9) (5,6) (0,1,2,4,5,7,8,9) (3,5) (1,2,6,9) (1,3,4,5,6,8,9) {29,48,23,23,46,51,30,25,30,62}")]
    [DataRow("[.#..#.#] (2,6) (2,3,5,6) (0,1,2,3,4,5) (0,1,6) (0,5) (0,3,4,5,6) (1,3,4) {48,51,46,56,47,41,56}")]
    [DataRow("[.#..#] (0,4) (1,3,4) (0,2,3,4) (1,4) (0,2,4) (2,3,4) (0,2,3) {39,20,30,33,45}")]
    [DataRow("[#.##.] (2,4) (1,2,4) (0,2,3) {111,5,134,111,23}")]
    [DataRow("[.##.] (0,1,2) (1,2) (0,1,3) {11,14,13,1}")]
    [DataRow("[..#.#.] (0,4) (0,2,5) (1,3) (0,1,3,4,5) (0,1,2,3,4) {61,36,39,36,41,22}")]
    [DataRow("[.##.] (0,1,2) (0) (2) (0,2) (2,3) (3) {32,15,160,136}")]
    [DataRow("[##.###] (1,3) (0,4,5) (1,2,4) (1,2) (2) (0,2,3,4) (0,1,3,5) {23,37,41,11,36,19}")]
    [DataRow("[.##.] (2,3) (0,1,3) (0,2,3) (0,1) {9,3,14,16}")]
    [DataRow("[...##.####] (0,1,3,5,6) (2,6,7,8) (1,2,4,5,6,7,8,9) (0,1,5,7,8,9) (0,3,4,7,8,9) (1,4,8) (0,2,3,6,7,9) (1,4,5,7,8,9) (0,2,5,8) (1,6,8,9) (0,1,2,4,6,7) (0,1,2,3,4,5,6,7) {72,112,79,37,80,63,96,108,88,75}")]
    [DataRow("[#.##..#] (0,3) (0,6) (2,4,5) (0,3,4,5,6) (0,2,4,5,6) (2,4) (0,4) (0,1,2,3,4,5) {36,10,20,24,29,22,9}")]
    [DataRow("[..##.###.#] (5,6,7,8) (1,4,7) (0,5,9) (7,9) (1,2,3,4,5,6,7,8,9) (1,2,3,6,8) (3,7,8) (4,6,9) (1,2,3,6,9) {11,26,21,23,37,27,43,27,19,54}")]
    [DataRow("[...####..#] (1,2,5,7,8) (0,2,6,7) (1,2,8,9) (0,1,3,4,5,9) (0,2,4,5,6,7,8) (7) (1,4,7) (1,2,3,5,7,8) (0) (1,6,9) (1,2,3,4,5,6,7,8) (0,4,5,7,8,9) (0,1,3,4,5,6) {49,58,68,37,50,67,60,92,62,24}")]
    [DataRow("[#####..#] (0,3,7) (0,1,2,3,4,5,7) (0,2,3,4,6) (0,2,4,5) (0,4) (1,2,4,5,6,7) (0,1,3,5,6) (0,2,5,6,7) {82,36,60,55,63,53,47,52}")]
    [DataRow("[##.###..#] (2,4,8) (2,3) (0,4,5,8) (0,1,2,3,5) (1,2,4,7) (6,7,8) (0,2,4,6,8) (0,1,2,4,5,6,8) (0,1,3,4,5,6,7) (0,3,7,8) {55,42,60,14,66,35,50,40,64}")]
    [DataRow("[###..#####] (1,2,4,5,6,7,8) (0,1,2,5,6,7,8,9) (2,4) (0,4,6,8) (2,3,4,6,9) (1,3,4,5,6,7,8,9) (0,1,2,3,6,7,9) (2,4,5,6,7) {23,34,192,173,193,28,192,41,26,178}")]
    [DataRow("[.#..#] (1,4) (2,3) (0) (1) (1,2,3) {14,42,18,18,12}")]
    [DataRow("[.##..] (0,3,4) (0,2) (1,3,4) (1,3) (1,4) (0,1,2,3) (1,2,3,4) {29,44,36,45,47}")]
    [DataRow("[#....#..##] (3,5,9) (2,3) (1,2,3,4,5,7,8,9) (4,5,6) (0,1,2,4,6,8,9) (0,1,2,3,4,7,9) (3,6) (0,1,2,3,4,8) {28,44,64,76,146,135,137,17,43,54}")]
    [DataRow("[..###.#..] (0,2,5,7) (0,1,3,5,6) (4) (1,8) (0,1,4,6,7,8) (0,2,3,6) (0,1,2,6,7,8) (0,1,2,5,6,7) (1,5,7,8) {36,67,25,6,25,27,31,48,63}")]
    [DataRow("[##.....] (1,3,5,6) (0,3) (0,5) (3,4) (1,2) (0,2,3,4,6) (0,1,5,6) (1,2,3,6) {13,19,21,39,18,1,16}")]
    [DataRow("[##.####] (0,1,5,6) (0,2,4) (1,3,6) (0,1,2,3,5,6) (1,2,3,4,6) {222,226,215,209,30,202,226}")]
    [DataRow("[.##.#] (3,4) (2,4) (1,2,3,4) (0,2,4) (0,2) {35,12,60,17,50}")]
    [DataRow("[...#..#] (2,3) (0,2,3,5,6) (1,2,4,5,6) (1,4,5) (2,3,4,5) (1,3) {18,204,46,33,202,220,38}")]
    [DataRow("[..##.#.#.#] (1,3,4,6,8) (2,5,7,9) (5,8) (2,4,5,7,8) (1,2,3,4,5,6,7) (0,1,3,4,5,6,7,8,9) (1,5,6) (0,1,8,9) (2,5,6,7,9) {9,42,53,34,44,54,48,54,33,33}")]
    [DataRow("[###..##.] (2,6) (0,1,3,4) (0,1,2,3,4,5,6) (3,4) (2,4,5,6,7) (1,2,4,6) (5,7) (0,1,3) {4,9,19,9,16,14,19,14}")]
    [DataRow("[#.#######.] (2,8,9) (0,5,6) (8) (1,2,4,6,7,8,9) (2,3,4,6,9) (0,1,2,4,6,8) (0,2,3,4,9) (0,1,2,3,5,7,8,9) (0,1,5,6,7,8) (1,3,5,7,8) (0,2,3,6) {35,15,60,42,39,4,31,9,30,44}")]
    [DataRow("[.##..##..] (0,5,6) (1,3,4) (0,1,3,5) (3,5,6,7) (0,1,5,7) (3,4,5,7,8) (1,2,5,6,8) (0,1,2,3,7) (0,2,3) {44,43,22,70,34,68,36,57,25}")]
    [DataRow("[.#...] (2) (1,2,3) (0,1) (3,4) (0,3,4) (1) {1,11,7,25,18}")]
    [DataRow("[..##] (0,3) (1,2,3) (0,2) (0) (1,3) {24,10,8,16}")]
    [DataRow("[.##.] (1,2,3) (1,3) (0,3) (0,1,2) (0) {44,28,16,40}")]
    [DataRow("[..#......] (1,3,4,5,6,7,8) (0,2,3,4,6,7,8) (0,1,5,6,8) (2,3,4,8) (0,1,2,5,6,8) (5,6) (3,4,6,8) (0,1,2,3,4,5,8) (0,2,3,6,8) (0,7) {51,42,37,24,16,52,64,5,63}")]
    [DataRow("[#.#.#] (1,2) (1,3) (0,1,2) (3) (1,4) {16,41,25,20,5}")]
    [DataRow("[.########.] (0,1,3,9) (0,1,2,7) (0,1,2,3,4,6,7,9) (0,2,3,4,5,8,9) (2,7) (0,1,2,3,6,8) (0,1,2,3,4,5,8,9) (0,1,2,5,7) (0,1,3,4,5,6) (1,2,4,5,6,9) (2,4) {79,87,96,54,60,50,48,42,26,44}")]
    [DataRow("[#.#.] (0,3) (1,2,3) (2,3) (2) {11,18,47,48}")]
    [DataRow("[#...] (1,2) (1,3) (2) (0,2,3) (0,3) (0,1,2) {36,27,39,36}")]
    [DataRow("[###.#.#.##] (0,1,2,4,6,8,9) (1,2,3,4,5,6,7,8,9) (0,1,3,5,6,8) (0,1,2,3,4,5,7) (0,1,2,3,4,6,7,8,9) (1,2,3,4,5,6,8) (0,1,2,5,6) (6,7,9) (4,5) (0,1,3,4,5,6,7,8,9) {71,81,42,58,57,91,252,217,54,213}")]
    [DataRow("[#.#####.] (0,2,3,4,5,7) (0,2,3,4,5,6) (3,5,6,7) (2,4,5) (2,3) (1,7) (0,2,3,4,5,6,7) {44,19,64,59,54,59,30,51}")]
    [DataRow("[###..] (0,2,4) (0,1,2,3) (1,4) {25,135,25,13,134}")]
    [DataRow("[#..###.#..] (3,4,6) (2,5,9) (8) (0,3,4,5,6,7,8) (0,1,2,4,6,8) (0,2,8) (0,2,3,5,6,7) (1,2,5,7,8) (2,3,4,5,6,7,8,9) (0,5,6,7,8) (1,2,3,4,5,6,7) (1,3,4,5,7,9) {20,27,175,49,47,190,41,40,39,171}")]
    [DataRow("[..#..##..] (0,3,4,5,6,8) (4,5,6,8) (3,6,8) (5,8) (0,1,2,4,5,6,8) (0,1,2,7) (0,2,3,4,7,8) {39,19,28,28,47,40,46,9,57}")]
    [DataRow("[#####] (0,1,3) (0) (0,1,2,3) (2,4) (3,4) (2) (0,1,3,4) {247,47,35,62,32}")]
    [DataRow("[..##..##] (0,1,3,4,5,6) (0,3,6) (2,5,6) (1,2,4) (1,2,3,4,5,7) (0,2,3,4,6) {21,19,30,23,25,27,36,2}")]
    [DataRow("[#...#.####] (1,2,3,7,8,9) (2,4,6,7,8) (0,1,2,3,4,7,8,9) (0,1,5,7,9) (0) (2,9) (1,2,5,8) (0,3,4,6,8) (0,1,2,3,5,6,7,9) (4,7) {56,47,58,52,41,25,32,47,53,47}")]
    [DataRow("[#...##] (1,2,4,5) (1,2,4) (1,2,3,5) (0,4) {1,21,21,14,8,16}")]
    [DataRow("[...#] (0,2) (1,2) (1,3) (0,1,2) (3) {21,19,33,19}")]
    [DataRow("[.#.#..] (2,5) (0,2,3,5) (1,2,3,5) (3,4,5) (0,1,4,5) {13,14,40,27,6,46}")]
    [DataRow("[#.#.#.###] (0,3,4,7) (2,3,5,7,8) (0,1,3,7,8) (0,5,7) (2) (2,5,6,7,8) (0,1,4,7) (3,4) {29,11,180,33,22,44,17,60,35}")]
    [DataRow("[##.######] (0,1,3,4,5,6,7,8) (0,3) (1,2,3,4,5,6,8) (0,3,5,6,7) (0,2,3,4,5,6,7) (0,1,2,4,5,7) (2,3,4) {58,27,40,56,46,50,37,42,14}")]
    [DataRow("[.#...] (0,1,3) (1) (0,4) (0,2,3) (1,2) (0,1,2) {13,33,18,9,3}")]
    [DataRow("[.#..###] (2,5,6) (0,1,2,3,4,5) (0,2,3,5,6) (1,4,5,6) (1,2,4,6) {19,33,40,19,33,25,31}")]
    [DataRow("[.#..#####.] (1,4,5,6,7,8) (1,3,5,9) (0,1,2,3,6,7,8,9) (2,5,6) (0,1,2,3,6,8) (3,7,9) (2,4,6,8,9) (1,3,6,7,8) {26,75,40,62,27,39,75,57,69,44}")]
    [DataRow("[##.#..] (2,3,4,5) (1,2,3) (3,4) (1,3) (0,5) {176,21,36,47,26,195}")]
    [DataRow("[##...#] (0,1,2,4,5) (1,4) (0,2,3,5) (0,1,2,3) (1,3,4,5) (0,3,4,5) {31,34,25,36,31,27}")]
    [DataRow("[#..##.#] (0,1,3,5,6) (1,2,5,6) (1,3,4,5,6) (0,1,2,3,4,5) (0,1,3,5) {27,56,30,45,37,56,30}")]
    [DataRow("[.##...##] (1,3,4,5,6,7) (1,2,6,7) (0,3,6,7) (0,4,5,6) (1,3,4,6,7) (1,2,5,6) {35,225,212,29,32,37,260,224}")]
    [DataRow("[...##.#.] (3,4,6) (3,4,5,6) (1,2,4,6) (0,1,2,7) (0,1,3,4,6,7) (0,1,3,6) {31,43,20,54,54,12,66,19}")]
    [DataRow("[....#.#..#] (4,7,9) (2,8) (0,2,3,4,6,7,9) (4,5,6,7,8,9) (0,1,2,3,4,5,8) (0,1,2,3,5,7,9) (2,6,8,9) (2,3,4,8) (1,8) (0,1,2,4,5,6,7,8) {7,20,36,14,34,15,33,23,60,34}")]
    [DataRow("[#.##...] (1,4) (0,1,3,4,6) (0,4) (3,4,6) (1,2,4,6) (1,2,6) (0,5) {46,39,13,15,56,19,28}")]
    [DataRow("[#.#.##.] (5,6) (4,6) (0,2,3,4,5,6) (4,5) (0,1,5) (0,1,2,4,6) (2,3,5) (1,2,3) (1,5,6) {32,40,29,14,39,61,55}")]
    [DataRow("[####..#..] (4,8) (1) (6,7) (0,2,5,6,7,8) (0,1,2,4,6,8) (1,2,8) (0,1,3,5,7) (0,1,2,4) (0,4,5,7,8) (2,3,5,6) (0,5) {61,56,61,33,31,64,38,32,49}")]
    [DataRow("[###.] (1,2,3) (0,1,3) (1,3) (0,2,3) (0,3) (0,2) {69,20,34,51}")]
    [DataRow("[..#...] (1,2) (5) (1,2,3,5) (0,2,4,5) (0,1,2,4) (0,3,5) (2) {38,40,55,27,22,45}")]
    [DataRow("[#.#.#..] (1,2,3,4,6) (0,1,4,5,6) (0,1,2,4,5) (2,4,5) (2,3) (3,4,6) (2,3,4,5) (0,1,2,3,4,5) (5,6) {19,24,49,36,44,37,22}")]
    [DataRow("[..###..] (1,3,6) (0,4,5) (4,5,6) (0,3,5,6) (0,4,6) (0,2,4) (1,2,3,4,5,6) {42,36,25,53,49,43,75}")]
    [DataRow("[.#####.] (1,2,4,6) (0,1,2,3,6) (0,2,3,4,5) (0,4,5,6) (1,3,4,6) {27,26,20,22,41,21,42}")]
    [DataRow("[#######.##] (4,6) (1,3,4) (0,3,4,5,8,9) (1,2,4,5,6,7) (2,3,4,6,7,8,9) (4,8) (0) (0,1,2,3,4,5,9) (0,2,3,4,5,8) (0,1,3,4,5,6,7,8,9) (0,1,4,5,6) {60,40,41,48,86,67,43,19,41,28}")]
    [DataRow("[..#..#..] (1,2,4,5,6) (6) (0,3) (0,3,4,7) (0,2,4,5,6,7) (1,7) (0,1,2,4,5,6) {31,17,23,13,33,23,32,20}")]
    [DataRow("[.##..#] (1,2,4,5) (1,2) (2,3,4,5) (1,4) (0,3) (5) {20,29,17,26,33,18}")]
    [DataRow("[.##.#.#] (1,2,4,6) (0,1,3,4) (0,6) (0,1,2,3,5) (1,4,5,6) (0,2,3,6) (1,3) (2,4,5) {36,47,49,42,35,36,30}")]
    [DataRow("[.##..##.#] (2,6) (0,3,4,5,6,8) (0,1,4) (3) (0,4,6,8) (1,8) (0,2,4,5,6) (2,3,4,5,6,8) (1,4) (4,5,8) (0,3,6,7,8) {36,46,18,30,64,23,37,0,49}")]
    [DataRow("[......##] (0,1,2,4,5,7) (2,3,7) (1,3,4,5,6,7) (2,3,4,7) (5,7) (1,4,7) (0,7) (0,1,4,7) {30,37,14,8,39,21,4,55}")]
    [DataRow("[.#..###.] (1) (4,6) (0,1,2,3,4,7) (0,1,4,7) (2,3,7) (0,1,5,7) (1,4,5) (0,3,5,6,7) (0,2,6,7) (0,1,2,3,5) {47,38,32,16,26,22,39,44}")]
    [DataRow("[##..] (0,2) (0,1,3) (1,2) {28,38,26,20}")]
    [DataRow("[#..###.] (0,1,2,3,5) (2,3,6) (0,3,4,6) (0,2,4,6) (0,1,6) (5,6) {43,20,39,44,23,19,53}")]
    [DataRow("[.#.##...] (0,4) (0,2,3,4,5,7) (1,3,4) (1,2,3,5,6,7) (0,1,3,4,7) (1,2,4,5,6,7) (3,4,6) (1,2,5,6) (1,3,6,7) (1,2,3,5) {22,81,47,79,52,47,66,50}")]
    [DataRow("[####] (0,1,2,3) (1,3) {12,15,12,15}")]
    [DataRow("[##..##.#] (0,1,2,3,5,6) (1,2,3,4,6,7) (1,2,6) (1,4,5,7) (2,5) (2,4) (0,2,3) (0,1,2,3,4) (6) (6,7) {22,62,78,42,51,26,52,27}")]
    [DataRow("[##..#.###] (3,4,5,6,8) (1,5,7) (0,3,4,5,6,7,8) (0,1,3,5,7) (0,1,2,3,5,6,8) (2,3,4,5,7) (0,1,2,3,5,6,7,8) {45,42,41,73,46,88,51,58,51}")]
    [DataRow("[###.] (1,2,3) (0,1,2) {11,29,29,18}")]
    [DataRow("[#..##.] (0,2,5) (1,4) (1,2,4) (1,2,4,5) (4,5) (1,3) (3,5) (0,1,5) {29,35,30,25,24,59}")]
    [DataRow("[##...#] (2) (1,4,5) (0,2,3,5) (0,1,4) (0,1,2,3,5) {33,31,39,25,25,42}")]
    [DataRow("[###.] (0,3) (3) (0,2) (0,1) (1,3) {17,17,7,18}")]
    [DataRow("[..##..#.] (0,6) (0,2,3,4,5,7) (0,2,3,5,6) (0,4,5) (0,1,2,3,4) (0,1,2,3,6) (5) (3,5) (0,7) {72,23,42,43,38,45,27,8}")]
    [DataRow("[..###.#.#] (0,1,2,3,4,5,6,8) (1,2,7,8) (3,4,6,7) (0,2,4,5,6,7) (0,1,2,4,5,6,7) (2,3,4,6,7,8) (0,1,4) {29,36,52,175,200,19,190,206,37}")]
    [DataRow("[.#.###] (0,4) (0,1,3,5) (2,3) (0,1,3) {31,26,5,31,5,7}")]
    [DataRow("[#.##] (0,1) (1,3) (1,2) (0,2) (0,1,3) (2,3) {35,37,27,16}")]
    [DataRow("[.....###.] (4,5,6) (0,2,6) (0,2,3,4,6,7,8) (0,2,3,4,7,8) (1,6,8) (0,2,5,6,7) (4,6,7) (0,5,6,7) (3,4) (3,7) (1,2,3,5,6,7,8) {62,9,54,42,56,36,85,68,34}")]
    [DataRow("[#....##] (4) (1,2,3,4) (0,1,3,4,5,6) (0,3,4,5) (0,5,6) (2,3,4) (0,1,4,5,6) (3) (0,1,2,4,6) {53,26,21,65,64,44,33}")]
    [DataRow("[..###] (0,1,3) (2,3,4) (0,2,4) (0,1,2) {53,38,39,22,19}")]
    [DataRow("[###...##..] (3,5,7,8,9) (8,9) (3,4,6,8,9) (0,3,7,9) (2,4) (1) (0,1,2,4,6,7,8,9) (0,1,2,6,7) (1,3,4,5,6,8,9) (1,2,4,5,7,9) (0,1,3,4,5,8,9) (2,4,5,7) (1,5,6,9) {34,63,41,50,64,53,33,64,52,88}")]
    [DataRow("[##.##..#.#] (4,6) (2,9) (0,1,2,3,4,5,7,8) (0,1,4,5,6,7) (0,1) (0,1,3,4,5,6,7,8) (2,6,9) (3,4,5,7,8,9) (0,2,3,8) (0,1,2,3,4,6,7) (2,7,9) (0,1,3,6,7,8) (2) {84,67,61,63,75,47,73,79,54,34}")]
    [DataRow("[.#.#..#...] (0,2,3,4,5,6,8,9) (1,2,3,4,6,9) (0,2,3,6,7,8) (0,1,3,6,7,8) (0,2,4,5,9) (1,3,5,6,9) (0,1,3,6,8,9) (1,2,6,7,8) (0,1,4,5,6,7,8) (4,6,7) (4,9) (0,1,2,3,4) {61,72,50,71,51,31,90,44,70,61}")]
    [DataRow("[#.#.#.] (1,3,4) (2,5) (0,1,2,4) (5) (0,1,3,4) (0,1,2,4,5) (2,4) {36,50,34,34,58,140}")]
    [DataRow("[.#.....#.] (4,5,6,7) (5) (0,1,4,5,6,8) (1,2,3,4,6,7,8) (0,2,6,7) (4,7) (1,6,7) (0,1,3,4,5,8) (1,7) {31,43,34,20,56,30,58,71,31}")]
    [DataRow("[####.#.] (4,6) (0,1,2,3,4,5,6) (1,2,6) (2,3,4,5) (0,1,2,3,5) {28,48,54,34,26,34,40}")]
    [DataRow("[...#.#...] (1,8) (0,2,4,5,6,8) (1,3,5,8) (0,2,4,5,7) (0,1,3,4,8) (0,3,5,6,7,8) (0,7,8) (0,6,7,8) {69,38,25,31,30,51,36,58,83}")]
    [DataRow("[.#...###.] (2,3,7) (0,3,4,5,8) (1,2,5) (1,2,3,4,5,7,8) (1,3,4,5,6,7,8) (0,1,2,3,4,5,6,7) (0,1,4,8) (2,3,6,7,8) {36,45,56,58,46,46,19,45,42}")]
    [DataRow("[##.#####] (0,1,2,3,5) (0,1,3,4,7) (1,3,4,5,6,7) (4,5,6) (2,3,6) (2,3,4,5,6,7) (1,4,5) (0,2,3,4,5,6,7) (2,7) {48,181,46,185,198,203,169,172}")]
    [DataRow("[#.#####.#] (2,4,5,6,7,8) (0,2,5) (1,3,4,6,7) (0,1,2,3,5,6,7,8) (0,2,3,4,5,6,8) (1,4,5,6,7,8) (0,1,2,7) (3,8) (0,2,5,6,8) {60,229,73,56,213,231,240,242,234}")]
    [DataRow("[.....#] (0,2,4) (1,2,3,4) (2) (0,1,2,4) (1,2,4,5) (5) (0,2,3,4,5) (3,4) {25,26,71,32,67,28}")]
    [DataRow("[.#.##.####] (0,2,3,4,6,7,8,9) (5,6,7,8) (1,2,3,4,6,7,9) (0,3,7,8,9) (0,2,3,5,7,9) (2,4,7) (0,1,2,3,4,6,7,9) (0,1,3,6,7) (0,3,8) (2,3,6,9) {65,49,53,83,40,22,67,88,32,53}")]
    [DataRow("[.###.] (0,2,3,4) (1,4) (2,3,4) {20,12,22,22,34}")]
    [DataRow("[..#..#] (0,1,2) (0,1,3,4,5) (0,1,2,5) (0,4) (1,3,5) (2,3,4,5) (0,1) {45,50,19,23,18,23}")]
    [DataRow("[.##.##...] (3,7) (0,1,2,3,5,6,7) (1,2,3,4,6,8) (1,2,5,8) (1,3) (0,1,2,5,6,7) (0,1,2,3,4,5,6,8) (0,3,5,7,8) (0,4,5,8) (4,7) {217,237,232,233,38,236,213,202,64}")]
    [DataRow("[###..#.] (0,3,4) (0,1,2,3,4,6) (3,4,5,6) (0,1,3,6) (0,1,5) (0,2,4) (3,4) {61,46,20,73,66,39,47}")]
    [DataRow("[#.#.#...] (0,1,2) (0,1,3,4,5,7) (2,3) (0,2,5,7) (3,4,5,6) (0,1,2,3,6) (2,4,5,6,7) {16,15,163,38,138,139,153,132}")]
    [DataRow("[#.###.] (1,3) (1,3,5) (5) (0,1,5) (0,2,3,4,5) (2,4,5) (2,3,5) (0,1) {23,19,34,32,26,47}")]
    [DataRow("[..####.] (3,4,6) (2,3,4,5) (0,3,5) (3,5) (1,2,4,5) (1,3,4,5) (3,4) (0,6) {18,8,26,63,63,34,32}")]
    [DataRow("[..##.#] (1,2,4) (0,1,2,5) (2,3,5) (0,1,2,4,5) {152,172,184,12,28,164}")]
    [DataRow("[.##....#] (1,2,4,5,6) (1,2,4) (0,1,2,3,5,6,7) (0,1,2,3,4,5) (3,5,6) (0,1,6) (3,5,7) (0,3) (1,2,4,5) {42,68,53,45,35,69,49,28}")]
    [DataRow("[..#.##] (2,4,5) (0,1,4) (1,4,5) (3,5) (1,2,3,4,5) (0,1,3,4) {29,200,26,46,209,199}")]
    [DataRow("[#.#.] (0,2) (1,3) {11,6,11,6}")]
    [DataRow("[.##......#] (1,3,4,6,8,9) (1,2,9) (0,1,3,4,7,8) (2,4,5,6,7,8,9) (0,1,2,3,4,7,8,9) (0,4,9) (6,7,8) (0,1,2,3,5,6,8,9) (0,2,3,5) {74,67,166,73,187,143,149,152,180,189}")]
    [DataRow("[##.####..] (2,4,5,7,8) (5,7) (0,1,3) (0,1,2,3,4,5,6,7) (1,2,4,5,7,8) (0,1,3,4,5,6) (0,1,2) (0,2,5,6,7,8) {54,58,56,44,63,68,40,51,33}")]
    [DataRow("[..#..] (0,1,3) (2,3,4) (1,2) (3,4) (0,4) (1,2,3,4) {21,32,27,29,22}")]
    [DataRow("[#..####] (0,3,5,6) (2,3,5) (1,3,4) (1,2,4) (0,4,5) (2,3,4,5) {9,24,14,28,34,18,0}")]
    [DataRow("[..#.#...#] (0,2,5) (1,3,6) (3,4) (1,2,7) (1,2,3,4,5) (0,2,4,7,8) (4,6) (0,3,4,5,6,7,8) (0,2,3,5,7,8) {41,32,60,52,58,50,26,32,26}")]
    [DataRow("[.##.##.#] (0,3,4,7) (0,7) (1,2,3,4,6) (2,6) (1,2,3,4) (0,1,2,5,6,7) (1,3,5,7) (0,2,3,6) {36,7,10,21,14,5,10,37}")]
    [DataRow("[#####.#.] (0,1,2,3,4,5) (1,2,6) (0,1,2,3,5,7) (3,7) (0,1,2,7) (5,6) (6) (0,3,6) (1,2,4) (0,5) {219,62,62,53,18,203,35,34}")]
    [DataRow("[#.###] (0) (0,1,2,4) (0,2) (1,2,4) (0,3) (0,1,3) (3,4) {200,188,30,185,17}")]
    [DataRow("[#..#..] (1,4) (0,3) (0,2,3,4) (0,2,4) (1,4,5) (0,5) {34,20,21,9,41,19}")]
    [DataRow("[#.#..] (2,4) (3) (0,1,4) (1,2,4) {8,10,12,4,20}")]
    [DataRow("[...#####.] (1,4,5,6,7,8) (1,4) (0,1,2,3,6,7,8) (2,5,6) (0,3,4,5,7,8) (1,2,3,5,7,8) (0,1,2,4) (1,4,8) (0,5,7,8) {26,53,208,23,36,206,218,40,43}")]
    [DataRow("[####.##.#] (0,3,5,6,8) (3) (2,3,5,6,7,8) (0,1,2,3,6,7,8) (1,2,3,8) (0,1,2,8) (0,1,2,3,4,6,8) (0,2,3,4,6) (0,5,6,7,8) (0,2,3,7) (2,3,8) {101,50,99,117,34,40,81,50,99}")]
    [DataRow("[....##] (0,1,2,3) (1,5) (0,3) (0,1,2,3,4) (0,1,4,5) (0,2,3,4,5) (4,5) {217,191,198,217,194,18}")]
    [DataRow("[##.##.] (1,4,5) (0,1,3,4) (0,2) (0,4) {7,8,2,3,10,5}")]
    [DataRow("[#...#...] (0,2,4,7) (0,1,2,4,5,6) (0,1) (3) (0,2,4) (1,2,5,6) (0,1,2,3,5,6) (1,2,3,5,6,7) (2,5) (1,2,3,5) {32,56,65,50,17,56,42,10}")]
    [DataRow("[...#.#..] (0,3,4,5) (2,3,4,5,6,7) (2,4,6,7) (2) (0,1,4,5,7) (0,1,2,4,5,6) {34,21,44,22,56,43,28,37}")]
    [DataRow("[#..#.#....] (0,1,2,3,4,6,7,8) (0,7,8,9) (2,5,6) (3,4,7,9) (0,1,2,3,5,6,7,8,9) (0,2,5,6,9) (3,8) (1,5,7,8,9) {213,24,58,30,26,47,58,209,200,212}")]
    public void Day10_Part2_InputLine(string input)
    {
        Console.WriteLine($"Input: {input}");

        var profiler = new Profiler();
        profiler.Start();
        var presses = Machine.Parse(input).FewestPressesToJoltage();
        profiler.Stop();

        Console.WriteLine($"Presses: {presses}");
        profiler.Print();

        Assert.IsGreaterThanOrEqualTo(0, presses, $"Line {input} returned {presses} (should be >= 0)");
    }

}
