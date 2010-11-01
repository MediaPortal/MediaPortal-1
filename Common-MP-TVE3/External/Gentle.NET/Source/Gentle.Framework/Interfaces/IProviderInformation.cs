/*
 * The interface for database providers.
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: IProviderInformation.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;

namespace Gentle.Framework
{
	/// <summary>
	/// Provides a standard structure to important information gathered from the actual provider.
	/// </summary>
	public interface IProviderInformation
	{
		/// <summary>
		/// Gets the name of the provider, as defined by the actual provider.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the version of the provider, as defined by the actual provider.
		/// </summary>
		Version Version { get; }
	}
}