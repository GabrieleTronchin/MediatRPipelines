using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediatR.Playground.Model.Primitives.Notifications;

public interface IPriorityNotificationHandler<in TNotification>: 
    INotificationHandler<TNotification>
    where TNotification : IPriorityNotification
{
    public int Priority { get; }

}
