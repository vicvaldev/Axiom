Users
- UserId (PK, GUID)
- Email (Unique)
- Name (varchar)

Systems
- SystemId (PK, long)
- EAI (varchar(20))
- Name (varchar(200))
- OwnerUserId (FK => Users)

KnowledgeTags
- KnowledgeTagId (PK, long)
- TagName (varchar(100), unique)

KnowledgeTypes
- TypeId (PK, long)
- Code (varchar, unique)
- Name (varchar(200)) -- Docs, BugFix, Architecture, Diagrams, etc...

IssueStates
- StateId (PK, int)
- Code (varchar, unique)
- Name (varchar(200))

KnowledgeStates
- StateId (PK, int)
- Code (varchar, unique)
- Name (varchar(200))

Knowledges
- KnowledgeId (PK, GUID)
- Title (varchar)
- Summary (varchar(200))
- Content (nvarchar(max))
- SystemId (FK => Systems)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- CreatedByUserId (FK => Users)
- KnowledgeTypeId (FK => KnowledgeTypes)
- KnowledgeStateId (FK => KnowledgeStates)
- IssueId (FK => Issues)
- VersionNumber (int)

KnowledgeKnowledgeTags
KnowledgeId (FK => Knowledges)
KnowledgeTagId (FK => KnowledgeTags)

Issues
- IssueId (PK, GUID)
- Summary (varchar(200))
- RitmNumber (varchar(20), UNIQUE NULLABLE)
- IncidentNumber (varchar(20), UNIQUE NULLABLE)
- SystemId (FK => Systems)
- Problem (nvarchar(max))
- Analysis (nvarchar(max))
- Resolution (nvarchar(max))
- StateId (FK => IssueStates)
- CreatedByUserId (FK => Users)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- ResolvedAt (DateTime)