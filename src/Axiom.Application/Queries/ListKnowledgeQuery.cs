using Axiom.Application.Dtos;
using MediatR;

namespace Axiom.Application.Queries;

/// <summary>
///     Consulta para obtener la lista completa de conocimientos registrados en el sistema.
///     No requiere ningún filtro; devuelve todos los elementos de conocimiento disponibles.
/// </summary>
/// <returns>
///     Colección de objetos <see cref="KnowledgeDto"/> que representan cada conocimiento almacenado.
/// </returns>
public record ListKnowledgeQuery : IRequest<IEnumerable<KnowledgeDto>>;
