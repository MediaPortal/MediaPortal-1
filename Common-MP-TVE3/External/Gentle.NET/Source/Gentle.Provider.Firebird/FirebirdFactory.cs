/*
 * Firebird specifics
 * Copyright (C) 2004 Carlos Guzmán Álvarez
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: FirebirdFactory.cs 1234 2008-03-14 11:41:44Z mm $
 */
using System;
using System.Collections;
using System.Data;
using FirebirdSql.Data.FirebirdClient;
using Gentle.Common;
using Gentle.Framework;

namespace Gentle.Provider.Firebird
{
	/// <summary>
	/// This class is an implementation of the <see cref="GentleSqlFactory"/> class for the Firebird RDBMS.
	/// </summary>
	public class FirebirdFactory : GentleSqlFactory
	{
		private static string schemaName = null;

		///	<summary>
		///	Container	class	to test	for	existence	of reserved	words.
		///	</summary>
		private class ReservedWords
		{
			///	<summary>
			///	Create a new <see	cref="ReservedWords"/> instance	and	fill
			///	internal <see	cref="Hashtable"/> with	reserved words for fast
			///	retrieval.
			///	</summary>
			public ReservedWords()
			{
				// Create	hashtable
				reservedWordsHashtable = new Hashtable(
					// Initialize	the	list to	contain	the	number of	elements in	the	list
					reservedWordsList.Length,
					// Do	case-insensitive search/hashing
					StringComparer.InvariantCultureIgnoreCase );
				// Fill	values
				foreach( string	reservedWord in	reservedWordsList )
				{
					reservedWordsHashtable.Add( reservedWord, true );
				}
			}

			///	<summary>
			///	Determine	if the specified token is	a	reserved word	in the current
			///	implementation.	Works	case insensitive.
			///	</summary>
			///	<param name="Token">The	token	that should	be checked.</param>
			///	<returns><see	langword="true"/>	if the specified token is	a	reserved
			///	word,	<see langword="false"/>	if not.</returns>
			///	<example>
			///	The	following	call returns <see	langword="false"/>:
			///	<code>IsReservedWord("FIRSTNAME");</code>
			///	The	following	call returns <see	langword="true"/>:
			///	<code>IsReservedWord("VARCHAR");</code>
			///</example>
			public bool IsReservedWord( string Token )
			{
				return reservedWordsHashtable.ContainsKey( Token );
			}

			///	<summary>
			///	<see cref="Hashtable"/>	containing all reserved	words	for	fast lookup.
			///	</summary>
			private Hashtable reservedWordsHashtable;

			private string[] reservedWordsList = new[]
			                                     	{
			                                     		"ABS", "ACTION", "ACTIVE", "ADD", "ADMIN", "AFTER", "ALL", "ALTER",
			                                     		"AND", "ANY", "AS", "ASC", "ASCENDING", "AT", "AUTO", "AUTODDL",
			                                     		"AVG", "BASED", "BASENAME", "BASE_NAME", "BEFORE", "BEGIN",
			                                     		"BETWEEN", "BIGINT", "BLOB", "BLOBEDIT", "BOOLEAN", "BOTH", "BREAK",
			                                     		"BUFFER", "BY", "CACHE", "CASCADE", "CASE", "CAST", "CHAR",
			                                     		"CHAR_LENGTH", "CHARACTER", "CHARACTER_LENGTH",
			                                     		"CHECK", "CHECK_POINT_LEN", "CHECK_POINT_LENGTH", "COALESCE",
			                                     		"COLLATE", "COLLATION", "COLUMN", "COMMIT", "COMMITTED",
			                                     		"COMPILETIME", "COMPUTED", "CLOSE", "CONDITIONAL", "CONNECT",
			                                     		"CONSTRAINT", "CONTAINING", "CONTINUE", "COUNT", "CREATE", "CSTRING",
			                                     		"CURRENT", "CURRENT_CONNECTION", "CURRENT_ROLE",
			                                     		"CURRENT_TRANSACTION", "CURRENT_USER", "CURSOR", "DATABASE", "DATE",
			                                     		"DB_KEY", "DEBUG", "DEC", "DECIMAL", "DECLARE", "DEFAULT", "DELETE",
			                                     		"DELETING", "DESC", "DESCENDING", "DESCRIBE", "DESCRIPTOR",
			                                     		"DISCONNECT", "DISPLAY", "DISTINCT", "DO", "DOMAIN", "DOUBLE",
			                                     		"DROP", "ECHO", "EDIT", "ELSE", "END", "ENTRY_POINT", "ESCAPE",
			                                     		"EVENT", "EXCEPTION", "EXECUTE", "EXISTS", "EXIT", "EXTERN",
			                                     		"EXTERNAL", "EXTRACT", "FALSE", "FETCH", "FILE", "FILTER", "FIRST",
			                                     		"FLOAT", "FOR", "FOREIGN", "FOUND", "FREE_IT", "FROM", "FULL",
			                                     		"FUNCTION", "GDSCODE", "GENERATOR", "GEN_ID", "GLOBAL", "GOTO",
			                                     		"GRANT", "GROUP", "GROUP_COMMIT_WAIT", "GROUP_COMMIT_WAIT_TIME",
			                                     		"HAVING", "HELP", "IF", "IIF", "IMMEDIATE", "IN", "INACTIVE",
			                                     		"INDEX", "INDICATOR", "INIT", "INNER", "INPUT", "INPUT_TYPE",
			                                     		"INSERT", "INSERTING", "INT", "INTEGER", "INTO", "IS", "ISOLATION",
			                                     		"ISQL", "JOIN", "KEY", "LAST", "LC_MESSAGES", "LC_TYPE", "LEADING",
			                                     		"LEAVE", "LEFT", "LENGTH", "LEV", "LEVEL", "LIKE", "LOCK",
			                                     		"LOGFILE", "LOG_BUFFER_SIZE", "LOG_BUF_SIZE", "LONG", "MANUAL",
			                                     		"MAX", "MAXIMUM", "MAXIMUM_SEGMENT", "MAX_SEGMENT", "MERGE",
			                                     		"MESSAGE", "MIN", "MINIMUM", "MODULE_NAME", "NAMES", "NATIONAL",
			                                     		"NATURAL", "NCHAR", "NO", "NOAUTO", "NOT", "NULL", "NULLIF", "NULLS",
			                                     		"NUMERIC", "NUM_LOG_BUFS", "NUM_LOG_BUFFERS", "OCTET_LENGTH", "OF",
			                                     		"ON", "ONLY", "OPEN", "OPTION", "OR", "ORDER", "OUTER", "OUTPUT",
			                                     		"OUTPUT_TYPE", "OVERFLOW", "PAGE", "PAGELENGTH", "PAGES",
			                                     		"PAGE_SIZE", "PARAMETER", "PASSWORD", "PERCENT", "PLAN", "POSITION",
			                                     		"POST_EVENT", "PRECISION", "PREPARE", "PRESERVE", "PROCEDURE",
			                                     		"PROTECTED", "PRIMARY", "PRIVILEGES", "PUBLIC", "QUIT",
			                                     		"RAW_PARTITIONS", "RDB$DB_KEY", "READ", "REAL", "RECORD_VERSION",
			                                     		"RECREATE", "REFERENCES", "RELEASE", "RESERV", "RESERVING",
			                                     		"RESTRICT", "RETAIN", "RETURN", "RETURNING_VALUES", "RETURNS",
			                                     		"REVOKE", "RIGHT", "ROLE", "ROLLBACK", "ROW_COUNT", "ROWS",
			                                     		"RUNTIME", "SCHEMA", "SEGMENT", "SELECT", "SET", "SHADOW", "SHARED",
			                                     		"SHELL", "SHOW", "SINGULAR", "SIZE", "SKIP", "SMALLINT", "SNAPSHOT",
			                                     		"SOME", "SORT", "SQL", "SQLCODE", "SQLERROR", "SQLWARNING",
			                                     		"STABILITY", "STARTING", "STARTS", "STATEMENT", "STATIC",
			                                     		"STATISTICS", "SUB_TYPE", "SUBSTRING", "SUM", "SUSPEND", "TABLE",
			                                     		"TEMPORARY", "TERMINATOR", "THEN", "TIES", "TO", "TRAILING",
			                                     		"TRANSACTION", "TRANSLATE", "TRANSLATION", "TRIGGER", "TRIM",
			                                     		"TRUE", "TYPE", "UNCOMMITTED", "UNION", "UNIQUE", "UPDATE",
			                                     		"UPDATING", "UPPER", "USER", "USING", "VALUE", "VALUES", "VARCHAR",
			                                     		"VARIABLE", "VARYING", "VERSION", "VIEW", "WAIT", "WHEN", "WHENEVER",
			                                     		"WHERE", "WHILE", "WITH", "WORK", "WRITE",
			                                     	};
		}

		///	<summary>
		///	Static singleton instance	for	access to	reserved words collection.
		///	</summary>
		private static ReservedWords reservedWords = new ReservedWords();

		public FirebirdFactory( IGentleProvider provider ) : base( provider )
		{
			// TODO schemaName is currently not specifyable!
		}

		/// <summary>
		/// Obtain an enum describing the supported database capabilities. The default is
		/// to support all capabilities. See <see cref="Capability"/> for details on the 
		/// available capabilities.
		/// </summary>
		public override Capability Capabilities
		{
			get
			{
				if( provider.ProviderInformation.Version.Major >= 2 )
				{
					// FIXME BatchQuery capability was added here because Gentle uses this capability 
					// to decide whether to insert and fetch identity using a single request. 
					return Capability.BatchQuery | Capability.Paging | Capability.NamedParameters;
				}
				else
				{
					return Capability.Paging | Capability.NamedParameters;
				}
			}
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details.
		/// </summary>
		public override long GetDbType( Type type )
		{
			FbDbType result = FbDbType.Integer;

			if( type.Equals( typeof(short) ) || type.Equals( typeof(Boolean) ) )
			{
				result = FbDbType.SmallInt;
			}
			else if( type.Equals( typeof(int) ) || type.IsEnum )
			{
				result = FbDbType.Integer;
			}
			else if( type.Equals( typeof(long) ) )
			{
				result = FbDbType.BigInt;
			}
			else if( type.Equals( typeof(double) ) || type.Equals( typeof(Double) ) )
			{
				result = FbDbType.Double;
			}
			else if( type.Equals( typeof(float) ) || type.Equals( typeof(Single) ) )
			{
				result = FbDbType.Float;
			}
			else if( type.Equals( typeof(decimal) ) || type.Equals( typeof(Decimal) ) )
			{
				result = FbDbType.Decimal;
			}
			else if( type.Equals( typeof(DateTime) ) )
			{
				result = FbDbType.TimeStamp;
			}
			else if( type.Equals( typeof(char) ) )
			{
				result = FbDbType.Char;
			}
			else if( type.Equals( typeof(string) ) )
			{
				result = FbDbType.VarChar;
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
			switch( dbType.Trim().ToLower() )
			{
				case "char":
					return (long) FbDbType.Char;
				case "varchar":
					return (long) FbDbType.VarChar;
				case "smallint":
					return (long) FbDbType.SmallInt;
				case "bigint":
					return (long) FbDbType.Integer;
				case "integer":
					return (long) FbDbType.BigInt;
				case "float":
					return (long) FbDbType.Float;
				case "double":
					return (long) FbDbType.Double;
				case "numeric":
					return (long) FbDbType.Numeric;
				case "decimal":
					return (long) FbDbType.Decimal;
				case "date":
					return (long) FbDbType.Date;
				case "time":
					return (long) FbDbType.Time;
				case "timestamp":
					return (long) FbDbType.TimeStamp;
				case "binary":
					return (long) FbDbType.Binary;
				case "text":
					return (long) FbDbType.Text;
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
			if( provider.ProviderInformation.Version.Major >= 2 )
			{
				// strip trailing whitespace and statement terminator
				sql = sql.TrimEnd( ' ', ';' );
				string name = om.IdentityMap.ColumnName;
				if( IsReservedWord( name ) )
				{
					name = QuoteReservedWord( name );
				}
				return String.Format( "{0} returns {1}", sql, name );
			}
			else // try to obtain value using sequence name
			{
				// If the second parameter to GEN_ID is 1 a new value will be generated.
				// If the value is 0, the current value is retrieved.
				if( om.IdentityMap.SequenceName != null )
				{
					return String.Format(
						"select GEN_ID({0}, 0) from rdb$database",
						om.IdentityMap.SequenceName );
				}
				else
				{
					return String.Format(
						"select GEN_ID(GEN_{0}_{1}, 0) from rdb$database",
						om.TableName.ToUpper(), om.IdentityMap.ColumnName.ToUpper() );
				}
			}
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
			return tableName;
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
		public override void AddParameter( IDbCommand cmd, string name, long dbType )
		{
			try
			{
				FbCommand fc = (FbCommand) cmd;
				// prefix parameters with @ for Firebird
				fc.Parameters.Add( GetParameterPrefix() + name, (FbDbType) dbType );
			}
			catch( Exception e )
			{
				Check.Fail( Error.Unspecified, e.Message );
				throw new GentleException( Error.Unspecified, "Unreachable code" );
			}
		}

		/// <summary>
		/// The current schema name or null if no schema.
		/// </summary>
		public string Schema
		{
			get { return schemaName; }
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

		///	<summary>
		///	Obtain a quoted	version	of the reserved	word to	allow	the	reserved word	to be	
		///	used in	queries	anyway.	If a reserved	word cannot	be quoted	this method	should
		///	raise	an error informing the user	that they	need to	pick a different name.
		///	</summary>
		///	<returns>The given reserved	word or	field	quoted to	avoid	errors.</returns>
		public override string QuoteReservedWord( string word )
		{
			return String.Format( "\"{0}\"", word );
		}
	}
}