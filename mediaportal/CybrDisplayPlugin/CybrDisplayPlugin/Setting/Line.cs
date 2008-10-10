namespace CybrDisplayPlugin.Setting
{
    using CybrDisplayPlugin;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml.Serialization;

    [Serializable]
    public class Line
    {
        [XmlAttribute]
        public CybrDisplayPlugin.Setting.Alignment Alignment;
        [XmlElement("PerformanceCounter", typeof(PerformanceCounter)), XmlElement("TextProgressBar", typeof(TextProgressBar)), XmlElement("Parse", typeof(Parse)), XmlElement("Property", typeof(Property)), XmlElement("Text", typeof(Text))]
        public List<Value> values;

        public Line()
        {
            this.values = new List<Value>();
        }

        public Line(Value value)
        {
            this.values = new List<Value>();
            this.values.Add(value);
        }

        public Line(string value)
        {
            this.values = new List<Value>();
            this.values.Add(new Parse(value));
        }

        public Line(Value value, CybrDisplayPlugin.Setting.Alignment alignment) : this(value)
        {
            this.Alignment = alignment;
        }

        public Line(string value, CybrDisplayPlugin.Setting.Alignment alignment) : this(value)
        {
            this.Alignment = alignment;
        }

        public string Process()
        {
            StringBuilder builder = new StringBuilder();
            foreach (Value value2 in this.values)
            {
                builder.Append(value2.Evaluate());
            }
            for (int i = 0; i < Settings.Instance.TranslateFrom.Length; i++)
            {
                builder.Replace(Settings.Instance.TranslateFrom[i], Settings.Instance.TranslateTo[i]);
            }
            return builder.ToString();
        }
    }
}

