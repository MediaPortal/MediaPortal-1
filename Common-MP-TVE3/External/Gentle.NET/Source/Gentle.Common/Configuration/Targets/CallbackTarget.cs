/*
 * Configuration target for methods (see below for details)
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: CallbackTarget.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System.Collections;
using System.Reflection;
using System.Xml;

namespace Gentle.Common
{
	/// <summary>
	/// This class is used for configuration elements that result in a method call. 
	/// Use this target when you need to obtain a list of elements from the configuration
	/// file; the callback method will be called once for every node matching the path.
	/// </summary>
	internal class CallbackTarget : ElementTarget
	{
		private object _lock = new object();
		private MethodDispatcher dispatcher;

		public CallbackTarget( ConfigurationAttribute ca, MethodInfo methodInfo ) : base( ca )
		{
			dispatcher = new MethodDispatcher( new MethodInvoker( methodInfo, ca.RequiredParameters ) );
		}

		public void AddCallbackMethod( MethodInfo methodInfo, int requiredParameters )
		{
			lock( _lock )
			{
				dispatcher.AddInvoker( new MethodInvoker( methodInfo, requiredParameters ) );
			}
		}

		private Hashtable ExtractAttributes( XmlNode node )
		{
			Hashtable result = new Hashtable();
			foreach( XmlAttribute attr in node.Attributes )
			{
				result[ attr.Name.ToLower() ] = attr.Value;
			}
			return result;
		}

		/// <summary>
		/// Use the supplied XmlNode to configure the target object. This configuration target 
		/// performs a method callback on the target object, and uses the attributes of the XmlNode
		/// as parameters. The method parameter names must match the names of the node attributes 
		/// (a leading underscore will be stripped to permit using C# reserved words in the XML file). 
		/// This method does not check whether the node matches the requested environment.
		/// </summary>
		/// <param name="target">The object to cofigure.</param>
		/// <param name="node">The node containing the configuration value(s).</param>
		public override void Configure( object target, XmlNode node )
		{
			// extract method parameters from node attributes
			Hashtable parameters = ExtractAttributes( node );
			// execute the (best matching) callback using helper class
			dispatcher.Invoke( target, parameters );
		}

		/// <summary>
		/// Use the supplied XmlNodes to configure the target object. This method filters the
		/// given list of nodes by environment before applying them individually. All nodes 
		/// matching the current environment AND all nodes not limited by an environment
		/// attribute are used for configuration.
		/// </summary>
		/// <param name="target">The object to cofigure.</param>
		/// <param name="nodes">The list of nodes containing the configuration value(s).</param>
		public override void Configure( object target, XmlNodeList nodes )
		{
			foreach( XmlNode node in nodes )
			{
				if( IsEnvironment( node, SystemSettings.Environment ) )
				{
					Configure( target, node );
				}
				else if( IsCommonEnvironment( node ) )
				{
					Configure( target, node );
				}
			}
		}
	}
}