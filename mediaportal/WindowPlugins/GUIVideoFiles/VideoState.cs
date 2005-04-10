using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// 
	/// </summary>
	public class VideoState
	{
    static int m_iTempPlaylistWindow = 0;
    static string m_strTempPlaylistDirectory = "";
    static int m_iStartWindow = (int)GUIWindow.Window.WINDOW_VIDEOS;
		static string view;
    public VideoState()
    {
      // 
      // TODO: Add constructor logic here
      //
		}
		static public string View
		{
			get { return view; }
			set { view = value; }
		}
    static public string TempPlaylistDirectory
    {
      get { return m_strTempPlaylistDirectory; }
      set { m_strTempPlaylistDirectory = value; }
    }
    static public int TempPlaylistWindow
    {
      get { return m_iTempPlaylistWindow; }
      set { m_iTempPlaylistWindow = value; }
    }
    static public int StartWindow
    {
      get { return m_iStartWindow; }
      set { m_iStartWindow = value; }
    }
  }
}
