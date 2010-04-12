/*
 * MS Access (Jet) specifics
 * Copyright (C) 2004 Vinicius (Vinny) A. DaSilva
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: JetFactory.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;
using System.Data.OleDb;
using Gentle.Common;
using Gentle.Framework;

namespace Gentle.Provider.Jet
{
	/// <summary>
	/// This class is an implementation of the <see cref="GentleSqlFactory"/> class for the 
	/// Microsoft Access (Jet) Database.
	/// </summary>
	public class JetFactory : GentleSqlFactory
	{
		public JetFactory( IGentleProvider provider ) : base( provider )
		{
		}

		/// <summary>
		/// This is the list of reserved words in MS Access. 
		///	</summary>
		public static string[] reservedWords = new[]
		                                       	{
		                                       		// this list was obtained from the URL below
		                                       		// http://support.microsoft.com/default.aspx?scid=
		                                       		//   http://support.microsoft.com:80/support/kb/articles/Q109/3/12.asp&NoWebContent=1
		                                       		// added since: USER
		                                       		"ADD", "ALL", "ALPHANUMERIC", "ALTER", "AND", "ANY", "APPLICATION", "AS", "ASC", "ASSISTANT",
		                                       		"AUTOINCREMENT", "AVG", "BETWEEN", "BINARY", "BIT", "BOOLEAN", "BY", "BYTE", "CHAR", "CHARACTER",
		                                       		"COLUMN", "COMPACTDATABASE", "CONSTRAINT", "CONTAINER", "COUNT", "COUNTER", "CREATE",
		                                       		"CREATEDATABASE",
		                                       		"CREATEFIELD", "CREATEGROUP", "CREATEINDEX", "CREATEOBJECT", "CREATEPROPERTY", "CREATERELATION",
		                                       		"CREATETABLEDEF", "CREATEUSER", "CREATEWORKSPACE", "CURRENCY", "CURRENTUSER", "DATABASE", "DATE",
		                                       		"DATETIME", "DELETE", "DESC", "DESCRIPTION", "DISALLOW", "DISTINCT", "DISTINCTROW", "DOCUMENT",
		                                       		"DOUBLE", "DROP", "ECHO", "ELSE", "ENABLED", "END", "EQV", "ERROR", "EXISTS", "EXIT", "FALSE",
		                                       		"FIELD",
		                                       		"FIELDS", "FILLCACHE", "FLOAT", "FLOAT4", "FLOAT8", "FOREIGN", "FORM", "FORMS", "FROM", "FULL",
		                                       		"FUNCTION", "GENERAL", "GETOBJECT", "GETOPTION", "GOTOPAGE", "GROUP", "GROUP BY", "GUID",
		                                       		"HAVING",
		                                       		"IDLE", "IEEEDOUBLE", "IEEESINGLE", "IF", "IGNORE", "IMP", "IN", "IN", "INDEX", "INDEX",
		                                       		"INDEXES",
		                                       		"INNER", "INSERT", "INSERTTEXT", "INT", "INTEGER", "INTEGER1", "INTEGER2", "INTEGER4", "INTO",
		                                       		"IS",
		                                       		"IS", "JOIN", "KEY", "LASTMODIFIED", "LEFT", "LEVEL", "LIKE", "LOGICAL", "LOGICAL1", "LONG",
		                                       		"LONGBINARY", "LONGTEXT", "MACRO", "MATCH", "MAX", "MIN", "MOD", "MEMO", "MODULE", "MONEY",
		                                       		"MOVE",
		                                       		"NAME", "NEWPASSWORD", "NO", "NOT", "NULL", "NUMBER", "NUMERIC", "OBJECT", "OLEOBJECT", "OFF",
		                                       		"ON",
		                                       		"OPENRECORDSET", "OPTION", "OR", "OR", "ORDER", "OUTER", "OWNERACCESS", "PASSWORD", "PARAMETER",
		                                       		"PARAMETERS",
		                                       		"PARTIAL", "PERCENT", "PIVOT", "PRIMARY", "PROCEDURE", "PROPERTY", "QUERIES", "QUERY", "QUIT",
		                                       		"REAL",
		                                       		"RECALC", "RECORDSET", "REFERENCES", "REFRESH", "REFRESHLINK", "REGISTERDATABASE", "RELATION",
		                                       		"REPAINT", "REPAIRDATABASE", "REPORT", "REPORTS", "REQUERY", "RIGHT", "SCREEN", "SECTION",
		                                       		"SELECT",
		                                       		"SET", "SETFOCUS", "SETOPTION", "SHORT", "SINGLE", "SMALLINT", "SOME", "SQL", "STDEV", "STDEVP",
		                                       		"STRING", "SUM", "TABLE", "TABLEDEF", "TABLEDEFS", "TABLEID", "TEXT", "TIME", "TIMESTAMP", "TOP",
		                                       		"TRANSFORM", "TRUE", "TYPE", "UNION", "UNIQUE", "UPDATE", "USER", "VALUE", "VALUES", "VAR",
		                                       		"VARP",
		                                       		"VARBINARY", "VARCHAR", "WHERE", "WITH", "WORKSPACE", "XOR", "YEAR", "YES", "YESNO"
		                                       	};

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override long GetDbType( Type type )
		{
			OleDbType result = OleDbType.Integer;
			// TODO it can hardly be right to have decimal as integer type?!
			if( type.Equals( typeof(int) ) || type.IsEnum )
			{
				result = OleDbType.Integer;
			}
			else if( type.Equals( typeof(long) ) )
			{
				result = OleDbType.BigInt;
			}
			else if( type.Equals( typeof(double) ) )
			{
				result = OleDbType.Double;
			}
			else if( type.Equals( typeof(DateTime) ) )
			{
				result = OleDbType.Date;
			}
			else if( type.Equals( typeof(bool) ) )
			{
				result = OleDbType.Boolean;
			}
			else if( type.Equals( typeof(decimal) ) )
			{
				result = OleDbType.Decimal;
			}
			else if( type.Equals( typeof(string) ) )
			{
				result = OleDbType.BSTR;
			}
			else if( type.Equals( typeof(Guid) ) )
			{
				result = OleDbType.BSTR; //Guids are treated like strings
			}
			else if( type.Equals( typeof(byte[]) ) )
			{
				result = OleDbType.Binary;
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
			throw new NotImplementedException( "This method is never called and thus not implemented for Jet." );
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override string GetParameterPrefix()
		{
			return "[?";
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override string GetParameterSuffix()
		{
			return "]";
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
			// Access 97/ Jet 3.x does no support the SELECT @@IDENTITY
			//return string.Format( "SELECT max ({0}) from {1}", om.IdentityMap.ColumnName, om.TableName );
		}

		/// <summary>
		/// Obtain an enum describing the supported database capabilities.  
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details.
		/// Access/Jet does not support Batch Command Processing, please refer to
		/// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpguide/html/cpconRetrievingIdentityOrAutonumberValues.asp
		/// </summary>
		public override Capability Capabilities
		{
			get { return 0; }
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override void AddParameter( IDbCommand cmd, string name, long dbType )
		{
			try
			{
				OleDbType oleType = (OleDbType) dbType;
				OleDbCommand sc = (OleDbCommand) cmd;
				//place suffix and prefix around parameter
				sc.Parameters.Add( GetParameterPrefix() + name + GetParameterSuffix(), oleType );
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
			if( word.IndexOfAny( new[] { ' ', '-' } ) >= 0 )
			{
				return true;
			}
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
			return String.Format( "{0}{1}{2}", "[", word, "]" );
		}
	}
}