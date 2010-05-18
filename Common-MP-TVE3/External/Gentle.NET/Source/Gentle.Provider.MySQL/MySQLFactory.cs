/*
 * MySql specifics
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: MySQLFactory.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;
using Gentle.Common;
using Gentle.Framework;
using MySql.Data.MySqlClient;

namespace Gentle.Provider.MySQL
{
	/// <summary>
	/// This class is an implementation of the <see cref="GentleSqlFactory"/> class for the MySQL RDBMS.
	/// </summary>
	public class MySQLFactory : GentleSqlFactory
	{
		public MySQLFactory( IGentleProvider provider ) : base( provider )
		{
		}

		/// <summary>
		/// Obtain the integer value of the database type corresponding to the given system type.
		/// The value returned must be castable to a valid type for the current persistence engine.
		/// </summary>
		/// <param name="type">The system type.</param>
		/// <returns>The corresponding database type.</returns>
		public override long GetDbType( Type type )
		{
			MySqlDbType result = MySqlDbType.Int32;
			if( type.Equals( typeof(int) ) || type.IsEnum )
			{
				result = MySqlDbType.Int32;
			}
			else if( type.Equals( typeof(long) ) )
			{
				result = MySqlDbType.Int64;
			}
			else if( type.Equals( typeof(double) ) || type.Equals( typeof(Single) ) )
			{
				result = MySqlDbType.Double;
			}
			else if( type.Equals( typeof(decimal) ) )
			{
				result = MySqlDbType.Decimal;
			}
			else if( type.Equals( typeof(DateTime) ) )
			{
				result = MySqlDbType.Datetime;
			}
			else if( type.Equals( typeof(bool) ) || type.Equals( typeof(Byte) ) || type.Equals( typeof(byte) ) )
			{
				result = MySqlDbType.Byte;
			}
			else if( type.Equals( typeof(string) ) )
			{
				result = MySqlDbType.String;
			}
			else if( type.Equals( typeof(byte[]) ) || type.Equals( typeof(Byte[]) ) )
			{
				// hmm.. possible type size loss here as we dont know the length of the array
				result = MySqlDbType.LongBlob;
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
				case "byte": // 1
					return (long) MySqlDbType.Byte;
				case "tinyint": // 1
					return (long) MySqlDbType.Byte;
				case "smallint": // 2
					return (long) MySqlDbType.Int16;
				case "int": // 4
					return (long) MySqlDbType.Int32;
				case "bigint": // 8
					return (long) MySqlDbType.Int64;
				case "float": // 8
					return (long) MySqlDbType.Float;
				case "datetime": // 8
					return (long) MySqlDbType.Datetime;
				case "decimal":
				case "numeric":
					return (long) MySqlDbType.Decimal;
				case "char":
					return (long) MySqlDbType.String;
				case "varchar":
					return (long) MySqlDbType.VarChar;
				case "text":
					return (long) MySqlDbType.String;
				case "tinyblob":
					return (long) MySqlDbType.TinyBlob;
				case "blob":
					return (long) MySqlDbType.Blob;
				case "mediumblob":
					return (long) MySqlDbType.MediumBlob;
				case "longblob":
					return (long) MySqlDbType.LongBlob;
				case "enum":
					return (long) MySqlDbType.Enum;
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
		/// in the correct case. 
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details on the
		/// purpose of this method. 
		/// </summary>
		/// <param name="sql">The sql string to which we should append</param>
		/// <param name="om">An <see cref="Gentle.Framework.ObjectMap"/> instance of the object for which to retrieve the identity select</param>
		/// <returns>The modified sql string which also retrieves the identity value</returns>
		public override string GetIdentitySelect( string sql, ObjectMap om )
		{
			return String.Format( "{0}{1} {2}", sql, GetStatementTerminator(),
			                      "select LAST_INSERT_ID()" );
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override string GetParameterPrefix()
		{
			return "?";
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override char GetQuoteCharacter()
		{
			return '"'; // was: '`';
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override void AddParameter( IDbCommand cmd, string name, long dbType )
		{
			try
			{
				MySqlCommand myc = (MySqlCommand) cmd;
				MySqlParameter param = new MySqlParameter( name, (MySqlDbType) dbType );
				param.Direction = ParameterDirection.Input;
				myc.Parameters.Add( param );
			}
			catch( Exception e )
			{
				Check.Fail( Error.Unspecified, e.Message );
				throw new GentleException( Error.Unspecified, "Unreachable code." );
			}
		}
	}
}