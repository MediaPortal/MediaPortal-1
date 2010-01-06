#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System.Threading;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.Threading;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUISettingsMusic : GUIInternalWindow
  {
    private enum Controls
    {
      CONTROL_AUTOSHUFFLE = 2,
      CONTROL_VISUALISATION = 4,
      CONTROL_ENABLECDDB = 5,
      CONTROL_ENABLEID3 = 6,
      CONTROL_BTNREORGDB = 8,
      CONTROL_BTNDELALBUMINFO = 10,
      CONTROL_BTNDELCDDB = 12,
      CONTROL_BTNDELALBUM = 13,
      CONTROL_MUSICREPEAT = 14,
      CONTROL_USEPLAYLIST = 15,
      CONTROL_AUTOSWITCH = 16,
      CONTROL_AUTOSWITCHBIG = 17
    } ;

    private bool m_bAutoShuffle;
    private bool m_bUseID3;

    public GUISettingsMusic()
    {
      GetID = (int)Window.WINDOW_SETTINGS_MUSIC;
    }

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        m_bAutoShuffle = xmlreader.GetValueAsBool("musicfiles", "autoshuffle", true);
        m_bUseID3 = xmlreader.GetValueAsBool("musicfiles", "showid3", m_bUseID3);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        xmlreader.SetValueAsBool("musicfiles", "autoshuffle", m_bAutoShuffle);
        xmlreader.SetValueAsBool("musicfiles", "showid3", m_bUseID3);
      }
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\SettingsMyMusic.xml");
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            GUIWindowManager.ShowPreviousWindow();
            return;
          }
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
            LoadSettings();

            if (m_bAutoShuffle)
            {
              GUIControl.SelectControl(GetID, (int)Controls.CONTROL_AUTOSHUFFLE);
            }
            if (m_bUseID3)
            {
              GUIControl.SelectControl(GetID, (int)Controls.CONTROL_ENABLEID3);
            }
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            SaveSettings();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;

            if (iControl == (int)Controls.CONTROL_AUTOSHUFFLE)
            {
              m_bAutoShuffle = !m_bAutoShuffle;
            }
            if (iControl == (int)Controls.CONTROL_ENABLEID3)
            {
              m_bUseID3 = !m_bUseID3;
            }

            if (iControl == (int)Controls.CONTROL_BTNDELALBUMINFO)
            {
              MusicDatabaseReorg dbreorg = new MusicDatabaseReorg();
              dbreorg.DeleteAlbumInfo();
            }
            if (iControl == (int)Controls.CONTROL_BTNDELALBUM)
            {
              MusicDatabaseReorg dbreorg = new MusicDatabaseReorg();
              dbreorg.DeleteSingleAlbum();
            }

            if (iControl == (int)Controls.CONTROL_BTNREORGDB)
            {
              GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
              if (null != dlgYesNo)
              {
                dlgYesNo.SetHeading(333);
                dlgYesNo.SetLine(1, "");
                dlgYesNo.SetLine(2, "");
                dlgYesNo.SetLine(3, "");
                dlgYesNo.DoModal(GetID);

                if (dlgYesNo.IsConfirmed)
                {
                  MusicDatabaseReorg reorg = new MusicDatabaseReorg(GetID);
                  Work work = new Work(new DoWorkHandler(reorg.ReorgAsync));
                  work.ThreadPriority = ThreadPriority.Lowest;
                  GlobalServiceProvider.Get<IThreadPool>().Add(work, QueuePriority.Low);
                }
              }
              /*
              GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
              if (null != dlgYesNo)
              {
                dlgYesNo.SetHeading(313);
                dlgYesNo.SetLine(1, "");
                dlgYesNo.SetLine(2, "");
                dlgYesNo.SetLine(3, "");
                dlgYesNo.DoModal(GetID);

                if (dlgYesNo.IsConfirmed)
                {
                  GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
                  if (null != dlgProgress)
                  {
                    dlgProgress.StartModal(GetID);

                    MusicDatabaseReorg reorg = new MusicDatabaseReorg();
                    int iReturnString = reorg.DoReorg();
                    dlgProgress.PageDestroy();

                    if (!dlgProgress.IsCanceled)
                    {
                      GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                      if (null != dlgOK)
                      {
                        dlgOK.SetHeading(313);
                        dlgOK.SetLine(1, iReturnString);
                        dlgOK.SetLine(2, "");
                        dlgOK.SetLine(3, "");
                        dlgOK.DoModal(GetID);
                      }
                    }
                  }
                }
              }
              */
            }
          }
          break;
      }
      return base.OnMessage(message);
    }
  }
}