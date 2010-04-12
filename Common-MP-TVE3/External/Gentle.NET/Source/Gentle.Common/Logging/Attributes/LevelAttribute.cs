/*
 * Attribute for decorating errors with severity levels
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: LevelAttribute.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;

namespace Gentle.Common
{
	/// <summary>
	/// This attribute is used to decorate the <see cref="Error"/> enumeration with 
	/// a predefined severity level. This allows the framework to handle errors
	/// differently depending on their severity (such as suppressing logging or
	/// by handling them internally). This is particularly useful when moving from
	/// development/testing to production, where you want a more resilient system.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
	public sealed class LevelAttribute : Attribute
	{
		private Severity severity;

		/// <summary>
		/// Create a new Level attribute.
		/// </summary>
		/// <param name="severity">The severity to associate with this LevelAttribute instance.</param>
		public LevelAttribute( Severity severity )
		{
			this.severity = severity;
		}

		/// <summary>
		/// The severity associated with this attribute.
		/// </summary>
		public Severity Severity
		{
			get { return severity; }
		}
	}
}