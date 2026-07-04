namespace Axiom.Application.Dtos;

/// <summary>
/// DTO que encapsula el resultado de la operación de sembrado (seed) de datos
/// de demostración. Cada propiedad indica cuántas entidades de cada tipo se
/// insertaron durante el proceso.
/// </summary>
public class DemoSeedResultDto
{
    /// <summary>
    /// Número de usuarios insertados.
    /// </summary>
    public int Users { get; init; }

    /// <summary>
    /// Número de sistemas insertados.
    /// </summary>
    public int Systems { get; init; }

    /// <summary>
    /// Número de tipos de conocimiento insertados.
    /// </summary>
    public int KnowledgeTypes { get; init; }

    /// <summary>
    /// Número de estados de issue insertados.
    /// </summary>
    public int IssueStates { get; init; }

    /// <summary>
    /// Número de estados de conocimiento insertados.
    /// </summary>
    public int KnowledgeStates { get; init; }

    /// <summary>
    /// Número de issues insertados.
    /// </summary>
    public int Issues { get; init; }

    /// <summary>
    /// Número de conocimientos insertados.
    /// </summary>
    public int Knowledges { get; init; }
}
