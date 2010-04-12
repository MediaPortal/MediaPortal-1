/*
 * The non-static edition of the main access point into the framework
 * Copyright (C) 2005 Clayton Harbour
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ValidationBroker.cs $
 */

namespace Gentle.Framework
{
	/// <summary>
	/// Singleton used to perform validations on an object.
	/// </summary>
	public sealed class ValidationBroker
	{
		/// <summary>
		/// Provides a static interface to perform validations on an object.
		/// </summary>
		/// <param name="obj">The object instance being operated on</param>
		public static void Validate( object obj )
		{
			ObjectMap map = ObjectFactory.GetMap( null, obj );

			bool valid = true;
			foreach( ValidationMap va in map.Validations )
			{
				bool currentValid = va.Validate( obj );
				if( ! currentValid )
				{
					valid = false;
					if( obj is IValidationPersistent )
					{
						(obj as IValidationPersistent).ValidationMessages.Add( va.Message );
					}
				}
			}
			if( !valid )
			{
				throw new ValidationException();
			}
		}
	}
}