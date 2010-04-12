/*
 * Enumeration of logging categories
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: LogCategory.cs 606 2005-01-17 00:44:42Z mm $
 */

using System;

namespace Gentle.Common
{
	/// <summary>
	/// Enumeration of logging categories used by Gentle. These can be used to 
	/// selectively enable or disable logging for the specified categories.
	/// </summary>
	[Flags]
	public enum LogCategories
	{
		/// <summary>
		/// Select none of the available categories.
		/// </summary>
		None = 0,
		/// <summary>
		/// Log execution of select statements and their parameters (before execution) and 
		/// rows returned/affected (after execution).
		/// </summary>
		StatementExecutionRead = 1,
		/// <summary>
		/// Log execution of insert/update/delete statements and their parameters (before execution) 
		/// and rows returned/affected (after execution).
		/// </summary>
		StatementExecutionWrite = 2,
		/// <summary>
		/// Log execution of all non-CRUD type of statements (than thand their parameters (before 
		/// execution) and rows returned/affected (after execution).
		/// </summary>
		StatementExecutionOther = 4,
		/// <summary>
		/// Log execution of all types of statements and their parameters (before execution) and 
		/// rows returned/affected (after execution).
		/// </summary>
		StatementExecution = StatementExecutionRead | StatementExecutionWrite | StatementExecutionOther,
		/// <summary>
		/// Log successful cache hits, cache lookup misses and various other 
		/// cache-related performance counters.
		/// </summary>
		Cache = 8,
		/// <summary>
		/// Log metadata information gathered from databases, attributes or XML files.
		/// </summary>
		Metadata = 16,
		/// <summary>
		/// Log general information (configuration errors and uncategorized logging).
		/// </summary>
		General = 32,
		/// <summary>
		/// Select all available categories.
		/// </summary>
		All = StatementExecution | Cache | Metadata | General
	}
}