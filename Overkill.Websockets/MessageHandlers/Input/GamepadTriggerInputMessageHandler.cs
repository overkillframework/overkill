using Microsoft.Extensions.DependencyInjection;
using Overkill.Core.Topics;
using Overkill.Core.Topics.Input;
using Overkill.PubSub.Interfaces;
using Overkill.Websockets.Interfaces;
using Overkill.Websockets.Messages;
using Overkill.Websockets.Messages.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Overkill.Websockets.MessageHandlers.Input
{
    public class GamepadTriggerInputMessageHandler : IWebsocketMessageHandler<GamepadTriggerInputMessage>
    {
        private readonly IPubSubService _pubSub;

        public GamepadTriggerInputMessageHandler(IPubSubService pubSubService)
        {
            _pubSub = pubSubService;
        }

        public Task<IWebsocketMessage> Handle(GamepadTriggerInputMessage triggerInput)
        {
            _pubSub.Dispatch(new GamepadTriggerInputTopic()
            {
                Name = triggerInput.Name,
                Value = triggerInput.Value
            });

            return null;
        }
    }
}
