using Datum.Domain.Interfaces.Services;

namespace Datum.BlogAPI.Middleware;

public class WebSocketMiddleware(RequestDelegate next, ILogger<WebSocketMiddleware> logger)
{
	private readonly RequestDelegate _next = next;
	private readonly ILogger<WebSocketMiddleware> _logger = logger;

	public async Task InvokeAsync(HttpContext context, INotificationService notificationService)
	{
		if (context.Request.Path == "/ws/notifications" && context.WebSockets.IsWebSocketRequest)
		{
			_logger.LogInformation("Nova conexão WebSocket de {IP}", context.Connection.RemoteIpAddress);
			var webSocket = await context.WebSockets.AcceptWebSocketAsync();
			await notificationService.AddClientAsync(webSocket);
		}
		else
		{
			await _next(context);
		}
	}
}
