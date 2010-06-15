/*
 * Enumeration of possible cache lifetime management strategies
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: UniqingScope.cs 1020 2006-05-26 21:30:35Z mm $
 */

namespace Gentle.Common
{
	/// <summary>
	/// This enumeration is used to select the scope within which to ensure 
	/// object uniqing. This is only appliccable when object caching is enabled.
	/// </summary>
	public enum UniqingScope
	{
		/// <summary>
		/// This value indicates that objects are uniqued within the scope of an ASP.NET
		/// web session. If no session exists Application scope is used.
		/// </summary>
		WebSession,
		/// <summary>
		/// This value indicates that objects are uniqued within a thread. This is the
		/// default scope. 
		/// </summary>
		Thread,
		/// <summary>
		/// This value indicates that objects are uniqued within an application. It is
		/// up to the application developer to ensure thread safety.
		/// </summary>
		Application
	}
}