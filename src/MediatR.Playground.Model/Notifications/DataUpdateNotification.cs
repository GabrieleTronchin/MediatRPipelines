﻿using MediatR.Playground.Model.Primitives.Notifications;

namespace MediatR.Playground.Model.Notifications;

public record DataUpdateNotification : IDataUpdateNotification
{
    public Guid Id { get; set; }
    public required string CacheKey { get; set; }
}
