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

namespace System.Windows
{
	public sealed class DependencyProperty
	{
		#region Methods

		public DependencyProperty AddOwner(Type ownerType)
		{
			throw new NotImplementedException();
		}

		public DependencyProperty AddOwner(Type ownerType, PropertyMetadata typeMetadata)
		{
			throw new NotImplementedException();
		}
			
		public static DependencyProperty FromName(string name, Type ownerType)
		{
			throw new NotImplementedException();
		}

		public override int GetHashCode()
		{
			return _globalIndex;
		}

		public PropertyMetadata GetMetadata(DependencyObject d)
		{
			throw new NotImplementedException();
		}

		public PropertyMetadata GetMetadata(Type forType)
		{
			throw new NotImplementedException();
		}

		public bool IsValidType(object value)
		{
			throw new NotImplementedException();
		}

		public bool IsValidValue(object value)
		{
			if(_validateValueCallback == null)
				return true;

			return _validateValueCallback(value);
		}

		public void OverrideMetadata(Type forType, PropertyMetadata typeMetadata)
		{
			throw new NotImplementedException();
		}

		public void OverrideMetadata(Type forType, PropertyMetadata typeMetadata, DependencyPropertyKey key)
		{
			throw new NotImplementedException();
		}
		
		public static DependencyProperty Register(string name, Type propertyType, Type ownerType)
		{
			return DependencyProperty.Register(name, propertyType, ownerType, null, null);
		}

		public static DependencyProperty Register(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata)
		{
			return DependencyProperty.Register(name, propertyType, ownerType, typeMetadata, null);
		}

		public static DependencyProperty Register(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata, ValidateValueCallback validateValueCallback)
		{
			throw new NotImplementedException();
		}

		public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType)
		{
			return DependencyProperty.RegisterAttached(name, propertyType, ownerType, null, null);
		}

		public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)
		{
			return DependencyProperty.RegisterAttached(name, propertyType, ownerType, defaultMetadata, null);
		}

		public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, ValidateValueCallback validateValueCallback)
		{
			throw new NotImplementedException();
		}

		public static DependencyPropertyKey RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)
		{
			return DependencyProperty.RegisterAttachedReadOnly(name, propertyType, ownerType, defaultMetadata, null);
		}

		public static DependencyPropertyKey RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, ValidateValueCallback validateValueCallback)
		{
			throw new NotImplementedException();
		}

		public static DependencyPropertyKey RegisterReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata)
		{
			return DependencyProperty.RegisterReadOnly(name, propertyType, ownerType, typeMetadata, null);
		}

		public static DependencyPropertyKey RegisterReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata, ValidateValueCallback validateValueCallback)
		{
			throw new NotImplementedException();
		}

		#endregion Methods

		#region Properties

		public PropertyMetadata DefaultMetadata
		{
			get { return _defaultMetadata; }
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

		PropertyMetadata			_defaultMetadata = null;
		int							_globalIndex = ++_globalIndexNext;
		static int					_globalIndexNext = 0;
		string						_name = string.Empty;
		Type						_ownerType = null;
		Type						_propertyType = null;
		ValidateValueCallback		_validateValueCallback = null;

		#endregion Fields
	}
}
