using Blake2Fast;
using System;
using System.Threading;

namespace VanityAddrGen
{
    internal class Program
    {
        private sealed class Job
        {
            public const string AddressPrefix = "nano_";

            private string keyword;
            private CancellationToken cancellationToken;
            private Random random;
            private byte[] seedBytes;
            private byte[] secretBytes;
            private byte[] indexBytes;
            private byte[] checksumBytes;
            private long attempts;
            private System.Text.StringBuilder sb;

            public long Attempts => attempts;

            public string FoundSeed { get; private set; }
            public string FoundAddress { get; private set; }

            public Job(string keyword, int randomSeed, CancellationToken cancellationToken)
            {
                this.keyword = keyword;
                this.cancellationToken = cancellationToken;
                random = new Random(randomSeed);
                seedBytes = new byte[32];
                secretBytes = new byte[32];
                indexBytes = new byte[4];
                checksumBytes = new byte[5];
                attempts = 0;
                sb = new System.Text.StringBuilder(100);
            }

            public void Run(object? arg)
            {
                string prefix1 = string.Concat(AddressPrefix, "3", keyword);
                string prefix2 = string.Concat(AddressPrefix, "1", keyword);
                string suffix1 = keyword;

                sb.Append(AddressPrefix);
                while (!cancellationToken.IsCancellationRequested)
                {
                    random.NextBytes(seedBytes);

                    var hasher = Blake2b.CreateIncrementalHasher(32);
                    hasher.Update(seedBytes);
                    hasher.Update(indexBytes);
                    hasher.Finish(secretBytes);

                    var publicKey = Chaos.NaCl.Ed25519.PublicKeyFromSeed(secretBytes);

                    nanoBase32(publicKey, sb);

                    Blake2b.ComputeAndWriteHash(5, publicKey, checksumBytes);
                    reverse(checksumBytes);
                    nanoBase32(checksumBytes, sb);

                    var address = sb.ToString();
                    if (address.StartsWith(prefix1)
                        || address.StartsWith(prefix2)
                        || address.EndsWith(suffix1))
                    {
                        FoundSeed = HexUtils.HexFromByteArray(seedBytes);
                        FoundAddress = address;
                        break;
                    }

                    ++attempts;
                    sb.Length = AddressPrefix.Length;
                }
            }

            private static void reverse(byte[] arr)
            {
                Span<byte> tmp = stackalloc byte[arr.Length];
                for (int i = 0, c = arr.Length; i < c; ++i)
                {
                    tmp[i] = arr[c - i - 1];
                }
                tmp.CopyTo(arr);
            }
        }

        private static readonly char[] nanoBase32Alphabet = "13456789abcdefghijkmnopqrstuwxyz".ToCharArray();

        private static void nanoBase32(byte[] arr, System.Text.StringBuilder sb)
        {
            int length = arr.Length;
            int leftover = (length * 8) % 5;
            int offset = leftover == 0 ? 0 : 5 - leftover;

            int value = 0;
            int bits = 0;

            for (int i = 0; i < length; ++i)
            {
                value = (value << 8) | arr[i];
                bits += 8;

                while (bits >= 5)
                {
                    sb.Append(nanoBase32Alphabet[(value >> (bits + offset - 5)) & 31]);
                    bits -= 5;
                }
            }

            if (bits > 0)
            {
                sb.Append(nanoBase32Alphabet[(value << (5 - (bits + offset))) & 31]);
            }
        }

        public static void Main()
        {
            Console.WriteLine("Vanity Address Generator - written by @alexanderdna");
            Console.WriteLine();

            int jobCount;
            while (true)
            {
                Console.Write("Threads (1 to 8): ");
                if (!int.TryParse(Console.ReadLine(), out int threads)
                    || threads < 1 || threads > 8)
                {
                    continue;
                }
                jobCount = threads;
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
                    if (Array.IndexOf(nanoBase32Alphabet, keyword[i]) < 0)
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
            var jobs = new Job[jobCount];
            int randomSeed = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            for (int i = 0, c = jobs.Length; i < c; ++i)
            {
                jobs[i] = new Job(keyword, randomSeed + i, cancellation.Token);
            }

            for (int i = 0, c = jobs.Length; i < c; ++i)
            {
                ThreadPool.QueueUserWorkItem(jobs[i].Run);
            }

            while (true)
            {
                string foundSeed = null;
                string foundAddress = null;

                long totalAttempts = 0;
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
                    Console.WriteLine("{0:N0} attempts made.", totalAttempts);
                    Console.WriteLine("Seed:    {0}", foundSeed.ToUpper());
                    Console.WriteLine("Address: {0}", foundAddress);
                    break;
                }
                else
                {
                    if (totalAttempts > 0)
                        Console.WriteLine("{0:N0} attempts made.", totalAttempts);

                    Thread.Sleep(1000);
                }
            }

            Console.ReadLine();
        }
    }
}