/*
 * Sybase ASA specifics
 * Copyright (C) 2004 Uwe Kitzmann
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: SybaseFactory.cs 1232 2008-03-14 05:36:00Z mm $
 */
//using iAnywhere.Data.AsaClient; 

using System;
using System.Data;
using Gentle.Common;
using Gentle.Framework;
using Mono.Data.SybaseClient;

namespace Gentle.Provider.Sybase
{
	/// <summary>
	/// This class is an implementation of the <see cref="GentleSqlFactory"/> class for the 
	/// Sybase Asa Database.
	/// </summary>
	public class SybaseFactory : GentleSqlFactory
	{
		public SybaseFactory( IGentleProvider provider ) : base( provider )
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
			SybaseType result = SybaseType.Int;

			if( type.Equals( typeof(decimal) ) )
			{
				result = SybaseType.Decimal;
			}
			else if( type.Equals( typeof(int) ) )
			{
				result = SybaseType.Int;
			}
			else if( type.Equals( typeof(long) ) )
			{
				result = SybaseType.BigInt;
			}
			else if( type.Equals( typeof(double) ) )
			{
				result = SybaseType.Decimal;
			}
			else if( type.Equals( typeof(DateTime) ) )
			{
				result = SybaseType.DateTime;
			}
			else if( type.Equals( typeof(bool) ) )
			{
				result = SybaseType.Bit;
			}
			else if( type.Equals( typeof(string) ) )
			{
				result = SybaseType.Text;
			}
			else if( type.Equals( typeof(Guid) ) )
			{
				result = SybaseType.UniqueIdentifier;
			}
			else
			{
				Check.Fail( Error.UnsupportedPropertyType, type.Name, provider.Name );
			}
			return (long) result;

			//			AsaDbType result = AsaDbType.Integer;
			//			if( type.Equals(typeof(decimal) ) )
			//				result = AsaDbType.Decimal; 
			//			else if (type.Equals( typeof(int)))
			//				 result = AsaDbType.Integer;
			//			else if( type.Equals( typeof(long) ) )
			//				result = AsaDbType.BigInt; 
			//			else if( type.Equals( typeof(double) ) )
			//				result = AsaDbType.Double;
			//			else if( type.Equals( typeof(DateTime) ) )
			//				result = AsaDbType.DateTime;
			//			else if( type.Equals( typeof(bool) ) )
			//				result = AsaDbType.Bit;
			//			else if( type.Equals( typeof(string) ) )
			//				result = AsaDbType.Text;
			//			else if( type.Equals( typeof(Guid) ) )
			//				result = AsaDbType.UniqueIdentifierStr;
			//			else 
			//				Check.Fail( Error.UnsupportedPropertyType, type.Name, this );
			//			return (long) result;
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
				case "bit": // 1
					return (long) SybaseType.Bit;
				case "tinyint": // 1
					return (long) SybaseType.TinyInt;
				case "smallint": // 2
					return (long) SybaseType.SmallInt;
				case "int": // 4
				case "integer":
					return (long) SybaseType.Int;
				case "bigint": // 8
					return (long) SybaseType.BigInt;
				case "real": // 4
					return (long) SybaseType.Real;
				case "float": // 8
					return (long) SybaseType.Float;
				case "smalldatetime": // 4
					return (long) SybaseType.SmallDateTime;
				case "datetime": // 8
					return (long) SybaseType.DateTime;
				case "money":
				case "decimal":
				case "numeric":
					return (long) SybaseType.Decimal;
					// TODO: Check unicode data types
					//				case "nchar": // unicode (max 4000 chars)
					//					return (long) AsaDbType.NChar;
					//				case "nvarchar":  // unicode (max 4000 chars)
					//					return (long) AsaDbType.NVarChar;
					//				case "ntext": // unicode (max 2^32 chars)
					//					return (long) AsaDbType.NText;
				case "char": // non-unicode (max 8000 chars)
					return (long) SybaseType.Char;
				case "varchar": // non-unicode (max 8000 chars)
					return (long) SybaseType.VarChar;
				case "text": // non-unicode (max 2^32 chars)
					return (long) SybaseType.Text;
				case "timestamp": // 8
					return (long) SybaseType.Timestamp;
				case "uniqueidentifier":
					return (long) SybaseType.UniqueIdentifier;
				case "uniqueidentifierstr":
					return (long) SybaseType.UniqueIdentifier;
				case "long varchar":
					return (long) SybaseType.Variant;
				default:
					Check.Fail( Error.UnsupportedColumnType, dbType, provider.Name );
					return NO_DBTYPE;
					//				default:
					//					System.Diagnostics.Debug.WriteLine(dbType);
					//					return NO_DBTYPE;
			}

			//			switch( dbType )
			//			{
			//				case "bit": // 1
			//					return (long) AsaDbType.Bit;
			//				case "tinyint": // 1
			//					return (long) AsaDbType.TinyInt;
			//				case "smallint": // 2
			//					return (long) AsaDbType.SmallInt;
			//				case "int": // 4
			//				case "integer":
			//					return (long) AsaDbType.Integer;
			//				case "bigint": // 8
			//					return (long) AsaDbType.BigInt;
			//				case "real": // 4
			//					return (long) AsaDbType.Real;
			//				case "float": // 8
			//					return (long) AsaDbType.Float;
			//				case "smalldatetime": // 4
			//					return (long) AsaDbType.SmallDateTime;
			//				case "datetime": // 8
			//					return (long) AsaDbType.DateTime;
			//				case "money":
			//				case "decimal":
			//				case "numeric":
			//					return (long) AsaDbType.Decimal;
			// TODO: Check unicode data types
			//				case "nchar": // unicode (max 4000 chars)
			//					return (long) AsaDbType.NChar;
			//				case "nvarchar":  // unicode (max 4000 chars)
			//					return (long) AsaDbType.NVarChar;
			//				case "ntext": // unicode (max 2^32 chars)
			//					return (long) AsaDbType.NText;
			//				case "char": // non-unicode (max 8000 chars)
			//					return (long) AsaDbType.Char;
			//				case "varchar": // non-unicode (max 8000 chars)
			//					return (long) AsaDbType.VarChar;
			//				case "text": // non-unicode (max 2^32 chars)
			//					return (long) AsaDbType.Text;
			//				case "timestamp": // 8
			//					return (long) AsaDbType.TimeStamp;
			//				case "uniqueidentifier":
			//					return (long) AsaDbType.UniqueIdentifier;
			//				case "uniqueidentifierstr":
			//					return (long) AsaDbType.UniqueIdentifierStr;
			//				case "long varchar":
			//					return (long) AsaDbType.LongVarchar;
			//				default:
			//					Check.Fail( Error.UnsupportedColumnType, dbType, this );
			//					return NO_DBTYPE;
			//				default:
			//					System.Diagnostics.Debug.WriteLine(dbType);
			//					return NO_DBTYPE;
			//			}
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
		public bool SupportsBatchCommandProcessing
		{
			get { return false; }
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override void AddParameter( IDbCommand cmd, string name, long dbType )
		{
			try
			{
				SybaseCommand sc = (SybaseCommand) cmd;
				//AsaCommand sc = (AsaCommand) cmd;
				//place suffix and prefix around parameter
				sc.Parameters.Add( GetParameterPrefix() + name + GetParameterSuffix(),
				                   (SybaseType) dbType );
				//(AsaDbType) dbType );
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