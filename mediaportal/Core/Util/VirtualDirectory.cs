using System;
using System.Collections;
using MediaPortal.GUI.Library;
using System.Management;
using System.IO;

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
      string strRoot = "";
      if (strDir != "cdda:")
        strRoot = System.IO.Path.GetFullPath(strDir);
      else 
        strRoot = strDir;

      foreach (Share share in m_shares)
      {
        try
        {
          if (System.IO.Path.GetFullPath(share.Path) == strRoot)
          {
            return true;
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
      string strRoot = "";
      if (strDir != "cdda:")
        strRoot = System.IO.Path.GetFullPath(strDir);
      else 
        strRoot = strDir;

      foreach (Share share in m_shares)
      {
        try
        {
          if (System.IO.Path.GetFullPath(share.Path) == strRoot)
          {
            iPincode = share.Pincode;
            if (share.Pincode >= 0)
              return true;
            return false;
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
      
      if (strDir.Substring(1) == @"\") strDir = strDir.Substring(0, strDir.Length - 1);
      ArrayList items = new ArrayList();
      
      string strParent = "";
      int ipos = strDir.LastIndexOf(@"\");
      if (ipos > 0)
      {
        strParent = strDir.Substring(0, ipos);
      }


      int iPincodeCorrect;
      if (IsProtectedShare(strDir, out iPincodeCorrect))
      {
        if (m_strPreviousDir.IndexOf(strDir) < 0)
        {
          //check pincode
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
            item.FileInfo = new System.IO.FileInfo(strFiles[i]);
            Utils.SetDefaultIcons(item);
            Utils.SetThumbnails(ref item);
            items.Add(item);
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
            item.FileInfo = new System.IO.FileInfo(strFiles[i]);
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
  }
}
