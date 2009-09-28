using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
    public class SectionItem
    {
        public SectionItem()
        {
            Params = new SectionParamCollection();
            IncludedGroups = new List<string>();
            Guid = System.Guid.NewGuid().ToString();
            ConditionGroup = string.Empty;
        }

        public SectionItem(SectionItem obj)
        {
            Name = obj.Name;
            Params = Params;
            IncludedGroups = obj.IncludedGroups;
            PanelName = obj.PanelName;
            ConditionGroup = obj.ConditionGroup;
        }
        [XmlAttribute]
        public string Guid { get; set; } 
        [XmlAttribute]
        public string Name { get; set; }
        public SectionParamCollection Params { get; set; }
        public List<string> IncludedGroups { get; set; }
        public string PanelName { get; set; }
        [XmlAttribute]
        public string ConditionGroup { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
