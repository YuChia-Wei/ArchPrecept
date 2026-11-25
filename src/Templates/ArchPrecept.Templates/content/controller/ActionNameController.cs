using System.Threading;
using System.Threading.Tasks;
using ApplicationNamespace;
using Microsoft.AspNetCore.Mvc;

namespace ItemNamespace;

public sealed record ActionNameRequest(string Value);
public sealed record ActionNameResponse(string Value);

[ApiController]
[Route("api/[controller]")]
public sealed class ActionNameController(IActionNameUseCase useCase) : ControllerBase
{
    [HttpPost]
    public async Task<ActionNameResponse> Post(ActionNameRequest request, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(new ActionNameInput(request.Value), cancellationToken);
        return new ActionNameResponse(result.Value);
    }
}
