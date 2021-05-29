using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using Microsoft.Extensions.Logging;
using Overkill.Proxies.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Overkill.Proxies
{
    /// <summary>
    /// Proxy class to assist in unit testing sound playback functionality
    /// </summary>
    public class SoundPlayerProxy : ISoundPlayerProxy
    {
        private readonly ILogger<SoundPlayerProxy> _logger;
        private readonly ISoundOut _soundOut;

        public SoundPlayerProxy(ISoundOut soundOut)
        {
            _soundOut = soundOut;
        }

        public void Play(string fileName)
        {
            _logger.LogInformation("Playing audio file: {fileName}", fileName);

            var waveSource = CodecFactory.Instance
                .GetCodec(fileName)
                .ToSampleSource()
                .ToMono()
                .ToWaveSource();

            _soundOut.Initialize(waveSource);
            _soundOut.Play();
        }
    }
}
