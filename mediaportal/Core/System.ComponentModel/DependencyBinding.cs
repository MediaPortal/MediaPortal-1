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
using System.ComponentModel;
using System.Reflection;

namespace System.ComponentModel
{
	public sealed class DependencyBinding
	{
		#region Constructors

		private DependencyBinding()
		{
		}

		private DependencyBinding(Type ownerType, PropertyInfo propertyInfo)
		{
//			if(_propertyInfo == null)
//				throw new ArgumentNullException("propertyInfo");

			_ownerType = ownerType;
			_propertyInfo = propertyInfo;
			_hash = _hashNext++;
		}

		#endregion Constructors		

		#region Methods

		public override int GetHashCode()
		{
			return _hash;
		}

		public object GetValue(object target)
		{
			return _propertyInfo.GetValue(target, null);
		}

		public static DependencyBinding Register(Type ownerType, string name)
		{
			if(ownerType == null)
				throw new ArgumentNullException("ownerType");

			if(name == null)
				throw new ArgumentNullException("name");

			return new DependencyBinding(ownerType, ownerType.GetProperty(name));
		}

		public void SetValue(object target, object value)
		{
			if(_propertyInfo.PropertyType == typeof(int))
				_propertyInfo.SetValue(target, (int)(double)value, null);

			if(_propertyInfo.PropertyType == typeof(double))
				_propertyInfo.SetValue(target, (double)value, null);
		}

		#endregion Methods

		#region Properties

		public string Name
		{
			get { return _propertyInfo.Name; }
		}

		internal PropertyInfo Property
		{
			get { return _propertyInfo; }
		}

		public Type PropertyType
		{
			get { return _propertyInfo.PropertyType; }
		}

		public Type OwnerType
		{
			get { return _ownerType; }
		}

		#endregion Properties

		#region Properties (Dependency)

//		public static readonly DependencyBinding X = DependencyBinding.Register(typeof(mpslide.Picturebox), "X");
//		public static readonly DependencyBinding Y = DependencyBinding.Register(typeof(mpslide.Picturebox), "Y");
//		public static readonly DependencyBinding Opacity = DependencyBinding.Register(typeof(System.Windows.Forms.Form), "Opacity");
//		public static readonly DependencyBinding Scale = DependencyBinding.Register(typeof(mpslide.Picturebox), "Scale");

		#endregion Properties (Dependency)

		#region Fields

		Type						_ownerType;
		PropertyInfo				_propertyInfo;
		int							_hash;
		static int					_hashNext = 1000;

		#endregion Fields
	}
}
