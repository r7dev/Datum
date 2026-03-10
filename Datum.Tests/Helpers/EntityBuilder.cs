using Datum.Domain.Entities;

namespace Datum.Tests.Helpers;

/// <summary>
/// Construtores de entidades com dados padrão para uso nos testes.
/// Facilita a criação de objetos sem repetir boilerplate.
/// </summary>
public static class EntityBuilder
{
	public static User BuildUser(
		int id = 1,
		string username = "testuser",
		string email = "test@datum.com",
		string passwordHash = "$2a$11$dummyHashForTestingPurposesOnly123456")
	{
		return new User
		{
			Id = id,
			Username = username,
			Email = email,
			PasswordHash = passwordHash,
			CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
		};
	}

	public static Post BuildPost(
		int id = 1,
		string title = "Post de Teste",
		string content = "Conteúdo do post de teste.",
		int userId = 1,
		User? author = null)
	{
		var post = new Post
		{
			Id = id,
			Title = title,
			Content = content,
			UserId = userId,
			CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			UpdatedAt = null,
			Author = author ?? BuildUser(id: userId)
		};
		return post;
	}
}
