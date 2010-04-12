/*
 * Base class for all rdbms backends
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: GentleProvider.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Data;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// This class implements some common methods for RDBMS backends (as defined by 
	/// the <see cref="IGentleProvider"/> interface).
	/// </summary>
	public abstract class GentleProvider : IGentleProvider
	{
		#region Members
		/// <summary>
		/// This field stores the connection string for connecting to the database engine.
		/// </summary>
		private string connectionString;
		/// <summary>
		/// This field stores the name identifying this provider.
		/// </summary>
		private string providerName;
		/// <summary>
		/// This field stores the schema name used with this provider (if any).
		/// </summary>
		private string schemaName;
		/// <summary>
		/// This field stores the PersistenceBroker used with this provider.
		/// </summary>
		private PersistenceBroker broker;
		/// <summary>
		/// Hashtable of Type-indexed hashtables of StatementType-indexed statements.
		/// </summary>
		[ThreadStatic]
		private static Hashtable stmtByType;
		#endregion

		#region Constructors
		/// <summary>
		/// This base constructor ensures that descendant must specify a connection string
		/// when new instances are created.
		/// </summary>
		/// <param name="providerName">The name identifying the provider used</param>
		/// <param name="connectionString">The connection string used to connect to the database</param>
		protected GentleProvider( string providerName, string connectionString ) :
			this( providerName, connectionString, null )
		{
		}

		/// <summary>
		/// This base constructor ensures that descendant must specify a connection string
		/// when new instances are created.
		/// </summary>
		/// <param name="providerName">The name identifying the provider used</param>
		/// <param name="connectionString">The connection string used to connect to the database</param>
		/// <param name="schemaName">The schema name used for queries made using this provider</param>
		protected GentleProvider( string providerName, string connectionString, string schemaName )
		{
			this.providerName = providerName;
			this.connectionString = connectionString;
			this.schemaName = schemaName;
		}
		#endregion

		#region IGentleProvider 
		/// <summary>
		/// Returns the name of this provider (the name identifies the type of database backend 
		/// the provider can connect to).
		/// </summary>
		public string Name
		{
			get { return providerName; }
		}

		/// <summary>
		/// Returns the connection string used by this provider instance.
		/// </summary>
		public string ConnectionString
		{
			get { return connectionString; }
		}

		/// <summary>
		/// Returns the schema name used by this provider instance, or null if no schema name is used.
		/// </summary>
		public string SchemaName
		{
			get { return schemaName; }
		}

		/// <summary>
		/// Returns the cache key of the current instance.
		/// </summary>
		public int IdentityHash
		{
			get
			{
				int result = Name.GetHashCode() + ConnectionString.GetHashCode();
				if( SchemaName != null )
				{
					result += SchemaName.GetHashCode();
				}
				return result;
			}
		}

		/// <summary>
		/// Returns the connection string used by this provider instance.
		/// </summary>
		public PersistenceBroker Broker
		{
			get
			{
				if( broker == null )
				{
					broker = new PersistenceBroker( this );
				}
				return broker;
			}
		}

		/// <summary>
		/// Override to provide information about the actual provider.
		/// Base implementation returns null.
		/// </summary>
		public virtual IProviderInformation ProviderInformation
		{
			get { return null; }
		}

		/// <summary>
		/// Abstract method declaration to obtain an <see cref="GentleSqlFactory"/> that
		/// encapsulated the RDBMS specifics for generating SQL statements.
		/// </summary>
		/// <returns>An GentleSqlFactory instance</returns>
		public abstract GentleSqlFactory GetSqlFactory();

		/// <summary>
		/// Abstract method declaration to obtain a new database analyzer instance.
		/// </summary>
		/// <returns>The new database analyzer</returns>
		public abstract GentleAnalyzer GetAnalyzer();

		/// <summary>
		/// Obtain an SQL renderer for constructing formatted SQL from SqlQuery objects.
		/// </summary>
		/// <returns>The SQL renderer for this database</returns>
		public abstract GentleRenderer GetRenderer();
		#endregion

		#region IPersistenceEngine Methods
		/// <summary>
		/// Abstract method declaration to obtain a new database connection.
		/// </summary>
		/// <returns>The new database connection</returns>
		public abstract IDbConnection GetConnection();

		/// <summary>
		/// Abstract method declaration to obtain a new <see cref="IDbCommand"/> object.
		/// </summary>
		/// <returns>The command object</returns>
		public abstract IDbCommand GetCommand();

		/// <summary>
		/// Execute the <see cref="SqlStatement"/> by providing it with a connection
		/// to the SQL engine.
		/// </summary>
		/// <param name="stmt">The SqlStatement instance</param>
		/// <returns>The result of the statement</returns>
		public virtual SqlResult ExecuteStatement( SqlStatement stmt )
		{
			Check.VerifyNotNull( stmt, Error.NullParameter, "stmt" );
			return stmt.Execute( GetConnection(), null );
		}
		#endregion

		#region Statement Cache Initialization Methods
		private void Initialize()
		{
			if( ! IsInitialized )
			{
				// create statement caches
				stmtByType = new Hashtable();
			}
		}

		public bool IsInitialized
		{
			get { return stmtByType != null; }
		}
		#endregion

		#region Statement Caching
		/// <summary>
		/// Add the specified statement to the statement cache.
		/// </summary>
		/// <param name="type">The business object with which to associate the statement</param>
		/// <param name="stmtType">The type of the SQL statement</param>
		/// <param name="stmt">The statement instance to cache</param>
		public void CacheStatement( Type type, StatementType stmtType, SqlStatement stmt )
		{
			if( GentleSettings.CacheStatements )
			{
				Initialize(); // ensure thread local variables have been initialized
				Hashtable stmts = (Hashtable) stmtByType[ type ];
				if( stmts == null ) // create a new hashtable for this class
				{
					stmts = new Hashtable();
					stmtByType[ type ] = stmts;
				}
				stmts[ stmtType ] = stmt;
			}
		}

		/// <summary>
		/// Retrieve a statement from the cache. If the statement is not present it will be generated
		/// and added to the cache.
		/// </summary>
		/// <param name="type">The business object with which the statement is associated</param>
		/// <param name="tableName">The table used in the statement</param>
		/// <param name="stmtType">The type of the SQL statement</param>
		/// <returns>An SqlStatement instance</returns>
		public SqlStatement GetStatement( Type type, string tableName, StatementType stmtType )
		{
			Initialize(); // ensure thread local variables have been initialized
			SqlStatement stmt;
			Hashtable stmts = (Hashtable) stmtByType[ type ];
			// check if an SqlStatement has been cached for the given type and StatementType
			bool isCached = stmts != null && stmts.ContainsKey( stmtType );
			if( isCached )
			{
				stmt = (SqlStatement) stmts[ stmtType ];
				// the npgsql library for postgres does not allow us to update the connection 
				// property when a transaction is active
				if( stmt.Command.Connection == null || providerName != "PostgreSQL" )
				{
					// make sure statement broker reference is updated before returning it
					stmt.SessionBroker = Broker;
					return stmt;
				}
			}
			// otherwise create and return fresh object
			PersistenceBroker broker = new PersistenceBroker( this );
			SqlBuilder sb = new SqlBuilder( broker, stmtType, type );
			// set the table name specified in the key (if null the default will be used instead)
			sb.SetTable( tableName );
			// get the statement using primary key fields as constraints
			stmt = sb.GetStatement( false );
			// dont cache statements for objects that map to multiple tables
			ObjectMap map = ObjectFactory.GetMap( broker, type );
			if( ! (map.IsDynamicTable || isCached) )
			{
				// TODO Prepare only works with a connection :-(
				// stmt.Prepare();
				CacheStatement( type, stmtType, stmt );
			}
			return stmt;
		}

		/// <summary>
		/// Clear the statement cache.
		/// </summary>
		public static void ClearCache()
		{
			stmtByType = null;
		}
		#endregion
	}
}