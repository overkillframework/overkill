using Microsoft.Extensions.Logging;
using Overkill.Proxies.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace Overkill.Proxies
{
    /// <summary>
    /// Proxy class to assist in unit testing serial port communication functionality
    /// </summary>
    public class SerialProxy : ISerialProxy
    {
        private readonly ILogger<SerialProxy> _logger;
        private readonly SerialPort _serialPort;

        ISerialProxy Create(string device, int baudRate)
            => new SerialProxy(_logger, device, baudRate);

        public bool IsOpen => _serialPort.IsOpen;

        public SerialProxy(ILogger<SerialProxy> logger) 
        {
            _logger = logger;
        }

        public SerialProxy(ILogger<SerialProxy> logger, string device, int baudRate)
        {
            _logger = logger;
            _serialPort = new SerialPort(device, baudRate);
        }

        public void Close()
        {
            _logger.LogInformation("Closing serial connection to {port}", _serialPort.PortName);
            _serialPort.Close();
        }

        public void Open()
        {
            _logger.LogInformation("Opening serial connection to {port}", _serialPort.PortName);
            _serialPort.Open();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            _logger.LogDebug("Reading {bytes} bytes of data from serial port {port}", count, _serialPort.PortName);
            return _serialPort.Read(buffer, offset, count);
        }

        public void Write(string data)
        {
            _logger.LogDebug("Writing to serial port {port}: {data}", _serialPort.PortName, data);
            _serialPort.Write(data);
        }
    }
}
