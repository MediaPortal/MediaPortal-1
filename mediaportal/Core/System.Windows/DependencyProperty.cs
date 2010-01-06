#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System.Collections;
using System.ComponentModel;
using MediaPortal.GUI.Library;

namespace System.Windows
{
  [TypeConverter(typeof (DependencyPropertyConverter))]
  public sealed class DependencyProperty
  {
    #region Constructors

    private DependencyProperty() {}

    private DependencyProperty(DependencyProperty property, PropertyMetadata defaultMetadata)
    {
      _defaultMetadata = defaultMetadata;
      _name = property._name;
      _ownerType = property._ownerType;
      _propertyType = property._propertyType;
      _validateValueCallback = property._validateValueCallback;

      _properties[_name + _ownerType] = this;
    }

    private DependencyProperty(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata,
                               ValidateValueCallback validateValueCallback)
    {
      _name = name;
      _propertyType = propertyType;
      _ownerType = ownerType;
      _defaultMetadata = defaultMetadata;
      _validateValueCallback = validateValueCallback;

      _properties[name + ownerType] = this;
    }

    #endregion Constructors

    #region Methods

    public DependencyProperty AddOwner(Type ownerType)
    {
      return AddOwner(ownerType, _defaultMetadata);
    }

    public DependencyProperty AddOwner(Type ownerType, PropertyMetadata defaultMetadata)
    {
      OverrideMetadata(ownerType, defaultMetadata);

      return this;
    }

    public static DependencyProperty FromName(string name, Type ownerType)
    {
      // MPSPECIFIC
      if (ownerType == typeof (GUIControl))
      {
        return (DependencyProperty)_properties[name + typeof (FrameworkElement)];
      }

      return (DependencyProperty)_properties[name + ownerType];
    }

    public override int GetHashCode()
    {
      return _globalIndex;
    }

    public PropertyMetadata GetMetadata(DependencyObject d)
    {
      return GetMetadata(d.DependencyObjectType.SystemType);
    }

    public PropertyMetadata GetMetadata(DependencyObjectType type)
    {
      return GetMetadata(type.SystemType);
    }

    public PropertyMetadata GetMetadata(Type ownerType)
    {
      PropertyMetadata metadata = _metadata[ownerType] as PropertyMetadata;

      if (metadata == null)
      {
        return _defaultMetadata;
      }

      return metadata;
    }

    public bool IsValidType(object value)
    {
      return _propertyType.IsInstanceOfType(value);
    }

    public bool IsValidValue(object value)
    {
      if (value == UnsetValue)
      {
        return false;
      }

      if (_validateValueCallback != null)
      {
        return _validateValueCallback(value);
      }

      return true;
    }

    public void OverrideMetadata(Type ownerType, PropertyMetadata defaultMetadata)
    {
      _metadata[ownerType] = defaultMetadata;
    }

    public void OverrideMetadata(Type ownerType, PropertyMetadata defaultMetadata, DependencyPropertyKey key)
    {
      if (key.DependencyProperty != this)
      {
        throw new InvalidOperationException();
      }

      OverrideMetadata(ownerType, defaultMetadata);
    }

    public static DependencyProperty Register(string name, Type propertyType, Type ownerType)
    {
      return Register(name, propertyType, ownerType, null);
    }

    public static DependencyProperty Register(string name, Type propertyType, Type ownerType,
                                              PropertyMetadata defaultMetadata)
    {
      return Register(name, propertyType, ownerType, defaultMetadata, null);
    }

    public static DependencyProperty Register(string name, Type propertyType, Type ownerType,
                                              PropertyMetadata defaultMetadata,
                                              ValidateValueCallback validateValueCallback)
    {
      return new DependencyProperty(name, propertyType, ownerType, defaultMetadata, validateValueCallback);
    }

    public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType)
    {
      return RegisterAttached(name, propertyType, ownerType, null);
    }

    public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType,
                                                      PropertyMetadata defaultMetadata)
    {
      return RegisterAttached(name, propertyType, ownerType, defaultMetadata, null);
    }

    public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType,
                                                      PropertyMetadata defaultMetadata,
                                                      ValidateValueCallback validateValueCallback)
    {
      // TODO: What should differ for attached properties???
      return Register(name, propertyType, ownerType, defaultMetadata, validateValueCallback);
    }

    public static DependencyPropertyKey RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType,
                                                                 PropertyMetadata defaultMetadata)
    {
      return RegisterAttachedReadOnly(name, propertyType, ownerType, defaultMetadata, null);
    }

    public static DependencyPropertyKey RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType,
                                                                 PropertyMetadata defaultMetadata,
                                                                 ValidateValueCallback validateValueCallback)
    {
      throw new NotImplementedException();
    }

    public static DependencyPropertyKey RegisterReadOnly(string name, Type propertyType, Type ownerType,
                                                         PropertyMetadata defaultMetadata)
    {
      return RegisterReadOnly(name, propertyType, ownerType, defaultMetadata, null);
    }

    public static DependencyPropertyKey RegisterReadOnly(string name, Type propertyType, Type ownerType,
                                                         PropertyMetadata defaultMetadata,
                                                         ValidateValueCallback validateValueCallback)
    {
      return new DependencyPropertyKey(name, propertyType, ownerType, defaultMetadata, validateValueCallback);
    }

    #endregion Methods

    #region Properties

    public PropertyMetadata DefaultMetadata
    {
      get
      {
        if (_defaultMetadata == null)
        {
          _defaultMetadata = new PropertyMetadata();
        }
        return _defaultMetadata;
      }
    }

    public int GlobalIndex
    {
      get { return _globalIndex; }
    }

    public string Name
    {
      get { return _name; }
    }

    public Type OwnerType
    {
      get { return _ownerType; }
    }

    public Type PropertyType
    {
      get { return _propertyType; }
    }

    public ValidateValueCallback ValidateValueCallback
    {
      get { return _validateValueCallback; }
    }

    #endregion Properties

    #region Fields

    private PropertyMetadata _defaultMetadata = null;
    private readonly int _globalIndex = _globalIndexNext++;
    private static int _globalIndexNext = 0;
    private string _name = string.Empty;
    private Type _ownerType = null;
    private static Hashtable _properties = new Hashtable(100);
    private static Hashtable _propertiesReadOnly = new Hashtable(100);
    private static Hashtable _propertiesAttached = new Hashtable(100);
    private Hashtable _metadata = new Hashtable(100);
    private Type _propertyType = null;
    private ValidateValueCallback _validateValueCallback = null;

    public static readonly object UnsetValue = new object();

    #endregion Fields
  }
}