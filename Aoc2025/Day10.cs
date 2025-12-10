namespace Advent_of_Code_2025;

[TestClass]
public class Day10
{
    class Machine
    {

        List<bool> _lights;
        readonly List<bool> _diagram;
        List<List<int>> _wirings;
        List<int> _requirements;

        public Machine(List<bool> diagram, List<List<int>> wirings, List<int> requirements)
        {
            _diagram = diagram;
            _lights = [.. new bool[_diagram.Count]];
            _wirings = wirings;
            _requirements = requirements;
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

        public int FewestStepsToGoal()
        {
            bool IsGoalState(List<bool> lights)
            {
                for (int i = 0; i < lights.Count; i++)
                {
                    int litCount = lights[i] ? 1 : 0;
                    if (litCount != _requirements[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            var initialState = (_lights, 0);
            var visited = new Dictionary<List<bool>, int>();
            var queue = new PriorityQueue<((List<bool> lights, int step) state, int steps), int>();
            queue.Enqueue((initialState, 0), 0);
            visited[initialState.Item1] = initialState.Item2;
            while (queue.Count > 0)
            {
                var (currentState, steps) = queue.Dequeue();
                var (currentLights, currentWiringIndex) = currentState;
                if (IsGoalState(currentLights))
                {
                    return steps;
                }
                // Toggle lights based on current wiring
                var newLights = new List<bool>(currentLights);
                foreach (var lightIndex in _wirings[currentWiringIndex])
                {
                    newLights[lightIndex] = !newLights[lightIndex];
                }
                for (var nextWiringIndex = 0; nextWiringIndex < _wirings.Count; nextWiringIndex++)
                {
                    var newState = (newLights, nextWiringIndex);
                    if (!visited.TryGetValue(newLights, out var visitedCount) && visitedCount > steps + 1)
                    {
                        visited[newLights] = steps + 1;
                        queue.Enqueue((newState, steps + 1), steps + 1);
                    }
                }
            }
            return -1; // No solution found
        }
    }

    private static string Part1(IEnumerable<string> input)
    {
        var result = new StringBuilder();
        foreach (var line in input)
        {
            Console.WriteLine(Machine.Parse(line).FewestStepsToGoal());
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
    public void Day10_Part1_Example01()
    {
        var input = """
            [.##.] (3) (1,3) (2) (2,3) (0,2) (0,1) {3,5,4,7}
            [...#.] (0,2,3,4) (2,3) (0,4) (0,1,2) (1,2,3,4) {7,5,12,7,2}
            [.###.#] (0,1,2,3,4) (0,3,4) (0,1,2,4,5) (1,2) {10,11,11,5,10,5}
            """;
        var result = Part1(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day10_Part1_Example02()
    {
        var input = """
            <TODO>
            """;
        var result = Part1(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day10_Part1()
    {
        var result = Part1(Common.DayInput(nameof(Day10), "2025"));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day10_Part2_Example01()
    {
        var input = """
            <TODO>
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day10_Part2_Example02()
    {
        var input = """
            <TODO>
            """;
        var result = Part2(Common.GetLines(input));
        Assert.AreEqual("", result);
    }
    
    [TestMethod]
    public void Day10_Part2()
    {
        var result = Part2(Common.DayInput(nameof(Day10), "2025"));
        Assert.AreEqual("", result);
    }
    
}
