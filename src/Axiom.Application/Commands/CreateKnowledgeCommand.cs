using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
/// Command to create a new knowledge entry.
/// </summary>
/// <param name="Title">The title of the knowledge entry.</param>
/// <param name="Description">An optional summary description.</param>
/// <param name="Content">The main body content of the entry.</param>
/// <param name="System">The associated system or application name.</param>
/// <param name="Tags">A list of tags for categorization.</param>
/// <param name="Author">The name of the author.</param>
/// <param name="Type">The content type classification.</param>
/// <param name="Status">The initial lifecycle status.</param>
public record CreateKnowledgeCommand(
    string Title,
    string Description,
    string Content,
    string System,
    List<string> Tags,
    string Author,
    KnowledgeType Type,
    KnowledgeStatusValue Status) : IRequest<KnowledgeEntry>;
