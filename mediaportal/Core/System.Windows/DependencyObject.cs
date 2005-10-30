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
using System.Threading;

namespace System.Windows
{
	public class DependencyObject : DispatcherObject
	{
		#region Constructors

		public DependencyObject()
		{
		}

		public DependencyObject(bool canBeUnbound)
		{
			// specifies whether this object can be detached from main thread
			_isCanBeUnbound = canBeUnbound;
		}

		#endregion Constructors

		#region Methods

		public void ClearValue(DependencyProperty property)
		{
			_properties.Remove(property);
		}

		public void ClearValue(DependencyPropertyKey key)
		{
			throw new NotImplementedException();
		}

		public LocalValueEnumerator GetLocalValueEnumerator()
		{
			return new LocalValueEnumerator(_properties);
		}

		public object GetValue(DependencyProperty property)
		{
			if(property.DefaultMetadata != null && property.DefaultMetadata.GetValueOverride != null)
				return property.DefaultMetadata.GetValueOverride(this);

			object value = _properties[property];

			if(value == null && property.DefaultMetadata != null)
				value = property.DefaultMetadata.DefaultValue;

			return value;
		}

		public object GetValueBase(DependencyProperty property)
		{
			// ms-help://MS.WinFXSDK.1033/Wcp_conceptual/html/1fbada8e-4867-4ed1-8d97-62c07dad7ebc.htm
			// - Animations
			// - Local
			// - Property triggers (TemplatedParent, Template, Style, ThemeStyle)
			// - TemplatedParent's template (ie, that template includes <Setter>s) 
			// - Style property 
			// - ThemeStyle 
			// - Inheritance ("property inheritance" -- from your parent element, not your superclass) 
			// - DefaultValue specified when you registered the property (or override metadata)

			return GetValueCore(property, _properties[property], property.GetMetadata(this));
		}

		protected virtual object GetValueCore(DependencyProperty property, object baseValue, PropertyMetadata metadata)
		{
			object value = _properties[property];

			if(value == null && property.DefaultMetadata != null)
				value = property.DefaultMetadata.DefaultValue;

			return value;
		}

		public void InvalidateProperty(DependencyProperty property)
		{
			OnPropertyInvalidated(property, property.DefaultMetadata);
		}

		protected virtual void OnPropertyInvalidated(DependencyProperty property, PropertyMetadata metadata)
		{
			if(metadata != null && metadata.PropertyInvalidatedCallback != null)
				metadata.PropertyInvalidatedCallback(this);
		}

		public object ReadLocalValue(DependencyProperty property)
		{
			if(property.DefaultMetadata != null && property.DefaultMetadata.ReadLocalValueOverride != null)
				return property.DefaultMetadata.ReadLocalValueOverride(this);

			object value = _properties[property];

			// should we really be returning default value here?
			if(value == null && property.DefaultMetadata != null)
				value = property.DefaultMetadata.DefaultValue;

			return value;
		}

		public void SetValue(DependencyProperty property, object value)
		{
			SetValueBase(property, value);
		}

		public void SetValue(DependencyPropertyKey key, object value)
		{
			SetValueBase(key, value);
		}

		public void SetValueBase(DependencyProperty property, object value)
		{
			_properties[property] = value;

			if(property.DefaultMetadata != null && property.DefaultMetadata.ReadOnly)
				throw new InvalidOperationException(string.Format("DependencyProperty '{0}' has been declared read-only", property.Name));

			if(property.DefaultMetadata != null && property.DefaultMetadata.SetValueOverride != null)
				property.DefaultMetadata.SetValueOverride(this, value);

			OnPropertyInvalidated(property, property.DefaultMetadata);			
		}

		public void SetValueBase(DependencyPropertyKey key, object value)
		{
			_properties[key.DependencyProperty] = value;

			OnPropertyInvalidated(key.DependencyProperty, key.DependencyProperty.DefaultMetadata);			
		}

		#endregion Methods

		#region Properties

		public DependencyObjectType DependencyObjectType
		{
			get { return _dependencyObjectType; }
		}

		#endregion Properties

		#region Fields

		DependencyObjectType		_dependencyObjectType = null;
		bool						_isCanBeUnbound = false;
		Hashtable					_properties = new Hashtable();

		#endregion Fields
	}
}
