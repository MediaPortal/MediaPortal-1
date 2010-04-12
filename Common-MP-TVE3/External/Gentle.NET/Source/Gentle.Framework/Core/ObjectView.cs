/*
 * Helper class for caching obtained meta-data on persistable objects
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ObjectView.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Data;
using System.Web.UI.WebControls;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Helper class for converting between arrays of business objects and DataView
	/// instances useable in UI components. To customize the layout and formatting of
	/// the generated DataViews, decorate properties with the <see cref="CustomViewAttribute"/> 
	/// attribute.
	/// </summary>
	public sealed class ObjectView
	{
		private ObjectMap map;
		private string viewName;
		private ArrayList viewMaps;

		internal ObjectView( ObjectMap map, string viewName )
		{
			this.map = map;
			this.viewName = viewName;
			viewMaps = new ArrayList();
		}

		/// <summary>
		/// Adds a column to this view. 
		/// </summary>
		/// <param name="viewMap">The view map instance to use for this column</param>
		public void AddColumn( ViewMap viewMap )
		{
			viewMaps.Add( viewMap );
		}

		/// <summary>
		/// Calling this method will sort the viewMaps ArrayList by the columnIndexes.
		/// </summary>
		internal void OrderColumns()
		{
			viewMaps.Sort();
		}

		/// <summary>
		/// Construct a DataTable with columns for the type represented.
		/// </summary>
		/// <returns>A new DataTable instance</returns>
		private DataTable GetDataTable()
		{
			// make sure our columns are sorted before we build the table
			OrderColumns();
			DataTable dt = new DataTable();
			// add all columns (assume order of elements is correct)
			foreach( ViewMap vm in viewMaps )
			{
				dt.Columns.Add( new DataColumn( vm.ColumnName, vm.PropertyType ) );
			}
			return dt;
		}

		/// <summary>
		/// Construct an item array of values gathered from the supplied instance. The item array
		/// is used to batch set all the values in a DataView row.
		/// </summary>
		/// <param name="instance">The instance whose property values are used</param>
		/// <returns>The item array</returns>
		private object[] GetItemArray( object instance )
		{
			object[] result = new object[viewMaps.Count];
			foreach( ViewMap vm in viewMaps )
			{
				// insert empty strings if null values are encountered
				object columnValue = map.GetMemberValue( instance, vm.PropertyName, typeof(string), false );
				if( vm.FormatString != null )
				{
					result[ vm.ColumnIndex ] = String.Format( vm.FormatString, columnValue );
				}
				else
				{
					result[ vm.ColumnIndex ] = columnValue;
				}
			}
			return result;
		}

		private DataView PopulateDataView( IList data )
		{
			DataTable dt = GetDataTable();
			DataRow dr;
			foreach( object instance in data )
			{
				dr = dt.NewRow();
				dr.ItemArray = GetItemArray( instance );
				dt.Rows.Add( dr );
			}
			return new DataView( dt );
		}

		private static ObjectView GetObjectView( PersistenceBroker broker, Type type, string viewName )
		{
			ObjectMap map = ObjectFactory.GetMap( broker, type );
			Check.VerifyNotNull( viewName, Error.NullParameter, "viewName" );
			ObjectView view = map.Views[ viewName ] as ObjectView;
			Check.VerifyNotNull( view, Error.NoSuchView, viewName, type );
			return view;
		}

		private static ObjectView GetObjectView( Type type, string viewName )
		{
			return GetObjectView( null, type, viewName );
		}

		/// <summary>
		/// Obtain a DataTable for the specified type and view.
		/// </summary>
		/// <returns>The new DataTable instance</returns>
		public static DataTable GetDataTable( Type type, string viewName )
		{
			try
			{
				ObjectView view = GetObjectView( type, viewName );
				return view.GetDataTable();
			}
			catch( Exception e )
			{
				Check.Fail( e, Error.ViewError, viewName, type );
				throw;
			}
		}

		/// <summary>
		/// Obtain a DataTable for the specified type using the "default" view.
		/// </summary>
		/// <returns>The new DataTable instance</returns>
		public static DataTable GetDataTable( Type type )
		{
			try
			{
				ObjectView view = GetObjectView( type, "default" );
				return view.GetDataTable();
			}
			catch( Exception e )
			{
				Check.Fail( e, Error.ViewError, "default", type );
				throw;
			}
		}

		/// <summary>
		/// Create and populate a DataView with the given name. Use this method when you need
		/// to specify the object type manually (which is the case when the view is specified
		/// on a base class and the list may contain various subclasses).
		/// </summary>
		/// <param name="type">The type associated with the given view name</param>
		/// <param name="viewName">The name of the view</param>
		/// <param name="data">The list of objects to include in the view</param>
		/// <returns>The new DataView instance</returns>
		public static DataView GetDataView( Type type, string viewName, IList data )
		{
			if( type == null )
			{
				type = data[ 0 ].GetType();
			}
			// construct the named view for the type found in the supplied data
			ObjectView view = GetObjectView( type, viewName );
			return view.PopulateDataView( data );
		}

		/// <summary>
		/// Create and populate a DataView using the specified view. The object type is automatically
		/// determined from the contents of the data parameter (which must not be null or empty).
		/// </summary>
		/// <param name="viewName">The name of the view</param>
		/// <param name="data">The list of objects to include in the view</param>
		/// <returns>The new DataView instance</returns>
		public static DataView GetDataView( string viewName, IList data )
		{
			Check.Verify( data != null && data.Count > 0, Error.NullParameter, "data" );
			Type type = data[ 0 ].GetType();
			// construct the named view for the type found in the supplied data
			ObjectView view = GetObjectView( type, viewName );
			return view.PopulateDataView( data );
		}

		/// <summary>
		/// Create and populate a DataView using the "default" view. The object type is automatically
		/// determined from the contents of the data parameter (which must not be null or empty).
		/// </summary>
		/// <param name="data">The list of objects to include in the view</param>
		/// <returns>The new DataView instance</returns>
		public static DataView GetDataView( IList data )
		{
			return GetDataView( "default", data );
		}

		private static Type GetColumnType( string columnName, ObjectMap map, SqlResult sr )
		{
			Type result = null;
			if( map != null )
			{
				FieldMap fm = map.GetFieldMapFromColumn( columnName );
				if( fm != null )
				{
					result = fm.Type;
				}
			}
			if( result == null && sr.RowsContained > 0 )
			{
				object obj = sr.GetObject( 0, columnName );
				if( obj != null )
				{
					result = obj.GetType();
				}
			}
			return result;
		}

		/// <summary>
		/// Create and populate a DataView from the given SqlResult. Columns in the result set
		/// are copied verbatim to the DataView.
		/// </summary>
		/// <param name="sr">The SqlResult used to generate and populate the DataView.</param>
		/// <returns>The new DataView instance</returns>
		public static DataView GetDataView( SqlResult sr )
		{
			DataTable dt = new DataTable();
			ObjectMap map = null;
			if( sr.Statement != null && sr.Statement.Type != null )
			{
				map = ObjectFactory.GetMap( sr.SessionBroker, sr.Statement.Type );
			}
			foreach( string columnName in sr.ColumnNames )
			{
				DataColumn column = new DataColumn( columnName );
				Type type = GetColumnType( columnName, map, sr );
				if( type != null )
				{
					column.DataType = type;
				}
				dt.Columns.Add( column );
			}
			DataRow dr;
			foreach( object[] row in sr.Rows )
			{
				dr = dt.NewRow();
				dr.ItemArray = row;
				dt.Rows.Add( dr );
			}
			return new DataView( dt );
		}

		private ArrayList GetGridColumns( string linkFieldName )
		{
			ArrayList columns = new ArrayList();
			OrderColumns();
			foreach( ViewMap vm in viewMaps )
			{
				if( vm.Style != null && vm.Style == "hyperlink" )
				{
					if( linkFieldName == null )
					{
						Check.VerifyNotNull( vm.PrimaryKeyName, Error.NullParameter, "PrimaryKeyName" );
						linkFieldName = vm.PrimaryKeyName;
					}
					HyperLinkColumn column = new HyperLinkColumn();
					column.DataTextField = vm.PropertyName;
					column.DataNavigateUrlField = linkFieldName;
					column.DataNavigateUrlFormatString = vm.NavigateUrlFormat;
					column.HeaderText = vm.ColumnName;
					column.SortExpression = vm.PropertyName;
					column.DataTextFormatString = vm.FormatString;
					columns.Add( column );
				}
				else
				{
					BoundColumn column = new BoundColumn();
					column.HeaderText = vm.ColumnName;
					column.SortExpression = vm.PropertyName;
					column.DataField = vm.PropertyName;
					column.DataFormatString = vm.FormatString;
					columns.Add( column );
				}
			}
			return columns;
		}

		public static void PopulateDataGrid( DataGrid dg, string viewName, IList data )
		{
			PopulateDataGrid( null, dg, viewName, data, null );
		}

		public static void PopulateDataGrid( DataGrid dg, string viewName, IList data, string linkFieldName )
		{
			PopulateDataGrid( data[ 0 ].GetType(), dg, viewName, data, linkFieldName );
		}

		public static void PopulateDataGrid( Type type, DataGrid dg, string viewName, IList data, string linkFieldName )
		{
			if( type == null )
			{
				type = data[ 0 ].GetType();
			}
			ObjectView ov = GetObjectView( type, viewName );
			dg.AutoGenerateColumns = false;
			dg.Columns.Clear();
			foreach( DataGridColumn column in ov.GetGridColumns( linkFieldName ) )
			{
				dg.Columns.Add( column );
			}
			dg.DataSource = data;
			dg.DataBind();
		}

		#region Properties
		public string ViewName
		{
			get { return viewName; }
		}
		public ArrayList ViewMaps
		{
			get { return viewMaps; }
		}
		#endregion
	}
}