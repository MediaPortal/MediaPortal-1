/*
 * Helper class for creating DataViews from object lists
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ViewMap.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;

namespace Gentle.Framework
{
	public class ViewMap : IComparable
	{
		private ObjectMap map;
		private string propertyName;
		private Type propertyType;
		private CustomViewAttribute cv;
		private string primaryKeyName;

		public ViewMap( ObjectMap map, string propertyName, Type propertyType, CustomViewAttribute cv )
		{
			this.map = map;
			this.propertyName = propertyName;
			this.propertyType = propertyType;
			this.cv = cv;
			if( map != null && cv.NavigateUrlFormat != null )
			{
				string[] keys = map.GetPrimaryKeyNames( true );
				primaryKeyName = keys[ 0 ];
			}
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
		public string ColumnName
		{
			get { return cv.ColumnName; }
		}
		public string FormatString
		{
			get { return cv.FormatString; }
		}
		public int ColumnIndex
		{
			get { return cv.ColumnIndex; }
		}
		public string ClickAction
		{
			get { return cv.ClickAction; }
		}
		public string Style
		{
			get { return cv.Style; }
		}
		public string PrimaryKeyName
		{
			get { return primaryKeyName; }
		}
		public string NavigateUrlFormat
		{
			get { return cv.NavigateUrlFormat; }
		}
		#endregion

		#region IComparable Members
		public int CompareTo( object obj )
		{
			ViewMap vm = obj as ViewMap;
			return cv.ColumnIndex.CompareTo( vm.ColumnIndex );
		}
		#endregion
	}
}