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
using System.Text;
using MediaPortal.Utils.Web.Http;

namespace MediaPortal.Utils.Web.Parser
{
    public class DataProfiler : Parser
    {
		char m_cTag;
		char m_cDelim;

        public DataProfiler(string strSource, char Tag, char Delim)
        {
            m_strSource = strSource;
            m_strProfile = strSource;
			m_cTag = Tag;
			m_cDelim = Delim;
			ProfilerStart();
        }

        override public string GetSource(int index)
        {
			int startTag = m_subProfile[index,0];
			int endTag = startTag + m_subProfile[index,1];
			int sourceStart = this.m_arrayTagPos[startTag,0];
			int sourceLength = this.m_arrayTagPos[endTag,1] - sourceStart + 1;
			return this.m_strSource.Substring(sourceStart, sourceLength);
        }

		override public Profiler GetPageProfiler(string strURL, string channelID)
		{
			HTMLPage webPage = new HTMLPage(strURL, false);
			Profiler retProfiler = new Profiler(webPage.GetPage(), m_cTag, m_cDelim);
			retProfiler.Template = GetProfileParser(0);
			return retProfiler;
		}

		override public ParserData GetProgramData(int index)
		{
			Parser Listing = this.GetProfileParser(index);
			return Template.GetProgram(Listing);
		}

		override public Parser GetProfileParser(int index)
		{
			Parser profileParser = new Parser(m_subProfile[index,1]*2);

			int startTag = m_subProfile[index,0];

			int sourceStart = 0; 
			int sourceLength = this.m_arrayTagPos[startTag,0];
			if(index > 0)
			{
				sourceStart = this.m_arrayTagPos[startTag-1,1] + 1;
				sourceLength = this.m_arrayTagPos[startTag,0] - sourceStart;
			}

			profileParser.Add(this.m_strSource.Substring(sourceStart, sourceLength));

			sourceStart = this.m_arrayTagPos[startTag,0];
			sourceLength = this.m_arrayTagPos[startTag,1] - sourceStart + 1;
			profileParser.Add(this.m_strSource.Substring(sourceStart, sourceLength));

			int i;
			for(i = 0; i < (m_subProfile[index,1] - 1); i++)
			{
				sourceStart = this.m_arrayTagPos[startTag+i, 1] + 1;
				sourceLength = this.m_arrayTagPos[startTag+i+1, 0] - sourceStart;
				profileParser.Add(this.m_strSource.Substring(sourceStart, sourceLength));

				sourceStart = this.m_arrayTagPos[startTag+i+1,0];
				sourceLength = this.m_arrayTagPos[startTag+i+1,1] - sourceStart + 1;
				profileParser.Add(this.m_strSource.Substring(sourceStart, sourceLength));
			}

			return profileParser;
		}

		private void ProfilerStart()
		{
			if (m_strSource.Length == 0)
				return;

			int index = 0;
			int profileIndex = 0;
			int tagCount = 0;
			int subProfileStart = 0;
			int subProfileIndex = 0;

			int [,] arrayProfilePos = new int[m_strSource.Length, 2];
			int [,] subProfilePos = new int[m_strSource.Length, 2];

			for(index = 0; index < m_strSource.Length; index++)
			{
				if(m_strSource[index] == m_cTag)
				{
					arrayProfilePos[profileIndex,0] = index;
					arrayProfilePos[profileIndex,1] = index;
					profileIndex++;
					tagCount++;
				}

				if(m_strSource[index] == m_cDelim)
				{
					arrayProfilePos[profileIndex,0] = index;
					arrayProfilePos[profileIndex,1] = index;
					profileIndex++;
					tagCount++;
					subProfilePos[subProfileIndex,0] = subProfileStart;
					subProfilePos[subProfileIndex,1] = tagCount;
					subProfileStart=profileIndex;
					tagCount=0;
					subProfileIndex++;
				}
			}

			m_arrayTagPos = new int[profileIndex,2];
			for (index = 0; index < profileIndex; index++)
			{
				m_arrayTagPos[index,0] = arrayProfilePos[index, 0];
				m_arrayTagPos[index,1] = arrayProfilePos[index, 1];
			}

			m_profileCount = subProfileIndex;
			m_subProfile = new int[subProfileIndex,2];
			for (index = 0; index < subProfileIndex; index++)
			{
				m_subProfile[index,0] = subProfilePos[index, 0];
				m_subProfile[index,1] = subProfilePos[index, 1];
			}
		}
    }
}
