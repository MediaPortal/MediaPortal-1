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
    partial class SetupForm
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
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.groupBoxMusicVideoSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.countryList = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.DeleteBtn = new System.Windows.Forms.Button();
            this.AddBtn = new System.Windows.Forms.Button();
            this.EditBtn = new System.Windows.Forms.Button();
            this.FavoriteList = new System.Windows.Forms.ListView();
            this.groupBoxBitRateSetting = new System.Windows.Forms.GroupBox();
            this.bitrate768 = new System.Windows.Forms.RadioButton();
            this.bitrate300 = new System.Windows.Forms.RadioButton();
            this.bitrate128 = new System.Windows.Forms.RadioButton();
            this.bitrate56 = new System.Windows.Forms.RadioButton();
            this.DoneBtn = new System.Windows.Forms.Button();
            this.VMR9CheckBox = new System.Windows.Forms.CheckBox();
            this.groupBoxMusicVideoSettings.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBoxBitRateSetting.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxMusicVideoSettings
            // 
            this.groupBoxMusicVideoSettings.Controls.Add(this.VMR9CheckBox);
            this.groupBoxMusicVideoSettings.Controls.Add(this.countryList);
            this.groupBoxMusicVideoSettings.Controls.Add(this.label1);
            this.groupBoxMusicVideoSettings.Controls.Add(this.groupBox2);
            this.groupBoxMusicVideoSettings.Controls.Add(this.groupBoxBitRateSetting);
            this.groupBoxMusicVideoSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.groupBoxMusicVideoSettings.Location = new System.Drawing.Point(12, 12);
            this.groupBoxMusicVideoSettings.Name = "groupBoxMusicVideoSettings";
            this.groupBoxMusicVideoSettings.Size = new System.Drawing.Size(370, 247);
            this.groupBoxMusicVideoSettings.TabIndex = 6;
            this.groupBoxMusicVideoSettings.TabStop = false;
            this.groupBoxMusicVideoSettings.Text = "Settings";
            // 
            // countryList
            // 
            this.countryList.FormattingEnabled = true;
            this.countryList.Location = new System.Drawing.Point(254, 32);
            this.countryList.Name = "countryList";
            this.countryList.Size = new System.Drawing.Size(98, 21);
            this.countryList.TabIndex = 11;
            this.countryList.SelectedIndexChanged += new System.EventHandler(this.countryList_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(251, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Default country";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.DeleteBtn);
            this.groupBox2.Controls.Add(this.AddBtn);
            this.groupBox2.Controls.Add(this.EditBtn);
            this.groupBox2.Controls.Add(this.FavoriteList);
            this.groupBox2.Location = new System.Drawing.Point(15, 29);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(205, 208);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "User favorite files";
            // 
            // DeleteBtn
            // 
            this.DeleteBtn.Location = new System.Drawing.Point(143, 159);
            this.DeleteBtn.Name = "DeleteBtn";
            this.DeleteBtn.Size = new System.Drawing.Size(50, 25);
            this.DeleteBtn.TabIndex = 7;
            this.DeleteBtn.Text = "Delete";
            this.DeleteBtn.UseVisualStyleBackColor = true;
            this.DeleteBtn.Click += new System.EventHandler(this.DeleteBtn_Click);
            // 
            // AddBtn
            // 
            this.AddBtn.Location = new System.Drawing.Point(13, 159);
            this.AddBtn.Name = "AddBtn";
            this.AddBtn.Size = new System.Drawing.Size(50, 25);
            this.AddBtn.TabIndex = 6;
            this.AddBtn.Text = "Add";
            this.AddBtn.UseVisualStyleBackColor = true;
            this.AddBtn.Click += new System.EventHandler(this.AddBtn_Click);
            // 
            // EditBtn
            // 
            this.EditBtn.Location = new System.Drawing.Point(78, 159);
            this.EditBtn.Name = "EditBtn";
            this.EditBtn.Size = new System.Drawing.Size(50, 25);
            this.EditBtn.TabIndex = 5;
            this.EditBtn.Text = "Edit";
            this.EditBtn.UseVisualStyleBackColor = true;
            this.EditBtn.Click += new System.EventHandler(this.EditBtn_Click);
            // 
            // FavoriteList
            // 
            this.FavoriteList.LabelWrap = false;
            this.FavoriteList.Location = new System.Drawing.Point(13, 22);
            this.FavoriteList.MultiSelect = false;
            this.FavoriteList.Name = "FavoriteList";
            this.FavoriteList.Size = new System.Drawing.Size(180, 131);
            this.FavoriteList.TabIndex = 3;
            this.FavoriteList.UseCompatibleStateImageBehavior = false;
            // 
            // groupBoxBitRateSetting
            // 
            this.groupBoxBitRateSetting.Controls.Add(this.bitrate768);
            this.groupBoxBitRateSetting.Controls.Add(this.bitrate300);
            this.groupBoxBitRateSetting.Controls.Add(this.bitrate128);
            this.groupBoxBitRateSetting.Controls.Add(this.bitrate56);
            this.groupBoxBitRateSetting.Location = new System.Drawing.Point(254, 103);
            this.groupBoxBitRateSetting.Name = "groupBoxBitRateSetting";
            this.groupBoxBitRateSetting.Size = new System.Drawing.Size(98, 134);
            this.groupBoxBitRateSetting.TabIndex = 7;
            this.groupBoxBitRateSetting.TabStop = false;
            this.groupBoxBitRateSetting.Text = "Preferred bitrate";
            // 
            // bitrate768
            // 
            this.bitrate768.AutoSize = true;
            this.bitrate768.Location = new System.Drawing.Point(28, 99);
            this.bitrate768.Name = "bitrate768";
            this.bitrate768.Size = new System.Drawing.Size(43, 17);
            this.bitrate768.TabIndex = 3;
            this.bitrate768.TabStop = true;
            this.bitrate768.Text = "768";
            this.bitrate768.UseVisualStyleBackColor = true;
            // 
            // bitrate300
            // 
            this.bitrate300.AutoSize = true;
            this.bitrate300.Location = new System.Drawing.Point(28, 74);
            this.bitrate300.Name = "bitrate300";
            this.bitrate300.Size = new System.Drawing.Size(43, 17);
            this.bitrate300.TabIndex = 2;
            this.bitrate300.TabStop = true;
            this.bitrate300.Text = "300";
            this.bitrate300.UseVisualStyleBackColor = true;
            // 
            // bitrate128
            // 
            this.bitrate128.AutoSize = true;
            this.bitrate128.Location = new System.Drawing.Point(28, 49);
            this.bitrate128.Name = "bitrate128";
            this.bitrate128.Size = new System.Drawing.Size(43, 17);
            this.bitrate128.TabIndex = 1;
            this.bitrate128.TabStop = true;
            this.bitrate128.Text = "128";
            this.bitrate128.UseVisualStyleBackColor = true;
            // 
            // bitrate56
            // 
            this.bitrate56.AutoSize = true;
            this.bitrate56.Location = new System.Drawing.Point(28, 24);
            this.bitrate56.Name = "bitrate56";
            this.bitrate56.Size = new System.Drawing.Size(37, 17);
            this.bitrate56.TabIndex = 0;
            this.bitrate56.TabStop = true;
            this.bitrate56.Text = "56";
            this.bitrate56.UseVisualStyleBackColor = true;
            // 
            // DoneBtn
            // 
            this.DoneBtn.Location = new System.Drawing.Point(307, 265);
            this.DoneBtn.Name = "DoneBtn";
            this.DoneBtn.Size = new System.Drawing.Size(75, 23);
            this.DoneBtn.TabIndex = 10;
            this.DoneBtn.Text = "Done";
            this.DoneBtn.UseVisualStyleBackColor = true;
            this.DoneBtn.Click += new System.EventHandler(this.DoneBtn_Click);
            // 
            // VMR9CheckBox
            // 
            this.VMR9CheckBox.AutoSize = true;
            this.VMR9CheckBox.Location = new System.Drawing.Point(254, 69);
            this.VMR9CheckBox.Name = "VMR9CheckBox";
            this.VMR9CheckBox.Size = new System.Drawing.Size(78, 17);
            this.VMR9CheckBox.TabIndex = 12;
            this.VMR9CheckBox.Text = "Use VMR9";
            this.VMR9CheckBox.UseVisualStyleBackColor = true;
            // 
            // SetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 295);
            this.Controls.Add(this.DoneBtn);
            this.Controls.Add(this.groupBoxMusicVideoSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SetupForm";
            this.Text = "Music video plugin settings";
            this.Load += new System.EventHandler(this.SetupForm_Load);
            this.groupBoxMusicVideoSettings.ResumeLayout(false);
            this.groupBoxMusicVideoSettings.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBoxBitRateSetting.ResumeLayout(false);
            this.groupBoxBitRateSetting.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker backgroundWorker1;
      private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxMusicVideoSettings;
      private System.Windows.Forms.ComboBox countryList;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.GroupBox groupBox2;
      private System.Windows.Forms.Button DeleteBtn;
      private System.Windows.Forms.Button AddBtn;
      private System.Windows.Forms.Button EditBtn;
      private System.Windows.Forms.ListView FavoriteList;
      private System.Windows.Forms.GroupBox groupBoxBitRateSetting;
      private System.Windows.Forms.RadioButton bitrate768;
      private System.Windows.Forms.RadioButton bitrate300;
      private System.Windows.Forms.RadioButton bitrate128;
      private System.Windows.Forms.RadioButton bitrate56;
        private System.Windows.Forms.Button DoneBtn;
        private System.Windows.Forms.CheckBox VMR9CheckBox;
    }
}