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

using System;
using System.Collections.Generic;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Picture.Database;
using MediaPortal.Util;
using MediaPortal.Video.Database;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIDialogFile : GUIInternalWindow, IRenderLayer
  {
    #region Base Dialog Variables

    private bool m_bRunning = false;
    private int m_dwParentWindowID = 0;
    private GUIWindow m_pParentWindow = null;

    #endregion

    [SkinControl(10)] protected GUIButtonControl btnNo = null;
    [SkinControl(11)] protected GUIButtonControl btnYes = null;
    [SkinControl(12)] protected GUIButtonControl btnAlways = null;
    [SkinControl(13)] protected GUIButtonControl btnNever = null;
    [SkinControl(14)] protected GUIButtonControl btnCancel = null;
    [SkinControl(100)] protected GUIImage imgProgressBackground = null;
    [SkinControl(20)] protected GUIProgressControl prgProgressBar = null;

    private bool m_bCanceled = false;
    private bool m_bOverlay = false;
    private int m_iFileMode = -1;
    private GUIListItem m_itemSourceItem = null;
    private int m_iNrOfItems = 0;
    private long m_dwTotalSize = 0;
    private int m_iFileNr = 0;
    private DirectoryHistory m_history = new DirectoryHistory();
    private string sourceFolder = string.Empty;
    private string destinationFolder = string.Empty;
    private VirtualDirectory m_directory = null;
    private bool m_bButtonYes = false;
    private bool m_bButtonNo = false;
    private bool m_bAlways = false;
    private bool m_bNever = false;
    private bool m_bBusy = false;
    private bool m_bDialogActive = false;
    private bool m_bReload = false;
    private MusicDatabase dbMusic = null;
    private bool m_preselectDelete = false;

    public GUIDialogFile()
    {
      GetID = (int) Window.WINDOW_DIALOG_FILE;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogFile.xml");
    }

    public override bool SupportsDelayedLoad
    {
      get { return true; }
    }

    public override void PreInit()
    {
    }


    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        Close();
        return;
      }
      base.OnAction(action);
    }

    #region Base Dialog Members

    public void Close()
    {
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        if (m_bDialogActive)
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
          OnMessage(msg);
        }

        GUIWindowManager.UnRoute();
        m_pParentWindow = null;
        m_bRunning = false;
        GUIGraphicsContext.Overlay = m_bOverlay;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
    }

    public void Progress()
    {
      if (m_bRunning)
      {
        GUIWindowManager.Process();
      }
    }

    public void ProgressKeys()
    {
      if (m_bRunning)
      {
        //TODO
        //g_application.FrameMove();
      }
    }


    public void DoModal(int dwParentId)
    {
      m_bOverlay = GUIGraphicsContext.Overlay;
      m_dwParentWindowID = dwParentId;
      m_pParentWindow = GUIWindowManager.GetWindow(m_dwParentWindowID);
      if (null == m_pParentWindow)
      {
        m_dwParentWindowID = 0;
        return;
      }

      if (m_directory == null)
      {
        m_directory = new VirtualDirectory();
      }

      // show menu
      ShowFileMenu(m_itemSourceItem);
    }

    #endregion

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            base.OnMessage(message);
            m_pParentWindow = null;
            m_bRunning = false;
            GUIGraphicsContext.Overlay = m_bOverlay;
            //base.OnMessage(message);
            FreeResources();
            DeInitControls();
            GUILayerManager.UnRegisterLayer(this);
            return true;
          }
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            m_bDialogActive = true;
            base.OnMessage(message);
            GUIGraphicsContext.Overlay = base.IsOverlayAllowed;
            m_pParentWindow = GUIWindowManager.GetWindow(m_dwParentWindowID);
            GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
          }
          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iAction = message.Param1;
            int iControl = message.SenderControlId;
            if (btnCancel != null && iControl == (int) btnCancel.GetID)
            {
              m_bCanceled = true;
              if (!m_bBusy)
              {
                Close();
              }
            }

            if (btnYes != null && iControl == (int) btnYes.GetID)
            {
              if (!m_bBusy)
              {
                m_bBusy = true;
                FileItemMC(m_itemSourceItem);
                m_bBusy = false;
                Close();
              }
              else
              {
                m_bButtonYes = true;
              }
            }

            if (btnNo != null && iControl == (int) btnNo.GetID)
            {
              m_bButtonNo = true;
            }

            if (btnAlways != null && iControl == (int) btnAlways.GetID)
            {
              m_bAlways = true;
            }

            if (btnNever != null && iControl == (int) btnNever.GetID)
            {
              m_bNever = true;
            }
          }
          break;
      }

      if (m_pParentWindow != null)
      {
        if (message.TargetWindowId == m_pParentWindow.GetID)
        {
          return m_pParentWindow.OnMessage(message);
        }
      }
      return base.OnMessage(message);
    }


    public bool IsCanceled
    {
      get { return m_bCanceled; }
    }

    public void SetMode(int iMode)
    {
      m_iFileMode = iMode;
    }

    public void SetSourceItem(GUIListItem item)
    {
      m_itemSourceItem = item;
      m_preselectDelete = false;
    }

    public void SetSourceDir(string value)
    {
      sourceFolder = value;
    }

    public string GetSourceDir()
    {
      return sourceFolder;
    }

    public void SetDestinationDir(string value)
    {
      destinationFolder = value;
    }

    public void SetDirectoryStructure(VirtualDirectory value)
    {
      m_directory = value;
    }

    public string GetDestinationDir()
    {
      return destinationFolder;
    }

    public void SetNewHeading(string strLine)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1, 0, 0, null);
      msg.Label = strLine;
      OnMessage(msg);
    }

    public void SetHeading(string strLine)
    {
      LoadSkin();
      AllocResources();
      InitControls();

      SetLine(1, string.Empty);
      SetLine(2, string.Empty);
      SetLine(3, string.Empty);
      SetLine(4, string.Empty);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1, 0, 0, null);
      msg.Label = strLine;
      OnMessage(msg);
    }

    public void SetHeading(int iString)
    {
      SetHeading(GUILocalizeStrings.Get(iString));
    }

    public void SetLine(int iLine, string strLine)
    {
      if (iLine < 1)
      {
        return;
      }
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1 + iLine, 0, 0, null);
      msg.Label = strLine;
      OnMessage(msg);
    }

    public void SetLine(int iLine, int iString)
    {
      SetLine(iLine, GUILocalizeStrings.Get(iString));
    }

    public void SetPercentage(int iPercentage)
    {
      if (prgProgressBar != null)
      {
        prgProgressBar.Percentage = iPercentage;
      }
    }

    public void ShowProgressBar(bool bOnOff)
    {
      if (bOnOff)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE, GetID, 0, prgProgressBar.GetID, 0, 0,
                                        null);
        OnMessage(msg);
      }
      else
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_HIDDEN, GetID, 0, prgProgressBar.GetID, 0, 0,
                                        null);
        OnMessage(msg);
      }
    }

    public void PreselectDelete()
    {
      m_preselectDelete = true;
    }

    private void FileItemMC(GUIListItem item)
    {
      if (m_bCanceled)
      {
        return;
      }

      // source file name
      string strItemFileName = item.Path.Replace(sourceFolder, "");
      if (strItemFileName.StartsWith("\\") == true)
      {
        strItemFileName = strItemFileName.Remove(0, 1);
      }

      // Check protected share
      int iPincodeCorrect;
      if (m_directory.IsProtectedShare(item.Path, out iPincodeCorrect))
      {
        ShowError(513, item.Path);
        return;
      }

      // update dialog information			
      SetPercentage((m_iFileNr*100)/m_iNrOfItems);
      string strFileOperation = "";
      if (m_iFileMode == 1)
      {
        strFileOperation = GUILocalizeStrings.Get(116);
      }
      else
      {
        strFileOperation = GUILocalizeStrings.Get(115);
      }
      strFileOperation += " " + m_iFileNr.ToString() + "/" + m_iNrOfItems.ToString() + " " + GUILocalizeStrings.Get(507);
      SetNewHeading(strFileOperation);
      SetLine(1, strItemFileName);

      // Handle messages
      GUIWindowManager.Process();

      if (item.IsFolder)
      {
        if (item.Label != "..")
        {
          string path = destinationFolder + strItemFileName;
          try
          {
            DirectoryInfo di = Directory.CreateDirectory(path);
          }
          catch (Exception)
          {
            ShowError(514, path);
            m_bCanceled = true;
            return;
          }

          List<GUIListItem> items = m_directory.GetDirectoryUnProtectedExt(item.Path, false);
          foreach (GUIListItem subItem in items)
          {
            FileItemMC(subItem);
            if (m_bCanceled)
            {
              return;
            }
          }

          // Move?
          if (m_iFileMode == 1 && strItemFileName != "")
          {
            try
            {
              Directory.Delete(item.Path);
            }
            catch (Exception)
            {
              ShowError(515, item.Path);
              m_bCanceled = true;
              return;
            }
          }
        }
      }
      else if (!item.IsRemote)
      {
        // Move,Copy
        m_iFileNr++;
        bool doNot = false;
        try
        {
          if (File.Exists(destinationFolder + strItemFileName))
          {
            m_bButtonYes = false;
            m_bButtonNo = false;

            if (!m_bAlways && !m_bNever && !m_bCanceled)
            {
              // ask user what to do
              SetLine(2, 509);
              SetButtonsHidden(false);

              // wait for input
              while (!m_bButtonYes && !m_bButtonNo && !m_bAlways && !m_bNever && !m_bCanceled)
              {
                GUIWindowManager.Process();
              }
              SetLine(2, "");
              SetButtonsHidden(true);
            }

            if (m_bButtonYes || m_bAlways)
            {
              doNot = false;
              try
              {
                File.Delete(destinationFolder + strItemFileName);
              }
              catch (Exception)
              {
                ShowError(516, destinationFolder + strItemFileName);
                doNot = true;
              }
            }
            else
            {
              doNot = true;
            }
          }

          if (doNot == false)
          {
            FileInfo fi = new FileInfo(item.Path);
            if (m_iFileMode == 1)
            {
              fi.MoveTo(destinationFolder + strItemFileName);
              // delete from database
              DeleteFromDatabase(item);
            }
            else
            {
              fi.CopyTo(destinationFolder + strItemFileName, false);
            }
          }
        }
        catch (Exception)
        {
          // fatal error
          ShowError(517, item.Path);
          m_bCanceled = true;

          Log.Info("FileMenu Error: from {0} to {1} MC:{2}", item.Path, destinationFolder + strItemFileName, m_iFileMode);
        }
      }
    }

    private void FileItemGetNrOfFiles(GUIListItem item)
    {
      if (item.IsFolder)
      {
        if (item.Label != "..")
        {
          List<GUIListItem> items = m_directory.GetDirectoryUnProtectedExt(item.Path, false);
          foreach (GUIListItem subItem in items)
          {
            FileItemGetNrOfFiles(subItem);
          }
        }
      }
      else if (!item.IsRemote)
      {
        m_iNrOfItems++;
        m_dwTotalSize += item.FileInfo.Length;
      }
    }

    private void ShowError(int iError, string SourceOfError)
    {
      // ask user what to do
      SetLine(1, SourceOfError);
      SetLine(2, iError);
      SetButtonsHidden(false);
      GUIControl.HideControl(GetID, (int) btnAlways.GetID);
      GUIControl.HideControl(GetID, (int) btnNever.GetID);
      GUIControl.HideControl(GetID, (int) btnCancel.GetID);
      GUIControl.HideControl(GetID, (int) btnNo.GetID);

      // wait for input
      while (!m_bButtonYes)
      {
        GUIWindowManager.Process();
      }

      SetLine(1, "");
      SetLine(2, "");
      SetButtonsHidden(true);
      GUIControl.ShowControl(GetID, (int) btnCancel.GetID);
      GUIControl.ShowControl(GetID, (int) btnNo.GetID);
    }

    private void ShowErrorDialog(int iError, string SourceOfError)
    {
      GUIDialogOK dlgOK = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      if (dlgOK != null)
      {
        dlgOK.SetHeading(iError);
        dlgOK.SetLine(1, SourceOfError);
        dlgOK.SetLine(2, 502);
        dlgOK.DoModal(m_dwParentWindowID);
      }
    }

    private void SetButtonsHidden(bool bHide)
    {
      if (bHide)
      {
        GUIControl.HideControl(GetID, (int) btnAlways.GetID);
        GUIControl.HideControl(GetID, (int) btnNever.GetID);
        GUIControl.ShowControl(GetID, (int) imgProgressBackground.GetID);
        ShowProgressBar(true);
      }
      else
      {
        GUIControl.ShowControl(GetID, (int) btnAlways.GetID);
        GUIControl.ShowControl(GetID, (int) btnNever.GetID);
        GUIControl.HideControl(GetID, (int) imgProgressBackground.GetID);
        ShowProgressBar(false);
      }

      // Handle messages
      GUIWindowManager.Process();
    }

    public void ShowFileMenu(GUIListItem item)
    {
      m_bReload = false;
      int iPincodeCorrect;

      if (item == null)
      {
        return;
      }
      if (item.IsFolder && item.Label == "..")
      {
        return;
      }
      if (m_directory.IsProtectedShare(item.Path, out iPincodeCorrect))
      {
        ShowErrorDialog(513, item.Path);
        Close();
        return;
      }
      if (m_preselectDelete)
      {
        OnDeleteItem(item);
        m_bReload = true;
        return;
      }

      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(500); // File menu

      sourceFolder = Path.GetDirectoryName(item.Path);
      if ((destinationFolder != "") && (destinationFolder != sourceFolder))
      {
        dlg.AddLocalizedString(115); //copy
        if (!Util.Utils.IsDVD(item.Path))
        {
          dlg.AddLocalizedString(116); //move					
        }
      }
      if (!Util.Utils.IsDVD(item.Path))
      {
        dlg.AddLocalizedString(118); //rename				
      }
      if (!Util.Utils.IsDVD(item.Path))
      {
        dlg.AddLocalizedString(117); //delete
      }
      if (!Util.Utils.IsDVD(item.Path))
      {
        dlg.AddLocalizedString(119); //new folder
      }

      if (item.IsFolder && !Util.Utils.IsDVD(item.Path))
      {
        dlg.AddLocalizedString(501); // Set as destination
      }
      if (destinationFolder != "")
      {
        dlg.AddLocalizedString(504); // Goto destination
      }

      dlg.DoModal(m_dwParentWindowID);
      if (dlg.SelectedId == -1)
      {
        return;
      }
      switch (dlg.SelectedId)
      {
        case 117: // delete
          OnDeleteItem(item);
          m_bReload = true;
          break;

        case 118: // rename
          {
            string strSourceName = "";
            string strExtension = Path.GetExtension(item.Path);

            if (item.IsFolder && !VirtualDirectory.IsImageFile(strExtension))
            {
              strSourceName = Path.GetFileName(item.Path);
            }
            else
            {
              strSourceName = Path.GetFileNameWithoutExtension(item.Path);
            }

            string strDestinationName = strSourceName;

            if (GetUserInputString(ref strDestinationName) == true)
            {
              if (item.IsFolder && !VirtualDirectory.IsImageFile(strExtension))
              {
                // directory rename
                if (Directory.Exists(sourceFolder + "\\" + strSourceName))
                {
                  try
                  {
                    Directory.Move(sourceFolder + "\\" + strSourceName, sourceFolder + "\\" + strDestinationName);
                  }
                  catch (Exception)
                  {
                    ShowErrorDialog(dlg.SelectedId, sourceFolder + "\\" + strSourceName);
                  }
                  m_bReload = true;
                }
              }
              else
              {
                // file rename
                if (File.Exists(item.Path))
                {
                  string strDestinationFile = sourceFolder + "\\" + strDestinationName + strExtension;
                  try
                  {
                    File.Move(item.Path, strDestinationFile);
                  }
                  catch (Exception)
                  {
                    ShowErrorDialog(dlg.SelectedId, sourceFolder + "\\" + strSourceName);
                  }
                  m_bReload = true;
                }
              }
            }
          }
          break;

        case 115: // copy				
          {
            SetMode(0); // copy
            FileItemDialog();
          }
          break;

        case 116: // move
          {
            SetMode(1); // move
            FileItemDialog();
            m_bReload = true;
          }
          break;

        case 119: // make dir
          {
            MakeDir();
            m_bReload = true;
          }
          break;

        case 501: // set as destiantion
          destinationFolder = Path.GetFullPath(item.Path) + "\\";
          break;

        case 504: // goto destination
          {
            sourceFolder = destinationFolder;
            m_bReload = true;
          }
          break;
      }
    }

    private void FileItemDialog()
    {
      // active this window...
      GUIWindowManager.IsSwitchingToNewWindow = true;
      GUIWindowManager.RouteToWindow(GetID);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, m_dwParentWindowID, 0,
                                      null);
      OnMessage(msg);

      string strFileOperation = "";
      if (m_iFileMode == 1)
      {
        strFileOperation = GUILocalizeStrings.Get(116);
      }
      else
      {
        strFileOperation = GUILocalizeStrings.Get(115);
      }

      SetHeading(strFileOperation);
      SetLine(1, 505);
      SetButtonsHidden(true);
      ShowProgressBar(true);
      SetPercentage(0);
      GUIWindowManager.Process();

      // calc nr of files
      m_dwTotalSize = 0;
      m_iNrOfItems = 0;
      FileItemGetNrOfFiles(m_itemSourceItem);

      // set number of objects
      strFileOperation += " " + m_iNrOfItems.ToString() + " " + GUILocalizeStrings.Get(507) + " (";
      if (m_dwTotalSize > 1024*1024)
      {
        strFileOperation += (m_dwTotalSize/(1024*1024)).ToString() + " MB)";
      }
      else if (m_dwTotalSize > 1024)
      {
        strFileOperation += (m_dwTotalSize/1024).ToString() + " KB)";
      }
      else
      {
        strFileOperation += m_dwTotalSize.ToString() + " Bytes)";
      }
      SetNewHeading(strFileOperation);
      SetLine(1, GUILocalizeStrings.Get(508) + " \"" + destinationFolder + "\" ?");

      m_bButtonYes = false;
      m_bCanceled = false;
      m_bBusy = false;
      m_bButtonNo = false;
      m_bAlways = false;
      m_bNever = false;
      m_bReload = false;
      m_iFileNr = 1;
      sourceFolder = Path.GetDirectoryName(m_itemSourceItem.Path);

      GUIWindowManager.IsSwitchingToNewWindow = false;

      m_bRunning = true;
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
    }

    private bool GetUserInputString(ref string sString)
    {
      VirtualKeyboard keyBoard = (VirtualKeyboard) GUIWindowManager.GetWindow((int) Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyBoard)
      {
        return false;
      }
      keyBoard.IsSearchKeyboard = true;
      keyBoard.Reset();
      keyBoard.Text = sString;
      keyBoard.DoModal(m_dwParentWindowID); // show it...
      if (keyBoard.IsConfirmed)
      {
        sString = keyBoard.Text;
      }
      return keyBoard.IsConfirmed;
    }

    private void OnDeleteItem(GUIListItem item)
    {
      if (item.IsRemote)
      {
        return;
      }

      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo)
      {
        return;
      }
      string strFileName = Path.GetFileName(item.Path);
      if (!item.IsFolder)
      {
        if (Util.Utils.IsAudio(item.Path))
        {
          dlgYesNo.SetHeading(518); // Audio
        }
        else if (Util.Utils.IsVideo(item.Path))
        {
          dlgYesNo.SetHeading(925); // Movie
        }
        else if (Util.Utils.IsPicture(item.Path))
        {
          dlgYesNo.SetHeading(664); // Picture
        }
        else
        {
          dlgYesNo.SetHeading(125); // Unknown file
        }
      }
      else
      {
        dlgYesNo.SetHeading(503);
      }
      dlgYesNo.SetLine(1, strFileName);
      dlgYesNo.SetLine(2, "");
      dlgYesNo.SetLine(3, "");
      dlgYesNo.DoModal(GetID);

      if (!dlgYesNo.IsConfirmed)
      {
        return;
      }
      DoDeleteItem(item);
    }

    private void DoDeleteItem(GUIListItem item)
    {
      if (item.IsFolder)
      {
        if (item.Label != "..")
        {
          List<GUIListItem> items = m_directory.GetDirectoryUnProtectedExt(item.Path, false);
          foreach (GUIListItem subItem in items)
          {
            DoDeleteItem(subItem);
          }

          Util.Utils.DirectoryDelete(item.Path,true);
        }
      }
      else if (!item.IsRemote)
      {
        if (Util.Utils.FileDelete(item.Path))
        {
          // delete from database
          DeleteFromDatabase(item);
        }
      }
    }

    private void DeleteFromDatabase(GUIListItem item)
    {
      // delete from database
      if (Util.Utils.IsPicture(item.Path))
      {
        //Remove from picture database
        PictureDatabase.DeletePicture(item.Path);
      }
      else if (Util.Utils.IsVideo(item.Path))
      {
        //Remove from video database
        VideoDatabase.DeleteMovie(item.Path);
      }
      else if (Util.Utils.IsAudio(item.Path))
      {
        //Remove from music database
        if (dbMusic == null)
        {
          dbMusic = MusicDatabase.Instance;
        }
        if (dbMusic != null)
        {
          dbMusic.DeleteSong(item.Path, true);
        }
      }
    }

    private void MakeDir()
    {
      // Get input string
      string verStr = "";
      GetUserInputString(ref verStr);

      // Ask user confirmation
      string path = sourceFolder + "\\" + verStr;
      try
      {
        // Determine whether the directory exists.
        if (Directory.Exists(path))
        {
          GUIDialogOK dlgOk = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
          dlgOk.SetHeading(119);
          dlgOk.SetLine(1, 2224);
          dlgOk.SetLine(2, "");
          dlgOk.DoModal(m_dwParentWindowID);
        }
        else
        {
          DirectoryInfo di = Directory.CreateDirectory(path);
        }
      }
      catch (Exception)
      {
        ShowErrorDialog(119, path);
      }
    }

    public bool Reload()
    {
      return m_bReload;
    }

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }

    #endregion
  }
}