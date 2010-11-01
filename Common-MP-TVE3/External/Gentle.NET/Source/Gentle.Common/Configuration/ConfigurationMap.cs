/*
 * Mapping class used to store information on elements to be configured
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ConfigurationMap.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace Gentle.Common
{
	/// <summary>
	/// This class maintains information on the configurable elements of a class.
	/// </summary>
	internal class ConfigurationMap
	{
		private IList instanceTargets = new ArrayList();
		private Hashtable instanceOverloads = new Hashtable();
		private IList staticTargets = new ArrayList();
		private Hashtable staticOverloads = new Hashtable();

		#region Constructors
		/// <summary>
		/// Construct a new ConfigurationMap for the given type. This involves reflecting 
		/// on the given type to find all static and instance members carrying 
		/// the <see cref="ConfigurationAttribute"/>.
		/// </summary>
		public ConfigurationMap( Type type )
		{
			// find instance members tagged for configuration
			IList memberAttributeInfos = Reflector.FindMembers( Reflector.InstanceCriteria, type, false, typeof(ConfigurationAttribute) );
			foreach( MemberAttributeInfo mai in memberAttributeInfos )
			{
				ConfigurationAttribute ca = mai.Attributes[ 0 ] as ConfigurationAttribute;
				switch( mai.MemberInfo.MemberType )
				{
					case MemberTypes.Method:
						CallbackTarget target = null;
						if( instanceOverloads.ContainsKey( ca.XmlNodePath ) )
						{
							target = instanceOverloads[ ca.XmlNodePath ] as CallbackTarget;
						}
						if( target != null )
						{
							target.AddCallbackMethod( mai.MemberInfo as MethodInfo, ca.RequiredParameters );
						}
						else
						{
							target = new CallbackTarget( ca, mai.MemberInfo as MethodInfo );
							instanceTargets.Add( target );
							instanceOverloads[ ca.XmlNodePath ] = target;
						}
						break;
					case MemberTypes.Field:
						instanceTargets.Add( new FieldTarget( ca, mai.MemberInfo as FieldInfo ) );
						break;
					case MemberTypes.Property:
						instanceTargets.Add( new PropertyTarget( ca, mai.MemberInfo as PropertyInfo ) );
						break;
					default:
						Check.LogError( LogCategories.Metadata, "Unknown configuration target type for member {0} on class {1}.", mai.MemberInfo.Name, type );
						break;
				}
			}
			// find static members tagged for configuration
			memberAttributeInfos = Reflector.FindMembers( Reflector.StaticCriteria, type, false, typeof(ConfigurationAttribute) );
			foreach( MemberAttributeInfo mai in memberAttributeInfos )
			{
				ConfigurationAttribute ca = mai.Attributes[ 0 ] as ConfigurationAttribute;
				switch( mai.MemberInfo.MemberType )
				{
					case MemberTypes.Method:
						CallbackTarget target = null;
						if( staticOverloads.ContainsKey( ca.XmlNodePath ) )
						{
							target = staticOverloads[ ca.XmlNodePath ] as CallbackTarget;
						}
						if( target != null )
						{
							target.AddCallbackMethod( mai.MemberInfo as MethodInfo, ca.RequiredParameters );
						}
						else
						{
							target = new CallbackTarget( ca, mai.MemberInfo as MethodInfo );
							staticTargets.Add( target );
							staticOverloads[ ca.XmlNodePath ] = target;
						}
						break;
					case MemberTypes.Field:
						staticTargets.Add( new FieldTarget( ca, mai.MemberInfo as FieldInfo ) );
						break;
					case MemberTypes.Property:
						staticTargets.Add( new PropertyTarget( ca, mai.MemberInfo as PropertyInfo ) );
						break;
					default:
						Check.LogError( LogCategories.Metadata, "Unknown configuration target type for member {0} on class {1}.", mai.MemberInfo.Name, type );
						break;
				}
			}
		}
		#endregion

		#region Client Methods (for servicing the Configurator class)
		/// <summary>
		/// Configure all targets in the specified object using the given ElementTree 
		/// as source. If a Type is passed in obj, static members will be configured.
		/// If an object instance is passed in obj, instance members will be configured.
		/// </summary>
		/// <param name="handlers">The list of handlers providing the source values</param>
		/// <param name="obj">The object to be configured</param>
		public void Configure( IList handlers, object obj )
		{
			IList targets = obj is Type ? staticTargets : instanceTargets;
			foreach( ElementTarget target in targets )
			{
				int maxHandlerIndex = target is CallbackTarget ? handlers.Count : 1;
				bool keyFound = false;
				for( int i = 0; i < maxHandlerIndex; i++ )
				{
					ConfigurationHandler handler = handlers[ i ] as ConfigurationHandler;
					if( target is CallbackTarget )
					{
						XmlNodeList nodes = handler.GetNodes( target.XmlNodePath );
						if( nodes != null && nodes.Count > 0 )
						{
							target.Configure( obj, nodes );
							keyFound = true;
							break;
						}
					}
					else
					{
						XmlNode node = handler.GetNode( target.XmlNodePath );
						if( node != null )
						{
							target.Configure( obj, node );
							keyFound = true;
							break;
						}
					}
				}
				VerifyKeyPresence( target, keyFound );
			}
		}
		#endregion

		#region Private Helper Methods
		private bool VerifyKeyPresence( ElementTarget target, bool keyFound )
		{
			if( ! keyFound && target.KeyPresenceRequirement != ConfigKeyPresence.Optional )
			{
				Check.Fail( Error.MissingConfigurationKey, target.XmlNodePath );
			}
			return keyFound;
		}
		#endregion
	}
}