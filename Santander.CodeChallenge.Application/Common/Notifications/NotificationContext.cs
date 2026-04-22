namespace Santander.CodeChallenge.Application.Common.Notifications;

public interface INotificationContext
{
    bool HasNotifications { get; }
    IReadOnlyCollection<string> Notifications { get; }
    void Add(string message);
}

public sealed class NotificationContext : INotificationContext
{
    private readonly List<string> _notifications = new();

    public bool HasNotifications => _notifications.Count > 0;
    public IReadOnlyCollection<string> Notifications => _notifications.AsReadOnly();

    public void Add(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            _notifications.Add(message);
        }
    }
}
