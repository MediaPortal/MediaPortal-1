using System;
using MediaPortal.GUI.Library;
namespace MediaPortal.GUI.Music
{
	/// <summary>
	/// 
	/// </summary>
	public class MusicState
	{
    static int      m_iTempPlaylistWindow=0;
    static string   m_strTempPlaylistDirectory="";
    static int      m_iStartWindow=(int)GUIWindow.Window.WINDOW_MUSIC_FILES;
		public MusicState()
		{
			// 
			// TODO: Add constructor logic here
			//
    }
    static public string TempPlaylistDirectory
    {
      get { return m_strTempPlaylistDirectory;}
      set {m_strTempPlaylistDirectory=value;}
    }
    static public int TempPlaylistWindow
    {
      get { return m_iTempPlaylistWindow;}
      set {m_iTempPlaylistWindow=value;}
    }
    static public int StartWindow
    {
      get { return m_iStartWindow;}
      set {m_iStartWindow=value;}
    }
	}
}
