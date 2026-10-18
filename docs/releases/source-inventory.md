# C# 檔案來源盤點

基線：baeb302，2026-10-18。以下只證明本地 Git 可追到的位置；所有檔案的上游 commit、原始 path、權利人及授權依據仍待核對。初次 import 不是原創證明；停在改名 commit 的檔案還需回接。

| 現行檔案 | git log --follow 最早紀錄 | 上游來源／授權結案 |
|---|---|---|
| `src/Analyzers/ArchPrecept.Analyzers.Core/AggregateInfrastructureDependencyAnalyzer.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Analyzers/ArchPrecept.Analyzers.Core/ControllerComplianceAnalyzer.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Analyzers/ArchPrecept.Analyzers.Core/DiagnosticCategories.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Analyzers/ArchPrecept.Analyzers.Core/EventSourcedAggregateMutationAnalyzer.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Analyzers/ArchPrecept.Analyzers.Core/MapperComplianceAnalyzer.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Analyzers/ArchPrecept.Analyzers.Core/ProjectionReadOnlyAnalyzer.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Analyzers/ArchPrecept.Analyzers.Core/RepositoryQueryMethodAnalyzer.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Analyzers/ArchPrecept.Analyzers.Core/RuleDescriptors.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Analyzers/ArchPrecept.Analyzers.Core/UseCaseServiceProviderInjectionAnalyzer.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Application/IAggregateRepository.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Application/IDomainEventDispatcher.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Application/IQueryRepository.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Application/PageResult.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Application/Result.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Domain/AggregateRoot.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Domain/DomainEntity.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Domain/EsAggregateRoot.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Domain/IAggregateRoot.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Domain/IDomainEntity.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Domain/IDomainEvent.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Domain/ValueObject.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Messaging.Abstractions/Configuration/MessagingTransportOptions.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Messaging.Abstractions/IIntegrationEvent.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Messaging.Abstractions/IIntegrationEventPublisher.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Messaging.Abstractions/IRequestContract.cs` | `d7e452b 2026-08-09 refactor(repo): adopt EngineeringGuardrails identities` | 待核對 |
| `src/Contracts/ArchPrecept.Contracts.Messaging.Abstractions/IntegrationMessageDelivery.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `tests/Analyzers/ArchPrecept.Analyzers.Core.Tests/AggregateInfrastructureDependencyAnalyzerTests.cs` | `d7e452b 2026-08-09 refactor(repo): adopt EngineeringGuardrails identities` | 待核對 |
| `tests/Analyzers/ArchPrecept.Analyzers.Core.Tests/AnalyzerTestHelper.cs` | `133ecc0 2026-07-27 feat(core): import building blocks and analyzers` | 待核對 |
| `tests/Analyzers/ArchPrecept.Analyzers.Core.Tests/ControllerComplianceAnalyzerTests.cs` | `d7e452b 2026-08-09 refactor(repo): adopt EngineeringGuardrails identities` | 待核對 |
| `tests/Analyzers/ArchPrecept.Analyzers.Core.Tests/DiagnosticDocumentationCoverageTests.cs` | `d0d23e9 2026-08-10 docs: add analyzer manuals and zh-TW XML summaries` | 待核對 |
| `tests/Analyzers/ArchPrecept.Analyzers.Core.Tests/EventSourcedAggregateMutationAnalyzerTests.cs` | `d7e452b 2026-08-09 refactor(repo): adopt EngineeringGuardrails identities` | 待核對 |
| `tests/Analyzers/ArchPrecept.Analyzers.Core.Tests/MapperComplianceAnalyzerTests.cs` | `d7e452b 2026-08-09 refactor(repo): adopt EngineeringGuardrails identities` | 待核對 |
| `tests/Analyzers/ArchPrecept.Analyzers.Core.Tests/ProjectionReadOnlyAnalyzerTests.cs` | `d7e452b 2026-08-09 refactor(repo): adopt EngineeringGuardrails identities` | 待核對 |
| `tests/Analyzers/ArchPrecept.Analyzers.Core.Tests/RepositoryQueryMethodAnalyzerTests.cs` | `d7e452b 2026-08-09 refactor(repo): adopt EngineeringGuardrails identities` | 待核對 |
| `tests/Analyzers/ArchPrecept.Analyzers.Core.Tests/UseCaseServiceProviderInjectionAnalyzerTests.cs` | `d7e452b 2026-08-09 refactor(repo): adopt EngineeringGuardrails identities` | 待核對 |
| `tests/Analyzers/ArchPrecept.Analyzers.Core.Tests/XmlDocumentationCoverageTests.cs` | `d0d23e9 2026-08-10 docs: add analyzer manuals and zh-TW XML summaries` | 待核對 |

另須核對非 C# 資產：17 份 docs/diagnostics/DBA*.md、產品文件與 README、Analyzer README、templates/Directory.Build.props.snippet、templates/analyzer-severity.editorconfig、samples/README.md，以及此次新增的 eng/ 與 workflows。此表不宣稱涵蓋所有非程式文字的來源。
