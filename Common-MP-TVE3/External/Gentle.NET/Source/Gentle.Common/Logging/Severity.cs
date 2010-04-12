/*
 * Enumeration of possible error severity levels
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: Severity.cs 646 2005-02-21 20:28:03Z mm $
 */

namespace Gentle.Common
{
	/// <summary>
	/// This enumeration lists severity levels used to classify exceptions. The levels
	/// have been loosely adapted from the UNIX syslog levels.
	/// </summary>
	public enum Severity
	{
		/// <summary>
		/// This severity level is used for messages containing debug information. 
		/// </summary>
		Debug,
		/// <summary>
		/// This severity level is used for informational messages.
		/// </summary>
		Info,
		/// <summary>
		/// This severity level is used for warning messages.
		/// </summary>
		Warning,
		/// <summary>
		/// This severity level is used for error messages. 
		/// </summary>
		Error,
		/// <summary>
		/// This severity level is used for error messages when the cause was of a critical nature.
		/// </summary>
		Critical,
		/// <summary>
		/// This severity level is used for messages whose severity has not been classified. In most
		/// cases this corresponds to the Debug classification.
		/// </summary>
		Unclassified
	} ;
}