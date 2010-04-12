/*
 * The core attributes for decorating business objects
 * Copyright (C) 2005 Clayton Harbour
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: RangeValidatorAttribute.cs  $
 */

using System;
using System.Globalization;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Validates if the given property is between the range values specifed.
	/// If the member value being validated matches any of the two values it will pass 
	/// validation (note: Gentle 1.2.5 and earlier did not consider this valid).
	/// If you pass in strings then these are assumed to contain dates formatted using 
	/// invariant culture. If either Min or Max is not specified the range is considered 
	/// to be open-ended and validation only occurs against the specified value. A null
	/// value never passes validation.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true )]
	public class RangeValidatorAttribute : ValidatorBaseAttribute
	{
		private object min;
		private object max;

		#region Properties
		/// <summary>
		/// The minimum permitted value (inclusive). 
		/// </summary>
		public object Min
		{
			get { return min; }
			set { min = value; }
		}

		/// <summary>
		/// The maximum permitted value (inclusive).
		/// </summary>
		public object Max
		{
			get { return max; }
			set { max = value; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// The default constructor for the <see cref="RangeValidatorAttribute"/> class.
		/// </summary>
		public RangeValidatorAttribute() : this( null, null )
		{
		}

		/// <summary>
		/// Constructor for the <see cref="RangeValidatorAttribute"/> class permitting you to
		/// specify the minimum and maximum values permitted.
		/// </summary>
		public RangeValidatorAttribute( object min, object max )
		{
			this.min = min;
			this.max = max;
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
			if( propertyValue == null )
			{
				// null is always considered outside the permitted range
				return false;
			}
			else
			{
				Type type = propertyValue.GetType();
				if( type == typeof(short) || type == typeof(int) || type == typeof(long) )
				{
					return ValidateRangeLong( Convert.ToInt64( propertyValue ) );
				}
				else if( type == typeof(float) || type == typeof(double) )
				{
					return ValidateRangeDouble( Convert.ToDouble( propertyValue ) );
				}
				else if( type == typeof(DateTime) )
				{
					return ValidateRangeDateTime( (DateTime) propertyValue );
				}
				// object is not a number
				return false;
			}
		}

		private bool ValidateRangeLong( long number )
		{
			long minValue = min == null ? long.MinValue : Convert.ToInt64( min );
			long maxValue = max == null ? long.MaxValue : Convert.ToInt64( max );
			return number >= minValue && number <= maxValue;
		}

		private bool ValidateRangeDouble( double number )
		{
			double minValue = min == null ? double.MinValue : Convert.ToDouble( min );
			double maxValue = max == null ? double.MaxValue : Convert.ToDouble( max );
			return number >= minValue && number <= maxValue;
		}

		private bool ValidateRangeDateTime( DateTime date )
		{
			DateTime minValue = min == null ? DateTime.MinValue : GetDateTime( min );
			DateTime maxValue = max == null ? DateTime.MaxValue : GetDateTime( max );
			return date >= minValue && date <= maxValue;
		}

		private DateTime GetDateTime( object value )
		{
			if( value.GetType() == typeof(DateTime) )
			{
				return (DateTime) value;
			}
			// convert string-encoded dates
			try
			{
				string defaults = "0001-01-01 00:00:00.000";
				string date = (string) value;
				date += defaults.Substring( date.Length, defaults.Length - date.Length );
				return DateTime.Parse( date, DateTimeFormatInfo.InvariantInfo );
			}
			catch( Exception e )
			{
				Check.Fail( e, Error.DeveloperError, String.Format( "The specified value {0} is not a valid DateTime." ) );
				throw;
			}
		}
	}
}