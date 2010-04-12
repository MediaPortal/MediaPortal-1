/*
 * The interface for dynamically mapping a class to multiple tables
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ITableName.cs 646 2005-02-21 20:28:03Z mm $
 */

namespace Gentle.Framework
{
	/// <summary>
	/// This interface is for classes than need to dynamically map to multiple tables.
	/// </summary>
	public interface ITableName
	{
		/// <summary>
		/// Returns the name of the table with which the current object instance is
		/// associated for all database operations performed on it.
		/// </summary>
		string TableName { get; set; }
	}
}