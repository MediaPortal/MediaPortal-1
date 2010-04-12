/*
 * Interface for persistence objects that perform validations.
 * Copyright (C) 2005 Clayton Harbour
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: IValidationPersistent.cs $
 */

using System.Collections;

namespace Gentle.Framework
{
	/// <summary>
	/// Interface for <see cref="Persistent"/> objects that want to implement validations.
	/// </summary>
	public interface IValidationPersistent
	{
		IList ValidationMessages { get; }
	}
}