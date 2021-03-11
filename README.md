# VanityAddrGen

Vanity Address Generator for Nano and Banano

In other words, an application that tries to generate a wallet address
that contains a keyword given by the user.

## Usage

This is an interactive console application. It doesn't require any
command line arguments.

It will ask you to enter the number of CPU threads to use, then the
keyword you want to appear in the address. After that it will run
until an address is found. Every second it will report how many
attempts it has made to find an address.

If an address is found, the keyword will appear at the beginning or
the end of it. The application currently doesn't support having a
keyword in the middle of the address, for practical reasons.

The result will be the seed and a Nano address. Since Nano and Banano
have the same architecture, you can use the seed for both coins.

## Build

This project uses .NET 5 and can be built with Visual Studio 2019.

### Dependencies

All dependencies are managed in NuGet.

* Microsoft.Extensions.ObjectPool
* [Blake2Fast](https://github.com/saucecontrol/Blake2Fast)

## Contribute

This is a hobby project. You can fork it or make pull requests. I will
try to process pull requests but I can't promise.

## Donate

You don't have to. But if you do, I really appreciate that and will pay it forward.

Banano: `ban_3ijd3yp7rhzgxsrf3oj1ipc57hkznstbmcadfcxbi4uef36p1o6tbococoo5`

Nano: `nano_3ijd3yp7rhzgxsrf3oj1ipc57hkznstbmcadfcxbi4uef36p1o6tbococoo5`
