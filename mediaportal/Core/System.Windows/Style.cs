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

using System.Collections;
using System.Windows.Serialization;

namespace System.Windows
{
  public class Style : IAddChild, INameScope, IResourceHost
  {
    #region Constructors

    public Style() {}

    public Style(Type targetType)
    {
      if (targetType == null)
      {
        throw new ArgumentNullException("targetType");
      }

      _targetType = targetType;
    }

    public Style(Type targetType, Style basedOn)
    {
      if (targetType == null)
      {
        throw new ArgumentNullException("targetType");
      }

      if (basedOn == null)
      {
        throw new ArgumentNullException("basedOn");
      }

      _targetType = targetType;
      _basedOn = basedOn;
    }

    #endregion Constructors

    #region Methods

    void IAddChild.AddChild(object child)
    {
      if (child == null)
      {
        throw new ArgumentNullException("child");
      }

      if (child is SetterBase == false)
      {
        throw new Exception(string.Format("Cannot convert '{0}' to type '{1}'", child.GetType(), typeof (SetterBase)));
      }

      Setters.Add((SetterBase)child);
    }

    void IAddChild.AddText(string text) {}

    object INameScope.FindName(string name)
    {
      return _styles[name];
    }

    public override int GetHashCode()
    {
      return _globalIndex;
    }

    object IResourceHost.GetResource(object key)
    {
      return _styles[key];
    }

    public void RegisterName(string name, object context)
    {
      _styles[name] = context;
    }

    public void UnregisterName(string name)
    {
      _styles.Remove(name);
    }

    #endregion Methods

    #region Properties

    public Style BasedOn
    {
      get { return _basedOn; }
      set { _basedOn = value; }
    }

    public bool IsSealed
    {
      get { return _isSealed; }
    }

    IResourceHost IResourceHost.ParentResourceHost
    {
      get { return _basedOn; }
    }

    public ResourceDictionary Resources
    {
      get
      {
        if (_resources == null)
        {
          _resources = new ResourceDictionary();
        }
        return _resources;
      }
    }

    public SetterBaseCollection Setters
    {
      get
      {
        if (_setters == null)
        {
          _setters = new SetterBaseCollection();
        }
        return _setters;
      }
    }

    public Type TargetType
    {
      get { return _targetType; }
      set { _targetType = value; }
    }

    public TriggerCollection Triggers
    {
      get
      {
        if (_triggers == null)
        {
          _triggers = new TriggerCollection();
        }
        return _triggers;
      }
    }

    #endregion Properties

    #region Fields

    private Style _basedOn;
    private readonly int _globalIndex = _globalIndexNext++;
    private static int _globalIndexNext = 0;
    private bool _isSealed = false;
    private ResourceDictionary _resources;
    private SetterBaseCollection _setters;
    private static Hashtable _styles = new Hashtable(100);
    private TriggerCollection _triggers;
    private Type _targetType;

    #endregion Fields
  }
}