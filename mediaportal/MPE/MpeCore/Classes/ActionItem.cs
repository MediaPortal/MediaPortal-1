using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MpeCore.Classes
{
    public class ActionItem
    {
        public ActionItem()
        {
            Params = new SectionParamCollection();
            ConditionGroup = string.Empty;
            ActionType = string.Empty;
            ExecuteLocation = ActionExecuteLocationEnum.AfterPanelShow;
        }

        public ActionItem(ActionItem obj)
        {
            Name = obj.Name;
            Params = Params;
            ConditionGroup = obj.ConditionGroup;
            ExecuteLocation = obj.ExecuteLocation;
        }

        public ActionItem(string actionType)
        {
            Name = actionType;
            ActionType = actionType;
            Params = new SectionParamCollection();
            ConditionGroup = string.Empty;
            ExecuteLocation = ActionExecuteLocationEnum.AfterPanelShow;
        }

        [XmlAttribute]
        public string Name { get; set; }
        
        [XmlAttribute]
        public string ActionType { get; set; }

        public SectionParamCollection Params { get; set; }
        [XmlAttribute]
        public string ConditionGroup { get; set; }

        public ActionExecuteLocationEnum ExecuteLocation { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
