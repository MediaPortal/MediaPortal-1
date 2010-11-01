/*
 * The core attributes for decorating business objects
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: SequenceNameAttribute.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;

namespace Gentle.Framework
{
	/// <summary>
	/// Use this attribute to name the sequence used for auto-generated columns. Gentle
	/// will use this sequence name when it is unable to obtain the information from
	/// the database automatically. This is the case for a.o. Firebird. If this attribute
	/// is not present, Gentle attempts to guess the sequence name from certain name
	/// conventions (for details, consult the SqlFactory class for the provider).
	/// </summary>
	[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
	public sealed class SequenceNameAttribute : Attribute
	{
		private string name;

		/// <summary>
		/// Construct a new SequenceName attribute. 
		/// </summary>
		/// <param name="sequenceName">The table to relate to</param>
		public SequenceNameAttribute( string sequenceName )
		{
			name = sequenceName;
		}

		/// <summary>
		/// The name of the sequence used by an auto-generated column.
		/// </summary>
		public string Name
		{
			get { return name; }
		}
	}
}