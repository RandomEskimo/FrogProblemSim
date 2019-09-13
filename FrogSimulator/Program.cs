using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FrogSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("+----------------------+");
            Console.WriteLine("Frog Problem simulator");
            Console.WriteLine("By Chris Tremayne 2019");
            Console.WriteLine("+----------------------+");

            int nPadsLower = GetNumber("Number of pads in pond (lower bound, inclusive): ");
            int nPadsUpper = GetNumber("Number of pads in pond (upper bound, inclusive): ");
            int kiloRuns = GetNumber("Simulations per pond in 1000's: ");

            Console.Write("Save to csv file? (y/n): ");
            bool saveToCsv = Console.ReadLine().ToLower() == "y";

            bool printResults = true;

            string fileName = null;
            if(saveToCsv)
            {
                Console.Write("Filename: ");
                fileName = Console.ReadLine();
                Console.Write("Print results to screen (y/n): ");
                printResults = Console.ReadLine().ToLower() == "y";
            }

            List<Tuple<int, decimal, TimeSpan>> results = new List<Tuple<int, decimal, TimeSpan>>();

            DateTime allStart = DateTime.Now;

            for(int i = nPadsLower;i < nPadsUpper;++i)
            {
                if (printResults)
                {
                    Console.Write("Peforming {0} simulations for {1} pads...", kiloRuns * 1000, i);
                }
                DateTime start = DateTime.Now;
                decimal result = SimulateNPads(i, kiloRuns);
                TimeSpan elapsed = DateTime.Now - start;
                results.Add(new Tuple<int, decimal, TimeSpan>(i, result, elapsed));
                if (printResults)
                {
                    Console.WriteLine(" {0} in {1}", result, elapsed);
                }
            }

            if(saveToCsv)
            {
                try
                {
                    StringBuilder csvBuilder = new StringBuilder();
                    csvBuilder.AppendLine("\"Pads\",\"Average Hops\",\"Time Taken\"");
                    foreach(Tuple<int, decimal, TimeSpan> result in results)
                    {
                        csvBuilder.AppendLine(string.Format("{0},{1},{2}", result.Item1, result.Item2, result.Item3));
                    }
                    File.WriteAllText(fileName, csvBuilder.ToString());
                }
                catch
                {
                    Console.Write("Unable to save to file");
                }
            }

            Console.WriteLine("Finished all in {0}", DateTime.Now - allStart);
        }

        private static int GetNumber(string Prompt)
        {
            int value = 0;
            while(true)
            {
                Console.Write(Prompt);
                string input = Console.ReadLine();
                if(int.TryParse(input, out value) && value > 0)
                {
                    return value;
                }
                Console.WriteLine("Invalid input");
            }
        }

        public static decimal SimulateNPads(int NPads, int KiloRuns)
        {
            Random r = new Random();
            decimal result = 0;
            object resultLock = new object();
            if(KiloRuns == 1)
            {
                result = Perform1000Runs(NPads, 1000, r.Next());
            }
            else
            {
                Parallel.For(0, KiloRuns, n =>
                {
                    int seed = 0;
                    lock (r)
                    {
                        seed = r.Next();
                    }
                    decimal partialResult = Perform1000Runs(NPads, KiloRuns * 1000, seed);
                    lock(resultLock)
                    {
                        result += partialResult;
                    }
                });
            }
            return result;
        }

        private static decimal Perform1000Runs(int NPads, int TotalRuns, int RandomSeed)
        {
            Random r = new Random();
            decimal result = 0;
            for (int i = 0; i < 1000; ++i)
            {
                int dist = NPads;
                int jumps = 0;
                for (; dist > 0; ++jumps)
                {
                    dist -= r.Next(dist) + 1;
                }
                result += jumps / (decimal)TotalRuns;
            }
            return result;
        }
    }
}
