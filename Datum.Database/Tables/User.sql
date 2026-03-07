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
GO
CREATE NONCLUSTERED INDEX [IX_Posts_CreatedAt_Desc]
ON [dbo].[Posts] ([CreatedAt] DESC)
INCLUDE ([Title], [UserId]);