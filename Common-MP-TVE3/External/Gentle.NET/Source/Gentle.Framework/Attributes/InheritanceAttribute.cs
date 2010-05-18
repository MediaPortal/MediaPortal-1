/*
 * The core attributes for decorating business objects
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: InheritanceAttribute.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;

namespace Gentle.Framework
{
	/// <summary>
	/// Use this attribute when mapping either multiple types to one table or one type
	/// to multiple tables (this latter is not yet supported). 
	/// </summary>
	[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
	public sealed class InheritanceAttribute : Attribute
	{
	}
}