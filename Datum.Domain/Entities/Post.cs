namespace Datum.Domain.Entities;

public class Post
{
	public int Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; set; }

	public int UserId { get; set; }
	public User Author { get; set; } = null!;
}
