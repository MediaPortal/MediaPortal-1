using ProjectInfinity.Messaging;
using System.Collections.Generic;

namespace Clock2C
{
    /// <summary>
    /// If Clock2C receives this message, new key/value pairs are added to each Time message. If a key already exists and Clock2C is configured to AllowRedefinition, the key/Value pair is updated.
    /// </summary>
    public class Add : Message
    {
        /// <summary>
        /// Holds the key/value pairs to add to the Time messages. The key must be a non-null string, the value must be a valid DateTime format string.
        /// </summary>
        public Dictionary<string, string> NewPairs;
    }
}
