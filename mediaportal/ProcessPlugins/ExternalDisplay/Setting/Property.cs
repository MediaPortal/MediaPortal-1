using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay.Setting
{
	/// <summary>
	/// The Property class represents a single property reference.
	/// </summary>
	public class Property : Value
	{
		public Property()
		{
		}

    public Property(string _text)
    {
      value = _text;
    }

    public Property(string _text, Condition _condition) : this(_text)
    {
      Condition = _condition;
    }

    /// <summary>
    /// Evaluates the property
    /// </summary>
    /// <returns>
    /// The property's value, or an empty string if the associated <see cref="Condition"/> evaluates to false.
    /// </returns>
    public override string Evaluate()
    {
      if (Condition==null || Condition.Evaluate())
      {
        return GUIPropertyManager.GetProperty(value);
      }
      return "";
    }

	}
}
