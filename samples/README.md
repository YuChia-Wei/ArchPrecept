# Samples

[Products Web API](Products/README.md) is the small create/read flow using actual
NuGet contracts/analyzers, a process-local repository, HTTP/use-case tests and
isolated ValueObject violations. It has no DB, broker or AI dependency.

`EventSourcing/Program.cs` demonstrates CT-005 replay after derived initialization,
pending events, failed commit validation, successful commit, and replay equivalence.
It uses the optional `ArchPrecept.Contracts.Domain.EventSourcing` package.

Messaging examples cover message JSON round trips (`Messaging/Messages.cs`),
publisher overloads and adapter failures (`Messaging/Publisher.cs`), and
non-shipping host-owned transport configuration (`Messaging/Host/`).
Message consumers need only `ArchPrecept.Contracts.Messaging`; publishers use
`ArchPrecept.Contracts.Application.Messaging`. Host configuration depends on
Microsoft.Extensions.Configuration, which is absent from product package dependencies.

Run all seven adoption modes, the rule/profile matrix and Products tests against packed assets:

```powershell
./eng/Build-Packages.ps1 -OutputDirectory artifacts/packages
./eng/Test-PackageConsumer.ps1 -SdkMajor 8 -PackageDirectory artifacts/packages
./eng/Test-PackageConsumer.ps1 -SdkMajor 10 -PackageDirectory artifacts/packages
```

The consumer script copies the source into an isolated project and restores from
the selected feed with a fresh cache. It also checks analyzer-only and ordinary
contract adoption, all nineteen rule IDs/severities, invalid settings, declared
coverage and generated-code exclusions from the installed analyzer. On Windows,
use a verified F: task directory through -ScratchRoot and an F: output directory.
The example simulates append success; it is not an Event Store adapter.
The host example only validates synthetic settings; it never connects to a broker.
See the [ES migration guide](../docs/product/event-sourcing-contract.md) and
[Messaging migration guide](../docs/product/messaging-contract.md).
