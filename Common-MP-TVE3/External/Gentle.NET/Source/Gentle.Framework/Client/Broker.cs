/*
 * The Broker class provides the main access point into the framework
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: Broker.cs 1234 2008-03-14 11:41:44Z mm $
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// This class is one of the main access point into the persistence framework. It exists
	/// to complement the PersistenceBroker class, which you should use if you want to access
	/// multiple databases or use multiple providers in a single project.
	/// </summary>
	/// <remarks>
	/// The broker is a simple proxy class that interacts with the default database provider
	/// through an instance of the PersistenceBroker class. This is convenient when you're only
	/// using a single database (and implied only a single provider) in your project.
	/// </remarks>
	public sealed class Broker
	{
		[ThreadStatic]
		private static PersistenceBroker _broker;

		private static PersistenceBroker broker
		{
			get
			{
				if( null == _broker )
				{
					_broker = new PersistenceBroker();
					// prevent default broker(s) from being garbage collected
					string key = String.Format( "PersistenceBroker|{0}|DefaultBroker", SystemSettings.ThreadIdentity );
					CacheManager.Insert( key, _broker, CacheStrategy.Permanent );
				}
				return _broker;
			}
		}

		// prevent instances of this class being created
		private Broker()
		{
		}

		/// <summary>
		/// Clear the current PersistenceBroker instance used by the Broker class. This is
		/// useful if you have changed the Config.DefaultProvider setting.
		/// </summary>
		public static void ClearPersistenceBroker()
		{
			_broker = null;
		}

		/// <summary>
		/// Returns the class name of the current database engine.
		/// </summary>
		public static string ProviderName
		{
			get { return broker.ProviderName; }
		}

		/// <summary>
		/// Returns the PersistenceBroker instance used by this thread.
		/// </summary>
		public static PersistenceBroker SessionBroker
		{
			get { return broker; }
		}

		/// <summary>
		/// Return the currently used IGentleProvider instance.
		/// </summary>
		/// <returns></returns>
		public static IGentleProvider Provider
		{
			get { return broker.Provider; }
		}

		/// <summary>
		/// Get a new SqlStatement instance for fully specified queries.
		/// </summary>
		/// <param name="sql">The entire SQL string to use for this statement. The statement
		/// type is determined from the SQL string. If the type cannot be determined the query 
		/// is executed using ExecuteNonQuery.</param>
		/// <returns>An SqlStatement instance</returns>
		public static SqlStatement GetStatement( string sql )
		{
			return GetStatement( StatementType.Unknown, sql );
		}

		/// <summary>
		/// Get a new SqlStatement instance for fully specified queries.
		/// </summary>
		/// <param name="sql">The entire SQL string to use for this statement</param>
		/// <param name="stmtType">The statement type. Select statements are executed using
		/// ExecuteReader. Insert, Update and Delete are executed using ExecuteNonQuery. 
		/// Count is executed using ExecuteScalar.</param>
		/// <returns>An SqlStatement instance</returns>
		public static SqlStatement GetStatement( StatementType stmtType, string sql )
		{
			return broker.GetStatement( stmtType, sql );
		}

		/// <summary>
		/// Get a new GentleSqlFactory instance for the current database engine. The SqlFactory
		/// encapsulates database specific differences and allows the <see cref="SqlBuilder"/> 
		/// to generate queries compatible with the current engine.
		/// </summary>
		/// <returns>An ISqlFactory implementation instance</returns>
		public static GentleSqlFactory GetSqlFactory()
		{
			return broker.Provider.GetSqlFactory();
		}

		/// <summary>
		/// Get a new database connection from the current persistence engine. Remember to
		/// close and release the connection when done with it.
		/// </summary>
		/// <returns>A new IDbConnection instance</returns>
		public static IDbConnection GetNewConnection()
		{
			return broker.Provider.GetConnection();
		}

		/// <summary>
		/// Retrieve data for the specified type. Throws an exception for unsupported types.
		/// </summary>
		/// <param name="type">The type of object</param>
		/// <param name="key">The key indentifying the object</param>
		/// <returns>An SqlResult instance</returns>
		public static SqlResult Retrieve( Type type, Key key )
		{
			return broker.Retrieve( type, key );
		}

		/// <summary>
		/// Refresh the properties of the given object with the values from the
		/// database. This is essentially a Retrieve operation, but instead of
		/// returning a new object, the existing is updated. This allows it to be
		/// used within constructors.
		/// Throws an exception for unsupported types.
		/// </summary>
		/// <param name="obj">The object instance to update</param>
		/// <returns>An SqlResult instance</returns>
		public static void Refresh( object obj )
		{
			broker.Refresh( obj );
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
		public static object RetrieveInstance( Type type, Key key )
		{
			IGentleProvider provider = ProviderFactory.GetProvider( type );
			return provider.Broker.RetrieveInstance( type, key );
		}

		/// <summary>
		/// Retrieve an object given its type and key. This method will raise an
		/// exception if not exactly one record matches the query. If you wish for
		/// null results to be allowed, use the <see cref="TryRetrieveInstance"/>
		/// method instead.
		/// </summary>
		/// <typeparam name="T">The type of objects to retrieve</typeparam>
		/// <param name="key">The key identifying the object</param>
		/// <returns>An object instance</returns>
		public static T RetrieveInstance<T>( Key key )
		{
			IGentleProvider provider = ProviderFactory.GetProvider( typeof(T) );
			return provider.Broker.RetrieveInstance<T>( key );
		}

		/// <summary>
		/// Retrieve an object given its type and key. This method returns null if no
		/// record matches the query.
		/// </summary>
		/// <param name="type">The type of object to create</param>
		/// <param name="key">The key of the object to retrieve</param>
		/// <returns>The created object instance or null if no records were retrieved.</returns>
		public static object TryRetrieveInstance( Type type, Key key )
		{
			IGentleProvider provider = ProviderFactory.GetProvider( type );
			return provider.Broker.TryRetrieveInstance( type, key );
		}
		/// <summary>
		/// Retrieve an object given its type and key. This method returns null if no
		/// record matches the query.
		/// </summary>
		/// <typeparam name="T">The type of objects to retrieve</typeparam>
		/// <param name="key">The key of the object to retrieve</param>
		/// <returns>The created object instance or null if no records were retrieved.</returns>
		public static T TryRetrieveInstance<T>( Key key )
		{
			IGentleProvider provider = ProviderFactory.GetProvider( typeof(T) );
			return provider.Broker.TryRetrieveInstance<T>( key );
		}

		/// <summary>
		/// Retrieve a list of objects of a given type.
		/// </summary>
		/// <param name="type">The type of objects to retrieve</param>
		/// <returns>A collection of object instances</returns>
		public static IList RetrieveList( Type type )
		{
			return RetrieveList( type, null, null );
		}

		/// <summary>
		/// Retrieve a list of objects of a given type.
		/// </summary>
		/// <typeparam name="T">The type of objects to retrieve</typeparam>
		/// <returns>A collection of object instances</returns>
		public static IList<T> RetrieveList<T>()
		{
			return RetrieveList<T>( null, null );
		}

		/// <summary>
		/// Retrieve a list of objects of a given type.
		/// </summary>
		/// <param name="type">The type of objects to retrieve</param>
		/// <param name="result">An optional existing container in which to store the created objects. If
		/// this parameter is null a new IList instance will be created.</param>
		/// <returns>A collection of object instances</returns>
		public static IList RetrieveList( Type type, IList result )
		{
			return RetrieveList( type, null, result );
		}

		/// <summary>
		/// Retrieve a list of objects of a given type.
		/// </summary>
		/// <typeparam name="T">The type of objects to retrieve</typeparam>
		/// <param name="result">An optional existing container in which to store the created objects. If
		/// this parameter is null a new IList instance will be created.</param>
		/// <returns>A collection of object instances</returns>
		public static IList<T> RetrieveList<T>( IList<T> result )
		{
			return RetrieveList<T>( null, result );
		}

		/// <summary>
		/// Retrieve a list of objects of a given type, optionally using the key as constraints.
		/// The key must contain column names and values (they will not be translated from
		/// property names to column names).
		/// </summary>
		/// <param name="type">The type of objects to retrieve</param>
		/// <param name="key">The key containing any constraints</param>
		/// <returns>A collection of object instances</returns>
		public static IList RetrieveList( Type type, Key key )
		{
			return RetrieveList( type, key, null );
		}

		/// <summary>
		/// Retrieve a list of objects of a given type, optionally using the key as constraints.
		/// The key must contain column names and values (they will not be translated from
		/// property names to column names).
		/// </summary>
		/// <typeparam name="T">The type of objects to retrieve</typeparam>
		/// <param name="key">The key containing any constraints</param>
		/// <returns>A collection of object instances</returns>
		public static IList<T> RetrieveList<T>( Key key )
		{
			return RetrieveList<T>( key, null );
		}

		/// <summary>
		/// Retrieve a list of objects of a given type, optionally using the key as constraints.
		/// The key must contain column names and values (they will not be translated from
		/// property names to column names).
		/// </summary>
		/// <param name="type">The type of objects to retrieve</param>
		/// <param name="key">The key containing any constraints</param>
		/// <param name="result">An optional existing container in which to store the created objects. If
		/// this parameter is null a new IList instance will be created.</param>
		/// <returns>A collection of object instances</returns>
		public static IList RetrieveList( Type type, Key key, IList result )
		{
			IGentleProvider provider = ProviderFactory.GetProvider( type );
			return provider.Broker.RetrieveList( type, key, result );
		}

		/// <summary>
		/// Retrieve a list of objects of a given type, optionally using the key as constraints.
		/// The key must contain column names and values (they will not be translated from
		/// property names to column names).
		/// </summary>
		/// <typeparam name="T">The type of objects to retrieve</typeparam>
		/// <param name="key">The key containing any constraints</param>
		/// <param name="result">An optional existing container in which to store the created objects. If
		/// this parameter is null a new IList instance will be created.</param>
		/// <returns>A collection of object instances</returns>
		public static IList<T> RetrieveList<T>( Key key, IList<T> result )
		{
			IGentleProvider provider = ProviderFactory.GetProvider( typeof(T) );
			return provider.Broker.RetrieveList<T>( key, result );
		}

		/// <summary>
		/// Persist (insert or update) an object. 
		/// Updates the Id property of AutoPersistent objects on insert.
		/// </summary>
		/// <param name="entity">The object to persist</param>
		public static void Persist( IEntity entity )
		{
			IGentleProvider provider = ProviderFactory.GetProvider( entity.GetType() );
			provider.Broker.Persist( entity );
		}

		/// <summary>
		/// Insert an object. This updates the identity property of objects with
		/// autogenerated primary keys.
		/// </summary>
		/// <param name="obj">The object to insert</param>
		public static void Insert( object obj )
		{
			IGentleProvider provider = ProviderFactory.GetProvider( obj.GetType() );
			provider.Broker.Insert( obj );
		}

		/// <summary>
		/// Update an existing object.
		/// </summary>
		/// <param name="obj">The object to update</param>
		public static void Update( object obj )
		{
			IGentleProvider provider = ProviderFactory.GetProvider( obj.GetType() );
			provider.Broker.Update( obj );
		}

		/// <summary>
		/// Permanently remove an object.
		/// </summary>
		/// <param name="obj">The object to persist</param>
		public static void Remove( object obj )
		{
			IGentleProvider provider = ProviderFactory.GetProvider( obj.GetType() );
			provider.Broker.Remove( obj );
		}

		/// <summary>
		/// Permanently remove an object.
		/// </summary>
		/// <param name="type">The type of object</param>
		/// <param name="key">The key indentifying the object</param>
		public static void Remove( Type type, Key key )
		{
			IGentleProvider provider = ProviderFactory.GetProvider( type );
			provider.Broker.Remove( type, key );
		}

		/// <summary>
		/// Permanently remove an object.
		/// </summary>
		/// <typeparam name="T">The type of objects to retrieve</typeparam>
		/// <param name="key">The key indentifying the object</param>
		public static void Remove<T>( Key key )
		{
			Remove( typeof(T), key  );
		}

		/// <summary>
		/// Execute a fully specified custom SQL query.
		/// </summary>
		/// <param name="sql">The fully specified SQL query to execute</param>
		/// <returns>An SqlResult instance</returns>
		public static SqlResult Execute( string sql )
		{
			return Execute( sql, null, null );
		}

		/// <summary>
		/// Execute a fully specified custom SQL query.
		/// </summary>
		/// <param name="sql">The fully specified SQL query to execute</param>
		/// <param name="conn">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous
		/// statement.</param>
		/// <returns>An SqlResult instance</returns>
		public static SqlResult Execute( string sql, IDbConnection conn )
		{
			return Execute( sql, conn, null );
		}

		/// <summary>
		/// Execute a fully specified custom SQL query.
		/// </summary>
		/// <param name="sql">The fully specified SQL query to execute</param>
		/// <param name="conn">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous
		/// statement.</param>
		/// <param name="tr">The database transaction for when participating in transactions</param>
		/// <returns>An SqlResult instance</returns>
		public static SqlResult Execute( string sql, IDbConnection conn, IDbTransaction tr )
		{
			return Execute( sql, StatementType.Unknown, null, conn, tr );
		}

		public static SqlResult Execute( string sql, Type type ) //lrb
		{
			return Execute( sql, StatementType.Unknown, type, null, null );
		}

		public static SqlResult Execute( string sql, StatementType stmtType, Type type ) //lrb
		{
			return Execute( sql, stmtType, type, null, null );
		}

		public static SqlResult Execute( string sql, StatementType stmtType, Type type, IDbConnection conn, IDbTransaction tr ) //lrb
		{
			return broker.Execute( sql, stmtType, type, conn, tr );
		}

		/// <summary>
		/// Execute a custom SQL statement.
		/// </summary>
		/// <param name="stmt">The statement to execute, wrapped in a SqlStatement object</param>
		/// <returns>An SqlResult instance</returns>
		public static SqlResult Execute( SqlStatement stmt )
		{
			return Execute( stmt, null, null );
		}

		/// <summary>
		/// Execute a custom SQL statement.
		/// </summary>
		/// <param name="stmt">The statement to execute, wrapped in a SqlStatement object</param>
		/// <param name="conn">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous
		/// statement.</param>
		/// <returns>An SqlResult instance</returns>
		public static SqlResult Execute( SqlStatement stmt, IDbConnection conn )
		{
			return Execute( stmt, conn, null );
		}

		/// <summary>
		/// Execute a custom SQL statement.
		/// </summary>
		/// <param name="stmt">The statement to execute, wrapped in a SqlStatement object</param>
		/// <param name="conn">An existing database connection to reuse. This is useful
		/// when you need to execute statements in the same session as a previous
		/// statement.</param>
		/// <param name="tr">The database transaction for when participating in transactions</param>
		/// <returns>An SqlResult instance</returns>
		public static SqlResult Execute( SqlStatement stmt, IDbConnection conn, IDbTransaction tr )
		{
			return broker.Execute( stmt, conn, tr );
		}

		/// <summary>
		/// Associate an SqlStatement with a system type (must be a descendant of Persistent).
		/// </summary>
		/// <param name="name">The name used to identify this statement</param>
		/// <param name="stmt">The SQL statement object</param>
		public static void RegisterStatement( string name, SqlStatement stmt )
		{
			broker.RegisterStatement( name, stmt );
		}

		/// <summary>
		/// Retrieve a previously registered SqlStatement.
		/// </summary>
		/// <param name="name">The name used to identify this statement</param>
		/// <returns>The previously registered SQL statement (if any)</returns>
		public static SqlStatement GetRegisteredStatement( string name )
		{
			return broker.GetRegisteredStatement( name );
		}
	}
}