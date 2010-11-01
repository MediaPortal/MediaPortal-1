/*
 * Placeholder for future implementations.
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: GentleRenderer.cs 646 2005-02-21 20:28:03Z mm $
 */

namespace Gentle.Framework
{
	public class GentleRenderer
	{
		private IGentleProvider provider;

		public GentleRenderer( IGentleProvider provider )
		{
			this.provider = provider;
		}
	}
}