using System.Threading;
using System.Threading.Tasks;

namespace ItemNamespace;

public interface QueryPortName : QueryPortBase
{
    Task<ReadModelType?> FindAsync(IdentityType id, CancellationToken cancellationToken);
}
