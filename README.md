# dotnet-architecture-kit

Reusable .NET architecture building blocks and Roslyn analyzers extracted from
the `dotnet-mq-arch-lab` and `ai-collaboration-prompts-dotnet-backend`
repositories.

This repository is in its initial extraction stage. The public product name,
NuGet package IDs, package boundaries, version policy, and publication contract
are intentionally not defined yet.

## Planned Repository Areas

- `src/Contracts/` — optional runtime contract and base-type projects.
- `src/Analyzers/` — Roslyn analyzer projects and configuration examples.
- `src/ArchitectureTesting/` — planned reusable architecture-testing capability.
- `src/Conformance/` — planned repository conformance and reporting capability.
- `tests/<Capability>/` — non-shipping automated verification projects grouped to mirror `src/`.
- `samples/` — package-consumer examples and smoke-test projects.
- `docs/` — product, packaging, and future AI Context planning notes.

All buildable, publishable product source belongs under a capability directory in
`src/`. Test and sample projects remain outside `src/` because they are
verification and adoption assets rather than product artifacts.

All projects remain non-packable until the package identity and release contract
are explicitly approved.
