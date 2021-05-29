using Microsoft.Extensions.Logging;
using Overkill.Proxies.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Overkill.Util.Helpers
{
    /// <summary>
    /// Proxy class to assist in unit testing child process management functionality
    /// </summary>
    public class ProcessProxy : IProcessProxy
    {
        private readonly ILogger<ProcessProxy> _logger;

        public ProcessProxy(ILogger<ProcessProxy> logger)
        {
            _logger = logger;
        }

        public Task<(int ExitCode, string Output, string ErrorOutput)> ExecuteShellCommand(string command, params string[] args)
        {
            _logger.LogInformation("Executing shell command: {command} {arguments}", command, args);

            var tsc = new TaskCompletionSource<(int ExitCode, string Output, string ErrorOutput)>();
            
            var proc = new Process()
            {
                StartInfo =
                {
                    FileName = command,
                    Arguments = string.Join(" ", args),
                    UseShellExecute = false,
                    RedirectStandardError = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = true
                }
            };

            try
            {
                proc.Start();
            } catch(Exception ex)
            {
                Console.WriteLine("Failed!");
                Console.WriteLine(ex.Message);
            }

            return tsc.Task;
        }
    }
}
