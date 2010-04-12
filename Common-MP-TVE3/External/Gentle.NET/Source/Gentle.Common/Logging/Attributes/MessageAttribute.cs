/*
 * Attribute for decorating errors with message format strings
 * Copyright (C) 2005 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: MessageAttribute.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;

namespace Gentle.Common
{
	/// <summary>
	/// This attribute is used to decorate the <see cref="Error"/> enumeration with 
	/// a predefined error message. The associated message will be used as format
	/// string for the error message when a GentleException with this Error is throw.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = false )]
	public sealed class MessageAttribute : Attribute
	{
		private string message;
		private int args; // -1 = dont validate (unknown format string/message), 0+ = argument count

		/// <summary>
		/// Create a new Message attribute.
		/// </summary>
		/// <param name="message">The message to associate with this attribute instance.</param>
		public MessageAttribute( string message )
		{
			this.message = message;
			args = Messages.GetFormatStringArgumentCount( message );
		}

		#region Properties
		/// <summary>
		/// The message (format string) associated with this attribute.
		/// </summary>
		public string Message
		{
			get { return message; }
		}

		/// <summary>
		/// The number of format string parameters required to format a message using
		/// the message associated with this attribute.
		/// </summary>
		public int ArgumentCount
		{
			get { return args; }
		}
		#endregion
	}
}