/*
 * The interface for persistent objects
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: IPersistent.cs 1223 2008-03-11 15:05:54Z mm $
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
	public interface IPersistent : IEntity
	{
		/// <summary>
		/// Insert or update the current object instanse.
		/// </summary>
		void Persist();

		/// <summary>
		/// Delete the current object instanse. 
		/// Note: This does not destroy or invalide the current object instance.
		/// </summary>
		void Remove();

		/// <summary>
		/// Select the current object instance and initialize all properties/fields with
		/// the values of the retrieved data. This method can be used in place of static
		/// constructors in implementations of IPersistant as it allows constructors to
		/// merely take a Key parameter (and then call this method to initialize all fields).
		/// </summary>
		void Refresh();
	}
}