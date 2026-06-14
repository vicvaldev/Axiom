using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;
using MediatR;

namespace Axiom.Application.Commands;

public record CreateCaseCommand(
    string System,
    string Problem,
    string Analysis,
    string Resolution,
    string LessonsLearned,
    string? RitmId,
    string? ChangeId) : IRequest<CaseRecord>;
