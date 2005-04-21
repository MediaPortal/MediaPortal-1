using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>
  /// The IsNullCondition evaluates a property and returns <b>true</b> if the property is empty
  /// </summary>
  [XmlType("IsNull")]
  public class IsNullCondition : Condition
  {
    /// <summary>
    /// Needed for XmlSerializer
    /// </summary>
    public IsNullCondition()
    {
    }

    public IsNullCondition(string _value)
    {
      Property = new Property(_value);
    }

    /// <summary>
    /// Evaluates the condition
    /// </summary>
    /// <returns><b>true</b> if the property is empty</returns>
    public override bool Evaluate()
    {
      if (Property.Evaluate().Length==0)
        return true;
      return false;
    }
  }
}
