# Third party dependency information

ArchPrecept's own code is licensed under Apache-2.0. Dependencies retain their
original licenses; this file does not relicense them.

The four ArchPrecept packages do not bundle third-party DLLs or source files.
The Messaging package depends on Microsoft.Extensions.Configuration.Abstractions
8.0.0 and transitively Microsoft.Extensions.Primitives 8.0.0. NuGet restores those
packages separately; both include MIT license text and their own third-party notices.
Preserve those upstream notices if redistributing the dependencies themselves.

Roslyn, NETStandard.Library and related packages are build dependencies. xUnit and
Microsoft Test Platform are test dependencies. They are not shipped inside these
ArchPrecept nupkg files. In particular, Microsoft.NETCore.Platforms 1.1.0 carries
Microsoft .NET Library terms, not an MIT declaration; its runtime resolution data
is used by the build and is not bundled in the ArchPrecept packages.

Version-specific licenses, upstream notice-file hashes and package hashes are
recorded in docs/releases/dependency-licenses.md and dependency-license-evidence.json.
If distribution is later expanded to vendor dependencies, source code, or tools,
re-evaluate the notices and redistribution terms for that expanded artifact.
