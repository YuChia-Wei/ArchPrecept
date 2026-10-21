# Samples

`EventSourcing/Program.cs` demonstrates CT-005 replay after derived initialization,
pending events, failed commit validation, successful commit, and replay equivalence.
It uses the optional `ArchPrecept.Contracts.Domain.EventSourcing` package.

Messaging examples cover message JSON round trips (`Messaging/Messages.cs`),
publisher overloads and adapter failures (`Messaging/Publisher.cs`), and
non-shipping host-owned transport configuration (`Messaging/Host/`).
Message consumers need only `ArchPrecept.Contracts.Messaging`; publishers use
`ArchPrecept.Contracts.Application.Messaging`. Host configuration depends on
Microsoft.Extensions.Configuration, which is absent from product package dependencies.

Run all six adoption modes against the actual packed assets with:

```powershell
./eng/Build-Packages.ps1 -OutputDirectory artifacts/contracts-0.0.2-preview
./eng/Test-PackageConsumer.ps1 -SdkMajor 8 -PackageDirectory artifacts/contracts-0.0.2-preview
./eng/Test-PackageConsumer.ps1 -SdkMajor 10 -PackageDirectory artifacts/contracts-0.0.2-preview
```

The consumer script copies the source into an isolated project and restores from
the selected feed with a fresh cache. It also checks analyzer-only and ordinary
contract adoption, plus DBA1001 and DBA1009 from the installed analyzer.
The example simulates append success; it is not an Event Store adapter.
The host example only validates synthetic settings; it never connects to a broker.
See the [ES migration guide](../docs/product/event-sourcing-contract.md) and
[Messaging migration guide](../docs/product/messaging-contract.md).
