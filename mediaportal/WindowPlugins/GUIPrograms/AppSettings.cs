/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.ComponentModel;
using System.Windows.Forms;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for AppSettings.
  /// </summary>
  public class AppSettings: UserControl
  {
    private IContainer components;
    protected OpenFileDialog dialogFile;
    protected FolderBrowserDialog dialogFolder;
    public ToolTip toolTip;
    protected ProgramConditionChecker m_Checker = new ProgramConditionChecker();

    public event EventHandler OnUpClick;
    public event EventHandler OnDownClick;

    string postLaunch = "";
    string preLaunch = "";


    public AppSettings()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call

    }

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

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.dialogFile = new System.Windows.Forms.OpenFileDialog();
      this.dialogFolder = new System.Windows.Forms.FolderBrowserDialog();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      // 
      // AppSettings
      // 
      this.Name = "AppSettings";

    }
    #endregion 

    public string PreLaunch
    {
      get { return preLaunch;}
      set { preLaunch = value;}
    }

    public string PostLaunch
    {
      get { return postLaunch;}
      set { postLaunch = value;}
    }

    public virtual bool AppObj2Form(AppItem curApp)
    {
      if (curApp != null)
      {
        preLaunch = curApp.PreLaunch;
        postLaunch = curApp.PostLaunch;
      }
      return true;
    }

    public virtual bool Applist2Form(Applist apps)
    {
      // virtual!
      return true;
    }


    public virtual void Form2AppObj(AppItem curApp)
    {
      if (curApp != null)
      {
        curApp.PreLaunch = preLaunch;
        curApp.PostLaunch = postLaunch;
      }
    }

    public virtual bool EntriesOK(AppItem curApp)
    {
      return true;
    }

    public virtual void LoadFromAppItem(AppItem tempApp)
    {
      // virtual!
    }

    protected void UpButtonClicked()
    {
      if (this.OnUpClick != null)
      {
        OnUpClick(this, null);
      }
    }

    protected void DownButtonClicked()
    {
      if (this.OnDownClick != null)
      {
        OnDownClick(this, null);
      }
    }

    protected void PrePostLaunchClick(string Title)
    {
      AppSettingsPrePost frmAppPrePost = new AppSettingsPrePost();
      frmAppPrePost.Title = Title;
      frmAppPrePost.PreLaunch = this.preLaunch;
      frmAppPrePost.PostLaunch = this.postLaunch;
      DialogResult dialogResult = frmAppPrePost.ShowDialog(this);
      if (dialogResult == DialogResult.OK)
      {
        this.preLaunch = frmAppPrePost.PreLaunch;
        this.postLaunch = frmAppPrePost.PostLaunch;
      }
    }

  }
}
