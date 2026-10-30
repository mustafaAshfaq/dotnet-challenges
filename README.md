# .NET Challenges

Standalone, black-box ASP.NET Core practice. The first challenge is HTTP API
contracts; its starter is intentionally incomplete and its tests name the
required effects.

## Setup

Install Git and the .NET 10 SDK or newer. Keep the projects targeting .NET 10.
If you use an older SDK, change the target framework yourself.

From the repository root:

```sh
cd request-validation
dotnet test
```

The starter is expected to fail until invalid create and replace requests are
rejected with a validation problem that names each invalid member and leaves
stored products unchanged. Valid create and replace statuses and a missing read
are also covered by the harness. Reference solutions and hints are published in
the catalog, not in this repository. The maintainer check runs on .NET 10 for
Windows, macOS, and Linux: this directory must fail `dotnet test` and name each
effect, and the catalog reference solution must pass.
