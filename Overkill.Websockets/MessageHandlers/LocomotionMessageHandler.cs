using Microsoft.Extensions.DependencyInjection;
using Overkill.Core.Topics;
using Overkill.Core.Topics.Control;
using Overkill.PubSub.Interfaces;
using Overkill.Websockets.Interfaces;
using Overkill.Websockets.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Overkill.Websockets.MessageHandlers
{
    /// <summary>
    /// Handler for generic Drive messages coming from users
    /// </summary>
    public class LocomotionMessageHandler : IWebsocketMessageHandler<LocomotionMessage>
    {
        private readonly IPubSubService _pubSub;

        public LocomotionMessageHandler(IPubSubService pubSubService)
        {
            _pubSub = pubSubService;
        }

        public Task<IWebsocketMessage> Handle(LocomotionMessage locomotion)
        {
            _pubSub.Dispatch(new LocomotionTopic()
            {
                Direction = locomotion.Direction,
                Speed = locomotion.Speed
            });

            return null;
        }
    }
}
