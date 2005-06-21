using System.Collections;
using System.Text;
using System.Xml.Serialization;
using ExternalDisplay.Setting;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>
  /// This class represents a single display line
  /// </summary>
  public class Line
  {
    /// <summary>
    /// Needed for XmlSerializer
    /// </summary>
    public Line()
    {}

    public Line(string value)
    {
      values.Add(new Parse(value));
    }

    public Line(string value, Alignment alignment) : this(value)
    {
      Alignment = alignment;

    }

    public Line(Value value)
    {
      values.Add(value);
    }

    public Line(Value value, Alignment alignment) : this(value)
    {
      Alignment = alignment;
    }

    /// <summary>
    /// Text alignment for this line
    /// </summary>
    [XmlAttribute] public Alignment Alignment = Alignment.Left;

    /// <summary>
/// List of values to display on this line
/// </summary>
    [XmlElement("Text", typeof (Text))]
    [XmlElement("Property", typeof (Property))]
    [XmlElement("Parse", typeof (Parse))] public IList values = new ArrayList();

    /// <summary>
    /// Process the line
    /// </summary>
    /// <returns>the string to display</returns>
    public string Process()
    {
      StringBuilder s = new StringBuilder();
      foreach (Value val in values)
      {
        s.Append(val.Evaluate());
      }
      for(int i=0; i<Settings.Instance.TranslateFrom.Length;i++)
      {
        s.Replace(Settings.Instance.TranslateFrom[i],Settings.Instance.TranslateTo[i]);
      }
      return s.ToString();
    }
  }
}