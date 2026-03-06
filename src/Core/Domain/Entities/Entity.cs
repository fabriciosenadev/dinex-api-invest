namespace DinExApi.Core;

public abstract class Entity : BaseEntity, INotifiableEntity
{
    private List<Notification>? _notifications;

    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }

    public IReadOnlyCollection<Notification> Notifications
        => (IReadOnlyCollection<Notification>?)_notifications ?? Array.Empty<Notification>();

    public bool IsValid => _notifications == null || _notifications.Count == 0;

    protected void AddNotifications(Notifiable<Notification>? source)
    {
        if (source == null || source.Notifications.Count == 0)
        {
            return;
        }

        _notifications ??= new List<Notification>(source.Notifications.Count);
        _notifications.AddRange(source.Notifications);
    }

    protected void AddNotifications(params Notifiable<Notification>?[] sources)
    {
        if (sources == null)
        {
            return;
        }

        foreach (var source in sources)
        {
            AddNotifications(source);
        }
    }

    protected void AddNotifications(IEnumerable<Notification>? notifications)
    {
        if (notifications == null)
        {
            return;
        }

        foreach (var notification in notifications)
        {
            AddNotification(notification);
        }
    }

    protected void AddNotification(string key, string message)
    {
        _notifications ??= [];
        _notifications.Add(new Notification(key, message));
    }

    protected void AddNotification(Notification notification)
    {
        _notifications ??= [];
        _notifications.Add(notification);
    }

    public void ClearNotifications() => _notifications?.Clear();

    public void MarkAsDeleted() => DeletedAt = DateTime.UtcNow;

    public bool IsDeleted() => DeletedAt.HasValue;

    public void EnsureNotDeleted(string entityName)
    {
        if (IsDeleted())
        {
            AddNotification(entityName, $"Entity {entityName} is deleted and cannot be used.");
        }
    }

    public void Restore()
    {
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
