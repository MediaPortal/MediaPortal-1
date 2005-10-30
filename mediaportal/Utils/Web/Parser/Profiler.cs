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

namespace MediaPortal.Utils.Web
{
    abstract public class Profiler
    {
		protected int m_profileCount = 0;
        protected string m_strSource = string.Empty;
        protected string m_strProfile = string.Empty;

        public Profiler()
        {
        }

        virtual public string GetSource()
        {
            return m_strSource;
        }

		public string ProfileString()
		{
			return m_strProfile;
		}

		virtual public int subProfileCount()
		{
			return m_profileCount;
		}

		abstract public void GetParserData(int index, ref ParserData data);

		abstract public Profiler GetPageProfiler(string strURL);

		abstract public Parser GetProfileParser(int index);
    }
}
