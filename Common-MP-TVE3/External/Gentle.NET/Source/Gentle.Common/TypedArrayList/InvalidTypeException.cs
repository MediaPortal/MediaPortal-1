/*
 * Exception which is thrown if an invalid type is to be stored in the TypedArrayList
 * Copyright (C) 2004 Andreas Seibt
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: InvalidTypeException.cs 646 2005-02-21 20:28:03Z mm $
 */
using System;

namespace Gentle.Common
{
	/// <summary>
	/// This exception is thrown by the
	/// <see cref="Gentle.Common"><c>TypedArrayList</c></see>
	/// class if an object of another type as defined was passed to the add-methods.
	/// </summary>
	[Serializable]
	public class InvalidTypeException : ApplicationException
	{
		/// <summary>
		/// Standard constructor.
		/// </summary>
		/// <param name="expectedType">The expected type</param>
		/// <param name="value">The wrong object</param>
		public InvalidTypeException( Type expectedType, object value) : 
			base("The passed object '" + value.ToString() + "' is not an instance of '" + expectedType.FullName + "'.")
		{
		}

		/// <summary>
		/// Standard constructor.
		/// </summary>
		/// <param name="expectedType">The expected type</param>
		public InvalidTypeException(Type expectedType) : base("A passed object is not an instance of '" + expectedType.FullName + "'.")
		{
		}
	}
}
