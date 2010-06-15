/*
 * Base class for configuration targets Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it under the terms of the
 * GNU Lesser General Public License 2.1 or later, as published by the Free Software Foundation. 
 * See the included License.txt or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ElementTarget.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System.Xml;

namespace Gentle.Common
{
	internal abstract class ElementTarget
	{
		internal readonly string XmlNodePath;
		internal readonly ConfigKeyPresence KeyPresenceRequirement;

		public ElementTarget( ConfigurationAttribute ca )
		{
			XmlNodePath = ca.XmlNodePath;
			KeyPresenceRequirement = ca.KeyPresenceRequirement;
		}

		/// <summary>
		/// Method called to determine if an XmlNode carries an "environment" attribute with the
		/// specified value. This is used to limits node visibility to a certain environment.
		/// </summary>
		/// <param name="node">The node to check.</param>
		/// <param name="environment">The environment value to check for.</param>
		/// <returns>True if the attribute named "environment" is found and has the value given in
		/// the environment parameter, false otherwise.</returns>
		protected bool IsEnvironment( XmlNode node, string environment )
		{
			foreach( XmlAttribute attr in node.Attributes )
			{
				if( attr.Name == "environment" )
				{
					return attr.Value == environment;
				}
			}
			return false;
		}

		/// <summary>
		/// Method called to determine if an XmlNode carries an "environment" attribute to limits
		/// its visibility to a certain environment.
		/// </summary>
		/// <param name="node">The node to check.</param>
		/// <returns>True if no attribute named "environment" is found, false otherwise.</returns>
		protected bool IsCommonEnvironment( XmlNode node )
		{
			bool found = false;
			foreach( XmlAttribute attr in node.Attributes )
			{
				found |= attr.Name == "environment";
			}
			return ! found;
		}

		public abstract void Configure( object target, XmlNode node );

		/// <summary>
		/// Use the supplied XmlNodes to configure the target object. This method filters the given
		/// list of nodes by environment before applying them individually.
		/// </summary>
		/// <param name="target">The object to cofigure.</param>
		/// <param name="nodes">The list of nodes containing the configuration value(s).</param>
		public virtual void Configure( object target, XmlNodeList nodes )
		{
			XmlNode common = null;
			foreach( XmlNode node in nodes )
			{
				if( IsEnvironment( node, SystemSettings.Environment ) )
				{
					Configure( target, node );
					break;
				}
				else if( IsCommonEnvironment( node ) )
				{
					common = node;
				}
			}
			// still here? use node without environment restriction (or as backup the first node)
			Configure( target, common != null ? common : nodes[ 0 ] );
		}
	}
}