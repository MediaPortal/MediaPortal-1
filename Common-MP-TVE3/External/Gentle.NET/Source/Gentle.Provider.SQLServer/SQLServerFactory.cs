/*
 * MS SQL Server specifics
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: SQLServerFactory.cs 1241 2008-04-21 14:49:35Z mm $
 */

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using Gentle.Common;
using Gentle.Framework;

namespace Gentle.Provider.SQLServer
{
	/// <summary>
	/// This class is an implementation of the <see cref="GentleSqlFactory"/> class for the 
	/// Microsoft SQL Server RDBMS.
	/// </summary>
	public class SQLServerFactory : GentleSqlFactory
	{
		public SQLServerFactory( IGentleProvider provider ) : base( provider )
		{
		}

		/// <summary>
		/// Container class to test for existence of reserved words.
		/// </summary>
		private class ReservedWords
		{
			/// <summary>
			/// Create a new <see cref="ReservedWords"/> instance and fill
			/// internal <see cref="Hashtable"/> with reserved words for fast
			/// retrieval.
			/// </summary>
			public ReservedWords()
			{
				// Create hashtable
				reservedWordsHashtable = new Hashtable(
					// Initialize the list to contain the number of elements in the list
					reservedWordsList.Length,
					// Do case-insensitive search/hashing
					StringComparer.InvariantCultureIgnoreCase );
				// Fill values
				foreach( string reservedWord in reservedWordsList )
				{
					reservedWordsHashtable.Add( reservedWord, true );
				}
			}

			/// <summary>
			/// Determine if the specified token is a reserved word in the current
			/// implementation. Works case insensitive.
			/// </summary>
			/// <param name="Token">The token that should be checked.</param>
			/// <returns><see langword="true"/> if the specified token is a reseved
			/// word, <see langword="false"/> if not.</returns>
			/// <example>
			/// The following call returns <see langword="false"/>:
			/// <code>IsReservedWord("FIRSTNAME");</code>
			/// The following call returns <see langword="true"/>:
			/// <code>IsReservedWord("VARCHAR");</code>
			///</example>
			public bool IsReservedWord( string Token )
			{
				return reservedWordsHashtable.ContainsKey( Token );
			}

			/// <summary>
			/// <see cref="Hashtable"/> containing all reserved words for fast lookup.
			/// </summary>
			private Hashtable reservedWordsHashtable;

			// Sources:
			// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/tsqlref/ts_ra-rz_9oj7.asp
			// http://www.ssw.com.au/SSW/kb/KB.aspx?KBID=Q931371
			private string[] reservedWordsList = new[]
			                                     	{
			                                     		"ABSOLUTE", "ACTION", "ADA", "ADD", "ADMIN", "AFTER", "AGGREGATE", "ALIAS", "ALL",
			                                     		"ALLOCATE", "ALTER", "AND", "ANY", "ARE", "ARRAY", "AS", "ASC", "ASSERTION", "AT",
			                                     		"AUTHORIZATION", "AVG", "BACKUP", "BEFORE", "BEGIN", "BETWEEN", "BINARY", "BIT",
			                                     		"BIT_LENGTH", "BLOB", "BOOLEAN", "BOTH", "BREADTH", "BREAK", "BROWSE", "BULK", "BY",
			                                     		"CALL", "CASCADE", "CASCADED", "CASE", "CAST", "CATALOG", "CHAR", "CHARACTER",
			                                     		"CHARACTER_LENGTH", "CHAR_LENGTH", "CHECK", "CHECKPOINT", "CLASS", "CLOB", "CLOSE",
			                                     		"CLUSTERED", "COALESCE", "COLLATE", "COLLATION", "COLUMN", "COMMIT", "COMPLETION",
			                                     		"COMPUTE", "CONNECT", "CONNECTION", "CONSTRAINT", "CONSTRAINTS", "CONSTRUCTOR",
			                                     		"CONTAINS", "CONTAINSTABLE", "CONTINUE", "CONVERT", "CORRESPONDING", "COUNT",
			                                     		"CREATE", "CROSS", "CUBE", "CURRENT", "CURRENT_DATE", "CURRENT_PATH", "CURRENT_ROLE",
			                                     		"CURRENT_TIME", "CURRENT_TIMESTAMP", "CURRENT_USER", "CURSOR", "CYCLE", "DATA",
			                                     		"DATABASE", "DATE", "DAY", "DBCC", "DEALLOCATE", "DEC", "DECIMAL", "DECLARE",
			                                     		"DEFAULT", "DEFERRABLE", "DEFERRED", "DELETE", "DENY", "DEPTH", "DEREF", "DESC",
			                                     		"DESCRIBE", "DESCRIPTOR", "DESTROY", "DESTRUCTOR", "DETERMINISTIC", "DIAGNOSTICS",
			                                     		"DICTIONARY", "DISCONNECT", "DISK", "DISTINCT", "DISTRIBUTED", "DOMAIN", "DOUBLE",
			                                     		"DROP", "DUMMY", "DUMP", "DYNAMIC", "EACH", "ELSE", "END", "END-EXEC", "EQUALS",
			                                     		"ERRLVL", "ESCAPE", "EVERY", "EXCEPT", "EXCEPTION", "EXEC", "EXECUTE", "EXISTS",
			                                     		"EXIT", "EXTERNAL", "EXTRACT", "FALSE", "FETCH", "FILE", "FILLFACTOR", "FIRST",
			                                     		"FLOAT", "FOR", "FOREIGN", "FORTRAN", "FOUND", "FREE", "FREETEXT", "FREETEXTTABLE",
			                                     		"FROM", "FULL", "FUNCTION", "GENERAL", "GET", "GLOBAL", "GO", "GOTO", "GRANT",
			                                     		"GROUP", "GROUPING", "HAVING", "HOLDLOCK", "HOST", "HOUR", "IDENTITY", "IDENTITYCOL",
			                                     		"IDENTITY_INSERT", "IF", "IGNORE", "IMMEDIATE", "IN", "INCLUDE", "INDEX", "INDICATOR",
			                                     		"INITIALIZE", "INITIALLY", "INNER", "INOUT", "INPUT", "INSENSITIVE", "INSERT", "INT",
			                                     		"INTEGER", "INTERSECT", "INTERVAL", "INTO", "IS", "ISOLATION", "ITERATE", "JOIN",
			                                     		"KEY", "KILL", "LANGUAGE", "LARGE", "LAST", "LATERAL", "LEADING", "LEFT", "LESS",
			                                     		"LEVEL", "LIKE", "LIMIT", "LINENO", "LOAD", "LOCAL", "LOCALTIME", "LOCALTIMESTAMP",
			                                     		"LOCATOR", "LOWER", "MAP", "MATCH", "MAX", "MIN", "MINUTE", "MODIFIES", "MODIFY",
			                                     		"MODULE", "MONTH", "NAMES", "NATIONAL", "NATURAL", "NCHAR", "NCLOB", "NEW", "NEXT",
			                                     		"NO", "NOCHECK", "NONCLUSTERED", "NONE", "NOT", "NULL", "NULLIF", "NUMERIC", "OBJECT",
			                                     		"OCTET_LENGTH", "OF", "OFF", "OFFSETS", "OLD", "ON", "ONLY", "OPEN", "OPENDATASOURCE",
			                                     		"OPENQUERY", "OPENROWSET", "OPENXML", "OPERATION", "OPTION", "OR", "ORDER",
			                                     		"ORDINALITY", "OUT", "OUTER", "OUTPUT", "OVER", "OVERLAPS", "PAD", "PARAMETER",
			                                     		"PARAMETERS", "PARTIAL", "PASCAL", "PATH", "PERCENT", "PLAN", "POSITION", "POSTFIX",
			                                     		"PRECISION", "PREFIX", "PREORDER", "PREPARE", "PRESERVE", "PRIMARY", "PRINT", "PRIOR",
			                                     		"PRIVILEGES", "PROC", "PROCEDURE", "PUBLIC", "RAISERROR", "READ", "READS", "READTEXT",
			                                     		"REAL", "RECONFIGURE", "RECURSIVE", "REF", "REFERENCES", "REFERENCING", "RELATIVE",
			                                     		"REPLICATION", "RESTORE", "RESTRICT", "RESULT", "RETURN", "RETURNS", "REVOKE",
			                                     		"RIGHT", "ROLE", "ROLLBACK", "ROLLUP", "ROUTINE", "ROW", "ROWCOUNT", "ROWGUIDCOL",
			                                     		"ROWS", "RULE", "SAVE", "SAVEPOINT", "SCHEMA", "SCOPE", "SCROLL", "SEARCH", "SECOND",
			                                     		"SECTION", "SELECT", "SEQUENCE", "SESSION", "SESSION_USER", "SET", "SETS", "SETUSER",
			                                     		"SHUTDOWN", "SIZE", "SMALLINT", "SOME", "SPACE", "SPECIFIC", "SPECIFICTYPE", "SQL",
			                                     		"SQLCA", "SQLCODE", "SQLERROR", "SQLEXCEPTION", "SQLSTATE", "SQLWARNING", "START",
			                                     		"STATE", "STATEMENT", "STATIC", "STATISTICS", "STRUCTURE", "SUBSTRING", "SUM",
			                                     		"SYSTEM_USER", "TABLE", "TEMPORARY", "TERMINATE", "TEXTSIZE", "THAN", "THEN", "TIME",
			                                     		"TIMESTAMP", "TIMEZONE_HOUR", "TIMEZONE_MINUTE", "TO", "TOP", "TRAILING", "TRAN",
			                                     		"TRANSACTION", "TRANSLATE", "TRANSLATION", "TREAT", "TRIGGER", "TRIM", "TRUE",
			                                     		"TRUNCATE", "TSEQUAL", "UNDER", "UNION", "UNIQUE", "UNKNOWN", "UNNEST", "UPDATE",
			                                     		"UPDATETEXT", "UPPER", "USAGE", "USE", "USER", "USING", "VALUE", "VALUES", "VARCHAR",
			                                     		"VARIABLE", "VARYING", "VIEW", "WAITFOR", "WHEN", "WHENEVER", "WHERE", "WHILE",
			                                     		"WITH", "WITHOUT", "WORK", "WRITE", "WRITETEXT", "YEAR", "ZONE"
			                                     	};
		}

		/// <summary>
		/// Static singleton instance for access to reserved words collection.
		/// </summary>
		private static ReservedWords reservedWords = new ReservedWords();

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override long GetDbType( Type type )
		{
			SqlDbType result = SqlDbType.Int;
			if( type.IsEnum )
			{
				type = Enum.GetUnderlyingType( type );
			}
			if( type.Equals( typeof(short) ) || type.Equals( typeof(ushort) ) )
			{
				result = SqlDbType.SmallInt;
			}
			else if( type.Equals( typeof(int) ) || type.Equals( typeof(uint) ) )
			{
				result = SqlDbType.Int;
			}
			else if( type.Equals( typeof(long) ) || type.Equals( typeof(ulong) ) )
			{
				result = SqlDbType.BigInt;
			}
			else if( type.Equals( typeof(Single) ) )
			{
				result = SqlDbType.Real;
			}
			else if( type.Equals( typeof(double) ) )
			{
				result = SqlDbType.Float;
			}
			else if( type.Equals( typeof(DateTime) ) )
			{
				result = SqlDbType.DateTime;
			}
			else if( type.Equals( typeof(bool) ) )
			{
				result = SqlDbType.Bit;
			}
			else if( type.Equals( typeof(char) ) )
			{
				result = SqlDbType.Char;
			}
			else if( type.Equals( typeof(byte) ) )
			{
				result = SqlDbType.TinyInt;
			}
			else if( type.Equals( typeof(string) ) )
			{
				result = SqlDbType.NVarChar; // was: Text
			}
			else if( type.Equals( typeof(decimal) ) )
			{
				result = SqlDbType.Decimal;
			}
			else if( type.Equals( typeof(byte[]) ) )
			{
				result = SqlDbType.Image;
			}
			else if( type.Equals( typeof(Guid) ) )
			{
				result = SqlDbType.UniqueIdentifier;
			}
			else
			{
				Check.Fail( Error.UnsupportedPropertyType, type.Name, GetType().Name );
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
				case "bit": // 1
					return (long) SqlDbType.Bit;
				case "tinyint": // 1
					return (long) SqlDbType.TinyInt;
				case "smallint": // 2
					return (long) SqlDbType.SmallInt;
				case "int": // 4
					return (long) SqlDbType.Int;
				case "bigint": // 8
					return (long) SqlDbType.BigInt;
				case "real": // 4
					return (long) SqlDbType.Real;
				case "float": // 8
					return (long) SqlDbType.Float;
				case "smalldatetime": // 4
					return (long) SqlDbType.SmallDateTime;
				case "datetime": // 8
					return (long) SqlDbType.DateTime;
				case "decimal":
				case "numeric":
					return (long) SqlDbType.Decimal;
				case "nchar": // unicode (max 4000 chars)
					return (long) SqlDbType.NChar;
				case "nvarchar": // unicode (max 4000 chars)
					return (long) SqlDbType.NVarChar;
				case "ntext": // unicode (max 2^32 chars)
					return (long) SqlDbType.NText;
				case "char": // non-unicode (max 8000 chars)
					return (long) SqlDbType.Char;
				case "varchar": // non-unicode (max 8000 chars)
					return (long) SqlDbType.VarChar;
				case "text": // non-unicode (max 2^32 chars)
					return (long) SqlDbType.Text;
				case "binary": // fixed size blob < 8000 bytes
					return (long) SqlDbType.Binary;
				case "varbinary": // variable size blob < 8000 bytes 
					return (long) SqlDbType.VarBinary;
				case "image": // 2^31 blob data
					return (long) SqlDbType.Image;
				case "uniqueidentifier": // GUID
					return (long) SqlDbType.UniqueIdentifier;
					// weird type sql server seems to use when a table has been modified.. we just
					// return NO_DBTYPE and ignore the returned value (the column has another,
					// proper entry so we'll be called again for the same column).
				case "sysname":
					return NO_DBTYPE;
					// fail for unsupported types (includes timestamp because its a read-only type)
				case "timestamp": // 8
					// return (long) SqlDbType.Timestamp;
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
		public override Type GetSystemType( long dbType )
		{
			SqlDbType sqlDbType = (SqlDbType) Enum.ToObject( typeof(SqlDbType), dbType );
			switch( sqlDbType )
			{
				case SqlDbType.BigInt:
					return typeof(long);
				case SqlDbType.Binary:
					return typeof(byte[]);
				case SqlDbType.Bit:
					return typeof(bool);
				case SqlDbType.Char:
					return typeof(char);
				case SqlDbType.DateTime:
					return typeof(DateTime);
				case SqlDbType.Decimal:
					return typeof(decimal);
				case SqlDbType.Float:
					return typeof(double);
				case SqlDbType.Image:
					return typeof(byte[]);
				case SqlDbType.Int:
					return typeof(int);
				case SqlDbType.Money:
					return typeof(object); // TODO
				case SqlDbType.NChar:
					return typeof(string);
				case SqlDbType.NText:
					return typeof(string);
				case SqlDbType.NVarChar:
					return typeof(string);
				case SqlDbType.Real:
					return typeof(double);
				case SqlDbType.SmallDateTime:
					return typeof(DateTime);
				case SqlDbType.SmallInt:
					return typeof(short);
				case SqlDbType.SmallMoney:
					return typeof(object); // TODO
				case SqlDbType.Text:
					return typeof(string);
				case SqlDbType.Timestamp:
					return typeof(DateTime);
				case SqlDbType.TinyInt:
					return typeof(byte);
				case SqlDbType.UniqueIdentifier:
					return typeof(Guid);
				case SqlDbType.VarBinary:
					return typeof(byte[]);
				case SqlDbType.VarChar:
					return typeof(string);
				case SqlDbType.Variant:
					return typeof(object);
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
		public override char GetQuoteCharacter()
		{
			return '\'';
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override string GetIdentitySelect( string sql, ObjectMap om )
		{
			if( provider.ProviderInformation.Version.Major >= 8 )
			{
				return string.Format( "{0}{1} select SCOPE_IDENTITY()", sql, GetStatementTerminator() );
			}
			else
			{
				return string.Format( "{0}{1} select @@IDENTITY", sql, GetStatementTerminator() );
			}
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override void AddParameter( IDbCommand cmd, string name, long dbType )
		{
			try
			{
				SqlCommand sc = (SqlCommand) cmd;
				// prefix parameters with @ for SQL Server
				sc.Parameters.Add( GetParameterPrefix() + name, (SqlDbType) dbType );
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
		/// <param name="word">The word that should be checked for.</param>
		/// <returns><see langword="true"/> if the word is reserved.</returns>
		public override bool IsReservedWord( string word )
		{
			Check.VerifyNotNull( word, Error.NullParameter, "word" );

			// Trim whitespaces
			word = word.Trim();

			// Tokens should not contain spaces or special characters
			// TODO: Add other characters not allowed (,;.:) (do they get
			// filtered before?)
			if( word.IndexOfAny( new[] { ' ', '-' } ) >= 0 )
			{
				return true;
			}

			// Lookup word in the list of reserved words
			if( reservedWords.IsReservedWord( word ) )
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Obtain a quoted version of the reserved word to allow the reserved word to be 
		/// used in queries anyway. If a reserved word cannot be quoted this method should
		/// raise an error informing the user that they need to pick a different name.
		/// </summary>
		/// <param name="word">The word that should be quoted to avoid errors.</param>
		/// <returns>The given reserved word or field quoted to avoid errors.</returns>
		public override string QuoteReservedWord( string word )
		{
			return String.Format( "{0}{1}{2}", "[", word, "]" );
		}
	}
}