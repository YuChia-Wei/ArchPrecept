# ArchPrecept

Reusable .NET engineering contracts and Roslyn architecture analyzers extracted from
the `dotnet-mq-arch-lab` and `ai-collaboration-prompts-dotnet-backend`
repositories.

This repository is in its initial extraction stage. Project names, default
assembly names, root namespaces, and the solution use the approved
`ArchPrecept` capability hierarchy. NuGet package IDs, package
boundaries, version policy, and publication contract remain intentionally
undefined. Candidate package names are documented in the
[product plan](docs/product/product-plan.md#126-package-id-命名候選).

The product was renamed from .NET Engineering Guardrails to ArchPrecept on
2026-10-18. Existing consumers must replace the `EngineeringGuardrails.*` project,
assembly, and namespace prefix with `ArchPrecept.*`, update references, and rebuild.
Diagnostic IDs `DBA1001`–`DBA1017` and rule behavior remain unchanged.

The GitHub repository currently remains `dotnet-engineering-guardrails`; its rename
will be handled separately. The local checkout directory `dotnet-architecture-kit`
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

All projects remain non-packable until the package identity and release contract
are explicitly approved.

Analyzer Diagnostic 的觸發條件、範例、修正與 suppression 指引，請見
[`docs/diagnostics/README.md`](docs/diagnostics/README.md)。
