/*
 * The exception that is thrown when validation fails.
 * Copyright (C) 2005 Clayton Harbour
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ValidationException.cs $
 */

using System;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Strongly typed exception used for dealing with exceptions.
	/// </summary>
	[Serializable]
	public class ValidationException : GentleException
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public ValidationException() : base( Error.Validation, null, null )
		{
		}

		/// <summary>
		/// Constructor taking an <see cref="Error"/> and a custom message to go with the error. 
		/// The framework consistently uses the <see cref="Error"/> enumeration to classify 
		/// raised exceptions.
		/// </summary>
		/// <param name="error">The Error condition leading to this exception</param>
		/// <param name="msg">An additional message text</param>
		public ValidationException( Error error, string msg ) : base( error, msg, null )
		{
		}
	}
}