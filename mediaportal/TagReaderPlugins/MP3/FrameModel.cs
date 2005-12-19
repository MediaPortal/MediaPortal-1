/* 
 *	Copyright (C) 2005 Team MediaPortal
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

// Copyright(C) 2002 Hugo Rumayor Montemayor, All rights reserved.
using System;

namespace id3
{
	/// <summary>
	/// Summary description for FrameModel.
	/// </summary>
	public class FrameModel
	{
		public FrameModel()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private Header _tagHeader  = new Header();
		private ExtendedHeader _tagExtendedHeader  = new ExtendedHeader();

		public Header Header
		{
			get{ return _tagHeader;}
			set{ _tagHeader = value;}
		}

	}
}
