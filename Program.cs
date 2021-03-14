using System;
using System.IO;
using System.Threading;

namespace VanityAddrGen
{
    internal class Program
    {
        private const string ProjectUrl = @"https://github.com/alexanderdna/VanityAddrGen";

        private static string resultFilePath;

        public static void Main()
        {
            Console.WriteLine("Vanity Address Generator - written by @alexanderdna");
            Console.WriteLine();

            ConfigFile configFile = null;
            try
            {
                configFile = ConfigFile.Load();
                if (configFile == null)
                {
                    Console.WriteLine("Cannot find config.txt");
                    Console.WriteLine($"Please visit {ProjectUrl} for more details.");
                    Console.ReadLine();
                    return;
                }
            }
            catch (ConfigFile.InvalidConfigFileException ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Cannot read config.txt, error: {ex.Message}.");
                Console.ForegroundColor = ConsoleColor.Gray;

                Console.WriteLine($"Please visit {ProjectUrl} for more details.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Configuration");

            Console.WriteLine("\tHardware: " + (configFile.JobHardware) switch
            {
                ConfigFile.Hardware.CPU => "CPU",
                ConfigFile.Hardware.GPU => "GPU",
                ConfigFile.Hardware.Both => "CPU & GPU",
                _ => "unknown"
            });

            if (configFile.JobHardware is ConfigFile.Hardware.CPU or ConfigFile.Hardware.Both)
                Console.WriteLine($"\tCPU threads: {configFile.CpuThreads:N0}");
            if (configFile.JobHardware is ConfigFile.Hardware.GPU or ConfigFile.Hardware.Both)
            {
                Console.WriteLine($"\tGPU threads: {configFile.GpuThreads:N0}");
                Console.WriteLine($"\tGPU platform: {configFile.GpuPlatformIndex} => {Cloo.ComputePlatform.Platforms[configFile.GpuPlatformIndex].Devices[0].Name}.");
            }

            Console.WriteLine("\tMatch: " + (configFile.AddressMatching) switch
            {
                ConfigFile.MatchingPolicy.Prefix => "prefix",
                ConfigFile.MatchingPolicy.Suffix => "suffix",
                ConfigFile.MatchingPolicy.Both => "prefix & suffix",
                _ => "unknown"
            });

            if (configFile.IsNonStop)
                Console.WriteLine("\tNon-stop: true (shows and appends addresses to result file).");
            else
                Console.WriteLine("\tNon-stop: false (shows 1 address and stops)");

            Console.WriteLine();

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
                        Console.WriteLine("Keyword cannot contain these characters: 0, 2, l (small letter L), v");
                        isKeywordOk = false;
                        break;
                    }
                }

                if (!isKeywordOk) continue;
                break;
            }
            Console.WriteLine();

            var cancellation = new CancellationTokenSource();
            int randomSeed = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            var jobParams = new GpuJob.Params
            {
                WorkSize = configFile.GpuThreads,
                PlatformIndex = configFile.GpuPlatformIndex,
                Keyword = keyword,
                CanMatchPrefix = configFile.AddressMatching is ConfigFile.MatchingPolicy.Prefix or ConfigFile.MatchingPolicy.Both,
                CanMatchSuffix = configFile.AddressMatching is ConfigFile.MatchingPolicy.Suffix or ConfigFile.MatchingPolicy.Both,
                RandomSeed = randomSeed,
                CancellationToken = cancellation.Token,
            };

            if (configFile.IsNonStop)
            {
                resultFilePath = Path.Combine(AppContext.BaseDirectory, $"result-{keyword}.txt");
                jobParams.ResultCallback = writeResult;
            }

            Job[] jobs;
            switch (configFile.JobHardware)
            {
                case ConfigFile.Hardware.CPU:
                    jobs = new Job[configFile.CpuThreads];
                    for (int i = 0, c = configFile.CpuThreads; i < c; ++i)
                    {
                        jobParams.RandomSeed = randomSeed + i;
                        jobs[i] = new CpuJob(jobParams);
                    }
                    break;

                case ConfigFile.Hardware.GPU:
                    jobs = new Job[] {
                        new GpuJob(jobParams)
                    };
                    break;

                case ConfigFile.Hardware.Both:
                    jobs = new Job[1 + configFile.CpuThreads];
                    jobs[0] = new GpuJob(jobParams);
                    for (int i = 0, c = configFile.CpuThreads; i < c; ++i)
                    {
                        jobParams.RandomSeed = randomSeed + i + 1;
                        jobs[i + 1] = new CpuJob(jobParams);
                    }
                    break;

                default:
                    return;
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
                    Console.WriteLine(foundSeed);

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

        private static void writeResult(string seed, string address)
        {
            string data = string.Concat(seed, " ", address, Environment.NewLine);
            File.AppendAllText(resultFilePath, data);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Seed:    ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(seed);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Address: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(address);

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}