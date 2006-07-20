#region Copyright (C) 2006 Team MediaPortal

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

namespace MediaPortal.GUI.MusicVideos
{
  partial class SetupFavoriteForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBoxMusicVideoFavs = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelFavName = new System.Windows.Forms.Label();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.buttonOk = new System.Windows.Forms.Button();
      this.textBoxFavoriteName = new System.Windows.Forms.TextBox();
      this.groupBoxMusicVideoFavs.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxMusicVideoFavs
      // 
      this.groupBoxMusicVideoFavs.Controls.Add(this.labelFavName);
      this.groupBoxMusicVideoFavs.Controls.Add(this.buttonCancel);
      this.groupBoxMusicVideoFavs.Controls.Add(this.buttonOk);
      this.groupBoxMusicVideoFavs.Controls.Add(this.textBoxFavoriteName);
      this.groupBoxMusicVideoFavs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxMusicVideoFavs.Location = new System.Drawing.Point(12, 12);
      this.groupBoxMusicVideoFavs.Name = "groupBoxMusicVideoFavs";
      this.groupBoxMusicVideoFavs.Size = new System.Drawing.Size(190, 109);
      this.groupBoxMusicVideoFavs.TabIndex = 4;
      this.groupBoxMusicVideoFavs.TabStop = false;
      this.groupBoxMusicVideoFavs.Text = "Music video settings";
      // 
      // labelFavName
      // 
      this.labelFavName.AutoSize = true;
      this.labelFavName.Location = new System.Drawing.Point(6, 25);
      this.labelFavName.Name = "labelFavName";
      this.labelFavName.Size = new System.Drawing.Size(122, 13);
      this.labelFavName.TabIndex = 7;
      this.labelFavName.Text = "Favorite settings for user";
      // 
      // buttonCancel
      // 
      this.buttonCancel.Location = new System.Drawing.Point(107, 78);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 6;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // buttonOk
      // 
      this.buttonOk.Location = new System.Drawing.Point(9, 78);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(75, 23);
      this.buttonOk.TabIndex = 5;
      this.buttonOk.Text = "OK";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // textBoxFavoriteName
      // 
      this.textBoxFavoriteName.Location = new System.Drawing.Point(9, 45);
      this.textBoxFavoriteName.Name = "textBoxFavoriteName";
      this.textBoxFavoriteName.Size = new System.Drawing.Size(173, 20);
      this.textBoxFavoriteName.TabIndex = 4;
      // 
      // SetupFavoriteForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(213, 133);
      this.Controls.Add(this.groupBoxMusicVideoFavs);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "SetupFavoriteForm";
      this.Text = "Setup favorites";
      this.Load += new System.EventHandler(this.SetupFavoriteForm_Load);
      this.groupBoxMusicVideoFavs.ResumeLayout(false);
      this.groupBoxMusicVideoFavs.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxMusicVideoFavs;
    private System.Windows.Forms.Label labelFavName;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonOk;
    public System.Windows.Forms.TextBox textBoxFavoriteName;
    //public string msFavoriteName
    //{
    //    get { return msFavoriteName; }
    //    set
    //    {
    //        msFavoriteName = value;
    //        this.TxtFavoriteName.Text = value;
    //    }
    //}
  }
}