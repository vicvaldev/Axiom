using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Controlador (handler) para la consulta <see cref="GetKnowledgeByIdQuery"/>.
/// Obtiene un conocimiento (<see cref="Knowledge"/>) a partir de su identificador único.
/// </summary>
public class GetKnowledgeByIdHandler : IRequestHandler<GetKnowledgeByIdQuery, Knowledge?>
{
    private readonly IKnowledgeRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia del handler con el repositorio de conocimientos especificado.
    /// </summary>
    /// <param name="repository">Repositorio de conocimientos <see cref="IKnowledgeRepository"/> utilizado para acceder a los datos.</param>
    public GetKnowledgeByIdHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja la consulta <see cref="GetKnowledgeByIdQuery"/> y devuelve el conocimiento correspondiente al identificador proporcionado.
    /// </summary>
    /// <param name="request">Consulta que contiene el identificador único (<see cref="GetKnowledgeByIdQuery.Id"/>) del conocimiento a recuperar.</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asincrónica.</param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica. El resultado contiene el objeto <see cref="Knowledge"/>
    /// si se encuentra; de lo contrario, <c>null</c>.
    /// </returns>
    public async Task<Knowledge?> Handle(GetKnowledgeByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}
