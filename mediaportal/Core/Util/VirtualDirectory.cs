using System;
using System.Collections;
using MediaPortal.GUI.Library;
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

    ArrayList m_shares = new ArrayList();
    ArrayList m_extensions = null;
    string	  m_strPreviousDir = String.Empty;
    
		/// <summary>
		/// constructor
		/// </summary>
    public VirtualDirectory()
		{
		}
    
    public void AddDrives()
    {
    }
    
		/// <summary>
		/// Method to set a list of all file extensions
		/// The virtual directory will only show folders and files which match one of those file extensions
		/// </summary>
		/// <param name="extensions">string arraylist of file extensions</param>
    public void SetExtensions(ArrayList extensions)
    {
			if (extensions==null) return;
      m_extensions = extensions;
    }
		/// <summary>
		/// Method to add a new file extension to the file extensions list
		/// </summary>
		/// <param name="strExtension">string containg the new extension in the format .mp3</param>
    public void AddExtension(string strExtension)
    {
			if (m_extensions == null)
				m_extensions = new ArrayList();
      m_extensions.Add(strExtension);
    }

		/// <summary>
		/// Add a new share to this virtual directory
		/// </summary>
		/// <param name="share">new Share</param>
    public void Add(Share share)
    {
			if (share==null) return;
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
      ArrayList items = new ArrayList();
      foreach (Share share in m_shares)
      {
        GUIListItem item = new GUIListItem();
        item.Label = share.Name;
        item.Path = share.Path;
        if (Utils.IsDVD(item.Path))
        {
          item.DVDLabel = Utils.GetDriveName(item.Path);
          item.DVDLabel = item.DVDLabel.Replace('_', ' ');
        }

        if (item.DVDLabel != "")
        {
          item.Label = String.Format( "({0}) {1}",  item.Path, item.DVDLabel);        
        }
        else
          item.Label = share.Name;
				item.IsFolder = true;

        if (share.IsFtpShare)
        {
          item.Path = String.Format("remote:{0}?{1}?{2}?{3}?{4}",
                share.FtpServer,share.FtpPort,share.FtpLoginName,share.FtpPassword,share.Path);
        }
				Utils.SetDefaultIcons(item);
        items.Add(item);
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
			if (strDir==null) return false;
      if (strDir.Length <= 0) return false;
      string strRoot = strDir;
      bool isRemote=IsRemote(strDir);
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
              string remoteFolder=String.Format("remote:{0}?{1}?{2}?{3}?{4}",
                share.FtpServer,share.FtpPort,share.FtpLoginName,share.FtpPassword,share.Path);
              if (remoteFolder==strDir) return true;
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
			if (strDir==null) return false;
      if (strDir.Length <= 0) return false;
      string strRoot = strDir;

      bool isRemote=IsRemote(strDir);
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
              string remoteFolder=String.Format("remote:{0}?{1}?{2}?{3}?{4}",
                share.FtpServer,share.FtpPort,share.FtpLoginName,share.FtpPassword,share.Path);
              if (strDir.IndexOf(remoteFolder)>=0) 
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
            if (System.IO.Path.GetFullPath(share.Path) == strRoot)
            {
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

		/// <summary>
		/// This method check is the given extension is a image file
		/// </summary>
		/// <param name="strExtension">file extension</param>
		/// <returns>
		/// true: if file is an image file (.img, .nrg, .bin, .iso)
		/// false: if the file is not an image file
		/// </returns>
    static public bool IsImageFile(string strExtension)
		{
			if (strExtension==null) return false;
			if (strExtension==String.Empty) return false;
      strExtension=strExtension.ToLower();
      if (strExtension==".img" ||strExtension==".bin" ||strExtension==".iso" || strExtension==".nrg" )
      {
        return true;
      }
      return false;
    }

    public bool IsRemote(string folder)
    {
      if (folder==null) return false;
      if (folder.IndexOf("remote:")==0) return true;
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
			if (strDir==null) 
			{
				m_strPreviousDir = "";
				return GetRoot();
			}
      if (strDir == "") 
      {
        m_strPreviousDir = "";
        return GetRoot();
      }
     
      //if we have a folder like D:\
      //then remove the \
      if (strDir.Length==2 && strDir.Substring(1) == @"\") 
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
      if (IsProtectedShare(strDir, out iPincodeCorrect))
      {
        //yes, check if this is a subdirectory of the share
        if (m_strPreviousDir.IndexOf(strDir) < 0)
        {
          //no, then ask user to enter the pincode
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
          GUIWindowManager.SendMessage(msg);
          int iPincode = -1;
          try
          {
             iPincode = Int32.Parse(msg.Label);
            
          }
          catch (Exception)
          {
          }
          if (iPincode != iPincodeCorrect)
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
      }


      //check if this is an image file like .iso, .nrg,...
      //ifso then ask daemontools to automount it

      bool VirtualShare=false;
      if (!IsRemote(strDir))
      {
        if (DaemonTools.IsEnabled)
        {
          string strExtension=System.IO.Path.GetExtension(strDir);
          if ( IsImageFile(strExtension) )
          {
            if (!DaemonTools.IsMounted(strDir))
            {
              string virtualPath;
              if (DaemonTools.Mount(strDir,out virtualPath))
              {
                strDir=virtualPath;
                VirtualShare=true;
              }
            }
            else
            {
              strDir=DaemonTools.GetVirtualDrive();
              VirtualShare=true;
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
        FTPClient ftp=GetFtpClient(strDir);
        if (ftp==null) return items;

        string folder=strDir.Substring( "remote:".Length);
        string[] subitems = folder.Split(new char[]{'?'});
        if (subitems[4]==String.Empty) subitems[4]="/";

        FTPFile[] files;
        try
        {
          ftp.ChDir(subitems[4]);
          files=ftp.DirDetails(subitems[4]);
        }
        catch(Exception)
        {
          //maybe this socket has timed out, remove it and get a new one
          FtpConnectionCache.Remove(ftp);
          ftp=GetFtpClient(strDir);
          if (ftp==null) return items;
          try
          {
            ftp.ChDir(subitems[4]);
            files=ftp.DirDetails(subitems[4]);
          }
          catch(Exception)
          {
            return items;
          }
        }
        for (int i=0; i < files.Length; ++i)
        {
          FTPFile file=files[i];
          if (file.Dir)
          {
            item = new GUIListItem();
            item.IsFolder = true;
            item.Label = file.Name;
            item.Label2 = "";
            item.Path = String.Format("{0}/{1}",strDir,file.Name);
            Utils.SetDefaultIcons(item);
            Utils.SetThumbnails(ref item);
            items.Add(item);
          }
          else
          {
            item = new GUIListItem();
            item.IsFolder = false;
            item.Label = file.Name;
            item.Label2 = "";
            item.Path = String.Format("{0}/{1}",strDir,file.Name);
            item.FileInfo = new FileInformation();
            DateTime modified=file.LastModified;
            item.FileInfo.CreationTime=modified;
            item.FileInfo.Length=file.Size;
            Utils.SetDefaultIcons(item);
            Utils.SetThumbnails(ref item);
            items.Add(item);
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
            string strExtension=System.IO.Path.GetExtension(strFiles[i]);
            if (IsImageFile(strExtension))
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
    public ArrayList GetDirectoryUnProtected(string strDir)
    {
			if (strDir==null) return GetRoot();
      if (strDir == "") return GetRoot();
      
      if (strDir.Substring(1) == @"\") strDir = strDir.Substring(0, strDir.Length - 1);
      ArrayList items = new ArrayList();
      
      string strParent = "";
      int ipos = strDir.LastIndexOf(@"\");
      if (ipos > 0)
      {
        strParent = strDir.Substring(0, ipos);
      }


      bool VirtualShare=false;
      if (DaemonTools.IsEnabled)
      {
        string strExtension=System.IO.Path.GetExtension(strDir);
        if ( IsImageFile(strExtension) )
        {
          if (!DaemonTools.IsMounted(strDir))
          {
            string virtualPath;
            if (DaemonTools.Mount(strDir,out virtualPath))
            {
              strDir=virtualPath;
              VirtualShare=true;
            }
          }
          else
          {
            strDir=DaemonTools.GetVirtualDrive();
            VirtualShare=true;
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
          string strExtension=System.IO.Path.GetExtension(strFiles[i]);
          if (IsImageFile(strExtension))
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
			if (strPath==null) return false;
			if (strPath==String.Empty) return false;
			try
			{
				if (!System.IO.Path.HasExtension(strPath)) return false;
			{
				string strExtFile = System.IO.Path.GetExtension(strPath);
				foreach (string strExt in m_extensions)
				{
					if (strExt.ToLower() == strExtFile.ToLower()) return true;
				}
			}
			}
			catch(Exception){}

      return false;
    }
    
    /// <summary>
    /// Returns the path of the default share
    /// </summary>
    /// <returns>Returns the path of the default share</returns>
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
    /// Function to split get the path and filename for a remote file
    /// </summary>
    /// <param name="remotefile">remote file</param>
    /// <param name="filename">on return contains the filename</param>
    /// <param name="path">on return contains the path</param>
    public void GetRemoteFileNameAndPath(string remotefile, out string filename, out string path)
    {
      filename=String.Empty;
      path="/";
      if (!IsRemote(remotefile)) return ;
      int slash=remotefile.LastIndexOf("/");

      if (slash>0) 
      {
        filename=remotefile.Substring(slash+1);
        remotefile=remotefile.Substring(0,slash);
      }
      int questionMark=remotefile.LastIndexOf("?");
      if (questionMark>0) path=remotefile.Substring(questionMark+1);
    }

    /// <summary>
    /// Returns the local filename for a downloaded file
    /// </summary>
    /// <param name="remotefile">remote filename+path</param>
    /// <returns>local filename+path</returns>
    public string GetLocalFilename(string remotefile)
    {
      //get the default share
      if (!IsRemote(remotefile)) return remotefile;

      foreach (Share share in m_shares)
      {
        if (!share.IsFtpShare && share.Default)
        {
          int slash=remotefile.LastIndexOf("/");
          if (slash>0) remotefile=remotefile.Substring(slash+1);
          else 
          {
            int questionMark=remotefile.LastIndexOf("?");
            if (questionMark>0) remotefile=remotefile.Substring(questionMark+1);
          }            
          
          string localFile=String.Format(@"{0}\{1}", share.Path,remotefile);
          return localFile;
        }
      }
      return String.Empty;
    }

    /// <summary>
    /// Function to check if a remote file has been downloaded or not
    /// </summary>
    /// <param name="file">remote filename + path</param>
    /// <returns>true: file is downloaded
    /// false: file is not downloaded or MP is busy downloading</returns>
    public bool IsRemoteFileDownloaded(string file)
    {
      if (!IsRemote(file)) return true;

      //check if we're still downloading
      string remoteFilename;
      string remotePath;
      GetRemoteFileNameAndPath(file, out remoteFilename, out remotePath);
      if (FtpConnectionCache.IsDownloading(remoteFilename))
      {
        return false;
      }
      
      //nop then check if local file exists
      string localFile=GetLocalFilename(file);
      if (localFile==String.Empty) return false;
      if (System.IO.File.Exists(localFile))
      {
        //already downloaded
        return true;
      }
      return false;
    }

    /// <summary>
    /// Function which checks if a remote file has been downloaded and if not
    /// asks the user whether it should download it
    /// </summary>
    /// <param name="file">remote file</param>
    /// <returns>true: download file
    /// false: do not download file</returns>
    public bool ShouldWeDownloadFile(string file)
    {

      string remoteFilename;
      string remotePath;
      GetRemoteFileNameAndPath(file, out remoteFilename, out remotePath);
      GUIMessage msg ;
      if (FtpConnectionCache.IsDownloading(remoteFilename))
      {
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING,0,0,0,0,0,0);
        msg.Param1=916;
        msg.Param2=921;
        msg.Param3=0;
        msg.Param4=0;
        GUIWindowManager.SendMessage(msg);
        return false;
      }

      //file is remote, ask if user wants to download it
      //this file is on a remote share, ask if user wants to download it
      //to the default local share
      msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO,0,0,0,0,0,0);
      msg.Param1=916;
      msg.Param2=917;
      msg.Param3=918;
      msg.Param4=919;
      GUIWindowManager.SendMessage(msg);
      if (msg.Param1==1) return true;
      return false;
    }

    /// <summary>
    /// Function to download a remote file
    /// </summary>
    /// <param name="file">remote file</param>
    /// <returns>true: download started,
    /// false: failed to start download</returns>
    /// <remarks>file is downloaded to the default share</remarks>
    public bool DownloadRemoteFile(string file)
    {
      if (IsRemoteFileDownloaded(file)) return true;

      //start download...
      FTPClient client = GetFtpClient(file);
      if (client ==null) return false;

      string remoteFilename;
      string remotePath;
      GetRemoteFileNameAndPath(file, out remoteFilename, out remotePath);
      try
      {
        client.ChDir(remotePath);
      }
      catch(Exception)
      {
        FtpConnectionCache.Remove(client);
        client = GetFtpClient(file);
        if (client==null) return false;
      }
      try
      {
        client.ChDir(remotePath);
        string localFile=String.Format(@"{0}\{1}", GetDefaultPath(),remoteFilename);
        return FtpConnectionCache.Download(client,remoteFilename,localFile);
      }
      catch(Exception)
      {
      }
      return false;
    }
  
    /// <summary>
    /// Function which gets an available ftp client 
    /// if none is available it creates a new one
    /// </summary>
    /// <param name="file">remote file/folder</param>
    /// <returns>FTP client or null</returns>
    FTPClient GetFtpClient(string file)
    {
      string folder=file.Substring( "remote:".Length);
      string[] subitems = folder.Split(new char[]{'?'});
      if (subitems[4]==String.Empty) subitems[4]="/";
      int port=21;
      unchecked
      {
        port=Int32.Parse(subitems[1]);
      }
      FTPClient ftp;
      if (!FtpConnectionCache.InCache(subitems[0],subitems[2],subitems[3], port,out ftp))
      {
        ftp=FtpConnectionCache.MakeConnection(subitems[0],subitems[2],subitems[3], port);
      }
      return ftp;
    }
  }
}
