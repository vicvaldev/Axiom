using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
/// Comando para eliminar un sistema registrado en la plataforma.
/// </summary>
/// <remarks>
/// Este comando solicita la eliminación de un sistema identificado por su <see cref="long"/> como clave primaria.
/// El manejador debe verificar que el sistema existe y que no tenga dependencias activas antes de eliminarlo.
/// </remarks>
/// <param name="Id">Identificador numérico del sistema a eliminar.</param>
/// <returns><c>true</c> si el sistema se eliminó correctamente; <c>false</c> en caso contrario.</returns>
public record DeleteSystemCommand(long Id) : IRequest<bool>;
