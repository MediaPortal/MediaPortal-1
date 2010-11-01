/*
 * The core attributes for decorating business objects
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TableColumnAttribute.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;

namespace Gentle.Framework
{
	/// <summary>
	/// Use this attribute to identify the properties of <see cref="Persistent"/> objects that map 
	/// to table columns. If no map name is supplied the column and property name must be identical
	/// (case differences are ignored). This attribute can also be used to override constraints,
	/// such as to allow NULL values (defaults to false).
	/// Note that metadata obtained directly from the database overrides anything (but the name)
	/// specified as parameters to this attribute.
	/// </summary>
	// note: this class used to be sealed but this prevents VB users from using an inherited copy to
	// specify NullValues, which apparently is needed due to limitations in how attributes are handled.
	[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
	public class TableColumnAttribute : Attribute
	{
		private string name;
		private bool notNull;
		private int size;
		private bool hasDbType; // true when DbType property has been set
		private long dbType;
		private object nullValue;
		private bool handleEnumAsString;
		private bool isReadOnly;
		private bool isUpdateAfterWrite;

		/// <summary>
		/// Constructor for table columns that are named after their property counterpart
		/// and whose value cannot be null.
		/// </summary>
		public TableColumnAttribute() : this( null, true )
		{
		}

		/// <summary>
		/// Constructor for table columns that are named after their property counterpart.
		/// </summary>
		/// <param name="notNull">A boolean telling whether null values are allowed in the database</param>
		public TableColumnAttribute( bool notNull ) : this( null, notNull )
		{
		}

		/// <summary>
		/// Constructor for table columns whose value cannot be null.
		/// </summary>
		/// <param name="name">The name of the database column</param>
		public TableColumnAttribute( string name ) : this( name, true )
		{
		}

		/// <summary>
		/// Constructor for table columns.
		/// </summary>
		/// <param name="name">The name of the database column</param>
		/// <param name="notNull">A boolean telling whether null values are allowed in the database</param>
		public TableColumnAttribute( string name, bool notNull )
		{
			this.name = name;
			this.notNull = notNull;
		}

		/// <summary>
		/// The name of the database column for storing the property decorated with this attribute.
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// This property (defaults to true) can be used to specify whether NULL values are
		/// allowed in the database. This allows the framework to fail early if a constraint
		/// is violated.
		/// </summary>
		public bool NotNull
		{
			get { return notNull; }
			set { notNull = value; }
		}

		/// <summary>
		/// The database type of the field in the database. Beware that the DbType enumeration
		/// values are NOT the ones used by the individual providers. Gentle does NOT convert
		/// the DbType to a "best match" for the provider. It is therefore recommended that 
		/// you use the DatabaseType below until a better type definition system is available.
		/// </summary>
		[Obsolete( "Please use DatabaseType instead." )]
		public DbType DbType
		{
			get { return (DbType) dbType; }
			set
			{
				hasDbType = true;
				dbType = (long) value;
			}
		}

		/// <summary>
		/// The database type of the field in the database. Convert the actual database type
		/// enumeration to a long by casting it in the declaration.
		/// </summary>
		public long DatabaseType
		{
			get { return dbType; }
			set
			{
				hasDbType = true;
				dbType = value;
			}
		}

		/// <summary>
		/// The size or length of the field in the database. String properties will be clipped
		/// to fit.
		/// This feature will obsoleted once Gentle is capable of extracting type and size 
		/// information directly from the database. If specified, the values must match
		/// those extracted from the database (when implemented).
		/// </summary>
		public int Size
		{
			get { return size; }
			set { size = value; }
		}

		/// <summary>
		/// This property indicates whether a DbType was specified. This construct is necessary
		/// because the DbType enum has no value for undefined.
		/// </summary>
		public bool HasDbType
		{
			get { return hasDbType; }
		}

		/// <summary>
		/// Obsolete, use NullValue instead.
		/// </summary>
		[Obsolete( "Use NullValue instead." )]
		public object MagicValue
		{
			get { return nullValue; }
			set { nullValue = value; }
		}

		/// <summary>
		/// This value of this property is used when a column is NotNull and the property value
		/// is null.  If this is undefined the framework will throw an error for NotNull columns
		/// whose values are null.
		/// </summary>
		public object NullValue
		{
			get { return nullValue; }
			set { nullValue = value; }
		}

		#region NullValue Helper Properties for VB.NET Users
		/// <summary>
		/// This property allows type-safe setting of the NullValue for VB users.
		/// </summary>
		public int NullValue_int
		{
			set { NullValue = value; }
		}
		/// <summary>
		/// This property allows type-safe setting of the NullValue for VB users.
		/// </summary>
		public NullOption NullValue_opt
		{
			set { NullValue = value; }
		}
		#endregion

		/// <summary>
		/// This value indicates that the column should not be set on insert and update. It is
		/// primarily useful for columns that are set internally by the database.
		/// </summary>
		public bool IsReadOnly
		{
			get { return isReadOnly; }
			set { isReadOnly = value; }
		}

		/// <summary>
		/// This value indicates that the column must be read after each insert and update. It is
		/// primarily useful for columns that are set internally by the database. Note that using
		/// this feature (by setting this to true for any column) will significantly impact 
		/// performance for the worse, as for every update/insert another select will be 
		/// performed. Also, fields will be updated using reflection after select, which is also
		/// quite slow (depending on the number of columns).
		/// </summary>
		public bool IsUpdateAfterWrite
		{
			get { return isUpdateAfterWrite; }
			set { isUpdateAfterWrite = value; }
		}

		/// <summary>
		/// If member which has this attribute attached is enum then this property
		/// indicates wheter framework saves it as string or as integer.
		/// Default is false, ie enums are saved as integers
		/// </summary>
		public bool HandleEnumAsString
		{
			get { return handleEnumAsString; }
			set { handleEnumAsString = value; }
		}
	}
}