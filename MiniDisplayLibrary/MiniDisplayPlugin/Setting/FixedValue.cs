using System;
using System.Xml.Serialization;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable]
  public abstract class FixedValue : Value
  {
    [XmlAttribute("Value")]
    public string value = "";

    protected FixedValue()
    {
    }
  }
}

