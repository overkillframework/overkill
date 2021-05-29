using Microsoft.Extensions.DependencyInjection;
using Overkill.Core.Topics.Lifecycle;
using Overkill.PubSub.Interfaces;
using Overkill.Websockets.Interfaces;
using Overkill.Websockets.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Overkill.Websockets.MessageHandlers
{
    public class VehiclePingMessageHandler : IWebsocketMessageHandler<VehiclePingMessage>
    {
        private readonly IPubSubService _pubSub;

        public VehiclePingMessageHandler(IPubSubService pubSubService)
        {
            _pubSub = pubSubService;
        }

        public async Task<IWebsocketMessage> Handle(VehiclePingMessage pingMsg)
        {
            _pubSub.Dispatch(new IsAliveTopic() { Timestamp = pingMsg.Timestamp });

            Thread.Sleep(10);

            return await Task.FromResult(new VehiclePongMessage());
        }
    }
}
