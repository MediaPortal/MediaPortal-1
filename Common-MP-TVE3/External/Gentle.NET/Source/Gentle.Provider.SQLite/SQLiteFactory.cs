/*
 * SQLite specifics
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: SQLiteFactory.cs 1226 2008-03-13 22:16:41Z mm $
 */

using System;
using System.Data;
using System.Data.SQLite;
using Gentle.Common;
using Gentle.Framework;

namespace Gentle.Provider.SQLite
{
	/// <summary>
	/// This class is an implementation of the <see cref="GentleSqlFactory"/> class for the SQLite RDBMS.
	/// </summary>
	public class SQLiteFactory : GentleSqlFactory
	{
		public SQLiteFactory( IGentleProvider provider ) : base( provider )
		{
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override long GetDbType( Type type )
		{
			DbType result = DbType.Int32;
			if( type.Equals( typeof(int) ) || type.IsEnum )
			{
				result = DbType.Int32;
			}
			else if( type.Equals( typeof(long) ) )
			{
				result = DbType.Int64;
			}
			else if( type.Equals( typeof(double) ) )
			{
				result = DbType.Double;
			}
			else if( type.Equals( typeof(DateTime) ) )
			{
				result = DbType.DateTime;
			}
			else if( type.Equals( typeof(bool) ) )
			{
				result = DbType.Boolean;
			}
			else if( type.Equals( typeof(string) ) )
			{
				result = DbType.String;
			}
			else if( type.Equals( typeof(byte[]) ) || type.Equals( typeof(Byte[]) ) )
			{
				result = DbType.Binary;
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
			Check.Fail( Error.NotImplemented );
			return 0;
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
			return sql + "; select last_insert_rowid()";
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleSqlFactory"/> for details. 
		/// </summary>
		public override string GetParameterPrefix()
		{
			return "@";
		}

		/// <summary>
		/// Obtain the name to use for name-based indexing into the IDbCommand.Parameters
		/// collections. Most databases omit the parameter prefix, whereas some require it
		/// to be present (e.g. SQLite).
		/// </summary>
		/// <param name="paramName">The parameter name without quoting or prefix/suffix.</param>
		/// <returns>The name to use when accessing the IDbCommand.Parameters hashtable.</returns>
		public override string GetParameterCollectionKey( string paramName )
		{
			return GetParameterPrefix() + paramName;
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
				SQLiteCommand sqlc = (SQLiteCommand) cmd;
				SQLiteParameter param = new SQLiteParameter( name, (DbType) dbType );
				param.Direction = ParameterDirection.Input;
				sqlc.Parameters.Add( param );
			}
			catch( Exception e )
			{
				Check.Fail( Error.Unspecified, e.Message );
				throw new GentleException( Error.Unspecified, "Unreachable code" );
			}
		}
	}
}