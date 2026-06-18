-- ============================================================
-- Axiom - Seed Data: Insert y consultas de ejemplo
-- Ejecutar DESPUES de aplicar la migración (migration.sql)
-- ============================================================

-- ============================================================
-- 1. Insertar un KnowledgeEntry
-- ============================================================
DECLARE @KnowledgeId UNIQUEIDENTIFIER = NEWID();

INSERT INTO [KnowledgeEntries] (
    [Id], [Title], [Description], [Content], [System],
    [Tags], [CreatedAt], [UpdatedAt], [Author],
    [Type], [Status], [DeletedAt]
)
VALUES (
    @KnowledgeId,
    N'Configuración de conexión a SQL Server',
    N'Guía rápida para configurar conexiones con autenticación integrada',
    N'Para conectar a SQL Server con autenticación integrada en Windows, '
    + N'asegúrate de usar Integrated Security=True en el connection string. '
    + N'En WSL, SQL Server debe correr en localhost y el usuario de Windows '
    + N'debe tener permisos en la base de datos.',
    N'AXIOM',
    N'SqlServer,configuración,WSL',
    SYSDATETIME(), SYSDATETIME(), N'dev-team',
    0,  -- Documentation
    0,  -- Draft
    NULL
);

-- ============================================================
-- 2. Insertar un CaseRecord
-- ============================================================
DECLARE @CaseId UNIQUEIDENTIFIER = NEWID();

INSERT INTO [CaseRecords] (
    [Id], [RitmId], [ChangeId], [System], [Problem],
    [Analysis], [Resolution], [LessonsLearned],
    [CreatedAt], [Status], [DeletedAt]
)
VALUES (
    @CaseId,
    N'RITM98765', N'CHG54321', N'AXIOM',
    N'La aplicación no puede conectar a la base de datos al iniciar',
    N'El servicio de SQL Server no estaba corriendo en localhost. '
    + N'Se verificó el estado del servicio y los logs del sistema.',
    N'Se inició el servicio de SQL Server y se verificó la conectividad '
    + N'con el comando sqlcmd.',
    N'Siempre verificar que SQL Server esté corriendo antes de desplegar '
    + N'la aplicación.',
    SYSDATETIME(),
    1,  -- InProgress
    NULL
);

-- ============================================================
-- 3. Consulta: Listar todos los KnowledgeEntries
-- ============================================================
PRINT '=== KnowledgeEntries ===';
SELECT [Id], [Title], [System], [Type], [Status], [CreatedAt]
FROM [KnowledgeEntries]
WHERE [DeletedAt] IS NULL
ORDER BY [CreatedAt] DESC;

-- ============================================================
-- 4. Consulta: Listar todos los CaseRecords
-- ============================================================
PRINT '=== CaseRecords ===';
SELECT [Id], [Problem], [System], [Status], [RitmId], [CreatedAt]
FROM [CaseRecords]
WHERE [DeletedAt] IS NULL
ORDER BY [CreatedAt] DESC;

-- ============================================================
-- 5. Consulta: Buscar conocimiento por texto
-- ============================================================
PRINT '=== Búsqueda: "SQL Server" ===';
SELECT [Id], [Title], [System], [Status]
FROM [KnowledgeEntries]
WHERE [DeletedAt] IS NULL
  AND ([Title] LIKE N'%SQL Server%'
    OR [Description] LIKE N'%SQL Server%'
    OR [Content] LIKE N'%SQL Server%')
ORDER BY [CreatedAt] DESC;

-- ============================================================
-- 6. Consulta: Casos activos (no cerrados)
-- ============================================================
PRINT '=== Casos Activos ===';
SELECT [Id], [Problem], [System], [Status], [CreatedAt]
FROM [CaseRecords]
WHERE [DeletedAt] IS NULL
  AND [Status] IN (0, 1)  -- Open, InProgress
ORDER BY [CreatedAt] DESC;

GO
