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
using System.ComponentModel;
using System.Globalization;
using System.Windows.Serialization;

namespace System.Windows
{
	public sealed class RoutedEventConverter : TypeConverter, ICanAddNamespaceEntries
	{
		#region Methods

		void ICanAddNamespaceEntries.AddNamespaceEntries(string[] namespaces)
		{
			_namespaces = namespaces;
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type t)
		{
			if(t == typeof(string))
				return true;

			return base.CanConvertFrom(context, t);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if(value is string)
				return Parse((string)value);

			return base.ConvertFrom(context, culture, value);
		}

		public RoutedEvent Parse(string text)
		{
			string[] tokens = text.Split('.');

			if(tokens.Length != 2)
				throw new ArgumentException("Expecting 'Type.Name'");

			Type type = null;

			foreach(string ns in _namespaces)
			{
				type = Type.GetType(ns + "." + tokens[0]);

				if(type != null)
					break;
			}

			if(type == null)
				throw new InvalidOperationException(string.Format("The type or namespace '{0}' could not be found", tokens[0]));

			return EventManager.GetRoutedEventFromName(tokens[1], type);
		}
		
		#endregion Methods

		#region Fields

		string[]					_namespaces = null;

		#endregion Fields
	}
}