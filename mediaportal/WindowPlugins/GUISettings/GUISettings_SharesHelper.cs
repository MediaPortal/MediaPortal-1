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

using System;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Music;
using MediaPortal.GUI.Video;
using MediaPortal.Profile;
using MediaPortal.Util;

namespace MediaPortal.GUI.Settings
{
  public class ShareData
  {
    public string Name;
    public string Folder;
    public string PinCode;

    public bool IsRemote = false;
    public string Server = "127.0.0.1";
    public string LoginName = string.Empty;
    public string PassWord = string.Empty;
    public string RemoteFolder = "/";
    public int Port = 21;
    public bool ActiveConnection = true;
    public GUIFacadeControl.Layout DefaultLayout = GUIFacadeControl.Layout.List;
    public bool ScanShare = false;
    public bool CreateThumbs = true;
    public bool EachFolderIsMovie = false;

    public bool HasPinCode
    {
      get { return (PinCode.Length > 0); }
    }

    public ShareData(string name, string folder, string pinCode, bool thumbs)
    {
      this.Name = name;
      this.Folder = folder;
      this.PinCode = pinCode;
      this.CreateThumbs = thumbs;
    }
  }

  /// <summary>
  /// 
  /// </summary>
  public class SettingsSharesHelper
  {
    private bool _rememberLastFolder;
    private bool _addOpticalDiskDrives;
    private bool _autoSwitchRemovableDrives;
    private List<GUIListItem> _shareListControl = new List<GUIListItem>();
    private String _defaultShare;


    //private ArrayList _layouts = new ArrayList();

    public List<GUIListItem> ShareListControl
    {
      get { return _shareListControl; }
      set { _shareListControl = value; }
    }

    private enum DriveType
    {
      Removable = 2,
      Fixed = 3,
      RemoteDisk = 4,
      CD = 5,
      DVD = 5,
      RamDisk = 6
    }

    public String DefaultShare
    {
      get { return _defaultShare; }
      set { _defaultShare = value; }
    }

    // Bool options
    public bool RememberLastFolder
    {
      get { return _rememberLastFolder; }
      set { _rememberLastFolder = value; }
    }

    public bool AddOpticalDiskDrives
    {
      get { return _addOpticalDiskDrives; }
      set { _addOpticalDiskDrives = value; }
    }

    public bool SwitchRemovableDrives
    {
      get { return _autoSwitchRemovableDrives; }
      set { _autoSwitchRemovableDrives = value; }
    }
    
    private void AddStaticShares(DriveType driveType, string defaultName)
    {
      string[] drives = Environment.GetLogicalDrives();

      foreach (string drive in drives)
      {
        if (Util.Utils.getDriveType(drive) == (int)driveType)
        {
          bool driveFound = false;
          string driveName = Util.Utils.GetDriveName(drive);

          if (driveName.Length == 0)
          {
            string driveLetter = drive.Substring(0, 1).ToUpperInvariant();
            driveName = String.Format("{0} {1}:", defaultName, driveLetter);
          }

          //
          // Check if the share already exists
          //
          foreach (GUIListItem listItem in _shareListControl)
          {
            if (listItem.Path == drive)
            {
              driveFound = true;
              break;
            }
          }

          if (driveFound == false)
          {
            //
            // Add share
            //
            string name = "";
            switch (driveType)
            {
              case DriveType.Removable:
                name = String.Format("({0}:) Removable", drive.Substring(0, 1).ToUpperInvariant());
                break;
              case DriveType.Fixed:
                name = String.Format("({0}:) Fixed", drive.Substring(0, 1).ToUpperInvariant());
                break;
              case DriveType.RemoteDisk:
                name = String.Format("({0}:) Remote", drive.Substring(0, 1).ToUpperInvariant());
                break;
              case DriveType.DVD: // or cd
                name = String.Format("({0}:) CD/DVD", drive.Substring(0, 1).ToUpperInvariant());
                break;
              case DriveType.RamDisk:
                name = String.Format("({0}:) Ram", drive.Substring(0, 1).ToUpperInvariant());
                break;
            }
            if (driveType == DriveType.Fixed || driveType == DriveType.RemoteDisk)
            {
              AddShare(new ShareData(name, drive, string.Empty, true), false);
            }
            else
            {
              AddShare(new ShareData(name, drive, string.Empty, false), false);
            }
          }
        }
      }
    }

    private void AddShare(ShareData shareData, bool check)
    {
      GUIListItem listItem = new GUIListItem();
      listItem.Label = shareData.Name;
      //listItem.Label3 = shareData.HasPinCode ? "Yes" : "No";
      listItem.Path = shareData.Folder;
      listItem.AlbumInfoTag = shareData; // Store full data


      if (shareData.IsRemote)
      {
        listItem.Path = String.Format("ftp://{0}:{1}{2}", shareData.Server, shareData.Port,
                                                  shareData.RemoteFolder);
      }
      // Default share
      listItem.IsPlayed = check;
      
      _shareListControl.Add(listItem);
    }

    public void LoadSettings(string section)
    {
      if (string.IsNullOrEmpty(section))
      {
        return;
      }

      using (Profile.Settings xmlreader = new MPSettings())
      {
        string defaultSharePath = string.Empty;

        switch (section)
        {
          case "movies":
            defaultSharePath = Win32API.GetFolderPath(Win32API.CSIDL_MYVIDEO);
            break;
          case "music":
            defaultSharePath = Win32API.GetFolderPath(Win32API.CSIDL_MYMUSIC);
            break;
          case "pictures":
            defaultSharePath = Win32API.GetFolderPath(Win32API.CSIDL_MYPICTURES);
            break;
        }

        DefaultShare = xmlreader.GetValueAsString(section, "default", "");
        _addOpticalDiskDrives = xmlreader.GetValueAsBool(section, "AddOpticalDiskDrives", true);
        _autoSwitchRemovableDrives = xmlreader.GetValueAsBool(section, "SwitchRemovableDrives", true);
        _rememberLastFolder = xmlreader.GetValueAsBool(section, "rememberlastfolder", false);

        // Shares
        for (int index = 0; index < 128; index++)
        {
          string shareName = String.Format("sharename{0}", index);
          string sharePath = String.Format("sharepath{0}", index);
          string sharePin = String.Format("pincode{0}", index);

          string shareType = String.Format("sharetype{0}", index);
          string shareServer = String.Format("shareserver{0}", index);
          string shareLogin = String.Format("sharelogin{0}", index);
          string sharePwd = String.Format("sharepassword{0}", index);
          string sharePort = String.Format("shareport{0}", index);
          string shareRemotePath = String.Format("shareremotepath{0}", index);
          string shareViewPath = String.Format("shareview{0}", index);

          string shareNameData = xmlreader.GetValueAsString(section, shareName, "");
          string sharePathData = xmlreader.GetValueAsString(section, sharePath, "");
          string sharePinData = Util.Utils.DecryptPin(xmlreader.GetValueAsString(section, sharePin, ""));

          // provide default shares
          if (index == 0 && shareNameData == string.Empty)
          {
            shareNameData = VirtualDirectory.GetShareNameDefault(defaultSharePath);
            sharePathData = defaultSharePath;
            sharePinData = string.Empty;

            AddStaticShares(DriveType.DVD, "DVD");
          }

          bool shareTypeData = xmlreader.GetValueAsBool(section, shareType, false);
          string shareServerData = xmlreader.GetValueAsString(section, shareServer, "");
          string shareLoginData = xmlreader.GetValueAsString(section, shareLogin, "");
          string sharePwdData = xmlreader.GetValueAsString(section, sharePwd, "");
          int sharePortData = xmlreader.GetValueAsInt(section, sharePort, 21);
          string shareRemotePathData = xmlreader.GetValueAsString(section, shareRemotePath, "/");
          int shareLayout = xmlreader.GetValueAsInt(section, shareViewPath, (int)GUIFacadeControl.Layout.List);

          // For Music Shares, we can indicate, if we want to scan them every time
          bool shareScanData = false;
          if (section == "music" || section == "movies")
          {
            string shareScan = String.Format("sharescan{0}", index);
            shareScanData = xmlreader.GetValueAsBool(section, shareScan, true);
          }
          // For Movies Shares, we can indicate, if we want to create thumbs
          bool thumbs = true;
          bool folderIsMovie = false;
          if (section == "movies")
          {
            string thumbsCreate = String.Format("videothumbscreate{0}", index);
            thumbs = xmlreader.GetValueAsBool(section, thumbsCreate, true);
            string eachFolderIsMovie = String.Format("eachfolderismovie{0}", index);
            folderIsMovie = xmlreader.GetValueAsBool(section, eachFolderIsMovie, false);
          }

          if (!String.IsNullOrEmpty(shareNameData))
          {
            ShareData newShare = new ShareData(shareNameData, sharePathData, sharePinData, thumbs);
            newShare.IsRemote = shareTypeData;
            newShare.Server = shareServerData;
            newShare.LoginName = shareLoginData;
            newShare.PassWord = sharePwdData;
            newShare.Port = sharePortData;
            newShare.RemoteFolder = shareRemotePathData;
            newShare.DefaultLayout = (GUIFacadeControl.Layout)shareLayout;

            newShare.ScanShare = shareScanData;
            
            if (section == "movies")
            {
              newShare.CreateThumbs = thumbs;
              newShare.EachFolderIsMovie = folderIsMovie;
            }
            AddShare(newShare, shareNameData.Equals(DefaultShare));
          }
        }
        if (_addOpticalDiskDrives)
        {
          AddStaticShares(DriveType.DVD, "DVD");
        }
      }
    }

    public void SetDefaultDrives(string section, bool addOpticalDrives)
    {
      using (Profile.Settings xmlwriter = new MPSettings())
      {
        for (int index = 0; index < 128; index++)
        {
          string shareName = String.Format("sharename{0}", index);
          string sharePath = String.Format("sharepath{0}", index);
          string sharePin = String.Format("pincode{0}", index);
          string shareType = String.Format("sharetype{0}", index);
          string shareServer = String.Format("shareserver{0}", index);
          string shareLogin = String.Format("sharelogin{0}", index);
          string sharePwd = String.Format("sharepassword{0}", index);
          string sharePort = String.Format("shareport{0}", index);
          string shareRemotePath = String.Format("shareremotepath{0}", index);
          string shareViewPath = String.Format("shareview{0}", index);

          xmlwriter.RemoveEntry(section, shareName);
          xmlwriter.RemoveEntry(section, sharePath);
          xmlwriter.RemoveEntry(section, sharePin);
          xmlwriter.RemoveEntry(section, shareType);
          xmlwriter.RemoveEntry(section, shareServer);
          xmlwriter.RemoveEntry(section, shareLogin);
          xmlwriter.RemoveEntry(section, sharePwd);
          xmlwriter.RemoveEntry(section, sharePort);
          xmlwriter.RemoveEntry(section, shareRemotePath);
          xmlwriter.RemoveEntry(section, shareViewPath);
        }
      }

      switch (section)
      {
        case "movies":
          VirtualDirectory.SetInitialDefaultShares(false, false, false, true);
          break;
        case "music":
          VirtualDirectory.SetInitialDefaultShares(false, true, false, false);
          break;
        case "pictures":
          VirtualDirectory.SetInitialDefaultShares(false, false, true, false);
          break;
      }
    }
    
    public void SaveSettings(string section)
    {
      if (AddOpticalDiskDrives)
      {
        AddStaticShares(DriveType.DVD, "DVD");
      }

      using (Profile.Settings xmlwriter = new MPSettings())
      {
        string defaultShare = string.Empty;

        for (int index = 0; index < 128; index++)
        {
          string shareName = String.Format("sharename{0}", index);
          string sharePath = String.Format("sharepath{0}", index);
          string sharePin = String.Format("pincode{0}", index);
          string shareType = String.Format("sharetype{0}", index);
          string shareServer = String.Format("shareserver{0}", index);
          string shareLogin = String.Format("sharelogin{0}", index);
          string sharePwd = String.Format("sharepassword{0}", index);
          string sharePort = String.Format("shareport{0}", index);
          string shareRemotePath = String.Format("shareremotepath{0}", index);
          string shareViewPath = String.Format("shareview{0}", index);

          xmlwriter.RemoveEntry(section, shareName);
          xmlwriter.RemoveEntry(section, sharePath);
          xmlwriter.RemoveEntry(section, sharePin);
          xmlwriter.RemoveEntry(section, shareType);
          xmlwriter.RemoveEntry(section, shareServer);
          xmlwriter.RemoveEntry(section, shareLogin);
          xmlwriter.RemoveEntry(section, sharePwd);
          xmlwriter.RemoveEntry(section, sharePort);
          xmlwriter.RemoveEntry(section, shareRemotePath);
          xmlwriter.RemoveEntry(section, shareViewPath);

          if (section == "music" || section == "movies")
          {
            string shareScan = String.Format("sharescan{0}", index);
            xmlwriter.RemoveEntry(section, shareScan);
          }

          if (section == "movies")
          {
            string thumbs = String.Format("videothumbscreate{0}", index);
            xmlwriter.RemoveEntry(section, thumbs);

            string movieFolder = String.Format("eachfolderismovie{0}", index);
            xmlwriter.RemoveEntry(section, movieFolder);
          }

          string shareNameData = string.Empty;
          string sharePathData = string.Empty;
          string sharePinData = string.Empty;
          bool shareTypeData = false;
          string shareServerData = string.Empty;
          string shareLoginData = string.Empty;
          string sharePwdData = string.Empty;
          int sharePortData = 21;
          string shareRemotePathData = string.Empty;
          int shareLayout = (int)MediaPortal.GUI.Library.GUIFacadeControl.Layout.List;
          bool shareScanData = false;
          //ThumbsCreate (default true)
          bool thumbsCreate = true;
          bool folderIsMovie = false;

          if (_shareListControl != null && _shareListControl.Count > index)
          {
            ShareData shareData = _shareListControl[index].AlbumInfoTag as ShareData;

            if (shareData != null && !String.IsNullOrEmpty(shareData.Name))
            {
              shareNameData = shareData.Name;
              sharePathData = shareData.Folder;
              sharePinData = shareData.PinCode;
              shareTypeData = shareData.IsRemote;
              shareServerData = shareData.Server;
              shareLoginData = shareData.LoginName;
              sharePwdData = shareData.PassWord;
              sharePortData = shareData.Port;
              shareRemotePathData = shareData.RemoteFolder;
              shareLayout = (int)shareData.DefaultLayout;
              shareScanData = shareData.ScanShare;
              // ThumbsCreate
              thumbsCreate = shareData.CreateThumbs;
              folderIsMovie = shareData.EachFolderIsMovie;

              if (shareNameData == _defaultShare)
              {
                defaultShare = shareNameData;
              }

              xmlwriter.SetValue(section, shareName, shareNameData);
              xmlwriter.SetValue(section, sharePath, sharePathData);
              xmlwriter.SetValue(section, sharePin, Util.Utils.EncryptPin(sharePinData));
              xmlwriter.SetValueAsBool(section, shareType, shareTypeData);
              xmlwriter.SetValue(section, shareServer, shareServerData);
              xmlwriter.SetValue(section, shareLogin, shareLoginData);
              xmlwriter.SetValue(section, sharePwd, sharePwdData);
              xmlwriter.SetValue(section, sharePort, sharePortData.ToString());
              xmlwriter.SetValue(section, shareRemotePath, shareRemotePathData);
              xmlwriter.SetValue(section, shareViewPath, shareLayout);

              if (section == "music" || section == "movies")
              {
                string shareScan = String.Format("sharescan{0}", index);
                xmlwriter.SetValueAsBool(section, shareScan, shareScanData);
              }
              
              if (section == "movies")
              {
                string thumbs = String.Format("videothumbscreate{0}", index);
                xmlwriter.SetValueAsBool(section, thumbs, thumbsCreate);

                string folderMovie = String.Format("eachfolderismovie{0}", index);
                xmlwriter.SetValueAsBool(section, folderMovie, folderIsMovie);
              }
            }
          }
        }
        xmlwriter.SetValue(section, "default", defaultShare);
        xmlwriter.SetValueAsBool(section, "rememberlastfolder", RememberLastFolder);
        xmlwriter.SetValueAsBool(section, "AddOpticalDiskDrives", AddOpticalDiskDrives);
        xmlwriter.SetValueAsBool(section, "SwitchRemovableDrives", SwitchRemovableDrives);
      }
      // Set new shares for internal plugins
      switch (section)
      {
        case "movies":
          GUIVideoFiles.ResetShares();
          break;
        case "music":
          GUIMusicFiles.ResetShares();
          break;
        case "pictures":
          Pictures.GUIPictures.ResetShares();
          break;
      }
    }

    public static GUIFacadeControl.Layout ProperLayoutFromDefault(int defaultView)
    {
      switch (defaultView)
      {
        case 1: return GUIFacadeControl.Layout.SmallIcons;
        case 2: return GUIFacadeControl.Layout.LargeIcons;
        case 3: return GUIFacadeControl.Layout.AlbumView;
        case 4: return GUIFacadeControl.Layout.Filmstrip;
        case 5: return GUIFacadeControl.Layout.CoverFlow;
        default: return GUIFacadeControl.Layout.List;
      }
    }

    public static int ProperDefaultFromLayout(GUIFacadeControl.Layout layout)
    {
      switch (layout)
      {
        case GUIFacadeControl.Layout.SmallIcons: return 1;
        case GUIFacadeControl.Layout.LargeIcons: return 2;
        case GUIFacadeControl.Layout.AlbumView: return 3;
        case GUIFacadeControl.Layout.Filmstrip: return 4;
        case GUIFacadeControl.Layout.CoverFlow: return 5;
        default: return 0;
      }
    }
  }
}