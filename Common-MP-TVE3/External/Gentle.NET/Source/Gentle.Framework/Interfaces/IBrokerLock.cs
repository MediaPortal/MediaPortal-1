/*
 * The interface for classes locked to a specific PersistenceBroker instance
 * Copyright (C) 2005 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: IBrokerLock.cs 1232 2008-03-14 05:36:00Z mm $
 */

namespace Gentle.Framework
{
	/// <summary>
	/// Interface for classes that need to be locked to a specific 
	/// PersistenceBroker instance.
	/// </summary>
	public interface IBrokerLock
	{
		/// <summary>
		/// The session broker provides a lock to the database engine. This is
		/// useful when connecting to multiple databases in a single application.
		/// </summary>
		PersistenceBroker SessionBroker { get; set; }
	}
}