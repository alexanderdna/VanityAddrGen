using System;
using System.Collections.Generic;
using System.IO;

namespace VanityAddrGen
{
    public sealed class ConfigFile
    {
        public enum Hardware
        {
            CPU,
            GPU,
            Both,
        }

        public enum MatchingPolicy
        {
            Prefix,
            Suffix,
            Both,
        }

        public sealed class InvalidConfigFileException : Exception
        {
            public InvalidConfigFileException(string message) : base(message)
            {
            }
        }

        public readonly Hardware JobHardware;
        public readonly MatchingPolicy AddressMatching;
        public readonly int CpuThreads;
        public readonly int GpuThreads;
        public readonly int GpuPlatformIndex;
        public readonly bool IsNonStop;

        private ConfigFile(Hardware jobHardware, MatchingPolicy addressMatching, int cpuThreads, int gpuThreads, int gpuPlatformIndex, bool isNonStop)
        {
            JobHardware = jobHardware;
            AddressMatching = addressMatching;
            CpuThreads = cpuThreads;
            GpuThreads = gpuThreads;
            GpuPlatformIndex = gpuPlatformIndex;
            IsNonStop = isNonStop;
        }

        public static ConfigFile Load()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "config.txt");
            if (!File.Exists(filePath)) return null;

            Dictionary<string, string> dict = new Dictionary<string, string>();

            var lines = File.ReadAllLines(filePath);
            for (int i = 0, c = lines.Length; i < c; ++i)
            {
                string line = lines[i].Trim();
                if (line.Length == 0) continue;

                string[] parts = line.Split('=', StringSplitOptions.None);
                if (parts.Length == 1)
                {
                    if (parts[0].Length > 0 && parts[0][0] == '#')
                        continue;
                    else
                        throw new InvalidConfigFileException("Invalid config format.");
                }
                else if (parts.Length == 2)
                {
                    string key = parts[0];
                    string value = parts[1];
                    if (dict.ContainsKey(key))
                        throw new InvalidConfigFileException("Duplicate config key.");

                    dict.Add(key, value);
                }
                else
                {
                    throw new InvalidConfigFileException("Invalid config format");
                }
            }

            Hardware hardware = Hardware.CPU;
            MatchingPolicy matchingPolicy = MatchingPolicy.Both;
            int cpuThreads = 1;
            int gpuThreads = 1000;
            int gpuPlatformIndex = 0;
            bool isNonStop = false;

            if (dict.ContainsKey("hardware"))
            {
                hardware = (dict["hardware"]) switch
                {
                    "cpu" => Hardware.CPU,
                    "gpu" => Hardware.GPU,
                    "cpu+gpu" => Hardware.Both,
                    _ => throw new InvalidConfigFileException("Invalid hardware value."),
                };
            }

            if (dict.ContainsKey("match"))
            {
                matchingPolicy = (dict["match"]) switch
                {
                    "prefix" => MatchingPolicy.Prefix,
                    "suffix" => MatchingPolicy.Suffix,
                    "prefix+suffix" => MatchingPolicy.Both,
                    _ => throw new InvalidConfigFileException("Invalid matching value."),
                };
            }

            if (dict.ContainsKey("cpu_threads"))
            {
                if (!int.TryParse(dict["cpu_threads"], out cpuThreads))
                    throw new InvalidConfigFileException("Invalid number for CPU threads.");

                if (cpuThreads < 1 || cpuThreads > 8)
                    throw new InvalidConfigFileException("CPU threads must be from 1 to 8.");
            }

            if (dict.ContainsKey("gpu_threads"))
            {
                if (!int.TryParse(dict["gpu_threads"], out gpuThreads))
                    throw new InvalidConfigFileException("Invalid number for GPU threads.");

                if (gpuThreads < 0 || gpuThreads > GpuJob.MaxWorkSize)
                    throw new InvalidConfigFileException($"GPU threads must be from 1 to {GpuJob.MaxWorkSize}.");
            }

            if (dict.ContainsKey("gpu_platform"))
            {
                if (!int.TryParse(dict["gpu_platform"], out gpuPlatformIndex))
                    throw new InvalidConfigFileException("Invalid number for GPU platform index.");

                int maxPlatformIndex = Cloo.ComputePlatform.Platforms.Count - 1;
                if (gpuPlatformIndex < 0 || gpuPlatformIndex > maxPlatformIndex)
                    throw new InvalidConfigFileException($"GPU platform index must be from 0 to {maxPlatformIndex}.");
            }

            if (dict.ContainsKey("non_stop"))
            {
                if (!int.TryParse(dict["non_stop"], out int n)
                    || (n != 0 && n != 1))
                    throw new InvalidConfigFileException("Invalid value for non_stop.");

                isNonStop = n == 1;
            }

            return new ConfigFile(hardware, matchingPolicy, cpuThreads, gpuThreads, gpuPlatformIndex, isNonStop);
        }
    }
}