using ProjectInfinity.Messaging;
using System.Collections.Generic;

namespace Clock2C
{
    /// <summary>
    /// Clock2C sends this message every Clock2CConfig.Interval milliseconds. The payload is found in Vars.
    /// </summary>
    public class Time : Message
    {
        /// <summary>
        /// Holds key/value pairs. The key is a predefined value like "short-date", the value is the culture specific DateTime. The DateTime format can be (re)defined with the Add message.
        /// </summary>
        public Dictionary<string, string> Vars;

        public Time()
		{
            Vars = new Dictionary<string, string>();
		}
    }
}
