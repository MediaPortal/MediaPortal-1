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
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows
{
	[TypeConverter(typeof(PropertyPathConverter))]
	public sealed class PropertyPath
	{
		#region Constructors

		public PropertyPath()
		{
		}

		private PropertyPath(string path, object[] propertyInfoArray)
		{
			_path = path;
			_propertyInfoArray = propertyInfoArray;
		}

		#endregion Constructors

		#region Methods

		private static Type GetType(string type)
		{
			Type t = null;

			foreach(string ns in _namespaces)
			{
				t = Type.GetType(ns + "." + type);

				if(t != null)
					break;
			}

			return t;
		}

		private static object[] InnerParse(string path)
		{
			string[] parts = path.Split('.');

			ArrayList list = new ArrayList();

			// this should definately be improved upon!!!
			for(int index = 0; index < parts.Length; index += 2)
			{
				string typename = parts[index].Trim();
				string property = parts[index + 1].Trim();

				if(typename == string.Empty || typename.StartsWith("(") == false)
					throw new ArgumentException(string.Format("( expected)"));

				if(property == string.Empty || property.EndsWith(")") == false)
					throw new ArgumentException(string.Format(") expected)"));

				// remove the ( from the type specifier
				typename = typename.Substring(1);

				Type t = GetType(typename);

				if(t == null)
					throw new ArgumentException(string.Format("The type or namespace '{0}' could not be found", t));

				// remove the ) from the property name
				property = property.Substring(0, property.Length - 1);

				PropertyInfo propertyInfo = t.GetProperty(property, BindingFlags.Instance | BindingFlags.Public);

				if(propertyInfo  == null)
					throw new ArgumentException(string.Format("'{0}' does not contain a definition for '{1}'", t, property));

				list.Add(propertyInfo);
			}

			return list.ToArray();
		}

		public static PropertyPath Parse(string path)
		{
			return new PropertyPath(path, InnerParse(path));
		}

		#endregion Methods

		#region Properties

		public string Path
		{
			get { return _path; }
			set { if(string.Compare(_path, value, true) == 0) { _propertyInfoArray = null; } }
		}

//		public DependencyBindingCollection PathParameters
		public object[] PathParameters
		{
			get { if(_propertyInfoArray == null) _propertyInfoArray = InnerParse(_path); return _propertyInfoArray; }
		}

		#endregion Properties

		#region Fields

		string						_path = string.Empty;
		object[]					_propertyInfoArray;
		static string[]				_namespaces = MediaPortal.Xaml.XamlParser.DefaultNamespaces;

		#endregion Fields
	}
}
