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