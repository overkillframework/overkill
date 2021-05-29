using Microsoft.Extensions.Logging;
using Overkill.Proxies.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Overkill.Proxies
{
    /// <summary>
    /// Proxy class to assist in unit testing thread related functionality
    /// </summary>
    public class ThreadProxy : IThreadProxy
    {
        private readonly ILogger<ThreadProxy> _logger;
        private readonly string _threadName;
        private readonly Thread _thread;

        public IThreadProxy Create(string threadName, ThreadStart function)
        {
            return new ThreadProxy(_logger, threadName, function);
        }

        public IThreadProxy Create(string threadName, ParameterizedThreadStart function)
        {
            return new ThreadProxy(_logger, threadName, function);
        }

        public ThreadProxy(ILogger<ThreadProxy> logger) 
        {
            _logger = logger;
        }

        public ThreadProxy(ILogger<ThreadProxy> logger, string threadName, ThreadStart function)
        {
            _logger = logger;
            _threadName = threadName;
            _thread = new Thread(function);
        }

        public ThreadProxy(ILogger<ThreadProxy> logger, string threadName, ParameterizedThreadStart function)
        {
            _logger = logger;
            _threadName = threadName;
            _thread = new Thread(function);
        }

        public void Start()
        {
            _logger.LogDebug("Starting new thread: {name}", _threadName);
            _thread.Start();
        }

        public void Abort()
        {
            _logger.LogDebug("Aborting thread: {name}", _threadName);
            _thread.Abort();
        }
    }
}
