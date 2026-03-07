-- ============================================================
-- Datum.Database
-- Script 04: Dados iniciais (seed)
-- OPCIONAL — Execute apenas para popular um ambiente de dev/teste
-- Senhas em BCrypt: todos os usuários de seed têm senha "Datum@123"
-- ============================================================

USE [datum];
GO

-- ── Usuários de exemplo ───────────────────────────────────────
-- Hash BCrypt de "Datum@123" (cost=11)
DECLARE @passwordHash NVARCHAR(255) = '$2a$11$K5L8Q2mXnPvRtUwYsZbJkO3eHfGiClDaMqNpOwVxTuSrYzAeBdCfE';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Email] = 'admin@datum.com')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [Email], [PasswordHash], [CreatedAt])
    VALUES ('admin', 'admin@datum.com', @passwordHash, GETUTCDATE());

    PRINT 'Usuário [admin@datum.com] inserido.';
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Email] = 'joao@datum.com')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [Email], [PasswordHash], [CreatedAt])
    VALUES ('joao', 'joao@datum.com', @passwordHash, GETUTCDATE());

    PRINT 'Usuário [joao@datum.com] inserido.';
END
GO

-- ── Posts de exemplo ──────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [dbo].[Posts] WHERE [Title] = 'Bem-vindo ao Datum Blog')
BEGIN
    DECLARE @adminId INT = (SELECT [Id] FROM [dbo].[Users] WHERE [Email] = 'admin@datum.com');

    INSERT INTO [dbo].[Posts] ([Title], [Content], [UserId], [CreatedAt])
    VALUES
    (
        'Bem-vindo ao Datum Blog',
        'Esta é a primeira postagem do blog Datum. Aqui você poderá compartilhar suas ideias e experiências.',
        @adminId,
        GETUTCDATE()
    );

    PRINT 'Post de boas-vindas inserido.';
END
GO

PRINT 'Seed data executado com sucesso.';
GO
