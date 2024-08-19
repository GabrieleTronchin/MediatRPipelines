using MediatR.Playground.Model.Primitives.Notifications;

namespace MediatR.Playground.Model.Notifications;

public class SampleParallelNotification : IParallelNotification
{
    public Guid Id { get; set; }

    public DateTime NotificationTime { get; set; }
}
