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

namespace MediaPortal.ControlDevices
{
  partial class ControlDeviceForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlDeviceForm));
      this.pictureBoxASLogo = new System.Windows.Forms.PictureBox();
      this.buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.tabPage6 = new System.Windows.Forms.TabPage();
      this.tabPage5 = new System.Windows.Forms.TabPage();
      this.tabPage4 = new System.Windows.Forms.TabPage();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.interCommandDelayNumUpDn = new System.Windows.Forms.NumericUpDown();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.commandRepeatNumUpDn = new System.Windows.Forms.NumericUpDown();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpCheckBox3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBox2 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBox1 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabControlSettings = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage3 = new System.Windows.Forms.TabPage();
      this.panel2 = new System.Windows.Forms.Panel();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxASLogo)).BeginInit();
      this.tabPage2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.interCommandDelayNumUpDn)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.commandRepeatNumUpDn)).BeginInit();
      this.tabPage1.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.tabControlSettings.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.SuspendLayout();
      // 
      // pictureBoxASLogo
      // 
      this.pictureBoxASLogo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBoxASLogo.ErrorImage = null;
      this.pictureBoxASLogo.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxASLogo.Image")));
      this.pictureBoxASLogo.InitialImage = null;
      this.pictureBoxASLogo.Location = new System.Drawing.Point(0, 0);
      this.pictureBoxASLogo.Name = "pictureBoxASLogo";
      this.pictureBoxASLogo.Size = new System.Drawing.Size(595, 439);
      this.pictureBoxASLogo.TabIndex = 2;
      this.pictureBoxASLogo.TabStop = false;
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.Location = new System.Drawing.Point(504, 404);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 1;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      // 
      // buttonOK
      // 
      this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOK.Location = new System.Drawing.Point(423, 404);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(75, 23);
      this.buttonOK.TabIndex = 0;
      this.buttonOK.Text = "Save";
      this.buttonOK.UseVisualStyleBackColor = true;
      // 
      // tabPage6
      // 
      this.tabPage6.Location = new System.Drawing.Point(4, 22);
      this.tabPage6.Name = "tabPage6";
      this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage6.Size = new System.Drawing.Size(562, 310);
      this.tabPage6.TabIndex = 5;
      this.tabPage6.Text = "Mapping";
      this.tabPage6.UseVisualStyleBackColor = true;
      // 
      // tabPage5
      // 
      this.tabPage5.Location = new System.Drawing.Point(4, 22);
      this.tabPage5.Name = "tabPage5";
      this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage5.Size = new System.Drawing.Size(562, 310);
      this.tabPage5.TabIndex = 4;
      this.tabPage5.Text = "External";
      this.tabPage5.UseVisualStyleBackColor = true;
      // 
      // tabPage4
      // 
      this.tabPage4.Location = new System.Drawing.Point(4, 22);
      this.tabPage4.Name = "tabPage4";
      this.tabPage4.Size = new System.Drawing.Size(562, 310);
      this.tabPage4.TabIndex = 2;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.interCommandDelayNumUpDn);
      this.tabPage2.Controls.Add(this.label6);
      this.tabPage2.Controls.Add(this.commandRepeatNumUpDn);
      this.tabPage2.Controls.Add(this.label5);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(562, 310);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "USB-UIRT";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // interCommandDelayNumUpDn
      // 
      this.interCommandDelayNumUpDn.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
      this.interCommandDelayNumUpDn.Location = new System.Drawing.Point(150, 39);
      this.interCommandDelayNumUpDn.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.interCommandDelayNumUpDn.Name = "interCommandDelayNumUpDn";
      this.interCommandDelayNumUpDn.Size = new System.Drawing.Size(48, 20);
      this.interCommandDelayNumUpDn.TabIndex = 16;
      this.interCommandDelayNumUpDn.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.interCommandDelayNumUpDn.ValueChanged += new System.EventHandler(this.interCommandDelayNumUpDn_ValueChanged);
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(16, 41);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(121, 13);
      this.label6.TabIndex = 15;
      this.label6.Text = "Inter-command delay ms";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.label6.Click += new System.EventHandler(this.label6_Click);
      // 
      // commandRepeatNumUpDn
      // 
      this.commandRepeatNumUpDn.Location = new System.Drawing.Point(150, 15);
      this.commandRepeatNumUpDn.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.commandRepeatNumUpDn.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.commandRepeatNumUpDn.Name = "commandRepeatNumUpDn";
      this.commandRepeatNumUpDn.Size = new System.Drawing.Size(48, 20);
      this.commandRepeatNumUpDn.TabIndex = 14;
      this.commandRepeatNumUpDn.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.commandRepeatNumUpDn.ValueChanged += new System.EventHandler(this.commandRepeatNumUpDn_ValueChanged);
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(16, 17);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(117, 13);
      this.label5.TabIndex = 13;
      this.label5.Text = "Command repeat count";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.label5.Click += new System.EventHandler(this.label5_Click);
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpGroupBox2);
      this.tabPage1.Controls.Add(this.mpLabel1);
      this.tabPage1.Controls.Add(this.mpGroupBox1);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(562, 310);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Plugin Settings";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(6, 108);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(245, 196);
      this.mpGroupBox2.TabIndex = 2;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Status";
      // 
      // mpLabel1
      // 
      this.mpLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpLabel1.Location = new System.Drawing.Point(257, 7);
      this.mpLabel1.Margin = new System.Windows.Forms.Padding(3);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Padding = new System.Windows.Forms.Padding(5);
      this.mpLabel1.Size = new System.Drawing.Size(299, 297);
      this.mpLabel1.TabIndex = 1;
      this.mpLabel1.Text = resources.GetString("mpLabel1.Text");
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.mpCheckBox3);
      this.mpGroupBox1.Controls.Add(this.mpCheckBox2);
      this.mpGroupBox1.Controls.Add(this.mpCheckBox1);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(6, 6);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(245, 96);
      this.mpGroupBox1.TabIndex = 0;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Basic Settings";
      // 
      // mpCheckBox3
      // 
      this.mpCheckBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.mpCheckBox3.AutoSize = true;
      this.mpCheckBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBox3.Location = new System.Drawing.Point(6, 65);
      this.mpCheckBox3.Name = "mpCheckBox3";
      this.mpCheckBox3.Size = new System.Drawing.Size(107, 17);
      this.mpCheckBox3.TabIndex = 2;
      this.mpCheckBox3.Text = "Extensive logging";
      this.mpCheckBox3.UseVisualStyleBackColor = true;
      // 
      // mpCheckBox2
      // 
      this.mpCheckBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.mpCheckBox2.AutoSize = true;
      this.mpCheckBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBox2.Location = new System.Drawing.Point(6, 42);
      this.mpCheckBox2.Name = "mpCheckBox2";
      this.mpCheckBox2.Size = new System.Drawing.Size(143, 17);
      this.mpCheckBox2.TabIndex = 1;
      this.mpCheckBox2.Text = "Enable as Output Device";
      this.mpCheckBox2.UseVisualStyleBackColor = true;
      this.mpCheckBox2.CheckedChanged += new System.EventHandler(this.mpCheckBox2_CheckedChanged);
      // 
      // mpCheckBox1
      // 
      this.mpCheckBox1.AutoSize = true;
      this.mpCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBox1.Location = new System.Drawing.Point(6, 19);
      this.mpCheckBox1.Name = "mpCheckBox1";
      this.mpCheckBox1.Size = new System.Drawing.Size(135, 17);
      this.mpCheckBox1.TabIndex = 0;
      this.mpCheckBox1.Text = "Enable as Input Device";
      this.mpCheckBox1.UseVisualStyleBackColor = true;
      // 
      // tabControlSettings
      // 
      this.tabControlSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControlSettings.Controls.Add(this.tabPage1);
      this.tabControlSettings.Controls.Add(this.tabPage2);
      this.tabControlSettings.Controls.Add(this.tabPage4);
      this.tabControlSettings.Controls.Add(this.tabPage5);
      this.tabControlSettings.Controls.Add(this.tabPage6);
      this.tabControlSettings.Controls.Add(this.tabPage3);
      this.tabControlSettings.Location = new System.Drawing.Point(13, 62);
      this.tabControlSettings.Name = "tabControlSettings";
      this.tabControlSettings.SelectedIndex = 0;
      this.tabControlSettings.Size = new System.Drawing.Size(570, 336);
      this.tabControlSettings.TabIndex = 3;
      // 
      // tabPage3
      // 
      this.tabPage3.Controls.Add(this.panel2);
      this.tabPage3.Location = new System.Drawing.Point(4, 22);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage3.Size = new System.Drawing.Size(562, 310);
      this.tabPage3.TabIndex = 6;
      this.tabPage3.Text = "tabPage3";
      this.tabPage3.UseVisualStyleBackColor = true;
      // 
      // panel2
      // 
      this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.panel2.Location = new System.Drawing.Point(6, 6);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(550, 298);
      this.panel2.TabIndex = 0;
      // 
      // ControlDeviceForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(595, 439);
      this.Controls.Add(this.tabControlSettings);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.pictureBoxASLogo);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "ControlDeviceForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "USB-UIRT Settings";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxASLogo)).EndInit();
      this.tabPage2.ResumeLayout(false);
      this.tabPage2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.interCommandDelayNumUpDn)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.commandRepeatNumUpDn)).EndInit();
      this.tabPage1.ResumeLayout(false);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.tabControlSettings.ResumeLayout(false);
      this.tabPage3.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPButton buttonOK;
    private MediaPortal.UserInterface.Controls.MPButton buttonCancel;
    private System.Windows.Forms.PictureBox pictureBoxASLogo;
    private System.Windows.Forms.TabPage tabPage6;
    private System.Windows.Forms.TabPage tabPage5;
    private System.Windows.Forms.TabPage tabPage4;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.TabPage tabPage1;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBox3;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBox2;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBox1;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControlSettings;
    private System.Windows.Forms.NumericUpDown interCommandDelayNumUpDn;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private System.Windows.Forms.NumericUpDown commandRepeatNumUpDn;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private System.Windows.Forms.TabPage tabPage3;
    private System.Windows.Forms.Panel panel2;
  }
}