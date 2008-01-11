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

namespace MediaPortal.Configuration.Sections
{
  partial class MusicDSP
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private new System.ComponentModel.IContainer components = null;

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.DSPTabPg = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBoxGain = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBoxDynamicAmplification = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.buttonSetGain = new MediaPortal.UserInterface.Controls.MPButton();
      this.label17 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxGainDBValue = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.checkBoxDAmp = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label23 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.trackBarGain = new System.Windows.Forms.TrackBar();
      this.label14 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxCompressor = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label11 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label22 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelCompThreshold = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label13 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxCompressor = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.trackBarCompressor = new System.Windows.Forms.TrackBar();
      this.MusicDSPTabCtl = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.PluginTabPg = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.listBoxFoundPlugins = new System.Windows.Forms.ListBox();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.listBoxSelectedPlugins = new System.Windows.Forms.ListBox();
      this.btnConfig = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonPluginRemove = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonPluginAdd = new MediaPortal.UserInterface.Controls.MPButton();
      this.btFileselect = new MediaPortal.UserInterface.Controls.MPButton();
      this.textBoxMusicFile = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btStop = new MediaPortal.UserInterface.Controls.MPButton();
      this.btPlay = new MediaPortal.UserInterface.Controls.MPButton();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.DSPTabPg.SuspendLayout();
      this.groupBoxGain.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarGain)).BeginInit();
      this.groupBoxCompressor.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarCompressor)).BeginInit();
      this.MusicDSPTabCtl.SuspendLayout();
      this.PluginTabPg.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // DSPTabPg
      // 
      this.DSPTabPg.BackColor = System.Drawing.SystemColors.Control;
      this.DSPTabPg.Controls.Add(this.groupBoxGain);
      this.DSPTabPg.Controls.Add(this.groupBoxCompressor);
      this.DSPTabPg.Location = new System.Drawing.Point(4, 22);
      this.DSPTabPg.Name = "DSPTabPg";
      this.DSPTabPg.Padding = new System.Windows.Forms.Padding(3);
      this.DSPTabPg.Size = new System.Drawing.Size(464, 315);
      this.DSPTabPg.TabIndex = 1;
      this.DSPTabPg.Text = "BASS DSP / FX";
      // 
      // groupBoxGain
      // 
      this.groupBoxGain.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.groupBoxGain.Controls.Add(this.comboBoxDynamicAmplification);
      this.groupBoxGain.Controls.Add(this.buttonSetGain);
      this.groupBoxGain.Controls.Add(this.label17);
      this.groupBoxGain.Controls.Add(this.textBoxGainDBValue);
      this.groupBoxGain.Controls.Add(this.checkBoxDAmp);
      this.groupBoxGain.Controls.Add(this.label23);
      this.groupBoxGain.Controls.Add(this.trackBarGain);
      this.groupBoxGain.Controls.Add(this.label14);
      this.groupBoxGain.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGain.Location = new System.Drawing.Point(61, 17);
      this.groupBoxGain.Name = "groupBoxGain";
      this.groupBoxGain.Size = new System.Drawing.Size(151, 271);
      this.groupBoxGain.TabIndex = 71;
      this.groupBoxGain.TabStop = false;
      this.groupBoxGain.Text = "Gain";
      // 
      // comboBoxDynamicAmplification
      // 
      this.comboBoxDynamicAmplification.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxDynamicAmplification.Enabled = false;
      this.comboBoxDynamicAmplification.FormattingEnabled = true;
      this.comboBoxDynamicAmplification.Items.AddRange(new object[] {
            "Soft",
            "Medium",
            "Hard"});
      this.comboBoxDynamicAmplification.Location = new System.Drawing.Point(11, 46);
      this.comboBoxDynamicAmplification.Name = "comboBoxDynamicAmplification";
      this.comboBoxDynamicAmplification.Size = new System.Drawing.Size(127, 21);
      this.comboBoxDynamicAmplification.TabIndex = 72;
      // 
      // buttonSetGain
      // 
      this.buttonSetGain.Location = new System.Drawing.Point(57, 234);
      this.buttonSetGain.Name = "buttonSetGain";
      this.buttonSetGain.Size = new System.Drawing.Size(54, 23);
      this.buttonSetGain.TabIndex = 67;
      this.buttonSetGain.Text = "Set";
      this.buttonSetGain.UseVisualStyleBackColor = true;
      this.buttonSetGain.Click += new System.EventHandler(this.buttonSetGain_Click);
      // 
      // label17
      // 
      this.label17.Location = new System.Drawing.Point(54, 79);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(38, 23);
      this.label17.TabIndex = 69;
      this.label17.Text = "+16dB";
      this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // textBoxGainDBValue
      // 
      this.textBoxGainDBValue.BorderColor = System.Drawing.Color.Empty;
      this.textBoxGainDBValue.Location = new System.Drawing.Point(11, 236);
      this.textBoxGainDBValue.Name = "textBoxGainDBValue";
      this.textBoxGainDBValue.Size = new System.Drawing.Size(40, 21);
      this.textBoxGainDBValue.TabIndex = 66;
      this.textBoxGainDBValue.Text = "0";
      this.textBoxGainDBValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // checkBoxDAmp
      // 
      this.checkBoxDAmp.AutoSize = true;
      this.checkBoxDAmp.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDAmp.Location = new System.Drawing.Point(11, 23);
      this.checkBoxDAmp.Name = "checkBoxDAmp";
      this.checkBoxDAmp.Size = new System.Drawing.Size(126, 17);
      this.checkBoxDAmp.TabIndex = 72;
      this.checkBoxDAmp.Text = "Dynamic amplification";
      this.checkBoxDAmp.UseVisualStyleBackColor = true;
      this.checkBoxDAmp.CheckedChanged += new System.EventHandler(this.checkBoxDAmp_CheckedChanged);
      // 
      // label23
      // 
      this.label23.Location = new System.Drawing.Point(62, 139);
      this.label23.Name = "label23";
      this.label23.Size = new System.Drawing.Size(38, 23);
      this.label23.TabIndex = 70;
      this.label23.Text = "0dB";
      this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // trackBarGain
      // 
      this.trackBarGain.LargeChange = 100;
      this.trackBarGain.Location = new System.Drawing.Point(11, 79);
      this.trackBarGain.Maximum = 16000;
      this.trackBarGain.Minimum = -16000;
      this.trackBarGain.Name = "trackBarGain";
      this.trackBarGain.Orientation = System.Windows.Forms.Orientation.Vertical;
      this.trackBarGain.Size = new System.Drawing.Size(42, 143);
      this.trackBarGain.SmallChange = 10;
      this.trackBarGain.TabIndex = 65;
      this.trackBarGain.TickFrequency = 2000;
      this.trackBarGain.ValueChanged += new System.EventHandler(this.trackBarGain_ValueChanged);
      // 
      // label14
      // 
      this.label14.Location = new System.Drawing.Point(54, 199);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(38, 23);
      this.label14.TabIndex = 68;
      this.label14.Text = "-16dB";
      this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBoxCompressor
      // 
      this.groupBoxCompressor.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.groupBoxCompressor.Controls.Add(this.label11);
      this.groupBoxCompressor.Controls.Add(this.label22);
      this.groupBoxCompressor.Controls.Add(this.labelCompThreshold);
      this.groupBoxCompressor.Controls.Add(this.label13);
      this.groupBoxCompressor.Controls.Add(this.checkBoxCompressor);
      this.groupBoxCompressor.Controls.Add(this.trackBarCompressor);
      this.groupBoxCompressor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxCompressor.Location = new System.Drawing.Point(252, 17);
      this.groupBoxCompressor.Name = "groupBoxCompressor";
      this.groupBoxCompressor.Size = new System.Drawing.Size(134, 271);
      this.groupBoxCompressor.TabIndex = 73;
      this.groupBoxCompressor.TabStop = false;
      this.groupBoxCompressor.Text = "Compressor";
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(65, 209);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(35, 13);
      this.label11.TabIndex = 74;
      this.label11.Text = "-25dB";
      this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label22
      // 
      this.label22.AutoSize = true;
      this.label22.Location = new System.Drawing.Point(71, 115);
      this.label22.Name = "label22";
      this.label22.Size = new System.Drawing.Size(29, 13);
      this.label22.TabIndex = 77;
      this.label22.Text = "-6dB";
      this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // labelCompThreshold
      // 
      this.labelCompThreshold.Location = new System.Drawing.Point(6, 46);
      this.labelCompThreshold.Name = "labelCompThreshold";
      this.labelCompThreshold.Size = new System.Drawing.Size(110, 23);
      this.labelCompThreshold.TabIndex = 75;
      this.labelCompThreshold.Text = "Threshold: -6.0 dB";
      this.labelCompThreshold.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(71, 84);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(29, 13);
      this.label13.TabIndex = 76;
      this.label13.Text = "-0dB";
      this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // checkBoxCompressor
      // 
      this.checkBoxCompressor.AutoSize = true;
      this.checkBoxCompressor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxCompressor.Location = new System.Drawing.Point(9, 23);
      this.checkBoxCompressor.Name = "checkBoxCompressor";
      this.checkBoxCompressor.Size = new System.Drawing.Size(81, 17);
      this.checkBoxCompressor.TabIndex = 72;
      this.checkBoxCompressor.Text = "Compressor";
      this.checkBoxCompressor.UseVisualStyleBackColor = true;
      this.checkBoxCompressor.CheckedChanged += new System.EventHandler(this.checkBoxCompressor_CheckedChanged);
      // 
      // trackBarCompressor
      // 
      this.trackBarCompressor.Location = new System.Drawing.Point(17, 84);
      this.trackBarCompressor.Maximum = 0;
      this.trackBarCompressor.Minimum = -250;
      this.trackBarCompressor.Name = "trackBarCompressor";
      this.trackBarCompressor.Orientation = System.Windows.Forms.Orientation.Vertical;
      this.trackBarCompressor.Size = new System.Drawing.Size(42, 138);
      this.trackBarCompressor.TabIndex = 73;
      this.trackBarCompressor.TickFrequency = 25;
      this.trackBarCompressor.Value = -60;
      this.trackBarCompressor.ValueChanged += new System.EventHandler(this.trackBarCompressor_ValueChanged);
      // 
      // MusicDSPTabCtl
      // 
      this.MusicDSPTabCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.MusicDSPTabCtl.Controls.Add(this.DSPTabPg);
      this.MusicDSPTabCtl.Controls.Add(this.PluginTabPg);
      this.MusicDSPTabCtl.Location = new System.Drawing.Point(0, 0);
      this.MusicDSPTabCtl.Name = "MusicDSPTabCtl";
      this.MusicDSPTabCtl.SelectedIndex = 0;
      this.MusicDSPTabCtl.Size = new System.Drawing.Size(472, 341);
      this.MusicDSPTabCtl.TabIndex = 0;
      // 
      // PluginTabPg
      // 
      this.PluginTabPg.BackColor = System.Drawing.Color.Transparent;
      this.PluginTabPg.Controls.Add(this.mpGroupBox2);
      this.PluginTabPg.Controls.Add(this.mpGroupBox1);
      this.PluginTabPg.Controls.Add(this.buttonPluginRemove);
      this.PluginTabPg.Controls.Add(this.buttonPluginAdd);
      this.PluginTabPg.Location = new System.Drawing.Point(4, 22);
      this.PluginTabPg.Name = "PluginTabPg";
      this.PluginTabPg.Padding = new System.Windows.Forms.Padding(3);
      this.PluginTabPg.Size = new System.Drawing.Size(464, 315);
      this.PluginTabPg.TabIndex = 2;
      this.PluginTabPg.Text = "VST / Winamp";
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Controls.Add(this.listBoxFoundPlugins);
      this.mpGroupBox2.Dock = System.Windows.Forms.DockStyle.Left;
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(3, 3);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(205, 309);
      this.mpGroupBox2.TabIndex = 24;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Available plugins";
      // 
      // listBoxFoundPlugins
      // 
      this.listBoxFoundPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listBoxFoundPlugins.FormattingEnabled = true;
      this.listBoxFoundPlugins.HorizontalScrollbar = true;
      this.listBoxFoundPlugins.Location = new System.Drawing.Point(6, 20);
      this.listBoxFoundPlugins.Name = "listBoxFoundPlugins";
      this.listBoxFoundPlugins.Size = new System.Drawing.Size(193, 277);
      this.listBoxFoundPlugins.Sorted = true;
      this.listBoxFoundPlugins.TabIndex = 10;
      this.listBoxFoundPlugins.DoubleClick += new System.EventHandler(this.listBoxFoundPlugins_DoubleClick);
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.listBoxSelectedPlugins);
      this.mpGroupBox1.Controls.Add(this.btnConfig);
      this.mpGroupBox1.Dock = System.Windows.Forms.DockStyle.Right;
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(256, 3);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(205, 309);
      this.mpGroupBox1.TabIndex = 23;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Selected plugins in ascending priority";
      // 
      // listBoxSelectedPlugins
      // 
      this.listBoxSelectedPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listBoxSelectedPlugins.FormattingEnabled = true;
      this.listBoxSelectedPlugins.HorizontalScrollbar = true;
      this.listBoxSelectedPlugins.Location = new System.Drawing.Point(6, 20);
      this.listBoxSelectedPlugins.Name = "listBoxSelectedPlugins";
      this.listBoxSelectedPlugins.Size = new System.Drawing.Size(193, 251);
      this.listBoxSelectedPlugins.TabIndex = 11;
      this.listBoxSelectedPlugins.DoubleClick += new System.EventHandler(this.listBoxSelectedPlugins_DoubleClick);
      // 
      // btnConfig
      // 
      this.btnConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnConfig.Location = new System.Drawing.Point(47, 280);
      this.btnConfig.Name = "btnConfig";
      this.btnConfig.Size = new System.Drawing.Size(152, 23);
      this.btnConfig.TabIndex = 22;
      this.btnConfig.Text = "Plugin configuration";
      this.btnConfig.UseVisualStyleBackColor = true;
      this.btnConfig.Click += new System.EventHandler(this.btnConfig_Click);
      // 
      // buttonPluginRemove
      // 
      this.buttonPluginRemove.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.buttonPluginRemove.Location = new System.Drawing.Point(214, 167);
      this.buttonPluginRemove.Name = "buttonPluginRemove";
      this.buttonPluginRemove.Size = new System.Drawing.Size(33, 23);
      this.buttonPluginRemove.TabIndex = 13;
      this.buttonPluginRemove.Text = "<";
      this.buttonPluginRemove.UseVisualStyleBackColor = true;
      this.buttonPluginRemove.Click += new System.EventHandler(this.buttonPluginRemove_Click);
      // 
      // buttonPluginAdd
      // 
      this.buttonPluginAdd.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.buttonPluginAdd.Location = new System.Drawing.Point(214, 118);
      this.buttonPluginAdd.Name = "buttonPluginAdd";
      this.buttonPluginAdd.Size = new System.Drawing.Size(33, 23);
      this.buttonPluginAdd.TabIndex = 12;
      this.buttonPluginAdd.Text = ">";
      this.buttonPluginAdd.UseVisualStyleBackColor = true;
      this.buttonPluginAdd.Click += new System.EventHandler(this.buttonPluginAdd_Click);
      // 
      // btFileselect
      // 
      this.btFileselect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btFileselect.Location = new System.Drawing.Point(440, 347);
      this.btFileselect.Name = "btFileselect";
      this.btFileselect.Size = new System.Drawing.Size(29, 23);
      this.btFileselect.TabIndex = 10;
      this.btFileselect.Text = "....";
      this.btFileselect.UseVisualStyleBackColor = true;
      this.btFileselect.Click += new System.EventHandler(this.btFileselect_Click);
      // 
      // textBoxMusicFile
      // 
      this.textBoxMusicFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxMusicFile.BorderColor = System.Drawing.Color.Empty;
      this.textBoxMusicFile.Location = new System.Drawing.Point(65, 347);
      this.textBoxMusicFile.Name = "textBoxMusicFile";
      this.textBoxMusicFile.Size = new System.Drawing.Size(369, 21);
      this.textBoxMusicFile.TabIndex = 9;
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(10, 350);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(49, 13);
      this.label1.TabIndex = 8;
      this.label1.Text = "Test file:";
      // 
      // btStop
      // 
      this.btStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btStop.Enabled = false;
      this.btStop.Location = new System.Drawing.Point(146, 374);
      this.btStop.Name = "btStop";
      this.btStop.Size = new System.Drawing.Size(75, 23);
      this.btStop.TabIndex = 7;
      this.btStop.Text = "Stop";
      this.btStop.UseVisualStyleBackColor = true;
      this.btStop.Click += new System.EventHandler(this.btStop_Click);
      // 
      // btPlay
      // 
      this.btPlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btPlay.Location = new System.Drawing.Point(65, 374);
      this.btPlay.Name = "btPlay";
      this.btPlay.Size = new System.Drawing.Size(75, 23);
      this.btPlay.TabIndex = 6;
      this.btPlay.Text = "Play";
      this.btPlay.UseVisualStyleBackColor = true;
      this.btPlay.Click += new System.EventHandler(this.btPlay_Click);
      // 
      // MusicDSP
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.btFileselect);
      this.Controls.Add(this.MusicDSPTabCtl);
      this.Controls.Add(this.textBoxMusicFile);
      this.Controls.Add(this.btPlay);
      this.Controls.Add(this.btStop);
      this.Controls.Add(this.label1);
      this.Name = "MusicDSP";
      this.Size = new System.Drawing.Size(472, 400);
      this.Load += new System.EventHandler(this.MusicDSP_Load);
      this.DSPTabPg.ResumeLayout(false);
      this.groupBoxGain.ResumeLayout(false);
      this.groupBoxGain.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarGain)).EndInit();
      this.groupBoxCompressor.ResumeLayout(false);
      this.groupBoxCompressor.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarCompressor)).EndInit();
      this.MusicDSPTabCtl.ResumeLayout(false);
      this.PluginTabPg.ResumeLayout(false);
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPTabPage DSPTabPg;
    private MediaPortal.UserInterface.Controls.MPTabControl MusicDSPTabCtl;
    private MediaPortal.UserInterface.Controls.MPTabPage PluginTabPg;
    private MediaPortal.UserInterface.Controls.MPButton btFileselect;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxMusicFile;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPButton btStop;
    private MediaPortal.UserInterface.Controls.MPButton btPlay;
    private MediaPortal.UserInterface.Controls.MPLabel label17;
    private MediaPortal.UserInterface.Controls.MPButton buttonSetGain;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxGainDBValue;
    private MediaPortal.UserInterface.Controls.MPLabel label23;
    private MediaPortal.UserInterface.Controls.MPLabel label14;
    private System.Windows.Forms.TrackBar trackBarGain;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxGain;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxDynamicAmplification;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxDAmp;
    private MediaPortal.UserInterface.Controls.MPButton buttonPluginRemove;
    private MediaPortal.UserInterface.Controls.MPButton buttonPluginAdd;
    private System.Windows.Forms.ListBox listBoxSelectedPlugins;
    private System.Windows.Forms.ListBox listBoxFoundPlugins;
    private MediaPortal.UserInterface.Controls.MPLabel label22;
    private MediaPortal.UserInterface.Controls.MPLabel label13;
    private MediaPortal.UserInterface.Controls.MPLabel label11;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxCompressor;
    private MediaPortal.UserInterface.Controls.MPLabel labelCompThreshold;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxCompressor;
    private System.Windows.Forms.TrackBar trackBarCompressor;
    private System.Windows.Forms.ToolTip toolTip;
    private MediaPortal.UserInterface.Controls.MPButton btnConfig;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;

  }
}
