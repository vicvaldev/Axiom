namespace Axiom.Application.Dtos;

/// <summary>
/// DTO genérico para tablas de referencia o catálogos. Se utiliza para
/// representar pares clave-valor (código y nombre) con un identificador
/// numérico, por ejemplo tipos de conocimiento, estados, etc.
/// </summary>
public class ReferenceCodeDto
{
    /// <summary>
    /// Identificador numérico de la entrada de referencia.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Código alfanumérico único que identifica la entrada (p. ej., "DRAFT", "PUBLISHED").
    /// </summary>
    public string Code { get; init; } = null!;

    /// <summary>
    /// Nombre descriptivo visible de la entrada (p. ej., "Borrador", "Publicado").
    /// </summary>
    public string Name { get; init; } = null!;
}
