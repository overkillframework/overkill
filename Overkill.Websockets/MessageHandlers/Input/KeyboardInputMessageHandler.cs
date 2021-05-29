using Microsoft.Extensions.DependencyInjection;
using Overkill.Core.Topics;
using Overkill.Core.Topics.Input;
using Overkill.PubSub.Interfaces;
using Overkill.Websockets.Interfaces;
using Overkill.Websockets.Messages.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Overkill.Websockets.MessageHandlers.Input
{
    public class KeyboardInputMessageHandler : IWebsocketMessageHandler<KeyboardInputMessage>
    {
        private readonly IPubSubService _pubSub;

        public KeyboardInputMessageHandler(IPubSubService pubSubService)
        {
            _pubSub = pubSubService;
        }

        public Task<IWebsocketMessage> Handle(KeyboardInputMessage keyboardInput)
        {
            _pubSub.Dispatch(new KeyboardInputTopic()
            {
                Name = keyboardInput.Name,
                IsPressed = keyboardInput.IsPressed
            });

            return null;
        }
    }
}
