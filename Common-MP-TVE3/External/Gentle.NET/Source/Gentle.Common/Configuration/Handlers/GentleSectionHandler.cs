/*
 * Handler for the standard .NET config file.
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: GentleSectionHandler.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System.Configuration;
using System.Xml;

namespace Gentle.Common
{
	/// <summary>
	/// This class is a handler for the standard .NET config files (App.config or Web.config). The
	/// section name to use is "gentle" and is hardcoded in the HandlerRegistry class.
	/// </summary>
	public class GentleSectionHandler : ConfigurationHandler, IConfigurationSectionHandler
	{
		/// <summary>
		/// Create a new .NET section handler for obtaining settings from App.config or Web.config.
		/// </summary>
		public GentleSectionHandler() : base( null )
		{
		}

		#region IConfigurationSectionHandler Members
		/// <summary>
		/// Implementaton of the .NET configuration file section handler. This is called by .NET
		/// when the associated section is fetched.
		/// </summary>
		public object Create( object parent, object configContext, XmlNode section )
		{
			root = section;
			return this;
		}
		#endregion
	}
}