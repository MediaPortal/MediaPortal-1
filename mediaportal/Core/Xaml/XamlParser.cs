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
using System.Text;
using System.Windows.Serialization;
using System.Xml;

namespace MediaPortal.Xaml
{
	public class XamlParser
	{
		#region Methods

		public Type GetType(string type)
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

		public object Load(string filename)
		{
			_xmlReader = new XmlTextReader(_filename = filename);
			_xmlReader.WhitespaceHandling = WhitespaceHandling.None;
			_xmlReader.Namespaces = true;

			return Read();
		}

		public object LoadXml(string fragment, XmlNodeType type, object target)
		{
			_xmlReader = new XmlTextReader(fragment, type, null);
			_xmlReader.WhitespaceHandling = WhitespaceHandling.None;

			_elementStack.Push(target);

			return Read();
		}

		object InvokeGetter()
		{
			string[] tokens = _xmlReader.Name.Split('.');

			return InvokeGetter(tokens[0], tokens[1]);
		}

		object InvokeGetter(string type, string property)
		{
			Type t = GetType(type);

			if(t == null)
				throw new XamlParserException(string.Format("The type or namespace '{0}' could not be found", type), _filename, _xmlReader);

			// walk the stack looking for an item of the correct type
			foreach(object target in _elementStack)
			{
				if(t.IsInstanceOfType(target) == false)
					continue;

				PropertyInfo propertyInfo = t.GetProperty(property);

				if(propertyInfo == null)
					throw new XamlParserException(string.Format("'{0}' does not contain a definition for '{1}'", t, property), _filename, _xmlReader);

				return propertyInfo.GetValue(target, null);
			}
			
			// A local variable named 'b' is already defined in this scope
			throw new InvalidOperationException(string.Format("No instance of '{0}' is defined in this scope'", t));
		}

		void InvokeSetter(object value)
		{
			string[] tokens = _xmlReader.Name.Split('.');

			InvokeSetter(tokens[0], tokens[1], value);
		}
		
		void InvokeSetter(string type, string property, object value)
		{
			Type t = GetType(type);

			if(t == null)
				throw new XamlParserException(string.Format("The type or namespace '{0}' could not be found", type), _filename, _xmlReader);
			
			foreach(object target in _elementStack)
			{
				if(t.IsInstanceOfType(target) == false)
					continue;

				PropertyInfo propertyInfo = t.GetProperty(property);

				if(propertyInfo == null)
					throw new XamlParserException(string.Format("'{0}' does not contain a definition for '{1}'", t, property), _filename, _xmlReader);

				if(propertyInfo.CanWrite == false)
					break;

				if(propertyInfo.PropertyType == typeof(object))
				{
					propertyInfo.SetValue(target, _target, null);
					break;
				}

				TypeConverter typeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);

				if(typeConverter is ICanAddNamespaceEntries)
					((ICanAddNamespaceEntries)typeConverter).AddNamespaceEntries(_namespaces);

				try
				{
					propertyInfo.SetValue(target, typeConverter.ConvertFrom(_target), null);
				}
				catch(FormatException)
				{
					throw new XamlParserException(string.Format("Cannot convert '{0}' to type '{1}'", _xmlReader.Value, propertyInfo.PropertyType), _filename, _xmlReader);
				}

				break;
			}
		}

		object Read()
		{
			while(_xmlReader.Read())
			{
				try
				{
					switch(_xmlReader.NodeType)
					{
						case XmlNodeType.Element:

							if(_xmlReader.Name.IndexOf('.') == -1)
							{
								ReadElement();
							}
							else
							{
								ReadElementCompoundProperty();
							}
							
							break;
						
						case XmlNodeType.Text:
						case XmlNodeType.CDATA:
							_elementText.Append(_xmlReader.Value);
							break;

						case XmlNodeType.EndElement:
							
							if(_xmlReader.Name.IndexOf('.') == -1)
							{
								ReadElementEnd();
							}
							else
							{
								ReadElementEndCompoundProperty();
							}

							break;
					}
				}
				catch(XamlParserException e)
				{
					MediaPortal.GUI.Library.Log.Write("XamlParser.Read: {0}", e.Message);
					MediaPortal.GUI.Library.Log.Write("Node: {0}", _xmlReader.Name);
				}
				catch(Exception e)
				{
					MediaPortal.GUI.Library.Log.Write("XamlParser.Read: {0}({1},{2}): {3}", _filename, _xmlReader.LineNumber, _xmlReader.LinePosition, e.Message);
					MediaPortal.GUI.Library.Log.Write("Node: {0}", _xmlReader.Name);
				}
			}

			_xmlReader.Close();
			_xmlReader = null;

			return _target;
		}

		void ReadAttributes()
		{
			object target = _elementStack.Peek();

			for(int index = 0; index < _xmlReader.AttributeCount; index++)
			{
				_xmlReader.MoveToAttribute(index);

				string name = _xmlReader.Name.Trim();
				string value = _xmlReader.Value.Trim();

				if(name.StartsWith("xmlns"))
					continue;

				if(string.Compare(name, "Name") == 0 || name.EndsWith(":Name"))
				{
					_namedItems[value] = target;
					continue;
				}

				if(string.Compare(name, "Key") == 0 || name.EndsWith(":Key"))
				{
					if(value.StartsWith("{"))
					{
						Application.Current.Resources.Add(ReadExtension(value), _target);
					}
					else
					{
						Application.Current.Resources.Add(value, _target);
					}

					continue;
				}

				Type t = target.GetType();

				PropertyInfo propertyInfo = t.GetProperty(name);

				if(propertyInfo == null)
					throw new XamlParserException(string.Format("'{0}' does not contain a definition for '{1}'", t, name), _filename, _xmlReader);

				if(value.StartsWith("{"))
				{
					propertyInfo.SetValue(target, ReadExtension(value), null);
					continue;
				}

				if(propertyInfo.PropertyType == typeof(object))
				{
					propertyInfo.SetValue(target, _xmlReader.Value, null);
					continue;
				}

				TypeConverter typeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);

				if(typeConverter is ICanAddNamespaceEntries)
					((ICanAddNamespaceEntries)typeConverter).AddNamespaceEntries(_namespaces);

				try
				{
					object convertedValue = typeConverter.ConvertFromString(_xmlReader.Value);

					propertyInfo.SetValue(target, convertedValue, null);
				}
				catch(FormatException)
				{
					throw new XamlParserException(string.Format("Cannot convert '{0}' to type '{1}'", _xmlReader.Value, propertyInfo.PropertyType), _filename, _xmlReader);
				}
			}
		}

		void ReadElement()
		{
			_elementText = _elementText.Length > 0 ? new StringBuilder() : _elementText;

			Type targetType = GetType(_xmlReader.Name);

			if(targetType == null)
				throw new XamlParserException(string.Format("The type or namespace '{0}' could not be found", _xmlReader.Name), _filename, _xmlReader);

			_target = Activator.CreateInstance(targetType);

			if(_target is ISupportInitialize)
				((ISupportInitialize)_target).BeginInit();

			if(_elementStack.Count != 0 && _elementStack.Peek() is IAddChild)
				((IAddChild)_elementStack.Peek()).AddChild(_target);

			if(_elementStack.Count != 0 && _elementStack.Peek() is IList)
				((IList)_elementStack.Peek()).Add(_target);

			_elementStack.Push(_target);

			bool isEmptyElement = _xmlReader.IsEmptyElement;

			ReadAttributes();

			if(isEmptyElement)
				ReadElementEnd();
		}

		void ReadElementCompoundProperty()
		{
			_elementStack.Push(InvokeGetter());
		}

		void ReadElementEnd()
		{
			_target = _elementStack.Pop();

			if(_target is IAddChild && _elementText.Length > 0)
				((IAddChild)_target).AddText(_elementText.ToString());

			if(_target is ISupportInitialize)
				((ISupportInitialize)_target).EndInit();
		}

		void ReadElementEndCompoundProperty()
		{
			InvokeSetter(_elementStack.Pop());
		}

		object ReadExtension(string value)
		{
			if(value.EndsWith("}") == false)
				throw new XamlParserException("} expected", _filename, _xmlReader);

			value = value.Substring(1, value.Length - 2).TrimStart();

			int endOfExtensionNameIndex = value.IndexOf(' ');

			string name;

			if(endOfExtensionNameIndex == -1)
			{
				name = value.TrimEnd();
				value = string.Empty;
			}
			else
			{
				name = value.Substring(0, endOfExtensionNameIndex);
				value = value.Substring(endOfExtensionNameIndex).Trim();
			}

			if(name.IndexOf(':') != -1)
				name = name.Substring(name.IndexOf(':') + 1);

			Type t = GetType(name + "Extension");

			if(t == null)
				t = GetType(name);

			if(t == null)
				throw new XamlParserException(string.Format("The parser extension '{0}' could not be found", name), _filename, _xmlReader);

			if(t.IsSubclassOf(typeof(MarkupExtension)) == false)
				throw new XamlParserException(string.Format("'{0}' is not of type 'System.Windows.Serialization.MarkupExtension'", t), _filename, _xmlReader);

			MarkupExtension extension = (MarkupExtension)Activator.CreateInstance(t);

			if(extension is ICanAddNamespaceEntries)
				((ICanAddNamespaceEntries)extension).AddNamespaceEntries(_namespaces);

			return extension.ProvideValue(_target, value);
		}

		void ReadNamespace(string name, string value)
		{
			string ns = string.Empty;

			int nameIndex = _xmlReader.Name.IndexOf(':');

			if(nameIndex != -1)
				ns = _xmlReader.Name.Substring(nameIndex);
		}

		#endregion Methods

		#region Fields

		StringBuilder				_elementText = new StringBuilder();
		Stack						_elementStack = new Stack();
		string						_filename = string.Empty;
		Hashtable					_namedItems = new Hashtable();
		static string[]				_namespaces = new string[] { "MediaPortal", "MediaPortal.Controls", "MediaPortal.Drawing", "MediaPortal.Drawing.Shapes", "MediaPortal.Drawing.Transforms", "MediaPortal.Animation", "System.Windows", "System.Windows.Serialization", "MediaPortal.Drawing.Paths", "MediaPortal.GUI.Library" };
		object						_target;
		XmlTextReader				_xmlReader;

		#endregion Fields
	}
}
