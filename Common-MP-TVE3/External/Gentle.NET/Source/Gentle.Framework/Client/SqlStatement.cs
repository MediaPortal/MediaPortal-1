/*
 * Encapsulation of an SQL query (regardless of type)
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: SqlStatement.cs 1234 2008-03-14 11:41:44Z mm $
 */

using System;
using System.Data;
using System.IO;
using System.Text;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Encapsulation of a database-specific SQL statement.
	/// </summary>
	public class SqlStatement : BrokerLock, ICacheKeyProvider
	{
		private IDbCommand cmd; // the database command object
		private StatementType statementType; // type of statement (select, insert, etc.)
		private ObjectMap map; // worker object for mapping between the database and objects
		private int rowLimit; // the maximum number of rows returned by this statement
		private int rowOffset; // the number of skipped rows 

		/// <summary>
		/// Determine the statement type of the supplied SQL statement by checking the first
		/// 3 characters of the string.
		/// </summary>
		/// <param name="sql">The SQL statement</param>
		/// <returns>The <see cref="StatementType"/> of the statement</returns>
		protected static StatementType GetStatementType( string sql )
		{
			Check.VerifyNotNull( sql, "Unable to determine type of null statement." );
			Check.Verify( sql.Length > 4, "Unable to determine type of invalid statements (too short)." );
			switch( sql.Substring( 0, 3 ).Trim().ToLower() )
			{
				case "sel":
					return StatementType.Select;
				case "ins":
					return StatementType.Insert;
				case "upd":
					return StatementType.Update;
				case "del":
					return StatementType.Delete;
				default:
					return StatementType.Unknown;
			}
		}

		/// <summary>
		/// Construct a new SqlStatement instance of the specified type. The command object to be used
		/// for executing the query and the fully specified sql query string must be specified.  
		/// </summary>
		/// <param name="stmtType">The type of this statement.</param>
		/// <param name="cmd">The command object to use when executing the query.</param>
		/// <param name="sql">The fully specified sql query string.</param>
		public SqlStatement( StatementType stmtType, IDbCommand cmd, string sql ) :
			this( stmtType, cmd, sql, null, 0, 0 )
		{
		}

		/// <summary>
		/// Construct a new SqlStatement instance of the specified type. The command object to be used
		/// for executing the query and the fully specified sql query string must be specified.  
		/// </summary>
		/// <param name="broker">The PersistenceBroker instance to use for database access.</param>
		/// <param name="stmtType">The type of this statement.</param>
		/// <param name="cmd">The command object to use when executing the query.</param>
		/// <param name="sql">The fully specified sql query string.</param>
		public SqlStatement( PersistenceBroker broker, StatementType stmtType, IDbCommand cmd, string sql ) :
			this( broker, stmtType, cmd, sql, null, 0, 0 )
		{
		}

		/// <summary>
		/// Construct a new SqlStatement instance of the specified type. The command object to be used
		/// for executing the query and the fully specified sql query string must be specified.  
		/// </summary>
		/// <param name="stmtType">The type of this statement.</param>
		/// <param name="cmd">The command object to use when executing the query.</param>
		/// <param name="sql">The fully specified sql query string.</param>
		/// <param name="rowLimit">The maximum number of rows to be returned by this query 
		/// or 0 for no limit.</param>
		/// <param name="rowOffset">The number of rows to be skipped by this statement. Note that
		/// for SQL Server this is applied after execution, whereas for other databases it is 
		/// embedded in the SQL string.</param>
		public SqlStatement( StatementType stmtType, IDbCommand cmd, string sql, int rowLimit, int rowOffset ) :
			this( null, stmtType, cmd, sql, null, rowLimit, rowOffset )
		{
		}

		/// <summary>
		/// Construct a new SqlStatement instance of the specified type. The command object to be used
		/// for executing the query and the fully specified sql query string must be specified.  
		/// </summary>
		/// <param name="broker">The PersistenceBroker instance to use for database access.</param>
		/// <param name="stmtType">The type of this statement.</param>
		/// <param name="cmd">The command object to use when executing the query.</param>
		/// <param name="sql">The fully specified sql query string.</param>
		/// <param name="rowLimit">The maximum number of rows to be returned by this query 
		/// or 0 for no limit.</param>
		/// <param name="rowOffset">The number of rows to be skipped by this statement. Note that
		/// for SQL Server this is applied after execution, whereas for other databases it is 
		/// embedded in the SQL string.</param>
		public SqlStatement( PersistenceBroker broker, StatementType stmtType, IDbCommand cmd,
		                     string sql, int rowLimit, int rowOffset ) :
		                     	this( broker, stmtType, cmd, sql, null, rowLimit, rowOffset )
		{
		}

		/// <summary>
		/// Construct a new SqlStatement instance of the specified type. The command object to be used
		/// for executing the query and the fully specified sql query string must be specified.  
		/// </summary>
		/// <param name="stmtType">The type of this statement.</param>
		/// <param name="cmd">The command object to use when executing the query.</param>
		/// <param name="sql">The fully specified sql query string.</param>
		/// <param name="type">The type of object being selected by this query or null if 
		/// not applicable.</param>
		public SqlStatement( StatementType stmtType, IDbCommand cmd, string sql, Type type ) :
			this( null, stmtType, cmd, sql, type, 0, 0 )
		{
		}

		/// <summary>
		/// Construct a new SqlStatement instance of the specified type. The command object to be used
		/// for executing the query and the fully specified sql query string must be specified.  
		/// </summary>
		/// <param name="broker">The PersistenceBroker instance to use for database access.</param>
		/// <param name="stmtType">The type of this statement.</param>
		/// <param name="cmd">The command object to use when executing the query.</param>
		/// <param name="sql">The fully specified sql query string.</param>
		/// <param name="type">The type of object being selected by this query or null if 
		/// not applicable.</param>
		public SqlStatement( PersistenceBroker broker, StatementType stmtType, IDbCommand cmd,
		                     string sql, Type type ) : this( broker, stmtType, cmd, sql, type, 0, 0 )
		{
		}

		/// <summary>
		/// Construct a new SqlStatement instance of the specified type. The command object to be used
		/// for executing the query and the fully specified sql query string must be specified.  
		/// </summary>
		/// <param name="stmtType">The type of this statement.</param>
		/// <param name="cmd">The command object to use when executing the query.</param>
		/// <param name="sql">The fully specified sql query string.</param>
		/// <param name="type">The type of object being selected by this query or null if 
		/// not applicable.</param>
		/// <param name="rowLimit">The maximum number of rows to be returned by this query 
		/// or 0 for no limit.</param>
		/// <param name="rowOffset">The number of rows to be skipped by this statement. Note that
		/// for SQL Server this is applied after execution, whereas for other databases it is 
		/// embedded in the SQL string.</param>
		public SqlStatement( StatementType stmtType, IDbCommand cmd, string sql, Type type,
		                     int rowLimit, int rowOffset ) :
		                     	this( null, stmtType, cmd, sql, type, rowLimit, rowOffset )
		{
		}

		/// <summary>
		/// Construct a new SqlStatement instance of the specified type. The command object to be used
		/// for executing the query and the fully specified sql query string must be specified.  
		/// </summary>
		/// <param name="broker">The PersistenceBroker instance to use for database access.</param>
		/// <param name="stmtType">The type of this statement.</param>
		/// <param name="cmd">The command object to use when executing the query.</param>
		/// <param name="sql">The fully specified sql query string.</param>
		/// <param name="type">The type of object being selected by this query or null if 
		/// not applicable.</param>
		/// <param name="rowLimit">The maximum number of rows to be returned by this query 
		/// or 0 for no limit.</param>
		/// <param name="rowOffset">The number of rows to be skipped by this statement. Note that
		/// for SQL Server this is applied after execution, whereas for other databases it is 
		/// embedded in the SQL string.</param>
		public SqlStatement( PersistenceBroker broker, StatementType stmtType, IDbCommand cmd,
		                     string sql, Type type, int rowLimit, int rowOffset ) : base( broker, type )
		{
			this.cmd = cmd;
			cmd.CommandText = sql;
			statementType = stmtType == StatementType.Unknown ? GetStatementType( sql ) : stmtType;
			map = type != null ? ObjectFactory.GetMap( broker, type ) : null;
			this.rowLimit = rowLimit;
			this.rowOffset = rowOffset;
		}

		private static DateTime EnsureValidDate( DateTime val )
		{
			// ensure date is within the range supported by the persistence engine
			// TODO static method cannot access broker instance
			GentleSqlFactory sf = Broker.Provider.GetSqlFactory();
			if( val < sf.MinimumSupportedDateTime )
			{
				return sf.MinimumSupportedDateTime;
			}
			if( val > sf.MaximumSupportedDateTime )
			{
				return sf.MaximumSupportedDateTime;
			}
			return val;
		}

		/// <summary>
		/// TODO FIXME 
		/// This method breaks use of multiple brokers, as it is static and references Broker (and thus DefaultProvider).
		/// </summary>
		internal static object GetParameterValue( object val, ObjectMap map, FieldMap fm, StatementType stmtType )
		{
			Check.VerifyNotNull( map, Error.NullParameter, "map" );
			Check.VerifyNotNull( fm, Error.NullParameter, "fm" );
			object result;
			// make a quick exit with the type name if the column is used for inheritance mapping
			if( map.InheritanceMap != null && fm.ColumnName == map.InheritanceMap.ColumnName )
			{
				return map.Type.AssemblyQualifiedName;
			}
			// null handling
			if( fm.IsNull( val ) )
			{
				// verify that null assignment isn't violating known defintions
				// dont check value types as they are never nullable, and allow null for autogenerated PKs
				if( val == null && ! fm.IsValueType && ! (fm.IsPrimaryKey && fm.IsAutoGenerated) )
				{
					Check.Verify( fm.IsNullable, Error.NullProperty, fm.MemberName, map.Type );
				}
				result = DBNull.Value;
			}
			else
			{
				// clip strings to make sure they fit target column
				if( fm.Type == typeof(string) && val != null &&
				    fm.Size > 0 && ((string) val).Length > fm.Size )
				{
					result = ((string) val).Substring( 0, fm.Size );
				} 
					// clip DateTime values
				else if( fm.Type == typeof(DateTime) )
				{
					// ensure that datetime values are clipped if outside database range
					result = EnsureValidDate( (DateTime) val );
				} 
					// convert enum values to numeric value
				else if( fm.Type.IsEnum )
				{
					// use the integer or string value of the enum
					result = fm.HandleEnumAsString
					         	? Convert.ChangeType( val, typeof(string) )
					         	: Convert.ChangeType( val, Enum.GetUnderlyingType( fm.Type ) );
				} 
					// if column is autogenerated primary key use DBNull if an integer value of 0 is given
				else if( fm.IsAutoGeneratedKeyAndSupportedType && val.Equals( 0 ) && stmtType == StatementType.Insert )
				{
					result = DBNull.Value;
				}
				else if( fm.Type == typeof(byte[]) || fm.Type == typeof(Byte[]) )
				{
					MemoryStream stream = new MemoryStream( (byte[]) val );
					result = stream.ToArray();
				}
				else if( fm.Type == typeof(Guid) && fm.Size == 16 )
				{
					result = TypeConverter.ToBinaryString( (Guid) val );
				}
				else if( fm.Type == typeof(Guid) && Broker.ProviderName.StartsWith( "Sybase" ) )
				{
					// Sybase needs the GUID as a string
					result = val.ToString();
				}
				else // no special handling for other types/values
				{
					result = val;
				}
			}
			return result;
		}

		private void SetParameter( IDataParameter param, object val, FieldMap fm )
		{
			Check.Verify( param.Direction == ParameterDirection.Input ||
			              param.Direction == ParameterDirection.InputOutput, "Cannot set value of output parameters!" );
			// do additional checking for known types
			if( map != null && fm != null )
			{
				param.Value = GetParameterValue( val, map, fm, statementType );
			}
			else // unknown type - no clipping or fancy checks, just do it
			{
				param.Value = val ?? DBNull.Value;
			}
		}

		/// <summary>
		/// Update a specific named parameter with the given value. 
		/// </summary>
		/// <param name="paramName">The parameter whose value to set</param>
		/// <param name="paramValue">The value being assigned to the parameter</param>
		public void SetParameter( string paramName, object paramValue )
		{
			FieldMap fm = map != null ? map.GetFieldMapFromColumn( paramName ) : null;
			// support setting parameters without using the prefix character
			if( ! cmd.Parameters.Contains( paramName ) )
			{
				GentleSqlFactory sf = broker.GetSqlFactory();
				paramName = sf.GetParameterPrefix() + paramName + sf.GetParameterSuffix();
			}
			Check.Verify( cmd.Parameters.Contains( paramName ), Error.NoSuchParameter, paramName );
			IDataParameter param = (IDataParameter) cmd.Parameters[ paramName ];
			SetParameter( param, paramValue, fm );
		}

		/// <summary>
		/// <p>Update the statement parameter values using the values from the key.</p>
		/// <p>Warning! This will erase all parameter values not in the key, so if you
		/// need to set additional parameters be sure to call this method before
		/// any calls to SetParameter.</p>
		/// </summary>
		/// <param name="key">The key instance containing the parameter values.</param>
		/// <param name="isUpdateAll">If true all statement parameters will be updated. If the
		/// key has no value a null value will be assigned.</param>
		public void SetParameters( Key key, bool isUpdateAll )
		{
			Check.VerifyNotNull( key, Error.NullParameter, "key" );
			GentleSqlFactory sf = broker.GetSqlFactory();
			foreach( IDataParameter param in cmd.Parameters )
			{
				// strip leading @ character if present
				string prefix = sf.GetParameterPrefix();
				string suffix = sf.GetParameterSuffix();
				string paramName = param.ParameterName;
				if( paramName.StartsWith( prefix ) )
				{
					paramName = paramName.Substring( prefix.Length, paramName.Length - prefix.Length );
				}
				//if needed, strip out the suffix from the paramName/column name
				if( suffix != "" && paramName.EndsWith( suffix ) )
				{
					paramName = paramName.Substring( 0, paramName.Length - suffix.Length );
				}
				FieldMap fm = map.GetFieldMapFromColumn( paramName );
				// handle special case where parameter is concurrency control column
				if( fm == null && map.ConcurrencyMap != null && paramName.StartsWith( "New" ) &&
				    (paramName.Substring( 3, paramName.Length - 3 ) == map.ConcurrencyMap.ColumnName) )
				{
					fm = map.ConcurrencyMap;
				}
				// if key contains property names translate parameter name into property name 
				string index = key.isPropertyKeys ? fm.MemberName : paramName;
				// check if we should set parameter value
				if( key.ContainsKey( index ) || isUpdateAll )
				{
					Check.VerifyNotNull( fm, Error.Unspecified, "Could not find a value for the parameter named {0}", index );
					SetParameter( param, key.ContainsKey( index ) ? key[ index ] : null, fm );
				}
			}
		}

		/// <summary>
		/// Update statement parameter values using the public properties of the source object.
		/// This method simply gathers all public properties of the given object into a key and
		/// calls the SetParameters method with the generated key and a boolean
		/// value of false to indicate that parameters not in the key are to be left alone.
		/// </summary>
		/// <param name="source">The object from which to pull parameter values</param>
		/// <param name="isUpdateAll">If true all statement parameters will be updated. If the
		/// key has no value a null value will be assigned.</param>
		public void SetParameters( object source, bool isUpdateAll )
		{
			Check.VerifyNotNull( source, Error.NullParameter, "source" );
			ObjectMap map = ObjectFactory.GetMap( broker, source );
			Key key = new Key( false );
			foreach( FieldMap fm in map.Fields )
			{
				key[ fm.ColumnName ] = fm.GetValue( source );
				// update parameter for soft deletion and concurrency control
				if( fm.IsSoftDeleteColumn && statementType == StatementType.SoftDelete )
				{
					// key[ /*"New"+*/ fm.ColumnName ] = -1;
					if( GentleSettings.ConcurrencyControl && fm.IsConcurrencyColumn )
					{
						key[ "New" + fm.ColumnName ] = -1;
					}
					else
					{
						key[ fm.ColumnName ] = -1;
					}
				}
				else if( fm.IsConcurrencyColumn )
				{
					long version = Convert.ToInt64( key[ fm.ColumnName ] );
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
					key[ "New" + fm.ColumnName ] = version;
				}
			}
			SetParameters( key, isUpdateAll );
		}

		/// <summary>
		/// Update the property values of the supplied object instance with the values of the first
		/// row of the rows contained in the supplied SqlResult.
		/// </summary>
		public void SetProperties( object obj, SqlResult sr )
		{
			Check.VerifyNotNull( obj, "Unable to update null object!" );
			Check.VerifyNotNull( sr, "Unable to update object (SqlResult was null)!" );
			ObjectMap map = ObjectFactory.GetMap( sr.SessionBroker, obj.GetType() );
			map.SetProperties( obj, sr.ColumnNames, (object[]) sr.Rows[ 0 ] );
		}

		/// <summary>
		/// Execute the current instance using the supplied database connection and return
		/// the result of the operation as an instance of the <see cref="SqlResult"/> class.
		/// </summary>
		/// <param name="conn">The database connection to use for executing this statement.</param>
		/// <param name="tr">The transaction instance to run this statement in. If there is no
		/// transaction context then this parameter should be null. This will cause the 
		/// connection to be closed after execution.</param>
		/// <returns>The result of the operation</returns>
		internal SqlResult Execute( IDbConnection conn, IDbTransaction tr )
		{
			Check.VerifyNotNull( conn, Error.NoNewConnection );
			cmd.Connection = conn;
			cmd.Transaction = tr;
			SqlResult sr = null;
			StringBuilder log = IsLoggingEnabled ? new StringBuilder( 500 ) : null;
			try
			{
				if( log != null )
				{
					log.AppendFormat( "Executing query: {0}{1}", cmd.CommandText, Environment.NewLine );
					foreach( IDataParameter param in cmd.Parameters )
					{
						object paramValue = param.Value == null ? "null" : param.Value;
						log.AppendFormat( "Parameter: {0} = {1}{2}", param.ParameterName, paramValue, Environment.NewLine );
					}
				}
				switch( statementType )
				{
					case StatementType.Count:
					case StatementType.Identity:
						sr = new SqlResult( broker, cmd.ExecuteScalar(), this );
						break;
					case StatementType.Select:
						sr = new SqlResult( broker, cmd.ExecuteReader(), this );
						break;
					case StatementType.Update:
					case StatementType.Delete:
					case StatementType.SoftDelete:
						// returns the number of rows affected
						sr = new SqlResult( broker, cmd.ExecuteNonQuery(), this );
						break;
					case StatementType.Insert:
						if( map != null && map.IdentityMap != null )
						{
							GentleSqlFactory sf = broker.GetSqlFactory();
							// check if we need to execute insert statement separately
							if( ! sf.HasCapability( Capability.BatchQuery ) )
							{
								// execute the insert command
								cmd.ExecuteNonQuery();
								// get the sql string for retrieving the generated id
								string sql = sf.GetIdentitySelect( null, map );
								// get an sql statement that will execute using ExecuteScalar
								SqlStatement stmt = broker.GetStatement( StatementType.Identity, sql );
								// execute the query and fetch the id
								sr = stmt.Execute( cmd.Connection, tr );
							}
							else
							{
								// returns the 1st column of the 1st row in the result set (i.e. new identity)
								sr = new SqlResult( broker, cmd.ExecuteScalar(), this );
							}
						}
						else
						{
							sr = new SqlResult( broker, cmd.ExecuteNonQuery(), this );
						}
						break;
					default:
						// returns the number of rows affected
						sr = new SqlResult( broker, cmd.ExecuteReader(), this );
						break;
				}
				if( log != null )
				{
					PerformExecutionLog( log, sr );
				}
			}
			catch( Exception e )
			{
				if( ! Check.IsCalledFrom( "Analyzer.Analyze", e ) || ! GentleSettings.AnalyzerSilent )
				{
					Check.LogError( LogCategories.StatementExecution, e );
				}
				if( e is GentleException )
				{
					throw;
				}
				else
				{
					throw new GentleException( Error.StatementError, cmd.CommandText, e );
				}
			}
			finally
			{
				// close the database connection (only if not in transaction)
				if( conn != null && conn.State == ConnectionState.Open && tr == null )
				{
					conn.Close();
				}
				// only if not in transaction; would otherwise break with PostgreSQL (not permitted)
				if( tr == null )
				{
					// clear the connection reference
					try
					{
						cmd.Connection = null;
					}
					catch
					{
						// ignore errors here.. like for the SQLite-provider which complains for no good reason
					}
					// clear the transaction reference 
					cmd.Transaction = null;
				}
			}
			return sr;
		}

		internal SqlResult Execute( IDbTransaction tr )
		{
			Check.VerifyNotNull( tr, Error.NullParameter, "tr" );
			return Execute( tr.Connection, tr );
		}

		/// <summary>
		/// Convenience method for executing statements without using the <see cref="Broker"/> class. 
		/// </summary>
		/// <returns>The SqlResult containing the result of the operation</returns>
		public SqlResult Execute()
		{
			return broker.Execute( this, null, null );
		}

		/// <summary>
		/// Prepare the statement against the database. Useful if you will be reusing the statement 
		/// for multiple queries.
		/// </summary>
		public void Prepare()
		{
			if( cmd != null )
			{
				cmd.Prepare();
			}
		}

		#region Paging Methods
		public SqlResult Previous()
		{
			int page = rowOffset / rowLimit;
			return Page( page < 1 ? 1 : page );
		}

		public SqlResult Next()
		{
			int page = 2 + rowOffset / rowLimit;
			return Page( page );
		}

		/// <summary>
		/// Return the specified result set page. Beware of the fact that page indexes are 1-based.
		/// </summary>
		/// <param name="page">The page number to fetch. Use a value of 1 to obtain the first page.</param>
		/// <returns>A new SqlResult instance with any matching rows</returns>
		public SqlResult Page( int page )
		{
			Check.Verify( rowLimit > 0, Error.DeveloperError,
			              "The paging methods are only available for statements with a row limit." );
			SqlBuilder sb = new SqlBuilder( broker );
			cmd.CommandText = sb.GetPagedSql( page, Sql, rowLimit, ref rowOffset );
			return Execute();
		}
		#endregion

		#region Properties
		/// <summary>
		/// The command object used by this instance.
		/// </summary>
		public IDbCommand Command
		{
			get { return cmd; }
		}
		/// <summary>
		/// The statement type of this instance.
		/// </summary>
		public StatementType StatementType
		{
			get { return statementType; }
			set
			{
				// only allow updates that are not in obvious conflict with the real world
				if( statementType == StatementType.Unknown )
				{
					statementType = value;
				}
			}
		}
		/// <summary>
		/// The type being selected by this instance or null if no type is being selected.
		/// </summary>
		public Type Type
		{
			get { return map != null ? map.Type : null; }
		}
		/// <summary>
		/// The query string this statement encapsulates.
		/// </summary>
		public string Sql
		{
			get { return cmd.CommandText; }
		}
		/// <summary>
		/// The maximum number of rows returned by executing this statement or 0 if no limit applies.
		/// </summary>
		public int RowLimit
		{
			get { return rowLimit; }
		}
		/// <summary>
		/// The maximum number of rows returned by executing this statement or 0 if no limit applies.
		/// </summary>
		public int RowOffset
		{
			get { return rowOffset; }
		}

		/// <summary>
		/// Construct a unique string identifying this statement instance (for caching purposes).
		/// </summary>
		public string CacheKey
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat( "{0}|{1}|{2}", "Query", map != null ? map.Type : Type.Missing, cmd.CommandText );
				foreach( IDataParameter param in cmd.Parameters )
				{
					sb.AppendFormat( "|{0}={1}", param.ParameterName, param.Value );
				}
				return sb.ToString();
			}
		}

		/// <summary>
		/// Check whether statement execution logging is on for the current instance.
		/// </summary>
		public bool IsLoggingEnabled
		{
			get
			{
				switch( StatementType )
				{
					case StatementType.Select:
					case StatementType.Count:
					case StatementType.Identity:
						return GentleSettings.IsLoggingEnabled( LogCategories.StatementExecutionRead, false );
					case StatementType.Insert:
					case StatementType.Update:
					case StatementType.Delete:
					case StatementType.SoftDelete:
						return GentleSettings.IsLoggingEnabled( LogCategories.StatementExecutionWrite, false );
					case StatementType.Unknown:
					default:
						return GentleSettings.IsLoggingEnabled( LogCategories.StatementExecutionOther, false );
				}
			}
		}
		#endregion

		private void PerformExecutionLog( StringBuilder log, SqlResult sr )
		{
			if( log != null )
			{
				log.Append( "Execution result: " );
				LogCategories category = LogCategories.StatementExecutionRead;
				switch( StatementType )
				{
					case StatementType.Select:
					case StatementType.Identity:
						log.AppendFormat( "{0} row{1} retrieved.", sr.RowsContained, sr.RowsContained == 1 ? "" : "s" );
						break;
					case StatementType.Count:
						log.AppendFormat( "{0} row{1} counted.", sr.Count, sr.Count == 1 ? "" : "s" );
						break;
					case StatementType.Insert:
					case StatementType.Update:
					case StatementType.Delete:
					case StatementType.SoftDelete:
						log.AppendFormat( "{0} row{1} affected.", sr.RowsAffected, sr.RowsAffected == 1 ? "" : "s" );
						category = LogCategories.StatementExecutionWrite;
						break;
					case StatementType.Unknown:
					default:
						category = LogCategories.StatementExecutionOther;
						break; // no post-execution log for these statements
				}
				log.Append( Environment.NewLine );
				Check.LogInfo( category, log.ToString() );
			}
		}
	}
}