using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using MediaPortal.GUI.Library;


namespace MyMail
{
	/// <summary>
	/// Zusammenfassung für MailOverlay.
	/// </summary>
	public class MailOverlay : GUIWindow
	{
    bool Enabled=false;
		enum Controls
		{
			CONTROL_INFO=2
		}
		public MailOverlay()
		{
			//
			// TODO: Fügen Sie hier die Konstruktorlogik hinzu
			//
		}
		public override bool DoesPostRender()
		{
      if (!Enabled) return false;
			if (GUIGraphicsContext.IsFullScreenVideo) return false;
			if (!GUIGraphicsContext.Overlay) return false;
			return true;
		}

		public override bool Init()
		{
			bool bResult=Load (GUIGraphicsContext.Skin+@"\mailnotify.xml");
			GetID=8002;

			GUIFadeLabel fader =(GUIFadeLabel)GetControl((int)Controls.CONTROL_INFO);
			if (fader!=null)
			{
				fader.IsVisible=false;// hide notification on init
			}

      if (PluginManager.IsPluginNameEnabled("My Mail"))
      {
        Enabled=true;
      }
			return bResult;
		}

		public override bool SupportsDelayedLoad
		{
			get { return false;}
		}    
		public override void PreInit()
		{
			AllocResources();
		}
		public override void PostRender(long timePassed, int iLayer)
		{
			if (iLayer!=3) return;
			GUIFadeLabel fader =(GUIFadeLabel)GetControl((int)Controls.CONTROL_INFO);
			if (fader!=null)
			{
				fader.AllowScrolling=true;
			}
			if (GUIGraphicsContext.Overlay==false) 
			{
				return;
			}
			base.Render(timePassed);
		}
	}
}
