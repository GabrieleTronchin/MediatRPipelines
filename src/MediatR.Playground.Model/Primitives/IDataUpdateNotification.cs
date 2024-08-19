using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediatR.Playground.Model.Primitives;

public interface IDataUpdateNotification : INotification
{
    Guid Id { get; set; }

    string CacheKey { get; set; }
}
