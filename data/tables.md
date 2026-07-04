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
- Name (varchar(200))

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
- KnowledgeId (FK => Knowledges)
- KnowledgeTagId (FK => KnowledgeTags)

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

TechnicalComponents
- ComponentId (PK, GUID)
- Name (varchar(200))
- TechnicalName (varchar(100), UNIQUE)
- ComponentType (varchar(50))
- Environment (varchar(50))
- Criticality (varchar(50))
- Description (nvarchar(max))
- SystemId (FK => Systems)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)

SystemComponents
- SystemComponentId (PK, GUID)
- SystemId (FK => Systems)
- ComponentId (FK => TechnicalComponents)
- UNIQUE (SystemId, ComponentId)

ComponentDependencies
- DependencyId (PK, GUID)
- SourceComponentId (FK => TechnicalComponents)
- TargetComponentId (FK => TechnicalComponents)
- DependencyType (varchar(50))
- Criticality (varchar(50))
- Status (varchar(50))
- Description (nvarchar(max))
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- UNIQUE (SourceComponentId, TargetComponentId, DependencyType)

DependencyTraceEvents
- TraceEventId (PK, GUID)
- DependencyId (FK => ComponentDependencies, CASCADE)
- EventType (varchar(50))
- Description (nvarchar(max))
- IssueId (GUID, nullable)
- KnowledgeId (GUID, nullable)
- RitmNumber (varchar(50), nullable)
- ChangeNumber (varchar(50), nullable)
- CreatedByUserId (GUID, nullable)
- CreatedAt (DateTime)
