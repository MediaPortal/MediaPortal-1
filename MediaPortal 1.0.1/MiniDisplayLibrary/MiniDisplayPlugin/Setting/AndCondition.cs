using System;
using System.Collections;
using System.Xml.Serialization;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable, XmlType("And")]
  public class AndCondition : Condition
  {
    private ArrayList m_Conditions;

    public AndCondition()
    {
      this.m_Conditions = new ArrayList();
    }

    public AndCondition(params Condition[] conditions)
    {
      this.m_Conditions = new ArrayList(conditions);
    }

    public override bool Evaluate()
    {
      for (int i = 0; i < this.m_Conditions.Count; i++)
      {
        if (!((Condition) this.m_Conditions[i]).Evaluate())
        {
          return false;
        }
      }
      return true;
    }

    [XmlElement("Or", typeof (OrCondition)), XmlElement("IsNull", typeof (IsNullCondition)),
     XmlElement("NotNull", typeof (NotNullCondition)), XmlElement("And", typeof (AndCondition))]
    public IList Conditions
    {
      get { return this.m_Conditions; }
    }
  }
}