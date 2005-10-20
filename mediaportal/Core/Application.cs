#region Copyright (C) 2005 Media Portal

/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Windows;

namespace MediaPortal
{
	public class App
	{
		#region Methods

		public object FindResource(object key)
		{
			if(_resources == null)
				return null;

			return _resources[key];
		}

		public void Run()
		{
			System.Windows.Forms.Application.Run();
		}

		#endregion Methods

		#region Properties

		public static App Current
		{
			get { if(_current == null) _current = new App(); return _current; }
		}

		public ResourceDictionary Resources
		{
			get { if(_resources == null) _resources = new ResourceDictionary(); return _resources; }
			set { _resources = value; }
		}

		#endregion Properties

		#region Fields

		static App					_current;
		ResourceDictionary			_resources;

		#endregion Fields
	}
}
