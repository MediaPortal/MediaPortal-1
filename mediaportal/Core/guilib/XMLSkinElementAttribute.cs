using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Indicates that a field can be initialized from XML skin data.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class XMLSkinElementAttribute : Attribute
	{
		string m_xmlElementName;
		public XMLSkinElementAttribute(string xmlElementName)
		{
			m_xmlElementName = xmlElementName;
		}

		public string XmlElementName
		{
			get { return m_xmlElementName; }
		}
	}
}
