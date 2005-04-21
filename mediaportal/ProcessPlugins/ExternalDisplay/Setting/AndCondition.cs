using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>
  /// This class represents an AND condition.  Every nested condition needs to evaluate to true, 
  /// in order for this condition to evalue to true
  /// </summary>
  [XmlType("And")]
	public class AndCondition : Condition
	{
    /// <summary>
    /// Parameterless constructor
    /// </summary>
    /// <remarks>Needed for the XmlSerializer</remarks>
		public AndCondition()
		{
      m_Conditions = new ArrayList();
		}

    public AndCondition(params Condition[] conditions)
    {
      m_Conditions = new ArrayList(conditions);
    }

    private ArrayList m_Conditions;
    /// <summary>
    /// The list of nested conditions
    /// </summary>
    [XmlElement("IsNull",typeof(IsNullCondition))]
    [XmlElement("NotNull",typeof(NotNullCondition))]
    [XmlElement("And",typeof(AndCondition))]
    [XmlElement("Or",typeof(OrCondition))]
    public IList Conditions
    {
      get { return m_Conditions; }
    }

    /// <summary>
    /// Evaluates this condition
    /// </summary>
    /// <returns>True if all nested conditions evaluate to true</returns>
	  public override bool Evaluate()
	  {
      for(int i=0; i<m_Conditions.Count; i++)
      {
        if (!((Condition)m_Conditions[i]).Evaluate())
          return false;
      }
      return true;
	  }
	}
}
