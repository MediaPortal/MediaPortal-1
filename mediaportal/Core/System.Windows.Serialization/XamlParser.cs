#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using MediaPortal.GUI.Library;

namespace System.Windows.Serialization
{
  public sealed class XamlParser
  {
    #region Constructors

    static XamlParser() {}

    #endregion Constructors

    #region Methods

    private Type GetType(string type)
    {
      Type t = null;

      foreach (string ns in _namespaces)
      {
        t = Type.GetType(ns + "." + type);

        if (t != null)
          break;
      }

      return t;
    }

    private TypeConverter GetTypeConverter(Type type)
    {
      TypeConverter converter = TypeDescriptor.GetConverter(type);

      if (converter is ICanAddNamespaceEntries)
        ((ICanAddNamespaceEntries)converter).AddNamespaceEntries(_namespaces);

      return converter;
    }

    public static object LoadXml(string filename)
    {
      XamlParser parser = new XamlParser();

      return parser.Read(filename);
    }

    public static object LoadXml(Stream stream)
    {
      throw new NotImplementedException();
    }

    public static object LoadXml(XmlReader reader)
    {
      XamlParser parser = new XamlParser();

      return parser.Read(reader);
    }

    public static object LoadXml(string fragment, XmlNodeType xmlNodeType, object target)
    {
      XamlParser parser = new XamlParser();

      return parser.Read(fragment, xmlNodeType, target);
    }

    private object InvokeGetter()
    {
      string[] tokens = _reader.Name.Split('.');

      return InvokeGetter(tokens[0], tokens[1]);
    }

    private object InvokeGetter(string type, string property)
    {
      Type t = GetType(type);

      if (t == null)
        throw new XamlParserException(string.Format("The type or namespace '{0}' could not be found", type), _filename,
                                      _reader);

      object target = WalkStackForInstanceOf(t);

      if (target == null)
        throw new XamlParserException(string.Format("No instance of '{0}' is defined in this scope", type), _filename,
                                      _reader);

      PropertyInfo propertyInfo = t.GetProperty(property);

      if (propertyInfo == null)
        throw new XamlParserException(string.Format("'{0}' does not contain a definition for '{1}'", t, property),
                                      _filename, _reader);

      object value = propertyInfo.GetValue(target, null);

      if (value == null && propertyInfo.CanWrite)
        value = Activator.CreateInstance(propertyInfo.PropertyType);

      return value;
    }

    private void InvokeSetter(object value)
    {
      string[] tokens = _reader.Name.Split('.');

      InvokeSetter(tokens[0], tokens[1], value);
    }

    private void InvokeSetter(string type, string property, object value)
    {
      Type t = GetType(type);

      if (t == null)
        throw new XamlParserException(string.Format("The type or namespace '{0}' could not be found", type), _filename,
                                      _reader);

      object target = WalkStackForInstanceOf(t);

      PropertyInfo propertyInfo = t.GetProperty(property);

      if (propertyInfo == null)
        throw new XamlParserException(string.Format("'{0}' does not contain a definition for '{1}'", t, property),
                                      _filename, _reader);

      if (propertyInfo.CanWrite == false)
        return;

      if (propertyInfo.PropertyType == typeof (object))
      {
        propertyInfo.SetValue(target, _target, null);
      }
      else if (propertyInfo.PropertyType.IsInstanceOfType(value))
      {
        propertyInfo.SetValue(target, _target, null);
      }
      else
      {
        TypeConverter typeConverter = GetTypeConverter(propertyInfo.PropertyType);

        try
        {
          propertyInfo.SetValue(target, typeConverter.ConvertFrom(_target), null);
        }
        catch (FormatException)
        {
          throw new XamlParserException(
            string.Format("Cannot convert '{0}' to type '{1}'", _reader.Value, propertyInfo.PropertyType), _filename,
            _reader);
        }
      }
    }

    private object Read(XmlReader reader)
    {
      throw new NotImplementedException();
    }

    private object Read(string filename)
    {
      _reader = new XmlTextReader(_filename = filename);

      return Read();
    }

    private object Read(string fragment, XmlNodeType xmlNodeType, object target)
    {
      _reader = new XmlTextReader(fragment, xmlNodeType, null);
      _elementStack.Push(target);

      return Read();
    }

    private object Read()
    {
      while (_reader.Read())
      {
        try
        {
          switch (_reader.NodeType)
          {
            case XmlNodeType.Element:

              if (_reader.Name.IndexOf('.') == -1)
                ReadElement();
              else
                ReadElementCompoundProperty();

              break;

            case XmlNodeType.Text:
            case XmlNodeType.CDATA:
              _elementText.Append(_reader.Value);
              break;

            case XmlNodeType.EndElement:

              if (_reader.Name.IndexOf('.') == -1)
                ReadElementEnd();
              else
                ReadElementEndCompoundProperty();

              break;
          }
        }
        catch (XamlParserException e)
        {
          Log.Info("XamlParser.Read: {0}", e.Message);
        }
        catch (Exception e)
        {
          Log.Info("XamlParser.Read: {0}({1},{2}): {3}", _filename, _reader.LineNumber, _reader.LinePosition, e.Message);
        }
      }

      _reader.Close();
      _reader = null;

      return _target;
    }

    private void ReadAttributes()
    {
      object target = _elementStack.Peek();

      for (int index = 0; index < _reader.AttributeCount; index++)
      {
        _reader.MoveToAttribute(index);

        string name = _reader.Name.Trim();
        string value = _reader.Value.Trim();

        if (name.StartsWith("xmlns"))
          continue;

        if (string.Compare(name, "Name") == 0 || name.EndsWith(":Name"))
        {
          INameScope nameScope = (INameScope)WalkStackForSubclassOf(typeof (INameScope));

          if (nameScope != null)
          {
            nameScope.RegisterName(value, target);
          }
          else
          {
            // there is no object in the stack that handles name registration so
            // we register the name with the Application's resource dictionary
            MediaPortal.App.Current.Resources.RegisterName(value, target);
          }
        }

        if (string.Compare(name, "Key") == 0 || name.EndsWith(":Key"))
        {
          if (value.StartsWith("{"))
            MediaPortal.App.Current.Resources.Add(ReadExtension(value), _target);
          else
            MediaPortal.App.Current.Resources.Add(value, _target);

          continue;
        }

        MemberInfo memberInfo;
        Type t;

        if (name.IndexOf('.') != -1)
        {
          string[] tokens = name.Split('.');

          t = GetType(tokens[0]);

          memberInfo = t.GetMethod("Set" + tokens[1], BindingFlags.Public | BindingFlags.Static);
          name = tokens[1];
        }
        else
        {
          t = target.GetType();
          memberInfo = t.GetProperty(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

          if (memberInfo == null)
            memberInfo = t.GetEvent(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        if (memberInfo == null)
          throw new XamlParserException(string.Format("'{0}' does not contain a definition for '{1}'", t, name),
                                        _filename, _reader);

        if (value.StartsWith("{"))
        {
          if (memberInfo is PropertyInfo)
          {
            ((PropertyInfo)memberInfo).SetValue(target, ReadExtension(value), null);
          }
          else if (memberInfo is MethodInfo)
          {
            ((MethodInfo)memberInfo).Invoke(null, new object[] {target, ReadExtension(value)});
          }

          continue;
        }

        if (memberInfo is MethodInfo)
        {
          MethodInfo methodInfo = (MethodInfo)memberInfo;

          TypeConverter typeConverter = GetTypeConverter(methodInfo.GetParameters()[1].ParameterType);

          try
          {
            object convertedFromString = typeConverter.ConvertFromString(_reader.Value);

            methodInfo.Invoke(null, new object[] {target, convertedFromString});
          }
          catch (FormatException)
          {
            throw new XamlParserException(
              string.Format("Cannot convert '{0}' to type '{1}'", _reader.Value,
                            methodInfo.GetParameters()[1].ParameterType), _filename, _reader);
          }

          continue;
        }
        else if (memberInfo is PropertyInfo)
        {
          PropertyInfo propertyInfo = (PropertyInfo)memberInfo;

          if (propertyInfo.PropertyType == typeof (object))
          {
            propertyInfo.SetValue(target, _reader.Value, null);
            continue;
          }

          TypeConverter typeConverter = GetTypeConverter(propertyInfo.PropertyType);

          try
          {
            object convertedFromString = typeConverter.ConvertFromString(_reader.Value);

            if (memberInfo is PropertyInfo)
              propertyInfo.SetValue(target, convertedFromString, null);
          }
          catch (FormatException)
          {
            throw new XamlParserException(
              string.Format("Cannot convert '{0}' to type '{1}'", _reader.Value, propertyInfo.PropertyType), _filename,
              _reader);
          }
        }
        else if (memberInfo is EventInfo)
        {
          // TODO: Hook up to event
          Log.Info("Event: {0}", memberInfo.Name);
        }
      }
    }

    private void ReadElement()
    {
      _elementText = _elementText.Length > 0 ? new StringBuilder() : _elementText;

      Type type = GetType(_reader.Name);

      if (type == null)
        throw new XamlParserException(string.Format("The type or namespace '{0}' could not be found", _reader.Name),
                                      _filename, _reader);

      _target = Activator.CreateInstance(type);

      if (_target is ISupportInitialize)
        ((ISupportInitialize)_target).BeginInit();

      if (_elementStack.Count != 0 && _elementStack.Peek() is IAddChild)
        ((IAddChild)_elementStack.Peek()).AddChild(_target);
      else if (_elementStack.Count != 0 && _elementStack.Peek() is IList)
        ((IList)_elementStack.Peek()).Add(_target);

      _elementStack.Push(_target);

      bool isEmptyElement = _reader.IsEmptyElement;

      ReadAttributes();

      if (isEmptyElement)
        ReadElementEnd();
    }

    private void ReadElementCompoundProperty()
    {
      _elementStack.Push(InvokeGetter());
    }

    private void ReadElementEnd()
    {
      _target = _elementStack.Pop();

      if (_target is IAddChild && _elementText.Length > 0)
        ((IAddChild)_target).AddText(_elementText.ToString());

      if (_target is ISupportInitialize)
        ((ISupportInitialize)_target).EndInit();
    }

    private void ReadElementEndCompoundProperty()
    {
      InvokeSetter(_target = _elementStack.Pop());
    }

    private object ReadExtension(string value)
    {
      if (value.EndsWith("}") == false)
        throw new XamlParserException("} expected", _filename, _reader);

      value = value.Substring(1, value.Length - 2).TrimStart();

      int endOfExtensionNameIndex = value.IndexOf(' ');

      string name;

      if (endOfExtensionNameIndex == -1)
      {
        name = value.TrimEnd();
        value = string.Empty;
      }
      else
      {
        name = value.Substring(0, endOfExtensionNameIndex);
        value = value.Substring(endOfExtensionNameIndex).Trim();
      }

      if (name.IndexOf(':') > 0)
        name = name.Substring(0, name.IndexOf(':'));

      Type t = GetType(name + "Extension");

      if (t == null)
        t = GetType(name);

      if (t == null)
        throw new XamlParserException(string.Format("The parser extension '{0}' could not be found", name), _filename,
                                      _reader);

      if (t.IsSubclassOf(typeof (MarkupExtension)) == false)
        throw new XamlParserException(
          string.Format("'{0}' is not of type 'System.Windows.Serialization.MarkupExtension'", t), _filename, _reader);

      MarkupExtension extension = (MarkupExtension)Activator.CreateInstance(t);

      if (extension is ICanAddNamespaceEntries)
        ((ICanAddNamespaceEntries)extension).AddNamespaceEntries(_namespaces);

      return extension.ProvideValue(_target, value);
    }

    private void ReadNamespace(string name, string value)
    {
      string ns = string.Empty;

      int nameIndex = _reader.Name.IndexOf(':');

      if (nameIndex != -1)
        ns = _reader.Name.Substring(nameIndex);
    }

    private object WalkStackForSubclassOf(Type typeWanted)
    {
      foreach (object target in _elementStack)
      {
        Type typeCurrent = target.GetType();

        if (typeCurrent.IsSubclassOf(typeWanted) == false)
          return target;
      }

      return null;
    }

    private object WalkStackForInstanceOf(Type typeWanted)
    {
      foreach (object target in _elementStack)
      {
        Type typeCurrent = target.GetType();

        if (typeCurrent.IsInstanceOfType(typeWanted) == false)
          return target;
      }

      return null;
    }

    #endregion Methods

    #region Fields

    private StringBuilder _elementText = new StringBuilder();
    private Stack _elementStack = new Stack();
    private string _filename = string.Empty;

    private static string[] _namespaces = new string[]
                                            {
                                              "MediaPortal", "System.Windows.Controls", "MediaPortal.Drawing",
                                              "MediaPortal.Drawing.Shapes", "MediaPortal.Drawing.Transforms",
                                              "System.Windows.Media.Animation", "System.Windows",
                                              "System.Windows.Serialization", "MediaPortal.Drawing.Paths",
                                              "MediaPortal.GUI.Library", "System.Windows.Input"
                                            };

    private object _target;
    private XmlTextReader _reader;

    #endregion Fields
  }
}