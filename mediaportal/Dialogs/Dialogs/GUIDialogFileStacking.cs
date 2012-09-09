#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using MediaPortal.GUI.Library;
using System.Collections;
using System.IO;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIDialogFileStacking : GUIDialogWindow
  {
    private int m_iSelectedFile = -1;
    private int m_iFrames = -1;
    private int m_iNumberOfFiles = 0;
    private int m_MaxNumberOfFiles = 0;
    private const int m_indexStackItemOffset = 100;
    private ArrayList lst_FilesNames;

    public GUIDialogFileStacking()
    {
      GetID = (int)Window.WINDOW_DIALOG_FILESTACKING;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\dialogFileStacking.xml"));
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        // User canceled selection, deselect file
        m_iSelectedFile = -1;
      }
      base.OnAction(action);
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
              GUIControl pControl = GetControl(i + m_indexStackItemOffset);
              if (pControl != null)
              {
                m_MaxNumberOfFiles = i;
                EnableControl(GetID, i + m_indexStackItemOffset);
                ShowControl(GetID, i + m_indexStackItemOffset);
                if (i < m_iNumberOfFiles)
                {
                  pControl.NavigateRight = i + 1 + m_indexStackItemOffset;
                }
                else
                {
                  pControl.NavigateRight = 101;
                }
              }
              else
              {
                Log.Error("Missing control ID ({0}) in dialogFileStacking.xml", i + m_indexStackItemOffset);
              }
            }

            // disable CD's we dont use
            for (int i = m_iNumberOfFiles + 1; i <= 40; ++i)
            {
              HideControl(GetID, i + m_indexStackItemOffset);
              DisableControl(GetID, i + m_indexStackItemOffset);
            }
            SetSkinProperties(0);
          }
          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            m_iSelectedFile = message.SenderControlId - m_indexStackItemOffset;
            PageDestroy();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
          {
            m_iSelectedFile = message.TargetControlId - 100;
            SetSkinProperties(m_iSelectedFile - 1);
          }
          break;
      }

      return base.OnMessage(message);
    }


    public override void Render(float timePassed)
    {
      if (m_iFrames <= 25)
      {
        // Dialog heading
        GUIControl pDialog = GetControl(1);
        GUIControl pDialogLine3 = GetControl(3); // Get Y-Position
        int distance = 32; // some safe value if skinner has used incorrect control IDs
        if (pDialog != null && pDialogLine3 != null)
        {
          GUIControl pControl = GetControl(m_indexStackItemOffset + 1);
          distance = (pDialog.Width - pControl.Width) / (m_MaxNumberOfFiles);

          // do not allow "loose stacking", i.e. less than half overlayed items
          if (distance > pControl.Width / 2)
          {
            distance = pControl.Width / 2;
          }

          // slide in...
          int dwStartPos = pDialog.XPosition + pDialog.Width - pControl.Width;
          for (int i = 1; i <= m_MaxNumberOfFiles; ++i)
          {
            pControl = GetControl(i + m_indexStackItemOffset);
            if (null != pControl)
            {
              float fStep = (m_MaxNumberOfFiles - i) * distance;
              fStep /= 25.0f;
              fStep *= m_iFrames;
              int dwPosX = (int)(dwStartPos - fStep);
              pControl.SetPosition(dwPosX, pDialogLine3.YPosition);
            }
          }
        }
        else
        {
          Log.Warn("GUIDialogFileStacking: Control(1) and control(3) is missing in common.dialog.xml");
          m_iFrames = 25;
        }
        if (m_iFrames == 25)
        {
          GUIControl pControl = GetControl(101);
          pControl.Focus = true;
        }
        m_iFrames++;
      }

      base.Render(timePassed);
    }

    public int SelectedFile
    {
      get { return m_iSelectedFile; }
    }

    public void SetFiles(ArrayList lstMovies)
    {
      SetNumberOfFiles(lstMovies.Count);
      lst_FilesNames = lstMovies;
    }

    public void SetNumberOfFiles(int iFiles)
    {
      //LoadSkin();
      AllocResources();
      InitControls();
      SetControlLabel(GetID, 1, GUILocalizeStrings.Get(6037));
      SetControlLabel(GetID, 2, GUILocalizeStrings.Get(6038));
      SetControlLabel(GetID, 3, string.Empty);
      SetControlLabel(GetID, 4, string.Empty);
      SetControlLabel(GetID, 5, string.Empty);
      m_iNumberOfFiles = iFiles;
      lst_FilesNames = new ArrayList();
    }

    private void SetSkinProperties(int seletedID)
    {
      if (seletedID >= 0 && seletedID < m_iNumberOfFiles && seletedID < lst_FilesNames.Count)
      {
        GUIPropertyManager.SetProperty("#stackedMovie.Selected",
                                       Path.GetFileNameWithoutExtension((string)lst_FilesNames[seletedID]));
        GUIPropertyManager.SetProperty("#stackedMovie.SelectedId", (seletedID + 1).ToString());
      }
      else
      {
        GUIPropertyManager.SetProperty("#stackedMovie.Selected", "");
        GUIPropertyManager.SetProperty("#stackedMovie.SelectedId", (seletedID + 1).ToString());
      }
    }
  }
}