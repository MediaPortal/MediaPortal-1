#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Drawing;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Alignment = MediaPortal.GUI.Library.GUIControl.Alignment;
using VAlignment = MediaPortal.GUI.Library.GUIControl.VAlignment;

namespace TvPlugin
{
  public class TVNotifyYesNoDialog: GUIDialogYesNo
  {
    [SkinControl(12)] protected GUIImage imgLogo = null;

    private bool m_bNeedRefresh = false;
    private string logoUrl = string.Empty;

    public TVNotifyYesNoDialog()
    {
      GetID = (int)Window.WINDOW_DIALOG_TVNOTIFYYESNO;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\dialogTVNotifyYesNo.xml");
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            if (imgLogo != null)
            {
              SetImage(logoUrl);
            }
          }
          return true;
      }

      return base.OnMessage(message);
    }

    public override void Reset()
    {
      logoUrl = string.Empty;
    }

    public override bool NeedRefresh()
    {
      if (m_bNeedRefresh)
      {
        m_bNeedRefresh = false;
        return true;
      }
      return false;
    }

    public void SetImage(string filename)
    {
      logoUrl = filename;
      if (MediaPortal.Util.Utils.FileExistsInCache(filename))            
      {
        if (imgLogo != null)
        {
          imgLogo.SetFileName(filename);
          m_bNeedRefresh = true;
          imgLogo.IsVisible = true;
        }
      }
      else
      {
        if (imgLogo != null)
        {
          imgLogo.IsVisible = false;
          m_bNeedRefresh = true;
        }
      }
    }

    public void SetImageDimensions(Size size, bool keepAspectRatio, bool centered)
    {
      if (imgLogo == null)
      {
        return;
      }
      imgLogo.Width = size.Width;
      imgLogo.Height = size.Height;
      imgLogo.KeepAspectRatio = keepAspectRatio;
      imgLogo.ImageAlignment = Alignment.ALIGN_CENTER;
      imgLogo.ImageVAlignment = VAlignment.ALIGN_MIDDLE;
    }
  }
}
