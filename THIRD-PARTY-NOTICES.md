# Third party dependency information

ArchPrecept's code and documentation are licensed under Apache-2.0.
Third-party components retain their respective licenses.

ArchPrecept packages contain the project's own assemblies, templates and documentation.
Their runtime package references are limited to other ArchPrecept contracts.
The host configuration example under `samples/Messaging/Host` uses
Microsoft.Extensions.Configuration. NuGet restores sample dependencies separately.

Roslyn and NETStandard.Library are build dependencies. xUnit and Microsoft Test
Platform are test dependencies. ArchPrecept packages exclude those dependencies'
assemblies and source. Microsoft.NETCore.Platforms 1.1.0 carries Microsoft .NET
Library terms for its runtime resolution data.

When redistributing third-party binaries, source code or tools, review the
terms of the versions included and retain their required licenses and notices.
