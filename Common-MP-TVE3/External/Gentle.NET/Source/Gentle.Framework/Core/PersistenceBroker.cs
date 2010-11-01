/*
 * The non-static edition of the main access point into the framework
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: PersistenceBroker.cs 1234 2008-03-14 11:41:44Z mm $
 */
// the cache component

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Singleton used to interface with the persistence layer. Caches SqlStatements and
	/// uses PersistenceEngine to execute statements.
	/// </summary>
	/// <remarks>
	/// This class is responsible for interacting with the SQL engine (RDBMS) used. It handles
	/// caching of statements and retrieved objects, and uses many of the other classes in the
	/// framework to perform its duties.
	/// </remarks>
	/// <see>NamespaceDoc</see>
	public sealed class PersistenceBroker
	{
		private IGentleProvider provider;

		#region Constructors
		/// <summary>
		/// This method constructs a PersistenceBroker instance. The specified provider (database
		/// engine name) and connection string will be used to instantiate the desired backend
		/// engine used by this broker. The name will be used to uniquely distinguish this instance
		/// from other providers, and can be used to later obtain a reference to the cached instance
		/// by calling <see cref="ProviderFactory.GetNamedProvider"/>.
		/// </summary>
		/// <param name="name">The unique name to associate with this provider/broker pair.</param>
		/// <param name="providerName">The database engine name</param>
		/// <param name="connectionString">The connection string for connecting to the database</param>
		public PersistenceBroker( string name, string providerName, string connectionString )
		{
			provider = ProviderFactory.GetProvider( name, providerName, connectionString );
		}

		/// <summary>
		/// This method constructs a PersistenceBroker instance. The specified provider (database
		/// engine name) and connection string will be used to instantiate the desired backend
		/// engine used by this broker. 
		/// </summary>
		/// <param name="providerName">The database engine name</param>
		/// <param name="connectionString">The connection string for connecting to the database</param>
		public PersistenceBroker( string providerName, string connectionString )
		{
			// determine backend engine and instatiate it
			provider = ProviderFactory.GetProvider( null, providerName, connectionString );
		}

		/// <summary>
		/// Constructor using the DefaultProvider from the configuration file.
		/// </summary>
		public PersistenceBroker() : this( null, null )
		{
		}

		/// <summary>
		/// Constructor using the supplied provider (or using DefaultProvider settings if null is passed).
		/// </summary>
		public PersistenceBroker( IGentleProvider provider )
		{
			this.provider = provider != null ? provider : ProviderFactory.GetProvider( null, null );
		}

		/// <summary>
		/// Constructor selecting the provider based on the namespace of the supplied type.
		/// </summary>
		public PersistenceBroker( Type type )
		{
			provider = ProviderFactory.GetProvider( type );
		}
		#endregion

		#region Properties
		/// <summary>
		/// Returns the class name of the current database engine.
		/// </summary>
		public string ProviderName
		{
			get { return Provider != null ? provider.Name : String.Empty; }
		}

		/// <summary>
		/// Return the currently used IGentleProvider instance.
		/// </summary>
		/// <returns></returns>
		public IGentleProvider Provider
		{
			get { return provider; }
		}
		#endregion

		/// <summary>
		/// Retrieve an GentleSqlFactory instance for the database backend used.
		/// </summary>
		/// <returns></returns>
		public GentleSqlFactory GetSqlFactory()
		{
			return provider.GetSqlFactory();
		}

		#region GetStatement Methods
		private SqlStatement GetStatement( Type type, Key key, StatementType stmtType )
		{
			return provider.GetStatement( type, key.TableName, stmtType );
		}

		private SqlStatement GetStatement( object instance, StatementType stmtType )
		{
			return GetStatement( instance.GetType(), Key.GetKey( this, true, instance ), stmtType );
		}

		/// <summary>
		/// Return a new SqlStatement instance (ready for execution) using the specified 
		/// sql query string.
		/// </summary>
		/// <param name="sql">The entire SQL string to use for this statement. The statement
		/// type is determined from the SQL string. If the type cannot be determined the query 
		/// is executed using ExecuteNonQuery.</param>
		/// <returns>An SqlStatement instance prepared for execution</returns>
		public SqlStatement GetStatement( string sql )
		{
			return GetStatement( sql, StatementType.Unknown, null, 0, 0 );
		}

		/// <summary>
		/// Return a new SqlStatement instance (ready for execution) using the specified 
		/// sql query string.
		/// </summary>
		/// <param name="sql">The query string to use for the SqlStatement</param>
		/// <param name="stmtType">The statement type. Select statements are executed using
		/// ExecuteReader. Insert, Update and Delete are executed using ExecuteNonQuery. 
		/// Count is executed using ExecuteScalar.</param>
		/// <returns>An SqlStatement instance</returns>
		public SqlStatement GetStatement( StatementType stmtType, string sql )
		{
			return GetStatement( sql, stmtType, null, 0, 0 );
		}

		public SqlStatement GetStatement( string sql, Type type )
		{
			return GetStatement( sql, StatementType.Unknown, type, 0, 0 );
		}

		public SqlStatement GetStatement( string sql, StatementType stmtType, Type type )
		{
			return GetStatement( sql, stmtType, type, 0, 0 );
		}

		public SqlStatement GetStatement( string sql, StatementType stmtType, Type type, int rowLimit, int rowOffset )
		{
			SqlBuilder sb = new SqlBuilder( type != null ? ProviderFactory.GetProvider( type ) : provider,
			                                stmtType, type );
			SqlStatement stmt = null;
			sb.SetRowOffset( rowOffset );
			sb.SetRowLimit( rowLimit );
			stmt = sb.GetStatement( sql, stmtType, type );
			return stmt;
		}

		private bool IsPrimaryKeyForType( Type type, Key key )
		{
			bool result = key != null;
			result &= key.SourceType == null || key.SourceType.Equals( type );
			ObjectMap map = ObjectFactory.GetMap( this, type );
			result &= key.IsPrimaryKeyFields( map, false );
			return result;
		}

		internal Key ResolveForeignKeyReferences( Type type, Key key )
		{
			// determine whether we need to translate names
			if( key.SourceType != null && key.SourceType != type )
			{
				// translate column names in key to foreign key name in table used by type 
				Key fkKey = new Key( type, false );
				ObjectMap map = ObjectFactory.GetMap( this, key.SourceType );
				ObjectMap fkMap = ObjectFactory.GetMap( this, type );
				foreach( string name in key.Keys )
				{
					string fieldName = key.isPropertyKeys ? name : map.GetPropertyName( name );
					FieldMap fkfm = fkMap.GetForeignKeyFieldMap( map.Type, fieldName );
					fkKey[ fkfm.ColumnName ] = key[ name ];
				}
				key = fkKey;
			}
			return key;
		}

		public SqlStatement GetRetrieveListStatement( Type type, Key key )
		{
			SqlBuilder sb = new SqlBuilder( this, StatementType.Select, type );
			if( key != null )
			{
				key = ResolveForeignKeyReferences( type, key );
				// add the constraints as specified in the key
				foreach( string name in key.Keys )
				{
					sb.AddConstraint( Operator.Equals, name, key[ name ] );
				}
				// set the table name specified in the key (if null the default will be used instead)
				sb.SetTable( key.TableName );
			}
			return sb.GetStatement( true );
		}

		private SqlStatement GetRetrieveStatement( Type type, Key key )
		{
			SqlStatement stmt = null;
			// check whether we can use cached statement (key must be for primary key)
			if( IsPrimaryKeyForType( type, key ) )
			{
				// select is by primary key - expect exactly one row and complain if not found
				stmt = GetStatement( type, key, StatementType.Select );
				stmt.SetParameters( key, true );
			}
			else
			{
				stmt = GetRetrieveListStatement( type, key );
			}
			Check.VerifyNotNull( stmt, "Unable to create query for type {0} using key {1}",
			                     type.Name, key.ToString() );
			return stmt;
		}
		#endregion

		/// <summary>
		/// Retrieve data for the specified type. Throws an exception for unsupported types.
		/// </summary>
		/// <param name="type">The type of object</param>
		/// <param name="key">The key indentifying the object</param>
		/// <returns>An SqlResult containing the returned rows and helper methods</returns>
		public SqlResult Retrieve( Type type, Key key )
		{
			return Retrieve( type, key, null, null );
		}

		/// <summary>
		/// Retrieve data for the specified type. Throws an exception for unsupported types.
		/// </summary>
		/// <param name="type">The type of object</param>
		/// <param name="key">The key indentifying the object</param>
		/// <param name="conn">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous statement.</param>
		/// <param name="tr">The database transaction for when participating in transactions.</param>
		/// <returns>An SqlResult containing the returned rows and helper methods</returns>
		public SqlResult Retrieve( Type type, Key key, IDbConnection conn, IDbTransaction tr )
		{
			SqlStatement stmt = GetRetrieveStatement( type, key );
			// connections are supplied from outside when in a transaction or executing batch queries
			conn = tr != null ? tr.Connection : conn ?? stmt.SessionBroker.Provider.GetConnection();
			SqlResult sr = stmt.Execute( conn, tr );
			// require that operation succeeded and a valid result
			if( IsPrimaryKeyForType( type, key ) )
			{
				Check.Verify( sr.ErrorCode == 0, Error.NoSuchRecord, type, key, sr.Error );
				Check.Verify( sr.RowsContained == 1, Error.UnexpectedRowCount, sr.RowsContained, 1 );
			}
			return sr;
		}

		/// <summary>
		/// Refresh the supplied object instance with the values of the object identified by
		/// the key.
		/// </summary>
		/// <param name="obj">The object to update</param>
		/// <param name="key">The key of the object to retrieve</param>
		/// <param name="conn">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous statement.</param>
		/// <param name="tr">The database transaction for when participating in transactions.</param>
		internal void Retrieve( object obj, Key key, IDbConnection conn, IDbTransaction tr )
		{
			Check.VerifyNotNull( obj, Error.NullParameter, "obj" );
			SqlResult sr = Retrieve( obj.GetType(), key, conn, tr );
			// update an existing object instance
			ObjectMap om = ObjectFactory.GetMap( this, obj.GetType() );
			om.SetProperties( obj, sr.ColumnNames, (object[]) sr.Rows[ 0 ] );
			if( obj is IEntity )
			{
				(obj as IEntity).IsPersisted = true;
			}
		}

		/// <summary>
		/// Refresh the properties of the given object with the values from the
		/// database. This is essentially a Retrieve operation, but instead of
		/// returning a new object, the existing is updated. This allows it to be
		/// used within constructors.
		/// Throws an exception for unsupported types.
		/// </summary>
		/// <param name="obj">The object instance to update</param>
		/// <param name="tr">The database transaction for when participating in transactions.</param>
		public void Refresh( object obj, IDbTransaction tr )
		{
			Retrieve( obj, Key.GetKey( this, true, obj ), tr.Connection, tr );
		}

		/// <summary>
		/// Refresh the properties of the given object with the values from the
		/// database. This is essentially a Retrieve operation, but instead of
		/// returning a new object, the existing is updated. This allows it to be
		/// used within constructors.
		/// Throws an exception for unsupported types.
		/// </summary>
		/// <param name="obj">The object instance to update</param>
		public void Refresh( object obj )
		{
			Retrieve( obj, Key.GetKey( this, true, obj ), null, null );
		}

		/// <summary>
		/// Refresh the supplied object instance with the values in the database.
		/// </summary>
		/// <param name="p">The object to update</param>
		/// <returns>The result of the operation</returns>
		public void Refresh( IEntity p )
		{
			Retrieve( p, p.GetKey(), null, null );
		}

		/// <summary>
		/// Retrieve an object given its type and key. This method will raise an
		/// exception if not exactly one record matches the query. If you wish for
		/// null results to be allowed, use the <see cref="TryRetrieveInstance"/>
		/// method instead.
		/// </summary>
		/// <param name="type">The type of object to retrieve</param>
		/// <param name="key">The key identifying the object</param>
		/// <returns>An object instance</returns>
		public object RetrieveInstance( Type type, Key key )
		{
			return RetrieveInstance( type, key, null, null );
		}

		/// <summary>
		/// Retrieve an object given its type and key. This method will raise an
		/// exception if not exactly one record matches the query. If you wish for
		/// null results to be allowed, use the <see cref="TryRetrieveInstance"/>
		/// method instead.
		/// </summary>
		/// <typeparam name="T">The type of object to retrieve</typeparam>
		/// <param name="key">The key identifying the object</param>
		/// <returns>An object instance</returns>
		public T RetrieveInstance<T>( Key key ) 
		{
			return (T) RetrieveInstance( typeof(T), key );
		}

		/// <summary>
		/// Retrieve an object given its type and key. This method returns null if no
		/// record matches the query.
		/// </summary>
		/// <param name="type">The type of object to create</param>
		/// <param name="key">The key of the object to retrieve</param>
		/// <returns>The created object instance or null if no records were retrieved.</returns>
		public object TryRetrieveInstance( Type type, Key key )
		{
			IList list = RetrieveList( type, key );
			return list.Count > 0 ? list[ 0 ] : null;
		}

		/// <summary>
		/// Retrieve an object given its type and key. This method returns null if no
		/// record matches the query.
		/// </summary>
		/// <typeparam name="T">The type of object to retrieve</typeparam>
		/// <param name="key">The key of the object to retrieve</param>
		/// <returns>The created object instance or null if no records were retrieved.</returns>
		public T TryRetrieveInstance<T>( Key key )
		{
			return (T) TryRetrieveInstance( typeof(T), key );
		}

		private static IList<T> GetCacheResult<T>( string statementHashKey )
		{
			if( GentleSettings.CacheObjects && GentleSettings.SkipQueryExecution )
			{
				IList cache = (IList) CacheManager.Get( statementHashKey );
				if( cache != null )
				{
					IList<T> result = ObjectFactory.MakeGenericList<T>();
					foreach( string instanceHashKey in cache )
					{
						object obj = CacheManager.Get( instanceHashKey );
						if( obj != null && obj is T )
						{
							result.Add( (T) obj );
						}
					}
					if( result.Count == cache.Count )
					{
						GentleStatistics.SkippedQueries += 1;
						return result;
					}
				}
			}
			return null;
		}

		private static IList GetCacheResult( string statementHashKey )
		{
			if( GentleSettings.CacheObjects && GentleSettings.SkipQueryExecution )
			{
				IList cache = (IList) CacheManager.Get( statementHashKey );
				if( cache != null )
				{
					IList result = new ArrayList();
					foreach( string instanceHashKey in cache )
					{
						object obj = CacheManager.Get( instanceHashKey );
						if( obj != null )
						{
							result.Add( obj );
						}
					}
					if( result.Count == cache.Count )
					{
						GentleStatistics.SkippedQueries += 1;
						return result;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Retrieve an instance of the given type and identified by the given key using the
		/// supplied connection and transaction.
		/// </summary>
		/// <param name="type">The type of object to create</param>
		/// <param name="key">The key of the object to retrieve</param>
		/// <param name="conn">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous statement.</param>
		/// <param name="tr">The database transaction for when participating in transactions.</param>
		/// <returns>The created object instance</returns>
		public object RetrieveInstance( Type type, Key key, IDbConnection conn, IDbTransaction tr )
		{
			SqlStatement stmt = GetRetrieveStatement( type, key );
			// check cache before execution if needed
			if( GentleSettings.CacheObjects && ObjectFactory.GetMap( this, type ).CacheStrategy != CacheStrategy.Never )
			{
				IList cache = GetCacheResult( stmt.CacheKey );
				if( cache != null && cache.Count == 1 )
				{
					return cache[ 0 ];
				}
			}
			// no cache result, proceed as normal
			conn = tr != null ? tr.Connection : conn ?? stmt.SessionBroker.Provider.GetConnection();
			SqlResult sr = stmt.Execute( conn, tr );
			if( sr.ErrorCode == 0 )
			{
				return ObjectFactory.GetInstance( type, sr, key );
			}
			Check.Fail( Error.NoSuchRecord, type, key, sr.Error );
			return null;
		}

		/// <summary>
		/// Retrieve an instance of the given type and identified by the given key using the
		/// supplied connection and transaction.
		/// </summary>
		/// <typeparam name="T">The type of object to retrieve</typeparam>
		/// <param name="key">The key of the object to retrieve</param>
		/// <param name="conn">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous statement.</param>
		/// <param name="tr">The database transaction for when participating in transactions.</param>
		/// <returns>The created object instance</returns>
		public T RetrieveInstance<T>( Key key, IDbConnection conn, IDbTransaction tr ) 
		{
			return (T) RetrieveInstance( typeof(T), key, conn, tr );
		}

		#region List Retrieval Methods
		/// <summary>
		/// Retrieve a list of objects of a given type.
		/// </summary>
		/// <param name="type">The type of objects to retrieve</param>
		/// <returns>A collection of object instances</returns>
		public IList RetrieveList( Type type )
		{
			return RetrieveList( type, null, null, null, null );
		}

		/// <summary>
		/// Retrieve a list of objects of a given type.
		/// </summary>
		/// <typeparam name="T">The type of object to retrieve</typeparam>
		/// <returns>A collection of object instances</returns>
		public IList<T> RetrieveList<T>()
		{
			return RetrieveList<T>( null, null, null, null );
		}

		/// <summary>
		/// Retrieve a list of objects of a given type.
		/// </summary>
		/// <param name="type">The type of objects to retrieve</param>
		/// <param name="result">An optional existing container in which to store the created objects. If
		/// this parameter is null a new IList instance will be created.</param>
		/// <returns>A collection of object instances</returns>
		public IList RetrieveList( Type type, IList result )
		{
			return RetrieveList( type, null, result, null, null );
		}

		/// <summary>
		/// Retrieve a list of objects of a given type.
		/// </summary>
		/// <typeparam name="T">The type of object to retrieve</typeparam>
		/// <param name="result">An optional existing container in which to store the created objects. If
		/// this parameter is null a new IList instance will be created.</param>
		/// <returns>A collection of object instances</returns>
		public IList<T> RetrieveList<T>( IList<T> result )
		{
			return RetrieveList<T>( null, result, null, null );
		}

		/// <summary>
		/// Retrieve a list of objects of a given type, optionally using the key as constraints.
		/// The key must contain column names and values (they will not be translated from
		/// property names to column names).
		/// </summary>
		/// <param name="type">The type of objects to retrieve</param>
		/// <param name="key">The key containing any constraints</param>
		/// <returns>A collection of object instances</returns>
		public IList RetrieveList( Type type, Key key )
		{
			return RetrieveList( type, key, null );
		}

		/// <summary>
		/// Retrieve a list of objects of a given type, optionally using the key as constraints.
		/// The key must contain column names and values (they will not be translated from
		/// property names to column names).
		/// </summary>
		/// <typeparam name="T">The type of object to retrieve</typeparam>
		/// <param name="key">The key containing any constraints</param>
		/// <returns>A collection of object instances</returns>
		public IList<T> RetrieveList<T>( Key key )
		{
			return RetrieveList<T>( key, null, null, null );
		}

		/// <summary>
		/// Retrieve multiple instances of the given type. The retrieved rows are limited by the fields
		/// and values specified in the given <see cref="Key"/> instance.
		/// </summary>
		/// <param name="type">The type of objects to create</param>
		/// <param name="key">The key of the objects to retrieve</param>
		/// <param name="result">An optional existing container in which to store the created objects. If
		/// this parameter is null a new IList instance will be created.</param>
		/// <returns>An array containing the created object instances</returns>
		public IList RetrieveList( Type type, Key key, IList result )
		{
			return RetrieveList( type, key, result, null, null );
		}

		/// <summary>
		/// Retrieve multiple instances of the given type. The retrieved rows are limited by the fields
		/// and values specified in the given <see cref="Key"/> instance.
		/// </summary>
		/// <typeparam name="T">The type of objects to create</typeparam>
		/// <param name="key">The key of the objects to retrieve</param>
		/// <param name="result">An optional existing container in which to store the created objects. If
		/// this parameter is null a new IList instance will be created.</param>
		/// <returns>An array containing the created object instances</returns>
		public IList<T> RetrieveList<T>(Key key, IList<T> result )
		{
			return RetrieveList<T>( key, result, null, null );
		}

		/// <summary>
		/// Retrieve multiple instances of the given type. The retrieved rows are limited by the fields
		/// and values specified in the given <see cref="Key"/> instance.
		/// </summary>
		/// <param name="type">The type of objects to create</param>
		/// <param name="key">The key of the objects to retrieve</param>
		/// <param name="result">An optional existing container in which to store the created objects. If
		/// this parameter is null a new IList instance will be created.</param>
		/// <param name="conn">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous statement.</param>
		/// <param name="tr">The database transaction for when participating in transactions.</param>
		/// <returns>An array containing the created object instances</returns>
		public IList RetrieveList( Type type, Key key, IList result, IDbConnection conn, IDbTransaction tr )
		{
			SqlResult sr;
			SqlStatement stmt = GetRetrieveListStatement( type, key );
			// check cache before execution
			IList cache = GetCacheResult( stmt.CacheKey );
			if( cache != null )
			{
				if( result != null )
				{
					return ListCopy( cache, result );
				}
				return cache;
			}
			// no cache result, proceed as normal
			conn = tr != null ? tr.Connection : conn ?? stmt.SessionBroker.Provider.GetConnection();
			sr = stmt.Execute( conn, tr );
			if( sr.ErrorCode == 0 )
			{
				return ObjectFactory.GetCollection( type, sr, result );
			}
			Check.Fail( Error.StatementError, "Unable to retrieve list of objects", sr.Statement.Sql );
			return null;
		}

		/// <summary>
		/// Retrieve multiple instances of the given type. The retrieved rows are limited by the fields
		/// and values specified in the given <see cref="Key"/> instance.
		/// </summary>
		/// <typeparam name="T">The type of objects to retrieve</typeparam>
		/// <param name="key">The key of the objects to retrieve</param>
		/// <param name="result">An optional existing container in which to store the created objects. If
		/// this parameter is null a new IList instance will be created.</param>
		/// <param name="conn">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous statement.</param>
		/// <param name="tr">The database transaction for when participating in transactions.</param>
		/// <returns>An array containing the created object instances</returns>
		public IList<T> RetrieveList<T>( Key key, IList<T> result, IDbConnection conn, IDbTransaction tr )
		{
			SqlResult sr;
			SqlStatement stmt = GetRetrieveListStatement( typeof(T), key );
			// check cache before execution
			IList<T> cache = GetCacheResult<T>( stmt.CacheKey );
			if( cache != null )
			{
				if( result != null )
				{
					return ListCopy<T>( cache, result );
				}
				return cache;
			}
			// no cache result, proceed as normal
			conn = tr != null ? tr.Connection : conn ?? stmt.SessionBroker.Provider.GetConnection();
			sr = stmt.Execute( conn, tr );
			if( sr.ErrorCode == 0 )
			{
				return ObjectFactory.GetCollection<T>( sr, result );
			}
			Check.Fail( Error.StatementError, "Unable to retrieve list of objects", sr.Statement.Sql );
			return null;
		}

		private static IList ListCopy( IList source, IList target )
		{
			if( target is ArrayList )
			{
				(target as ArrayList).AddRange( source );
			}
			else
			{
				foreach( object obj in source )
				{
					target.Add( obj );
				}
			}
			return target;
		}

		private static IList<T> ListCopy<T>( IList<T> source, IList<T> target )
		{
			if( target is List<T> )
			{
				(target as List<T>).AddRange( source );
			}
			else
			{
				foreach( T obj in source )
				{
					target.Add( obj );
				}
			}
			return target;
		}

		/// <summary>
		/// Retrieve multiple instances of the given type. The retrieved rows are limited by the fields
		/// and values specified in the given <see cref="Key"/> instance.
		/// </summary>
		/// <param name="type">The type of objects to create</param>
		/// <param name="key">The key of the objects to retrieve</param>
		/// <returns>An SqlResult with the returned rows</returns>
		internal SqlResult RetrieveListRaw( Type type, Key key )
		{
			return RetrieveListRaw( type, key, null, null );
		}

		/// <summary>
		/// Retrieve multiple instances of the given type. The retrieved rows are limited by the fields
		/// and values specified in the given <see cref="Key"/> instance.
		/// </summary>
		/// <param name="type">The type of objects to create</param>
		/// <param name="key">The key of the objects to retrieve</param>
		/// <param name="conn">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous statement.</param>
		/// <param name="tr">The database transaction for when participating in transactions.</param>
		/// <returns>An SqlResult with the returned rows</returns>
		internal SqlResult RetrieveListRaw( Type type, Key key, IDbConnection conn, IDbTransaction tr )
		{
			return Retrieve( type, key, conn, tr );
		}
		#endregion

		/// <summary>
		/// Internal helper method used for standard CRUD operations on known types.
		/// </summary>
		/// <param name="obj">The object instance being operated on</param>
		/// <param name="st">The statement type</param>
		/// <param name="conn">The database connection for when participating in transactions</param>
		/// <returns>The result of the operation</returns>
		internal SqlResult Execute( object obj, StatementType st, IDbConnection conn )
		{
			return Execute( obj, st, conn, null );
		}

		/// <summary>
		/// Internal helper method used for standard CRUD operations on known types.
		/// </summary>
		/// <param name="obj">The object instance being operated on</param>
		/// <param name="st">The statement type</param>
		/// <param name="conn">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous statement.</param>
		/// <param name="tr">The database transaction for when participating in transactions.</param>
		/// <returns>The result of the operation</returns>
		internal SqlResult Execute( object obj, StatementType st, IDbConnection conn, IDbTransaction tr )
		{
			ObjectMap map = ObjectFactory.GetMap( this, obj );

			// perform validations first
			if( StatementType.Insert == st || StatementType.Update == st )
			{
				ValidationBroker.Validate( obj );
			}

			if( map.IsSoftDelete && st == StatementType.Delete )
			{
				st = StatementType.SoftDelete;
			}
			SqlStatement stmt = GetStatement( obj, st );
			stmt.SetParameters( obj, true );
			// connections are supplied from outside when in a transaction or executing batch queries
			conn = tr != null ? tr.Connection : conn ?? stmt.SessionBroker.Provider.GetConnection();
			SqlResult sr = stmt.Execute( conn, tr );
			// throw an error if execution failed
			Check.Verify( sr.ErrorCode == 0, Error.StatementError, sr.Error, stmt.Sql );
			// check that statement affected a row
			if( st == StatementType.Insert || st == StatementType.Update ||
			    st == StatementType.Delete || st == StatementType.SoftDelete )
			{
				Check.Verify( sr.RowsAffected >= 1, Error.UnexpectedRowCount, sr.RowsAffected, "1+" );
			}
			// update identity values for inserts
			if( st == StatementType.Insert )
			{
				if( sr.LastRowId != 0 && map.IdentityMap != null )
				{
					map.SetIdentity( obj, sr.LastRowId );
				}
				if( obj is IEntity )
				{
					(obj as IEntity).IsPersisted = true;
				}
				// this is not really necessary but nice for error checking (also used in test cases)
				if( map.InheritanceMap != null )
				{
					map.InheritanceMap.SetValue( obj, obj.GetType().AssemblyQualifiedName );
				}
			}
			// update/invalidate the cache
			if( GentleSettings.CacheObjects && map.CacheStrategy != CacheStrategy.Never )
			{
				if( st == StatementType.Insert )
				{
					// insert new object into cache
					CacheManager.Insert( map.GetInstanceHashKey( obj ), obj, map.CacheStrategy );
					// invalidate query results for select statements for this type, reducing the
					// effectiveness of the cache (but ensuring correct results)
					if( GentleSettings.CacheStatements )
					{
						CacheManager.ClearQueryResultsByType( map.Type );
					}
				}
				else if( st == StatementType.Delete || st == StatementType.SoftDelete )
				{
					// remove invalidated object from the cache
					CacheManager.Remove( obj );
					// invalidate query results for select statements for this type, reducing the
					// effectiveness of the cache (but ensuring correct results)
					if( GentleSettings.CacheStatements )
					{
						CacheManager.ClearQueryResultsByType( map.Type );
					}
				}
			}
			// update the in-memory version/revision counter for objects under concurrency control
			if( st == StatementType.Update && GentleSettings.ConcurrencyControl )
			{
				if( map.ConcurrencyMap != null )
				{
					FieldMap fm = map.ConcurrencyMap;
					long version = Convert.ToInt64( fm.GetValue( obj ) );
					// handle wrap-around of the version counter
					if( (fm.Type.Equals( typeof(int) ) && version == int.MaxValue) ||
					    (fm.Type.Equals( typeof(long) ) && version == long.MaxValue) )
					{
						version = 1;
					}
					else
					{
						version += 1;
					}
					map.ConcurrencyMap.SetValue( obj, version );
				}
			}
			// update object with database-created values if UpdateAfterWrite is set to true
			if( map.IsUpdateAfterWrite && (st == StatementType.Insert || st == StatementType.Update) )
			{
				if( tr != null )
				{
					Refresh( obj, tr );
				}
				else
				{
					Refresh( obj );
				}
			}
			return sr;
		}

		/// <summary>
		/// Persist (insert or update) an object. Updates the property decorated with the 
		/// PrimaryKey field if the AutoGenerated property has been set to true.
		/// </summary>
		/// <param name="obj">The object to persist</param>
		public void Persist( IEntity obj )
		{
			Execute( obj, obj.IsPersisted ? StatementType.Update : StatementType.Insert, null, null );
		}

		/// <summary>
		/// Persist (insert or update) an object. Updates the property decorated with the 
		/// PrimaryKey field if the AutoGenerated property has been set to true.
		/// </summary>
		/// <param name="obj">The object to persist</param>
		/// <param name="dbTransaction">The database transaction for when participating in transactions</param>
		public void Persist( IEntity obj, IDbTransaction dbTransaction )
		{
			Execute( obj, obj.IsPersisted ? StatementType.Update : StatementType.Insert,
			         dbTransaction.Connection, dbTransaction );
		}

		/// <summary>
		/// Insert an object. Updates the property decorated with the PrimaryKey field if 
		/// the AutoGenerated property has been set to true.
		/// </summary>
		/// <param name="obj">The object to insert</param>
		public void Insert( object obj )
		{
			Execute( obj, StatementType.Insert, null, null );
		}

		/// <summary>
		/// Insert an object. Updates the property decorated with the PrimaryKey field if 
		/// the AutoGenerated property has been set to true.
		/// </summary>
		/// <param name="obj">The object to insert</param>
		/// <param name="dbTransaction">The database transaction for when participating in transactions</param>
		public void Insert( object obj, IDbTransaction dbTransaction )
		{
			Execute( obj, StatementType.Insert, null, dbTransaction );
		}

		/// <summary>
		/// Update an existing object.
		/// </summary>
		/// <param name="obj">The object to update</param>
		public void Update( object obj )
		{
			Execute( obj, StatementType.Update, null, null );
		}

		/// <summary>
		/// Update an existing object.
		/// </summary>
		/// <param name="obj">The object to update</param>
		/// <param name="dbTransaction">The database transaction for when participating in transactions</param>
		public void Update( object obj, IDbTransaction dbTransaction )
		{
			Execute( obj, StatementType.Update, null, dbTransaction );
		}

		/// <summary>
		/// Permanently remove an object.
		/// </summary>
		/// <param name="obj">The object to persist</param>
		public void Remove( object obj )
		{
			Remove( obj, null );
		}

		/// <summary>
		/// Permanently remove an object.
		/// </summary>
		/// <param name="obj">The object to persist</param>
		/// <param name="dbTransaction">The database transaction for when participating in transactions</param>
		public void Remove( object obj, IDbTransaction dbTransaction )
		{
			SqlResult sr = Execute( obj, StatementType.Delete, null, dbTransaction );
			if( sr.ErrorCode == 0 )
			{
				// clear the identity column if any
				ObjectMap map = ObjectFactory.GetMap( this, obj );
				if( map.IdentityMap != null )
				{
					map.SetIdentity( obj, 0 );
				}
				// make sure the object does not think it is persisted
				if( obj is IEntity )
				{
					(obj as IEntity).IsPersisted = false;
				}
			}
		}

		/// <summary>
		/// Permanently remove an object.
		/// </summary>
		/// <param name="type">The type of object</param>
		/// <param name="key">The key indentifying the object</param>
		public SqlResult Remove( Type type, Key key )
		{
			return Remove( type, key, null );
		}

		/// <summary>
		/// Permanently remove an object.
		/// </summary>
		/// <typeparam name="T">The type of object</typeparam>
		/// <param name="key">The key indentifying the object</param>
		public SqlResult Remove<T>( Key key )
		{
			return Remove<T>( key, null );
		}

		/// <summary>
		/// Permanently remove an object.
		/// </summary>
		/// <param name="type">The type of object</param>
		/// <param name="key">The key indentifying the object</param>
		/// <param name="tr">The database transaction for when participating in transactions</param>
		public SqlResult Remove( Type type, Key key, IDbTransaction tr )
		{
			SqlStatement stmt;
			ObjectMap map = ObjectFactory.GetMap( this, type );
			if( map.IsSoftDelete && key.IsPrimaryKeyFields( map, false ) )
			{
				key[ map.ConcurrencyMap ] = -1;
				stmt = GetStatement( type, key, StatementType.SoftDelete );
			}
			else
			{
				stmt = GetStatement( type, key, StatementType.Delete );
			}
			stmt.SetParameters( key, true );
			IDbConnection conn = tr != null ? tr.Connection : stmt.SessionBroker.Provider.GetConnection();
			return stmt.Execute( conn, tr );
		}

		/// <summary>
		/// Permanently remove an object.
		/// </summary>
		/// <typeparam name="T">The type of object</typeparam>
		/// <param name="key">The key indentifying the object</param>
		/// <param name="tr">The database transaction for when participating in transactions</param>
		public SqlResult Remove<T>( Key key, IDbTransaction tr )
		{
			return Remove( typeof(T), key, tr );
		}

		/// <summary>
		/// Execute a fully specified custom SQL query.
		/// </summary>
		/// <param name="sql">The fully specified SQL query to execute</param>
		/// <param name="stmtType">The statement type (used to determine the query execution mode)</param>
		/// <param name="type">The type associated with this statement</param>
		/// <param name="dbConnection">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous
		/// statement.</param>
		/// <param name="dbTransaction">The database transaction for when participating in transactions</param>
		/// <returns>An SqlResult instance</returns>
		public SqlResult Execute( string sql, StatementType stmtType, Type type, IDbConnection dbConnection, IDbTransaction dbTransaction )
		{
			SqlStatement stmt = GetStatement( sql, stmtType, type, 0, 0 );
			return Execute( stmt, dbConnection, dbTransaction );
		}

		/// <summary>
		/// Execute a fully specified custom SQL query.
		/// </summary>
		/// <param name="sql">The fully specified SQL query to execute</param>
		/// <param name="dbConnection">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous
		/// statement.</param>
		/// <param name="dbTransaction">The database transaction for when participating in transactions</param>
		/// <returns>An SqlResult instance</returns>
		public SqlResult Execute( string sql, IDbConnection dbConnection, IDbTransaction dbTransaction )
		{
			return Execute( sql, StatementType.Unknown, null, dbConnection, dbTransaction );
		}

		/// <summary>
		/// Execute a fully specified custom SQL query.
		/// </summary>
		/// <param name="sql">The fully specified SQL query to execute</param>
		/// <param name="dbTransaction">The database transaction for when participating in transactions</param>
		/// <returns>An SqlResult instance</returns>
		public SqlResult Execute( string sql, IDbTransaction dbTransaction )
		{
			return Execute( sql, StatementType.Unknown, null, null, dbTransaction );
		}

		/// <summary>
		/// Execute a fully specified custom SQL query.
		/// </summary>
		/// <param name="sql">The fully specified SQL query to execute</param>
		/// <returns>An SqlResult instance</returns>
		public SqlResult Execute( string sql )
		{
			return Execute( sql, StatementType.Unknown, null, null, null );
		}

		/// <summary>
		/// Execute a custom SQL statement.
		/// </summary>
		/// <param name="stmt">The statement to execute, wrapped in a SqlStatement object</param>
		/// <returns>An SqlResult instance</returns>
		public SqlResult Execute( SqlStatement stmt )
		{
			return stmt.Execute();
		}

		/// <summary>
		/// Execute a custom SQL statement.
		/// </summary>
		/// <param name="stmt">The statement to execute, wrapped in a SqlStatement object</param>
		/// <param name="dbConnection">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous
		/// statement.</param>
		/// <param name="dbTransaction">The database transaction for when participating in transactions</param>
		/// <returns>An SqlResult instance</returns>
		public SqlResult Execute( SqlStatement stmt, IDbConnection dbConnection, IDbTransaction dbTransaction )
		{
			if( dbTransaction != null )
			{
				return stmt.Execute( dbTransaction.Connection, dbTransaction );
			}
			else
			{
				return stmt.Execute( dbConnection != null ? dbConnection : stmt.SessionBroker.Provider.GetConnection(), null );
			}
		}

		/// <summary>
		/// Execute a custom SQL statement.
		/// </summary>
		/// <param name="stmt">The statement to execute, wrapped in a SqlStatement object</param>
		/// <param name="dbTransaction">The database transaction for when participating in transactions</param>
		/// <returns>An SqlResult instance</returns>
		public SqlResult Execute( SqlStatement stmt, IDbTransaction dbTransaction )
		{
			return stmt.Execute( dbTransaction.Connection, dbTransaction );
		}

		/// <summary>
		/// Associate an SqlStatement with a system type (must be a descendant of Persistent).
		/// </summary>
		/// <param name="name">The name used to identify this statement</param>
		/// <param name="stmt">The SQL statement object</param>
		public void RegisterStatement( string name, SqlStatement stmt )
		{
			CacheManager.Insert( name, stmt, CacheStrategy.Permanent );
		}

		/// <summary>
		/// Retrieve a previously registered SqlStatement.
		/// </summary>
		/// <param name="name">The name used to identify this statement</param>
		/// <returns>The previously registered SQL statement (if any)</returns>
		public SqlStatement GetRegisteredStatement( string name )
		{
			return CacheManager.Get( name ) as SqlStatement;
		}
	}
}