using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;

namespace MediaPortal.Plugins.Process
{
  public class UnattendedWindow : GUIWindow
  {
    public UnattendedWindow()
    {
      GetID = (int)GUIWindow.Window.WINDOW_PSCLIENTPLUGIN_UNATTENDED;
    }

    public override bool Init()
    {
      MediaPortal.GUI.Library.Log.Info("PSClientPlugin.UnattendedWindow.Init");

      return Load(GUIGraphicsContext.Skin + @"\psclientplugin_unattended.xml");
    }
  }
}
