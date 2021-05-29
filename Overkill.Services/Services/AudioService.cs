using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using Overkill.Proxies.Interfaces;
using Overkill.Services.Interfaces.Services;
using Overkill.Util;
using Overkill.Util.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Overkill.Services.Services
{
    /// <summary>
    /// Manages the playing of sound files (local and remote) as well as text to speech
    /// </summary>
    public class AudioService : IAudioService
    {
        private IProcessProxy _processProxy;
        private IFilesystemProxy _filesystemProxy;
        private IHttpProxy _httpProxy;

        public AudioService(
            IProcessProxy processProxy, 
            IFilesystemProxy filesystemProxy, 
            IHttpProxy httpProxy
        )
        {
            _filesystemProxy = filesystemProxy;
            _httpProxy = httpProxy;
            _processProxy = processProxy;

            if (!Directory.Exists("sound_cache"))
                Directory.CreateDirectory("sound_cache");
        }

        /// <summary>
        /// Download and play a sound file from a remote URL
        /// </summary>
        /// <param name="url">The direct URL to the sound file</param>
        public void PlayAudioFromURL(string url)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(url);
                var hashBytes = md5.ComputeHash(inputBytes);
                var sb = new StringBuilder();
                for(int i=0;i<hashBytes.Length;i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                var hashedUrl = sb.ToString();
                var hashedFilename = $"{hashedUrl}.wav";

                if(File.Exists(Path.Combine("sound_cache", hashedFilename)))
                {
                    PlayFromLocalFile(Path.GetFullPath(Path.Combine("sound_cache", hashedFilename)));
                }
                else
                {
                    new Thread(new ThreadStart(() =>
                    {
                        var extension = url.Substring(url.LastIndexOf(".") + 1).Split('?')[0];
                        var localFileName = _filesystemProxy.GenerateTempFilename(extension);
                        _httpProxy.DownloadFile(url, localFileName).Wait();

                        File.WriteAllBytes(Path.Combine("sound_cache", hashedFilename), File.ReadAllBytes(localFileName));

                        PlayFromLocalFile(localFileName);
                    })).Start();
                }
            }

        }

        /// <summary>
        /// Play a local sound file
        /// </summary>
        /// <param name="localFile"></param>
        public void PlayFromLocalFile(string localFile)
        {
            _processProxy.ExecuteShellCommand("aplay", localFile);
        }

        /// <summary>
        /// Play Text2Speech
        /// </summary>
        /// <param name="text">The text to play from the speaker</param>
        public void SayText(string text)
        {
            throw new NotImplementedException();
        }
    }
}
