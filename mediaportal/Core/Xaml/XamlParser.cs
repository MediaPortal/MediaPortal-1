using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Windows.Serialization;
using System.Xml;

namespace MediaPortal.Xaml
{
	public class XamlParser : IDisposable
	{
		#region Methods

		public void Dispose()
		{
			if(_xmlReader != null)
			{
				_xmlReader.Close();
				_xmlReader = null;
			}
		}

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
			_xmlReader.XmlResolver = null;
			_xmlReader.Namespaces = true;

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
#if !DEBUG 
					System.Windows.Forms.MessageBox.Show(e.Message);
#endif
					Console.WriteLine(e.Message);
				}
				catch(Exception e)
				{
#if !DEBUG 
					System.Windows.Forms.MessageBox.Show(string.Format("{0}({1},{2}): {3}", _filename, _xmlReader.LineNumber, _xmlReader.LinePosition, e.Message));
#endif
					Console.WriteLine("{0}({1},{2}): {3}", _filename, _xmlReader.LineNumber, _xmlReader.LinePosition, e.Message);
				}
			}

			return _target;
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

			if(_elementStack.Count > 0 && _elementStack.Peek() is IAddChild)
				((IAddChild)_elementStack.Peek()).AddChild(_target);

			if(_elementStack.Count > 0 && _elementStack.Peek() is IList)
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

			return ((MarkupExtension)Activator.CreateInstance(t)).ProvideValue(_target, value);
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
		string[]					_namespaces = new string[] { "MediaPortal", "MediaPortal.Controls", "MediaPortal.Drawing", "MediaPortal.Drawing.Shapes", "MediaPortal.Drawing.Transforms", "MediaPortal.Animation", "System.Windows", "System.Windows.Serialization", "MediaPortal.Drawing.Paths" };
		object						_target;
		XmlTextReader				_xmlReader;

		#endregion Fields
	}
}
