using Microsoft.Extensions.DependencyInjection;
using Overkill.Common.Enums;
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
    public class GamepadButtonInputMessageHandler : IWebsocketMessageHandler<GamepadButtonInputMessage>
    {
        private readonly IPubSubService _pubSub;

        public GamepadButtonInputMessageHandler(IPubSubService pubSubService)
        {
            _pubSub = pubSubService;
        }

        public Task<IWebsocketMessage> Handle(GamepadButtonInputMessage gamepadButton)
        {
            _pubSub.Dispatch(new GamepadButtonInputTopic()
            {
                Name = gamepadButton.Name,
                State = gamepadButton.IsPressed ? InputState.Pressed : InputState.Released
            });

            return null;
        }
    }
}
