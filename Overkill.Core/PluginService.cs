using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Overkill.Core.Interfaces;
using Overkill.Core.Topics;
using Overkill.Proxies.Interfaces;
using Overkill.PubSub.Interfaces;
using Overkill.Services.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Overkill.Core
{
    public class PluginService : IPluginService
    {
        private readonly ILogger<PluginService> _logger;
        private readonly IPubSubService _pubSub;
        private readonly IThreadProxy _threadCreator;

        private readonly Socket _socket;
        private IThreadProxy _thread;

        public PluginService(
            ILogger<PluginService> logger,
            IPubSubService pubSub, 
            IThreadProxy threadCreator
        )
        {
            _logger = logger;
            _pubSub = pubSub;
            _threadCreator = threadCreator;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Start()
        {
            _socket.Bind(new IPEndPoint(IPAddress.Parse("192.168.4.1"), 13337));
            _thread = _threadCreator.Create("Plugin Message Listener", MessageListener);
            _thread.Start();
        }

        private void MessageListener()
        {
            while (true)
            {
                var buffer = new byte[1024 * 1024];
                if (_socket.Available == 0)
                    continue;
                
                int receivedBytes = _socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

                var message = buffer.Take(receivedBytes).ToArray();
                var json = Encoding.UTF8.GetString(message);
                _logger.LogInformation("Got plugin message: {message}", json);

                var topic = JsonConvert.DeserializeObject<PluginMessageTopic>(json);
                _pubSub.Dispatch(topic);
            }
        }
    }
}
