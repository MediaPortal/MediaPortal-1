/*
 * The attribute for declaring DataViews
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: CustomViewAttribute.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;

namespace Gentle.Framework
{
	/// <summary>
	/// <p>Use this attribute to designate properties to include in the named DataView. Use
	/// the <see cref="ObjectView"/> class to construct the named DataView given a list
	/// of objects or to update object values using data found in an existing DataView.</p>
	/// <p>This is a convenient method for using standard UI components to show and edit 
	/// business objects.</p>
	/// <p>Separate views (by grouping attributes belonging to the same view) using the view
	/// name. A single property can be in any number of views, but only once in every view.</p>
	/// </summary>
	[AttributeUsage( AttributeTargets.Property, AllowMultiple = true, Inherited = true )]
	public sealed class CustomViewAttribute : Attribute
	{
		private string viewName;
		private int columnIndex;
		private string columnName;
		private string formatString;
		private string clickAction;
		private string style;
		private string navigateUrlFormat;

		/// <summary>
		/// Mark this member as a DataView column in the specified view. 
		/// </summary>
		/// <param name="viewName">The name of this custom view</param>
		/// <param name="columnIndex">The 0-based index of this column</param>
		/// <param name="columnName">The name of the column</param>
		/// <param name="formatString">The format string used for formatting data</param>
		public CustomViewAttribute( string viewName, int columnIndex, string columnName, string formatString )
		{
			this.viewName = viewName;
			this.columnIndex = columnIndex;
			this.columnName = columnName;
			if( formatString != null && formatString.IndexOf( "{0}" ) == -1 )
			{
				this.formatString = "{0}";
			}
			else
			{
				this.formatString = formatString;
			}
		}

		/// <summary>
		/// Mark this member as a DataView column in the default view (named "default").
		/// </summary>
		/// <param name="columnIndex">The 0-based index of this column</param>
		/// <param name="columnName">The name of the column</param>
		/// <param name="formatString">The format string used for formatting data</param>
		public CustomViewAttribute( int columnIndex, string columnName, string formatString ) :
			this( "default", columnIndex, columnName, formatString )
		{
		}

		/// <summary>
		/// Mark this member as a DataView column in the specified view. 
		/// </summary>
		/// <param name="viewName">The name of this custom view</param>
		/// <param name="columnIndex">The 0-based index of this column</param>
		/// <param name="columnName">The name of the column</param>
		public CustomViewAttribute( string viewName, int columnIndex, string columnName ) :
			this( viewName, columnIndex, columnName, "{0}" )
		{
		}

		/// <summary>
		/// Mark this member as a DataView column in the default view (named "default").
		/// </summary>
		/// <param name="columnIndex">The 0-based index of this column</param>
		/// <param name="columnName">The name of the column</param>
		public CustomViewAttribute( int columnIndex, string columnName ) :
			this( "default", columnIndex, columnName, "{0}" )
		{
		}

		/// <summary>
		/// The name of the view to which this attribute belongs.
		/// </summary>
		public string ViewName
		{
			get { return viewName; }
		}

		/// <summary>
		/// The index of the column in the view. 
		/// </summary>
		public int ColumnIndex
		{
			get { return columnIndex; }
			set { columnIndex = value; }
		}

		/// <summary>
		/// The name of the column in the view.
		/// </summary>
		public string ColumnName
		{
			get { return columnName; }
			set { columnName = value; }
		}

		/// <summary>
		/// The name of the database column for storing the property decorated with this attribute.
		/// </summary>
		public string FormatString
		{
			get { return formatString; }
			set { formatString = value; }
		}

		/// <summary>
		/// Currently unused. 
		/// </summary>
		public string ClickAction
		{
			get { return clickAction; }
			set { clickAction = value; }
		}
		/// <summary>
		/// If specified the string will be used to render hyperlink columns (only when using the
		/// ObjectView.PopulateDataGrid method). The primary key will be inserted in place of {0}
		/// in the specified format string. This is only useable with Gentle supported classes.
		/// </summary>
		public string NavigateUrlFormat
		{
			get { return navigateUrlFormat; }
			set { navigateUrlFormat = value; }
		}
		/// <summary>
		/// If specified and valid, the column in the DataView will be rendered in the specified
		/// style. Possible values are "none" (the default), "hyperlink" and "button".
		/// </summary>
		public string Style
		{
			get { return style; }
			set { style = value; }
		}
	}
}