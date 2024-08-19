namespace MediatR.Playground.Model.Primitives;

public interface IDataUpdateNotification : INotification
{
    Guid Id { get; set; }

    string CacheKey { get; set; }
}
