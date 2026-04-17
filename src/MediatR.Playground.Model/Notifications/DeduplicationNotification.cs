namespace MediatR.Playground.Model.Notifications;

public record DeduplicationNotification : INotification
{
    public Guid Id { get; set; }

    public DateTime NotificationTime { get; set; }
}
