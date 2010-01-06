using MediaPortal.GUI.Library;

namespace MediaPortal.Plugins.Process
{
  public class UnattendedWindow : GUIInternalWindow
  {
    public UnattendedWindow()
    {
      GetID = (int)Window.WINDOW_PSCLIENTPLUGIN_UNATTENDED;
    }

    public override bool Init()
    {
      Log.Info("PSClientPlugin.UnattendedWindow.Init");

      return Load(GUIGraphicsContext.Skin + @"\psclientplugin_unattended.xml");
    }
  }
}