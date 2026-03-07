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