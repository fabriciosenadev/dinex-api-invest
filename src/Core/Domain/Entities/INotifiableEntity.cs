namespace DinExApi.Core;

public interface INotifiableEntity
{
    IReadOnlyCollection<Notification> Notifications { get; }
    bool IsValid { get; }
    void ClearNotifications();
}
