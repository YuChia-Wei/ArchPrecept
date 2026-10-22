using Microsoft.CodeAnalysis;

namespace ArchPrecept.Analyzers.Core;

internal static class RuleDescriptors
{
    internal static readonly DiagnosticDescriptor RepositoryQueryMethod = RuleCatalog.Descriptor("DBA1001");
    internal static readonly DiagnosticDescriptor UseCaseServiceProviderInjection = RuleCatalog.Descriptor("DBA1002");
    internal static readonly DiagnosticDescriptor AggregateInfrastructureDependency = RuleCatalog.Descriptor("DBA1003");
    internal static readonly DiagnosticDescriptor ControllerApiAttribute = RuleCatalog.Descriptor("DBA1004");
    internal static readonly DiagnosticDescriptor ControllerPersistenceAccess = RuleCatalog.Descriptor("DBA1005");
    internal static readonly DiagnosticDescriptor ControllerDirectConstruction = RuleCatalog.Descriptor("DBA1006");
    internal static readonly DiagnosticDescriptor MapperMustBeStatic = RuleCatalog.Descriptor("DBA1007");
    internal static readonly DiagnosticDescriptor MapperForbiddenDependency = RuleCatalog.Descriptor("DBA1008");
    internal static readonly DiagnosticDescriptor EventSourcedAggregateDirectMutation = RuleCatalog.Descriptor("DBA1009");
    internal static readonly DiagnosticDescriptor UseCaseDependencyResolution = RuleCatalog.Descriptor("DBA1010");
    internal static readonly DiagnosticDescriptor MixedCommandQueryHandler = RuleCatalog.Descriptor("DBA1011");
    internal static readonly DiagnosticDescriptor UseCaseDirectRepositoryConstruction = RuleCatalog.Descriptor("DBA1012");
    internal static readonly DiagnosticDescriptor ProjectionWriteOperation = RuleCatalog.Descriptor("DBA1013");
    internal static readonly DiagnosticDescriptor ControllerForbiddenDependency = RuleCatalog.Descriptor("DBA1014");
    internal static readonly DiagnosticDescriptor UseCaseContractShape = RuleCatalog.Descriptor("DBA1015");
    internal static readonly DiagnosticDescriptor UseCaseForbiddenDependency = RuleCatalog.Descriptor("DBA1016");
    internal static readonly DiagnosticDescriptor DispatchHandlerBoundary = RuleCatalog.Descriptor("DBA1017");
    internal static readonly DiagnosticDescriptor ValueObjectMutableState = RuleCatalog.Descriptor("DBA1018");
    internal static readonly DiagnosticDescriptor ValueObjectEntityReference = RuleCatalog.Descriptor("DBA1019");
}
