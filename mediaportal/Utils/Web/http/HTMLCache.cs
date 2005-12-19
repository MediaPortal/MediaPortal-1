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
using System.IO;
using System.Net;
using System.Web;
//using MediaPortal.Webepg.GUI.Library;

namespace MediaPortal.Utils.Web
{
	public class HTMLCache
	{
		const string CACHE_DIR = "WebCache";
		static public bool Caching = false;
		static string m_strPageSource;

		static HTMLCache()
		{
		}

		static public void WebCacheIntialise()
		{
			if(!System.IO.Directory.Exists(CACHE_DIR))
				System.IO.Directory.CreateDirectory(CACHE_DIR);

			Caching = true;
		}

		static public void DeleteCachePage(string strURL)
		{
			string file = GetCacheFileName(strURL);

			if(System.IO.File.Exists(file))
				System.IO.File.Delete(file);
		}

		static public bool LoadPage(string strURL)
		{
			if(Caching)
			{
				if(LoadCacheFile(GetCacheFileName(strURL)))
					return true;
			}
			return false;
        }

		static public void SavePage(string strURL, string strSource)
		{
			if(Caching)
				SaveCacheFile(GetCacheFileName(strURL), strSource);
		}

        static public string GetPage() //string strURL, string strEncode)
        {
            return m_strPageSource;
        }

		static private bool LoadCacheFile(string file)
		{
			if(System.IO.File.Exists(file))
			{
				TextReader CacheFile = new StreamReader(file);
				m_strPageSource = CacheFile.ReadToEnd();
				CacheFile.Close();

				return true;
			}

			return false;
		}

		static private void SaveCacheFile(string file, string source)
		{
			if(System.IO.File.Exists(file))
				System.IO.File.Delete(file);

			TextWriter CacheFile = new StreamWriter(file);
			CacheFile.Write(source);
			CacheFile.Close();
		}

		static private string GetCacheFileName(string strURL)
		{
			Uri Page = new Uri(strURL);

			int hash = Page.GetHashCode();
				
			return CACHE_DIR + "/" + hash.ToString() + ".html";
		}

    }
}
