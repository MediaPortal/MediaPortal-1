/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

namespace MediaPortal.Utils.Web
{
	public class HTMLCache
	{
		const string CACHE_DIR = "WebCache";
    static bool _initialised = false;
    static Mode _cacheMode = Mode.Disabled;
		static string _strPageSource;

    public enum Mode
    {
      Disabled = 0,
      Enabled = 1,
      Replace = 2
    }

		static HTMLCache()
		{
		}

		static public void WebCacheInitialise()
		{
			if(!System.IO.Directory.Exists(CACHE_DIR))
				System.IO.Directory.CreateDirectory(CACHE_DIR);

      _initialised = true;
		}

    static public bool Initialised
    {
      get { return _initialised; }
    }

    static public Mode CacheMode
    {
      get { return _cacheMode; }
      set { _cacheMode = value; }
    }

    static public void DeleteCachePage(Uri pageUri)
		{
			string file = GetCacheFileName(pageUri);

			if(System.IO.File.Exists(file))
				System.IO.File.Delete(file);
		}

		static public bool LoadPage(Uri pageUri)
		{
			if(_cacheMode == Mode.Enabled)
			{
				if(LoadCacheFile(GetCacheFileName(pageUri)))
					return true;
			}
			return false;
    }

		static public void SavePage(Uri pageUri, string strSource)
		{
			if(_cacheMode != Mode.Disabled)
				SaveCacheFile(GetCacheFileName(pageUri), strSource);
		}

    static public string GetPage() //string strURL, string strEncode)
    {
      return _strPageSource;
    }

		static private bool LoadCacheFile(string file)
		{
			if(System.IO.File.Exists(file))
			{
				TextReader CacheFile = new StreamReader(file);
				_strPageSource = CacheFile.ReadToEnd();
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

    static private string GetCacheFileName(Uri Page)
		{
			int hash = Page.GetHashCode();
				
			return CACHE_DIR + "/" + hash.ToString() + ".html";
		}
  }
}
