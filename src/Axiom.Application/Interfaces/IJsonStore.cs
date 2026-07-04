namespace Axiom.Application.Interfaces;

/// <summary>
/// Almacén persistente basado en archivos JSON. Proporciona operaciones CRUD
/// simples sobre colecciones de entidades serializadas en disco, emulando un
/// repositorio NoQL ligero. Cada entidad se almacena en un archivo independiente
/// dentro de una estructura de directorios.
/// </summary>
public interface IJsonStore
{
    /// <summary>
    /// Obtiene la ruta base del directorio donde se almacenan los archivos JSON.
    /// </summary>
    string BasePath { get; }

    /// <summary>
    /// Agrega una nueva entrada a la colección especificada. Si el archivo no existe,
    /// se crea automáticamente.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad a almacenar.</typeparam>
    /// <param name="entityName">Nombre de la colección (se usa como nombre de archivo sin extensión).</param>
    /// <param name="entry">Objeto de tipo <typeparamref name="T"/> a agregar.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    Task AppendAsync<T>(string entityName, T entry, CancellationToken ct = default);

    /// <summary>
    /// Lee todas las entradas de una colección.
    /// </summary>
    /// <typeparam name="T">Tipo de las entidades almacenadas.</typeparam>
    /// <param name="entityName">Nombre de la colección a leer.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Lista de objetos de tipo <typeparamref name="T"/> encontrados en la colección.</returns>
    Task<List<T>> ReadAllAsync<T>(string entityName, CancellationToken ct = default);

    /// <summary>
    /// Busca una entrada en la colección que cumpla con el predicado especificado.
    /// </summary>
    /// <typeparam name="T">Tipo de las entidades almacenadas.</typeparam>
    /// <param name="entityName">Nombre de la colección donde buscar.</param>
    /// <param name="predicate">Función de predicado para filtrar la entrada deseada.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>El primer objeto de tipo <typeparamref name="T"/> que cumple el predicado, o <c>null</c> si no se encuentra.</returns>
    Task<T?> FindByIdAsync<T>(string entityName, Func<T, bool> predicate, CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe un archivo de colección para la entidad especificada.
    /// </summary>
    /// <param name="entityName">Nombre de la colección a verificar.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns><c>true</c> si el archivo de la colección existe; de lo contrario, <c>false</c>.</returns>
    Task<bool> ExistsAsync(string entityName, CancellationToken ct = default);

    /// <summary>
    /// Actualiza la primera entrada de la colección que cumpla con el predicado,
    /// reemplazándola por completo con el nuevo objeto proporcionado.
    /// </summary>
    /// <typeparam name="T">Tipo de las entidades almacenadas.</typeparam>
    /// <param name="entityName">Nombre de la colección donde actualizar.</param>
    /// <param name="predicate">Función de predicado para localizar la entrada a actualizar.</param>
    /// <param name="updatedEntry">Objeto de tipo <typeparamref name="T"/> con los datos actualizados.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns><c>true</c> si se encontró y actualizó la entrada; <c>false</c> si no se encontró ninguna coincidencia.</returns>
    Task<bool> UpdateAsync<T>(string entityName, Func<T, bool> predicate, T updatedEntry, CancellationToken ct = default);

    /// <summary>
    /// Elimina la primera entrada de la colección que cumpla con el predicado.
    /// </summary>
    /// <typeparam name="T">Tipo de las entidades almacenadas.</typeparam>
    /// <param name="entityName">Nombre de la colección donde eliminar.</param>
    /// <param name="predicate">Función de predicado para localizar la entrada a eliminar.</param>
    /// <param name="ct">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns><c>true</c> si se encontró y eliminó la entrada; <c>false</c> si no se encontró ninguna coincidencia.</returns>
    Task<bool> DeleteAsync<T>(string entityName, Func<T, bool> predicate, CancellationToken ct = default);
}
