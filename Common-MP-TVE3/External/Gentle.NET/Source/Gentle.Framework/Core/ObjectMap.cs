/*
 * Helper class for caching obtained meta-data on persistable objects
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ObjectMap.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Helper class for determining constructors, parameter ordering and whatever else is needed
	/// in order to construct objects.
	/// </summary>
	public class ObjectMap : TableMap
	{
		private Type type;
		private bool isDynamicTable;
		private CacheStrategy cacheStrategy;
		private string schemaName;
		private FieldMap concurrencyMap;
		private FieldMap softDeleteMap;
		private FieldMap inheritanceMap;
		private ObjectConstructor objectConstructor;
		//private Hashtable dbMaps; // providerName-indexed list of TableMap instances
		private Hashtable views; // name-indexed list of custom views
		private ArrayList validations;

		/// <summary>
		/// Construct an ObjectMap instance for the given type.
		/// </summary>
		/// <param name="broker">The PersistenceBroker instance to which this map is related.</param>
		/// <param name="type">The type for which this instance is a map</param>
		public ObjectMap( PersistenceBroker broker, Type type ) : base( broker )
		{
			this.type = type;
			if( type != null )
			{
				SetTableName( type );
			}
			//this.dbMaps = new Hashtable();
			views = new Hashtable();
			validations = new ArrayList();
			objectConstructor = new ObjectConstructor( this );
		}

		/// <summary>
		/// Update the current ObjectMap instance with the table name obtained from
		/// the Gentle TableNameAttribute. 
		/// Also determines whether table name is dynamic or fixed.
		/// </summary>
		internal void SetTableName( Type type )
		{
			// determine if table name is dynamic or fixed
			isDynamicTable = type.GetInterface( "ITableName", false ) != null;
			// get fixed name if present
			object[] attrs = type.GetCustomAttributes( typeof(TableNameAttribute), true );
			if( attrs != null && attrs.Length == 1 )
			{
				TableNameAttribute attr = (TableNameAttribute) attrs[ 0 ];
				TableName = attr.Name;
				cacheStrategy = attr.CacheStrategy;
				schemaName = attr.Schema;
			}
			else
			{
				Check.Verify( isDynamicTable, Error.DeveloperError,
				              "The type {0} must either have a TableName attribute or implement ITableName.", type );
			}
		}

		/// <summary>
		/// This method is called to obtain the table name for the current object instance.
		/// </summary>
		/// <param name="instance">The instance from which to obtain the table name.</param>
		/// <returns>The table name</returns>
		internal string GetTableName( object instance )
		{
			if( ! isDynamicTable )
			{
				return TableName;
			}
			else
			{
				string name = (instance as ITableName).TableName;
				Check.VerifyNotNull( name, Error.NullProperty, "TableName" );
				return name;
			}
		}

		/// <summary>
		/// Method to scan for and determine the least expensive constructor for a given array of 
		/// column names. 
		/// </summary>
		/// <param name="columnNames">The column names of the result set</param>
		/// <param name="row">A sample row used to determine if type conversion is needed</param>
		/// <returns>A hash which can be used as constructor selector subsequently or 0 if no
		/// valid constructor could be found for the given columns.</returns>
		public int DetermineConstructor( string[] columnNames, object[] row )
		{
			return objectConstructor.DetermineConstructor( columnNames, row );
		}

		/// <summary>
		/// Construct an object instance using the constructor associated with the given hash code.
		/// </summary>
		/// <param name="columnComboHashCode">The hash code of the column names used to select a 
		/// specific constructor</param>
		/// <param name="row">The data to use for object creation</param>
		/// <param name="broker">The PersistenceBroker instance used to fetch the current row</param>
		/// <returns>An new object instance</returns>
		/// <exception cref="GentleException"> will be raised if no object could be created</exception>
		public object Construct( int columnComboHashCode, object[] row, PersistenceBroker broker )
		{
			return objectConstructor.Construct( columnComboHashCode, row, broker );
		}

		/// <summary>
		/// Construct an object instance using the data supplied in the key. The type for which this 
		/// is a map is scanned for constructors that can be used with the available keys from the
		/// given Key instance.
		/// </summary>
		/// <param name="key">A full set of key/value pairs to use for constructing the object. The
		/// type being constructed must have a constructor with exactly the number of arguments
		/// contained in the given Key instance.</param>
		/// <param name="broker">The PersistenceBroker instance used to fetch the current row</param>
		/// <returns>An instance of the given type</returns>
		/// <exception cref="GentleException"> will be raised if no object could be created</exception>
		internal object Construct( Key key, PersistenceBroker broker )
		{
			// split key into seperate arrays of property names and their values
			string[] names = new string[key.Count];
			key.Keys.CopyTo( names, 0 );
			object[] values = new object[key.Count];
			key.Values.CopyTo( values, 0 );
			// find constructor to use
			int hash = objectConstructor.DetermineConstructor( names, values );
			return Construct( hash, values, broker );
		}

		/// <summary>
		/// Determine the index position of the given rowName in the supplied columnNames array.
		/// </summary>
		/// <param name="row">The array of column names to search</param>
		/// <param name="columnName">The name to look for</param>
		/// <returns>The index of name in the array or -1 if not found</returns>
		private static int GetRowIndex( string[] row, string columnName )
		{
			for( int i = 0; i < row.Length; i++ )
			{
				if( String.Compare( columnName, row[ i ], true ) == 0 )
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Update the properties of the given object using the data in the supplied column name
		/// and column data arrays. Column names are mapped to their corresponding property, and
		/// only non-primary key columns are updated. The primary key columns are assumed to have
		/// been updated in the constructor used to create the object instance.
		/// </summary>
		/// <param name="obj">The object to update</param>
		/// <param name="columnNames">The column names</param>
		/// <param name="row">The column values</param>
		internal void SetProperties( object obj, string[] columnNames, object[] row )
		{
			foreach( FieldMap fm in Fields )
			{
				// object should already have the proper primary key values, no need
				// to try to update those
				if( ! fm.IsPrimaryKey )
				{
					int index = GetRowIndex( columnNames, fm.ColumnName );
					if( index >= 0 ) // allow row to contain columns not for this object type
					{
						object val = row[ index ];
						fm.SetValue( obj, val );
					}
				}
			}
		}

		internal object GetMemberValue( object obj, string memberName, Type targetType, bool allowNull )
		{
			Check.VerifyEquals( type, obj.GetType(), "Type mismatch in ObjectMap (internal error)." );
			MemberInfo[] mi = type.GetMember( memberName, MemberTypes.Field | MemberTypes.Property, Reflector.InstanceCriteria );
			try
			{
				object result = null;
				if( mi != null && mi.Length > 0 )
				{
					if( mi[ 0 ] is PropertyInfo )
					{
						result = (mi[ 0 ] as PropertyInfo).GetValue( obj, Reflector.InstanceCriteria, null, null, null );
					}
					else
					{
						result = (mi[ 0 ] as FieldInfo).GetValue( obj );
					}
				}
				if( result != null && targetType != null && result.GetType() != targetType )
				{
					result = TypeConverter.Get( targetType, result.ToString() );
				}
				return result == null && ! allowNull ? "" : result;
			}
			catch( Exception e )
			{
				if( allowNull )
				{
					return null;
				}
				else
				{
					throw new GentleException( Error.Unspecified, "Unable to perform type conversion.", e );
				}
			}
		}

		internal void SetMemberValue( object obj, string memberName, object value )
		{
			Check.VerifyEquals( type, obj.GetType(), "Type mismatch in ObjectMap (internal error)." );
			MemberInfo[] mi = type.GetMember( memberName, MemberTypes.Field | MemberTypes.Property, Reflector.InstanceCriteria );
			if( mi != null && mi.Length > 0 )
			{
				if( mi[ 0 ] is PropertyInfo )
				{
					PropertyInfo pi = mi[ 0 ] as PropertyInfo;
					if( value != null && value != null && value.GetType() != pi.PropertyType )
					{
						value = TypeConverter.Get( pi.PropertyType, value );
					}
					pi.SetValue( obj, value, Reflector.InstanceCriteria, null, null, null );
				}
				else
				{
					FieldInfo fi = mi[ 0 ] as FieldInfo;
					if( value != null && value != null && value.GetType() != fi.FieldType )
					{
						value = TypeConverter.Get( fi.FieldType, value );
					}
					fi.SetValue( obj, value, Reflector.InstanceCriteria, null, null );
				}
			}
		}

		/// <summary>
		/// Obtain a <see cref="FieldMap"/> instance for the given property name.
		/// </summary>
		/// <param name="propertyName">The name of the property</param>
		/// <returns>The FieldMap of the property</returns>
		public FieldMap GetFieldMap( string propertyName )
		{
			return Fields.FindProperty( propertyName );
		}

		#region Methods for obtaining reference information (i.e. object dependency info)
		/// <summary>
		/// Obtain a <see cref="FieldMap"/> instance from the current map. The field
		/// must be a foreign key reference to the given property on the given type.
		/// </summary>
		/// <param name="referencedType">The type to which the property should refer</param>
		/// <param name="propertyName">The name of the property on the referenced type</param>
		/// <returns>The FieldMap of the foreign key property from this map</returns>
		public FieldMap GetForeignKeyFieldMap( Type referencedType, string propertyName )
		{
			ObjectMap referencedMap = ObjectFactory.GetMap( broker, referencedType );
			FieldMap fm = GetForeignKeyFieldMap( referencedMap.QuotedTableName,
			                                     referencedMap.GetFieldMap( propertyName ).QuotedColumnName );

			// If no relation was found, try the reverse
			// TODO this is only here because relation info is only stored with the map
			// having the foreign key column; it will be removed in the future
			if( fm == null )
			{
				foreach( FieldMap reverse in Fields )
				{
					fm = referencedMap.GetForeignKeyFieldMap( QuotedTableName, reverse.QuotedColumnName );
					if( fm != null && fm.MemberName == propertyName )
					{
						return reverse;
					}
					else
					{
						fm = null;
					}
				}
			}
			Check.VerifyNotNull( fm, Error.DeveloperError,
			                     "The type {0} does not have a foreign key relation to the property {1} " +
			                     "of type {2}. Check if you need to add a ForeignKey attribute.",
			                     Type.Name, propertyName, referencedType );
			return fm;
		}
		#endregion

		/// <summary>
		/// Obtain the column name corresponding to a given property name.
		/// </summary>
		/// <param name="propertyName">The name of the property</param>
		/// <returns>The name of the column</returns>
		public string GetColumnName( string propertyName )
		{
			FieldMap fm = Fields.FindProperty( propertyName );
			return fm != null ? fm.ColumnName : null;
		}

		/// <summary>
		/// Obtain the property name of a given column name.
		/// </summary>
		/// <param name="columnName">The name of the column</param>
		/// <returns>The name of the property</returns>
		public string GetPropertyName( string columnName )
		{
			FieldMap fm = Fields.FindColumn( columnName );
			return fm != null ? fm.MemberName : null;
		}

		/// <summary>
		/// Obtain the system type of the given property.
		/// </summary>
		/// <param name="propertyName">The name of the property</param>
		/// <returns>The system type of the property</returns>
		public Type GetPropertyType( string propertyName )
		{
			FieldMap fm = Fields.FindProperty( propertyName );
			return fm != null ? fm.Type : null;
		}

		/// <summary>
		/// Obtain a string array holding the (member or column) names of all the primary keys.
		/// </summary>
		/// <returns></returns>
		public string[] GetPrimaryKeyNames( bool isMemberNames )
		{
			string[] result = new string[PrimaryKeyCount];
			int pos = 0;
			foreach( FieldMap fm in Fields )
			{
				if( fm.IsPrimaryKey )
				{
					result[ pos++ ] = isMemberNames ? fm.MemberName : fm.ColumnName;
				}
			}
			return result;
		}

		public string GetInstanceHashKey( object instance )
		{
			StringBuilder sb = new StringBuilder();
			sb.Append( type.FullName );
			int validPrimaryKeyCount = 0;
			foreach( FieldMap fm in Fields )
			{
				if( fm.IsPrimaryKey )
				{
					object value = fm.GetValue( instance );
					sb.AppendFormat( "|{0}={1}", fm.MemberName, value != null ? value : "null" );
					if( value != null )
					{
						validPrimaryKeyCount++;
					}
				}
			}
			// add error checking to ensure that a valid cache key was produced
			Check.Verify( validPrimaryKeyCount > 0, Error.InvalidRequest, String.Format( "{0}{1}Reason: {2}",
			                                                                             "Unable to construct an instance key for adding object to the cache.",
			                                                                             Environment.NewLine,
			                                                                             "No primary key defined for object (all instances would be considered equal by the cache)." ) );
			Check.VerifyEquals( validPrimaryKeyCount, PrimaryKeyCount, Error.InvalidRequest, String.Format( "{0}{1}Reason: {2}",
			                                                                                                "Unable to construct an instance key for adding object to the cache.",
			                                                                                                Environment.NewLine,
			                                                                                                "One or more of the primary key fields contained a null value." ) );
			return sb.ToString();
		}

		public string GetInstanceHashKey( Hashtable values )
		{
			StringBuilder sb = new StringBuilder();
			sb.Append( type.FullName );
			int validPrimaryKeyCount = 0;
			foreach( FieldMap fm in Fields )
			{
				if( fm.IsPrimaryKey )
				{
					object value = values[ fm.MemberName ];
					sb.AppendFormat( "|{0}={1}", fm.MemberName, value != null ? value : "null" );
					if( value != null )
					{
						validPrimaryKeyCount++;
					}
				}
			}
			// add error checking to ensure that a valid cache key was produced
			Check.Verify( validPrimaryKeyCount > 0, Error.InvalidRequest, String.Format( "{0}{1}Reason: {2}",
			                                                                             "Unable to construct an instance key for adding object to the cache.",
			                                                                             Environment.NewLine,
			                                                                             "No primary key defined for object (all instances would be considered equal by the cache)." ) );
			Check.VerifyEquals( validPrimaryKeyCount, PrimaryKeyCount, Error.InvalidRequest, String.Format( "{0}{1}Reason: {2}",
			                                                                                                "Unable to construct an instance key for adding object to the cache.",
			                                                                                                Environment.NewLine,
			                                                                                                "One or more of the primary key fields contained a null value." ) );
			return sb.ToString();
		}

		/// <summary>
		/// Update the identity property with the value from the supplied <see cref="SqlResult"/>
		/// instance. The identity property must be a single integer value for this to work. The 
		/// identity property is the one holding the PrimaryKey attribute with the AutoGenerated
		/// value set to true.
		/// </summary>
		/// <param name="obj">The object instance whose identity should be set</param>
		/// <param name="identity">The row identity (i.e. the LastRowId property of SqlResult)</param>
		public void SetIdentity( object obj, int identity )
		{
			Check.VerifyEquals( type, obj.GetType(),
			                    "ObjectMap for {0} cannot set identity on {1}.", type, obj.GetType() );
			Check.VerifyNotNull( IdentityMap, Error.NullIdentity, type );
			object tmp = Convert.ChangeType( identity, IdentityMap.Type );
			IdentityMap.SetValue( obj, tmp );
		}

		/// <summary>
		/// Adds a column to the specified named DataView. The ViewMap is a small helper class
		/// used to connect DataView columns with object properties.
		/// </summary>
		/// <param name="viewName">The name of the DataView</param>
		/// <param name="viewMap">The ViewMap instance to add to this view</param>
		public void AddViewColumn( string viewName, ViewMap viewMap )
		{
			ObjectView view;
			if( ! views.ContainsKey( viewName ) )
			{
				view = new ObjectView( this, viewName );
				views[ viewName ] = view;
			}
			else
			{
				view = (ObjectView) views[ viewName ];
			}
			view.AddColumn( viewMap );
		}

		#region Properties
		/// <summary>
		/// The business class for which this instance holds mapping information.
		/// </summary>
		public Type Type
		{
			get { return type; }
		}
		/// <summary>
		/// True if the type to which this ObjectMap belongs maps to multiple tables.
		/// </summary>
		public bool IsDynamicTable
		{
			get { return isDynamicTable; }
		}

		public bool IsAutoGeneratedPrimaryKey
		{
			get { return IdentityMap != null; }
		}

		/// <summary>
		/// A collection of named DataViews we can generate for the type represented.
		/// </summary>
		public Hashtable Views
		{
			get { return views; }
		}
		/// <summary>
		/// Get the FieldMap for the concurrency column.
		/// </summary>
		internal FieldMap ConcurrencyMap
		{
			get { return concurrencyMap; }
			set { concurrencyMap = value; }
		}
		/// <summary>
		/// Get the FieldMap for the soft delete column.
		/// </summary>
		internal FieldMap SoftDeleteMap
		{
			get { return softDeleteMap; }
			set { softDeleteMap = value; }
		}
		/// <summary>
		/// Get the FieldMap for the inheritance column (where the actual type name is stored).
		/// </summary>
		internal FieldMap InheritanceMap
		{
			get { return inheritanceMap; }
			set { inheritanceMap = value; }
		}
		/// <summary>
		/// Get the validations that are to be performed on the object.
		/// </summary>
		internal ArrayList Validations
		{
			get { return validations; }
		}
		/// <summary>
		/// True if the type to which this ObjectMap belongs should use soft delete (i.e. mark
		/// rows as deleted and filter them from queries on select) instead of hard delete.
		/// </summary>
		internal bool IsSoftDelete
		{
			get { return softDeleteMap != null; }
		}

		/// <summary>
		/// This value indicates whether the table (to which this type maps) has any columns
		/// that are updated by the database on write. 
		/// </summary>
		internal bool IsUpdateAfterWrite
		{
			get
			{
				bool result = false;
				foreach( FieldMap fm in Fields )
				{
					result |= fm.IsUpdateAfterWrite;
				}
				return result;
			}
		}

		/// <summary>
		/// The cache behavior for objects of this type. Can be either permanent,
		/// temporary (the default unless overridden in the config file), or never.
		/// </summary>
		public CacheStrategy CacheStrategy
		{
			get { return cacheStrategy; }
			set { cacheStrategy = value; }
		}

		/// <summary>
		/// The schema name to use for objects of this type. This overrides any value
		/// defined for a specific provider. This is currently unused and has no effect.
		/// </summary>
		public string SchemaName
		{
			get { return schemaName; }
			set { schemaName = value; }
		}
		#endregion
	}
}