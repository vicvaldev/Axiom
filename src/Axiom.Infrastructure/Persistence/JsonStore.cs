using System.Collections.Concurrent;
using System.Text.Json;
using Axiom.Application.Interfaces;

namespace Axiom.Infrastructure.Persistence;

/// <summary>
/// Almacenamiento local basado en archivos JSON para persistencia simple de entidades.
/// Cada tipo de entidad se guarda en un archivo independiente dentro de un directorio base.
/// Las operaciones de escritura (append, update, delete) están protegidas con semáforos
/// por nombre de entidad para garantizar exclusión mutua entre operaciones concurrentes.
/// </summary>
/// <typeparam name="T">Tipo genérico de la entidad a serializar/deserializar.</typeparam>
public class JsonStore : IJsonStore
{
    private readonly string _basePath;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Inicializa el almacén en el directorio <c>~/.axiom/data</c>.
    /// Crea el directorio si no existe.
    /// </summary>
    public JsonStore()
    {
        _basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".axiom", "data");
        Directory.CreateDirectory(_basePath);
    }

    /// <summary>
    /// Inicializa el almacén en la ruta especificada.
    /// Crea el directorio si no existe.
    /// </summary>
    /// <param name="basePath">Ruta absoluta o relativa del directorio donde se almacenarán los archivos JSON.</param>
    public JsonStore(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(_basePath);
    }

    /// <summary>
    /// Obtiene la ruta base del directorio donde se guardan los archivos JSON.
    /// </summary>
    public string BasePath => _basePath;

    /// <summary>
    /// Agrega una nueva entrada al archivo JSON de la entidad especificada.
    /// Si el archivo no existe, lo crea; si existe, lee el contenido existente,
    /// agrega la nueva entrada y sobrescribe el archivo.
    /// La operación está protegida por un semáforo por nombre de entidad.
    /// </summary>
    /// <typeparam name="T">Tipo de la entrada a agregar.</typeparam>
    /// <param name="entityName">Nombre de la entidad que determina el nombre del archivo (ej. "users" → users.json).</param>
    /// <param name="entry">Objeto a agregar a la colección.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>Una tarea que representa la operación asincrónica de agregado.</returns>
    public async Task AppendAsync<T>(string entityName, T entry, CancellationToken ct = default)
    {
        var semaphore = _locks.GetOrAdd(entityName, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        try
        {
            var filePath = GetFilePath(entityName);
            List<T> entries;

            if (File.Exists(filePath))
            {
                var existingJson = await File.ReadAllTextAsync(filePath, ct);
                entries = JsonSerializer.Deserialize<List<T>>(existingJson, JsonOptions) ?? [];
            }
            else
            {
                entries = [];
            }

            entries.Add(entry);
            var json = JsonSerializer.Serialize(entries, JsonOptions);
            await File.WriteAllTextAsync(filePath, json, ct);
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// Lee todas las entradas del archivo JSON correspondiente al nombre de entidad especificado.
    /// Si el archivo no existe, retorna una lista vacía.
    /// </summary>
    /// <typeparam name="T">Tipo de las entradas a deserializar.</typeparam>
    /// <param name="entityName">Nombre de la entidad que determina el archivo a leer.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>Lista de entradas deserializadas; lista vacía si el archivo no existe.</returns>
    public async Task<List<T>> ReadAllAsync<T>(string entityName, CancellationToken ct = default)
    {
        var filePath = GetFilePath(entityName);
        if (!File.Exists(filePath))
            return [];

        var json = await File.ReadAllTextAsync(filePath, ct);
        return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? [];
    }

    /// <summary>
    /// Busca la primera entrada que cumpla con el predicado especificado.
    /// </summary>
    /// <typeparam name="T">Tipo de la entrada a buscar.</typeparam>
    /// <param name="entityName">Nombre de la entidad sobre la cual realizar la búsqueda.</param>
    /// <param name="predicate">Función de predicado para filtrar la entrada deseada.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>
    /// La primera entrada que satisface el predicado; <c>null</c> si no se encuentra ninguna.
    /// </returns>
    public async Task<T?> FindByIdAsync<T>(string entityName, Func<T, bool> predicate, CancellationToken ct = default)
    {
        var entries = await ReadAllAsync<T>(entityName, ct);
        return entries.FirstOrDefault(predicate);
    }

    /// <summary>
    /// Verifica si existe al menos una entrada en el archivo JSON de la entidad especificada.
    /// </summary>
    /// <param name="entityName">Nombre de la entidad a verificar.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>
    /// <c>true</c> si el archivo existe y contiene al menos una entrada; <c>false</c> en caso contrario.
    /// </returns>
    public async Task<bool> ExistsAsync(string entityName, CancellationToken ct = default)
    {
        var filePath = GetFilePath(entityName);
        return File.Exists(filePath) && (await ReadAllAsync<object>(entityName, ct)).Count > 0;
    }

    /// <summary>
    /// Actualiza la primera entrada que cumpla con el predicado, reemplazándola por el nuevo valor.
    /// La operación está protegida por un semáforo por nombre de entidad.
    /// </summary>
    /// <typeparam name="T">Tipo de la entrada a actualizar.</typeparam>
    /// <param name="entityName">Nombre de la entidad sobre la cual realizar la actualización.</param>
    /// <param name="predicate">Función de predicado para localizar la entrada a actualizar.</param>
    /// <param name="updatedEntry">Nuevo valor que reemplazará a la entrada existente.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>
    /// <c>true</c> si se encontró y actualizó una entrada; <c>false</c> si ninguna entrada cumple el predicado.
    /// </returns>
    public async Task<bool> UpdateAsync<T>(string entityName, Func<T, bool> predicate, T updatedEntry, CancellationToken ct = default)
    {
        var semaphore = _locks.GetOrAdd(entityName, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        try
        {
            var entries = await ReadAllAsync<T>(entityName, ct);
            var index = entries.FindIndex(e => predicate(e));
            if (index < 0)
                return false;

            entries[index] = updatedEntry;
            var filePath = GetFilePath(entityName);
            var json = JsonSerializer.Serialize(entries, JsonOptions);
            await File.WriteAllTextAsync(filePath, json, ct);
            return true;
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// Elimina la primera entrada que cumpla con el predicado.
    /// La operación está protegida por un semáforo por nombre de entidad.
    /// </summary>
    /// <typeparam name="T">Tipo de la entrada a eliminar.</typeparam>
    /// <param name="entityName">Nombre de la entidad sobre la cual realizar la eliminación.</param>
    /// <param name="predicate">Función de predicado para localizar la entrada a eliminar.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asincrónica.</param>
    /// <returns>
    /// <c>true</c> si se encontró y eliminó una entrada; <c>false</c> si ninguna entrada cumple el predicado.
    /// </returns>
    public async Task<bool> DeleteAsync<T>(string entityName, Func<T, bool> predicate, CancellationToken ct = default)
    {
        var semaphore = _locks.GetOrAdd(entityName, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        try
        {
            var entries = await ReadAllAsync<T>(entityName, ct);
            var index = entries.FindIndex(e => predicate(e));
            if (index < 0)
                return false;

            entries.RemoveAt(index);
            var filePath = GetFilePath(entityName);
            var json = JsonSerializer.Serialize(entries, JsonOptions);
            await File.WriteAllTextAsync(filePath, json, ct);
            return true;
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// Construye la ruta completa del archivo JSON para una entidad.
    /// El archivo se nombra con el nombre de la entidad en minúsculas y extensión <c>.json</c>.
    /// </summary>
    /// <param name="entityName">Nombre de la entidad.</param>
    /// <returns>Ruta completa del archivo JSON.</returns>
    private string GetFilePath(string entityName)
    {
        return Path.Combine(_basePath, $"{entityName.ToLowerInvariant()}.json");
    }
}
