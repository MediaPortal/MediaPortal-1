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
using System.Text.RegularExpressions;
using MediaPortal.WebEPGUtils;
using MediaPortal.Util;
using MediaPortal.Webepg.GUI.Library;

namespace MediaPortal.EPG
{
	public class HTMLProfiler : Profiler
	{
		//bool m_bAhrefs;
		string m_strTags;
		string m_strSubProfile;
		string m_strPageStart;
		string m_strPageEnd;
		string m_strEncoding="";

		public HTMLProfiler(string strSource, string tags) //bool ahrefs)
		{
			m_strSource = strSource.Replace("\r", "");
			m_strSource = m_strSource.Replace("\n", "");
			m_strSource = m_strSource.Replace("\t", "");
			//m_bAhrefs=ahrefs;
			m_strTags = tags;
			TableProfiler();
		}

		public HTMLProfiler(string strSource, string tags, string PageStart, string PageEnd, string encoding):this(strSource, tags)
		{
			m_strPageStart = PageStart;
			m_strPageEnd = PageEnd;
			m_strEncoding = encoding;
		}

		public HTMLProfiler(string strSource, string tags, string strSubProfile):this(strSource, tags)
		{
			m_strSubProfile = strSubProfile;
		}

		override public Profiler GetPageProfiler(string strURL, string channelID)
		{
			HTMLPage webPage = new HTMLPage(strURL, true, m_strEncoding);
			if(!webPage.SetStart(m_strPageStart))
				Log.WriteFile(Log.LogType.Log, true, "WebEPG: Start String not found");
			if(!webPage.SetEnd(m_strPageEnd))
				Log.WriteFile(Log.LogType.Log, true, "WebEPG: End String not found");
			HTMLProfiler retProfiler = new HTMLProfiler(webPage.SubPage(), m_strTags, ProfileString());
			retProfiler.Template = GetProfileParser(0);
			return retProfiler;
		}


        override public int subProfileCount()
        {

			if(m_strProfile == null)
				return 0;

            int[] arraySubProfiles = new int[m_strProfile.Length];
            int count = 0;
            int index = 0;
            int nextSubProfile = 0;

            while ((nextSubProfile = m_strProfile.IndexOf(m_strSubProfile, index)) != -1)
            {
                arraySubProfiles[count] = nextSubProfile;
                count++;
                index = nextSubProfile + m_strSubProfile.Length - 1;
            }

            m_subProfile = new int[count, 2];
            for (index = 0; index < count; index++)
            {
                m_subProfile[index, 0] = arraySubProfiles[index];
                m_subProfile[index, 1] = m_strSubProfile.Length;
            }

			return count;
        }

		private int TagEnd(string strSource, int StartPos)
		{
			int index=0;
			int nesting=0;

			if (strSource[StartPos] == '<')
				index++;

			while (StartPos + index < strSource.Length)
			{
				if (strSource[StartPos + index] == '<')
					nesting++;
				if (strSource[StartPos + index] == '>')
				{
					if(nesting > 0)
						nesting--;
					else
						break;
				}
				index++;
			}

			return index;

		}

		protected string PreProcess(string strSource)
		{
			int index = 0;
			int tagLength;
			bool endTag;
			string strStripped = "";

			while (index < strSource.Length)
			{
				if (strSource[index] == '<')
				{
					endTag=false;
					if (strSource[index + 1] == '/')
					{
						index++;
						endTag=true;
					}
					char TagS = char.ToUpper(strSource[index + 1]);

					if(TagS == 'B' && char.ToUpper(strSource[index + 2]) == 'R')
					{
						strStripped += "<br>";
	
						while (index < strSource.Length &&
							strSource[index] != '>')
							index++;
						index++;
					}
					else
					{
						if(m_strTags.IndexOf(TagS) != -1 || TagS == '#')
						{
							tagLength = TagEnd(strSource, index);
							if(endTag)
								strStripped += '<';

							int copyLength = tagLength;
							if(TagS != 'A')
							{
								int strip;
								if((strip = strSource.IndexOf(' ', index, copyLength)) != -1)
									copyLength = strip;
							}
							strStripped += strSource.Substring(index, copyLength);
							strStripped += '>';

							index += tagLength + 1;
						}
						else
						{
							tagLength = TagEnd(strSource, index);
							index += tagLength + 1;
						}
					}


					/*
					switch (char.ToUpper(strSource[index + 1]))
					{
						case 'B': // BR
							if(char.ToUpper(strSource[index + 2]) == 'R')
							{
								strStripped += "<br>";
							}
							while (index < strSource.Length &&
								strSource[index] != '>')
								index++;
							index++;
							break;
						case 'D': // div
						case 'P': // P
						case 'L': // LI, LINK
						case 'S': // span
						case 'T': // table
						case '#': // My tags
							if(endTag)
								strStripped+='<';

							while (index < strSource.Length)
							{
								if (strSource[index] == ' ' || strSource[index] == '>')
									break;
								strStripped += strSource[index++];
							}
							while (index < strSource.Length &&
								strSource[index] != '>')
								index++;

							break;
						case 'A':
							tagLength = TagEnd(strSource, index);
							if(m_bAhrefs)
							{
								if(endTag)
									strStripped+='<';
								strStripped += strSource.Substring(index, tagLength);
								if(endTag)
									strStripped+='>';
							}
							index += tagLength + 1;
							break;
						default:  // all others FONT, B, etc remove
							tagLength = TagEnd(strSource, index);
							index += tagLength + 1;
							break;
					}*/
				}
				else
				{
					if(strSource[index] != '\x06')
						strStripped += strSource[index];

					index++;
				}

			}

			return strStripped;
		}

		override public Parser GetProfileParser(int index)
		{
			Parser profileParser = new Parser(m_subProfile[index,1]*2 - 1);

			int startTag = m_subProfile[index,0];
			int sourceStart = this.m_arrayTagPos[startTag,0];
			int sourceLength = this.m_arrayTagPos[startTag,1] - sourceStart + 1;
			string element = PreProcess(this.m_strSource.Substring(sourceStart, sourceLength));
			profileParser.Add(element);

			for(int i=0; i < (m_subProfile[index,1] - 1); i++)
			{
				sourceStart = this.m_arrayTagPos[startTag+i, 1] + 1;
				sourceLength = this.m_arrayTagPos[startTag+i+1, 0] - sourceStart;
				element = PreProcess(this.m_strSource.Substring(sourceStart, sourceLength));
				profileParser.Add(element);

				sourceStart = this.m_arrayTagPos[startTag+i+1,0];
				sourceLength = this.m_arrayTagPos[startTag+i+1,1] - sourceStart + 1;
				element = PreProcess(this.m_strSource.Substring(sourceStart, sourceLength));
				profileParser.Add(element);
			}

			return profileParser;
		}

		public string SearchRegex(int index, string regex, bool remove)
		{
			int startTag = m_subProfile[index,0];
			int endTag = m_subProfile[index,1];
			int sourceStart = this.m_arrayTagPos[startTag,0];
			int sourceLength = this.m_arrayTagPos[startTag+endTag,1] - sourceStart + 1;

			Match result = null;
			try
			{
				Regex searchRegex = new Regex(regex);
				result = searchRegex.Match(m_strSource.ToLower(), sourceStart, sourceLength);
			}
			catch(System.ArgumentException ex)
			{
				Log.WriteFile(Log.LogType.Log, true, "WebEPG: Regex error: {0} {1}", regex, ex.ToString());
				return "";
			}
			
			if(result.Success)
			{
				if(remove)
				{
					char[] sourceArray = m_strSource.ToCharArray();
					for(int i=result.Index; i < result.Index + result.Length; i++)
						sourceArray[i] = '\x06';
					m_strSource = new string(sourceArray);
				}
				return m_strSource.Substring(result.Index, result.Length);
			}

			return "";
		}

		public string GetHyperLink(int profileIndex, string match, string linkURL)
		{

			string regex = "<a href=[^>]*" + match.ToLower() + "[^>]*>";

			string result = SearchRegex(profileIndex, regex, false);

			string strLinkURL="";

			if(result != "")
			{
				int start = -1;
				char delim = '>';

				if((start = result.IndexOf("=")) != -1)
				{
					for(int i=0; i < result.Length - start; i++)
					{
						if(result[start+i] == '\"' || result[start+i] == '\'')
						{
							delim = result[start+i];
							break;
						}
					}
				}

				if(delim != '>')
				{
					start = -1;
					int end = -1;
					start = result.IndexOf(delim);
					if(start != -1)
						end = result.IndexOf(delim, ++start);
					if(end != -1)
						strLinkURL = result.Substring(start, end-start);
				}
			}

			if(strLinkURL.ToLower().IndexOf("http") == -1)
			{
				if(strLinkURL.ToLower().IndexOf("javascript") != -1)
				{
					string [] param = GetJavaSubLinkParams(strLinkURL);

					strLinkURL = linkURL;

					for(int i = 0; i < param.Length; i++)
						strLinkURL = strLinkURL.Replace("#" + (i + 1).ToString(), param[i]);
				}
				else
				{
					strLinkURL = linkURL + strLinkURL;
				}
			}

			return strLinkURL;
		}

		private string [] GetJavaSubLinkParams(string link)
		{

			int args = -1;
			int [,] param = null;
			int start = -1;

			if((start = link.IndexOf("(")) != -1)
			{
				args = 0;
				param = new int[link.Length - start,2];
				param[0,0] = start+1;
				for(int i = 0; i < link.Length - start; i++)
				{
					if(link[start + i] == ',')
					{
						param[args,1] = start+i;
						args++;
						param[args,0] = start+i+1;
					}
					if(link[start + i] == ')')
					{
						param[args,1] = start+i;
						break;
					}
				}
			}

			string [] array = null;
			if( args != -1 && param != null)
			{
				args++;
				array = new string[args];
				for(int i = 0; i < args; i++)
					array[i] = link.Substring(param[i,0], param[i,1]-param[i,0]).Trim('\"');
			}

			return array;
		}

        private void TableProfiler()
        {
            if (m_strSource.Length == 0)
                return;

			int index = 0;
			int nextTag = 0;
			int profileIndex = 0;
			int tagLength;
			char tag;
			char tagS;
			bool endTag;

            int [,] arrayProfilePos = new int[m_strSource.Length, 2];
			char [] arrayProfile = new char[m_strSource.Length];


            while (index < m_strSource.Length && 
				(nextTag = m_strSource.IndexOf('<', index)) != -1)
            {
                arrayProfilePos[profileIndex,0] = nextTag;

				nextTag++;

				endTag = false;
				if (m_strSource[nextTag] == '/')
				{
					nextTag++;
					endTag = true;
				}
				

				tag = char.ToUpper(m_strSource[nextTag]);
				tagS = tag;

				if (tag == 'T')
				{
					nextTag++;
					if(char.ToUpper(m_strSource[nextTag]) != 'A')
						tag = char.ToUpper(m_strSource[nextTag]);
				}

				tagLength = TagEnd(m_strSource, nextTag);
				nextTag += tagLength;

				arrayProfilePos[profileIndex,1] = nextTag;

				if (endTag)
					arrayProfile[profileIndex] = tag;
				else
					arrayProfile[profileIndex] = char.ToLower(tag);

				if(m_strTags.IndexOf(tagS) != -1)
					profileIndex++;
/*
				switch (tagS)
				{
					case 'A':				// A HREF
						if (m_bAhrefs)
							profileIndex++;
						break;
					case 'D':				// div
					case 'P':				// P
					case 'L':				// LI, LINK
					case 'S':				// Span
					case 'T':				// TABLE, TH, TD, TR
						profileIndex++;
						break;
					default:				// All other tags BR, IMG, etc
						break;
				}*/
                index = nextTag + 1;
            }


			m_strProfile = "";
			m_arrayTagPos = new int[profileIndex,2];
			for (index = 0; index < profileIndex; index++)
			{
				m_strProfile += arrayProfile[index];
				m_arrayTagPos[index,0] = arrayProfilePos[index, 0];
				m_arrayTagPos[index,1] = arrayProfilePos[index, 1];
			}

			if(m_subProfile == null)
			{
				m_subProfile = new int[1,2];
				m_subProfile[0,0]=0;
				m_subProfile[0,1]=m_strProfile.Length;
			}
        }
    }

}
