using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable, XmlInclude(typeof (TextProgressBar)), XmlInclude(typeof (PerformanceCounter)),
   XmlInclude(typeof (Property)), XmlInclude(typeof (Text)), XmlInclude(typeof (Parse))]
  public abstract class Value
  {
    [XmlElement("Or", typeof (OrCondition)), XmlElement("IsNull", typeof (IsNullCondition)), DefaultValue((string) null)
    , XmlElement("NotNull", typeof (NotNullCondition)), XmlElement("And", typeof (AndCondition))] public Condition
      Condition;

    protected Value()
    {
    }

    protected abstract string DoEvaluate();

    public string Evaluate()
    {
      if ((this.Condition != null) && !this.Condition.Evaluate())
      {
        return "";
      }
      return this.DoEvaluate();
    }
  }
}