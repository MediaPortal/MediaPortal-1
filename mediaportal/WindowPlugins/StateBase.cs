using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;

namespace WindowPlugins
{
  public class StateBase
  {
    protected static int m_iTempPlaylistWindow = 0;
    protected static string m_strTempPlaylistDirectory = "";
    protected static int m_iStartWindow = 0;
    protected static string view;

    public StateBase()
    {
      // 
      // TODO: Add constructor logic here
      //
    }

    public static string View
    {
      get { return view; }
      set { view = value; }
    }

    public static string TempPlaylistDirectory
    {
      get { return m_strTempPlaylistDirectory; }
      set { m_strTempPlaylistDirectory = value; }
    }

    public static int TempPlaylistWindow
    {
      get { return m_iTempPlaylistWindow; }
      set { m_iTempPlaylistWindow = value; }
    }

    public static int StartWindow
    {
      get { return m_iStartWindow; }
      set { m_iStartWindow = value; }
    }
  }
}
