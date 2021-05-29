﻿using Overkill.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Overkill.Core.Connections.Data
{
    public struct TcpData : ICommunicationPayload
    {
        public byte[] Data { get; set; }
    }
}
