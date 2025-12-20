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
            
            return SolveWithButtonCounts(wiringMatrix, target);
        }

        private static int SolveWithButtonCounts(int[][] wiringMatrix, int[] target)
        {
            int numButtons = wiringMatrix.Length;
            int targetLen = target.Length;
            
            // State = how many times each button has been pressed
            var pq = new PriorityQueue<(int[] buttonCounts, int totalPresses), int>();
            var visited = new HashSet<long>();
            
            var start = new int[numButtons];
            var startHash = ComputeButtonHash(start);
            
            pq.Enqueue((start, 0), 0);
            visited.Add(startHash);
            
            while (pq.Count > 0 && visited.Count < 10000000)
            {
                var (buttonCounts, totalPresses) = pq.Dequeue();
                
                // Calculate current joltage levels from button counts
                var currentLevels = new int[targetLen];
                for (int w = 0; w < numButtons; w++)
                {
                    for (int i = 0; i < targetLen; i++)
                    {
                        currentLevels[i] += buttonCounts[w] * wiringMatrix[w][i];
                    }
                }
                
                // Check if we reached the target
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
                
                // Try pressing each button one more time
                for (int w = 0; w < numButtons; w++)
                {
                    // Check if pressing this button would exceed any target
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
                    
                    // Create new button count state
                    var nextCounts = new int[numButtons];
                    for (int b = 0; b < numButtons; b++)
                    {
                        nextCounts[b] = buttonCounts[b];
                    }
                    nextCounts[w]++; // FIX: Actually increment the button being pressed!
                    
                    var nextHash = ComputeButtonHash(nextCounts);
                    
                    if (visited.Add(nextHash))
                    {
                        // Calculate heuristic
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
