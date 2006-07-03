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
using MediaPortal.Webepg.TV.Database;

namespace MediaPortal.WebEPGUtils
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Parser
	{
		int index;
		string [] m_aProfile;

		public Parser(int size)
		{
			m_aProfile = new string[size];
			index=0;
		}

		public void Add(string element)
		{
			if(index < m_aProfile.Length)
				m_aProfile[index++] = ReplaceSpecial(element);
		}

		public ProgramData GetProgram(Parser Listing)
		{
			int TagStart;
			int TagEnd;
			int ListStart;
			int ListEnd;
			string searchStart;
			string searchEnd;
			string tag;

			ProgramData program = new ProgramData();

			if(this.m_aProfile.Length != Listing.m_aProfile.Length)
				return null;

			for(int i = 0; i < this.m_aProfile.Length; i++)
			{
				if(this.m_aProfile[i].Length > 0)
				{
					string tempElement = this.m_aProfile[i];
					string listElement = Listing.m_aProfile[i];

					TagStart=0;
					TagEnd=0;
					ListEnd=0;
					while((TagStart = tempElement.IndexOf("<#", TagStart)) != -1)
					{
						tag="";
						searchStart = "";
						searchEnd = "";
						int t;
						for(t=0; t < tempElement.Length; t++)
						{
							if(tempElement[TagStart+t] == ':')
							{
								t++;
								while(tempElement[TagStart+t] != ',' && t < tempElement.Length)
								{
									searchStart += tempElement[TagStart+t];
									t++;
								}

								t++;
								while(tempElement[TagStart+t] != '>' && t < tempElement.Length)
								{
									searchEnd += tempElement[TagStart+t];
									t++;
								}
							}
							tag += tempElement[TagStart+t];
							if(tempElement[TagStart+t] == '>')
								break;
						}

						if(tempElement[TagStart+t] != '>')
							break; //error

						ListStart=ListEnd;
						if(TagStart > 0)
						{
							if(TagStart-TagEnd > 2)
								ListEnd++;
							if(searchStart == "")
								searchStart = tempElement[TagStart-1].ToString();
							ListStart = listElement.IndexOf(searchStart, ListEnd);
							if(ListStart == -1)
								break; // error start not found give up
							ListStart+=searchStart.Length;
						}
						TagStart += t;
						TagEnd = TagStart;
						ListEnd = listElement.Length;
						if(TagStart < tempElement.Length-1)
						{
							if(searchEnd == "")
								searchEnd = tempElement[TagStart+1].ToString();
							ListEnd = listElement.IndexOf(searchEnd, ListStart);
							if(ListEnd == -1)
								 ListEnd = listElement.Length; // error end not found .. take whole element
						}

						string element = listElement.Substring(ListStart, ListEnd-ListStart);

						switch (tag)
						{
							case "<#START>":
								program.StartTime = getTime(element);
								break;
							case "<#END>":
								program.EndTime = getTime(element);
								break;
							case "<#DAY>":
								program.Day = int.Parse(element);
								break;
							case "<#DESCRIPTION>":
								program.Description = element.Trim(' ', '\n', '\t');
								break;
							case "<#MONTH>":
								program.Month = element.Trim(' ', '\n', '\t'); //getMonth(element);
								break;
							case "<#TITLE>":
								program.Title = element.Trim(' ', '\n', '\t');
								break;
							case "<#SUBTITLE>":
								program.SubTitle = element.Trim(' ', '\n', '\t');
								break;
							case "<#GENRE>":
								program.Genre = element.Trim(' ', '\n', '\t');
								break;
							default:
								break;
						}
					}
				}
			}

			return program;
		}


		private int[] getTime(string strTime)
		{
			int sepPos;
			int[] iTime = new int[2];

			if ((sepPos = strTime.IndexOf(":")) != -1)
			{
				iTime[0] = int.Parse(strTime.Substring(0, sepPos));
				iTime[1] = int.Parse(strTime.Substring(sepPos+1, 2));
			}

			if ((sepPos = strTime.IndexOf(".")) != -1)
			{
				iTime[0] = int.Parse(strTime.Substring(0, sepPos));
				iTime[1] = int.Parse(strTime.Substring(sepPos+1, 2));
			}

			if ((sepPos = strTime.IndexOf("h")) != -1)
			{
				iTime[0] = int.Parse(strTime.Substring(0, sepPos));
				iTime[1] = int.Parse(strTime.Substring(sepPos+1, 2));
			}

			if (strTime.ToLower().IndexOf("pm") != -1 && iTime[0] != 0)
			{
				if(iTime[0] != 12)
					iTime[0] += 12;
			}

			if (strTime.ToLower().IndexOf("am") != -1 && iTime[0] == 12)
				iTime[0] = 0;

			return iTime;
		}

		private string ReplaceSpecial(string strSource)
		{
			int index = 0;
			string strDest = "";

			strSource = strSource.Replace("<br>", "\n");
			strSource = strSource.Replace("<BR>", "\n");
			strSource = strSource.Replace("&amp;", "&");
			strSource = strSource.Replace("&nbsp;", " ");
			strSource = strSource.Replace("&rsquo;", "’");

			while(index < strSource.Length)
			{
				if (strSource[index] == '&' && strSource[index+1]=='#')
				{
					index+=2;
					int ipos = 0;
					string szDigit="";
					while ( ipos < 12 && index < strSource.Length && Char.IsDigit(strSource[index])) 
					{
						szDigit+=strSource[index];
						ipos++;
						index++;
					}
					if(strSource[index] == ';')
						index++;
					int dig = Int32.Parse(szDigit);
					switch(dig)
					{
						case 145:
							strDest += '’';
							break;
						case 150:
							strDest += '-';
							break;
						default:
							strDest += (char) dig;
							break;
					}
				}
				else
				{
					strDest+= strSource[index++];
				}
			}

			return strDest;
		}	

		//			string profile template.GetProfile();
		//
		//			m_aTemplate = new string[(profile.Length*2)-1];

		//			int tempIndex = 0;
		//			for(int i = 0; i < profile.Length; i++)
		//			{
		//				m_aTemplate[tempIndex++] = profile[i];
		//				m_aTemplate[
		//		}
	}
}
