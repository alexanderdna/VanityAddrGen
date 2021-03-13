# VanityAddrGen

Vanity Address Generator for Nano and Banano

In other words, an application that tries to generate a wallet address
that contains a keyword given by the user.

## Usage

This is an interactive console application. It doesn't require any
command line arguments.

It will ask you to enter the number of CPU/GPU threads to use, then the
keyword you want to appear in the address. After that it will run
until an address is found. Every second it will report how many
attempts it has made to find an address.

If an address is found, the keyword will appear at the beginning or
the end of it. The application currently doesn't support having a
keyword in the middle of the address, for practical reasons.

The result will be the seed and a Banano address. Since Nano and Banano
have the same architecture, you can use the seed for both coins.

Disclaimer: running this application can be a CPU intensive task.
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

You don't have to. But if you do, I really appreciate that and will pay it forward.

Banano: `ban_3cintamnxp58wmqkto8d84ph6cbchsmera3ykc36qsdptqgcbfb64cwi4crf`

Nano: `nano_3cintamnxp58wmqkto8d84ph6cbchsmera3ykc36qsdptqgcbfb64cwi4crf`
