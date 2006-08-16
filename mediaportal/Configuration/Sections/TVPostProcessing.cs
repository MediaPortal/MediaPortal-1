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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.Utils.Services;

#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class TVPostProcessing : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox3;
    private MediaPortal.UserInterface.Controls.MPCheckBox ffdshowCheckBox;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPTextBox bottomscanlinesTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPTextBox topscanlinesTextBox;
    private System.ComponentModel.IContainer components = null;

    public TVPostProcessing()
      : this("Post Processing")
    {
    }

    public TVPostProcessing(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
    }

    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(base._config.Get(Config.Options.ConfigPath) + "MediaPortal.xml"))
      {
        ffdshowCheckBox.Checked = xmlreader.GetValueAsBool("mytv", "ffdshow", false);
        topscanlinesTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("mytv", "topscanlinestoremove", 0));
        bottomscanlinesTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("mytv", "bottomscanlinestoremove", 0));
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(base._config.Get(Config.Options.ConfigPath) + "MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("mytv", "ffdshow", ffdshowCheckBox.Checked);
        xmlwriter.SetValue("mytv", "topscanlinestoremove", topscanlinesTextBox.Text);
        xmlwriter.SetValue("mytv", "bottomscanlinestoremove", bottomscanlinesTextBox.Text);
      }
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
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.ffdshowCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.topscanlinesTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.bottomscanlinesTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpGroupBox3.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox3.Controls.Add(this.ffdshowCheckBox);
      this.mpGroupBox3.Controls.Add(this.label3);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(0, 0);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(472, 96);
      this.mpGroupBox3.TabIndex = 0;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Settings";
      // 
      // ffdshowCheckBox
      // 
      this.ffdshowCheckBox.AutoSize = true;
      this.ffdshowCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ffdshowCheckBox.Location = new System.Drawing.Point(16, 64);
      this.ffdshowCheckBox.Name = "ffdshowCheckBox";
      this.ffdshowCheckBox.Size = new System.Drawing.Size(182, 17);
      this.ffdshowCheckBox.TabIndex = 1;
      this.ffdshowCheckBox.Text = "Enable FFDshow post processing";
      this.ffdshowCheckBox.UseVisualStyleBackColor = true;
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.Location = new System.Drawing.Point(16, 24);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(440, 32);
      this.label3.TabIndex = 0;
      this.label3.Text = "Note that you need to install ffdshow separately to make any this option work. Pl" +
          "ease read the MediaPortal manual for more information.";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.bottomscanlinesTextBox);
      this.mpGroupBox1.Controls.Add(this.mpLabel2);
      this.mpGroupBox1.Controls.Add(this.label1);
      this.mpGroupBox1.Controls.Add(this.topscanlinesTextBox);
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 104);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(472, 112);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Croping";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 48);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(123, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "Top scanlines to remove";
      // 
      // mpLabel1
      // 
      this.mpLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpLabel1.Location = new System.Drawing.Point(16, 24);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(440, 24);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "MediaPortal can crop the picture for you if you need to remove unwanted video.";
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(16, 80);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(137, 13);
      this.mpLabel2.TabIndex = 4;
      this.mpLabel2.Text = "Bottom scanlines to remove";
      // 
      // topscanlinesTextBox
      // 
      this.topscanlinesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.topscanlinesTextBox.BorderColor = System.Drawing.Color.Empty;
      this.topscanlinesTextBox.Location = new System.Drawing.Point(184, 48);
      this.topscanlinesTextBox.MaxLength = 3;
      this.topscanlinesTextBox.Name = "topscanlinesTextBox";
      this.topscanlinesTextBox.Size = new System.Drawing.Size(26, 20);
      this.topscanlinesTextBox.TabIndex = 2;
      this.topscanlinesTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // bottomscanlinesTextBox
      // 
      this.bottomscanlinesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.bottomscanlinesTextBox.BorderColor = System.Drawing.Color.Empty;
      this.bottomscanlinesTextBox.Location = new System.Drawing.Point(184, 80);
      this.bottomscanlinesTextBox.MaxLength = 3;
      this.bottomscanlinesTextBox.Name = "bottomscanlinesTextBox";
      this.bottomscanlinesTextBox.Size = new System.Drawing.Size(26, 20);
      this.bottomscanlinesTextBox.TabIndex = 5;
      this.bottomscanlinesTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // TVPostProcessing
      // 
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.mpGroupBox3);
      this.Name = "TVPostProcessing";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion


  }
}

