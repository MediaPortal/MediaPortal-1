/*
 * Configuration target for properties
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: PropertyTarget.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System.Reflection;
using System.Xml;

namespace Gentle.Common
{
	internal class PropertyTarget : ElementTarget
	{
		public readonly PropertyInfo PropertyInfo;

		public PropertyTarget( ConfigurationAttribute ca, PropertyInfo propertyInfo ) : base( ca )
		{
			PropertyInfo = propertyInfo;
		}

		/// <summary>
		/// Use the supplied XmlNode to configure the target object. This method does not check
		/// whether the node matches the requested environment.
		/// </summary>
		/// <param name="target">The object to cofigure.</param>
		/// <param name="node">The node containing the configuration value(s).</param>
		public override void Configure( object target, XmlNode node )
		{
			// convert string to target type
			object value = TypeConverter.Get( PropertyInfo.PropertyType, node );
			// set value
			PropertyInfo.SetValue( target, value, Reflector.InstanceCriteria, null, null, null );
		}
	}
}