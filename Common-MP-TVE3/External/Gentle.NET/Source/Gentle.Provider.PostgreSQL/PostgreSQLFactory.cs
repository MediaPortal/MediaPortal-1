/*
 * PostgreSQL specifics
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: PostgreSQLFactory.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;
using Gentle.Common;
using Gentle.Framework;
using Npgsql;
using NpgsqlTypes;

namespace Gentle.Provider.PostgreSQL
{
	/// <summary>
	/// This class is an implementation of the <see cref="GentleSqlFactory"/> class for the PostgreSQL RDBMS.
	/// </summary>
	public class PostgreSQLFactory : GentleSqlFactory
	{
		public PostgreSQLFactory( IGentleProvider provider ) : base( provider )
		{
			// TODO schema name is currently hardcoded
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override long GetDbType( Type type )
		{
			NpgsqlDbType result = NpgsqlDbType.Integer;
			if( type.Equals( typeof(int) ) || type.IsEnum )
			{
				result = NpgsqlDbType.Integer;
			}
			else if( type.Equals( typeof(long) ) )
			{
				result = NpgsqlDbType.Bigint;
			}
			else if( type.Equals( typeof(double) ) )
			{
				result = NpgsqlDbType.Double;
			}
			else if( type.Equals( typeof(decimal) ) || type.Equals( typeof(Decimal) ) )
			{
				result = NpgsqlDbType.Numeric;
			}
			else if( type.Equals( typeof(DateTime) ) )
			{
				result = NpgsqlDbType.Timestamp;
			}
			else if( type.Equals( typeof(bool) ) )
			{
				result = NpgsqlDbType.Boolean;
			}
			else if( type.Equals( typeof(string) ) )
			{
				result = NpgsqlDbType.Text;
			}
			else if( type.Equals( typeof(byte[]) ) )
			{
				result = NpgsqlDbType.Bytea;
			}
			else
			{
				Check.Fail( Error.UnsupportedPropertyType, type.Name, provider.Name );
			}
			return (long) result;
		}

		/// <summary>
		/// This method converts the given string (as extracted from the database system tables) 
		/// to the corresponding type enumeration value. 
		/// </summary>
		/// <param name="dbType">The name of the type with the database engine used.</param>
		/// <param name="isUnsigned">A boolean value indicating whether the type is unsigned. This
		/// is not supported by most engines and/or data providers and is thus fairly useless at
		/// this point.</param>
		/// <returns>The value of the corresponding database type enumeration. The enum is converted
		/// to its numeric (long) representation because each provider uses its own enum (and they
		/// are not compatible with the generic DbType defined in System.Data).</returns>
		public override long GetDbType( string dbType, bool isUnsigned )
		{
			switch( dbType )
			{
				case "bool": // 1
					return (long) NpgsqlDbType.Boolean;
				case "int2": // 2
					return (long) NpgsqlDbType.Smallint;
				case "int4": // 4
					return (long) NpgsqlDbType.Integer;
				case "int8": // 8
					return (long) NpgsqlDbType.Bigint;
				case "float4": // 8
					return (long) NpgsqlDbType.Real;
				case "float8": // 8
					return (long) NpgsqlDbType.Double;
				case "date": // 8
					return (long) NpgsqlDbType.Date;
				case "time": // 8
					return (long) NpgsqlDbType.Time;
				case "timestamp":
					return (long) NpgsqlDbType.Timestamp;
				case "numeric":
					return (long) NpgsqlDbType.Numeric;
				case "bpchar":
				case "varchar":
				case "text":
					return (long) NpgsqlDbType.Text;
				case "point":
					return (long) NpgsqlDbType.Point;
				case "box":
					return (long) NpgsqlDbType.Box;
				case "lseg":
					return (long) NpgsqlDbType.LSeg;
				case "path":
					return (long) NpgsqlDbType.Path;
				case "polygon":
					return (long) NpgsqlDbType.Polygon;
				case "circle":
					return (long) NpgsqlDbType.Circle;
				case "bytea":
					return (long) NpgsqlDbType.Bytea;
				default:
					if( ! GentleSettings.AnalyzerSilent )
					{
						Check.Fail( Error.UnsupportedColumnType, dbType, provider.Name );
					}
					return NO_DBTYPE; // unreachable
			}
		}

		/// <summary>
		/// This method assumes that the tableName and identityColumn parameters are passed
		/// in the correct case. For PostgreSQL, it looks as if the tableName must be an exact 
		/// case-sensitive match and that the identityColumn is always lowercased (at least
		/// for auto-generated sequences). 
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details on the
		/// purpose of this method. 
		/// </summary>
		/// <param name="sql">The sql string to which we should append</param>
		/// <param name="om">An <see cref="Gentle.Framework.ObjectMap"/> instance of the object for which to retrieve the identity select</param>
		/// <returns>The modified sql string which also retrieves the identity value</returns>
		public override string GetIdentitySelect( string sql, ObjectMap om )
		{
			string fmtSql = " select currval('{0}{1}_{2}_seq')";
			string identitySql;
			// select currval('[<schemaName>.]<tableName>_<columnName>_seq');
			if( provider.SchemaName != null )
			{
				identitySql = String.Format( fmtSql, provider.SchemaName + ".", om.TableName, om.IdentityMap.ColumnName.ToLower() );
			}
			else
			{
				identitySql = String.Format( fmtSql, "", om.TableName, om.IdentityMap.ColumnName.ToLower() );
			}
			return sql + GetStatementTerminator() + identitySql;
		}

		/// <summary>
		/// Formats the given table name for use in queries. This may include prefixing
		/// it with a schema name or suffixing it with an alias (for multi-table selects).
		/// This default implementation simply returns the string given.
		/// </summary>
		/// <param name="tableName">The table name to format</param>
		/// <returns>The formatted table name</returns>
		public override string GetTableName( string tableName )
		{
			return provider.SchemaName != null ? provider.SchemaName + "." + tableName : tableName;
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override string GetParameterPrefix()
		{
			return ":";
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override char GetQuoteCharacter()
		{
			return '\'';
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override void AddParameter( IDbCommand cmd, string name, long dbType )
		{
			try
			{
				NpgsqlCommand pgc = (NpgsqlCommand) cmd;
				NpgsqlParameter param = new NpgsqlParameter( name, (NpgsqlDbType) dbType );
				param.Direction = ParameterDirection.Input;
				pgc.Parameters.Add( param );
			}
			catch( Exception e )
			{
				Check.Fail( Error.Unspecified, e.Message );
				throw new GentleException( Error.Unspecified, "Unreachable code" );
			}
		}
	}
}