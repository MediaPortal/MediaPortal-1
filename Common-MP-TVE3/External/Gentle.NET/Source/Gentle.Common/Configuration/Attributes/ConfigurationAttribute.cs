/*
 * Attribute used on members that need to be initialized from the configuration file
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ConfigurationAttribute.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;

namespace Gentle.Common
{
	/// <summary>
	/// Attribute with which to decorate members that need configuring. The configuration 
	/// subsystem automatically converts the value to the proper type of the target. If
	/// this attribute is used on a method then the method will be called once for every
	/// node matching the specified XmlNodePath (refer to <see cref="CallbackTarget"/> for 
	/// additional information on this).
	/// </summary>
	[AttributeUsage( AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field,
		AllowMultiple = false, Inherited = true )]
	public sealed class ConfigurationAttribute : Attribute
	{
		/// <summary>
		/// The XML path to the node containing the configuration value(s).
		/// </summary>
		private string xmlNodePath;

		/// <summary>
		/// The number of callback method parameters whose presence is required in order
		/// for the method to be eligible for invocation. Only parameters after this 
		/// number of parameters will be considered for null or default value substition.
		/// </summary>
		private int requiredParameters;

		/// <summary>
		/// This denotes whether the configuration key used for a configuration target
		/// is optional or mandatory. When mandatory (the default) an exception is raised 
		/// if the specified key is not found in the configuration file.
		/// </summary>
		private ConfigKeyPresence keyPresenceRequirement = ConfigKeyPresence.Mandatory;

		/// <summary>
		/// Creates a new ConfigurationAttribute using the given path as the XPath
		/// to the target node in the configuration tree. The node must be present or an
		/// exception will be raised.
		/// </summary>
		/// <param name="xmlNodePath">The XPath to the node. This should always start with the
		/// root node "Gentle.Framework" (this is also true when the .NET configuration file
		/// is used instead of Gentle.config).</param>
		public ConfigurationAttribute( string xmlNodePath )
		{
			this.xmlNodePath = xmlNodePath;
		}

		/// <summary>
		/// Creates a new ConfigurationAttribute using the given path as the XPath
		/// to the target node in the configuration tree. The node must be present or an
		/// exception will be raised.
		/// </summary>
		/// <param name="xmlNodePath">The XPath to the node. This should always start with the
		/// root node "Gentle.Framework" (this is also true when the .NET configuration file
		/// is used instead of Gentle.config).</param>
		/// <param name="keyPresenceRequirement">The presence requirement of the specified key.
		/// Refer to <see cref="ConfigKeyPresence"/> for the available options.</param>
		public ConfigurationAttribute( string xmlNodePath, ConfigKeyPresence keyPresenceRequirement )
		{
			this.xmlNodePath = xmlNodePath;
			this.keyPresenceRequirement = keyPresenceRequirement;
		}

		#region Public Properties
		/// <summary>
		/// The XML path to the node containing the configuration value(s).
		/// </summary>
		public string XmlNodePath
		{
			get { return xmlNodePath; }
			set { xmlNodePath = value; }
		}

		/// <summary>
		/// The number of callback method parameters whose presence is required in order
		/// for the method to be eligible for invocation. Only parameters after this 
		/// number of parameters will be considered for null or default value substition.
		/// </summary>
		public int RequiredParameters
		{
			get { return requiredParameters; }
			set { requiredParameters = value; }
		}
		/// <summary>
		/// This denotes whether the configuration key used for a configuration target
		/// is optional or mandatory. When mandatory (the default) an exception is raised 
		/// if the specified key is not found in the configuration file.
		/// </summary>
		public ConfigKeyPresence KeyPresenceRequirement
		{
			get { return keyPresenceRequirement; }
		}
		#endregion
	}
}