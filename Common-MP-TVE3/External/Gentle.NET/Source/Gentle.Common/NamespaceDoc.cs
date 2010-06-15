/*
 * Namespace Summary
 * Copyright (C) 2004 Andreas Seibt
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: NamespaceDoc.cs 1232 2008-03-14 05:36:00Z mm $
 */

using Gentle.Common.Attributes;

namespace Gentle.Common
{
	/// <summary>
	/// <p>The <see cref="Gentle.Common"/> namespace contains code used by but not
	/// specific to Gentle.</p><p></p>
	/// <p>The TypedArrayList folder contains a typed version of the .NET Framework
	/// ArrayList class. The advantage against the standard <see cref="System.Collections.ArrayList"/>
	/// is, that the <see cref="TypedArrayList"/> could be bound to a DataGrid control without
	/// restrictions.</p>
	/// <p>Although it is possible to bind a standard ArrayList to a DataGrid control you will not be
	/// able to add or remove rows or to sort the data because the standard ArrayList doesn't implement
	/// the needed interfaces <see cref="System.ComponentModel.ITypedList"/> and
	/// <see cref="System.ComponentModel.IBindingList"/>.</p>
	/// <p>To make the binding more comfortable, the namespace contains some attributes which changes
	/// the default behaviour of the binding process of the DataGrid control. If you use the attributes
	/// you are able to</p><br />
	/// <list type="bullet">
	///   <item><description>define user defined column headers with the <see cref="CaptionAttribute"/></description></item>
	///   <item><description>allow / disallow sorting of specific columns with the <see cref="AllowSortAttribute"/></description></item>
	///   <item><description>hide specific columns with the <see cref="VisibleAttribute"/></description></item>
	/// </list>
	/// <br />
	/// <p>The code is based on the article "Sammelleidenschaften" in the dot.net Magazin 02/2003 by Thomas Fenske.</p>
	/// </summary>
	public class NamespaceDoc
	{
	}
}