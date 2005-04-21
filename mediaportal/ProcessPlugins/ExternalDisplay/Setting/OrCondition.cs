using System.Collections;
using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>
  /// The OrCondition evaluates all its nested <seealso cref="Condition"/>s. If one of the nested 
  /// conditions evaluates to true, the OrCondition is true
  /// </summary>
  [XmlType("Or")]
	public class OrCondition : Condition
	{
		public OrCondition()
		{
		}

    private ArrayList m_Conditions = new ArrayList();
    [XmlArray]
    [XmlArrayItem(typeof(Condition))]
    public IList Conditions
    {
      get { return m_Conditions; }
    }

    /// <summary>
    /// Evaluates the condition
    /// </summary>
    /// <returns><b>true</b> if one of the nested conditions is true</returns>
	  public override bool Evaluate()
	  {
      for(int i=0; i<m_Conditions.Count; i++)
      {
        if (((Condition)m_Conditions[i]).Evaluate())
          return true;
      }
      return false;
	  }
	}
}
