using Microsoft.Extensions.DependencyInjection;
using Overkill.PubSub.Interfaces;
using Overkill.Services.Interfaces.Services;
using Overkill.Websockets.Interfaces;
using Overkill.Websockets.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Overkill.Websockets.MessageHandlers
{
    public class VehiclePlaySoundMessageHandler : IWebsocketMessageHandler<VehiclePlaySoundMessage>
    {
        private readonly IAudioService _audioService;

        public VehiclePlaySoundMessageHandler(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public Task<IWebsocketMessage> Handle(VehiclePlaySoundMessage playSound)
        {
            _audioService.PlayAudioFromURL(playSound.URL);

            return null;
        }
    }
}
