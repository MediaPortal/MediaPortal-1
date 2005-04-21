using System.ComponentModel;
using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
	/// <summary>
	/// The abstract base class for all values.
	/// </summary>
	[XmlInclude(typeof(Property))]
  [XmlInclude(typeof(Text))]
  [XmlInclude(typeof(Parse))]
	public abstract class Value
	{

    [XmlAttribute("Value")]
    public string value = "";


    [XmlElement("IsNull",typeof(IsNullCondition))]
    [XmlElement("NotNull",typeof(NotNullCondition))]
    [XmlElement("And",typeof(AndCondition))]
    [XmlElement("Or",typeof(OrCondition))]
    [DefaultValue(null)]
    public Condition Condition = null;

    public abstract string Evaluate();
  }
}
