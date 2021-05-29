using Overkill.PubSub.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Overkill.Core.Topics.Control
{
    public class PlaySoundTopic : IPubSubTopic
    {
        public string URL { get; set; }
    }
}
