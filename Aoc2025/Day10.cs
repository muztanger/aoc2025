




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
            List<int> Values;
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
            var initialState = (_levels, 0);
            var visited = new Dictionary<(JoltageLevels joltageLevels, int step), int>();
            var queue = new Queue<((JoltageLevels joltageLevels, int step) state, int steps)>();
            queue.Enqueue((initialState, 0));
            visited[initialState] = 0;
            while (queue.Count > 0)
            {   
                var (currentState, steps) = queue.Dequeue();
                var (currentJoltageLevels, currentWiringIndex) = currentState;
                if (currentJoltageLevels.Equals(_requirements))
                {
                    return steps;
                }
                Assert.IsNotNull(currentJoltageLevels);
                
                var newJoltageLevels = new JoltageLevels(currentJoltageLevels);
                newJoltageLevels.Press(_wirings[currentWiringIndex]);
                if (newJoltageLevels.IsTooHighLevel(_requirements))
                {
                    continue;
                }

                for (var nextWiringIndex = 0; nextWiringIndex < _wirings.Count; nextWiringIndex++)
                {
                    var newState = (newJoltageLevels, nextWiringIndex);
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
        var result = Part2(Common.GetLines(example));
        Assert.AreEqual("33", result);
    }
    
    [TestMethod]
    public void Day10_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day10), "2025"));
        Assert.AreEqual("", result);
    }
    
}
