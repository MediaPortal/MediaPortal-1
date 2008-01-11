#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;

namespace System.Collections
{
	public sealed class NullEnumerable : IEnumerable
	{
		#region Constructors

		private NullEnumerable()
		{
		}
		
		#endregion Constructors

		#region Methods

		public IEnumerator GetEnumerator()
		{
			return NullEnumerator.Instance;
		}

		#endregion Methods

		#region Properties

		public static IEnumerable Instance
		{
			get { if(_instance == null) _instance = new NullEnumerable(); return _instance; }
		}

		#endregion Properties

		#region Fields

		static NullEnumerable		_instance;		

		#endregion Fields
	}
}
