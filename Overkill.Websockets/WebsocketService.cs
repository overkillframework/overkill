using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Overkill.Core.Interfaces;
using Overkill.Core.Topics;
using Overkill.PubSub.Interfaces;
using Overkill.Services.Interfaces.Services;
using Overkill.Websockets.Interfaces;
using Overkill.Websockets.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp.NetCore;

namespace Overkill.Websockets
{
    /// <summary>
    /// Manages the Websocket client communication between Overkill and Overkill Web Services
    /// This service utilizes reflection to discover messages and their respective handler functions.
    /// Check out the README to see how easy it is to add new message types
    /// </summary>
    public class WebsocketService : IWebsocketService
    {
        private readonly ILogger<WebsocketService> _logger;
        private IServiceProvider _serviceProvider;
        private INetworkingService _networkService;
        private IOverkillConfiguration _config;
        private Dictionary<string, Type> _messageTypeCache;
        private WebSocket webSocket;

        public WebsocketService(
            ILogger<WebsocketService> logger,
            IServiceProvider serviceProvider, 
            INetworkingService networkService, 
            IOverkillConfiguration configuration, 
            IPubSubService pubSub
        )
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _networkService = networkService;
            _config = configuration;

            _messageTypeCache = new Dictionary<string, Type>();

            pubSub.Subscribe<PluginMessageTopic>(message =>
            {
                SendMessage(new CustomMessage() {
                    MessageType = message.MessageType,
                    JSON = message.JSON
                });
            });
        }

        /// <summary>
        /// Start up the client by registering message types, handlers, and then connecting
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            RegisterMessages();
            await Connect();
        }

        /// <summary>
        /// Configure the websocket and assign event handlers, connect using system configuration
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            var localIP = IPAddress.Parse(_networkService.GetLocalInterfaceAddress(_config.Client.Interface));
            Console.WriteLine($"Connecting via {localIP}");
            webSocket = new WebSocket(new IPEndPoint(localIP, 0), _config.Client.ConnectionString);

            webSocket.WaitTime = TimeSpan.FromSeconds(1);
            webSocket.OnMessage += async (sender, evt) => await Handle(evt.Data);
            webSocket.OnOpen += (sender, evt) => OnConnected();
            webSocket.OnError += (sender, evt) => Console.WriteLine(evt.Message);
            webSocket.OnClose += (sender, evt) => OnDisconnected();
            await webSocket.ConnectAsync();
        }

        /// <summary>
        /// Event handler for a successful connection to online services. Sends an authentication message.
        /// </summary>
        private void OnConnected()
        {
            if(webSocket.IsAlive)
            {
                Console.WriteLine("Authenticating with Web Services...");
                SendMessage(new VehicleAuthorizationMessage()
                {
                    Token = _config.System.AuthorizationToken
                });
            }
        }

        /// <summary>
        /// Event handler that fires off when connection is lost with web services.
        /// TODO: Dispatch kill switch?
        /// </summary>
        private void OnDisconnected()
        {
            Console.WriteLine("Lost connection with web services. Reconnecting...");

            //Wait a second and re-connect
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                webSocket.ConnectAsync();
            });
        }

        /// <summary>
        /// Function to parse an incoming message, check if its a known message type, and send it off to its respective handler
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task Handle(string json)
        {
            //Parse the message type
            var message = JObject.Parse(json);
            var messageType = (string)message["type"];

            DateTimeOffset time = DateTime.UtcNow;
            if (message.ContainsKey("time"))
                DateTimeOffset.TryParse((string)message["time"], out time);

            //if ((DateTime.UtcNow - time).TotalSeconds > 1)
                //return;

            if(!_messageTypeCache.ContainsKey(messageType))
            {
                _logger.LogWarning("Could not find message type: {messageType}", messageType);
                return;
            }

            //Check to see if we have a Type cached for this
            var messageClassType = _messageTypeCache[messageType];
            
            //Otherwise, deserialize the JSON into this Type
            var convertedMessage = message.ToObject(messageClassType);

            var handlerType = typeof(IWebsocketMessageHandler<>)
                .MakeGenericType(convertedMessage.GetType());

            dynamic handler = _serviceProvider.GetService(handlerType);
            //Retrieve the IWebsocketMessageHandler and invoke it
            if(handler == null)
            {
                _logger.LogWarning("Could not find handler for message type: {messageType}", messageType);
                return;
            }

            _logger.LogDebug("Processing message: {messageType}", messageType);
            Task<IWebsocketMessage> task = handler.Handle((dynamic)convertedMessage);
            
            //If there is no response, return. Otherwise, send the response.
            if (task == null) return;
            SendMessage(await task);
        }

        /// <summary>
        /// Use reflection to discover message types in loaded assemblies by searchign for IWebsocketMessage inheritance.
        /// Additionally, ensure these classes obey the naming conventions of having "Message" at the end of their name.
        /// Cache them in a dictionary for future lookups.
        /// </summary>
        public void RegisterMessages()
        {
            var messageTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(IWebsocketMessage).IsAssignableFrom(x) && !x.IsInterface)
                .Where(x => x.Name.ToLower().EndsWith("message"))
                .ToList();

            _logger.LogInformation("{count} websocket message types found", messageTypes.Count);

            messageTypes.ForEach(type =>
            {
                var messageType = type.Name.ToLower().Split(new[] { "message" }, StringSplitOptions.None)[0];

                _messageTypeCache.Add(
                    messageType,
                    type
                );

                _logger.LogInformation("Registered websocket message: {messageType}", messageType);
            });
        }

        /// <summary>
        /// Send a message to the server.
        /// A "type" property will automatically be assigned based on the Type name.
        /// </summary>
        /// <param name="message">A message inheriting the IWebsocketMessage interface</param>
        public void SendMessage(IWebsocketMessage message)
        {
            if (!_messageTypeCache.Any(x => x.Value == message.GetType())) return;

            var messageType = _messageTypeCache.FirstOrDefault(x => x.Value == message.GetType()).Key;

            var jObject = JObject.FromObject(message);
            jObject.Add("type", messageType);
            webSocket.Send(jObject.ToString());
        }
    }
}
