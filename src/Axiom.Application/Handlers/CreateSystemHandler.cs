using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Manejador del comando <see cref="CreateSystemCommand"/>.
/// Crea una nueva entidad <see cref="AxiomSystem"/> a partir de los datos proporcionados
/// en el comando y la persiste a través del repositorio <see cref="ISystemRepository"/>.
/// </summary>
public class CreateSystemHandler : IRequestHandler<CreateSystemCommand, AxiomSystem>
{
    private readonly ISystemRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CreateSystemHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de sistemas utilizado para persistir la entidad <see cref="AxiomSystem"/>.</param>
    public CreateSystemHandler(ISystemRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja la creación de un nuevo sistema.
    /// Construye una entidad <see cref="AxiomSystem"/> con el identificador EAI, el nombre
    /// y el identificador del usuario propietario especificados en el comando,
    /// la guarda en el repositorio y la retorna.
    /// </summary>
    /// <param name="request">Comando que contiene los datos del sistema a crear (<see cref="CreateSystemCommand"/>).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="AxiomSystem"/> recién creada y persistida.</returns>
    public async Task<AxiomSystem> Handle(CreateSystemCommand request, CancellationToken cancellationToken)
    {
        var system = new AxiomSystem(request.EAI, request.Name, request.OwnerUserId);
        await _repository.SaveAsync(system, cancellationToken);
        return system;
    }
}
