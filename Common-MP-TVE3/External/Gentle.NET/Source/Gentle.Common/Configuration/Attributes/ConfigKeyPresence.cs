/*
 * Enumeration of configuration key requirements (mandatory or optional)
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ConfigKeyPresence.cs 646 2005-02-21 20:28:03Z mm $
 */

namespace Gentle.Common
{
	/// <summary>
	/// Enumeration of configuration key requirements (mandatory or optional).
	/// </summary>
	public enum ConfigKeyPresence
	{
		/// <summary>
		/// This value is used to mark required configuration keys. An exception will
		/// be thrown if the key is not found during configuration of a type or instance.
		/// </summary>
		Mandatory,
		/// <summary>
		/// This value is used to mark optional configuration keys. No exception will
		/// be thrown if the key is missing from the configuration file.
		/// </summary>
		Optional
	}
}