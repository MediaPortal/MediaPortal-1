#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Windows.Forms;
using AxSHDocVw;

namespace Mpe.Designers
{
  /// <summary>
  /// Summary description for MpeHelpBrowser.
  /// </summary>
  public class MpeHelpBrowser : UserControl, MpeDesigner
  {
    #region Variables

    private Container components = null;
    private MediaPortalEditor mpe;
    private AxWebBrowser browser;

    #endregion

    #region Contructors

    public MpeHelpBrowser(MediaPortalEditor mpe)
    {
      this.mpe = mpe;
      InitializeComponent();
    }

    #endregion

    #region Methods

    public void ShowHelp(FileInfo file)
    {
      if (file == null || file.Exists == false)
      {
        throw new DesignerException("Invalid help file");
      }
      try
      {
        browser.Navigate(FileToUrl(file));
      }
      catch (Exception e)
      {
        MpeLog.Error(e);
        throw new DesignerException(e.Message);
      }
    }

    public void Initialize()
    {
      //
    }

    public void Save()
    {
      //
    }

    public void Cancel()
    {
      //
    }

    public void Destroy()
    {
      browser.Dispose();
    }

    public void Pause()
    {
      mpe.PropertyManager.SelectedResource = null;
      mpe.PropertyManager.HideResourceList();
    }

    public void Resume()
    {
      mpe.PropertyManager.SelectedResource = null;
      mpe.PropertyManager.HideResourceList();
    }

    private string FileToUrl(FileInfo file)
    {
      if (file == null)
      {
        throw new Exception("File cannot be null!");
      }
      return "file:///" + file.FullName.Replace("\\", "/");
    }

    private string FileToUrl(DirectoryInfo file)
    {
      if (file == null)
      {
        throw new Exception("File cannot be null!");
      }
      return "file:///" + file.FullName.Replace("\\", "/");
    }

    private string FileToUrl(string path)
    {
      return "file:///" + path.Replace("\\", "/");
    }

    #endregion

    #region Properties

    public string ResourceName
    {
      get { return "Help"; }
    }

    public bool AllowAdditions
    {
      get { return false; }
    }

    public bool AllowDeletions
    {
      get { return false; }
    }

    #endregion	

    #region Component Designer Generated Code

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      ResourceManager resources = new ResourceManager(typeof(MpeHelpBrowser));
      browser = new AxWebBrowser();
      ((ISupportInitialize) (browser)).BeginInit();
      SuspendLayout();
      // 
      // browser
      // 
      browser.Dock = DockStyle.Fill;
      browser.Enabled = true;
      browser.Location = new Point(0, 0);
      browser.OcxState = ((AxHost.State) (resources.GetObject("browser.OcxState")));
      browser.Size = new Size(368, 224);
      browser.TabIndex = 0;
      // 
      // MpeHelpBrowser
      // 
      Controls.Add(browser);
      Name = "MpeHelpBrowser";
      Size = new Size(368, 224);
      ((ISupportInitialize) (browser)).EndInit();
      ResumeLayout(false);
    }

    #endregion
  }
}