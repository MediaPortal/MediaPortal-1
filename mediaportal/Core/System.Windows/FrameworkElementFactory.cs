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

using System.Windows.Data;

namespace System.Windows
{
  public class FrameworkElementFactory
  {
    #region Constructors

    public FrameworkElementFactory() {}

    public FrameworkElementFactory(string text)
    {
      _text = text;
    }

    public FrameworkElementFactory(Type type)
    {
      _type = type;
    }

    public FrameworkElementFactory(Type type, string name)
    {
      _type = type;
      _name = name;
    }

    #endregion Constructors

    #region Methods

    public void AddHandler(RoutedEvent routedEvent, Delegate handler) {}

    public void AddHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo) {}

    public void AppendChild(FrameworkElementFactory child) {}

    public void RemoveHandler(RoutedEvent routedEvent, Delegate handler) {}

    public void SetBinding(DependencyProperty dp, Binding binding) {}

    public void SetResourceReference(DependencyProperty dp, object name) {}

    public void SetValue(DependencyProperty dp, object value) {}

    #endregion Methods

    #region Properties

    public FrameworkElementFactory FirstChild
    {
      get { throw new NotImplementedException(); }
    }

    public bool IsSealed
    {
      get { return _isSealed; }
    }

    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    public FrameworkElementFactory NextSibling
    {
      get { throw new NotImplementedException(); }
    }

    public FrameworkElementFactory Parent
    {
      get { throw new NotImplementedException(); }
    }

    public string Text
    {
      get { return _text; }
      set { _text = value; }
    }

    public Type Type
    {
      get { return _type; }
      set { _type = value; }
    }

    #endregion Properties

    #region Fields

    private bool _isSealed = false;
    private string _name = string.Empty;
    private string _text = string.Empty;
    private Type _type = null;

    #endregion Fields
  }
}