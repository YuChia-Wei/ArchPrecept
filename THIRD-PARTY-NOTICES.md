# Third party dependency information

ArchPrecept's own code is licensed under Apache-2.0. Dependencies retain their
original licenses; this file does not relicense them.

The six current ArchPrecept packages do not bundle third-party DLLs or source files.
Their runtime package references are limited to other ArchPrecept contracts.
The historical 0.0.1-preview Messaging.Abstractions package depended on
Microsoft.Extensions.Configuration.Abstractions 8.0.0; the current Messaging
contracts no longer have that dependency. Configuration is now a non-shipping,
host-owned example under samples/Messaging/Host, tested using
Microsoft.Extensions.Configuration 8.0.0 and its transitive dependencies.
NuGet restores sample dependencies separately. Preserve upstream licenses and
notices if redistributing those dependencies themselves.

Roslyn, NETStandard.Library and related packages are build dependencies. xUnit and
Microsoft Test Platform are test dependencies. They are not shipped inside these
ArchPrecept nupkg files. In particular, Microsoft.NETCore.Platforms 1.1.0 carries
Microsoft .NET Library terms, not an MIT declaration; its runtime resolution data
is used by the build and is not bundled in the ArchPrecept packages.

Version-specific licenses, upstream notice-file hashes and package hashes are
recorded for the initial release in docs/releases/dependency-licenses.md and
dependency-license-evidence.json; those files are historical evidence, not a current lockfile.
If distribution is later expanded to vendor dependencies, source code, or tools,
re-evaluate the notices and redistribution terms for that expanded artifact.
