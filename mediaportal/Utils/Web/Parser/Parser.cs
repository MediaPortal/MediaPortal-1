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
//using MediaPortal.Utils.Web.Parser;

namespace MediaPortal.Utils.Web
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Parser
	{
		int index;
		string [] _aProfile;

		public Parser(int size)
		{
			_aProfile = new string[size];
			index=0;
		}

		public void Add(string element)
		{
			if(index < _aProfile.Length)
				_aProfile[index++] = ReplaceSpecial(element);
		}

		public void GetData(Parser Listing, ref ParserData data)
		{
			int TagStart;
			int TagEnd;
			int ListStart;
			int ListEnd;
			string searchStart;
			string searchEnd;
			string tag;

			if(this._aProfile.Length != Listing._aProfile.Length)
			{
				data = null;
				return; //null;
			}

			for(int i = 0; i < this._aProfile.Length; i++)
			{
				if(this._aProfile[i].Length > 0)
				{
					string tempElement = this._aProfile[i];
					string listElement = Listing._aProfile[i];

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

						data.SetElement(tag, listElement.Substring(ListStart, ListEnd-ListStart));
					}
				}
			}
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
	}
}
