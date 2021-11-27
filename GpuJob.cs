using Cloo;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace VanityAddrGen
{
    public sealed class GpuJob : Job
    {
        public new sealed class Params : Job.Params
        {
            public int WorkSize;
            public int PlatformIndex;
        }

        public const int MaxWorkSize = 100_000;

        private readonly int workSize;
        private readonly int platformIndex;

        public GpuJob(Params @params)
            : base(@params)
        {
            workSize = @params.WorkSize;
            platformIndex = @params.PlatformIndex;
        }

        public override void Run(object? arg)
        {
            string prefix1 = string.Concat(AddressPrefix, "3", keyword);
            string prefix2 = string.Concat(AddressPrefix, "1", keyword);
            string suffix1 = keyword;

            int workSize = this.workSize;

            byte[] bigPublicKeyBytes = new byte[32 * workSize];
            byte[] bigChecksumBytes = new byte[5 * workSize];
            byte[] bigSeedBytes = new byte[31 + workSize];

            AddressBuffer addressBuffer = new(AddressPrefix.Length + 60);

            bool canMatchPrefix = this.canMatchPrefix;
            bool canMatchSuffix = this.canMatchSuffix;
            CancellationToken cancellationToken = this.cancellationToken;

            GCHandle hPublicKey = GCHandle.Alloc(bigPublicKeyBytes, GCHandleType.Pinned);
            GCHandle hChecksum = GCHandle.Alloc(bigChecksumBytes, GCHandleType.Pinned);
            GCHandle hSeed = GCHandle.Alloc(bigSeedBytes, GCHandleType.Pinned);

            ComputePlatform platform = ComputePlatform.Platforms[platformIndex];
            ComputeContext context = new(
                ComputeDeviceTypes.Gpu,
                new ComputeContextPropertyList(platform),
                null, IntPtr.Zero);
            ComputeProgram program = new(context, new string[]
            {
                OpenCl.Blake2b,
                OpenCl.Curve25519Constants,
                OpenCl.Curve25519Constants2,
                OpenCl.Curve25519,
                OpenCl.Entry,
            });
            program.Build(null, null, null, IntPtr.Zero);
            ComputeKernel kernel = program.CreateKernel("generate_pubkey");
            ComputeCommandQueue queue = new(context, context.Devices[0], ComputeCommandQueueFlags.None);

            ComputeBuffer<byte> argPublicKey = new(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, bigPublicKeyBytes);
            ComputeBuffer<byte> argChecksum = new(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, bigChecksumBytes);
            ComputeBuffer<byte> argSeed = new(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, bigSeedBytes);

            addressBuffer.Append(AddressPrefix);
            while (!cancellationToken.IsCancellationRequested)
            {
                random.GetBytes(bigSeedBytes);
                queue.Write(argSeed, true, 0, bigSeedBytes.Length, hSeed.AddrOfPinnedObject(), null);
                kernel.SetMemoryArgument(0, argPublicKey);
                kernel.SetMemoryArgument(1, argChecksum);
                kernel.SetMemoryArgument(2, argSeed);
                queue.Execute(kernel, null, new long[] { workSize }, null, null);
                queue.Read(argPublicKey, true, 0, bigPublicKeyBytes.Length, hPublicKey.AddrOfPinnedObject(), null);
                queue.Read(argChecksum, true, 0, bigChecksumBytes.Length, hChecksum.AddrOfPinnedObject(), null);
                queue.Finish();

                string foundSeed = null;
                for (int i = 0; i < workSize; ++i)
                {
                    ArraySegment<byte> seed = new(bigSeedBytes, i, 32);
                    ArraySegment<byte> publicKey = new(bigPublicKeyBytes, i * 32, 32);
                    ArraySegment<byte> checksum = new(bigChecksumBytes, i * 5, 5);
                    Reverse(checksum);

                    NanoBase32(publicKey, ref addressBuffer);
                    NanoBase32(checksum, ref addressBuffer);

                    bool isMatched = false;
                    if (canMatchPrefix)
                        isMatched = addressBuffer.StartsWith(prefix1) || addressBuffer.StartsWith(prefix2);
                    if (!isMatched && canMatchSuffix)
                        isMatched = addressBuffer.EndsWith(suffix1);

                    if (isMatched)
                    {
                        var address = addressBuffer.ToString();
                        if (resultCallback != null)
                        {
                            resultCallback.Invoke(HexUtils.HexFromByteArray(seed), address);
                        }
                        else
                        {
                            foundSeed = HexUtils.HexFromByteArray(seed);
                            FoundSeed = foundSeed;
                            FoundAddress = address;

                            attempts += i;
                            break;
                        }
                    }

                    addressBuffer.Length = AddressPrefix.Length;
                }

                if (foundSeed != null)
                    break;

                attempts += workSize;
            }

            argPublicKey.Dispose();
            argChecksum.Dispose();
            argSeed.Dispose();

            hPublicKey.Free();
            hChecksum.Free();
            hSeed.Free();
        }
    }
}