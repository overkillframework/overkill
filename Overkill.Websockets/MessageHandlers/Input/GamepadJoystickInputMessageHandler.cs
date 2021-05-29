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
    public class GamepadJoystickInputMessageHandler : IWebsocketMessageHandler<GamepadJoystickInputMessage>
    {
        private readonly IPubSubService _pubSub;

        public GamepadJoystickInputMessageHandler(IPubSubService pubSubService)
        {
            _pubSub = pubSubService;
        }

        public Task<IWebsocketMessage> Handle(GamepadJoystickInputMessage joystickInput)
        {
            _pubSub.Dispatch(new GamepadJoystickInputTopic()
            {
                Name = joystickInput.Name,
                IsPressed = joystickInput.IsPressed,
                X = joystickInput.X,
                Y = joystickInput.Y
            });

            return null;
        }
    }
}
