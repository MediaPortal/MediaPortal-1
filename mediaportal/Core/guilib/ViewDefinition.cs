using System;
using System.Collections;
using System.Net;
using System.Xml.Serialization;

namespace MediaPortal.GUI.View
{
	/// <summary>
	/// Summary description for ViewDefinition.
	/// </summary>
	[Serializable]
	public class ViewDefinition
	{
		protected ArrayList listFilters = new ArrayList();
		string							name;
		public ViewDefinition()
		{
		}

		[XmlElement("Name")]
		public string Name
		{
			get { return name;}
			set { name=value;}
		}

		[XmlElement("Filters")]
		public ArrayList Filters
		{
			get { return listFilters;}
			set { listFilters=value;}
		}
	}
}
