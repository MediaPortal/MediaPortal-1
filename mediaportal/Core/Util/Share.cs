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

using System;

namespace MediaPortal.Util
{
  /// <summary>
  /// Helper class which contains a single share
  /// a share has a 
  /// - name
  /// - drive/path
  /// - pincode
  /// and can be the default share or not. WHen a share is default it will
  /// be shown when the user first selects the share
  /// </summary>
  public class Share
  {
    public enum Views
    {
      List = 0,
      Icons = 1,
      BigIcons = 2,
      Albums = 3,
      FilmStrip = 4
    }

    private string m_strPath = string.Empty;
    private string m_strName = string.Empty;
    private bool m_bDefault = false;
    private int m_iPincode = -1;
    private bool isRemote = false;
    private string remoteServer = string.Empty;
    private string remoteLogin = string.Empty;
    private string remotePassword = string.Empty;
    private string remoteFolder = string.Empty;
    private int remotePort = 21;
    private bool runtimeAdded;
    public Views DefaultView = Views.List;

    /// <summary>
    /// empty constructor
    /// </summary>
    public Share() {}

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="strName">share name</param>
    /// <param name="strPath">share folder</param>
    public Share(string strName, string strPath)
    {
      if (strName == null || strPath == null)
        return;
      if (strName == string.Empty || strPath == string.Empty)
        return;
      m_strName = strName;
      m_strPath = Utils.RemoveTrailingSlash(strPath);
    }


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="strName">share name</param>
    /// <param name="strPath">share folder</param>
    /// <param name="iPincode">pincode for folder (-1 = no pincode)</param>
    public Share(string strName, string strPath, int iPincode)
    {
      if (strName == null || strPath == null)
        return;
      if (strName == string.Empty || strPath == string.Empty)
        return;
      m_strName = strName;
      m_strPath = Utils.RemoveTrailingSlash(strPath);
      m_iPincode = iPincode;
    }

    /// <summary>
    /// property to get/set the pincode for the share
    /// (-1 means no pincode)
    /// </summary>
    public int Pincode
    {
      get { return m_iPincode; }
      set { m_iPincode = value; }
    }

    /// <summary>
    /// Property to get/set the share name
    /// </summary>
    public string Name
    {
      get { return m_strName; }
      set
      {
        if (value == null)
          return;
        m_strName = value;
      }
    }

    /// <summary>
    /// Property to get/set the share folder
    /// </summary>
    public string Path
    {
      get { return m_strPath; }
      set
      {
        if (value == null)
          return;
        m_strPath = Utils.RemoveTrailingSlash(value);
      }
    }

    /// <summary>
    /// Property to get/set whether this is a local or ftp share
    /// </summary>
    public bool IsFtpShare
    {
      get { return isRemote; }
      set { isRemote = value; }
    }

    /// <summary>
    /// Property to get/set the ftp server
    /// </summary>
    public string FtpServer
    {
      get { return remoteServer; }
      set
      {
        if (value == null)
          return;
        remoteServer = value;
      }
    }

    /// <summary>
    /// Property to get/set the ftp login name
    /// </summary>
    public string FtpLoginName
    {
      get { return remoteLogin; }
      set
      {
        if (value == null)
          return;
        remoteLogin = value;
      }
    }

    /// <summary>
    /// Property to get/set the ftp folder
    /// </summary>
    public string FtpFolder
    {
      get { return remoteFolder; }
      set
      {
        if (value == null)
          return;
        remoteFolder = value;
      }
    }

    /// <summary>
    /// Property to get/set the ftp password
    /// </summary>
    public string FtpPassword
    {
      get { return remotePassword; }
      set
      {
        if (value == null)
          return;
        remotePassword = value;
      }
    }

    /// <summary>
    /// Property to get/set the ftp port
    /// </summary>
    public int FtpPort
    {
      get { return remotePort; }
      set
      {
        if (value <= 0)
          return;
        remotePort = value;
      }
    }

    /// <summary>
    /// Property to get/set this share as the default share
    /// </summary>
    public bool Default
    {
      get { return m_bDefault; }
      set { m_bDefault = value; }
    }

    public bool RuntimeAdded
    {
      get { return runtimeAdded; }
      set { runtimeAdded = value; }
    }
  }
}