using System;
using System.Threading;

namespace VanityAddrGen
{
    internal class Program
    {
        public static void Main()
        {
            Console.WriteLine("Vanity Address Generator - written by @alexanderdna");
            Console.WriteLine();

            Console.Write("Use GPU? Y/n ");
            bool useGpu = Console.ReadLine().ToLower() != "n";

            int jobCount;
            int maxJobCount = useGpu ? GpuJob.MaxWorkSize : 8;
            while (true)
            {
                Console.Write("Threads (1 to {0}): ", maxJobCount);
                if (!int.TryParse(Console.ReadLine(), out int chosenJobCount)
                    || chosenJobCount < 1 || chosenJobCount > maxJobCount)
                {
                    continue;
                }
                jobCount = chosenJobCount;
                break;
            }

            string keyword;
            while (true)
            {
                Console.Write("Keyword: ");
                keyword = Console.ReadLine().Trim();
                if (keyword.Length == 0 || keyword.Length > 10)
                {
                    Console.WriteLine("Keyword must contain 1 to 10 characters.");
                    continue;
                }

                bool isKeywordOk = true;
                for (int i = 0, c = keyword.Length; i < c; ++i)
                {
                    if (Array.IndexOf(Job.NanoBase32Alphabet, keyword[i]) < 0)
                    {
                        Console.WriteLine("Keyword contains invalid character: {0}", keyword[i]);
                        isKeywordOk = false;
                        break;
                    }
                }

                if (!isKeywordOk) continue;
                break;
            }

            var cancellation = new CancellationTokenSource();
            var jobs = new Job[useGpu ? 1 : jobCount];
            int randomSeed = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            if (useGpu)
            {
                jobs[0] = new GpuJob(jobCount, keyword, randomSeed, cancellation.Token);
            }
            else
            {
                for (int i = 0, c = jobs.Length; i < c; ++i)
                {
                    jobs[i] = new CpuJob(keyword, randomSeed + i, cancellation.Token);
                }
            }

            for (int i = 0, c = jobs.Length; i < c; ++i)
            {
                ThreadPool.QueueUserWorkItem(jobs[i].Run);
            }

            long totalAttempts = 0;
            long lastTotalAttempts = 0;
            while (true)
            {
                string foundSeed = null;
                string foundAddress = null;

                totalAttempts = 0;

                for (int i = 0, c = jobs.Length; i < c; ++i)
                {
                    var job = jobs[i];
                    if (foundAddress == null && job.FoundAddress != null)
                    {
                        foundSeed = job.FoundSeed;
                        foundAddress = job.FoundAddress;
                        cancellation.Cancel();
                    }
                    totalAttempts += job.Attempts;
                }

                if (foundSeed != null && foundAddress != null)
                {
                    Console.WriteLine("{0:N0} attempts made. {1:N0} more since last log.", totalAttempts, totalAttempts - lastTotalAttempts);

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Seed:    ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(foundSeed.ToUpper());

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Address: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(foundAddress);

                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                }
                else
                {
                    if (totalAttempts > 0)
                        Console.WriteLine("{0:N0} attempts made. {1:N0} more since last log.", totalAttempts, totalAttempts - lastTotalAttempts);

                    lastTotalAttempts = totalAttempts;

                    Thread.Sleep(1000);
                }
            }

            Console.ReadLine();
        }
    }
}