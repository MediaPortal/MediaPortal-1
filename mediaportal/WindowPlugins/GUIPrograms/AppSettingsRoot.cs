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
using System.ComponentModel;
using System.Windows.Forms;
using Programs.Utils;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  public class AppSettingsRoot: AppSettings
  {
    private Label label3;
    private Label rootItemLabel;
    private TextBox PluginTitle;
    private Button ResetButton;
    private IContainer components = null;

    bool Loaded = false;

    public AppSettingsRoot()
    {
      // This call is required by the Windows Form Designer.
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

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.label3 = new System.Windows.Forms.Label();
      this.rootItemLabel = new System.Windows.Forms.Label();
      this.PluginTitle = new System.Windows.Forms.TextBox();
      this.ResetButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // label3
      // 
      this.label3.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label3.Location = new System.Drawing.Point(8, 8);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(216, 32);
      this.label3.TabIndex = 81;
      this.label3.Text = "my Programs root";
      // 
      // rootItemLabel
      // 
      this.rootItemLabel.Location = new System.Drawing.Point(8, 51);
      this.rootItemLabel.Name = "rootItemLabel";
      this.rootItemLabel.Size = new System.Drawing.Size(64, 16);
      this.rootItemLabel.TabIndex = 82;
      this.rootItemLabel.Text = "Plugin title";
      // 
      // PluginTitle
      // 
      this.PluginTitle.Location = new System.Drawing.Point(72, 48);
      this.PluginTitle.Name = "PluginTitle";
      this.PluginTitle.Size = new System.Drawing.Size(171, 20);
      this.PluginTitle.TabIndex = 85;
      this.PluginTitle.Text = "";
      this.toolTip.SetToolTip(this.PluginTitle, "This text will appear in the MP home screen.");
      // 
      // ResetButton
      // 
      this.ResetButton.Location = new System.Drawing.Point(249, 46);
      this.ResetButton.Name = "ResetButton";
      this.ResetButton.Size = new System.Drawing.Size(72, 24);
      this.ResetButton.TabIndex = 86;
      this.ResetButton.Text = "Reset";
      this.toolTip.SetToolTip(this.ResetButton, "Reset to the default localized plugin title");
      this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
      // 
      // AppSettingsRoot
      // 
      this.Controls.Add(this.ResetButton);
      this.Controls.Add(this.PluginTitle);
      this.Controls.Add(this.rootItemLabel);
      this.Controls.Add(this.label3);
      this.Name = "AppSettingsRoot";
      this.Size = new System.Drawing.Size(336, 248);
      this.Load += new System.EventHandler(this.AppSettingsRoot_Load);
      this.ResumeLayout(false);

    }
    #endregion 



    public override bool AppObj2Form(AppItem curApp)
    {
      base.AppObj2Form(curApp);
      PluginTitle.Text = ProgramSettings.ReadSetting(ProgramUtils.cPLUGINTITLE);
      if (PluginTitle.Text == "")
      {
        // doesn't work! PluginTitle.Text = GUILocalizeStrings.Get(0);
        PluginTitle.Text = "My Programs";
      }
      return true;
    }


    public override void Form2AppObj(AppItem curApp)
    {
      // curApp is null!
      if (Loaded)
      {
        if ((PluginTitle.Text != "") && (PluginTitle.Text != "My Programs"))
        {
          ProgramSettings.WriteSetting(ProgramUtils.cPLUGINTITLE, PluginTitle.Text);
        }
        else
        {
          ProgramSettings.DeleteSetting(ProgramUtils.cPLUGINTITLE);
        }
      }
    }

    private void ResetButton_Click(object sender, EventArgs e)
    {
      PluginTitle.Text = "My Programs";
    }

    private void AppSettingsRoot_Load(object sender, EventArgs e)
    {
      AppObj2Form(null);
      Loaded = true;
    }


  }
}
