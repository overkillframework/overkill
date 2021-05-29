using Microsoft.Extensions.Logging;
using Overkill.Proxies.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Overkill.Util.Helpers
{
    /// <summary>
    /// Proxy class to support unit testing of Filesystem I/O
    /// </summary>
    public class FilesystemProxy : IFilesystemProxy
    {
        private readonly ILogger<FilesystemProxy> _logger;

        public FilesystemProxy(ILogger<FilesystemProxy> logger)
        {
            _logger = logger;
        }

        public string GenerateTempFilename(string extension)
        {
            var filePath = $"{Path.GetTempPath()}{Guid.NewGuid()}.{extension}";

            _logger.LogDebug("Generated temporary file: {filePath}", filePath);
            
            return filePath;
        }

        public void WriteFile(string fileName, string data)
        {
            _logger.LogDebug("Writing {length} bytes to {filePath}", data.Length, fileName);

            File.WriteAllText(fileName, data);
        }

        public string ReadFile(string fileName)
        {
            _logger.LogDebug("Reading from {fileName}", fileName);

            return File.ReadAllText(fileName);
        }

        public void DeleteFile(string fileName)
        {
            _logger.LogDebug("Deleting file: {fileName}", fileName);

            File.Delete(fileName);
        }
    }
}
