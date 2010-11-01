/*
 * Helper class for storing information on members and their attributes
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: MemberAttributeInfo.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Reflection;

namespace Gentle.Common
{
	/// <summary>
	/// Helper class used to store information on a member and the attributes it is decorated with.
	/// </summary>
	public class MemberAttributeInfo
	{
		/// <summary>
		/// The member (method, field, property, etc.) with which the attributes are associated.
		/// </summary>
		public readonly MemberInfo MemberInfo;
		/// <summary>
		/// The list of attributes found.
		/// </summary>
		public readonly IList Attributes;

		/// <summary>
		/// Create a new MemberAttributeInfo instance linking a MemberInfo object to the
		/// attributes it is carrying.
		/// </summary>
		public MemberAttributeInfo( MemberInfo memberInfo, IList attributes )
		{
			MemberInfo = memberInfo;
			Attributes = attributes;
		}

		/// <summary>
		/// Obtain one of the attributes associated with the MemberInfo.
		/// </summary>
		public Attribute this[ int index ]
		{
			get { return Attributes[ index ] as Attribute; }
		}
	}
}