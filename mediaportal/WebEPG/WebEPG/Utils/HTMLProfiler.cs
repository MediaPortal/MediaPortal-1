using System;
//using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.WebEPGUtils;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

namespace MediaPortal.EPG
{
	public class HTMLProfiler : Profiler
	{
		bool m_bAhrefs;
		string m_strSubProfile;
		string m_strPageStart;
		string m_strPageEnd;

		public HTMLProfiler(string strSource, bool ahrefs)
		{
			m_strSource = strSource.Replace("\r", "");
			m_strSource = m_strSource.Replace("\n", "");
			m_strSource = m_strSource.Replace("\t", "");
			m_bAhrefs=ahrefs;
			TableProfiler();
		}

		public HTMLProfiler(string strSource, bool ahrefs, string PageStart, string PageEnd):this(strSource, ahrefs)
		{
			m_strPageStart = PageStart;
			m_strPageEnd = PageEnd;
		}

		public HTMLProfiler(string strSource, bool ahrefs, string strSubProfile):this(strSource, ahrefs)
		{
			m_strSubProfile = strSubProfile;
		}

		override public Profiler GetPageProfiler(string strURL)
		{
			HTMLPage webPage = new HTMLPage(strURL, true);
			if(!webPage.SetStart(m_strPageStart))
				Log.WriteFile(Log.LogType.Log, true, "WebEPG: Start String not found");
			if(!webPage.SetEnd(m_strPageEnd))
				Log.WriteFile(Log.LogType.Log, true, "WebEPG: End String not found");
			HTMLProfiler retProfiler = new HTMLProfiler(webPage.SubPage(), m_bAhrefs, ProfileString());
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
                index = nextSubProfile + 1;
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
						case 'P': // P
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
					}
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

			Regex searchRegex = new Regex(regex);
			Match result = searchRegex.Match(m_strSource, sourceStart, sourceLength);

			if(result.Success)
			{
				if(remove)
				{
					char[] sourceArray = m_strSource.ToCharArray();
					for(int i=result.Index; i < result.Index + result.Length; i++)
						sourceArray[i] = '\x06';
					m_strSource = new string(sourceArray);
				}
				return result.Value;
			}

			return "";
		}

		public string GetHyperLink(int profileIndex, string match)
		{
			string source = this.GetSource(profileIndex);

			int pos=0;
			string strLinkURL="";
			while((pos = source.IndexOf("<a href=", pos))!=-1)
			{
				pos+=9;
				int endIndex = source.IndexOf("\"", pos);
				if(endIndex != -1)
				{
					strLinkURL = source.Substring(pos, endIndex-pos);
					if(strLinkURL.IndexOf(match) != -1)
						break;
				}
				strLinkURL="";

			}

			return strLinkURL;
//
//			int startTag = m_subProfile[profileIndex,0];
//			int endTag = m_subProfile[profileIndex,1];
//			int sourceStart = this.m_arrayTagPos[startTag,0];
//			int sourceLength = this.m_arrayTagPos[endTag,1] - sourceStart + 1;
//			string source = this.m_strSource.Substring(sourceStart, sourceLength);
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

				switch (tagS)
				{
					case 'A':				// A HREF
						if (m_bAhrefs)
							profileIndex++;
						break;
					case 'P':				// P
					case 'T':				// TABLE, TH, TD, TR
						profileIndex++;
						break;
					default:				// All other tags BR, IMG, etc
						break;
				}
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
