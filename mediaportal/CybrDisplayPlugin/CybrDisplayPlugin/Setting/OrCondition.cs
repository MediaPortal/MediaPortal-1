namespace CybrDisplayPlugin.Setting
{
    using System;
    using System.Collections;
    using System.Xml.Serialization;

    [Serializable, XmlType("Or")]
    public class OrCondition : Condition
    {
        private ArrayList m_Conditions = new ArrayList();

        public override bool Evaluate()
        {
            for (int i = 0; i < this.m_Conditions.Count; i++)
            {
                if (((Condition) this.m_Conditions[i]).Evaluate())
                {
                    return true;
                }
            }
            return false;
        }

        [XmlArray, XmlArrayItem(typeof(Condition))]
        public IList Conditions
        {
            get
            {
                return this.m_Conditions;
            }
        }
    }
}

