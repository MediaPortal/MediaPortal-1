/*
 * The interface for SQL engines
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: IPersistenceEngine.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;

namespace Gentle.Framework
{
	/// <summary>
	/// The interface to implement in order to support a specific persistence engine,
	/// i.e. a specific vendors RDBMS.
	/// </summary>
	public interface IPersistenceEngine
	{
		/// <summary>
		/// Execute an SQL statement and produce a corresponding result set (even when no rows are returned).
		/// </summary>
		/// <param name="stmt">The statement object to execute</param>
		/// <returns>The result set produced</returns>
		SqlResult ExecuteStatement( SqlStatement stmt );

		/// <summary>
		/// Obtain a command instance.
		/// </summary>
		/// <returns>The database command object</returns>
		IDbCommand GetCommand();

		/// <summary>
		/// Obtain a connection to the persistence engine. The returned connection should be open.
		/// </summary>
		/// <returns>The database connection</returns>
		IDbConnection GetConnection();

		/// <summary>
		/// Cache a CRUD-type statement for the given type.
		/// </summary>
		void CacheStatement( Type type, StatementType stmtType, SqlStatement stmt );

		/// <summary>
		/// Retrieve a cached CRUD-type statement for the given type.
		/// </summary>
		SqlStatement GetStatement( Type type, string tableName, StatementType stmtType );
	}
}