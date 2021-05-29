using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Overkill.Core.Interfaces;
using Overkill.Proxies.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace Overkill.Core
{
    /// <summary>
    /// Video transmission service which uses FFmpeg to pipe data coming from a capture device to a remote Websocket server using MPEG1
    /// </summary>
    public class FFmpegVideoTransmissionService : IVideoTransmissionService
    {
        const int BITRATE = 150000;

        private readonly ILogger<FFmpegVideoTransmissionService> _logger;
        private readonly IOverkillConfiguration _config;

        public FFmpegVideoTransmissionService(
            ILogger<FFmpegVideoTransmissionService> logger,
            IOverkillConfiguration config
        )
        {
            _logger = logger;
            _config = config;

            FFmpeg.SetExecutablesPath(config.Streaming.FFmpegExecutablePath);
        }

        /// <summary>
        /// Compiles a set of FFmpeg arguments and starts a process
        /// </summary>
        public async Task Start()
        {
            var iface = _config.Streaming.Devices[0];
            var endpoint = _config.Streaming.Endpoint;

            _logger.LogInformation("Starting FFMPEG stream from interface {interface} to {url}", iface, endpoint);

            try
            {
                var ffmpeg = FFmpeg.Conversions.New();
                var arguments = ffmpeg
                    .AddParameter($"-i {iface}", ParameterPosition.PreInput)
                    .AddParameter("-f alsa", ParameterPosition.PreInput)
                    .AddParameter("-i default", ParameterPosition.PreInput)
                    .AddParameter("-f mpegts", ParameterPosition.PostInput)
                    .AddParameter("-vcodec mpeg1video", ParameterPosition.PostInput)
                    .AddParameter("-pix_fmt yuv420p", ParameterPosition.PostInput)
                    .AddParameter("-acodec mp2", ParameterPosition.PostInput)
                    .AddParameter("-ar 48000", ParameterPosition.PostInput)
                    .AddParameter("-ac 2", ParameterPosition.PostInput)
                    .AddParameter("-b:a 128k", ParameterPosition.PostInput)
                    .AddParameter("-preset fast", ParameterPosition.PostInput)
                    .AddParameter("-tune zerolatency", ParameterPosition.PostInput)
                    .AddParameter("-fflags nobuffer", ParameterPosition.PostInput)
                    .AddParameter("-s 460x480", ParameterPosition.PostInput)
                    .AddParameter("-analyzeduration 1", ParameterPosition.PostInput)
                    .AddParameter("-probesize 32", ParameterPosition.PostInput)
                    .AddParameter($"-user-agent \"{_config.System.AuthorizationToken}\"")
                    .SetVideoBitrate(BITRATE)
                    .SetOutput(endpoint)
                    .Build();
                await ffmpeg.Start(arguments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to begin FFMpeg stream");
            }
        }
    }
}
