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
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using Programs.Utils;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  public class AppSettingsRoot : AppSettings
  {
    private MediaPortal.UserInterface.Controls.MPTextBox PluginTitle;
    private MediaPortal.UserInterface.Controls.MPButton ResetButton;
    private IContainer components = null;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;

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
      this.PluginTitle = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.ResetButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // PluginTitle
      // 
      this.PluginTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.PluginTitle.BorderColor = System.Drawing.Color.Empty;
      this.PluginTitle.Location = new System.Drawing.Point(6, 20);
      this.PluginTitle.Name = "PluginTitle";
      this.PluginTitle.Size = new System.Drawing.Size(243, 21);
      this.PluginTitle.TabIndex = 85;
      this.toolTip.SetToolTip(this.PluginTitle, "This text will appear in the MP home screen.");
      // 
      // ResetButton
      // 
      this.ResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.ResetButton.Location = new System.Drawing.Point(255, 20);
      this.ResetButton.Name = "ResetButton";
      this.ResetButton.Size = new System.Drawing.Size(72, 22);
      this.ResetButton.TabIndex = 86;
      this.ResetButton.Text = "Reset";
      this.toolTip.SetToolTip(this.ResetButton, "Reset to the default localized plugin title");
      this.ResetButton.UseVisualStyleBackColor = true;
      this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.PluginTitle);
      this.mpGroupBox1.Controls.Add(this.ResetButton);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(3, 3);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(333, 54);
      this.mpGroupBox1.TabIndex = 87;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Plugin Title";
      // 
      // AppSettingsRoot
      // 
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "AppSettingsRoot";
      this.Size = new System.Drawing.Size(336, 248);
      this.Load += new System.EventHandler(this.AppSettingsRoot_Load);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion



    public override bool AppObj2Form(AppItem curApp)
    {
      base.AppObj2Form(curApp);
      PluginTitle.Text = ProgramSettings.ReadSetting(ProgramUtils.cPLUGINTITLE);
      if (PluginTitle.Text == "")
      {
        PluginTitle.Text = GUILocalizeStrings.Get(0);
      }
      return true;
    }


    public override void Form2AppObj(AppItem curApp)
    {
      // curApp is null!
      if (Loaded)
      {
        if ((PluginTitle.Text != "") && (PluginTitle.Text != GUILocalizeStrings.Get(0)))
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
      PluginTitle.Text = GUILocalizeStrings.Get(0);
    }

    private void AppSettingsRoot_Load(object sender, EventArgs e)
    {
      AppObj2Form(null);
      Loaded = true;
    }


  }
}
