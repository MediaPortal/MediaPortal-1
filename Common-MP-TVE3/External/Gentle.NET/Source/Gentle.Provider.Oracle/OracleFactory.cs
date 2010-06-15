/*
 * Oracle specifics
 * Copyright (C) 2004 Andreas Seibt
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: OracleFactory.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;
using System.Data.OracleClient;
using Gentle.Common;
using Gentle.Framework;

namespace Gentle.Provider.Oracle
{
	/// <summary>
	/// This class is an implementation of the <see cref="GentleSqlFactory"/> class for the 
	/// Oracle RDBMS.
	/// </summary>
	public class OracleFactory : GentleSqlFactory
	{
		public OracleFactory( IGentleProvider provider ) : base( provider )
		{
		}

		public static string[] reservedWords = new[]
		                                       	{
		                                       		"ACCESS", "ADD", "ALL", "ALTER", "AND", "ANY", "AS", "ASC", "AUDIT",
		                                       		"BETWEEN", "BY",
		                                       		"CHAR", "CHECK", "CLUSTER", "COLUMN", "COMMENT", "COMPRESS", "CONNECT", "CREATE", "CURRENT",
		                                       		"DATE", "DECIMAL", "DEFAULT", "DELETE", "DESC", "DISTINCT", "DROP",
		                                       		"ELSE", "EXCLUSIVE", "EXISTS",
		                                       		"FILE", "FLOAT", "FOR", "FROM",
		                                       		"GRANT", "GROUP",
		                                       		"HAVING",
		                                       		"IDENTIFIED", "IMMEDIATE", "IN", "INCREMENT", "INDEX", "INITIAL", "INSERT", "INTEGER",
		                                       		"INTERSECT", "INTO", "IS",
		                                       		"LEVEL", "LIKE", "LOCK", "LONG",
		                                       		"MAXEXTENTS", "MINUS", "MLSLABEL", "MODE", "MODIFY",
		                                       		"NOAUDIT", "NOCOMPRESS", "NOT", "NOWAIT", "NULL", "NUMBER",
		                                       		"OF", "OFFLINE", "ON", "ONLINE", "OPTION", "OR", "ORDER",
		                                       		"PCTFREE", "PRIOR", "PRIVILEGES", "PUBLIC",
		                                       		"RAW", "RENAME", "RESOURCE", "REVOKE", "ROW", "ROWID", "ROWNUM", "ROWS",
		                                       		"SELECT", "SESSION", "SET", "SHARE", "SIZE", "SMALLINT", "START", "SUCCESSFUL", "SYNONYM",
		                                       		"SYSDATE",
		                                       		"TABLE", "THEN", "TO", "TRIGGER",
		                                       		"UID", "UNION", "UNIQUE", "UPDATE", "USER",
		                                       		"VALIDATE", "VALUES", "VARCHAR", "VARCHAR2", "VIEW",
		                                       		"WHENEVER", "WHERE", "WITH"
		                                       	};

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override long GetDbType( Type type )
		{
			OracleType result = OracleType.Int32;
			if( type.Equals( typeof(byte) ) || type.Equals( typeof(Byte) ) )
			{
				result = OracleType.Byte;
			}
			else if( type.Equals( typeof(sbyte) ) || type.Equals( typeof(SByte) ) )
			{
				result = OracleType.SByte;
			}
			else if( type.Equals( typeof(ushort) ) || type.Equals( typeof(UInt16) ) )
			{
				result = OracleType.UInt16;
			}
			else if( type.Equals( typeof(short) ) || type.Equals( typeof(Int16) ) )
			{
				result = OracleType.Int16;
			}
			else if( type.Equals( typeof(int) ) || type.Equals( typeof(Int32) ) || type.IsEnum )
			{
				result = OracleType.Int32;
			}
			else if( type.Equals( typeof(uint) ) || type.Equals( typeof(UInt32) ) )
			{
				result = OracleType.UInt32;
			}
			else if( type.Equals( typeof(long) ) || type.Equals( typeof(Int64) ) )
			{
				result = OracleType.Number;
			}
			else if( type.Equals( typeof(ulong) ) || type.Equals( typeof(UInt64) ) )
			{
				result = OracleType.Number;
			}
			else if( type.Equals( typeof(float) ) || type.Equals( typeof(Single) ) )
			{
				result = OracleType.Float;
			}
			else if( type.Equals( typeof(double) ) )
			{
				result = OracleType.Double;
			}
			else if( type.Equals( typeof(decimal) ) || type.Equals( typeof(Decimal) ) )
			{
				result = OracleType.Number;
			}
			else if( type.Equals( typeof(DateTime) ) )
			{
				result = OracleType.DateTime;
			}
			else if( type.Equals( typeof(bool) ) )
			{
				result = OracleType.Byte;
			}
			else if( type.Equals( typeof(string) ) )
			{
				result = OracleType.VarChar;
			}
			else if( type.Equals( typeof(byte[]) ) )
			{
				result = OracleType.Blob;
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
			string tmp = dbType.ToLower();
			switch( tmp )
			{
				case "bfile":
					return (long) OracleType.BFile;
				case "blob":
					return (long) OracleType.Blob;
				case "byte":
					return (long) OracleType.Byte;
				case "char":
					return (long) OracleType.Char;
				case "clob":
					return (long) OracleType.Clob;
				case "cursor":
					return (long) OracleType.Cursor;
				case "date":
				case "datetime":
					return (long) OracleType.DateTime;
				case "double":
					return (long) OracleType.Double;
				case "float":
					return (long) OracleType.Float;
				case "int16":
					return (long) OracleType.Int16;
				case "int32":
					return (long) OracleType.Int32;
				case "intervaldaytosecond":
					return (long) OracleType.IntervalDayToSecond;
				case "intervalyeartomonth":
					return (long) OracleType.IntervalYearToMonth;
				case "longraw":
				case "long raw":
					return (long) OracleType.LongRaw;
				case "longvarchar":
					return (long) OracleType.LongVarChar;
				case "nchar":
					return (long) OracleType.NChar;
				case "nclob":
					return (long) OracleType.NClob;
				case "number":
					return (long) OracleType.Number;
				case "nvarchar":
				case "nvarchar2":
					return (long) OracleType.NVarChar;
				case "raw":
					return (long) OracleType.Raw;
				case "rowid":
				case "urowid":
					return (long) OracleType.RowId;
				case "sbyte":
					return (long) OracleType.SByte;
				case "timestamp":
					return (long) OracleType.Timestamp;
				case "timestamplocal":
					return (long) OracleType.TimestampLocal;
				case "timestampwithtz":
					return (long) OracleType.TimestampWithTZ;
				case "uint16":
					return (long) OracleType.UInt16;
				case "uint32":
					return (long) OracleType.UInt32;
				case "varchar":
				case "varchar2":
					return (long) OracleType.VarChar;
				default:
					if( ! GentleSettings.AnalyzerSilent )
					{
						Check.Fail( Error.UnsupportedColumnType, dbType, provider.Name );
					}
					return NO_DBTYPE; // unreachable
			}
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
		public override string GetStatementTerminator()
		{
			return string.Empty;
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override string GetIdentitySelect( string sql, ObjectMap om )
		{
			string seqName = (om.IdentityMap.SequenceName != null ? om.IdentityMap.SequenceName : (om.TableName + "_seq").ToUpper());
			return String.Format( "select {0}.currval from dual", seqName );
		}

		/// <summary>
		/// Obtain an enum describing the supported database capabilities.  
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details.
		/// </summary>
		public override Capability Capabilities
		{
			get { return Capability.Paging | Capability.NamedParameters; }
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override void AddParameter( IDbCommand cmd, string name, long dbType )
		{
			try
			{
				OracleCommand sc = (OracleCommand) cmd;
				// prefix parameters with @ for SQL Server
				sc.Parameters.Add( GetParameterPrefix() + name, (OracleType) dbType );
			}
			catch( Exception e )
			{
				Check.Fail( Error.Unspecified, e.Message );
				throw new GentleException( Error.Unspecified, "Unreachable code" );
			}
		}

		/// <summary>
		/// Determine is a word is reserved and needs special quoting.
		/// </summary>
		/// <returns>True if the word is reserved</returns>
		public override bool IsReservedWord( string word )
		{
			Check.VerifyNotNull( word, Error.NullParameter, "word" );
			word = word.ToUpper();
			foreach( string reserved in reservedWords )
			{
				if( word.Equals( reserved ) )
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Obtain a quoted version of the reserved word to allow the reserved word to be 
		/// used in queries anyway. If a reserved word cannot be quoted this method should
		/// raise an error informing the user that they need to pick a different name.
		/// </summary>
		/// <returns>The given reserved word or field quoted to avoid errors.</returns>
		public override string QuoteReservedWord( string word )
		{
			return word;
			// Won't work with the current framework version
			//			return String.Format( "{0}{1}{2}", "\"", word, "\"" );
		}
	}
}