using System;
using System.Collections;
using MediaPortal.GUI.Library;
using System.Management;
using System.IO;

namespace MediaPortal.Util
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class VirtualDirectory
	{
    const int Removable = 2;
    const int LocalDisk = 3;
    const int Network = 4;
    const int CD = 5;

    ArrayList m_shares = new ArrayList();
    ArrayList m_extensions = null;
    string m_strPreviousDir = "";
    
    public VirtualDirectory()
		{
		}
    
    public void AddDrives()
    {
      /*
      //get drive collection 
      string[] drives=System.IO.Directory.GetLogicalDrives();
      foreach (string strDrive in drives)
      {
        bool bAdd=true;
        if (strDrive.ToLower().StartsWith("a:") || strDrive.ToLower().StartsWith("b:"))
        {
          bAdd=false;
        }
        if (bAdd)
        {
          string strDes=String.Format("{0} ({1})", Utils.GetDriveName(strDrive), strDrive);
          Share share=new Share( strDes, strDrive);
          m_shares.Add(share);
        }
      }*/
    }
    
    public void SetExtensions(ArrayList extensions)
    {
      m_extensions = extensions;
    }
    public void AddExtension(string strExtension)
    {
			if (m_extensions == null)
				m_extensions = new ArrayList();
      m_extensions.Add(strExtension);
    }

    public void Add(Share share)
    {
      m_shares.Add(share);
		}

		public void Clear()
		{
			m_shares.Clear();
		}

    public ArrayList GetRoot()
    {
      ArrayList items = new ArrayList();
      foreach (Share share in m_shares)
      {
        GUIListItem item = new GUIListItem();
        item.Label = share.Name;
        item.Path = share.Path;
				item.IsFolder = true;
				Utils.SetDefaultIcons(item);
        items.Add(item);
      }
      return items;
    }

    public bool IsRootShare(string strDir)
    {
      
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

    public bool IsProtectedShare(string strDir, out int iPincode)
    {
      iPincode = -1;
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

    static public bool IsImageFile(string strExtension)
    {
      strExtension=strExtension.ToLower();
      if (strExtension==".img" ||strExtension==".bin" ||strExtension==".iso" || strExtension==".nrg" )
      {
        return true;
      }
      return false;
    }

    public ArrayList GetDirectory(string strDir)
    {
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
          GUIGraphicsContext.SendMessage(msg);
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


    public ArrayList GetDirectoryUnProtected(string strDir)
    {
      if (strDir == "") 
      {
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

    bool IsValidExtension(string strPath)
    {
      if (!System.IO.Path.HasExtension(strPath)) return false;
      {
        string strExtFile = System.IO.Path.GetExtension(strPath);
        foreach (string strExt in m_extensions)
        {
          if (strExt.ToLower() == strExtFile.ToLower()) return true;
        }
      }
      return false;
    }
  }
}
