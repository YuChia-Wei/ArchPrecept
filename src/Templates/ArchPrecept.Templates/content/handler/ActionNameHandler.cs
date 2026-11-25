using System.Threading;
using System.Threading.Tasks;
using ApplicationNamespace;

namespace ItemNamespace;

public sealed record ActionNameCommand(string Value);

public sealed class ActionNameHandler(IActionNameUseCase useCase)
{
    public Task<ActionNameOutput> HandleAsync(ActionNameCommand command, CancellationToken cancellationToken)
        => useCase.ExecuteAsync(new ActionNameInput(command.Value), cancellationToken);
}
