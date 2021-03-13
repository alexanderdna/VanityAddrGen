using Blake2Fast;
using System.Threading;

namespace VanityAddrGen
{
    public sealed class CpuJob : Job
    {
        public CpuJob(string keyword, int randomSeed, CancellationToken cancellationToken)
            : base(keyword, randomSeed, cancellationToken)
        {
        }

        public override void Run(object? arg)
        {
            string prefix1 = string.Concat(AddressPrefix, "3", keyword);
            string prefix2 = string.Concat(AddressPrefix, "1", keyword);
            string suffix1 = keyword;

            byte[] publicKey = new byte[32];
            byte[] tmp = new byte[64];

            addressBuffer.Append(AddressPrefix);
            while (!cancellationToken.IsCancellationRequested)
            {
                random.NextBytes(seedBytes);

                var hasher = Blake2b.CreateIncrementalHasher(32);
                hasher.Update(seedBytes);
                hasher.Update(indexBytes);
                hasher.Finish(secretBytes);

                Chaos.NaCl.Internal.Ed25519Ref10.Ed25519Operations.crypto_public_key(
                    secretBytes, 0, publicKey, 0, tmp);

                Blake2b.ComputeAndWriteHash(5, publicKey, checksumBytes);
                Reverse(checksumBytes);

                NanoBase32(publicKey, ref addressBuffer);
                NanoBase32(checksumBytes, ref addressBuffer);

                if (addressBuffer.StartsWith(prefix1)
                    || addressBuffer.StartsWith(prefix2)
                    || addressBuffer.EndsWith(suffix1))
                {
                    var address = addressBuffer.ToString();
                    FoundSeed = HexUtils.HexFromByteArray(seedBytes);
                    FoundAddress = address;
                    break;
                }

                ++attempts;
                addressBuffer.Length = AddressPrefix.Length;
            }
        }
    }
}