using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
/// Command to create a new case record.
/// </summary>
/// <param name="System">The associated system or application name.</param>
/// <param name="Problem">A description of the problem or issue.</param>
/// <param name="Analysis">Root cause analysis notes.</param>
/// <param name="Resolution">Resolution steps applied.</param>
/// <param name="LessonsLearned">Lessons learned from the case.</param>
/// <param name="RitmId">An optional RITM identifier.</param>
/// <param name="ChangeId">An optional change request identifier.</param>
public record CreateCaseCommand(
    string System,
    string Problem,
    string Analysis,
    string Resolution,
    string LessonsLearned,
    string? RitmId,
    string? ChangeId) : IRequest<CaseRecord>;
