using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections
{
    [Serializable]
    public class InvalidDescriptorTagException : Exception
    {
        public InvalidDescriptorTagException() { }
        public InvalidDescriptorTagException(string message) : base(message) { }
        public InvalidDescriptorTagException(string message, Exception inner) : base(message, inner) { }
        protected InvalidDescriptorTagException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
