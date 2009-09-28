using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
    public enum ValueTypeEnum
    {
        String,
        File
    }
    public class SectionParam
    {
        public SectionParam()
        {
            Name = string.Empty;
            Value = string.Empty;
            ValueType = ValueTypeEnum.String;
        }

        public SectionParam(string name, string value, ValueTypeEnum valueType, string description)
        {
            Name = name;
            Value = value;
            ValueType = valueType;
            Description = description;
        }
        [XmlAttribute]
        public string Name { get; set; }
        public string Value { get; set; }
        public ValueTypeEnum ValueType { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
