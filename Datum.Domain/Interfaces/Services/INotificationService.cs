using Datum.Domain.DTOs;
using System.Net.WebSockets;

namespace Datum.Domain.Interfaces.Services;

public interface INotificationService
{
	Task AddClientAsync(WebSocket socket);
	Task NotifyNewPostAsync(PostNotification notification);
}
