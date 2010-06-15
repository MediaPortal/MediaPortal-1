/*
 * Base class for all Gentle configuration handlers
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ConfigurationHandler.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System.Xml;
using System.Xml.XPath;

namespace Gentle.Common
{
	/// <summary>
	/// Abstract base class for all configuration handlers. A handler encapsulates the 
	/// specifics of extracting the Gentle configuration segment from a backend store,
	/// such as the file system or another configuration file.
	/// </summary>
	public class ConfigurationHandler
	{
		/// <summary>
		/// The root node of the Gentle configuration fragment.
		/// </summary>
		protected XmlNode root;

		/// <summary>
		/// Constructor used to create a handler when an XML fragment is passed 
		/// to the Configurator class.
		/// </summary>
		/// <param name="root">The root configuration node.</param>
		internal ConfigurationHandler( XmlNode root )
		{
			this.root = root;
		}

		/// <summary>
		/// Determines whether this section handler has access to a valid configuration store
		/// and was able to read the required information from it.
		/// </summary>
		/// <returns>True if this handler can be used as settings input, false otherwise.</returns>
		public bool IsValid
		{
			get { return root != null; }
		}

		/// <summary>
		/// Obtains the root node of the configuration fragment from the handler.
		/// </summary>
		public IXPathNavigable XmlRoot
		{
			get { return root; }
		}

		#region Tree Access Methods
		/// <summary>
		/// Obtains the requested configuration node from the tree.
		/// </summary>
		public XmlNode GetNode( string path )
		{
			return root.SelectSingleNode( path );
		}

		/// <summary>
		/// Obtains the requested configuration node from the tree.
		/// </summary>
		public XmlNodeList GetNodes( string path )
		{
			return root.SelectNodes( path );
		}
		#endregion
	}
}