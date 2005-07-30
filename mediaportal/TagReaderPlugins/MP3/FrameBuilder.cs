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

// Copyright(C) 2002 Hugo Rumayor Montemayor, All rights reserved.
using System;

namespace id3
{
	/// <summary>
	/// Build the adecuate frame parser
	/// </summary>
	public class FrameBuilder
	{
		/// <summary>
		/// Builds a frame base, based on the tag type
		/// </summary>
		public static FrameBase Build(string tag)
		{
			
			switch(tag)
			{
				case "USLT":
				case "COMM":
				{
					return new FrameLCText(tag);
				}
				case "APIC":
				{
					return new FrameAPIC(tag);
				}
				case "GEOB":
				{
					return new FrameGEOB(tag);
				}
			}
			if(tag[0] == 'T') //(TXXX) Decault Text tag
			{
				return new FrameText(tag);	//default
			}
			if(tag[0] == 'W') //(WXXX) Decault Text tag
			{
				return new FrameURL(tag);	//default
			}
			
			throw new Exception("Tag not impemented.");//TODO: Change this to a null FrameBase handler
		}
	}
}
