namespace Kcow.Application.Common;

public interface IDatabaseReadiness
{
    Task<bool> IsReadyAsync(CancellationToken cancellationToken);
}
