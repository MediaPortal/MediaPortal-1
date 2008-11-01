namespace CybrDisplayPlugin.Setting
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    [Serializable, XmlInclude(typeof(IsNullCondition)), XmlInclude(typeof(AndCondition)), XmlInclude(typeof(OrCondition)), XmlInclude(typeof(NotNullCondition))]
    public abstract class Condition
    {
        [XmlIgnore]
        protected CybrDisplayPlugin.Setting.Property Property;

        protected Condition()
        {
        }

        public abstract bool Evaluate();

        [DefaultValue(""), XmlAttribute]
        public string Value
        {
            get
            {
                if (this.Property == null)
                {
                    return "";
                }
                return this.Property.value;
            }
            set
            {
                this.Property = new CybrDisplayPlugin.Setting.Property(value);
            }
        }
    }
}

