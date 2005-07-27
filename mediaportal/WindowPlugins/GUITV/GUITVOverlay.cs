using System;
using System.Drawing;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Player;
namespace MediaPortal.GUI.TV
{
	/// <summary>
	/// 
	/// </summary>
	public class GUITVOverlay:GUIOverlayWindow
	{
    public GUITVOverlay()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TV_OVERLAY;
		}

    public override bool Init()
    {
      bool bResult=Load (GUIGraphicsContext.Skin+@"\tvOverlay.xml");
      GetID=(int)GUIWindow.Window.WINDOW_TV_OVERLAY;
      return bResult;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false;}
    }    
    
    public override void PreInit()
		{
			base.PreInit();
      AllocResources();
    
    }

    public override void Render(float timePassed)
    {
    }
    public override bool DoesPostRender()
    {
			if (GUIGraphicsContext.IsFullScreenVideo) return false;
			if (Recorder.IsAnyCardRecording()) return true;
			return false;
	  }

    public override void PostRender(float timePassed,int iLayer)
    {
      if (iLayer!=2) return;
      base.Render(timePassed);
    }
	}
}
