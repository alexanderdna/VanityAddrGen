using Blake2Fast;
using System.Threading;

namespace VanityAddrGen
{
    public sealed class CpuJob : Job
    {
        public CpuJob(Params @params)
            : base(@params)
        {
        }

        public override void Run(object? arg)
        {
            string prefix1 = string.Concat(AddressPrefix, "3", keyword);
            string prefix2 = string.Concat(AddressPrefix, "1", keyword);
            string suffix1 = keyword;

            byte[] seedBytes = new byte[32];
            byte[] secretBytes = new byte[32];
            byte[] indexBytes = new byte[4];
            byte[] publicKeyBytes = new byte[32];
            byte[] checksumBytes = new byte[5];

            byte[] tmp = new byte[64];

            AddressBuffer addressBuffer = new(AddressPrefix.Length + 60);

            bool canMatchPrefix = this.canMatchPrefix;
            bool canMatchSuffix = this.canMatchSuffix;
            CancellationToken cancellationToken = this.cancellationToken;
            System.Action<string, string> resultCallback = this.resultCallback;

            addressBuffer.Append(AddressPrefix);
            while (!cancellationToken.IsCancellationRequested)
            {
                random.NextBytes(seedBytes);

                var hasher = Blake2b.CreateIncrementalHasher(32);
                hasher.Update(seedBytes);
                hasher.Update(indexBytes);
                hasher.Finish(secretBytes);

                Chaos.NaCl.Internal.Ed25519Ref10.Ed25519Operations.crypto_public_key(
                    secretBytes, 0, publicKeyBytes, 0, tmp);

                Blake2b.ComputeAndWriteHash(5, publicKeyBytes, checksumBytes);
                Reverse(checksumBytes);

                NanoBase32(publicKeyBytes, ref addressBuffer);
                NanoBase32(checksumBytes, ref addressBuffer);

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
                        resultCallback.Invoke(HexUtils.HexFromByteArray(seedBytes), address);
                    }
                    else
                    {
                        FoundSeed = HexUtils.HexFromByteArray(seedBytes);
                        FoundAddress = address;
                        break;
                    }
                }

                ++attempts;
                addressBuffer.Length = AddressPrefix.Length;
            }
        }
    }
}