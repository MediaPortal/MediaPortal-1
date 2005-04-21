using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
	/// <summary>
	/// This class represents a display message
	/// </summary>
	/// <remarks>
	/// A message has a number of triggers like the MediaPortal status, a list of window IDs and a 
	/// condition.  Only if all triggers match the message will be sent to the 
	/// <see cref="DisplayHandler"/> who is responsible for formatting it and sending it to the display.
	/// </remarks>
	public class Message
	{
    /// <summary>
    /// MP status that will trigger this message
    /// </summary>
    [XmlAttribute]
    [DefaultValue(Status.Any)]
    public Status Status = Status.Any;

    /// <summary>
    /// List of active windows that will trigger this message
    /// </summary>
    [XmlElement("Window",typeof(int))]
    public IList Windows = new ArrayList();

    /// <summary>
    /// List of lines that this message contains
    /// </summary>
    [XmlElement("Line",typeof(Line))]
    public IList Lines = new ArrayList();

    /// <summary>
    /// Condition for this message
    /// </summary>
    [XmlElement("IsNull",typeof(IsNullCondition))]
    [XmlElement("NotNull",typeof(NotNullCondition))]
    [XmlElement("And",typeof(AndCondition))]
    [XmlElement("Or",typeof(OrCondition))]
    [DefaultValue(null)]
    public Condition Condition = null;

    /// <summary>
    /// Process the message
    /// </summary>
    /// <param name="_keeper">The <see cref="DisplayHandler"/> that will put this message on the display</param>
    /// <returns>A boolean, indicating whether this message is processed</returns>
    public bool Process(DisplayHandler _keeper)
    {
      if (Condition==null || Condition.Evaluate())
      {
        for(int i=0; i<Lines.Count; i++)
        {
            _keeper.SetLine(i,(Line)Lines[i]);
        }
        return true;
      }
      return false;
    }
	}
}
