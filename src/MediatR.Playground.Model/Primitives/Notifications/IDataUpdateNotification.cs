namespace MediatR.Playground.Model.Primitives.Notifications;

public interface IDataUpdateNotification : INotification
{
    Guid Id { get; set; }

    string CacheKey { get; set; }
}
