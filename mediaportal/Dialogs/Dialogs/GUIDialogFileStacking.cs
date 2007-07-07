#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

#endregion

using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIDialogFileStacking : GUIDialogWindow
  {
    int m_iSelectedFile = -1;
    int m_iFrames = -1;
    int m_iNumberOfFiles = 0;
    public GUIDialogFileStacking()
    {
      GetID = (int)Window.WINDOW_DIALOG_FILESTACKING;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\dialogFileStacking.xml");
    }
    
    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            m_iSelectedFile = -1;
            m_iFrames = 0;

            // enable the CD's
            for (int i = 1; i <= m_iNumberOfFiles; ++i)
            {
              EnableControl(GetID, i);
              ShowControl(GetID, i);
            }

            // disable CD's we dont use
            for (int i = m_iNumberOfFiles + 1; i <= 40; ++i)
            {
              HideControl(GetID, i);
              DisableControl(GetID, i);
            }
          }
          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            m_iSelectedFile = message.SenderControlId;
            PageDestroy();
          }
          break;
      }

      return base.OnMessage(message);
    }


    public override void Render(float timePassed)
    {
      if (m_iFrames <= 25)
      {
        // slide in...
        int dwScreenWidth = GUIGraphicsContext.Width;
        for (int i = 1; i <= m_iNumberOfFiles; ++i)
        {
          GUIControl pControl = GetControl(i);
          if (null != pControl)
          {
            int dwEndPos = dwScreenWidth - ((m_iNumberOfFiles - i) * 32) - 140;
            int dwStartPos = dwScreenWidth;
            float fStep = dwStartPos - dwEndPos;
            fStep /= 25.0f;
            fStep *= m_iFrames;
            int dwPosX = (int)(dwStartPos - fStep);
            pControl.SetPosition(dwPosX, pControl.YPosition);
          }
        }
        m_iFrames++;
      }

      base.Render(timePassed);
    }

    public int SelectedFile
    {

      get { return m_iSelectedFile; }
    }
    public void SetNumberOfFiles(int iFiles)
    {
      LoadSkin();
      AllocResources();
      InitControls();

      m_iNumberOfFiles = iFiles;
    }

  }
}

