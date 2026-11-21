# ArchPrecept

ArchPrecept provides independently usable .NET architecture analyzers and optional
reference contracts. It checks selected boundaries and conventions at compile time:
Aggregate and query responsibilities, application and delivery separation, explicit
dependencies, event-sourced state transitions, and Value Object state.

The analyzer works with your own contracts through explicit role mapping. Installing
the reference contracts, an AI tool, or a collaboration framework is optional.
Current supported consumers use .NET 8 or 10.

This source targets **0.0.5-preview**. Install `ArchPrecept.Templates` with `dotnet new install` to create a starter or add a scalar Value Object to an existing project. The starter configures the analyzer for either ArchPrecept reference contracts or a consumer-owned contract. Follow the [starter and Price walkthrough](docs/templates.md).
Start with the [installation guide](docs/getting-started.md),
[package usage](docs/package-usage.md), [rule rationale](docs/product/rule-design-rationale.md),
and [configuration](docs/product/analyzer-policy.md).
See the [0.0.5 release record](docs/releases/0.0.5-preview.md) for publication and
validation status; the version in source alone does not establish feed availability.
The [rule-contract changes](docs/releases/rule-contract-candidate.md) describe the
changes from [0.0.3-preview](docs/releases/0.0.3-preview.md).
For setup instructions, examples and diagnostic fixes, visit the [ArchPrecept documentation site](https://yuchia-wei.github.io/ArchPrecept/). To read the Markdown for this revision, open the [documentation index](docs/README.md).
CT-005 adds the optional `ArchPrecept.Contracts.Domain.EventSourcing` package.
CT-004 separates `ArchPrecept.Contracts.Messaging` message contracts from the
`ArchPrecept.Contracts.Application.Messaging` publisher port. Transport configuration
belongs to the consuming host; a non-shipping configuration example lives under `samples/Messaging/Host`.
See the [0.0.2 release history](docs/releases/0.0.2-preview.md),
[ES contract](docs/product/event-sourcing-contract.md), and
[Messaging boundaries and usage](docs/product/messaging-contract.md).
Domain and Analyzer users do not need either optional capability.
Pull requests run quality checks. After a change reaches `main`, maintainers
publish packages and the documentation site with separate release tags.
See the [release rules and CI/CD guide](docs/releases/cicd-plan.md).

Before the first public release without any prerelease suffix, APIs, rule IDs,
configuration and defaults may change without backward compatibility support.
Use the documentation for the package version you install. Stable-release readiness
requires source-backed rule explanations, complete usage guidance and readable
API XML documentation delivered in the packages; see the
[version and readiness policy](docs/releases/version-policy-proposal.md) and
[stable release and documentation roadmap](docs/releases/stable-release-roadmap.md).

ArchPrecept adopts concrete, sometimes stricter policies from DDD, Clean Architecture,
Hexagonal Architecture and SOLID principles. A diagnostic checks its documented
predicate; a clean build is not proof of complete methodology conformance.

## Planned Repository Areas

- `src/Templates/` — SDK project and item templates for consumers.
- `src/Contracts/` — optional runtime contract and base-type projects.
- `src/Analyzers/` — Roslyn analyzer projects and configuration examples.
- `src/ArchitectureTesting/` — planned reusable architecture-testing capability.
- `src/Conformance/` — planned repository conformance and reporting capability.
- `tests/<Capability>/` — non-shipping automated verification projects grouped to mirror `src/`.
- `samples/` — package-consumer examples and smoke-test projects.
- `docs/` — product, analyzer Diagnostic manuals, packaging and release records.
- `rules/` — the single machine-readable rule catalog and configuration schemas.
- `.dev/` — development guidance and the selected framework adoption boundary.

All buildable, publishable product source belongs under a capability directory in
`src/`. Test and sample projects remain outside `src/` because they are
verification and adoption assets rather than product artifacts.

Only the seven implemented product projects are packable. Tests and samples do not
ship. ArchPrecept's own code and documentation are licensed under
[Apache-2.0](LICENSE); dependencies retain their respective licenses.
The preview targets GitHub Packages. A package release tag must match the
version in `Directory.Build.props` and point to a commit on `main`.
The release builds and tests the packages, then verifies public registry metadata
and consumption on .NET 8 and 10. Documentation tags publish the DocFX site to GitHub Pages.
See the [current release record](docs/releases/0.0.5-preview.md),
[0.0.2 record](docs/releases/0.0.2-preview.md), and [first preview record](docs/releases/preview.md) for evidence.

Analyzer Diagnostic 的觸發條件、範例、修正與 suppression 指引，請見
[`docs/diagnostics/README.md`](docs/diagnostics/README.md)。
