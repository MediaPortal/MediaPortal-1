/*
 * Sybase ASA specifics
 * Copyright (C) 2004 Uwe Kitzmann
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: SybaseASAFactory.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;
using System.Text;
using Gentle.Common;
using Gentle.Framework;
using iAnywhere.Data.AsaClient;

namespace Gentle.Provider.SybaseASA
{
	/// <summary>
	/// This class is an implementation of the <see cref="GentleSqlFactory"/> class for the 
	/// Sybase Asa Database.
	/// </summary>
	public class SybaseASAFactory : GentleSqlFactory
	{
		public SybaseASAFactory( IGentleProvider provider ) : base( provider )
		{
		}

		/// <summary>
		/// This is the list of reserved words in Sybase SQL Anywhere. 
		/// </summary>
		public static string[] reservedWords = new[]
		                                       	{
		                                       		"ADD", "ALL", "ALTER", "AND", "ANY", "AS", "ASC", "BACKUP", "BEGIN", "BETWEEN", "BIGINT",
		                                       		"BINARY", "BIT",
		                                       		"BOTTOM", "BREAK", "BY", "CALL", "CAPABILITY", "CASCADE", "CASE", "CAST", "CHAR", "CHAR_CONVERT",
		                                       		"CHARACTER",
		                                       		"CHECK", "CHECKPOINT", "CLOSE", "COMMENT", "COMMIT", "CONNECT", "CONSTRAINT", "CONTAINS",
		                                       		"CONTINUE",
		                                       		"CONVERT", "CREATE", "CROSS", "CUBE", "CURRENT", "CURRENT_TIMESTAMP", "CURRENT_USER", "CURSOR",
		                                       		"DATE",
		                                       		"DBSPACE", "DEALLOCATE", "DEC", "DECIMAL", "DECLARE", "DEFAULT", "DELETE", "DELETING", "DESC",
		                                       		"DISTINCT",
		                                       		"DO", "DOUBLE", "DROP", "DYNAMIC", "ELSE", "ELSEIF", "ENCRYPTED", "END", "ENDIF", "ESCAPE",
		                                       		"EXCEPT",
		                                       		"EXCEPTION", "EXEC", "EXECUTE", "EXISTING", "EXISTS", "EXTERNLOGIN", "FETCH", "FIRST", "FLOAT",
		                                       		"FOR", "FORCE",
		                                       		"FOREIGN", "FORWARD", "FROM", "FULL", "GOTO", "GRANT", "GROUP", "HAVING", "HOLDLOCK",
		                                       		"IDENTIFIED", "IF", "IN",
		                                       		"INDEX", "INDEX_LPAREN", "INNER", "INOUT", "INSENSITIVE", "INSERT", "INSERTING", "INSTALL",
		                                       		"INSTEAD", "INT",
		                                       		"INTEGER", "INTEGRATED", "INTERSECT", "INTO", "IQ", "IS", "ISOLATION", "JOIN", "KEY", "LATERAL",
		                                       		"LEFT", "LIKE",
		                                       		"LOCK", "LOGIN", "LONG", "MATCH", "MEMBERSHIP", "MESSAGE", "MODE", "MODIFY", "NATURAL", "NEW",
		                                       		"NO",
		                                       		"NOHOLDLOCK", "NOT", "NOTIFY", "NULL", "NUMERIC", "OF", "OFF", "ON", "OPEN", "OPTION", "OPTIONS",
		                                       		"OR", "ORDER",
		                                       		"OTHERS", "OUT", "OUTER", "OVER", "PASSTHROUGH", "PRECISION", "PREPARE", "PRIMARY", "PRINT",
		                                       		"PRIVILEGES",
		                                       		"PROC", "PROCEDURE", "PUBLICATION", "RAISERROR", "READTEXT", "REAL", "REFERENCE", "REFERENCES",
		                                       		"RELEASE",
		                                       		"REMOTE", "REMOVE", "RENAME", "REORGANIZE", "RESOURCE", "RESTORE", "RESTRICT", "RETURN", "REVOKE"
		                                       		, "RIGHT",
		                                       		"ROLLBACK", "ROLLUP", "SAVE", "SAVEPOINT", "SCROLL", "SELECT", "SENSITIVE", "SESSION", "SET",
		                                       		"SETUSER", "SHARE",
		                                       		"SMALLINT", "SOME", "SQLCODE", "SQLSTATE", "START", "STOP", "SUBTRANS", "SUBTRANSACTION",
		                                       		"SYNCHRONIZE",
		                                       		"SYNTAX_ERROR", "TABLE", "TEMPORARY", "THEN", "TIME", "TIMESTAMP", "TINYINT", "TO", "TOP", "TRAN"
		                                       		, "TRIGGER",
		                                       		"TRUNCATE", "TSEQUAL", "UNBOUNDED", "UNION", "UNIQUE", "UNKNOWN", "UNSIGNED", "UPDATE",
		                                       		"UPDATING",
		                                       		"USER", "USING", "VALIDATE", "VALUES", "VARBINARY", "VARCHAR", "VARIABLE", "VARYING", "VIEW",
		                                       		"WAIT", "WAITFOR",
		                                       		"WHEN", "WHERE", "WHILE", "WINDOW", "WITH", "WITH_CUBE", "WITH_LPAREN", "WITH_ROLLUP", "WITHIN",
		                                       		"WORK", "WRITETEXT"
		                                       	};

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override long GetDbType( Type type )
		{
			AsaDbType result = AsaDbType.Integer;
			if( type.Equals( typeof(decimal) ) )
			{
				result = AsaDbType.Decimal;
			}
			else if( type.Equals( typeof(Int16) ) )
			{
				result = AsaDbType.SmallInt;
			}
			else if( type.Equals( typeof(Int32) ) || type.IsEnum )
			{
				result = AsaDbType.Integer;
			}
			else if( type.Equals( typeof(long) ) )
			{
				result = AsaDbType.BigInt;
			}
			else if( type.Equals( typeof(double) ) )
			{
				result = AsaDbType.Double;
			}
			else if( type.Equals( typeof(DateTime) ) )
			{
				result = AsaDbType.DateTime;
			}
			else if( type.Equals( typeof(bool) ) )
			{
				result = AsaDbType.Bit;
			}
			else if( type.Equals( typeof(string) ) )
			{
				result = AsaDbType.Text;
			}
			else if( type.Equals( typeof(Guid) ) )
			{
				result = AsaDbType.UniqueIdentifierStr;
			}
			else
			{
				result = (AsaDbType) NO_DBTYPE;
				Check.Fail( Error.UnsupportedPropertyType, type.Name, this );
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
			switch( dbType.ToLower() )
			{
				case "bit":
					return (long) AsaDbType.Bit;
				case "tinyint":
					return (long) AsaDbType.TinyInt;
				case "smallint":
					return (long) AsaDbType.SmallInt;
				case "int":
				case "integer":
					return (long) AsaDbType.Integer;
				case "bigint":
					return (long) AsaDbType.BigInt;
				case "real":
					return (long) AsaDbType.Real;
				case "float":
					return (long) AsaDbType.Float;
				case "smalldatetime":
					return (long) AsaDbType.SmallDateTime;
				case "datetime":
					return (long) AsaDbType.DateTime;
				case "money":
				case "decimal":
				case "numeric":
					return (long) AsaDbType.Decimal;
				case "char":
					return (long) AsaDbType.Char;
				case "varchar":
					return (long) AsaDbType.VarChar;
				case "text":
					return (long) AsaDbType.Text;
				case "timestamp":
					return (long) AsaDbType.TimeStamp;
				case "uniqueidentifier":
					return (long) AsaDbType.UniqueIdentifier;
				case "uniqueidentifierstr":
					return (long) AsaDbType.UniqueIdentifierStr;
				case "long varchar":
					return (long) AsaDbType.LongVarchar;
				default:
					Check.Fail( Error.UnsupportedColumnType, dbType, this );
					return NO_DBTYPE;
			}
		}

		public override Type GetSystemType( long dbType )
		{
			AsaDbType asaDbType = (AsaDbType) Enum.ToObject( typeof(AsaDbType), dbType );
			switch( asaDbType )
			{
				case AsaDbType.BigInt:
					return typeof(long);
				case AsaDbType.Binary:
					return typeof(byte[]);
				case AsaDbType.Bit:
					return typeof(bool);
				case AsaDbType.Char:
					return typeof(char);
				case AsaDbType.Date:
				case AsaDbType.DateTime:
					return typeof(DateTime);
				case AsaDbType.Decimal:
					return typeof(decimal);
				case AsaDbType.Double:
				case AsaDbType.Float:
					return typeof(double);
				case AsaDbType.Image:
					return typeof(byte[]);
				case AsaDbType.Integer:
					return typeof(int);
				case AsaDbType.LongBinary:
					return typeof(byte[]);
				case AsaDbType.LongVarchar:
					return typeof(string);
				case AsaDbType.Money:
					return typeof(decimal);
				case AsaDbType.Numeric:
					return typeof(decimal);
				case AsaDbType.OldBit:
					return typeof(bool);
				case AsaDbType.Real:
					return typeof(decimal);
				case AsaDbType.SmallDateTime:
					return typeof(DateTime);
				case AsaDbType.SmallInt:
					return typeof(int);
				case AsaDbType.SmallMoney:
					return typeof(decimal);
				case AsaDbType.SysName:
					return typeof(string);
				case AsaDbType.Text:
					return typeof(string);
				case AsaDbType.Time:
				case AsaDbType.TimeStamp:
					return typeof(DateTime);
				case AsaDbType.TinyInt:
					return typeof(int);
				case AsaDbType.UniqueIdentifier:
				case AsaDbType.UniqueIdentifierStr:
					return typeof(Guid);
				case AsaDbType.UnsignedBigInt:
					return typeof(long);
				case AsaDbType.UnsignedInt:
					return typeof(long);
				case AsaDbType.UnsignedSmallInt:
					return typeof(long);
				case AsaDbType.VarBinary:
					return typeof(byte[]);
				case AsaDbType.VarChar:
					return typeof(string);
				default:
					return typeof(object);
			}
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override string GetParameterPrefix()
		{
			return "@";
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override string GetParameterSuffix()
		{
			return "";
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
		public override string GetIdentitySelect( string sql, ObjectMap om )
		{
			//Jet does not support more than one SQL statement
			//To be executed at the same time
			return "SELECT @@IDENTITY";
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details.
		/// Sybase ASA Data Provider does not support BatchCommandProcessing
		/// </summary>
		public override Capability Capabilities
		{
			get { return Capability.Paging; }
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override void AddParameter( IDbCommand cmd, string name, long dbType )
		{
			try
			{
				StringBuilder sb = new StringBuilder();
				sb.Append( GetParameterPrefix() );
				sb.Append( name );
				sb.Append( GetParameterSuffix() );

				AsaCommand asac = (AsaCommand) cmd;
				AsaParameter param = new AsaParameter( sb.ToString(), (AsaDbType) dbType );
				param.Direction = ParameterDirection.Input;
				asac.Parameters.Add( param );
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
	}
}