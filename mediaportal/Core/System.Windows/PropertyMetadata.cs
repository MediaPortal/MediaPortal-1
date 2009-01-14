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

namespace System.Windows
{
  public class PropertyMetadata
  {
    #region Constructors

    public PropertyMetadata()
    {
    }

    public PropertyMetadata(GetValueOverride getValueOverride)
    {
      _getValueOverride = getValueOverride;
    }

    public PropertyMetadata(object defaultValue)
    {
      _defaultValue = defaultValue;
    }

    public PropertyMetadata(PropertyInvalidatedCallback propertyInvalidatedCallback)
    {
      _propertyInvalidatedCallback = propertyInvalidatedCallback;
    }

    public PropertyMetadata(SetValueOverride setValueOverride)
    {
      _setValueOverride = setValueOverride;
    }

    public PropertyMetadata(GetValueOverride getValueOverride, SetValueOverride setValueOverride)
    {
      _getValueOverride = getValueOverride;
      _setValueOverride = setValueOverride;
    }

    public PropertyMetadata(object defaultValue, PropertyInvalidatedCallback propertyInvalidatedCallback)
    {
      _defaultValue = defaultValue;
      _propertyInvalidatedCallback = propertyInvalidatedCallback;
    }

    public PropertyMetadata(object defaultValue, PropertyInvalidatedCallback propertyInvalidatedCallback,
                            GetValueOverride getValueOverride)
    {
      _defaultValue = defaultValue;
      _propertyInvalidatedCallback = propertyInvalidatedCallback;
    }

    public PropertyMetadata(PropertyInvalidatedCallback propertyInvalidatedCallback, GetValueOverride getValueOverride)
    {
      _propertyInvalidatedCallback = propertyInvalidatedCallback;
      _getValueOverride = getValueOverride;
    }

    public PropertyMetadata(PropertyInvalidatedCallback propertyInvalidatedCallback, GetValueOverride getValueOverride,
                            SetValueOverride setValueOverride)
    {
      _propertyInvalidatedCallback = propertyInvalidatedCallback;
      _getValueOverride = getValueOverride;
      _setValueOverride = setValueOverride;
    }

    public PropertyMetadata(object defaultValue, PropertyInvalidatedCallback propertyInvalidatedCallback,
                            GetValueOverride getValueOverride, SetValueOverride setValueOverride)
    {
      _defaultValue = defaultValue;
      _propertyInvalidatedCallback = propertyInvalidatedCallback;
    }

    #endregion Constructors

    #region Methods

    protected void ClearCachedDefaultValue(DependencyObject owner)
    {
      _defaultValue = null;
    }

    protected virtual object CreateDefaultValue(DependencyObject owner, DependencyProperty property)
    {
      throw new NotImplementedException();
    }

    protected virtual PropertyMetadata CreateInstance()
    {
      return new PropertyMetadata();
    }

    protected virtual void Merge(PropertyMetadata baseMetadata, DependencyProperty property)
    {
    }

    protected virtual void OnApply(DependencyProperty dp, Type targetType)
    {
      _isSealed = true;
    }

    protected internal void SetReadOnly()
    {
      _isReadOnly = true;
    }

    #endregion Methods

    #region Properties

    public object DefaultValue
    {
      get { return _defaultValue; }
      set { _defaultValue = value; }
    }

    public GetValueOverride GetValueOverride
    {
      get { return _getValueOverride; }
      set { _getValueOverride = value; }
    }

    protected bool IsSealed
    {
      get { return _isSealed; }
    }

    public PropertyInvalidatedCallback PropertyInvalidatedCallback
    {
      get { return _propertyInvalidatedCallback; }
      set { _propertyInvalidatedCallback = value; }
    }

    public ReadLocalValueOverride ReadLocalValueOverride
    {
      get { return _readLocalValueOverride; }
      set { _readLocalValueOverride = value; }
    }

    public bool ReadOnly
    {
      get { return _isReadOnly; }
    }

    public SetValueOverride SetValueOverride
    {
      get { return _setValueOverride; }
      set { _setValueOverride = value; }
    }

    public WriteLocalValueOverride WriteLocalValueOverride
    {
      get { return _writeLocalValueOverride; }
      set { _writeLocalValueOverride = value; }
    }

    #endregion Properties

    #region Fields

    private object _defaultValue = DependencyProperty.UnsetValue;
    private GetValueOverride _getValueOverride;
    private bool _isSealed;
    private PropertyInvalidatedCallback _propertyInvalidatedCallback;
    private ReadLocalValueOverride _readLocalValueOverride;
    private bool _isReadOnly = false;
    private SetValueOverride _setValueOverride;
    private WriteLocalValueOverride _writeLocalValueOverride;

    #endregion Fields
  }
}