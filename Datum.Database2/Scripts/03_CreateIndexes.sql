-- ============================================================
-- Datum.Database
-- Script 03: Criação de índices adicionais de performance
-- Execute no banco de dados [datum]
-- ============================================================

USE [datum];
GO

-- ── Índice: Posts por UserId (consultas de posts por autor) ──
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Posts_UserId'
      AND object_id = OBJECT_ID('[dbo].[Posts]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Posts_UserId]
        ON [dbo].[Posts] ([UserId] ASC)
        INCLUDE ([Title], [CreatedAt]);

    PRINT 'Índice [IX_Posts_UserId] criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Índice [IX_Posts_UserId] já existe. Nenhuma ação necessária.';
END
GO

-- ── Índice: Posts ordenados por data de criação (listagem) ───
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Posts_CreatedAt_Desc'
      AND object_id = OBJECT_ID('[dbo].[Posts]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Posts_CreatedAt_Desc]
        ON [dbo].[Posts] ([CreatedAt] DESC)
        INCLUDE ([Title], [UserId]);

    PRINT 'Índice [IX_Posts_CreatedAt_Desc] criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Índice [IX_Posts_CreatedAt_Desc] já existe. Nenhuma ação necessária.';
END
GO

-- ── Índice: Users por Email (login rápido) ────────────────────
-- Nota: UQ_Users_Email já cria um índice único no campo Email.
-- Nenhum índice adicional necessário para Email.
PRINT 'Índices verificados com sucesso.';
GO
