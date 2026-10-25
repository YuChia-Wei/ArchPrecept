# ArchPrecept

ArchPrecept provides independently usable .NET architecture analyzers and optional
reference contracts. It checks selected boundaries and conventions at compile time:
Aggregate and query responsibilities, application and delivery separation, explicit
dependencies, event-sourced state transitions, and Value Object state.

The analyzer works with your own contracts through explicit role mapping. Installing
the reference contracts, an AI tool, or a collaboration framework is optional.
Current supported consumers use .NET 8 or 10.

The current **0.0.3-preview** release contains six public GitHub Packages.
It adds selectable rule profiles, shared role mapping/evidence,
the domain-event dispatcher correction, two opt-in ValueObject rules and a Products API sample.
Start with the [installation and usage guide](docs/getting-started.md),
[rule design rationale](docs/product/rule-design-rationale.md),
[methodology references](docs/product/methodology-references.md), and
[analyzer configuration](docs/product/analyzer-policy.md).
The [0.0.3 release record](docs/releases/0.0.3-preview.md) identifies the published
package and validation evidence; documentation of a target policy does not imply
that every part is already enforced.
CT-005 adds the optional `ArchPrecept.Contracts.Domain.EventSourcing` package.
CT-004 separates `ArchPrecept.Contracts.Messaging` message contracts from the
`ArchPrecept.Contracts.Application.Messaging` publisher port. Transport configuration
belongs to the consuming host; a non-shipping configuration example lives under `samples/Messaging/Host`.
See the [0.0.2 release history](docs/releases/0.0.2-preview.md),
[ES contract](docs/product/event-sourcing-contract.md), and
[Messaging boundaries and usage](docs/product/messaging-contract.md).
Domain and Analyzer users do not need either optional capability.
The [release rules and CI/CD guide](docs/releases/cicd-plan.md) define the initial
fast-forward merge to main and main-push build/publication flow for six public packages.

Before the first public release without a `-preview` suffix, APIs, rule IDs,
configuration and defaults may change without backward compatibility support.
Use the documentation for the package version you install. Stable-release readiness
requires source-backed rule explanations, complete usage guidance and readable
API XML documentation delivered in the packages; see the
[version and readiness policy](docs/releases/version-policy-proposal.md).

ArchPrecept adopts concrete, sometimes stricter policies from DDD, Clean Architecture,
Hexagonal Architecture and SOLID principles. A diagnostic checks its documented
predicate; a clean build is not proof of complete methodology conformance.

## Planned Repository Areas

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

Only the six implemented product projects are packable. Tests and samples do not
ship. ArchPrecept's own code and documentation are licensed under
[Apache-2.0](LICENSE); dependencies retain their respective licenses.
All six current package IDs are approved for public publication. The initial flow
publishes an unshipped version after main-push CI and verifies public metadata and
registry consumption on .NET 8 and 10. See the [current release record](docs/releases/0.0.3-preview.md),
[0.0.2 record](docs/releases/0.0.2-preview.md), and [first preview record](docs/releases/preview.md) for evidence.

Analyzer Diagnostic 的觸發條件、範例、修正與 suppression 指引，請見
[`docs/diagnostics/README.md`](docs/diagnostics/README.md)。
