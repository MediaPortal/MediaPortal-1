/*
 * The base attribute for persistent validations.
 * Copyright (C) 2005 Clayton Harbour
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ValidatorBaseAttribute.cs $
 */

using System;

namespace Gentle.Framework
{
	/// <summary>
	/// This is the base validator attribute, it provides a common interface 
	/// for all custom validations.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true )]
	public abstract class ValidatorBaseAttribute : Attribute
	{
		private ValidationMessage message = new ValidationMessage();
		private bool failOnFirst;

		/// <summary>
		/// Internal validation message object.
		/// </summary>
		public ValidationMessage Message
		{
			get { return message; }
		}
		/// <summary>
		/// Optional message id to use for validations.  Provide this if you would prefer to use 
		/// a resource bundle or some other means of looking up strings from an external source.
		/// </summary>
		public string MessageId
		{
			get { return message.Id; }
			set { message.Id = value; }
		}

		/// <summary>
		/// Message to provide the client/ user that explains what is incorrect about the value
		/// they have entered.
		/// </summary>
		public string MessageText
		{
			get { return message.Text; }
			set { message.Text = value; }
		}

		/// <summary>
		/// Indicate if the validation of an object should fail on the first failure or if the
		/// entire object should be validated.
		/// </summary>
		/// <value>Default: <see langword="false"/></value>
		public bool FailOnFirst
		{
			get { return failOnFirst; }
			set { failOnFirst = value; }
		}

		/// <summary>
		/// Override this method to add the code that performs validation on the property.
		/// </summary>
		/// <param name="propertyName">The name of the property being validated.</param>
		/// <param name="propertyValue">The property value to validate.</param>
		/// <param name="propertyOwner">The persistent object that holds the property being validated.</param>
		/// <returns>This method should return true if the validation succeeds and false otherwise. The
		/// default implementation always returns true.</returns>
		public virtual bool Validate( string propertyName, object propertyValue, object propertyOwner )
		{
			return true;
		}
	}
}