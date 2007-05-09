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
    private IContainer components = null;
    private MediaPortal.UserInterface.Controls.MPTextBox txtTitle;
    private MediaPortal.UserInterface.Controls.MPButton btnTitleReset;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private MediaPortal.UserInterface.Controls.MPButton btnSlideSpeedReset;
    private MediaPortal.UserInterface.Controls.MPNumericUpDown numSlideSpeed;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;

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
      this.txtTitle = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.btnTitleReset = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.btnSlideSpeedReset = new MediaPortal.UserInterface.Controls.MPButton();
      this.numSlideSpeed = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numSlideSpeed)).BeginInit();
      this.SuspendLayout();
      // 
      // txtTitle
      // 
      this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtTitle.BorderColor = System.Drawing.Color.Empty;
      this.txtTitle.Location = new System.Drawing.Point(6, 20);
      this.txtTitle.Name = "txtTitle";
      this.txtTitle.Size = new System.Drawing.Size(246, 21);
      this.txtTitle.TabIndex = 0;
      this.toolTip.SetToolTip(this.txtTitle, "This text will appear in the MP home screen.");
      // 
      // btnTitleReset
      // 
      this.btnTitleReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnTitleReset.Location = new System.Drawing.Point(258, 20);
      this.btnTitleReset.Name = "btnTitleReset";
      this.btnTitleReset.Size = new System.Drawing.Size(72, 22);
      this.btnTitleReset.TabIndex = 1;
      this.btnTitleReset.Text = "Reset";
      this.toolTip.SetToolTip(this.btnTitleReset, "Reset to the default localized plugin title");
      this.btnTitleReset.UseVisualStyleBackColor = true;
      this.btnTitleReset.Click += new System.EventHandler(this.btnTitleReset_Click);
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.txtTitle);
      this.mpGroupBox1.Controls.Add(this.btnTitleReset);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 0);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(336, 54);
      this.mpGroupBox1.TabIndex = 0;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Plugin Title";
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox2.Controls.Add(this.btnSlideSpeedReset);
      this.mpGroupBox2.Controls.Add(this.numSlideSpeed);
      this.mpGroupBox2.Controls.Add(this.mpLabel1);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(0, 60);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(336, 56);
      this.mpGroupBox2.TabIndex = 1;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Slideshow";
      // 
      // btnSlideSpeedReset
      // 
      this.btnSlideSpeedReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSlideSpeedReset.Location = new System.Drawing.Point(258, 20);
      this.btnSlideSpeedReset.Name = "btnSlideSpeedReset";
      this.btnSlideSpeedReset.Size = new System.Drawing.Size(72, 22);
      this.btnSlideSpeedReset.TabIndex = 2;
      this.btnSlideSpeedReset.Text = "Reset";
      this.toolTip.SetToolTip(this.btnSlideSpeedReset, "Reset to the default localized plugin title");
      this.btnSlideSpeedReset.UseVisualStyleBackColor = true;
      this.btnSlideSpeedReset.Click += new System.EventHandler(this.btnSlideSpeedReset_Click);
      // 
      // numSlideSpeed
      // 
      this.numSlideSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.numSlideSpeed.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.numSlideSpeed.Location = new System.Drawing.Point(154, 20);
      this.numSlideSpeed.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      this.numSlideSpeed.Minimum = new decimal(new int[] {
            200,
            0,
            0,
            0});
      this.numSlideSpeed.Name = "numSlideSpeed";
      this.numSlideSpeed.Size = new System.Drawing.Size(98, 21);
      this.numSlideSpeed.TabIndex = 1;
      this.numSlideSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.numSlideSpeed.Value = new decimal(new int[] {
            3000,
            0,
            0,
            0});
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(6, 25);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(142, 13);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "Time between slides (in ms):";
      // 
      // AppSettingsRoot
      // 
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "AppSettingsRoot";
      this.Size = new System.Drawing.Size(336, 248);
      this.Load += new System.EventHandler(this.AppSettingsRoot_Load);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numSlideSpeed)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion



    public override bool AppObj2Form(AppItem curApp)
    {
      base.AppObj2Form(curApp);
      txtTitle.Text = ProgramSettings.ReadSetting(ProgramUtils.cPLUGINTITLE);
      if (txtTitle.Text == "")
      {
        txtTitle.Text = GUILocalizeStrings.Get(0);
      }

      string _slideSpeed = ProgramSettings.ReadSetting(ProgramUtils.cSLIDESPEED);
      if ((_slideSpeed != "") && (_slideSpeed != null))
      {
        numSlideSpeed.Value = int.Parse(_slideSpeed);
      }
      else
      {
        numSlideSpeed.Value = 3000;
      }
      return true;
    }


    public override void Form2AppObj(AppItem curApp)
    {
      // curApp is null!
      if (Loaded)
      {
        if ((txtTitle.Text != "") && (txtTitle.Text != GUILocalizeStrings.Get(0)))
        {
          ProgramSettings.WriteSetting(ProgramUtils.cPLUGINTITLE, txtTitle.Text);
        }
        else
        {
          ProgramSettings.DeleteSetting(ProgramUtils.cPLUGINTITLE);
        }

        ProgramSettings.WriteSetting(ProgramUtils.cSLIDESPEED, numSlideSpeed.Value.ToString());
      }
    }

    private void AppSettingsRoot_Load(object sender, EventArgs e)
    {
      AppObj2Form(null);
      Loaded = true;
    }

    private void btnTitleReset_Click(object sender, EventArgs e)
    {
      txtTitle.Text = GUILocalizeStrings.Get(0);
    }

    private void btnSlideSpeedReset_Click(object sender, EventArgs e)
    {
      numSlideSpeed.Value = 3000;
    }


  }
}
