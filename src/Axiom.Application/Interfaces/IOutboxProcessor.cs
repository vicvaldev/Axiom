namespace Axiom.Application.Interfaces;

/// <summary>
/// Procesador de la bandeja de salida (outbox) para la publicación de eventos
/// e integraciones asíncronas. Lee los mensajes pendientes del almacén de outbox
/// y los envía a los destinos correspondientes (por ejemplo, índices de búsqueda,
/// colas de mensajería, etc.).
/// </summary>
public interface IOutboxProcessor
{
    /// <summary>
    /// Procesa un lote de mensajes pendientes de la bandeja de salida.
    /// Los mensajes se publican en el orden en que fueron creados.
    /// </summary>
    /// <param name="batchSize">Número máximo de mensajes a procesar en esta ejecución. Valor predeterminado: 50.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>El número total de mensajes procesados exitosamente.</returns>
    Task<int> ProcessPendingAsync(int batchSize = 50, CancellationToken ct = default);
}
