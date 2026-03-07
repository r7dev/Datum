namespace Datum.Domain.DTOs;

// ── Auth ──────────────────────────────────────────────────────────────────────
public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Username, string Email);

// ── Posts ─────────────────────────────────────────────────────────────────────
public record CreatePostRequest(string Title, string Content);
public record UpdatePostRequest(string Title, string Content);

public record PostResponse(
	int Id,
	string Title,
	string Content,
	string AuthorUsername,
	int AuthorId,
	DateTime CreatedAt,
	DateTime? UpdatedAt
);

// ── WebSocket Notifications ───────────────────────────────────────────────────
public record PostNotification(
	int PostId,
	string Title,
	string AuthorUsername,
	DateTime CreatedAt
);
