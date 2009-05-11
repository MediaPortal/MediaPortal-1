using MediaPortal.GUI.Library;
using System;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable]
  public class Property : FixedValue
  {
    public Property()
    {
    }

    public Property(string _text)
    {
      base.value = _text;
    }

    public Property(string _text, Condition _condition)
      : this(_text)
    {
      base.Condition = _condition;
    }

    protected override string DoEvaluate()
    {
      return GUIPropertyManager.GetProperty(base.value);
    }
  }
}

