using System;
using System.Collections.Generic;
using System.Text;

namespace Overkill.Common.Exceptions
{
    public class UpdateRequiredException : Exception
    {
        public UpdateRequiredException() : base("An update is underway, exiting...")
        {
            
        }
    }
}
