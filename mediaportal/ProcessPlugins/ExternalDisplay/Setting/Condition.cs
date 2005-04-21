using System.ComponentModel;
using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>
  /// The abstract base class for all conditions
  /// </summary>
  [XmlInclude(typeof(NotNullCondition))]
  [XmlInclude(typeof(IsNullCondition))]
  [XmlInclude(typeof(AndCondition))]
  [XmlInclude(typeof(OrCondition))]
  public abstract class Condition
  {
    /// <summary>
    /// The property to evaluate
    /// </summary>
    [XmlIgnore]
    protected Property Property;

    [XmlAttribute]
    [DefaultValue("")]
    public string Value
    {
      get
      {
        if (Property==null)
          return "";
        return Property.value;
      }
      set { Property = new Property(value); }
    }

    /// <summary>
    /// Evaluates the condition
    /// </summary>
    /// <returns></returns>
    public abstract bool Evaluate();
  }
}
