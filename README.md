# SECURITY ISSUE NOTICE

THERE IS A **SECURITY ISSUE** WITH VERSIONS PRIOR TO 1.2.

**PLEASE UPGRADE TO VERSION 1.2** OR HIGHER.

IF YOU ARE USING ADDRESSES GENERATED FROM THIS APPLICATION IN VERSIONS PRIOR TO 1.2,
PLEASE IMMEDIATELY TRANSFER YOUR FUNDS TO WALLETS NOT GENERATED FROM THOSE VERSIONS.

# VanityAddrGen

Vanity Address Generator for Nano and Banano

In other words, an application that tries to generate a wallet address
that contains a keyword given by the user.

## Usage

This application doesn't require any command line arguments. Just type in
the keyword you want and press Enter. The application will perform
calculations to find a suitable address.

Configuration is loaded from `config.txt` whose content may look
like the following:

```
# This is configuration file for VanityAddrGen

# cpu OR gpu OR cpu+gpu
hardware=gpu

# prefix OR suffix OR prefix+suffix
match=prefix

# 1 to 8
cpu_threads=1

# 1 to 100000
gpu_threads=100000

# platform index, usually 0 but can be higher
gpu_platform=0

# 0 for stopping when first address is found
# 1 for continously logging found addresses
non_stop=1
```

For `non_stop`, if you set it to 0, the application will stops when it
finds 1 address and will show that address in the window. If you set it
to 1, the application will not stop when finding an address. It will show
the address and append it to a `result-<your keyword>.txt` file.

### Disclaimer

Running this application can be a CPU/GPU intensive task.
By using this application, you agree that the author will not be
held responsible should there be any loss or damages.

## Build

This project uses .NET 5 and can be built with Visual Studio 2019.

### Dependencies

All dependencies are managed in NuGet.

* Cloo.clSharp
* Microsoft.Extensions.ObjectPool
* [Blake2Fast](https://github.com/saucecontrol/Blake2Fast)

## Contribute

This is a hobby project. You can fork it or make pull requests. I will
try to process pull requests but I can't promise.

## Special Thanks

I would like to thank the author(s) of the OpenCL code that I copied from
https://github.com/BananoCoin/banano-vanity. GPU support is impossible
without your code.

## Donate

Banano: `ban_3cintamrbeqg3qspt146cn7cxu69mwpnfdqzdubicki1u1d5exdhbp651cy5`

Nano: `nano_3cintamrbeqg3qspt146cn7cxu69mwpnfdqzdubicki1u1d5exdhbp651cy5`

Thank you!
