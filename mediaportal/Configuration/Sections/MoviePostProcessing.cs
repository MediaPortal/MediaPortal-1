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
  public class MoviePostProcessing : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox3;
    private MediaPortal.UserInterface.Controls.MPCheckBox ffdshowCheckBox;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private System.ComponentModel.IContainer components = null;

    public MoviePostProcessing()
      : this("Movie Post Processing")
    {
    }

    public MoviePostProcessing(string name)
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
        ffdshowCheckBox.Checked = xmlreader.GetValueAsBool("movieplayer", "ffdshow", false);
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(base._config.Get(Config.Options.ConfigPath) + "MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("movieplayer", "ffdshow", ffdshowCheckBox.Checked);
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
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.ffdshowCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox3.Controls.Add(this.label3);
      this.mpGroupBox3.Controls.Add(this.ffdshowCheckBox);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(0, 0);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(472, 96);
      this.mpGroupBox3.TabIndex = 0;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Settings";
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
      // ffdshowCheckBox
      // 
      this.ffdshowCheckBox.AutoSize = true;
      this.ffdshowCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ffdshowCheckBox.Location = new System.Drawing.Point(16, 64);
      this.ffdshowCheckBox.Name = "ffdshowCheckBox";
      this.ffdshowCheckBox.Size = new System.Drawing.Size(182, 17);
      this.ffdshowCheckBox.TabIndex = 1;
      this.ffdshowCheckBox.Text = "Enable FFDshow post processing";
      // 
      // MoviePostProcessing
      // 
      this.Controls.Add(this.mpGroupBox3);
      this.Name = "MoviePostProcessing";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion
  }
}

