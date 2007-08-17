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
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Resources;
using System.Globalization;
using System.Xml;

namespace MediaPortal.DeployTool
{
  public sealed class Localizer
  {
    #region Singleton implementation
    static readonly Localizer _instance = new Localizer();
    static Localizer()
    {
    }
    Localizer()
    {
      _rscMan = new ResourceManager("MediaPortal.DeployTool.MediaPortal.DeployTool", System.Reflection.Assembly.GetExecutingAssembly());
    }
    public static Localizer Instance
    {
      get
      {
        return _instance;
      }
    }
    #endregion

    #region Variables
    private ResourceManager _rscMan;
    #endregion

    public string GetString(string id)
    {
      return _rscMan.GetString(id);
    }
    public void SwitchCulture(string cultureId)
    {
      System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureId);
    }
  }
  class Utils
  {
    #region DialogHelper
    public static void ErrorDlg(string msg)
    {
      MessageBox.Show(msg,"MediaPortal Deploy Tool -- Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
    }
    public static void InfoDlg(string msg)
    {
      MessageBox.Show(msg, "MediaPortal Deploy Tool -- Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    #endregion

    public static string GetDownloadURL(string id)
    {
      XmlDocument doc=new XmlDocument();
      doc.Load(Application.StartupPath+"\\ApplicationLocations.xml");
      XmlNode node=doc.SelectSingleNode("/Applications/"+id+"/URL");
      return node.InnerText;
    }
    public static string GetDownloadFile(string id)
    {
      XmlDocument doc = new XmlDocument();
      doc.Load(Application.StartupPath + "\\ApplicationLocations.xml");
      XmlNode node = doc.SelectSingleNode("/Applications/" + id + "/FILE");
      return node.InnerText;
    }

    public static bool CheckTargetDir(string dir)
    {
      if (dir == "")
        return false;
      if (Directory.Exists(dir))
        return true;
      DirectoryInfo info=null;
      try
      {
        info=Directory.CreateDirectory(dir);
      }
      catch
      {
        return false;
      }
      if (info == null)
        return false;
      else
      {
        Directory.Delete(dir);
        return true;
      }
    }

    public static void UnzipFile(string zipArchive, string fileToExtract, string targetFile)
    {
      File.Delete(targetFile);
      ZipFile zip=new ZipFile(zipArchive);
      int idx=zip.FindEntry(fileToExtract,true);
      Stream zipStream=zip.GetInputStream(idx);
      FileStream target=new FileStream(targetFile,FileMode.CreateNew);
      byte[] buffer=new byte[1024];
      int bytesRead;
      do
      {
        bytesRead = zipStream.Read(buffer, 0, buffer.Length);
        if (bytesRead > 0)
          target.Write(buffer, 0, bytesRead);
      } while (bytesRead > 0);
      target.Close();
      zipStream.Close();
      zip.Close();
    }

  }
}
