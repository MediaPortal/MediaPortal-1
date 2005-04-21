using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>
  /// The Parse class represents a string containing references to properties.
  /// </summary>
  /// <remarks>
  /// When the Parse is evaluated all references to properties are replaces by their property values.
  /// </remarks>
  public class Parse : Value
  {
    public Parse()
    {
    }

    public Parse(string _text)
    {
      value = _text;
    }

    public Parse(string _text, Condition _condition) : this(_text)
    {
      Condition = _condition;
    }

    /// <summary>
    /// Evaluates the <see cref="Parse"/>.
    /// </summary>
    /// <returns>The Parse string with all propertie references replaced by their values, or an empty
    /// string if the associated <see cref="Condition"/> evaluates to false.</returns>
    public override string Evaluate()
    {
      if (Condition==null || Condition.Evaluate())
      {
        return GUIPropertyManager.Parse(value);
      }
      return "";
    }

  }
}
