using System;

namespace MediaPortal.Playlists
{
	/// <summary>
	/// 
	/// </summary>
	public class PlayListFactory
	{
		public PlayListFactory()
		{
		}
		static public PlayList Create(string strFileName)
		{
			string strExtension=System.IO.Path.GetExtension(strFileName);
			strExtension.ToLower();
			if (strExtension==".m3u")
			{
				return new PlayListM3U();
			}
			if (strExtension==".pls")
			{
				return new PlayListPLS();
			}
			if (strExtension==".b4s")
			{
				return new PlayListB4S();
			}
			return null;
		}

		static public bool IsPlayList(string strFileName)
		{
			string strExtension=System.IO.Path.GetExtension(strFileName);
			strExtension.ToLower();
			if (strExtension==".m3u") return true;
			if (strExtension==".pls") return true;
			if (strExtension==".b4s") return true;
			return false;
		}
	}
}
