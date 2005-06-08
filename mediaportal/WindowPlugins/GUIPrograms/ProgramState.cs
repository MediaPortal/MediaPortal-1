using System;
using MediaPortal.GUI.Library;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for ProgramState.
  /// </summary>
  public class ProgramState
  {
    static int m_iStartWindow = (int) GUIWindow.Window.WINDOW_FILES;
    static string view = "";

    public ProgramState()
    {
      //
      // TODO: Add constructor logic here
      //
    }

    static public int StartWindow
    {
      get { return m_iStartWindow; }
      set { m_iStartWindow = value; }
    }

    static public string View
    {
      get { return view; }
      set { view = value; }
    }
  }

}