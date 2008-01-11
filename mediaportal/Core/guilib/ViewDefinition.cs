#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.Net;
using System.Xml.Serialization;
using MediaPortal.GUI.Library;

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
        public string LocalizedName
        {
            get
            {
                String localizedName = this.name;
                GUILocalizeStrings.LocalizeLabel(ref localizedName);
                return localizedName;
            }
        }

        public override string ToString()
        {
            return LocalizedName;
        }
	}
}
