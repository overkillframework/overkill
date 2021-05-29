using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Overkill.Proxies.Interfaces
{
    public interface IThreadProxy
    {
        IThreadProxy Create(string threadName, ThreadStart function);
        IThreadProxy Create(string threadName, ParameterizedThreadStart function);
        void Start();
        void Abort();
    }
}
