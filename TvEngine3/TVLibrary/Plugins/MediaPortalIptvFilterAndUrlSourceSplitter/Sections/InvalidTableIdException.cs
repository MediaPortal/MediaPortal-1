using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections
{
    [Serializable]
    public class InvalidTableIdException : Exception
    {
        public InvalidTableIdException() { }
        public InvalidTableIdException(string message) : base(message) { }
        public InvalidTableIdException(string message, Exception inner) : base(message, inner) { }
        protected InvalidTableIdException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
