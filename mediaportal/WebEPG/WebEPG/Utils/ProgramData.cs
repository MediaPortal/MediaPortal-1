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

using System;
//using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Webepg.TV.Database
{
    public class ProgramData
    {
        public string ChannelID = String.Empty;
        public string Title = String.Empty;
		public string SubTitle = String.Empty;
        public string Description = String.Empty;
		public string Month = String.Empty;
		public string Genre = String.Empty;
		public int Day = 0;
        public int[] StartTime;
        public int[] EndTime;

    }
}
