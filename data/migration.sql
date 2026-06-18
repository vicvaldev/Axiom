IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [CaseRecords] (
    [Id] uniqueidentifier NOT NULL,
    [RitmId] nvarchar(100) NULL,
    [ChangeId] nvarchar(100) NULL,
    [System] nvarchar(200) NOT NULL,
    [Problem] nvarchar(max) NOT NULL,
    [Analysis] nvarchar(max) NOT NULL,
    [Resolution] nvarchar(max) NOT NULL,
    [LessonsLearned] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [Status] tinyint NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_CaseRecords] PRIMARY KEY ([Id])
);

CREATE TABLE [KnowledgeEntries] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(500) NOT NULL,
    [Description] nvarchar(2000) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [System] nvarchar(200) NOT NULL,
    [Tags] nvarchar(2000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [Author] nvarchar(200) NOT NULL,
    [Type] tinyint NOT NULL,
    [Status] tinyint NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_KnowledgeEntries] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260618225440_InitialCreate', N'10.0.9');

COMMIT;
GO

