/*
 * An attribute that is used to ensure the contents of a text field match an expression.
 * Copyright (C) 2005 Clayton Harbour
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: RegExValidatorAttribute.cs $
 */
using System;
using System.Text.RegularExpressions;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Determines if the given property matches a regular expression. If a value is null then
	/// it will always pass (note: you can use the <see cref="RequiredValidatorAttribute"/> to 
	/// ensure that a value is not null).
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true )]
	public class RegexValidatorAttribute : ValidatorBaseAttribute
	{
		private string expression;

		/// <summary>
		/// The regular expression to validate against.
		/// </summary>
		public string Expression
		{
			get { return expression; }
			set { expression = value; }
		}

		#region Constructors
		/// <summary>
		/// The default constructor for the <see cref="RegexValidatorAttribute"/> class.
		/// </summary>
		public RegexValidatorAttribute()
		{
		}

		/// <summary>
		/// Constructor for the <see cref="RegexValidatorAttribute"/> class permitting you
		/// to specify the regular expression to validate against.
		/// </summary>
		public RegexValidatorAttribute( string expression )
		{
			this.expression = expression;
		}
		#endregion

		/// <summary>
		/// This method is invoked to perform the actual range validation on the given property.
		/// </summary>
		/// <param name="propertyName">The name of the property being validated.</param>
		/// <param name="propertyValue">The property value to validate.</param>
		/// <param name="propertyOwner">The persistent object that holds the property being validated.</param>
		/// <returns>This method returns true if the validation succeeds and false otherwise.</returns>
		public override bool Validate( string propertyName, object propertyValue, object propertyOwner )
		{
			Check.VerifyNotNull( expression, "Expression is required." );
			// if the object is null then do not evaluate expression.
			if( propertyValue == null )
			{
				return true;
			}
			Match match = Regex.Match( propertyValue.ToString(), expression );
			return match.Success;
		}
	}
}