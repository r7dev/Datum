using Datum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Datum.Infrastructure.Data;

/// <summary>
/// DbContext configurado para Database First com SQL Server.
/// As entidades refletem as tabelas criadas pelos scripts em Datum.Database.
/// Não utilizar migrations — o schema é gerenciado pelos scripts SQL.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
	public DbSet<User> Users => Set<User>();
	public DbSet<Post> Posts => Set<Post>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// ── Users ──────────────────────────────────────────────────────────
		modelBuilder.Entity<User>(entity =>
		{
			entity.ToTable("Users", "dbo");
			entity.HasKey(u => u.Id);

			entity.Property(u => u.Id)
				  .HasColumnName("Id")
				  .UseIdentityColumn();

			entity.Property(u => u.Username)
				  .HasColumnName("Username")
				  .HasMaxLength(50)
				  .IsRequired();

			entity.Property(u => u.Email)
				  .HasColumnName("Email")
				  .HasMaxLength(100)
				  .IsRequired();

			entity.Property(u => u.PasswordHash)
				  .HasColumnName("PasswordHash")
				  .HasMaxLength(255)
				  .IsRequired();

			entity.Property(u => u.CreatedAt)
				  .HasColumnName("CreatedAt")
				  .HasDefaultValueSql("GETUTCDATE()");

			entity.HasIndex(u => u.Email)
				  .IsUnique()
				  .HasDatabaseName("UQ_Users_Email");

			entity.HasIndex(u => u.Username)
				  .IsUnique()
				  .HasDatabaseName("UQ_Users_Username");
		});

		// ── Posts ──────────────────────────────────────────────────────────
		modelBuilder.Entity<Post>(entity =>
		{
			entity.ToTable("Posts", "dbo");
			entity.HasKey(p => p.Id);

			entity.Property(p => p.Id)
				  .HasColumnName("Id")
				  .UseIdentityColumn();

			entity.Property(p => p.Title)
				  .HasColumnName("Title")
				  .HasMaxLength(200)
				  .IsRequired();

			entity.Property(p => p.Content)
				  .HasColumnName("Content")
				  .HasColumnType("NVARCHAR(MAX)")
				  .IsRequired();

			entity.Property(p => p.CreatedAt)
				  .HasColumnName("CreatedAt")
				  .HasDefaultValueSql("GETUTCDATE()");

			entity.Property(p => p.UpdatedAt)
				  .HasColumnName("UpdatedAt")
				  .IsRequired(false);

			entity.Property(p => p.UserId)
				  .HasColumnName("UserId")
				  .IsRequired();

			entity.HasOne(p => p.Author)
				  .WithMany(u => u.Posts)
				  .HasForeignKey(p => p.UserId)
				  .OnDelete(DeleteBehavior.Cascade)
				  .HasConstraintName("FK_Posts_Users");
		});
	}
}
