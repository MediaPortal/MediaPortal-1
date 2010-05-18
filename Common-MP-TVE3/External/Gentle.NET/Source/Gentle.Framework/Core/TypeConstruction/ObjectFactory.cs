/*
 * Object construction facility
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ObjectFactory.cs 1234 2008-03-14 11:41:44Z mm $
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// This class handles contructon of objects (created from SqlResult objects).
	/// </summary>
	public class ObjectFactory
	{
		internal static Hashtable maps = new Hashtable(); // object maps indexed by type name

		private ObjectFactory()
		{
		}

		public static void ClearMaps()
		{
			lock( maps.SyncRoot )
			{
				if( maps != null && maps.Count > 0 )
				{
					maps.Clear();
				}
			}
		}

		/// <summary>
		/// Process the given type and construct the corresponding ObjectMap instance.
		/// </summary>
		/// <param name="broker">The PersistenceBroker to use for obtaining metadata on the type. If
		/// null is passed the DefaultProvider will be used.</param>
		/// <param name="type">The type to process.</param>
		/// <returns>An ObjectMap instance describing the type</returns>
		protected static ObjectMap ConstructMap( PersistenceBroker broker, Type type )
		{
			Check.Verify( IsTypeSupported( type ), Error.UnsupportedType, type );
			ObjectMap map = new ObjectMap( broker, type );
			// get persistence attributes
			IList memberAttributeInfos = Reflector.FindMembers( Reflector.InstanceCriteria,
			                                                    type, false, typeof(TableColumnAttribute),
			                                                    typeof(PrimaryKeyAttribute), typeof(ForeignKeyAttribute),
			                                                    typeof(ConcurrencyAttribute), typeof(SoftDeleteAttribute),
			                                                    typeof(SequenceNameAttribute), typeof(InheritanceAttribute) );
			foreach( MemberAttributeInfo mai in memberAttributeInfos )
			{
				map.Fields.Add( new FieldMap( map, mai ) );
			}
			// get custom view(s)
			memberAttributeInfos = Reflector.FindMembers( Reflector.InstanceCriteria, type, true, typeof(CustomViewAttribute) );
			foreach( MemberAttributeInfo mai in memberAttributeInfos )
			{
				if( mai.MemberInfo is PropertyInfo )
				{
					foreach( CustomViewAttribute cv in mai.Attributes )
					{
						PropertyInfo pi = mai.MemberInfo as PropertyInfo;
						// create a viewmap for this attribute
						ViewMap vm = new ViewMap( map, pi.Name, pi.PropertyType, cv );
						// add it to the specified view
						map.AddViewColumn( cv.ViewName, vm );
					}
				}
			}
			// get validation(s)
			memberAttributeInfos = Reflector.FindMembers( Reflector.InstanceCriteria, type, false,
			                                              typeof(ValidatorBaseAttribute) );
			foreach( MemberAttributeInfo mai in memberAttributeInfos )
			{
				foreach( ValidatorBaseAttribute vb in mai.Attributes )
				{
					PropertyInfo pi = mai.MemberInfo as PropertyInfo;
					// create a validation map for this attribute
					ValidationMap va = new ValidationMap( map, pi.Name, pi.PropertyType, vb );
					// add it to the specified view
					map.Validations.Add( va );
				}
			}

			// analyze the actual database schema and update our internal maps accordingly
			if( GentleSettings.AnalyzerLevel != AnalyzerLevel.None )
			{
				if( broker == null )
				{
					broker = new PersistenceBroker( type );
				}
				IDatabaseAnalyzer da = broker.Provider.GetAnalyzer();
				if( da != null ) // not all persistence engines may support this
				{
					da.UpdateObjectMap( map );
				}
			}
			// return the generated object <-> database map
			return map;
		}

		/// <summary>
		/// Process the given type and construct the corresponding ObjectMap instance. If the
		/// map has been created previously a cached instance is returned.
		/// </summary>
		/// <param name="broker">The PersistenceBroker to use for obtaining metadata on the type. If
		/// null is passed the DefaultProvider will be used.</param>
		/// <param name="type">The type to process</param>
		/// <returns>An ObjectMap instance describing the type</returns>
		public static ObjectMap GetMap( PersistenceBroker broker, Type type )
		{
			ObjectMap om = null;
			lock( maps.SyncRoot )
			{
				if( ! maps.ContainsKey( type ) )
				{
					if( broker == null )
					{
						IGentleProvider provider = ProviderFactory.GetProvider( type );
						broker = provider.Broker;
					}
					om = ConstructMap( broker, type );
					maps[ type ] = om;
				}
				else
				{
					om = (ObjectMap) maps[ type ];
				}
			}
			// throw an exception for unsupported types
			Check.VerifyNotNull( om, Error.UnsupportedType, type );
			return om;
		}

		/// <summary>
		/// Process the given object's type and construct the corresponding ObjectMap instance. If the
		/// map has been created previously a cached instance is returned.
		/// </summary>
		/// <param name="broker">The PersistenceBroker to use for obtaining metadata on the type. If
		/// null is passed the DefaultProvider will be used.</param>
		/// <param name="obj">The instance to process</param>
		/// <returns>An ObjectMap instance describing the type</returns>
		public static ObjectMap GetMap( PersistenceBroker broker, object obj )
		{
			Check.VerifyNotNull( obj, Error.NullParameter, "obj" );
			Type type = obj.GetType();
			if( type == typeof(string) )
			{
				return GetMap( obj as string );
			}
			return GetMap( broker, obj.GetType() );
		}

		/// <summary>
		/// Retrieve the ObjectMap for the type persisted to the specified table name. The map
		/// must have been created previously or null will be returned. If multiple types map
		/// to the same table, the first match is returned.
		/// </summary>
		/// <param name="tableName">The table name identifying the type</param>
		/// <returns>An ObjectMap instance describing the type</returns>
		internal static ObjectMap GetMap( string tableName )
		{
			lock( maps.SyncRoot )
			{
				foreach( ObjectMap map in maps.Values )
				{
					// ToUpperInvariant is CLR optimized for speed
					if( map.TableName.ToUpperInvariant() == tableName.ToUpperInvariant() )
					{
						return map;
					}
				}
			}
			Check.Fail( Error.NoObjectMapForTable, tableName );
			return null;
		}

		/// <summary>
		/// Construct an instance of the given type using data from the first row of the
		/// supplied SqlResult instance. 
		/// Refer to the Construct method of the <see cref="ObjectMap"/> class for details.
		/// </summary>
		/// <param name="type">The type of object to construct</param>
		/// <param name="sr">The SqlResult instance holding the data</param>
		/// <param name="key">Additional fields not available in the result set (e.g. primary key fields)</param>
		/// <returns>An instance of the given type</returns>
		/// <exception cref="GentleException"> will be raised if no object could be created</exception>
		public static object GetInstance( Type type, SqlResult sr, Key key )
		{
			Check.Verify( sr != null && sr.ErrorCode == 0 && sr.RowsContained == 1,
			              Error.UnexpectedRowCount, sr.RowsContained, 1 );
			ObjectMap objectMap = GetMap( sr.SessionBroker, type );
			// check for dynamic type
			if( objectMap.InheritanceMap != null )
			{
				string assemblyQualifiedName = sr.GetString( 0, objectMap.InheritanceMap.ColumnName );
				type = LoadType( assemblyQualifiedName );
				objectMap = GetMap( sr.SessionBroker, type ); // update objectmap to actual type from database
			}
			// determine constructor
			object[] row = (object[]) sr.Rows[ 0 ];
			int columnComboHashCode = objectMap.DetermineConstructor( sr.ColumnNames, row );
			// create object
			object result = objectMap.Construct( columnComboHashCode, row, sr.SessionBroker );
			// check whether to store query result information
			if( GentleSettings.CacheObjects && GentleSettings.SkipQueryExecution &&
			    objectMap.CacheStrategy != CacheStrategy.Never )
			{
				IList<string> results = new List<string> { objectMap.GetInstanceHashKey( result ) };
				CacheManager.Insert( sr.Statement.CacheKey, results );
			}
			// mark as persisted
			if( result is IEntity )
			{
				(result as IEntity).IsPersisted = true;
			}
			return result;
		}
		public static T GetInstance<T>( SqlResult sr, Key key )
		{
			return (T) GetInstance( typeof(T), sr, key );
		}

		/// <summary>
		/// Load the type specified using its fully qualified name (its AssemblyQualifiedName property).
		/// </summary>
		/// <param name="assemblyQualifiedName">The fully qualified type name. This is a string consisting of
		/// up to 4 items: type name, assembly name, culture, public key token (strong named assemblies only).</param>
		/// <returns>A reference to the specified type. If an error occurs loading the type and exception
		/// will be raised.</returns>
		private static Type LoadType( string assemblyQualifiedName )
		{
			Type type;
			type = Type.GetType( assemblyQualifiedName, false );
			if( type == null )
			{
				string[] typeData = assemblyQualifiedName.Split( ',' );
				Assembly assembly = null;
				string assemblyName = null;

				// if typeData contains only 1 element, activate Gentle 1.2.5 compatibility mode 
				if( typeData.Length == 1 )
				{
					// try to extract the assembly name from the type name
					string assemblyNameGuess = assemblyQualifiedName.Substring( 0, assemblyQualifiedName.LastIndexOf( '.' ) );
					assemblyName = assemblyNameGuess;
					while( assembly == null && assemblyNameGuess.Length != 0 )
					{
						assembly = Assembly.Load( assemblyNameGuess );
						if( assembly == null )
						{
#pragma warning disable 0618
							assembly = Assembly.LoadWithPartialName( assemblyNameGuess );
						}
						assemblyNameGuess = assemblyNameGuess.Substring( 0, Math.Max( 0, assemblyNameGuess.LastIndexOf( '.' ) ) );
					}
				}
				else // use the assembly name specified as part of the FQTN
				{
					assemblyName = typeData[ 1 ];
					assembly = Assembly.Load( assemblyName );
					if( assembly == null )
					{
						assembly = Assembly.LoadWithPartialName( assemblyName );
#pragma warning restore 0618
					}
				}
				Check.VerifyNotNull( assembly, Error.UnknownAssembly, assemblyName );

				// try to find type using fully qualified name (includes culture and assembly publickeytoken)
				type = assembly.GetType( assemblyQualifiedName, false );
				// try to find type using its name only
				if( type == null )
				{
					type = assembly.GetType( typeData[ 0 ] );
				}
			}
			Check.VerifyNotNull( type, Error.DeveloperError, "The type '{0}' could not be loaded.", assemblyQualifiedName );
			return type;
		}

		/// <summary>
		/// Construct an instance of the given type using data from the first row of the
		/// supplied SqlResult instance. 
		/// Refer to the Construct method of the <see cref="ObjectMap"/> class for details.
		/// </summary>
		/// <param name="type">The type of object to construct</param>
		/// <param name="sr">The SqlResult instance holding the data</param>
		/// <returns>An instance of the given type</returns>
		/// <exception cref="GentleException"> will be raised if no object could be created</exception>
		public static object GetInstance( Type type, SqlResult sr )
		{
			return GetInstance( type, sr, null );
		}
		public static T GetInstance<T>( SqlResult sr )
		{
			return GetInstance<T>( sr, null );
		}

		/// <summary>
		/// Construct multiple objects of a given type from the data contained in the given SqlResult
		/// object. Refer to the Construct method of the <see cref="ObjectMap"/> class for details.
		/// </summary>
		/// <param name="type">The type of object to construct</param>
		/// <param name="sr">The SqlResult instance holding the data</param>
		/// <returns>An IList holding the created objects</returns>
		/// <exception cref="GentleException"> will be raised if no object could be created</exception>
		public static IList GetCollection( Type type, SqlResult sr )
		{
			return GetCollection( type, sr, null );
		}
		public static IList<T> GetCollection<T>( SqlResult sr )
		{
			return GetCollection<T>( sr, null );
		}

		/// <summary>
		/// Construct multiple objects of a given type from the data contained in the given SqlResult
		/// object. Refer to the Construct method of the <see cref="ObjectMap"/> class for details.
		/// </summary>
		/// <param name="type">The type of object to construct</param>
		/// <param name="sr">The SqlResult instance holding the data</param>
		/// <param name="result">An optional existing container in which to store the created objects. If
		/// this parameter is null a new IList instance will be created.</param>
		/// <returns>An IList holding the created objects</returns>
		/// <exception cref="GentleException"> will be raised if no object could be created</exception>
		public static IList GetCollection( Type type, SqlResult sr, IList result )
		{
			if( result == null )
			{
				result = MakeGenericList( type );
			}
			ObjectMap objectMap = GetMap( sr.SessionBroker, type );
			// check whether to store query result information
			bool isCache = GentleSettings.CacheObjects && GentleSettings.SkipQueryExecution &&
			               objectMap.CacheStrategy != CacheStrategy.Never;
			IList cache = isCache ? new ArrayList() : null;
			// remember keys of cached objects to permit SQE
			// process result set
			if( sr.RowsContained > 0 )
			{
				ObjectMap actualMap = objectMap;
				int columnComboHashCode = 0;
				// cache constructor info for dynamic type construction
				Hashtable typeHash = null;
				Hashtable typeMaps = null;
				if( objectMap.InheritanceMap != null )
				{
					typeHash = new Hashtable();
					typeMaps = new Hashtable();
				}
				else // precompute fixed hash
				{
					columnComboHashCode = objectMap.DetermineConstructor( sr.ColumnNames, (object[]) sr.Rows[ 0 ] );
				}
				// process result set
				for( int i = 0; i < sr.Rows.Count; i++ )
				{
					object[] row = (object[]) sr.Rows[ i ];
					// dynamic object construction handling
					if( typeHash != null )
					{
						string assemblyQualifiedName = sr.GetString( i, objectMap.InheritanceMap.ColumnName );
						if( typeHash.ContainsKey( assemblyQualifiedName ) )
						{
							columnComboHashCode = (int) typeHash[ assemblyQualifiedName ];
							actualMap = (ObjectMap) typeMaps[ assemblyQualifiedName ];
						}
						else
						{
							Type rowType = LoadType( assemblyQualifiedName );
							actualMap = GetMap( sr.SessionBroker, rowType );
							columnComboHashCode = actualMap.DetermineConstructor( sr.ColumnNames, (object[]) sr.Rows[ 0 ] );
							typeMaps[ assemblyQualifiedName ] = actualMap;
							typeHash[ assemblyQualifiedName ] = columnComboHashCode;
						}
					}
					// skip non-derived classes for dynamic types
					if( actualMap.Type == objectMap.Type || actualMap.Type.IsSubclassOf( objectMap.Type ) )
					{
						object obj = actualMap.Construct( columnComboHashCode, row, sr.SessionBroker );
						if( obj is IEntity )
							(obj as IEntity).IsPersisted = true;
						result.Add( obj );
						// cache result if necessary
						if( isCache )
						{
							cache.Add( actualMap.GetInstanceHashKey( obj ) );
						}
					}
				}
			}
			if( isCache )
			{
				CacheManager.Insert( sr.Statement.CacheKey, cache );
			}
			return result;
		}
		public static IList<T> GetCollection<T>( SqlResult sr, IList<T> result )
		{
			Check.Verify( result == null || result is IList, Error.InvalidRequest, "The supplied generic list does not implement IList." );
			return (IList<T>) GetCollection( typeof(T), sr, (IList) (result ?? MakeGenericList<T>()) );
		}

		/// <summary>
		/// Produce an empty generic list of the specified argument type
		/// </summary>
		/// <typeparam name="T">The entity type to be stored in the list</typeparam>
		/// <returns>A generic list instance</returns>
		public static List<T> MakeGenericList<T>()
		{
			Type entityType = typeof(T);
			return MakeGenericList( entityType ) as List<T>;
		}

		/// <summary>
		/// Produce an empty generic list of the specified argument type
		/// </summary>
		/// <param name="entityType">The entity type to be stored in the list</param>
		/// <returns>A generic list instance cast to IList</returns>
		public static IList MakeGenericList( Type entityType )
		{
			Type baseListType = typeof(List<>);
            Type listType = baseListType.MakeGenericType( entityType );
			return Activator.CreateInstance( listType ) as IList;
		}

		/// <summary>
		/// Checks whether the given type is supported by the Gentle.NET framework. This method
		/// merely verifies the presence of the TableNameAttribute attribute on the class.
		/// </summary>
		/// <param name="type">The type to check</param>
		/// <returns>True if the type has the TableNameAttribute attribute</returns>
		public static bool IsTypeSupported( Type type )
		{
			return type.IsDefined( typeof(TableNameAttribute), true );
		}

		/// <summary>
		/// Register a single type. Calling this method makes Gentle create an ObjectMap
		/// for the given type. 
		/// If schema data is needed the supplied <see cref="PersistenceBroker"/>
		/// instance will be used. This method will silently ignore unsupported types and is
		/// therefore safe to call with any type.
		/// </summary>
		/// <param name="broker">The <see cref="PersistenceBroker"/> instance used to obtain
		/// metadata from the database schema.</param>
		/// <param name="type">The type for which to create an ObjectMap.</param>
		public static void RegisterType( PersistenceBroker broker, Type type )
		{
			if( IsTypeSupported( type ) )
			{
				GetMap( broker, type );
			}
		}

		/// <summary>
		/// Register multiple types. Calling this method makes Gentle create an ObjectMap 
		/// for all supported types in the assembly.
		/// If schema data is needed the supplied <see cref="PersistenceBroker"/>
		/// instance will be used.
		/// </summary>
		/// <param name="broker">The <see cref="PersistenceBroker"/> instance used to obtain
		/// metadata from the database schema.</param>
		/// <param name="assembly">The assembly whose types should be registered.</param>
		public static void RegisterAssembly( PersistenceBroker broker, Assembly assembly )
		{
			foreach( Type type in assembly.GetTypes() )
			{
				RegisterType( broker, type );
			}
		}
	}
}