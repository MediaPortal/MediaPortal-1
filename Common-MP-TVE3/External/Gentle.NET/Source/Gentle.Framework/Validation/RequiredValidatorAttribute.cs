/*
 * An attribute that is used to validate a required field is present.
 * Copyright (C) 2005 Clayton Harbour
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: RequiredValidatorAttribute.cs $
 */

using System;

namespace Gentle.Framework
{
	/// <summary>
	/// Apply this attribute to members which must have a value assigned. This is mostly
	/// useful with strings.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true )]
	public class RequiredValidatorAttribute : ValidatorBaseAttribute
	{
		private bool allowNull;

		/// <summary>
		/// Set to <see langword="true"/> to allow a null value to pass validation.
		/// </summary>
		public bool AllowNull
		{
			get { return allowNull; }
			set { allowNull = value; }
		}

		/// <summary>
		/// This method is invoked to perform the actual range validation on the given property.
		/// </summary>
		/// <param name="propertyName">The name of the property being validated.</param>
		/// <param name="propertyValue">The property value to validate.</param>
		/// <param name="propertyOwner">The persistent object that holds the property being validated.</param>
		/// <returns>This method returns true if the validation succeeds and false otherwise.</returns>
		public override bool Validate( string propertyName, object propertyValue, object propertyOwner )
		{
			if( propertyValue == null )
			{
				return allowNull;
			}
			return ! (propertyValue is string && string.Empty == (string) propertyValue);
		}
	}
}