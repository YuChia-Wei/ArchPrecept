using System.Threading;
using System.Threading.Tasks;
#if (HasInput)
using UseCaseInput = ExistingInput;
#else
using UseCaseInput = ItemNamespace.ActionNameInput;
#endif
#if (HasOutput)
using UseCaseOutput = ExistingOutput;
#else
using UseCaseOutput = ItemNamespace.ActionNameOutput;
#endif

namespace ItemNamespace;

#if (!HasInput)
public sealed record ActionNameInput(string Value);

#endif
#if (!HasOutput)
public sealed record ActionNameOutput(string Value);

#endif
#if (HasBase)
public interface IActionNameUseCase : UseCaseBase<UseCaseInput, UseCaseOutput>
{
}
#else
public interface IActionNameUseCase
{
    Task<UseCaseOutput> ExecuteAsync(UseCaseInput input, CancellationToken cancellationToken);
}
#endif

public sealed class ActionNameUseCase : IActionNameUseCase
{
#if (HasPort)
    private readonly ExistingPort _port;

    public ActionNameUseCase(ExistingPort port) => _port = port;

#endif
    public Task<UseCaseOutput> ExecuteAsync(UseCaseInput input, CancellationToken cancellationToken)
        => throw new global::System.NotImplementedException("Implement the application behavior.");
}
