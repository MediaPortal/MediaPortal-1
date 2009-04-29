using System;
using System.Xml.Serialization;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable, XmlType("NotNull")]
  public class NotNullCondition : Condition
  {
    public NotNullCondition()
    {
    }

    public NotNullCondition(string _value)
    {
      base.Property = new Property(_value);
    }

    public override bool Evaluate()
    {
      return (base.Property.Evaluate().Length > 0);
    }
  }
}