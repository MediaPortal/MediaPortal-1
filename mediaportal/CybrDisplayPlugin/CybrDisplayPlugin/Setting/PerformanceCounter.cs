namespace CybrDisplayPlugin.Setting
{
    using System;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable]
    public class PerformanceCounter : Value
    {
        private string categoryName;
        private System.Diagnostics.PerformanceCounter counter;
        private string counterName;
        private string format;
        private string instanceName;

        public PerformanceCounter()
        {
        }

        public PerformanceCounter(string categoryName, string counterName, string instanceName)
        {
            this.categoryName = categoryName;
            this.counterName = counterName;
            this.instanceName = instanceName;
        }

        protected override string DoEvaluate()
        {
            if (this.counter == null)
            {
                return "";
            }
            float num = this.counter.NextValue();
            if (this.format == null)
            {
                return num.ToString();
            }
            return num.ToString(this.format);
        }

        private void Initialize()
        {
            if (((this.categoryName != null) && (this.counterName != null)) && (this.instanceName != null))
            {
                if (this.counter != null)
                {
                    this.counter.Dispose();
                }
                this.counter = new System.Diagnostics.PerformanceCounter(this.categoryName, this.counterName, this.instanceName);
            }
        }

        [XmlAttribute("CategoryName")]
        public string CategoryName
        {
            get
            {
                return this.categoryName;
            }
            set
            {
                this.categoryName = value;
                this.Initialize();
            }
        }

        [XmlAttribute("CounterName")]
        public string CounterName
        {
            get
            {
                return this.counterName;
            }
            set
            {
                this.counterName = value;
                this.Initialize();
            }
        }

        [XmlAttribute("Format")]
        public string Format
        {
            get
            {
                return this.format;
            }
            set
            {
                this.format = value;
            }
        }

        [XmlAttribute("InstanceName")]
        public string InstanceName
        {
            get
            {
                return this.instanceName;
            }
            set
            {
                this.instanceName = value;
                this.Initialize();
            }
        }
    }
}

