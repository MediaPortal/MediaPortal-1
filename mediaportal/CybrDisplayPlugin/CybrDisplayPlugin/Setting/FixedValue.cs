namespace CybrDisplayPlugin.Setting
{
    using System;
    using System.Xml.Serialization;

    [Serializable]
    public abstract class FixedValue : Value
    {
        [XmlAttribute("Value")]
        public string value = "";

        protected FixedValue()
        {
        }
    }
}

