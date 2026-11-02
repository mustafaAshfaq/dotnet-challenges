# .NET Challenges

Standalone, black-box ASP.NET Core practice. Each challenge directory ships a
naive starter and a harness. The starter fails `dotnet test` until that
challenge's effects are present, and the failure names each missing effect.

## Setup

Install Git and the .NET 10 SDK or newer. Keep the projects targeting .NET 10.
If you use an older SDK, change the target framework yourself.

Reference solutions and hints are published in the catalog, not in this
repository. The maintainer check runs on .NET 10 for Windows, macOS, and Linux:
each shipped directory must fail `dotnet test` and name each effect, and the
catalog reference solution must pass.

## HTTP API contracts

From the repository root:

```sh
cd request-validation
dotnet test
```

The starter fails until an invalid create or replace returns a validation
problem that names each invalid member and leaves the stored product unchanged.

## Input trust

From the repository root:

```sh
cd input-trust
dotnet test
```

The starter fails until a JSON create or replace stores the title and keeps
the price at 20, including when the body sends a price or omits it.
