using Overkill.PubSub.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Overkill.Core.Topics.Lifecycle
{
    public class IsAliveTopic : IPubSubTopic
    {
        public DateTimeOffset Timestamp { get; set; }
    }
}
