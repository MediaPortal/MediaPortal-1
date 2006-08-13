using System;
using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
    /// <author>JoeDalton</author>
    [Serializable]
    public abstract class FixedValue : Value
    {
        [XmlAttribute("Value")]
        public string value = "";
    }
}