using Datum.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Datum.Tests.Helpers;

/// <summary>
/// Fábrica de ApplicationDbContext em memória para testes de repositórios.
/// Cada chamada cria um banco isolado com nome único, evitando interferência entre testes.
/// </summary>
public static class DbContextFactory
{
	public static ApplicationDbContext Create(string? dbName = null)
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
			.Options;

		var context = new ApplicationDbContext(options);
		context.Database.EnsureCreated();
		return context;
	}
}
