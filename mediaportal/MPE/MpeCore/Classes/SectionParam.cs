using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
    public enum ValueTypeEnum
    {
        String,
        File,
        Template,
        Bool
    }
    public class SectionParam
    {
        public SectionParam()
        {
            Name = string.Empty;
            Value = string.Empty;
            ValueType = ValueTypeEnum.String;
            Description = string.Empty;
        }

        public SectionParam(SectionParam sectionParam)
        {
            Name = sectionParam.Name;
            Value = sectionParam.Value;
            ValueType = sectionParam.ValueType;
            Description = sectionParam.Description;
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

        /// <summary>
        /// Gets the value as a real path. 
        /// This function only usable if the type is Template
        /// </summary>
        /// <returns></returns>
        public string GetValueAsPath()
        {
            return MpeInstaller.TransformInRealPath(Value);
        }

        /// <summary>
        /// Gets the value as bool.
        /// This function only usable if the type is Bool 
        /// </summary>
        /// <returns></returns>
        public bool GetValueAsBool()
        {
            if (this.Value.ToUpper() == "YES")
                return true;
            return false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
