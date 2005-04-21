using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>
  /// The NotNullCondition evaluates a property and returns <b>true</b> if the property is not empty
  /// </summary>
  [XmlType("NotNull")]
	public class NotNullCondition : Condition
	{
		public NotNullCondition()
		{
		}

    public NotNullCondition(string _value)
    {
      Property = new Property(_value);
    }

    /// <summary>
    /// Evaluates the condition
    /// </summary>
    /// <returns><b>true</b> if the property is not empty</returns>
    public override bool Evaluate()
    {
      if (Property.Evaluate().Length>0)
        return true;
      return false;
    }

	}
}
