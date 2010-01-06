using System;
using System.Xml.Serialization;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable, XmlType("IsNull")]
  public class IsNullCondition : Condition
  {
    public IsNullCondition() {}

    public IsNullCondition(string _value)
    {
      base.Property = new Property(_value);
    }

    public override bool Evaluate()
    {
      return (base.Property.Evaluate().Length == 0);
    }
  }
}