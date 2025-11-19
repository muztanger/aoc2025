using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aoc2024.Commons
{
    public class Profiler
    {
        private Stopwatch timer = new Stopwatch();
        private long mem = -1;

        public void Start()
        {
            timer.Reset();
            mem = GC.GetTotalAllocatedBytes();
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
            mem = GC.GetTotalAllocatedBytes() - mem;
        }

        public void Print(string title = "")
        {
            if (!string.IsNullOrEmpty(title))
            {
                Console.WriteLine(title);
            }
            Console.WriteLine($"Elapsed: {timer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Allocated memory: {mem / 1024.0:N2} kb");
        }
    }
}
