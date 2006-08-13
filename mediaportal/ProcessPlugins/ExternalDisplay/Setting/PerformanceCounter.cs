using System;
using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
    /// <summary>Class for getting Windows Performance Counter values</summary>
    /// <author>JoeDalton</author>
    [Serializable]
    public class PerformanceCounter : Value
    {
        private string categoryName;
        private string counterName;
        private string instanceName;
        private string format;
        private System.Diagnostics.PerformanceCounter counter;

        public PerformanceCounter()
        {}
        
        public PerformanceCounter(string categoryName, string counterName, string instanceName)
        {
            this.categoryName = categoryName;
            this.counterName = counterName;
            this.instanceName = instanceName;
        }

        [XmlAttribute("CategoryName")]
        public string CategoryName
        {
            get { return categoryName; }
            set
            {
                categoryName = value;
                Initialize();
            }
        }

        [XmlAttribute("CounterName")]
        public string CounterName
        {
            get { return counterName; }
            set
            {
                counterName = value;
                Initialize();
            }
        }

        [XmlAttribute("InstanceName")]
        public string InstanceName
        {
            get { return instanceName; }
            set
            {
                instanceName = value;
                Initialize();
            }
        }

        [XmlAttribute("Format")]
        public string Format
        {
            get { return format; }
            set { format = value; }
        }

        public override string Evaluate()
        {
            if (counter == null)
            {
                return "";
            }
            float result = counter.NextValue();
            if (format == null)
                return result.ToString();
            return result.ToString(format);
        }

        private void Initialize()
        {
            if (categoryName == null || counterName == null || instanceName == null)
            {
                return;
            }
            if (counter != null)
            {
                counter.Dispose();
            }
            counter = new System.Diagnostics.PerformanceCounter(categoryName, counterName, instanceName);
        }
    }
}