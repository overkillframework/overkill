using Microsoft.Extensions.Logging;
using Overkill.Proxies.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Overkill.Util.Helpers
{
    /// <summary>
    /// Proxy class to assist in unit testing HTTP related functionality
    /// </summary>
    public class HttpProxy : IHttpProxy
    {
        private readonly ILogger<HttpProxy> _logger;

        public HttpProxy(ILogger<HttpProxy> logger)
        {
            _logger = logger;
        }

        public async Task DownloadFile(string url, string localFile)
        {
            _logger.LogInformation("Downloading remote file {url} to {localPath}", url, localFile);

            using (var httpClient = new HttpClient())
            {
                using (var stream = await httpClient.GetStreamAsync(url))
                {
                    using(var fs = File.OpenWrite(localFile))
                    {
                        await stream.CopyToAsync(fs);
                        await stream.FlushAsync();
                    }
                }
            }
        }

        public async Task<string> Get(string url)
        {
            _logger.LogInformation("Performing GET request on {url}", url);

            using (var webClient = new WebClient())
            {
                return await webClient.DownloadStringTaskAsync(new Uri(url));
            }
        }

        public async Task<string> Post(string url, object payload)
        {
            _logger.LogInformation("Performing POST request on {url}: {data}", url, payload);

            using (var webClient = new WebClient())
            {
                var stringPayload = JsonSerializer.Serialize(payload);
                return await webClient.UploadStringTaskAsync(new Uri(url), stringPayload);
            }
        }
    }
}
