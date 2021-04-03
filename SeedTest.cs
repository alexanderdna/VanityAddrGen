using Blake2Fast;
using Cloo;
using System;
using System.Runtime.InteropServices;

namespace VanityAddrGen
{
    public static class SeedTest
    {
        /// <summary>
        /// Returns address from the given seed. Uses CPU for calculation.
        /// </summary>
        public static string TestCpu(byte[] seed)
        {
            byte[] secretBytes = new byte[32];
            byte[] indexBytes = new byte[4];
            byte[] publicKeyBytes = new byte[32];
            byte[] checksumBytes = new byte[5];

            byte[] tmp = new byte[64];

            Job.AddressBuffer addressBuffer = new(Job.AddressPrefix.Length + 60);

            addressBuffer.Append(Job.AddressPrefix);

            var hasher = Blake2b.CreateIncrementalHasher(32);
            hasher.Update(seed);
            hasher.Update(indexBytes);
            hasher.Finish(secretBytes);

            Chaos.NaCl.Internal.Ed25519Ref10.Ed25519Operations.crypto_public_key(
                secretBytes, 0, publicKeyBytes, 0, tmp);

            Blake2b.ComputeAndWriteHash(5, publicKeyBytes, checksumBytes);
            Job.Reverse(checksumBytes);

            Job.NanoBase32(publicKeyBytes, ref addressBuffer);
            Job.NanoBase32(checksumBytes, ref addressBuffer);

            return addressBuffer.ToString();
        }

        /// <summary>
        /// Returns address from the given seed. Uses GPU for calculation.
        /// </summary>
        public static string TestGpu(byte[] seed, int platformIndex)
        {
            byte[] publicKeyBytes = new byte[32];
            byte[] checksumBytes = new byte[5];

            Job.AddressBuffer addressBuffer = new(Job.AddressPrefix.Length + 60);

            GCHandle hPublicKey = GCHandle.Alloc(publicKeyBytes, GCHandleType.Pinned);
            GCHandle hChecksum = GCHandle.Alloc(checksumBytes, GCHandleType.Pinned);
            GCHandle hSeed = GCHandle.Alloc(seed, GCHandleType.Pinned);

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

            ComputeBuffer<byte> argPublicKey = new(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, publicKeyBytes);
            ComputeBuffer<byte> argChecksum = new(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, checksumBytes);
            ComputeBuffer<byte> argSeed = new(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, seed);

            addressBuffer.Append(Job.AddressPrefix);

            queue.Write(argSeed, true, 0, seed.Length, hSeed.AddrOfPinnedObject(), null);
            kernel.SetMemoryArgument(0, argPublicKey);
            kernel.SetMemoryArgument(1, argChecksum);
            kernel.SetMemoryArgument(2, argSeed);
            queue.Execute(kernel, null, new long[] { 1 }, null, null);
            queue.Read(argPublicKey, true, 0, publicKeyBytes.Length, hPublicKey.AddrOfPinnedObject(), null);
            queue.Read(argChecksum, true, 0, checksumBytes.Length, hChecksum.AddrOfPinnedObject(), null);
            queue.Finish();

            Job.Reverse(checksumBytes);

            Job.NanoBase32(publicKeyBytes, ref addressBuffer);
            Job.NanoBase32(checksumBytes, ref addressBuffer);

            argPublicKey.Dispose();
            argChecksum.Dispose();
            argSeed.Dispose();

            hPublicKey.Free();
            hChecksum.Free();
            hSeed.Free();

            return addressBuffer.ToString();
        }
    }
}