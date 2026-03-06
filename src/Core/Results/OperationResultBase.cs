namespace DinExApi.Core;

public abstract class OperationResultBase
{
    private readonly List<Notification> _notifications = [];

    public IReadOnlyCollection<string> Errors => _notifications.Select(x => x.Message).ToArray();
    public bool Succeeded => !IsNotFound && !InternalServerError && _notifications.Count == 0;
    public bool IsNotFound { get; private set; }
    public bool InternalServerError { get; private set; }

    public void AddError(string error)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            _notifications.Add(new Notification(string.Empty, error));
        }
    }

    public void AddErrors(IEnumerable<string> errors)
    {
        foreach (var error in errors)
        {
            AddError(error);
        }
    }

    public void AddNotifications(IEnumerable<Notification>? notifications)
    {
        if (notifications == null)
        {
            return;
        }

        _notifications.AddRange(notifications);
    }

    public void SetAsNotFound() => IsNotFound = true;

    public void SetAsInternalServerError() => InternalServerError = true;

    public bool HasErrors() => _notifications.Count > 0;
}
