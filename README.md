# ArchPrecept

Reusable .NET engineering contracts and Roslyn architecture analyzers extracted from
the `dotnet-distributed-architecture-lab` (formerly `dotnet-mq-arch-lab`) and
`ai-collaboration-framework` (formerly `ai-collaboration-prompts-dotnet-backend`)
repositories.

This repository is in its initial extraction stage. Project names, default
assembly names, root namespaces, and the solution use the approved
`ArchPrecept` capability hierarchy. The four initial packages were published
to GitHub Packages as `0.0.1-preview`; see the [preview release guide](docs/releases/preview.md)
for package IDs, compatibility, CI and publication readiness. The
[version policy proposal](docs/releases/version-policy-proposal.md) is deferred until
stable-release planning. Current supported consumers are .NET 8 and 10; .NET 11
will be evaluated after a later release. See the
[License and provenance assessment](docs/releases/licensing-assessment.md) for the completed review and its scope.

All six current packages are published to GitHub Packages as **0.0.2-preview** with public visibility.
CT-005 adds the optional `ArchPrecept.Contracts.Domain.EventSourcing` package.
CT-004 separates `ArchPrecept.Contracts.Messaging` message contracts from the
`ArchPrecept.Contracts.Application.Messaging` publisher port. Transport configuration
belongs to the consuming host; a non-shipping migration example lives under `samples/Messaging/Host`.
See the [candidate changes](docs/releases/0.0.2-preview.md),
[ES contract](docs/product/event-sourcing-contract.md), and
[Messaging boundaries and migration](docs/product/messaging-contract.md).
Domain and Analyzer users do not need either optional capability.
The [release rules and CI/CD guide](docs/releases/cicd-plan.md) define the initial
fast-forward merge to main and main-push build/publication flow for six public packages.

The product was renamed from .NET Engineering Guardrails to ArchPrecept on
2026-10-18. Existing consumers must replace the `EngineeringGuardrails.*` project,
assembly, and namespace prefix with `ArchPrecept.*`, update references, and rebuild.
Diagnostic IDs `DBA1001`–`DBA1017` and rule behavior remain unchanged.

The GitHub repository is now `YuChia-Wei/ArchPrecept`.
The local checkout directory `dotnet-architecture-kit`
is intentionally retained to preserve chat/Codex session continuity and does not
need to match the online repository name.

## Planned Repository Areas

- `src/Contracts/` — optional runtime contract and base-type projects.
- `src/Analyzers/` — Roslyn analyzer projects and configuration examples.
- `src/ArchitectureTesting/` — planned reusable architecture-testing capability.
- `src/Conformance/` — planned repository conformance and reporting capability.
- `tests/<Capability>/` — non-shipping automated verification projects grouped to mirror `src/`.
- `samples/` — package-consumer examples and smoke-test projects.
- `docs/` — product, analyzer Diagnostic manuals, packaging, and future AI Context planning notes.

All buildable, publishable product source belongs under a capability directory in
`src/`. Test and sample projects remain outside `src/` because they are
verification and adoption assets rather than product artifacts.

Only the six implemented product projects are packable. Tests and samples do not
ship. ArchPrecept's own code and documentation are licensed under
[Apache-2.0](LICENSE); dependencies retain their respective licenses.
All six current package IDs are approved for public publication. The initial flow
publishes an unshipped version after main-push CI and verifies public metadata and
registry consumption on .NET 8 and 10. See the [current release record](docs/releases/0.0.2-preview.md)
for actual publication results and the [previous release record](docs/releases/preview.md) for history.

Analyzer Diagnostic 的觸發條件、範例、修正與 suppression 指引，請見
[`docs/diagnostics/README.md`](docs/diagnostics/README.md)。
