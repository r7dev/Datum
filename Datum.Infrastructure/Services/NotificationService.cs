using Datum.Domain.DTOs;
using Datum.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Datum.Infrastructure.Services;

public class NotificationService(ILogger<NotificationService> logger) : INotificationService
{
	private readonly ConcurrentBag<WebSocket> _clients = [];
	private readonly ILogger<NotificationService> _logger = logger;

	public async Task AddClientAsync(WebSocket socket)
	{
		_clients.Add(socket);
		_logger.LogInformation("WebSocket conectado. Clientes ativos: {Count}",
			_clients.Count(c => c.State == WebSocketState.Open));

		var buffer = new byte[1024 * 4];

		while (socket.State == WebSocketState.Open)
		{
			var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

			if (result.MessageType == WebSocketMessageType.Close)
			{
				await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
				_logger.LogInformation("WebSocket desconectado.");
			}
		}
	}

	public async Task NotifyNewPostAsync(PostNotification notification)
	{
		var payload = JsonSerializer.Serialize(new
		{
			type = "NEW_POST",
			data = notification
		});

		var bytes   = Encoding.UTF8.GetBytes(payload);
		var segment = new ArraySegment<byte>(bytes);

		var activeClients = _clients
			.Where(c => c.State == WebSocketState.Open)
			.ToList();

		_logger.LogInformation("Notificando {Count} cliente(s) sobre novo post: \"{Title}\"",
			activeClients.Count, notification.Title);

		var tasks = activeClients.Select(client =>
			client.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None));

		await Task.WhenAll(tasks);
	}
}
