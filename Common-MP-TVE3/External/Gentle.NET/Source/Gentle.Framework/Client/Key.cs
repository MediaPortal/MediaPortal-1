/*
 * Helper class used to transport multiple named values
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: Key.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Text;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// The Key class is used to encapsulate sets of related key/value pairs, such as
	/// statement parameters, primary keys, etc.
	/// </summary>
	/// <remarks>
	/// </remarks>
	public class Key : Hashtable
	{
		#region Members
		/// <summary>
		/// This value is false when the key contains column names as indexers.
		/// </summary>
		internal bool isPropertyKeys;
		/// <summary>
		/// The type from which the key values have been gathered (null if not applicable)
		/// </summary>
		internal Type source;
		/// <summary>
		/// The table name to use for queries made using the current key instance. When
		/// this value is null the default table name (from the TableName attribute) is used.
		/// </summary>
		internal string tableName;
		#endregion

		#region Constructors
		/// <summary>
		/// Construct a new key instance.
		/// </summary>
		/// <param name="tableName">The name of the table used for queries made with this key</param>
		/// <param name="source">The type from which the contained key/value pairs originate. This 
		/// parameter is required for Gentle to be able to translate foreign keys into the proper
		/// column name for the type being selected. If it is null no name translations will take
		/// place.</param>
		/// <param name="isPropertyKeys">True if the keys index values are property names, false
		/// if they are column names.</param>
		public Key( string tableName, Type source, bool isPropertyKeys )
		{
			this.tableName = tableName;
			this.source = source;
			this.isPropertyKeys = isPropertyKeys;
		}

		/// <summary>
		/// Construct a new key instance.
		/// </summary>
		/// <param name="source">The type from which the contained key/value pairs originate. This 
		/// parameter is required for Gentle to be able to translate foreign keys into the proper
		/// column name for the type being selected. If it is null no name translations will take
		/// place.</param>
		/// <param name="isPropertyKeys">True if the keys index values are property names, false
		/// if they are column names.</param>
		public Key( Type source, bool isPropertyKeys ) : this( null, source, isPropertyKeys )
		{
		}

		/// <summary>
		/// Construct a new key instance.
		/// </summary>
		/// <param name="tableName">The name of the table used for queries made with this key</param>
		/// <param name="isPropertyKeys">True if the keys index values are property names, false
		/// if they are column names.</param>
		public Key( string tableName, bool isPropertyKeys ) : this( tableName, null, isPropertyKeys )
		{
		}

		/// <summary>
		/// Construct a new key instance.
		/// </summary>
		/// <param name="isPropertyKeys">True if the keys index values are property names, false
		/// if they are column names.</param>
		public Key( bool isPropertyKeys ) : this( null, null, isPropertyKeys )
		{
		}

		/// <summary>
		/// Construct a new key instance and specify a single key/value pair to add.
		/// </summary>
		/// <param name="tableName">The name of the table used for queries made with this key</param>
		/// <param name="source">The type from which the contained key/value pairs originate. This 
		/// parameter is required for Gentle to be able to translate foreign keys into the proper
		/// column name for the type being selected. If it is null no name translations will take
		/// place.</param>
		/// <param name="isPropertyKeys">True if the keys index values are property names, false
		/// if they are column names.</param>
		/// <param name="name1">The name used as index for a value to be added to this key.</param>
		/// <param name="value1">The value to be added to this key.</param>
		public Key( string tableName, Type source, bool isPropertyKeys, string name1, object value1 ) :
			this( tableName, source, isPropertyKeys )
		{
			this[ name1 ] = value1;
		}

		/// <summary>
		/// Construct a new key instance and specify a single key/value pair to add.
		/// </summary>
		/// <param name="source">The type from which the contained key/value pairs originate. This 
		/// parameter is required for Gentle to be able to translate foreign keys into the proper
		/// column name for the type being selected. If it is null no name translations will take
		/// place.</param>
		/// <param name="isPropertyKeys">True if the keys index values are property names, false
		/// if they are column names.</param>
		/// <param name="name1">The name used as index for a value to be added to this key.</param>
		/// <param name="value1">The value to be added to this key.</param>
		public Key( Type source, bool isPropertyKeys, string name1, object value1 ) :
			this( null, source, isPropertyKeys, name1, value1 )
		{
		}

		/// <summary>
		/// Construct a new key instance and specify a single key/value pair to add.
		/// </summary>
		/// <param name="isPropertyKeys">True if the keys index values are property names, false
		/// if they are column names.</param>
		/// <param name="name1">The name used as index for a value to be added to this key.</param>
		/// <param name="value1">The value to be added to this key.</param>
		public Key( bool isPropertyKeys, string name1, object value1 ) :
			this( null, null, isPropertyKeys, name1, value1 )
		{
		}

		/// <summary>
		/// Construct a new key instance and specify two key/value pairs to add.
		/// </summary>
		/// <param name="tableName">The name of the table used for queries made with this key</param>
		/// <param name="source">The type from which the contained key/value pairs originate. This 
		/// parameter is required for Gentle to be able to translate foreign keys into the proper
		/// column name for the type being selected. If it is null no name translations will take
		/// place.</param>
		/// <param name="isPropertyKeys">True if the keys index values are property names, false
		/// if they are column names.</param>
		/// <param name="name1">The name used as index for the first value to be added to this key.</param>
		/// <param name="value1">The first value to be added to this key.</param>
		/// <param name="name2">The name used as index for the second value to be added to this key.</param>
		/// <param name="value2">The second value to be added to this key.</param>
		public Key( string tableName, Type source, bool isPropertyKeys,
		            string name1, object value1, string name2, object value2 ) :
		            	this( tableName, source, isPropertyKeys, name1, value1 )
		{
			this[ name2 ] = value2;
		}

		/// <summary>
		/// Construct a new key instance and specify two key/value pairs to add.
		/// </summary>
		/// <param name="source">The type from which the contained key/value pairs originate. This 
		/// parameter is required for Gentle to be able to translate foreign keys into the proper
		/// column name for the type being selected. If it is null no name translations will take
		/// place.</param>
		/// <param name="isPropertyKeys">True if the keys index values are property names, false
		/// if they are column names.</param>
		/// <param name="name1">The name used as index for the first value to be added to this key.</param>
		/// <param name="value1">The first value to be added to this key.</param>
		/// <param name="name2">The name used as index for the second value to be added to this key.</param>
		/// <param name="value2">The second value to be added to this key.</param>
		public Key( Type source, bool isPropertyKeys,
		            string name1, object value1, string name2, object value2 ) :
		            	this( null, source, isPropertyKeys, name1, value1, name2, value2 )
		{
		}

		/// <summary>
		/// Construct a new key instance and specify two key/value pairs to add.
		/// </summary>
		/// <param name="isPropertyKeys">True if the keys index values are property names, false
		/// if they are column names.</param>
		/// <param name="name1">The name used as index for the first value to be added to this key.</param>
		/// <param name="value1">The first value to be added to this key.</param>
		/// <param name="name2">The name used as index for the second value to be added to this key.</param>
		/// <param name="value2">The second value to be added to this key.</param>
		public Key( bool isPropertyKeys, string name1, object value1, string name2, object value2 ) :
			this( null, null, isPropertyKeys, name1, value1, name2, value2 )
		{
		}
		#endregion

		#region Element Accessors
		/// <summary>
		/// Add or update a value stored in the key.
		/// </summary>
		/// <param name="name">The name of the entry</param>
		/// <param name="value">The associated value</param>
		public void Add( string name, object value )
		{
			// hashtable.Add doesn't update so we override the Add method to allow overwriting
			this[ name ] = value;
		}

		/// <summary>
		/// Default property that allows us to use "key[ keyName ]" to access the keys
		/// </summary>
		public object this[ string name ]
		{
			get
			{
				if( ! ContainsKey( name ) )
				{
					throw new GentleException( Error.InvalidRequest,
					                           "No field named \"" + name + "\" was specified in this key instance." );
				}
				return base[ name ];
			}
			// note: the hashtable will add the key if it's missing
			set { base[ name ] = value; }
		}
		#endregion

		#region Properties
		/// <summary>
		/// The type from which the key values have been gathered (null if not applicable). If the
		/// object type being selected is different from the source type the contained keys are
		/// assumed to be foreign keys and translated to the target type before being applied as
		/// constraints.
		/// </summary>
		public Type SourceType
		{
			get { return source; }
		}

		/// <summary>
		/// Returns the table name associated with this key or null if none has been set. The
		/// table name specified on a key overrides the table name specified in the TableName
		/// attribute and thus allows dynamic mapping of objects to multiple tables.
		/// </summary>
		public string TableName
		{
			get { return tableName; }
		}
		#endregion

		#region IsPrimaryKeyFields Helper Method
		/// <summary>
		/// Returns a boolean value indicating whether the current key instance contains exactly
		/// the fields compromising the primary key fields of the given type. This is used to 
		/// determine whether a cached statement can be used (non-primary key select statements 
		/// are not cached).
		/// </summary>
		/// <param name="map">The ObjectMap describing a type and its table mapping.</param>
		/// <param name="ignoreConcurrencyColumn"></param>
		/// <returns>True if the key contains only and exactly the primary key fields of the 
		/// given type.</returns>
		public bool IsPrimaryKeyFields( ObjectMap map, bool ignoreConcurrencyColumn )
		{
			// determine expected field count
			int expectedKeyCount = map.PrimaryKeyCount;
			if( GentleSettings.ConcurrencyControl && map.ConcurrencyMap != null &&
			    ! ignoreConcurrencyColumn && ! map.ConcurrencyMap.IsPrimaryKey )
			{
				expectedKeyCount += 1;
			}
			bool result = Count == expectedKeyCount;
			if( result )
			{
				foreach( FieldMap fm in map.Fields )
				{
					string name = isPropertyKeys ? fm.MemberName : fm.ColumnName;
					result &= ! fm.IsPrimaryKey || (fm.IsPrimaryKey && ContainsKey( name ));
				}
			}
			return result;
		}
		#endregion

		#region Static Constructors (GetKey Methods)

		#region GetKey( PersistenceBroker broker, Key key, bool isPropertyKeys, object instance, params string[] members )
		/// <summary>
		/// Obtain a key for the specified object instance and property names. The returned
		/// key will contain the corresponding column names for the type, and thus foreign
		/// key columns must use identical naming for this to work.
		/// </summary>
		/// <param name="broker">The optional PersistenceBroker instance to use for obtaining the
		/// ObjectMap for the supplied object instance. If this parameter is null, Gentle will try
		/// to infer the broker from the object instance. If that fails, it will use the default
		/// provider.</param>
		/// <param name="key">An optional existing key to add the values to</param>
		/// <param name="isPropertyKeys">False is key indexers are column names, true for property names</param>
		/// <param name="instance">The object instance whose property values will be used</param>
		/// <param name="members">The names of the properties to include in the key</param>
		/// <returns>The key</returns>
		public static Key GetKey( PersistenceBroker broker, Key key, bool isPropertyKeys, object instance, params string[] members )
		{
			Check.VerifyNotNull( instance, Error.NullParameter, "instance" );
			// try to infer broker from instance
			if( broker == null && instance is IBrokerLock )
			{
				broker = (instance as IBrokerLock).SessionBroker;
			}
			// WARNING/TODO if broker is null here and no ObjectMap yet exists for the type,
			// the DefaultProvider will be used to create the ObjectMap 
			ObjectMap map = ObjectFactory.GetMap( broker, instance );
			// only set source type reference if this is a new key
			if( key == null )
			{
				key = new Key( map.GetTableName( instance ), instance.GetType(), isPropertyKeys );
			}
			//else
			//	Check.Verify( ! key.isPropertyKeys, Error.DeveloperError, 
			//		"Unable to combine keys containing property names due to possible name clashes." );
			Check.VerifyEquals( key.isPropertyKeys, isPropertyKeys, "Cannot combine property and " +
			                                                        "column names in a single key - use one or the other." );
			Check.VerifyNotNull( members, Error.NullParameter, "members" );
			// process the list of specified properties
			foreach( string memberName in members )
			{
				FieldMap fm = isPropertyKeys ? map.GetFieldMap( memberName ) : map.GetFieldMapFromColumn( memberName );
				Check.VerifyNotNull( fm, Error.NoProperty, map.Type, memberName ); // FIXME outdated error message
				object memberValue = fm.GetValue( instance );
				// translate foreign references to local names
				if( key.SourceType != map.Type )
				{
					// WARNING/TODO if broker is null here and no ObjectMap yet exists for the type,
					// the DefaultProvider will be used to create the ObjectMap 
					ObjectMap keyMap = ObjectFactory.GetMap( broker, key.SourceType );
					fm = keyMap.GetForeignKeyFieldMap( map.TableName, fm.ColumnName );
				}
				key[ isPropertyKeys ? fm.MemberName : fm.ColumnName ] = memberValue;
			}
			// add concurrency value if enabled and instance has revision column
			if( GentleSettings.ConcurrencyControl && map.ConcurrencyMap != null )
			{
				long version = Convert.ToInt64( map.ConcurrencyMap.GetValue( instance ) );
				key[ isPropertyKeys ? map.ConcurrencyMap.MemberName : map.ConcurrencyMap.ColumnName ] = version;
			}
			return key;
		}

		/// <summary>
		/// Obtain a key for the specified object instance and property names. The returned
		/// key will contain the corresponding column names for the type, and thus foreign
		/// key columns must use identical naming for this to work.
		/// </summary>
		/// <param name="key">An optional existing key to add the values to</param>
		/// <param name="isPropertyKeys">False is key indexers are column names, true for property names</param>
		/// <param name="instance">The object instance whose property values will be used</param>
		/// <param name="members">The names of the properties to include in the key</param>
		/// <returns>The key</returns>
		public static Key GetKey( Key key, bool isPropertyKeys, object instance, params string[] members )
		{
			return GetKey( null, key, isPropertyKeys, instance, members );
		}
		#endregion

		#region GetKey( PersistenceBroker broker, bool isPropertyKeys, object instance, params string[] properties )
		/// <summary>
		/// Obtain a new key for the specified object instance and property names. The returned
		/// key will contain the corresponding column names for the type, and thus foreign
		/// key columns must use identical naming for this to work.
		/// </summary>
		/// <param name="broker">The optional PersistenceBroker instance to use for obtaining the
		/// ObjectMap for the supplied object instance. If this parameter is null, Gentle will try
		/// to infer the broker from the object instance. If that fails, it will use the default
		/// provider.</param>
		/// <param name="isPropertyKeys">False is key indexers are column names, true for property names</param>
		/// <param name="instance">The object instance whose property values will be used</param>
		/// <param name="properties">The names of the properties to include in the key</param>
		/// <returns>The requested key</returns>
		public static Key GetKey( PersistenceBroker broker, bool isPropertyKeys, object instance, params string[] properties )
		{
			return GetKey( broker, null, isPropertyKeys, instance, properties );
		}

		/// <summary>
		/// Obtain a new key for the specified object instance and property names. The returned
		/// key will contain the corresponding column names for the type, and thus foreign
		/// key columns must use identical naming for this to work.
		/// </summary>
		/// <param name="isPropertyKeys">False is key indexers are column names, true for property names</param>
		/// <param name="instance">The object instance whose property values will be used</param>
		/// <param name="properties">The names of the properties to include in the key</param>
		/// <returns>The requested key</returns>
		public static Key GetKey( bool isPropertyKeys, object instance, params string[] properties )
		{
			return GetKey( null, null, isPropertyKeys, instance, properties );
		}
		#endregion

		#region GetKey( PersistenceBroker broker, Key key, bool isPropertyKeys, object instance )
		/// <summary>
		/// Obtain a new key for the specified object instance. The returned key will contain the
		/// primary keys of the given instance.
		/// </summary>
		/// <param name="broker">The optional PersistenceBroker instance to use for obtaining the
		/// ObjectMap for the supplied object instance. If this parameter is null, Gentle will try
		/// to infer the broker from the object instance. If that fails, it will use the default
		/// provider.</param>
		/// <param name="key">An optional existing key to add the values to</param>
		/// <param name="isPropertyKeys">False is key indexers are column names, true for property names</param>
		/// <param name="instance">The object instance whose property values will be used</param>
		/// <returns>A key instance containing the primary key values of the given object instance</returns>
		public static Key GetKey( PersistenceBroker broker, Key key, bool isPropertyKeys, object instance )
		{
			// try to infer broker from instance
			if( broker == null && instance is IBrokerLock )
			{
				broker = (instance as IBrokerLock).SessionBroker;
			}
			// WARNING/TODO if broker is null here and no ObjectMap yet exists for the type,
			// the DefaultProvider will be used to create the ObjectMap 
			ObjectMap map = ObjectFactory.GetMap( broker, instance );
			return GetKey( broker, key, isPropertyKeys, instance, map.GetPrimaryKeyNames( isPropertyKeys ) );
		}

		/// <summary>
		/// Obtain a new key for the specified object instance. The returned key will contain the
		/// primary keys of the given instance.
		/// </summary>
		/// <param name="key">An optional existing key to add the values to</param>
		/// <param name="isPropertyKeys">False is key indexers are column names, true for property names</param>
		/// <param name="instance">The object instance whose property values will be used</param>
		/// <returns>A key instance containing the primary key values of the given object instance</returns>
		public static Key GetKey( Key key, bool isPropertyKeys, object instance )
		{
			return GetKey( null, key, isPropertyKeys, instance );
		}
		#endregion

		#region GetKey( PersistenceBroker broker, bool isPropertyKeys, object instance )
		/// <summary>
		/// Obtain a new key for the specified object instance. The returned key will contain the
		/// primary keys of the given instance.
		/// </summary>
		/// <param name="broker">The optional PersistenceBroker instance to use for obtaining the
		/// ObjectMap for the supplied object instance. If this parameter is null, Gentle will try
		/// to infer the broker from the object instance. If that fails, it will use the default
		/// provider.</param>
		/// <param name="isPropertyKeys">False is key indexers are column names, true for property names</param>
		/// <param name="instance">The object instance whose property values will be used</param>
		/// <returns>A key instance containing the primary key values of the given object instance</returns>
		public static Key GetKey( PersistenceBroker broker, bool isPropertyKeys, object instance )
		{
			return GetKey( broker, null, isPropertyKeys, instance );
		}

		/// <summary>
		/// Obtain a new key for the specified object instance. The returned key will contain the
		/// primary keys of the given instance.
		/// </summary>
		/// <param name="isPropertyKeys">False is key indexers are column names, true for property names</param>
		/// <param name="instance">The object instance whose property values will be used</param>
		/// <returns>A key instance containing the primary key values of the given object instance</returns>
		public static Key GetKey( bool isPropertyKeys, object instance )
		{
			return GetKey( null, null, isPropertyKeys, instance );
		}
		#endregion

		#endregion

		#region Equals Overrides
		/// <summary>
		/// Calling this method returns the current key instance as a comma-separated list of key=value 
		/// pairs enclosed in a parenthesis.
		/// </summary>
		/// <returns>The string representation of this key</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append( "(" );
			int count = 0;
			foreach( string key in Keys )
			{
				sb.AppendFormat( "{0}={1}", key, this[ key ] );
				count++;
				if( count < Count )
				{
					sb.Append( "," );
				}
			}
			sb.Append( ")" );
			return sb.ToString();
		}

		/// <summary>
		/// Obtain the hashcode of this key instance. This is calculated as the sum of all 
		/// hashcodes of all keys and values.
		/// </summary>
		/// <returns>The hashcode of this Key instance</returns>
		public override int GetHashCode()
		{
			int hash = 0;
			if( Count == 0 )
			{
				return base.GetHashCode();
			}
			foreach( string entry in Keys )
			{
				hash += entry.GetHashCode();
				hash += this[ entry ].GetHashCode();
			}
			return hash;
		}

		/// <summary>
		/// Compare this key instance to another object. 
		/// </summary>
		/// <param name="obj">The object to compare this instance to</param>
		/// <returns>True if the object passed is a Key instance with identical hashcode value</returns>
		public override bool Equals( object obj )
		{
			if( obj == null || obj.GetType() != GetType() )
			{
				return false;
			}
			Key other = obj as Key;
			bool result = GetHashCode() == other.GetHashCode();
			result &= other.GetHashCode() == GetHashCode();
			return result;
		}
		#endregion
	}
}