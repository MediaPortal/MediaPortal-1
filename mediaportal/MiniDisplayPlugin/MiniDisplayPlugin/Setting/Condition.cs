using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable, XmlInclude(typeof(IsNullCondition)), XmlInclude(typeof(AndCondition)), XmlInclude(typeof(OrCondition)), XmlInclude(typeof(NotNullCondition))]
  public abstract class Condition
  {
    [XmlIgnore]
    protected MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting.Property Property;

    protected Condition()
    {
    }

    public abstract bool Evaluate();

    [DefaultValue(""), XmlAttribute]
    public string Value
    {
      get
      {
        if (this.Property == null)
        {
          return "";
        }
        return this.Property.value;
      }
      set
      {
        this.Property = new MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting.Property(value);
      }
    }
  }
}

