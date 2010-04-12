/*
 * DB2 specifics
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: DB2Factory.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;
using Gentle.Common;
using Gentle.Framework;
using IBM.Data.DB2;

namespace Gentle.Provider.DB2
{
	/// <summary>
	/// This class is an implementation of the <see cref="GentleSqlFactory"/> class for the DB2 RDBMS.
	/// </summary>
	public class DB2Factory : GentleSqlFactory
	{
		public DB2Factory( IGentleProvider provider ) : base( provider )
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
			DB2Type result = DB2Type.Integer;
			if( type.Equals( typeof(int) ) || type.IsEnum )
			{
				result = DB2Type.Integer;
			}
			else if( type.Equals( typeof(long) ) )
			{
				result = DB2Type.BigInt;
			}
			else if( type.Equals( typeof(float) ) )
			{
				result = DB2Type.Real;
			}
			else if( type.Equals( typeof(double) ) )
			{
				result = DB2Type.Double;
			}
			else if( type.Equals( typeof(decimal) ) )
			{
				result = DB2Type.Decimal;
			}
			else if( type.Equals( typeof(DateTime) ) )
			{
				result = DB2Type.Timestamp;
			}
			else if( type.Equals( typeof(bool) ) || type.Equals( typeof(Byte) ) || type.Equals( typeof(byte) ) )
			{
				result = DB2Type.SmallInt;
			}
			else if( type.Equals( typeof(string) ) )
			{
				result = DB2Type.VarChar;
			}
			else if( type.Equals( typeof(byte[]) ) || type.Equals( typeof(Byte[]) ) )
			{
				// hmm.. possible type size loss here as we dont know the length of the array
				result = DB2Type.Blob;
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
				case "smallint": // 2
					return (long) DB2Type.SmallInt;
				case "int": // 4
					return (long) DB2Type.Integer;
				case "bigint": // 8
					return (long) DB2Type.BigInt;
				case "real": // 4
					return (long) DB2Type.Real;
				case "double": // 8
				case "double precision": // 8
					return (long) DB2Type.Double;
				case "float": // 8
					return (long) DB2Type.Float;
				case "decimal":
				case "numeric":
					return (long) DB2Type.Decimal;
				case "date": // 8
					return (long) DB2Type.Date;
				case "time": // 8
					return (long) DB2Type.Time;
				case "timestamp": // 8
					return (long) DB2Type.Timestamp;
				case "char":
					return (long) DB2Type.Char;
				case "varchar":
					return (long) DB2Type.VarChar;
					// not supported by the DB2 data provider:
					//case "longvarchar":
					//case "long varchar":
					//	return (long) DB2Type.LongVarChar;
				case "binary":
				case "char for bit data":
					return (long) DB2Type.Graphic;
				case "varbinary":
				case "varchar for bit data":
					return (long) DB2Type.VarGraphic;
					// not supported by the DB2 data provider:
					//case "longvarbinary":
					//case "long varbinary":
					//case "long varchar for bit data":
					//	return (long) DB2Type.Graphic;
				case "graphic":
					return (long) DB2Type.Graphic;
				case "vargraphic":
					return (long) DB2Type.VarGraphic;
					// not supported by the DB2 data provider:
					//case "longvargraphic":
					//case "long vargraphic":
					//	return (long) DB2Type.Graphic;
				case "clob":
					return (long) DB2Type.Clob;
				case "blob":
					return (long) DB2Type.Blob;
				case "dbclob":
					return (long) DB2Type.DbClob;
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
			return '\'';
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override void AddParameter( IDbCommand cmd, string name, long dbType )
		{
			try
			{
				DB2Command myc = (DB2Command) cmd;
				DB2Parameter param = new DB2Parameter( name, (DB2Type) dbType );
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