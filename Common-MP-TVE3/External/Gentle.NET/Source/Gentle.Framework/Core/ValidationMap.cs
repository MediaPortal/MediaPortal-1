/*
 * Helper class for creating DataViews from object lists
 * Copyright (C) 2005 Clayton Harbour
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ValidationMap.cs $
 */

using System;

namespace Gentle.Framework
{
	public class ValidationMap : IComparable
	{
		private ObjectMap map;
		private string propertyName;
		private Type propertyType;
		private ValidatorBaseAttribute va;

		public ValidationMap( ObjectMap map, string propertyName, Type propertyType, ValidatorBaseAttribute va )
		{
			this.map = map;
			this.propertyName = propertyName;
			this.propertyType = propertyType;
			this.va = va;
		}

		#region Properties
		public string PropertyName
		{
			get { return propertyName; }
		}
		public Type PropertyType
		{
			get { return propertyType; }
		}
		public ValidationMessage Message
		{
			get { return va.Message; }
		}
		#endregion

		#region IComparable Members
		public int CompareTo( object obj )
		{
			ValidationMap va = obj as ValidationMap;
			return va.PropertyName.CompareTo( va.PropertyName );
		}
		#endregion

		public bool Validate( object obj )
		{
			object val = map.GetMemberValue( obj, propertyName, propertyType, true );
			return va.Validate( propertyName, val, obj );
		}
	}
}