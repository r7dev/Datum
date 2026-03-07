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
GO
CREATE NONCLUSTERED INDEX [IX_Posts_UserId]
ON [dbo].[Posts] ([UserId] ASC)
INCLUDE ([Title], [CreatedAt]);