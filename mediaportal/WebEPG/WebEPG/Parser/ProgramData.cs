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

using System;
using System.Text;
using MediaPortal.Utils.Web;

namespace MediaPortal.WebEPG
{
    public class ProgramData : ParserData
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

		override public void SetElement(string tag, string element)
		{
			switch (tag)
			{
				case "<#START>":
					StartTime = getTime(element);
					break;
				case "<#END>":
					EndTime = getTime(element);
					break;
				case "<#DAY>":
					Day = int.Parse(element);
					break;
				case "<#DESCRIPTION>":
					Description = element.Trim(' ', '\n', '\t');
					break;
				case "<#MONTH>":
					Month = element.Trim(' ', '\n', '\t');
					break;
				case "<#TITLE>":
					Title = element.Trim(' ', '\n', '\t');
					break;
				case "<#SUBTITLE>":
					SubTitle = element.Trim(' ', '\n', '\t');
					break;
				case "<#GENRE>":
					Genre = element.Trim(' ', '\n', '\t');
					break;
				default:
					break;
			}
		}

		private int[] getTime(string strTime)
		{
            if(strTime == "")
                return null;

			int sepPos;
            bool found = false;
			int[] iTime = new int[2];

			if ((sepPos = strTime.IndexOf(":")) != -1)
			{
				iTime[0] = int.Parse(strTime.Substring(0, sepPos));
				iTime[1] = int.Parse(strTime.Substring(sepPos+1, 2));
                found = true;
			}

			if ((sepPos = strTime.IndexOf(".")) != -1)
			{
				iTime[0] = int.Parse(strTime.Substring(0, sepPos));
				iTime[1] = int.Parse(strTime.Substring(sepPos+1, 2));
                found = true;
			}

			if ((sepPos = strTime.IndexOf("h")) != -1)
			{
				iTime[0] = int.Parse(strTime.Substring(0, sepPos));
				iTime[1] = int.Parse(strTime.Substring(sepPos+1, 2));
                found = true;
			}

            if (!found)
                return null;

			if (strTime.ToLower().IndexOf("pm") != -1 && iTime[0] != 0)
			{
				if(iTime[0] != 12)
					iTime[0] += 12;
			}

			if (strTime.ToLower().IndexOf("am") != -1 && iTime[0] == 12)
				iTime[0] = 0;

			return iTime;
		}
    }
}
