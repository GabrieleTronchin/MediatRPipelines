namespace MediatR.Playground.Model.Notifications;

public record SampleNotification : INotification
{
    public Guid Id { get; set; }

    public DateTime NotificationTime { get; set; }
}
