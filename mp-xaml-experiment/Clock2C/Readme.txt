Clock2C


This plugin sends the current time in a message. It's a service, so you need another plugin to see what's going on.



Messages: 

Clock2C.Add
===========
If Clock2C receives this message, new key/value pairs are added to each Time message. If a key already exists and Clock2C is configured to AllowRedefinition, the key/Value pair is updated.

NewPairs
Holds the key/value pairs to add to the Time messages. The key must be a non-null string, the value must be a valid DateTime format string.

    public class Add : Message
    {
        public Dictionary<string, string> NewPairs;
    }



Clock2C.Time
============
Clock2C sends this message every Clock2CConfig.Interval milliseconds. The payload is found in Vars.

Vars
Holds key/value pairs. The key is a predefined value like "short-date", the value is the culture specific DateTime. The DateTime format can be (re)defined with the Add message.

    public class Time : Message
    {
        public Dictionary<string, string> Vars;
    }