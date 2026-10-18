# ArchPrecept

Reusable .NET engineering contracts and Roslyn architecture analyzers extracted from
the `dotnet-mq-arch-lab` and `ai-collaboration-prompts-dotnet-backend`
repositories.

This repository is in its initial extraction stage. Project names, default
assembly names, root namespaces, and the solution use the approved
`ArchPrecept` capability hierarchy. The four implemented packages are prepared
as `0.0.1-preview`; see the [preview release guide](docs/releases/preview.md)
for package IDs, compatibility, CI and publication readiness. The
[version policy proposal](docs/releases/version-policy-proposal.md) awaits owner review.

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

Only the four implemented product projects are packable. Tests and samples do not
ship. Package publication is disabled pending License/provenance and access review;
CI artifacts are validation candidates, not published releases.

Analyzer Diagnostic 的觸發條件、範例、修正與 suppression 指引，請見
[`docs/diagnostics/README.md`](docs/diagnostics/README.md)。
