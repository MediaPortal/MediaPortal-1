/*
 * Enumeration used to configure NullValue handling for non-constant fields
 * Copyright (C) 2005 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: NullValue.cs 801 2005-07-08 21:07:55Z mm $
 */

namespace Gentle.Framework
{
	/// <summary>
	/// Enumeration to indicate if a field should use the types MinValue or MaxValue
	/// as the NULL translation value. This is useful for DateTime and Decimal fields,
	/// where it is not possible to use the actual value in the TableColumn attribute
	/// declaration. 
	/// </summary>
	public enum NullOption
	{
		/// <summary>
		/// &lt;type&gt;.MinValue will be stored as NULL, and NULL will be read as &lt;type&gt;.MinValue.
		/// </summary>
		MinValue,
		/// <summary>
		/// &lt;type&gt;.MaxValue will be stored as NULL, and NULL will be read as &lt;type&gt;.MaxValue.
		/// </summary>
		MaxValue,
		/// <summary>
		/// 0 (or the equivalent for other numeric types) will be stored as NULL, and NULL will be read as 0.
		/// </summary> This value can only be used with numeric types (such as decimal).
		Zero,
		/// <summary>
		/// Guid.Empty will be stored as NULL, and NULL will be read as Guid.Empty. This value can only be
		/// used with Guid fields.
		/// </summary>
		EmptyGuid
	}
}