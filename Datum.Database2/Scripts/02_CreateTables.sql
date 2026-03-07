-- ============================================================
-- Datum.Database
-- Script 02: Criação das tabelas
-- Execute no banco de dados [datum]
-- ============================================================

USE [datum];
GO

-- ── Tabela: dbo.Users ────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'dbo' AND t.name = 'Users'
)
BEGIN
    CREATE TABLE [dbo].[Users]
    (
        [Id]           INT            NOT NULL IDENTITY(1,1),
        [Username]     NVARCHAR(50)   NOT NULL,
        [Email]        NVARCHAR(100)  NOT NULL,
        [PasswordHash] NVARCHAR(255)  NOT NULL,
        [CreatedAt]    DATETIME2(7)   NOT NULL CONSTRAINT [DF_Users_CreatedAt] DEFAULT (GETUTCDATE()),

        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_Users_Email]    UNIQUE NONCLUSTERED ([Email] ASC),
        CONSTRAINT [UQ_Users_Username] UNIQUE NONCLUSTERED ([Username] ASC)
    );

    PRINT 'Tabela [dbo].[Users] criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela [dbo].[Users] já existe. Nenhuma ação necessária.';
END
GO

-- ── Tabela: dbo.Posts ────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'dbo' AND t.name = 'Posts'
)
BEGIN
    CREATE TABLE [dbo].[Posts]
    (
        [Id]        INT            NOT NULL IDENTITY(1,1),
        [Title]     NVARCHAR(200)  NOT NULL,
        [Content]   NVARCHAR(MAX)  NOT NULL,
        [CreatedAt] DATETIME2(7)   NOT NULL CONSTRAINT [DF_Posts_CreatedAt] DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2(7)   NULL,
        [UserId]    INT            NOT NULL,

        CONSTRAINT [PK_Posts]       PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Posts_Users] FOREIGN KEY ([UserId])
            REFERENCES [dbo].[Users] ([Id])
            ON DELETE CASCADE
            ON UPDATE NO ACTION
    );

    PRINT 'Tabela [dbo].[Posts] criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela [dbo].[Posts] já existe. Nenhuma ação necessária.';
END
GO
