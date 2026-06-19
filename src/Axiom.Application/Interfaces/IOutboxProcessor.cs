namespace Axiom.Application.Interfaces;

public interface IOutboxProcessor
{
    Task<int> ProcessPendingAsync(int batchSize = 50, CancellationToken ct = default);
}
