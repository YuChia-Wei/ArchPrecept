using Microsoft.CodeAnalysis;
using Xunit;

namespace ArchPrecept.Analyzers.Core.Tests;

public sealed class ApplicationBoundaryPrecisionTests
{
    private const string Imports = "using System; using System.Threading; using System.Threading.Tasks; ";
    private const string ControllerConfig = """
        <ArchPrecept version="1" profiles="web-api,framework" namingConventions="true">
          <Rule id="DBA1004" enabled="false" />
        </ArchPrecept>
        """;

    [Theory]
    [InlineData("Task", "CancellationToken", "OrderInput", false)]
    [InlineData("ValueTask", "CancellationToken", "OrderInput", true)]
    [InlineData("Task", "CancellationToken", "OrderCommand", true)]
    [InlineData("Task", "CancellationToken", "Guid", false)]
    public async Task Canonical_shape_is_independent_of_transport_attribution(string result, string cancellation, string input, bool invalid)
    {
        var source = Imports + $$"""
            public record OrderInput(); public record OrderCommand();
            public interface IPlaceUseCase { {{result}} ExecuteAsync({{input}} input, {{cancellation}} token); }
            """;
        var diagnostics = await Analyze(source);
        Assert.Equal(invalid ? 1 : 0, diagnostics.Count(d => d.Id == "DBA1015"));
        Assert.DoesNotContain(diagnostics, d => d.GetMessage().Contains("transport input"));
    }

    [Theory]
    [InlineData("IActionResult")]
    [InlineData("MyEnvelope")]
    [InlineData("MyAcknowledgement")]
    public async Task Unrelated_output_names_are_not_transport(string name)
    {
        Assert.Empty(await Analyze(Imports + $$"""
            public class {{name}} { }
            public interface IGetUseCase { Task<{{name}}> ExecuteAsync(CancellationToken token); }
            """));
    }

    [Theory]
    [InlineData("Microsoft.AspNetCore.Http.HttpRequest")]
    [InlineData("System.Collections.Generic.List<Microsoft.AspNetCore.Http.HttpRequest>")]
    public async Task Real_transport_inside_direct_input_member_is_rejected(string memberType)
    {
        var diagnostics = await Analyze(Imports + $$"""
            public record OrderInput({{memberType}} Request);
            public interface IPlaceUseCase { Task ExecuteAsync(OrderInput input, CancellationToken token); }
            """);
        Assert.Contains(diagnostics, d => d.Id == "DBA1015" && d.GetMessage().Contains("transport input"));
    }

    [Fact]
    public async Task Real_http_result_is_rejected_but_domain_command_name_is_only_shape()
    {
        var diagnostics = await Analyze(Imports + """
            public interface IGetUseCase { Task<Microsoft.AspNetCore.Mvc.IActionResult> ExecuteAsync(CancellationToken token); }
            """);
        Assert.Contains(diagnostics, d => d.Id == "DBA1015" && d.GetMessage().Contains("framework output"));
    }

    [Theory]
    [InlineData("Task", "namespace System.Threading.Tasks { public class Task { } }")]
    [InlineData("CancellationToken", "namespace System.Threading { public struct CancellationToken { } }")]
    [InlineData("Guid", "public class Guid { }")]
    public async Task Source_bcl_lookalikes_do_not_satisfy_canonical_contract(string name, string declaration)
    {
        var input = name == "Guid" ? "Guid value, " : "";
        var diagnostics = await Analyze(Imports + declaration + $$"""
            public interface IPlaceUseCase { Task ExecuteAsync({{input}}CancellationToken token); }
            """);
        Assert.Contains(diagnostics, d => d.Id == "DBA1015");
    }

    [Fact]
    public async Task Source_service_provider_lookalike_and_service_locator_name_do_not_trigger()
    {
        Assert.Empty(await Analyze("""
            namespace System { public interface IServiceProvider { } }
            public class LocalServiceLocator { }
            public class LocalHandler(System.IServiceProvider provider, LocalServiceLocator locator) { }
            """));
    }

    [Fact]
    public async Task Generic_inherited_contract_explicit_implementation_and_override_are_effective_operations()
    {
        Assert.Empty(await Analyze(Imports + """
            public record OrderInput();
            public interface IBaseUseCase<TInput> { Task ExecuteAsync(TInput input, CancellationToken token); }
            public interface IOrderUseCase : IBaseUseCase<OrderInput> { }
            public class ExplicitUseCase : IOrderUseCase
            { Task IBaseUseCase<OrderInput>.ExecuteAsync(OrderInput input, CancellationToken token) => Task.CompletedTask; }
            public abstract class BaseUseCase : IOrderUseCase
            { public virtual Task ExecuteAsync(OrderInput input, CancellationToken token) => Task.CompletedTask; }
            public class InheritedUseCase : BaseUseCase { }
            public class OverrideUseCase : BaseUseCase
            { public override Task ExecuteAsync(OrderInput input, CancellationToken token) => Task.CompletedTask; }
            """));
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("int value, ", true)]
    public async Task Same_signature_hiding_is_one_operation_but_another_overload_is_not(string extraParameter, bool invalid)
    {
        var diagnostics = await Analyze(Imports + $$"""
            public class OperationBase
            { public Task ExecuteAsync(CancellationToken token) => Task.CompletedTask; }
            public class LocalUseCase : OperationBase
            { public new Task ExecuteAsync({{extraParameter}}CancellationToken token) => Task.CompletedTask; }
            """);
        Assert.Equal(invalid ? 1 : 0, diagnostics.Count(d => d.Id == "DBA1015"));
    }

    [Theory]
    [InlineData("public void Handle() { }", "", true)]
    [InlineData("private void Handle() { }", "", false)]
    [InlineData("", ", IDispatch", true)]
    public async Task Use_case_handler_entry_check_includes_inherited_and_explicit_contracts(string baseMember, string contract, bool invalid)
    {
        var source = Imports + $$"""
            public interface IDispatch { void HandleAsync(); }
            public class EntryBase { {{baseMember}} }
            public class LocalUseCase : EntryBase{{contract}}
            {
                public Task ExecuteAsync(CancellationToken token) => Task.CompletedTask;
                {{(contract.Length > 0 ? "void IDispatch.HandleAsync() { }" : "")}}
            }
            """;
        var diagnostics = await Analyze(source);
        Assert.Equal(invalid ? 1 : 0, diagnostics.Count(d => d.Id == "DBA1015"));
    }

    [Fact]
    public async Task Actual_injection_attribute_is_detected()
    {
        var diagnostics = await Analyze("""
            public class LocalHandler
            { [Microsoft.AspNetCore.Components.Inject] public object Dependency { get; set; } = new(); }
            """);
        Assert.Single(diagnostics, d => d.Id == "DBA1010");
    }

    [Fact]
    public async Task Explicit_service_locator_role_is_detected()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new UseCaseServiceProviderInjectionAnalyzer(),
            "public interface IResolver { } public class LocalHandler(IResolver resolver) { }",
            """
            <ArchPrecept version="1"><Role name="service-locator" metadataName="IResolver" assembly="AnalyzerTests" /></ArchPrecept>
            """);
        Assert.Single(diagnostics, d => d.Id == "DBA1010");
    }

    [Theory]
    [InlineData("private IMessageBus dependency;")]
    [InlineData("public IMessageBus Dependency { get; set; }")]
    [InlineData("public void Helper(IMessageBus dependency) { }")]
    [InlineData("public LocalUseCase(IMessageBus dependency) { }")]
    public async Task Forbidden_dependencies_cover_declared_fields_properties_and_parameters(string member)
    {
        var diagnostics = await Analyze(Imports + $$"""
            public interface IMessageBus { }
            public class LocalUseCase {
              {{member}}
              public Task ExecuteAsync(CancellationToken token) => Task.CompletedTask;
            }
            """);
        Assert.Single(diagnostics, d => d.Id == "DBA1016");
    }

    [Fact]
    public async Task Direct_method_use_is_detected_without_injected_dependency()
    {
        var diagnostics = await Analyze(Imports + """
            public class OtherUseCase { public Task ExecuteAsync(CancellationToken token) => Task.CompletedTask; }
            public class LocalUseCase {
              public Task ExecuteAsync(CancellationToken token) => new OtherUseCase().ExecuteAsync(token);
            }
            """);
        Assert.Single(diagnostics, d => d.Id == "DBA1016");
    }

    [Theory]
    [InlineData("integration-event-publisher")]
    [InlineData("outbox-port")]
    public async Task Application_owned_port_does_not_launder_mixed_bus(string role)
    {
        const string source = """
            using System.Threading; using System.Threading.Tasks;
            public interface ILocalDispatcher { }
            public interface IExternalBus { }
            public interface IMixed : ILocalDispatcher, IExternalBus { }
            public class LocalUseCase(ILocalDispatcher allowed, IMixed forbidden) {
              public Task ExecuteAsync(CancellationToken token) => Task.CompletedTask;
            }
            """;
        var config = $$"""
            <ArchPrecept version="1">
              <Role name="{{role}}" metadataName="ILocalDispatcher" assembly="AnalyzerTests" />
              <Role name="bus" metadataName="IExternalBus" assembly="AnalyzerTests" />
            </ArchPrecept>
            """;
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new UseCaseServiceProviderInjectionAnalyzer(), source, config);
        var forbidden = Assert.Single(diagnostics, d => d.Id == "DBA1016");
        Assert.Contains("IMixed", forbidden.GetMessage());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Dispatch_policy_counts_distinct_targets_not_execution_count(int targets)
    {
        var dependencies = targets switch { 0 => "", 1 => "IOneUseCase one", _ => "IOneUseCase one, ITwoUseCase two" };
        var calls = targets == 0 ? "" : "if (flag) { one.ExecuteAsync(default); one.ExecuteAsync(default); }";
        var diagnostics = await Analyze(Imports + $$"""
            public interface IOneUseCase { Task ExecuteAsync(CancellationToken token); }
            public interface ITwoUseCase { Task ExecuteAsync(CancellationToken token); }
            public class LocalCommandHandler({{dependencies}}) { public void Handle(bool flag) { {{calls}} } }
            """);
        Assert.Equal(targets == 1 ? 0 : 1, diagnostics.Count(d => d.Id == "DBA1017"));
    }

    [Fact]
    public async Task Target_typed_repository_and_use_case_construction_are_detected()
    {
        var useCase = await Analyze("public class SqlRepository { } public class LocalHandler { public SqlRepository Make() => new(); }");
        Assert.Single(useCase, d => d.Id == "DBA1012");
        var controller = await AnalyzerTestHelper.GetDiagnosticsAsync(new ControllerComplianceAnalyzer(),
            "public class LocalUseCase { } public class LocalController { public LocalUseCase Make() => new(); }", ControllerConfig);
        Assert.Single(controller, d => d.Id == "DBA1006");
    }

    [Theory]
    [InlineData("[Microsoft.AspNetCore.Mvc.ApiController] public class LocalController { }")]
    [InlineData("[Microsoft.AspNetCore.Mvc.ApiController] public abstract class Base : Microsoft.AspNetCore.Mvc.ControllerBase { } public class LocalController : Base { }")]
    [InlineData("[assembly: Microsoft.AspNetCore.Mvc.ApiController] public class LocalController { }")]
    public async Task Actual_api_attribute_applies_directly_through_base_or_assembly(string source)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new ControllerComplianceAnalyzer(), source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Same_named_attribute_does_not_satisfy_api_policy()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new ControllerComplianceAnalyzer(),
            "public class ApiControllerAttribute : System.Attribute { } [ApiController] public class LocalController { }");
        Assert.Single(diagnostics, d => d.Id == "DBA1004");
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public async Task Query_exception_requires_explicit_selection_and_actual_query_only_port(bool selected, bool mixed, bool invalid)
    {
        var config = ControllerConfig.Replace("</ArchPrecept>", selected
            ? "<PureQuery type=\"LocalController\" assembly=\"AnalyzerTests\" enabled=\"true\" /></ArchPrecept>" : "</ArchPrecept>");
        var source = $$"""
            public interface IQueryRepository { }
            public interface IRead : IQueryRepository { int Read(); {{(mixed ? "void Save();" : "")}} }
            public class LocalController(IRead query) { }
            """;
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new ControllerComplianceAnalyzer(), source, config);
        Assert.Equal(invalid ? 1 : 0, diagnostics.Count(d => d.Id == "DBA1014"));
    }

    [Theory]
    [InlineData("integration-event-publisher", "inherited")]
    [InlineData("integration-event-publisher", "method")]
    [InlineData("integration-event-publisher", "property")]
    [InlineData("outbox-port", "inherited")]
    [InlineData("outbox-port", "method")]
    [InlineData("outbox-port", "property")]
    public async Task Pure_query_cannot_expose_application_publication_capabilities(string role, string exposure)
    {
        var config = ControllerConfig.Replace("</ArchPrecept>", $$"""
            <Role name="query-service" metadataName="IRead" assembly="AnalyzerTests" />
            <Role name="{{role}}" metadataName="ICapability" assembly="AnalyzerTests" />
            <PureQuery type="LocalController" assembly="AnalyzerTests" enabled="true" />
            </ArchPrecept>
            """);
        var source = $$"""
            public interface ICapability { System.Threading.Tasks.Task PublishAsync(); }
            public interface IRead {{(exposure == "inherited" ? ": ICapability" : "")}}
            {
                int Read();
                {{(exposure == "method" ? "System.Threading.Tasks.Task<ICapability[]> GetCapabilities();" : "")}}
                {{(exposure == "property" ? "ICapability Publisher { get; }" : "")}}
            }
            public class LocalController(IRead reader) { }
            """;
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new ControllerComplianceAnalyzer(), source, config);
        Assert.Single(diagnostics, d => d.Id == "DBA1014");
    }

    [Theory]
    [InlineData("<PureQuery type=\"Missing\" assembly=\"AnalyzerTests\" enabled=\"true\" />")]
    [InlineData("<PureQuery type=\"LocalController\" assembly=\"Wrong\" enabled=\"true\" />")]
    [InlineData("<PureQuery type=\"LocalController\" assembly=\"AnalyzerTests\" enabled=\"maybe\" />")]
    [InlineData("<PureQuery type=\"LocalController\" assembly=\"AnalyzerTests\" enabled=\"true\" /><PureQuery type=\"LocalController\" assembly=\"AnalyzerTests\" enabled=\"false\" />")]
    public async Task Invalid_pure_query_selection_fails_explicitly(string selection)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new ConfigurationCoverageAnalyzer(),
            "public class LocalController { }", "<ArchPrecept version=\"1\">" + selection + "</ArchPrecept>");
        Assert.Contains(diagnostics, d => d.Id == "DBA0001");
    }

    [Fact]
    public async Task Project_can_revoke_repository_pure_query_selection()
    {
        var source = "public interface IQueryRepository { } public class LocalController(IQueryRepository query) { }";
        var repository = ControllerConfig.Replace("</ArchPrecept>", "<PureQuery type=\"LocalController\" assembly=\"AnalyzerTests\" enabled=\"true\" /></ArchPrecept>");
        var project = "<ArchPrecept version=\"1\"><PureQuery type=\"LocalController\" assembly=\"AnalyzerTests\" enabled=\"false\" /></ArchPrecept>";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new ControllerComplianceAnalyzer(), source, repository, project);
        Assert.Single(diagnostics, d => d.Id == "DBA1014");
    }

    private static Task<System.Collections.Immutable.ImmutableArray<Diagnostic>> Analyze(string source) =>
        AnalyzerTestHelper.GetDiagnosticsAsync(new UseCaseServiceProviderInjectionAnalyzer(), source);
}
