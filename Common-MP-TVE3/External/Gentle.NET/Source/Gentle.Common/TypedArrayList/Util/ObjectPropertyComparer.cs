/*
 * Comparer for objects in a TypedArrayList
 * Copyright (C) 2004 Andreas Seibt
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ObjectPropertyComparer.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;

namespace Gentle.Common.Util
{
	/// <summary>
	/// Internal class for sorting objects.
	/// </summary>
	/// <remarks>
	/// The <c>ObjectPropertyComparer</c> acts a a comparer for the objects contained in a
	/// <see cref="TypedArrayList"/>. It sorts the objects based on the 
	/// name of the property passed in the constructor. This comparer is used internally,
	/// if the TypedArrayList is bound to a DataGrid control and the user clicks on a 
	/// column header to sort the data.
	/// </remarks>
	internal class ObjectPropertyComparer : IComparer
	{
		private string propertyName;

		/// <summary>
		/// Initialize a new Comparer for a given property.
		/// </summary>
		/// <param name="propertyName">The name of the property to compare.</param>
		internal ObjectPropertyComparer( string propertyName )
		{
			this.propertyName = propertyName;
		}

		/// <summary>
		/// Compare two objects based on the property name passed to the constructor.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>
		/// <div class="tablediv">
		/// <table class="dtTABLE" cellspacing="0">
		///		<tr>
		///			<th width="50%">Value</th>
		///			<th width="50%">Condition</th>
		///		</tr>
		///		<tr>
		///			<td width="50%">Less than zero</td>
		///			<td width="50%"><i>a</i> is less than <i>b</i></td>
		///		</tr>
		///		<tr>
		///			<td width="50%">Zero</td>
		///			<td width="50%"><i>a</i> equals <i>b</i></td>
		///		</tr>
		///		<tr>
		///			<td width="50%">Greater than zero</td>
		///			<td width="50%"><i>a</i> is greater than <i>b</i></td>
		///		</tr>
		/// </table>
		/// </div>
		/// </returns>
		public int Compare( object x, object y )
		{
			object a = x.GetType().GetProperty( propertyName ).GetValue( x, null );
			object b = y.GetType().GetProperty( propertyName ).GetValue( y, null );

			if( a != null && b == null )
			{
				return 1;
			}
			if( a == null && b != null )
			{
				return -1;
			}
			if( a == null && b == null )
			{
				return 0;
			}
			return ((IComparable) a).CompareTo( b );
		}
	}
}