namespace MediatR.Playground.Model.Primitives.Notifications;

public interface IPriorityNotificationHandler<in TNotification>
    : INotificationHandler<TNotification>
    where TNotification : IPriorityNotification
{
    public int Priority { get; }
}
