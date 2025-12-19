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
            
            return BFSWithPruning(wiringMatrix, target);
        }

        private static int BFSWithPruning(int[][] wiringMatrix, int[] target)
        {
            int targetLen = target.Length;
            
            // Find minimum target value
            int minTarget = int.MaxValue;
            for (int i = 0; i < targetLen; i++)
            {
                if (target[i] < minTarget)
                    minTarget = target[i];
            }
            
            // Calculate differences from minimum
            var targetDiffsFromMin = new int[targetLen];
            for (int i = 0; i < targetLen; i++)
            {
                targetDiffsFromMin[i] = target[i] - minTarget;
            }
            
            Console.WriteLine($"Min target value: {minTarget}");
            Console.WriteLine($"Target differences from min: [{string.Join(", ", targetDiffsFromMin)}]");
            
            // Step 1: Find a state where differences from minimum match target
            var (matchingState, pressesToMatching) = FindStateWithMatchingDifferencesFromMin(wiringMatrix, targetDiffsFromMin, target);
            
            if (matchingState == null)
            {
                Console.WriteLine("Cannot find state with matching differences from min, using direct search");
                return DirectBFS(wiringMatrix, target);
            }
            
            Console.WriteLine($"Found state with matching differences: [{string.Join(", ", matchingState)}] in {pressesToMatching} presses");
            
            // Step 2: Calculate offset needed
            int minMatching = int.MaxValue;
            for (int i = 0; i < targetLen; i++)
            {
                if (matchingState[i] < minMatching)
                    minMatching = matchingState[i];
            }
            
            int offset = minTarget - minMatching;
            Console.WriteLine($"Min in matching state: {minMatching}, offset needed: {offset}");
            
            // Verify the structure is correct
            bool validStructure = true;
            for (int i = 0; i < targetLen; i++)
            {
                int expectedDiff = targetDiffsFromMin[i];
                int actualDiff = matchingState[i] - minMatching;
                if (actualDiff != expectedDiff)
                {
                    validStructure = false;
                    Console.WriteLine($"Warning: Diff mismatch at index {i}: expected {expectedDiff}, got {actualDiff}");
                }
            }
            
            if (!validStructure)
            {
                Console.WriteLine("Structure doesn't match, searching from matching state to target");
                int remaining = FindPathToState(wiringMatrix, matchingState, target);
                if (remaining < 0)
                {
                    return DirectBFS(wiringMatrix, target);
                }
                return pressesToMatching + remaining;
            }
            
            // Step 3: Add uniform offset to reach target
            int pressesToAdd = AddUniformOffset(wiringMatrix, matchingState, offset, target);
            
            if (pressesToAdd < 0)
            {
                Console.WriteLine("Cannot add uniform offset, searching from matching state");
                int remaining = FindPathToState(wiringMatrix, matchingState, target);
                if (remaining < 0)
                {
                    return DirectBFS(wiringMatrix, target);
                }
                return pressesToMatching + remaining;
            }
            
            Console.WriteLine($"Added offset in {pressesToAdd} presses");
            
            return pressesToMatching + pressesToAdd;
        }

        private static (int[] state, int presses) FindStateWithMatchingDifferencesFromMin(int[][] wiringMatrix, int[] targetDiffsFromMin, int[] target)
        {
            int targetLen = target.Length;
            int numButtons = wiringMatrix.Length;
            
            var visited = new HashSet<long>();
            var queue = new Queue<(int[] state, int depth)>();
            
            var start = new int[targetLen];
            var startHash = ComputeHashFast(start);
            visited.Add(startHash);
            queue.Enqueue((start, 0));
            
            const int maxDepth = 300;
            const int maxStates = 2000000;
            
            while (queue.Count > 0)
            {
                var (state, depth) = queue.Dequeue();
                
                // Find minimum in current state
                int minState = int.MaxValue;
                for (int i = 0; i < targetLen; i++)
                {
                    if (state[i] < minState)
                        minState = state[i];
                }
                
                // Check if differences from minimum match target
                bool diffsMatch = true;
                for (int i = 0; i < targetLen; i++)
                {
                    int stateDiff = state[i] - minState;
                    if (stateDiff != targetDiffsFromMin[i])
                    {
                        diffsMatch = false;
                        break;
                    }
                }
                
                if (diffsMatch)
                {
                    // Found a state with matching structure!
                    return (state, depth);
                }

                if (depth >= maxDepth)
                {
                    continue;
                }
                
                // Try each button
                for (int w = 0; w < numButtons; w++)
                {
                    bool valid = true;
                    
                    // Don't exceed target values
                    for (int i = 0; i < targetLen; i++)
                    {
                        if (state[i] + wiringMatrix[w][i] > target[i])
                        {
                            valid = false;
                            break;
                        }
                    }
                    
                    if (!valid) continue;
                    
                    var next = new int[targetLen];
                    for (int i = 0; i < targetLen; i++)
                    {
                        next[i] = state[i] + wiringMatrix[w][i];
                    }
                    
                    var nextHash = ComputeHashFast(next);
                    
                    if (visited.Add(nextHash))
                    {
                        queue.Enqueue((next, depth + 1));
                    }
                }
                
                // Memory limit
                if (visited.Count > maxStates)
                {
                    Console.WriteLine($"Memory limit in FindStateWithMatchingDifferencesFromMin. Visited: {visited.Count}");
                    return (null, -1);
                }
            }
            
            return (null, -1);
        }

        private static int AddUniformOffset(int[][] wiringMatrix, int[] start, int offset, int[] target)
        {
            if (offset == 0)
                return 0;
            
            if (offset < 0)
                return -1; // Cannot subtract
            
            int targetLen = start.Length;
            int numButtons = wiringMatrix.Length;
            
            // Find button that increments all counters by 1
            int uniformButton = -1;
            for (int w = 0; w < numButtons; w++)
            {
                bool isUniform = true;
                for (int i = 0; i < targetLen; i++)
                {
                    if (wiringMatrix[w][i] != 1)
                    {
                        isUniform = false;
                        break;
                    }
                }
                if (isUniform)
                {
                    uniformButton = w;
                    break;
                }
            }
            
            if (uniformButton >= 0)
            {
                Console.WriteLine($"Found uniform button {uniformButton}, pressing {offset} times");
                return offset;
            }
            
            Console.WriteLine("No uniform button found, need to search for offset");
            
            // Search for a way to add offset
            var current = new int[targetLen];
            for (int i = 0; i < targetLen; i++)
            {
                current[i] = start[i];
            }
            
            return FindPathToState(wiringMatrix, current, target);
        }

        private static int FindPathToState(int[][] wiringMatrix, int[] start, int[] target)
        {
            int targetLen = target.Length;
            int numButtons = wiringMatrix.Length;
            
            var visited = new HashSet<long>();
            var queue = new Queue<(int[] state, int depth)>();
            
            var startHash = ComputeHashFast(start);
            visited.Add(startHash);
            queue.Enqueue((start, 0));
            
            var targetHash = ComputeHashFast(target);
            
            const int maxDepth = 200;
            const int maxStates = 1000000;
            
            while (queue.Count > 0)
            {
                var (state, depth) = queue.Dequeue();
                
                var hash = ComputeHashFast(state);
                if (hash == targetHash)
                {
                    return depth;
                }

                if (depth >= maxDepth)
                {
                    continue;
                }
                
                // Try each button
                for (int w = 0; w < numButtons; w++)
                {
                    bool valid = true;
                    
                    for (int i = 0; i < targetLen; i++)
                    {
                        if (state[i] + wiringMatrix[w][i] > target[i])
                        {
                            valid = false;
                            break;
                        }
                    }
                    
                    if (!valid) continue;
                    
                    var next = new int[targetLen];
                    for (int i = 0; i < targetLen; i++)
                    {
                        next[i] = state[i] + wiringMatrix[w][i];
                    }
                    
                    var nextHash = ComputeHashFast(next);
                    
                    if (visited.Add(nextHash))
                    {
                        queue.Enqueue((next, depth + 1));
                    }
                }
                
                // Memory limit
                if (visited.Count > maxStates)
                {
                    Console.WriteLine($"Memory limit in FindPathToState. Visited: {visited.Count}");
                    return -1;
                }
            }
            
            return -1;
        }

        private static int DirectBFS(int[][] wiringMatrix, int[] target)
        {
            int targetLen = target.Length;
            int numButtons = wiringMatrix.Length;
            
            var visited = new Dictionary<long, int>();
            var queue = new Queue<(int[] state, int depth)>();
            
            var start = new int[targetLen];
            var startHash = ComputeHashFast(start);
            visited[startHash] = 0;
            queue.Enqueue((start, 0));
            
            var targetHash = ComputeHashFast(target);

            const int maxDepth = 400;

            while (queue.Count > 0)
            {
                var (state, depth) = queue.Dequeue();
                
                var hash = ComputeHashFast(state);
                if (hash == targetHash)
                {
                    return depth;
                }

                if (depth >= maxDepth)
                {
                    continue;
                }
                
                // Try each button
                for (int w = 0; w < numButtons; w++)
                {
                    bool valid = true;
                    
                    for (int i = 0; i < targetLen; i++)
                    {
                        if (state[i] + wiringMatrix[w][i] > target[i])
                        {
                            valid = false;
                            break;
                        }
                    }
                    
                    if (!valid) continue;
                    
                    var next = new int[targetLen];
                    for (int i = 0; i < targetLen; i++)
                    {
                        next[i] = state[i] + wiringMatrix[w][i];
                    }
                    
                    var nextHash = ComputeHashFast(next);
                    int newDepth = depth + 1;
                    
                    if (!visited.TryGetValue(nextHash, out var existing) || newDepth < existing)
                    {
                        visited[nextHash] = newDepth;
                        queue.Enqueue((next, newDepth));
                    }
                }
                
                // Memory limit
                if (visited.Count > 10_000_000)
                {
                    Console.WriteLine("Memory limit in DirectBFS. Visited: " + visited.Count);
                    break;
                }
            }
            
            return -1;
        }

        private static long ComputeHashFast(int[] levels)
        {
            long hash = 17;
            for (int i = 0; i < levels.Length; i++)
            {
                hash = hash * 31 + levels[i];
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
}
