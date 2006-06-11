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
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using MediaPortal.GUI.Library;


namespace MyMail
{
  /// <summary>
  /// Zusammenfassung für MailOverlay.
  /// </summary>
  public class MailOverlay : GUIOverlayWindow
  {
    bool Enabled = false;
    enum Controls
    {
      CONTROL_INFO = 2
    }
    public MailOverlay()
    {
      GetID = 8002;
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
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mailnotify.xml");
      GetID = 8002;

      GUIFadeLabel fader = (GUIFadeLabel)GetControl((int)Controls.CONTROL_INFO);
      if (fader != null)
      {
        fader.IsVisible = false;// hide notification on init
      }

      if (PluginManager.IsPluginNameEnabled("My Mail"))
      {
        Enabled = true;
      }
      return bResult;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }
    public override void PreInit()
    {
      base.PreInit();
      AllocResources();
    }
    public override void PostRender(float timePassed, int iLayer)
    {
      if (iLayer != 3) return;
      GUIFadeLabel fader = (GUIFadeLabel)GetControl((int)Controls.CONTROL_INFO);
      if (fader != null)
      {
        fader.AllowScrolling = true;
      }
      if (GUIGraphicsContext.Overlay == false)
      {
        return;
      }
      base.Render(timePassed);
    }
  }
}
