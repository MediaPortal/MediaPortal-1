/*
 * The interface for database providers.
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: FirebirdProviderInformation.cs 1232 2008-03-14 05:36:00Z mm $
 */
using System;
using Gentle.Framework;

namespace Gentle.Provider.Firebird
{
	/// <summary>
	/// The class encapsulates information about the underlying database provider,
	/// as well as the logic to extract it.
	/// </summary>
	public class FirebirdProviderInformation : IProviderInformation
	{
		private FirebirdProvider provider;
		private string name = "Firebird";
		private Version version = new Version( 1, 0, 0 );

		public FirebirdProviderInformation( FirebirdProvider provider )
		{
			this.provider = provider;
		}

		#region IProviderInformation
		/// <summary>
		/// Please refer to the documentation of <see cref="IProviderInformation"/>
		/// for details.
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="IProviderInformation"/>
		/// for details.
		/// </summary>
		public Version Version
		{
			get { return version; }
		}
		#endregion
	}
}