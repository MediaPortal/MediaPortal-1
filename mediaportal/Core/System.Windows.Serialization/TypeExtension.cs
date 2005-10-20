#region Copyright (C) 2005 Media Portal

/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

namespace System.Windows.Serialization
{
	public class TypeExtension : MarkupExtension
	{
		#region Methods

		public override object ProvideValue(object target, object value)
		{
			Type t = null;

			foreach(string ns in _namespaces)
			{
				t = Type.GetType(ns + "." + (string)value);

				if(t != null)
					return t;
			}

			return null;
		}

		#endregion Methods

		#region Fields

		static string[]				_namespaces = new string[] { "MediaPortal", "MediaPortal.Controls", "MediaPortal.Drawing", "MediaPortal.Drawing.Shapes", "MediaPortal.Drawing.Transforms", "MediaPortal.Animation", "System.Windows", "System.Windows.Serialization", "MediaPortal.Drawing.Paths" };

		#endregion Fields
	}
}
