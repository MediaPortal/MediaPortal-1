using System;
using System.Xml.Serialization;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable]
  public class Parse : Value
  {
    [XmlAttribute("Value")] public string value;

    public Parse()
    {
      this.value = "";
    }

    public Parse(string _text)
    {
      this.value = "";
      this.value = _text;
    }

    public Parse(string _text, Condition _condition)
      : this(_text)
    {
      base.Condition = _condition;
    }

    protected override string DoEvaluate()
    {
      return GUIPropertyManager.Parse(this.value);
    }
  }
}