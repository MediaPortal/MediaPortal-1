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

		public void ClearValue(DependencyProperty dp)
		{
			_properties.Remove(dp);
		}

		public void ClearValue(DependencyPropertyKey key)
		{
			throw new NotImplementedException();
		}

		public LocalValueEnumerator GetLocalValueEnumerator()
		{
			return new LocalValueEnumerator(_properties);
		}

		public object GetValue(DependencyProperty dp)
		{
			if(dp.DefaultMetadata != null && dp.DefaultMetadata.GetValueOverride != null)
				return dp.DefaultMetadata.GetValueOverride(this);

			object value = _properties[dp];

			if(value == null && dp.DefaultMetadata != null)
				value = dp.DefaultMetadata.DefaultValue;

			return value;
		}

		public object GetValueBase(DependencyProperty dp)
		{
			return GetValueCore(dp, _properties[dp], dp.GetMetadata(this));
		}

		protected virtual object GetValueCore(DependencyProperty dp, object baseValue, PropertyMetadata metadata)
		{
			object value = _properties[dp];

			if(value == null && dp.DefaultMetadata != null)
				value = dp.DefaultMetadata.DefaultValue;

			return value;
		}

		public void InvalidateProperty(DependencyProperty dp)
		{
			OnPropertyInvalidated(dp, dp.DefaultMetadata);
		}

		protected virtual void OnPropertyInvalidated(DependencyProperty dp, PropertyMetadata metadata)
		{
			if(metadata != null && metadata.PropertyInvalidatedCallback != null)
				metadata.PropertyInvalidatedCallback(this);
		}

		public object ReadLocalValue(DependencyProperty dp)
		{
			if(dp.DefaultMetadata != null && dp.DefaultMetadata.ReadLocalValueOverride != null)
				return dp.DefaultMetadata.ReadLocalValueOverride(this);

			object value = _properties[dp];

			// should we really be returning default value here?
			if(value == null && dp.DefaultMetadata != null)
				value = dp.DefaultMetadata.DefaultValue;

			return value;
		}

		public void SetValue(DependencyProperty dp, object value)
		{
			SetValueBase(dp, value);
		}

		public void SetValue(DependencyPropertyKey key, object value)
		{
			SetValueBase(key, value);
		}

		public void SetValueBase(DependencyProperty dp, object value)
		{
			_properties[dp] = value;

			if(dp.DefaultMetadata != null && dp.DefaultMetadata.ReadOnly)
				throw new InvalidOperationException(string.Format("DependencyProperty '{0}' has been declared read-only", dp.Name));

			if(dp.DefaultMetadata != null && dp.DefaultMetadata.SetValueOverride != null)
				dp.DefaultMetadata.SetValueOverride(this, value);

			OnPropertyInvalidated(dp, dp.DefaultMetadata);			
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
