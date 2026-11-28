# ArchPrecept

ArchPrecept provides a standalone .NET architecture analyzer and optional reference contracts. The analyzer checks aggregate and query responsibilities, application and delivery boundaries, explicit dependencies, event-sourced state transitions, and value object state at compile time. Explicit role mappings let you use your own interfaces and base types. Consumer projects can use .NET SDK 8 or 10. [Platform support and version policy](docs/releases/version-policy.md)

This source targets the **0.1.0-preview.1 release candidate**. Check package availability separately for NuGet.org and GitHub Packages, and use the documentation for the version you install. [Release candidate and installation](docs/releases/0.1.0-preview.1.md) · [Documentation and version index (Traditional Chinese)](docs/README.md)

## Getting started

- Install the analyzer, map roles in an existing project, or choose optional reference contracts. [Installation](docs/getting-started.md) · [Package usage](docs/package-usage.md) · [Analyzer configuration](docs/product/analyzer-policy.md)
- Generate code with starter and item templates, or use xUnit to check a direct assembly dependency. [Templates](docs/templates.md) · [Architecture testing](docs/architecture-testing.md)
- Look up a `DBA` diagnostic for its trigger conditions, passing and failing examples, fixes, and suppression guidance. [Diagnostic index](docs/diagnostics/README.md)
- Learn how to replay events and confirm commits, or use message contracts and the application publisher port with your own transport. [Event Sourcing contract](docs/product/event-sourcing-contract.md) · [Messaging contract](docs/product/messaging-contract.md)

The documentation site includes installation guides, usage examples, and diagnostic help. [ArchPrecept documentation](https://yuchia-wei.github.io/ArchPrecept/)

## Product scope and source layout

ArchPrecept uses specific rules informed by DDD, Clean Architecture, Hexagonal Architecture, and SOLID. Diagnostic results cover conditions the enabled rules can evaluate in the current compilation. [Product scope and direction](docs/product/README.md) · [Rule design rationale](docs/product/rule-design-rationale.md)

| Path | Purpose |
|---|---|
| `src/Analyzers/`, `src/Contracts/`, `src/Templates/` | Analyzer, reference contracts, and SDK templates |
| `tests/`, `samples/` | Automated tests and consumer examples |
| `docs/`, `rules/` | Product documentation, release notes, machine-readable rule catalog, and configuration schema |
| `.dev/` | Product requirements and repository development policies |

The seven packages share a version and can be adopted individually. ArchPrecept's code and documentation are licensed under Apache-2.0. Third-party dependencies retain their respective licenses. [LICENSE](LICENSE) · [NOTICE](NOTICE)
