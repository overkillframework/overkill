using Overkill.Websockets.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Overkill.Websockets.Messages
{
    public class VehiclePingMessage : IWebsocketMessage
    {
        public DateTimeOffset Timestamp { get; set; }
    }
}
