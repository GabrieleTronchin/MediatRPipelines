using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediatR.Playground.Model.Notifications;

public record SampleNotification : INotification
{
    public Guid Id { get; set; }

    public DateTime NotificationTime { get; set; }
}
