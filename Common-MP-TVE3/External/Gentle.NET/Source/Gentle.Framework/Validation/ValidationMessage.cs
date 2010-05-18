/*
 * The message object that is used to indicate what validations failed.
 * Copyright (C) 2005 Clayton Harbour
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ValidationMessage.cs $
 */

using System;
using System.Resources;

namespace Gentle.Framework
{
	/// <summary>
	/// This class is used to convey information on validation failures.
	/// </summary>
	[Serializable]
	public class ValidationMessage
	{
		private string id;
		private string text;
		private ResourceManager rm;

		/// <summary>
		/// Id of the message, used for il8n.
		/// </summary>
		public string Id
		{
			get { return id; }
			set
			{
				id = value;
				text = GetMessage( id );
			}
		}

		/// <summary>
		/// Text of the error message, used if il8n is a bit overkill for you.
		/// </summary>
		public string Text
		{
			get { return text; }
			set { text = value; }
		}

		/// <summary>
		/// Public constructor.
		/// </summary>
		public ValidationMessage()
		{
			rm = new ResourceManager( "Validation", GetType().Assembly );
		}

		/// <summary>
		/// Get the message string for the current culture.
		/// </summary>
		/// <param name="messageId">The message id to retrieve.</param>
		/// <param name="args">Optional replacement values for a templated string.</param>
		/// <returns>The string value of the message to return.</returns>
		protected string GetMessage( string messageId, params object[] args )
		{
			return rm.GetString( String.Format( messageId, args ) );
		}
	}
}