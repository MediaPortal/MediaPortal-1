#region Copyright (C) 2005-2006 Team MediaPortal

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

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Ripper;
using MediaPortal.Player;
using System.Management;
using System.IO;
using EnterpriseDT.Net.Ftp;
using Core.Util;


namespace MediaPortal.Util
{
  /// <summary>
  /// This class is used to present a virtual directory to MediaPortal
  /// A virtual directory holds one or more shares (see share.cs)
  /// 
  /// </summary>

  public class VirtualDirectory
  {
    const int Removable = 2;
    const int LocalDisk = 3;
    const int Network = 4;
    const int CD = 5;

    List<Share> m_shares = new List<Share>();
    List<string> m_extensions = null;
    string m_strPreviousDir = String.Empty;
    string currentShare = String.Empty;
    string previousShare = String.Empty;
    string m_strLocalFolder = String.Empty;
    bool showFilesWithoutExtension = false;
    /// <summary>
    /// constructor
    /// </summary>
    public VirtualDirectory()
    {
    }

    public bool ShowFilesWithoutExtension
    {
      get { return showFilesWithoutExtension; }
      set { showFilesWithoutExtension = value; }
    }

    public void AddDrives()
    {
    }

    public void Reset()
    {
      previousShare = String.Empty;
      m_strPreviousDir = String.Empty;
    }

    /// <summary>
    /// Method to set a list of all file extensions
    /// The virtual directory will only show folders and files which match one of those file extensions
    /// </summary>
    /// <param name="extensions">string arraylist of file extensions</param>
    public void SetExtensions(ArrayList extensions)
    {
      if (extensions == null) return;
      if (m_extensions == null)
        m_extensions = new List<string>();

      foreach (string ext in extensions)
      {
        m_extensions.Add(ext);
      }
      for (int i = 0; i < m_extensions.Count; ++i)
      {
        m_extensions[i] = ((string)m_extensions[i]).ToLower();
      }
    }
    /// <summary>
    /// Method to add a new file extension to the file extensions list
    /// </summary>
    /// <param name="extensionension">string containg the new extension in the format .mp3</param>
    public void AddExtension(string extensionension)
    {
      if (m_extensions == null)
        m_extensions = new List<string>();
      m_extensions.Add(extensionension.ToLower());
    }

    /// <summary>
    /// Add a new share to this virtual directory
    /// </summary>
    /// <param name="share">new Share</param>
    public void Add(Share share)
    {
      if (share == null) return;
      m_shares.Add(share);
    }

    /// <summary>
    /// This method removes all shares from the virtual directory
    /// </summary>
    public void Clear()
    {
      m_shares.Clear();
    }

    /// <summary>
    /// This method will return the root folder
    /// which contains a list of all shares
    /// </summary>
    /// <returns>
    /// ArrayList containing a GUIListItem for each share
    /// </returns>
    public ArrayList GetRoot()
    {
      previousShare = String.Empty;

      ArrayList items = new ArrayList();
      foreach (Share share in m_shares)
      {
        GUIListItem item = new GUIListItem();
        item.Label = share.Name;
        item.Path = share.Path;
        if (Utils.IsRemovable(item.Path) && Directory.Exists(item.Path))
        {
          string strDriveName = Utils.GetDriveName(item.Path);
          if (strDriveName == "") strDriveName = "Removable";
          item.Label = String.Format("{1} {0}", item.Path, strDriveName);
        }
        if (Utils.IsDVD(item.Path))
        {
          item.DVDLabel = Utils.GetDriveName(item.Path);
          item.DVDLabel = item.DVDLabel.Replace('_', ' ');
        }

        if (item.DVDLabel != "")
        {
          item.Label = String.Format("({0}) {1}", item.Path, item.DVDLabel);
        }
        else
          item.Label = share.Name;
        item.IsFolder = true;

        if (share.IsFtpShare)
        {
          item.Path = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
                share.FtpServer, share.FtpPort, share.FtpLoginName, share.FtpPassword, Utils.RemoveTrailingSlash(share.FtpFolder));
        }
        if (File.Exists(item.Path + "\folder.jpg"))
        {
          item.IconImage = Utils.GetCoverArtName(item.Path, "folder");
          item.IconImageBig = Utils.GetLargeCoverArtName(item.Path, "folder");
        }
        else
        {
          Utils.SetDefaultIcons(item);
        }
        items.Add(item);
      }

      // add removable drives with media
      string[] drives = Environment.GetLogicalDrives();
      foreach (string drive in drives)
      {
        if (drive[0] > 'B' && Util.Utils.getDriveType(drive) == Removable)
        {
          bool driveFound = false;
          string driveName = Util.Utils.GetDriveName(drive);
          string driveLetter = drive.Substring(0, 1).ToUpper() + ":";
          if (driveName == "") driveName = "Removable";

          //
          // Check if the share already exists
          //
          foreach (Share share in m_shares)
          {
            if (share.Path == driveLetter)
            {
              driveFound = true;
              break;
            }
          }

          if (driveFound == false)
          {
            GUIListItem item = new GUIListItem();
            item.Path = driveLetter;
            item.Label = String.Format("{0} {1}", driveName, driveLetter);
            item.IsFolder = true;

            Utils.SetDefaultIcons(item);

            // dont add removable shares without media
            // virtual cd/dvd drive (daemontools) without mounted image
            if (!Directory.Exists(item.Path))
              break;

            items.Add(item);
          }
        }
      }

      return items;
    }

    /// <summary>
    /// Method which checks if the given folder is the root share folder or not
    /// </summary>
    /// <param name="strDir">folder name</param>
    /// <returns>
    /// true: this is the root folder. There's no parent folder
    /// false: this is not the root folder, there is a parent folder
    /// </returns>
    public bool IsRootShare(string strDir)
    {
      if (strDir == null) return false;
      if (strDir.Length <= 0) return false;
      string strRoot = strDir;
      bool isRemote = IsRemote(strDir);
      if (!isRemote)
      {
        if (strDir != "cdda:")
        {
          try
          {
            strRoot = System.IO.Path.GetFullPath(strDir);
          }
          catch (Exception ex)
          {
            Log.Write("VirtualDirectory: Unable to get path for Root Share:{0} reason:{1}", strDir, ex.Message);
          }
        }
      }

      foreach (Share share in m_shares)
      {
        try
        {
          if (isRemote)
          {
            if (share.IsFtpShare)
            {
              string remoteFolder = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
                share.FtpServer, share.FtpPort, share.FtpLoginName, share.FtpPassword, Utils.RemoveTrailingSlash(share.FtpFolder));
              if (remoteFolder == strDir) return true;
            }
          }
          else
          {
            if (System.IO.Path.GetFullPath(share.Path) == strRoot)
            {
              return true;
            }
          }
        }
        catch (Exception)
        {
        }
      }
      return false;
    }

    /// <summary>
    /// This method checks if the specified folder is protected by a pincode
    /// </summary>
    /// <param name="strDir">folder to check</param>
    /// <param name="iPincode">on return: pincode to use or -1 when no pincode is needed</param>
    /// <returns>
    /// true : folder is protected by a pincode
    /// false: folder is not protected
    /// </returns>
    public bool IsProtectedShare(string strDir, out int iPincode)
    {
      iPincode = -1;
      if (strDir == null) return false;
      if (strDir.Length <= 0) return false;
      string strRoot = strDir;

      bool isRemote = IsRemote(strDir);
      if (!isRemote)
      {
        if (strDir != "cdda:")
        {
          try
          {
            strRoot = System.IO.Path.GetFullPath(strDir);
          }
          catch (Exception ex)
          {
            Log.Write("VirtualDirectory: Unable to get path for Protected Share:{0} reason:{1)", strDir, ex.Message);
          }
        }
      }

      foreach (Share share in m_shares)
      {
        try
        {
          if (isRemote)
          {
            if (share.IsFtpShare)
            {
              string remoteFolder = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
                share.FtpServer, share.FtpPort, share.FtpLoginName, share.FtpPassword, Utils.RemoveTrailingSlash(share.FtpFolder));
              if (strDir == remoteFolder)
              {
                iPincode = share.Pincode;
                if (share.Pincode >= 0)
                  return true;
                return false;
              }
            }
          }
          else
          {
            string strFullPath = System.IO.Path.GetFullPath(share.Path);
            //if (strRoot.ToLower().StartsWith(strFullPath.ToLower()))
            if (strRoot.ToLower() == strFullPath.ToLower())
            {
              currentShare = strFullPath;
              iPincode = share.Pincode;
              if (share.Pincode >= 0)
                return true;
              return false;
            }
          }
        }
        catch (Exception)
        {
        }
      }
      return false;
    }
    public Share GetShare(string strDir)
    {
      if (strDir == null) return null;
      if (strDir.Length <= 0) return null;
      string strRoot = strDir;

      bool isRemote = IsRemote(strDir);
      if (!isRemote)
      {
        if (strDir != "cdda:")
          strRoot = System.IO.Path.GetFullPath(strDir);
      }

      foreach (Share share in m_shares)
      {
        try
        {
          if (isRemote)
          {
            if (share.IsFtpShare)
            {
              string remoteFolder = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
                share.FtpServer, share.FtpPort, share.FtpLoginName, share.FtpPassword, Utils.RemoveTrailingSlash(share.FtpFolder));
              if (strDir == remoteFolder)
              {
                return share;
              }
            }
          }
          else
          {
            string strFullPath = System.IO.Path.GetFullPath(share.Path);
            if (strRoot.ToLower().StartsWith(strFullPath.ToLower()))
            {
              currentShare = strFullPath;
              return share;
            }
          }
        }
        catch (Exception)
        {
        }
      }
      return null;
    }

    /// <summary>
    /// This method check is the given extension is a image file
    /// </summary>
    /// <param name="extensionension">file extension</param>
    /// <returns>
    /// true: if file is an image file (.img, .nrg, .bin, .iso)
    /// false: if the file is not an image file
    /// </returns>
    static public bool IsImageFile(string extensionension)
    {
      if (extensionension == null) return false;
      if (extensionension == String.Empty) return false;
      extensionension = extensionension.ToLower();
      if (extensionension == ".img" || extensionension == ".bin" || extensionension == ".iso" || extensionension == ".nrg")
      {
        return true;
      }
      return false;
    }

    public bool IsRemote(string folder)
    {
      if (folder == null) return false;
      if (folder.IndexOf("remote:") == 0) return true;
      return false;
    }

    /// <summary>
    /// This method returns an arraylist of GUIListItems for the specified folder
    /// If the folder is protected by a pincode then the user is asked to enter the pincode
    /// and the folder contents is only returned when the pincode is correct
    /// </summary>
    /// <param name="strDir">folder</param>
    /// <returns>
    /// returns an arraylist of GUIListItems for the specified folder
    /// </returns>
    public ArrayList GetDirectory(string strDir)
    {
      if ((strDir == null) || (strDir == ""))
      {
        m_strPreviousDir = "";
        return GetRoot();
      }

      //if we have a folder like D:\
      //then remove the \
      if (strDir.Length == 2 && strDir.Substring(1) == @"\")
        strDir = strDir.Substring(0, strDir.Length - 1);

      ArrayList items = new ArrayList();

      //get the parent folder
      string strParent = "";
      if (IsRemote(strDir))
      {
        int ipos = strDir.LastIndexOf(@"/");
        if (ipos > 0)
        {
          strParent = strDir.Substring(0, ipos);
        }
      }
      else
      {
        int ipos = strDir.LastIndexOf(@"\");
        if (ipos > 0)
        {
          strParent = strDir.Substring(0, ipos);
        }
      }


      //is this directory protected
      int iPincodeCorrect;
      if (IsProtectedShare(strDir, out iPincodeCorrect) && !m_strPreviousDir.StartsWith(currentShare))
      {
        bool retry = true;
        {
          while (retry)
          {
            //yes, check if this is a subdirectory of the share
            if (previousShare != currentShare)
            //if (previousShare==String.Empty || strDir.IndexOf(previousShare) < 0)
            {
              //no, then ask user to enter the pincode
              GUIMessage msgGetPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
              GUIWindowManager.SendMessage(msgGetPassword);
              int iPincode = -1;
              try
              {
                iPincode = Int32.Parse(msgGetPassword.Label);
              }
              catch (Exception)
              {
              }
              if (iPincode != iPincodeCorrect)
              {
                GUIMessage msgWrongPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WRONG_PASSWORD, 0, 0, 0, 0, 0, 0);
                GUIWindowManager.SendMessage(msgWrongPassword);

                if (!(bool)msgWrongPassword.Object)
                {
                  GUIListItem itemTmp = new GUIListItem();
                  itemTmp.IsFolder = true;
                  itemTmp.Label = "..";
                  itemTmp.Label2 = "";
                  itemTmp.Path = m_strPreviousDir;
                  Utils.SetDefaultIcons(itemTmp);
                  Utils.SetThumbnails(ref itemTmp);
                  items.Add(itemTmp);
                  return items;
                }
              }
              else
                retry = false;
            }
          }
        }
      }

      //check if this is an image file like .iso, .nrg,...
      //ifso then ask daemontools to automount it

      bool VirtualShare = false;
      if (!IsRemote(strDir))
      {
        if (DaemonTools.IsEnabled)
        {
          string extensionension = System.IO.Path.GetExtension(strDir);
          if (IsImageFile(extensionension))
          {
            bool askBeforePlayingDVDImage = false;

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
            {
              askBeforePlayingDVDImage = xmlreader.GetValueAsBool("daemon", "askbeforeplaying", false);
            }

            if (!DaemonTools.IsMounted(strDir))
            {
              if (!askBeforePlayingDVDImage)
              {
                AutoPlay.StopListening();

                // yield some time before we try to mount
                System.Threading.Thread.Sleep(50);
              }

              string virtualPath;
              if (DaemonTools.Mount(strDir, out virtualPath))
              {
                strDir = virtualPath;
                VirtualShare = true;
              }

              if (!askBeforePlayingDVDImage)
              {
                AutoPlay.StartListening();
              }
            }
            else
            {
              strDir = DaemonTools.GetVirtualDrive();
              VirtualShare = true;
            }

            if (VirtualShare/* && !g_Player.Playing*/) // dont interrupt if we're already playing
            {
              if (!askBeforePlayingDVDImage)
              {
                // If it looks like a DVD directory structure then return so
                // that the playing of the DVD is handled by the caller.
                if (System.IO.File.Exists(strDir + @"\VIDEO_TS\VIDEO_TS.IFO"))
                {
                  return items;
                }
              }
            }
          }
        }
      }

      string[] strDirs = null;
      string[] strFiles = null;
      GUIListItem item = null;
      if (!IsRootShare(strDir) || VirtualShare)
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        Utils.SetDefaultIcons(item);
        Utils.SetThumbnails(ref item);

        if (strParent == strDir)
        {
          item.Path = "";
        }
        else
          item.Path = strParent;
        items.Add(item);
      }
      else
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        item.Path = "";
        Utils.SetDefaultIcons(item);
        Utils.SetThumbnails(ref item);
        items.Add(item);
      }

      if (IsRemote(strDir))
      {
        FTPClient ftp = GetFtpClient(strDir);
        if (ftp == null) return items;

        string folder = strDir.Substring("remote:".Length);
        string[] subitems = folder.Split(new char[] { '?' });
        if (subitems[4] == String.Empty) subitems[4] = "/";

        FTPFile[] files;
        try
        {
          ftp.ChDir(subitems[4]);
          files = ftp.DirDetails(); //subitems[4]);
        }
        catch (Exception)
        {
          //maybe this socket has timed out, remove it and get a new one
          FtpConnectionCache.Remove(ftp);
          ftp = GetFtpClient(strDir);
          if (ftp == null) return items;
          try
          {
            ftp.ChDir(subitems[4]);
          }
          catch (Exception ex)
          {
            Log.Write("VirtualDirectory:unable to chdir to remote folder:{0} reason:{1} {2}", subitems[4], ex.Message, ex.StackTrace);
            return items;
          }
          try
          {
            files = ftp.DirDetails(); //subitems[4]);
          }
          catch (Exception ex)
          {
            Log.Write("VirtualDirectory:unable to get remote folder:{0} reason:{1}  {2}", subitems[4], ex.Message, ex.StackTrace);
            return items;
          }
        }
        for (int i = 0; i < files.Length; ++i)
        {
          FTPFile file = files[i];
          //Log.Write("VirtualDirectory: {0} {1}",file.Name,file.Dir);
          if (file.Dir)
          {
            if (file.Name != "." && file.Name != "..")
            {
              item = new GUIListItem();
              item.IsFolder = true;
              item.Label = file.Name;
              item.Label2 = "";
              item.Path = String.Format("{0}/{1}", strDir, file.Name);
              item.IsRemote = true;
              item.FileInfo = null;
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
            }
          }
          else
          {
            if (IsValidExtension(file.Name))
            {
              item = new GUIListItem();
              item.IsFolder = false;
              item.Label = file.Name;
              item.Label2 = "";
              item.Path = String.Format("{0}/{1}", strDir, file.Name);
              item.IsRemote = true;
              if (IsRemoteFileDownloaded(item.Path, file.Size))
              {
                item.Path = GetLocalFilename(item.Path);
                item.IsRemote = false;
              }
              else if (FtpConnectionCache.IsDownloading(item.Path))
              {
                item.IsDownloading = true;
              }
              item.FileInfo = new FileInformation();
              DateTime modified = file.LastModified;
              item.FileInfo.CreationTime = modified;
              item.FileInfo.Length = file.Size;
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
            }
          }
        }
      }
      else
      {
        try
        {
          strDirs = System.IO.Directory.GetDirectories(strDir + @"\");
          strFiles = System.IO.Directory.GetFiles(strDir + @"\");
        }
        catch (Exception)
        {
        }


        if (strDirs != null)
        {
          for (int i = 0; i < strDirs.Length; ++i)
          {
            string strPath = strDirs[i].Substring(strDir.Length + 1);

            // Skip hidden folders
            if ((File.GetAttributes(strDir + @"\" + strPath) & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
              continue;
            }

            item = new GUIListItem();
            item.IsFolder = true;
            item.Label = strPath;
            item.Label2 = "";
            item.Path = strDirs[i];
            Utils.SetDefaultIcons(item);
            Utils.SetThumbnails(ref item);

            items.Add(item);
          }
        }

        if (strFiles != null)
        {
          for (int i = 0; i < strFiles.Length; ++i)
          {
            string extensionension = System.IO.Path.GetExtension(strFiles[i]);
            if (IsImageFile(extensionension))
            {
              if (DaemonTools.IsEnabled)
              {

                item = new GUIListItem();
                item.IsFolder = true;
                item.Label = Utils.GetFilename(strFiles[i]);
                item.Label2 = "";
                item.Path = strFiles[i];
                item.FileInfo = null;
                Utils.SetDefaultIcons(item);
                Utils.SetThumbnails(ref item);
                items.Add(item);
                continue;
              }
            }
            if (IsValidExtension(strFiles[i]))
            {
              // Skip hidden files
              if ((File.GetAttributes(strFiles[i]) & FileAttributes.Hidden) == FileAttributes.Hidden)
              {
                continue;
              }

              item = new GUIListItem();
              item.IsFolder = false;
              item.Label = Utils.GetFilename(strFiles[i]);
              item.Label2 = "";
              item.Path = strFiles[i];

              item.FileInfo = new FileInformation(strFiles[i]);
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
            }
          }
        }
      }
      m_strPreviousDir = strDir;
      return items;
    }


    /// <summary>
    /// This method returns an arraylist of GUIListItems for the specified folder
    /// This method does not check if the folder is protected by an pincode. it will
    /// always return all files/subfolders present
    /// </summary>
    /// <param name="strDir">folder</param>
    /// <returns>
    /// returns an arraylist of GUIListItems for the specified folder
    /// </returns>
    public ArrayList GetDirectoryUnProtected(string strDir, bool useExtensions)
    {
      if (strDir == null) return GetRoot();
      if (strDir == "") return GetRoot();

      if (strDir.Substring(1) == @"\") strDir = strDir.Substring(0, strDir.Length - 1);
      ArrayList items = new ArrayList();

      string strParent = "";
      int ipos = strDir.LastIndexOf(@"\");
      if (ipos > 0)
      {
        strParent = strDir.Substring(0, ipos);
      }


      bool VirtualShare = false;
      if (DaemonTools.IsEnabled)
      {
        string extensionension = System.IO.Path.GetExtension(strDir);
        if (IsImageFile(extensionension))
        {
          if (!DaemonTools.IsMounted(strDir))
          {
            string virtualPath;
            if (DaemonTools.Mount(strDir, out virtualPath))
            {
              strDir = virtualPath;
              VirtualShare = true;
            }
          }
          else
          {
            strDir = DaemonTools.GetVirtualDrive();
            VirtualShare = true;
          }
        }
      }

      string[] strDirs = null;
      string[] strFiles = null;
      try
      {
        strDirs = System.IO.Directory.GetDirectories(strDir + @"\");
        strFiles = System.IO.Directory.GetFiles(strDir + @"\");
      }
      catch (Exception)
      {
      }


      GUIListItem item = null;
      if (!IsRootShare(strDir) || VirtualShare)
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        Utils.SetDefaultIcons(item);
        Utils.SetThumbnails(ref item);

        if (strParent == strDir)
        {
          item.Path = "";
        }
        else
          item.Path = strParent;
        items.Add(item);
      }
      else
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        item.Path = "";
        Utils.SetDefaultIcons(item);
        Utils.SetThumbnails(ref item);
        items.Add(item);
      }
      if (strDirs != null)
      {
        for (int i = 0; i < strDirs.Length; ++i)
        {
          string strPath = strDirs[i].Substring(strDir.Length + 1);

          // Skip hidden folders
          if ((File.GetAttributes(strDir + @"\" + strPath) & FileAttributes.Hidden) == FileAttributes.Hidden)
          {
            continue;
          }

          item = new GUIListItem();
          item.IsFolder = true;
          item.Label = strPath;
          item.Label2 = "";
          item.Path = strDirs[i];
          Utils.SetDefaultIcons(item);
          Utils.SetThumbnails(ref item);

          items.Add(item);
        }
      }

      if (strFiles != null)
      {
        for (int i = 0; i < strFiles.Length; ++i)
        {
          string extensionension = System.IO.Path.GetExtension(strFiles[i]);
          if (IsImageFile(extensionension))
          {
            if (DaemonTools.IsEnabled)
            {

              item = new GUIListItem();
              item.IsFolder = true;
              item.Label = Utils.GetFilename(strFiles[i]);
              item.Label2 = "";
              item.Path = strFiles[i];
              item.FileInfo = null;
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
              continue;
            }
          }
          if (IsValidExtension(strFiles[i]) || (!useExtensions))
          {
            // Skip hidden files
            if ((File.GetAttributes(strFiles[i]) & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
              if (useExtensions)
                continue;
            }

            item = new GUIListItem();
            item.IsFolder = false;
            item.Label = Utils.GetFilename(strFiles[i]);
            item.Label2 = "";
            item.Path = strFiles[i];
            item.FileInfo = new FileInformation(strFiles[i]);
            Utils.SetDefaultIcons(item);
            Utils.SetThumbnails(ref item);
            items.Add(item);
          }
        }
      }
      return items;
    }

    /// <summary>
    /// This method checks if the extension of the specified file is valid for the current virtual folder
    /// The virtual directory will only show files with valid extensions
    /// </summary>
    /// <param name="strPath">filename</param>
    /// <returns>
    /// true : file has a valid extension
    /// false: file has an unknown extension
    /// </returns>
    bool IsValidExtension(string strPath)
    {
      if (strPath == null) return false;
      if (strPath == String.Empty) return false;
      try
      {
        //				if (!System.IO.Path.HasExtension(strPath)) return false;
        // waeberd: allow searching for files without an extension
        if (!System.IO.Path.HasExtension(strPath)) return showFilesWithoutExtension;
        string extensionFile = System.IO.Path.GetExtension(strPath).ToLower();
        if ((m_extensions[0] as string) == "*") return true;   // added for explorer modul by gucky
        for (int i = 0; i < m_extensions.Count; ++i)
        {
          if ((m_extensions[i] as string) == extensionFile) return true;
        }
      }
      catch (Exception) { }

      return false;
    }

    /// <summary>
    /// Returns the path of the default share
    /// </summary>
    /// <returns>Returns the path of the default share
    /// or empty if no default path has been set
    /// </returns>
    public string GetDefaultPath()
    {
      foreach (Share share in m_shares)
      {
        if (!share.IsFtpShare && share.Default)
        {
          return share.Path;
        }
      }
      return String.Empty;
    }

    /// <summary>
    /// Function to split a string containg the remote file+path 
    /// and get the path and filename for a remote file
    /// remote file is in format remote:hostname?port?login?password?folder
    /// </summary>
    /// <param name="remotefile">string containing the remote file</param>
    /// <param name="filename">on return contains the filename</param>
    /// <param name="path">on return contains the path</param>
    public void GetRemoteFileNameAndPath(string remotefile, out string filename, out string path)
    {
      filename = String.Empty;
      path = "/";
      if (!IsRemote(remotefile)) return;
      int slash = remotefile.LastIndexOf("/");

      if (slash > 0)
      {
        filename = remotefile.Substring(slash + 1);
        remotefile = remotefile.Substring(0, slash);
      }
      int questionMark = remotefile.LastIndexOf("?");
      if (questionMark > 0) path = remotefile.Substring(questionMark + 1);
    }

    /// <summary>
    /// Returns the local filename for a downloaded file
    /// remote file is in format remote:hostname?port?login?password?folder
    /// </summary>
    /// <param name="remotefile">remote filename+path</param>
    /// <returns>local filename+path</returns>
    public string GetLocalFilename(string remotefile)
    {
      //get the default share
      if (!IsRemote(remotefile)) return remotefile;

      foreach (Share share in m_shares)
      {
        if (share.IsFtpShare)
        {
          string filename, path;
          GetRemoteFileNameAndPath(remotefile, out filename, out path);

          string remoteFolder = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
            share.FtpServer, share.FtpPort, share.FtpLoginName, share.FtpPassword, Utils.RemoveTrailingSlash(share.FtpFolder));

          if (remotefile.IndexOf(remoteFolder) == 0)
          {
            string localFile = string.Format(@"{0}\{1}", Utils.RemoveTrailingSlash(share.Path), filename);
            return localFile;
          }
        }
      }
      return String.Empty;
    }

    /// <summary>
    /// Function to check if a remote file has been downloaded or not
    /// remote file is in format remote:hostname?port?login?password?folder
    /// </summary>
    /// <param name="file">remote filename + path</param>
    /// <returns>true: file is downloaded
    /// false: file is not downloaded or MP is busy downloading</returns>
    public bool IsRemoteFileDownloaded(string file, long size)
    {
      if (!IsRemote(file)) return true;

      //check if we're still downloading
      if (FtpConnectionCache.IsDownloading(file))
      {
        return false;
      }

      //nop then check if local file exists
      string localFile = GetLocalFilename(file);
      if (localFile == String.Empty) return false;
      if (System.IO.File.Exists(localFile))
      {
        FileInfo info = new FileInfo(localFile);
        if (info.Length == size)
        {
          //already downloaded
          return true;
        }
        // not completely downloaded yet
      }
      return false;
    }

    /// <summary>
    /// Function which checks if a remote file has been downloaded and if not
    /// asks the user whether it should download it
    /// remote file is in format remote:hostname?port?login?password?folder
    /// </summary>
    /// <param name="file">remote file</param>
    /// <returns>true: download file
    /// false: do not download file</returns>
    public bool ShouldWeDownloadFile(string file)
    {

      GUIMessage msg;
      if (FtpConnectionCache.IsDownloading(file))
      {
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, 0, 0, 0, 0, 0, 0);
        msg.Param1 = 916;
        msg.Param2 = 921;
        msg.Param3 = 0;
        msg.Param4 = 0;
        GUIWindowManager.SendMessage(msg);
        return false;
      }

      //file is remote, ask if user wants to download it
      //this file is on a remote share, ask if user wants to download it
      //to the default local share
      msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, 0);
      msg.Param1 = 916;
      msg.Param2 = 917;
      msg.Param3 = 918;
      msg.Param4 = 919;
      GUIWindowManager.SendMessage(msg);
      if (msg.Param1 == 1) return true;
      return false;
    }

    /// <summary>
    /// Function to download a remote file
    /// remote file is in format remote:hostname?port?login?password?folder
    /// </summary>
    /// <param name="file">remote file</param>
    /// <returns>true: download started,
    /// false: failed to start download</returns>
    /// <remarks>file is downloaded to the local folder specified in the setup</remarks>
    public bool DownloadRemoteFile(string file, long size)
    {
      if (IsRemoteFileDownloaded(file, size)) return true;

      //start download...
      FTPClient client = GetFtpClient(file);
      if (client == null) return false;

      string remoteFilename;
      string remotePath;
      GetRemoteFileNameAndPath(file, out remoteFilename, out remotePath);
      try
      {
        client.ChDir(remotePath);
      }
      catch (Exception)
      {
        FtpConnectionCache.Remove(client);
        client = GetFtpClient(file);
        if (client == null) return false;
      }
      try
      {
        client.ChDir(remotePath);
        string localFile = GetLocalFilename(file);
        return FtpConnectionCache.Download(client, file, remoteFilename, localFile);
      }
      catch (Exception ex)
      {
        Log.Write("VirtualDirectory:unable to start download:{0}", ex.Message);
      }
      return false;
    }

    /// <summary>
    /// Function which gets an available ftp client 
    /// if none is available it creates a new one
    /// remote file is in format remote:hostname?port?login?password?folder
    /// </summary>
    /// <param name="file">remote file/folder</param>
    /// <returns>FTP client or null</returns>
    FTPClient GetFtpClient(string file)
    {
      string folder = file.Substring("remote:".Length);
      string[] subitems = folder.Split(new char[] { '?' });
      if (subitems[4] == String.Empty) subitems[4] = "/";
      int port = 21;
      unchecked
      {
        port = Int32.Parse(subitems[1]);
      }
      FTPClient ftp;
      if (!FtpConnectionCache.InCache(subitems[0], subitems[2], subitems[3], port, out ftp))
      {
        ftp = FtpConnectionCache.MakeConnection(subitems[0], subitems[2], subitems[3], port);
      }
      if (ftp == null)
        Log.Write("VirtualDirectory:unable to connect to remote share");
      return ftp;
    }
    #region generics
    /// <summary>
    /// This method returns an arraylist of GUIListItems for the specified folder
    /// If the folder is protected by a pincode then the user is asked to enter the pincode
    /// and the folder contents is only returned when the pincode is correct
    /// </summary>
    /// <param name="strDir">folder</param>
    /// <returns>
    /// returns an arraylist of GUIListItems for the specified folder
    /// </returns>
    public List<GUIListItem> GetDirectoryExt(string strDir)
    {
      if (strDir == null)
      {
        m_strPreviousDir = "";
        return GetRootExt();
      }
      if (strDir == "")
      {
        m_strPreviousDir = "";
        return GetRootExt();
      }

      //if we have a folder like D:\
      //then remove the \
      if (strDir.Length == 2 && strDir.Substring(1) == @"\")
        strDir = strDir.Substring(0, strDir.Length - 1);

      List<GUIListItem> items = new List<GUIListItem>();

      //get the parent folder
      string strParent = "";
      if (IsRemote(strDir))
      {
        int ipos = strDir.LastIndexOf(@"/");
        if (ipos > 0)
        {
          strParent = strDir.Substring(0, ipos);
        }
      }
      else
      {
        int ipos = strDir.LastIndexOf(@"\");
        if (ipos > 0)
        {
          strParent = strDir.Substring(0, ipos);
        }
      }


      //is this directory protected
      int iPincodeCorrect;
      if (IsProtectedShare(strDir, out iPincodeCorrect))
      {
        bool retry = true;
        {
          while (retry)
          {
            //yes, check if this is a subdirectory of the share
            if (previousShare != currentShare)
            //if (previousShare==String.Empty || strDir.IndexOf(previousShare) < 0)
            {
              //no, then ask user to enter the pincode
              GUIMessage msgGetPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
              GUIWindowManager.SendMessage(msgGetPassword);
              int iPincode = -1;
              try
              {
                iPincode = Int32.Parse(msgGetPassword.Label);
              }
              catch (Exception)
              {
              }
              if (iPincode != iPincodeCorrect)
              {
                GUIMessage msgWrongPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WRONG_PASSWORD, 0, 0, 0, 0, 0, 0);
                GUIWindowManager.SendMessage(msgWrongPassword);

                if (!(bool)msgWrongPassword.Object)
                {
                  GUIListItem itemTmp = new GUIListItem();
                  itemTmp.IsFolder = true;
                  itemTmp.Label = "..";
                  itemTmp.Label2 = "";
                  itemTmp.Path = m_strPreviousDir;
                  Utils.SetDefaultIcons(itemTmp);
                  Utils.SetThumbnails(ref itemTmp);
                  items.Add(itemTmp);
                  return items;
                }
              }
              else
                retry = false;
            }
          }
        }
      }
      previousShare = currentShare;

      //check if this is an image file like .iso, .nrg,...
      //ifso then ask daemontools to automount it

      bool VirtualShare = false;
      if (!IsRemote(strDir))
      {
        if (DaemonTools.IsEnabled)
        {
          string extensionension = System.IO.Path.GetExtension(strDir);
          if (IsImageFile(extensionension))
          {
            bool askBeforePlayingDVDImage = false;

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
            {
              askBeforePlayingDVDImage = xmlreader.GetValueAsBool("daemon", "askbeforeplaying", false);
            }

            if (!DaemonTools.IsMounted(strDir))
            {
              if (!askBeforePlayingDVDImage)
              {
                AutoPlay.StopListening();

                // yield some time before we try to mount
                System.Threading.Thread.Sleep(50);
              }

              string virtualPath;
              if (DaemonTools.Mount(strDir, out virtualPath))
              {
                strDir = virtualPath;
                VirtualShare = true;
              }

              if (!askBeforePlayingDVDImage)
              {
                AutoPlay.StartListening();
              }
            }
            else
            {
              strDir = DaemonTools.GetVirtualDrive();
              VirtualShare = true;
            }

            if (VirtualShare /*&& !g_Player.Playing*/) // dont interrupt if we're already playing
            {
              if (!askBeforePlayingDVDImage)
              {
                // If it looks like a DVD directory structure then return so
                // that the playing of the DVD is handled by the caller.
                if (System.IO.File.Exists(strDir + @"\VIDEO_TS\VIDEO_TS.IFO"))
                {
                  return items;
                }
              }
            }
          }
        }
      }

      string[] strDirs = null;
      string[] strFiles = null;
      GUIListItem item = null;
      if (!IsRootShare(strDir) || VirtualShare)
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        Utils.SetDefaultIcons(item);
        Utils.SetThumbnails(ref item);

        if (strParent == strDir)
        {
          item.Path = "";
        }
        else
          item.Path = strParent;
        items.Add(item);
      }
      else
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        item.Path = "";
        Utils.SetDefaultIcons(item);
        Utils.SetThumbnails(ref item);
        items.Add(item);
      }

      if (IsRemote(strDir))
      {
        FTPClient ftp = GetFtpClient(strDir);
        if (ftp == null) return items;

        string folder = strDir.Substring("remote:".Length);
        string[] subitems = folder.Split(new char[] { '?' });
        if (subitems[4] == String.Empty) subitems[4] = "/";

        FTPFile[] files;
        try
        {
          ftp.ChDir(subitems[4]);
          files = ftp.DirDetails(); //subitems[4]);
        }
        catch (Exception)
        {
          //maybe this socket has timed out, remove it and get a new one
          FtpConnectionCache.Remove(ftp);
          ftp = GetFtpClient(strDir);
          if (ftp == null) return items;
          try
          {
            ftp.ChDir(subitems[4]);
          }
          catch (Exception ex)
          {
            Log.Write("VirtualDirectory:unable to chdir to remote folder:{0} reason:{1} {2}", subitems[4], ex.Message, ex.StackTrace);
            return items;
          }
          try
          {
            files = ftp.DirDetails(); //subitems[4]);
          }
          catch (Exception ex)
          {
            Log.Write("VirtualDirectory:unable to get remote folder:{0} reason:{1}  {2}", subitems[4], ex.Message, ex.StackTrace);
            return items;
          }
        }
        for (int i = 0; i < files.Length; ++i)
        {
          FTPFile file = files[i];
          //Log.Write("VirtualDirectory: {0} {1}",file.Name,file.Dir);
          if (file.Dir)
          {
            if (file.Name != "." && file.Name != "..")
            {
              item = new GUIListItem();
              item.IsFolder = true;
              item.Label = file.Name;
              item.Label2 = "";
              item.Path = String.Format("{0}/{1}", strDir, file.Name);
              item.IsRemote = true;
              item.FileInfo = null;
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
            }
          }
          else
          {
            if (IsValidExtension(file.Name))
            {
              item = new GUIListItem();
              item.IsFolder = false;
              item.Label = file.Name;
              item.Label2 = "";
              item.Path = String.Format("{0}/{1}", strDir, file.Name);
              item.IsRemote = true;
              if (IsRemoteFileDownloaded(item.Path, file.Size))
              {
                item.Path = GetLocalFilename(item.Path);
                item.IsRemote = false;
              }
              else if (FtpConnectionCache.IsDownloading(item.Path))
              {
                item.IsDownloading = true;
              }
              item.FileInfo = new FileInformation();
              DateTime modified = file.LastModified;
              item.FileInfo.CreationTime = modified;
              item.FileInfo.Length = file.Size;
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
            }
          }
        }
      }
      else
      {
        try
        {
          strDirs = System.IO.Directory.GetDirectories(strDir + @"\");
          strFiles = System.IO.Directory.GetFiles(strDir + @"\");
        }
        catch (Exception)
        {
        }


        if (strDirs != null)
        {
          for (int i = 0; i < strDirs.Length; ++i)
          {
            string strPath = strDirs[i].Substring(strDir.Length + 1);

            // Skip hidden folders
            if ((File.GetAttributes(strDir + @"\" + strPath) & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
              continue;
            }

            item = new GUIListItem();
            item.IsFolder = true;
            item.Label = strPath;
            item.Label2 = "";
            item.Path = strDirs[i];
            Utils.SetDefaultIcons(item);
            Utils.SetThumbnails(ref item);

            items.Add(item);
          }
        }

        if (strFiles != null)
        {
          for (int i = 0; i < strFiles.Length; ++i)
          {
            string extensionension = System.IO.Path.GetExtension(strFiles[i]);
            if (IsImageFile(extensionension))
            {
              if (DaemonTools.IsEnabled)
              {

                item = new GUIListItem();
                item.IsFolder = true;
                item.Label = Utils.GetFilename(strFiles[i]);
                item.Label2 = "";
                item.Path = strFiles[i];
                item.FileInfo = null;
                Utils.SetDefaultIcons(item);
                Utils.SetThumbnails(ref item);
                items.Add(item);
                continue;
              }
            }
            if (IsValidExtension(strFiles[i]))
            {
              // Skip hidden files
              if ((File.GetAttributes(strFiles[i]) & FileAttributes.Hidden) == FileAttributes.Hidden)
              {
                continue;
              }

              item = new GUIListItem();
              item.IsFolder = false;
              item.Label = Utils.GetFilename(strFiles[i]);
              item.Label2 = "";
              item.Path = strFiles[i];

              item.FileInfo = new FileInformation(strFiles[i]);
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
            }
          }
        }
      }
      m_strPreviousDir = strDir;
      return items;
    }


    /// <summary>
    /// This method returns an arraylist of GUIListItems for the specified folder
    /// This method does not check if the folder is protected by an pincode. it will
    /// always return all files/subfolders present
    /// </summary>
    /// <param name="strDir">folder</param>
    /// <returns>
    /// returns an arraylist of GUIListItems for the specified folder
    /// </returns>
    public List<GUIListItem> GetDirectoryUnProtectedExt(string strDir, bool useExtensions)
    {
      if (strDir == null) return GetRootExt();
      if (strDir == "") return GetRootExt();

      if (strDir.Substring(1) == @"\") strDir = strDir.Substring(0, strDir.Length - 1);
      List<GUIListItem> items = new List<GUIListItem>();

      string strParent = "";
      int ipos = strDir.LastIndexOf(@"\");
      if (ipos > 0)
      {
        strParent = strDir.Substring(0, ipos);
      }

      GUIListItem item = null;
      if (IsRemote(strDir))
      {
        FTPClient ftp = GetFtpClient(strDir);
        if (ftp == null) return items;

        string folder = strDir.Substring("remote:".Length);
        string[] subitems = folder.Split(new char[] { '?' });
        if (subitems[4] == String.Empty) subitems[4] = "/";

        FTPFile[] files;
        try
        {
          ftp.ChDir(subitems[4]);
          files = ftp.DirDetails(); //subitems[4]);
        }
        catch (Exception)
        {
          //maybe this socket has timed out, remove it and get a new one
          FtpConnectionCache.Remove(ftp);
          ftp = GetFtpClient(strDir);
          if (ftp == null) return items;
          try
          {
            ftp.ChDir(subitems[4]);
          }
          catch (Exception ex)
          {
            Log.Write("VirtualDirectory:unable to chdir to remote folder:{0} reason:{1} {2}", subitems[4], ex.Message, ex.StackTrace);
            return items;
          }
          try
          {
            files = ftp.DirDetails(); //subitems[4]);
          }
          catch (Exception ex)
          {
            Log.Write("VirtualDirectory:unable to get remote folder:{0} reason:{1}  {2}", subitems[4], ex.Message, ex.StackTrace);
            return items;
          }
        }
        for (int i = 0; i < files.Length; ++i)
        {
          FTPFile file = files[i];
          //Log.Write("VirtualDirectory: {0} {1}",file.Name,file.Dir);
          if (file.Dir)
          {
            if (file.Name != "." && file.Name != "..")
            {
              item = new GUIListItem();
              item.IsFolder = true;
              item.Label = file.Name;
              item.Label2 = "";
              item.Path = String.Format("{0}/{1}", strDir, file.Name);
              item.IsRemote = true;
              item.FileInfo = null;
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
            }
          }
          else
          {
            if (IsValidExtension(file.Name) || (useExtensions == false))
            {
              item = new GUIListItem();
              item.IsFolder = false;
              item.Label = file.Name;
              item.Label2 = "";
              item.Path = String.Format("{0}/{1}", strDir, file.Name);
              item.IsRemote = true;
              if (IsRemoteFileDownloaded(item.Path, file.Size))
              {
                item.Path = GetLocalFilename(item.Path);
                item.IsRemote = false;
              }
              else if (FtpConnectionCache.IsDownloading(item.Path))
              {
                item.IsDownloading = true;
              }
              item.FileInfo = new FileInformation();
              DateTime modified = file.LastModified;
              item.FileInfo.CreationTime = modified;
              item.FileInfo.Length = file.Size;
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
            }
          }
        }
      }


      bool VirtualShare = false;
      if (DaemonTools.IsEnabled)
      {
        string extensionension = System.IO.Path.GetExtension(strDir);
        if (IsImageFile(extensionension))
        {
          if (!DaemonTools.IsMounted(strDir))
          {
            string virtualPath;
            if (DaemonTools.Mount(strDir, out virtualPath))
            {
              strDir = virtualPath;
              VirtualShare = true;
            }
          }
          else
          {
            strDir = DaemonTools.GetVirtualDrive();
            VirtualShare = true;
          }
        }
      }

      string[] strDirs = null;
      string[] strFiles = null;
      try
      {
        strDirs = System.IO.Directory.GetDirectories(strDir + @"\");
        strFiles = System.IO.Directory.GetFiles(strDir + @"\");
      }
      catch (Exception)
      {
      }


      if (!IsRootShare(strDir) || VirtualShare)
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        Utils.SetDefaultIcons(item);
        Utils.SetThumbnails(ref item);

        if (strParent == strDir)
        {
          item.Path = "";
        }
        else
          item.Path = strParent;
        items.Add(item);
      }
      else
      {
        item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        item.Label2 = "";
        item.Path = "";
        Utils.SetDefaultIcons(item);
        Utils.SetThumbnails(ref item);
        items.Add(item);
      }
      if (strDirs != null)
      {
        for (int i = 0; i < strDirs.Length; ++i)
        {
          string strPath = strDirs[i].Substring(strDir.Length + 1);

          // Skip hidden folders
          if ((File.GetAttributes(strDir + @"\" + strPath) & FileAttributes.Hidden) == FileAttributes.Hidden)
          {
            continue;
          }

          item = new GUIListItem();
          item.IsFolder = true;
          item.Label = strPath;
          item.Label2 = "";
          item.Path = strDirs[i];
          Utils.SetDefaultIcons(item);
          Utils.SetThumbnails(ref item);

          items.Add(item);
        }
      }

      if (strFiles != null)
      {
        for (int i = 0; i < strFiles.Length; ++i)
        {
          string extensionension = System.IO.Path.GetExtension(strFiles[i]);
          if (IsImageFile(extensionension))
          {
            if (DaemonTools.IsEnabled)
            {

              item = new GUIListItem();
              item.IsFolder = true;
              item.Label = Utils.GetFilename(strFiles[i]);
              item.Label2 = "";
              item.Path = strFiles[i];
              item.FileInfo = null;
              Utils.SetDefaultIcons(item);
              Utils.SetThumbnails(ref item);
              items.Add(item);
              continue;
            }
          }
          if (IsValidExtension(strFiles[i]) || (!useExtensions))
          {
            // Skip hidden files
            if ((File.GetAttributes(strFiles[i]) & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
              if (useExtensions)
                continue;
            }

            item = new GUIListItem();
            item.IsFolder = false;
            item.Label = Utils.GetFilename(strFiles[i]);
            item.Label2 = "";
            item.Path = strFiles[i];
            item.FileInfo = new FileInformation(strFiles[i]);
            Utils.SetDefaultIcons(item);
            Utils.SetThumbnails(ref item);
            items.Add(item);
          }
        }
      }
      return items;
    }
    /// <summary>
    /// This method will return the root folder
    /// which contains a list of all shares
    /// </summary>
    /// <returns>
    /// ArrayList containing a GUIListItem for each share
    /// </returns>
    public List<GUIListItem> GetRootExt()
    {
      previousShare = String.Empty;

      List<GUIListItem> items = new List<GUIListItem>();
      foreach (Share share in m_shares)
      {
        GUIListItem item = new GUIListItem();
        item.Label = share.Name;
        item.Path = share.Path;
        if (Utils.IsRemovable(item.Path) && Directory.Exists(item.Path))
        {
          string strDriveName = Utils.GetDriveName(item.Path);
          if (strDriveName == "") strDriveName = "Removable";
          item.Label = String.Format("{1} {0}", item.Path, strDriveName);
        }
        if (Utils.IsDVD(item.Path))
        {
          item.DVDLabel = Utils.GetDriveName(item.Path);
          item.DVDLabel = item.DVDLabel.Replace('_', ' ');
        }

        if (item.DVDLabel != "")
        {
          item.Label = String.Format("({0}) {1}", item.Path, item.DVDLabel);
        }
        else
          item.Label = share.Name;
        item.IsFolder = true;

        if (share.IsFtpShare)
        {
          item.Path = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
                share.FtpServer, share.FtpPort, share.FtpLoginName, share.FtpPassword, Utils.RemoveTrailingSlash(share.FtpFolder));
        }
        if (Directory.Exists(item.Path))
        {
          item.IconImage = Utils.GetCoverArtName(item.Path, "folder");
          item.IconImageBig = Utils.GetLargeCoverArtName(item.Path, "folder");
        }
        else
        {
          Utils.SetDefaultIcons(item);
        }
        items.Add(item);
      }

      // add removable drives with media
      string[] drives = Environment.GetLogicalDrives();
      foreach (string drive in drives)
      {
        if (drive[0] > 'B' && Util.Utils.getDriveType(drive) == Removable)
        {
          bool driveFound = false;
          string driveName = Util.Utils.GetDriveName(drive);
          string driveLetter = drive.Substring(0, 1).ToUpper() + ":";
          if (driveName == "") driveName = "Removable";

          //
          // Check if the share already exists
          //
          foreach (Share share in m_shares)
          {
            if (share.Path == driveLetter)
            {
              driveFound = true;
              break;
            }
          }

          if (driveFound == false)
          {
            GUIListItem item = new GUIListItem();
            item.Path = driveLetter;
            item.Label = String.Format("{0} {1}", driveName, driveLetter);
            item.IsFolder = true;

            Utils.SetDefaultIcons(item);

            // dont add removable shares without media
            // virtual cd/dvd drive (daemontools) without mounted image
            if (!Directory.Exists(item.Path))
              break;

            items.Add(item);
          }
        }
      }

      return items;
    }

    #endregion
  }
}
