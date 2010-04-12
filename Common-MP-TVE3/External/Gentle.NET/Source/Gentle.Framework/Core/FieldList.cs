/*
 * Typed FieldMap collection
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: FieldList.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Helper class for storing multiple <see cref="FieldMap"/> entries in
	/// a single array. It contains methods to locate and translate between
	/// column and property names.
	/// </summary>
	public sealed class FieldList : IList
	{
		private ArrayList fields;
		private HybridDictionary properties;
		private HybridDictionary columns;

		internal FieldList()
		{
			fields = new ArrayList();
		}

		#region Find Property or Column (by name)
		/// <summary>
		/// Obtain the column name corresponding to a given property name.
		/// </summary>
		/// <param name="name">The name of the property</param>
		/// <returns>The corresponding FieldMap</returns>
		public FieldMap FindProperty( string name )
		{
			Check.VerifyNotNull( name, Error.NullParameter, "name" );
			if( properties == null )
			{
				properties = new HybridDictionary( fields.Count, true );
				foreach( FieldMap fm in fields )
				{
					properties.Add( fm.MemberName, fm );
				}
			}
			return properties[ name ] as FieldMap;
		}

		/// <summary>
		/// Obtain the property name corresponding to a given column name.
		/// </summary>
		/// <param name="name">The name of the column</param>
		/// <returns>The corresponding FieldMap</returns>
		public FieldMap FindColumn( string name )
		{
			Check.VerifyNotNull( name, Error.NullParameter, "name" );
			if( columns == null )
			{
				columns = new HybridDictionary( fields.Count, true );
				foreach( FieldMap fm in fields )
				{
					columns.Add( fm.ColumnName, fm );
				}
			}
			return columns[ name ] as FieldMap;
		}
		#endregion

		#region Find Column (by columnId)
		// note: this method is used only by the Sybase providers
		public FieldMap FindColumnById( int columnId )
		{
			foreach( FieldMap fm in this )
			{
				// TODO fix this nasty bit where we assume that -1 is never used
				if( columnId != -1 && columnId == fm.ColumnId )
				{
					return fm;
				}
			}
			return null;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Returns the number of primary key fields in the array.
		/// </summary>
		public int PrimaryKeyCount
		{
			get
			{
				int count = 0;
				foreach( FieldMap fm in this )
				{
					if( fm.IsPrimaryKey )
					{
						count++;
					}
				}
				return count;
			}
		}

		public FieldMap this[ int index ]
		{
			get { return fields[ index ] as FieldMap; }
		}
		#endregion

		#region IList Members
		public bool IsReadOnly
		{
			get { return false; }
		}

		object IList.this[ int index ]
		{
			get { return fields[ index ]; }
			set { fields[ index ] = value; }
		}

		public void RemoveAt( int index )
		{
			FieldMap fm = fields[ index ] as FieldMap;
			Remove( fm );
		}

		public void Insert( int index, object value )
		{
			FieldMap fm = value as FieldMap;
			fields.Insert( index, fm );
			// reset internal search dictionaries
			properties = null;
			columns = null;
		}

		public void Remove( object value )
		{
			FieldMap fm = value as FieldMap;
			fields.Remove( fm );
			if( properties != null )
			{
				properties.Remove( fm.MemberName );
			}
			if( columns != null )
			{
				columns.Remove( fm.ColumnName );
			}
		}

		public bool Contains( object value )
		{
			return fields.Contains( value );
		}

		public void Clear()
		{
			fields.Clear();
			properties = null;
			columns = null;
		}

		public int IndexOf( object value )
		{
			return fields.IndexOf( value );
		}

		public int Add( object value )
		{
			Insert( fields.Count, value );
			return fields.Count;
		}

		public bool IsFixedSize
		{
			get { return false; }
		}
		#endregion

		#region ICollection Members
		public bool IsSynchronized
		{
			get { return false; }
		}

		public int Count
		{
			get { return fields.Count; }
		}

		public void CopyTo( Array array, int index )
		{
			fields.CopyTo( array, index );
		}

		public object SyncRoot
		{
			get { return fields.SyncRoot; }
		}
		#endregion

		#region IEnumerable Members
		public IEnumerator GetEnumerator()
		{
			return fields.GetEnumerator();
		}
		#endregion
	}
}