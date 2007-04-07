using System;
using System.Collections.Generic;
using System.Text;

namespace Clock2C
{
    /// <summary>
    /// This class holds the configuration of Clock2C.
    /// </summary>
    class Clock2CConfig
    {
		public double Interval; // The interval in milliseconds Clock2C sends its Time message
        public Dictionary<string, string> KVPairs; // Holds the key names and value definitions
		public bool AllowRedefinition; // If true, the Add message may override a key/value pair
        
		public Clock2CConfig()
		{
			KVPairs = new Dictionary<string, string>();
			AllowRedefinition = true;
		}
    }
}
