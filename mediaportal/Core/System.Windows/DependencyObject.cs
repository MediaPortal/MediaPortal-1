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
using System.Windows.Threading;

namespace System.Windows
{
  public class DependencyObject : DispatcherObject
  {
    #region Constructors

    public DependencyObject() {}

    public DependencyObject(bool canBeUnbound)
    {
      // specifies whether this object can be detached from main thread
      _isCanBeUnbound = canBeUnbound;
    }

    #endregion Constructors

    #region Methods

    public void ClearValue(DependencyProperty property)
    {
      _localValues.Remove(property);
    }

    public void ClearValue(DependencyPropertyKey key)
    {
      _localValues.Remove(key.DependencyProperty);
    }

    public LocalValueEnumerator GetLocalValueEnumerator()
    {
      return new LocalValueEnumerator(_localValues);
    }

    // http://www.64bit-world.com/forums/microsoft-public-developer-winfx-avalon/10124-xamlpad-exe-nullreferencexception-winfx-ctp-sept.html
    public object GetValue(DependencyProperty property)
    {
      PropertyMetadata metadata = property.GetMetadata(this.GetType());

      if (metadata.GetValueOverride != null)
      {
        return metadata.GetValueOverride(this);
      }

      return GetValueCommon(property, metadata);
    }

    public object GetValueBase(DependencyProperty property)
    {
      return GetValueCommon(property, property.GetMetadata(this.GetType()));
    }

    private object GetValueCommon(DependencyProperty property, PropertyMetadata metadata)
    {
      object commonValue = DependencyProperty.UnsetValue;

      if (_localValues.ContainsKey(property.GlobalIndex))
      {
        commonValue = _localValues[property.GlobalIndex];
      }

      if (commonValue != DependencyProperty.UnsetValue)
      {
        return commonValue;
      }

      if (metadata.ReadLocalValueOverride != null)
      {
        commonValue = metadata.ReadLocalValueOverride(this);
      }

      if (commonValue != DependencyProperty.UnsetValue)
      {
        return commonValue;
      }

      commonValue = GetValueCore(property, commonValue, metadata);

      if (commonValue != DependencyProperty.UnsetValue)
      {
        return commonValue;
      }

      return metadata.DefaultValue;
    }

    // ms-help://MS.WinFXSDK.1033/Wcp_conceptual/html/1fbada8e-4867-4ed1-8d97-62c07dad7ebc.htm
    // - Animations
    // - Local
    // - Property triggers (TemplatedParent, Template, Style, ThemeStyle)
    // - TemplatedParent's template (ie, that template includes <Setter>s) 
    // - Style property 
    // - ThemeStyle 
    // - Inheritance ("property inheritance" -- from your parent element, not your superclass) 
    // - DefaultValue specified when you registered the property (or override metadata)

    protected virtual object GetValueCore(DependencyProperty property, object baseValue, PropertyMetadata metadata)
    {
      return baseValue;
    }

    public void InvalidateProperty(DependencyProperty property)
    {
      InvalidateProperty(property, property.GetMetadata(GetType()));
    }

    private void InvalidateProperty(DependencyProperty property, PropertyMetadata metadata)
    {
      if (metadata != null)
      {
        if (metadata.PropertyInvalidatedCallback != null)
        {
          metadata.PropertyInvalidatedCallback(this);
        }
      }

      OnPropertyInvalidated(property, metadata);
    }

    protected virtual void OnPropertyInvalidated(DependencyProperty property, PropertyMetadata metadata)
    {
      // no default implementation
    }

    public object ReadLocalValue(DependencyProperty property)
    {
      // this needs to be rewritten
      throw new NotImplementedException();

/*			if(property.DefaultMetadata != null && property.DefaultMetadata.ReadLocalValueOverride != null)
				return property.DefaultMetadata.ReadLocalValueOverride(this);

			object value = _localValues[property.GlobalIndex];

			// should we really be returning default value here?
			if(value == null && property.DefaultMetadata != null)
				value = property.DefaultMetadata.DefaultValue;

			return value;
*/
    }

    public void SetValue(DependencyProperty property, object value)
    {
      PropertyMetadata metadata = property.GetMetadata(GetType());

      if (metadata != null)
      {
        if (metadata.ReadOnly)
        {
          throw new InvalidOperationException(string.Format("DependencyProperty '{0}' has been declared read-only",
                                                            property.Name));
        }

        if (metadata.SetValueOverride != null)
        {
          metadata.SetValueOverride(this, value);
          return;
        }
      }

      SetValueCommon(property, metadata, value);
    }

    public void SetValue(DependencyPropertyKey key, object value)
    {
      throw new NotImplementedException();
    }

    public void SetValueBase(DependencyProperty property, object value)
    {
      PropertyMetadata metadata = property.GetMetadata(GetType());

      SetValueCommon(property, metadata, value);
    }

    public void SetValueBase(DependencyPropertyKey key, object value)
    {
      throw new NotImplementedException();
    }

    protected void SetValueCommon(DependencyProperty property, PropertyMetadata metadata, object value)
    {
      _localValues[property.GlobalIndex] = value;

      InvalidateProperty(property, metadata);
    }

    #endregion Methods

    #region Properties

    public DependencyObjectType DependencyObjectType
    {
      get { return _dependencyObjectType; }
    }

    #endregion Properties

    #region Fields

    private DependencyObjectType _dependencyObjectType = null;
    private bool _isCanBeUnbound = false;
    private Hashtable _localValues = new Hashtable();

    #endregion Fields
  }
}