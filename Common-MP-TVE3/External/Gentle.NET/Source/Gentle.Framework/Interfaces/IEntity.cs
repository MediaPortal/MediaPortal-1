/*
 * The interface for persistent objects
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: IEntity.cs 1223 2008-03-11 15:05:54Z mm $
 */

namespace Gentle.Framework
{
	/// <summary>
	/// This interface specifies methods that objects must implement in order for the
	/// persistence framework to accept them. 
	/// 
	/// Additionally, it specified convenience methods that allows clients to call the
	/// persistence methods directly on the object, rather than using the PersistenceBroker 
	/// directly.
	/// </summary>
	public interface IEntity
	{
		/// <summary>
		/// The Key property is a Hashtable of key/value pairs that specify the selection 
		/// criteria unique to the current object instance. The Key usually corresponds
		/// to the primary keys of the row holding the object.
		/// </summary>
		/// <returns>The unique Key of the current instance. The generated key should
		/// contain property values.</returns>
		Key GetKey();

		/// <summary>
		/// True if the current instance has been persisted to the database.
		/// </summary>
		bool IsPersisted { get; set; }
	}
}