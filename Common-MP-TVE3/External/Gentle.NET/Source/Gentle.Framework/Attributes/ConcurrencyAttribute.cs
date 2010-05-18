/*
 * The core attributes for decorating business objects
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ConcurrencyAttribute.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;

namespace Gentle.Framework
{
	/// <summary>
	/// <p>Use this attribute to designate the property holding the row version integer used
	/// for concurrency control. Updates (for objects under concurrency control) will only
	/// succeed if the version of the row being updated matches the objects version.</p> 
	/// <p>Support for concurrency control using DateTime values is not supported.</p>
	/// </summary>
	[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field,
		AllowMultiple = false, Inherited = true )]
	public sealed class ConcurrencyAttribute : Attribute
	{
	}
}