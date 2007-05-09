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
      this.label11 = new MediaPortal.UserInterface.Controls.MPLabel();
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
      this.label22 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelCompThreshold = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label13 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxCompressor = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.trackBarCompressor = new System.Windows.Forms.TrackBar();
      this.MusicDSPTabCtl = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.PluginTabPg = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.labelVSTWarning = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonPluginRemove = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonPluginAdd = new MediaPortal.UserInterface.Controls.MPButton();
      this.listBoxSelectedPlugins = new System.Windows.Forms.ListBox();
      this.listBoxFoundPlugins = new System.Windows.Forms.ListBox();
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
      this.SuspendLayout();
      // 
      // DSPTabPg
      // 
      this.DSPTabPg.BackColor = System.Drawing.SystemColors.Control;
      this.DSPTabPg.Controls.Add(this.label11);
      this.DSPTabPg.Controls.Add(this.groupBoxGain);
      this.DSPTabPg.Controls.Add(this.groupBoxCompressor);
      this.DSPTabPg.Location = new System.Drawing.Point(4, 22);
      this.DSPTabPg.Name = "DSPTabPg";
      this.DSPTabPg.Padding = new System.Windows.Forms.Padding(3);
      this.DSPTabPg.Size = new System.Drawing.Size(458, 305);
      this.DSPTabPg.TabIndex = 1;
      this.DSPTabPg.Text = "BASS DSP / FX";
      // 
      // label11
      // 
      this.label11.Location = new System.Drawing.Point(337, 207);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(38, 23);
      this.label11.TabIndex = 74;
      this.label11.Text = "-25dB";
      this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBoxGain
      // 
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
      this.textBoxGainDBValue.Size = new System.Drawing.Size(40, 20);
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
      this.trackBarGain.Size = new System.Drawing.Size(45, 143);
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
      this.groupBoxCompressor.Controls.Add(this.label22);
      this.groupBoxCompressor.Controls.Add(this.labelCompThreshold);
      this.groupBoxCompressor.Controls.Add(this.label13);
      this.groupBoxCompressor.Controls.Add(this.checkBoxCompressor);
      this.groupBoxCompressor.Controls.Add(this.trackBarCompressor);
      this.groupBoxCompressor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxCompressor.Location = new System.Drawing.Point(252, 17);
      this.groupBoxCompressor.Name = "groupBoxCompressor";
      this.groupBoxCompressor.Size = new System.Drawing.Size(151, 271);
      this.groupBoxCompressor.TabIndex = 73;
      this.groupBoxCompressor.TabStop = false;
      this.groupBoxCompressor.Text = "Compressor";
      // 
      // label22
      // 
      this.label22.Location = new System.Drawing.Point(74, 113);
      this.label22.Name = "label22";
      this.label22.Size = new System.Drawing.Size(38, 23);
      this.label22.TabIndex = 77;
      this.label22.Text = "-6dB";
      this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelCompThreshold
      // 
      this.labelCompThreshold.Location = new System.Drawing.Point(12, 46);
      this.labelCompThreshold.Name = "labelCompThreshold";
      this.labelCompThreshold.Size = new System.Drawing.Size(110, 23);
      this.labelCompThreshold.TabIndex = 75;
      this.labelCompThreshold.Text = "Threshold: -6.0 dB";
      this.labelCompThreshold.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label13
      // 
      this.label13.Location = new System.Drawing.Point(74, 84);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(38, 23);
      this.label13.TabIndex = 76;
      this.label13.Text = "-0dB";
      this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // checkBoxCompressor
      // 
      this.checkBoxCompressor.AutoSize = true;
      this.checkBoxCompressor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxCompressor.Location = new System.Drawing.Point(15, 23);
      this.checkBoxCompressor.Name = "checkBoxCompressor";
      this.checkBoxCompressor.Size = new System.Drawing.Size(79, 17);
      this.checkBoxCompressor.TabIndex = 72;
      this.checkBoxCompressor.Text = "Compressor";
      this.checkBoxCompressor.UseVisualStyleBackColor = true;
      this.checkBoxCompressor.CheckedChanged += new System.EventHandler(this.checkBoxCompressor_CheckedChanged);
      // 
      // trackBarCompressor
      // 
      this.trackBarCompressor.Location = new System.Drawing.Point(23, 84);
      this.trackBarCompressor.Maximum = 0;
      this.trackBarCompressor.Minimum = -250;
      this.trackBarCompressor.Name = "trackBarCompressor";
      this.trackBarCompressor.Orientation = System.Windows.Forms.Orientation.Vertical;
      this.trackBarCompressor.Size = new System.Drawing.Size(45, 143);
      this.trackBarCompressor.TabIndex = 73;
      this.trackBarCompressor.TickFrequency = 25;
      this.trackBarCompressor.Value = -60;
      this.trackBarCompressor.ValueChanged += new System.EventHandler(this.trackBarCompressor_ValueChanged);
      // 
      // MusicDSPTabCtl
      // 
      this.MusicDSPTabCtl.Controls.Add(this.DSPTabPg);
      this.MusicDSPTabCtl.Controls.Add(this.PluginTabPg);
      this.MusicDSPTabCtl.Location = new System.Drawing.Point(3, 3);
      this.MusicDSPTabCtl.Name = "MusicDSPTabCtl";
      this.MusicDSPTabCtl.SelectedIndex = 0;
      this.MusicDSPTabCtl.Size = new System.Drawing.Size(466, 331);
      this.MusicDSPTabCtl.TabIndex = 0;
      // 
      // PluginTabPg
      // 
      this.PluginTabPg.BackColor = System.Drawing.Color.Transparent;
      this.PluginTabPg.Controls.Add(this.labelVSTWarning);
      this.PluginTabPg.Controls.Add(this.label5);
      this.PluginTabPg.Controls.Add(this.label4);
      this.PluginTabPg.Controls.Add(this.label3);
      this.PluginTabPg.Controls.Add(this.buttonPluginRemove);
      this.PluginTabPg.Controls.Add(this.buttonPluginAdd);
      this.PluginTabPg.Controls.Add(this.listBoxSelectedPlugins);
      this.PluginTabPg.Controls.Add(this.listBoxFoundPlugins);
      this.PluginTabPg.Location = new System.Drawing.Point(4, 22);
      this.PluginTabPg.Name = "PluginTabPg";
      this.PluginTabPg.Padding = new System.Windows.Forms.Padding(3);
      this.PluginTabPg.Size = new System.Drawing.Size(458, 305);
      this.PluginTabPg.TabIndex = 2;
      this.PluginTabPg.Text = "VST / Winamp";
      this.PluginTabPg.UseVisualStyleBackColor = true;
      // 
      // labelVSTWarning
      // 
      this.labelVSTWarning.AutoSize = true;
      this.labelVSTWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelVSTWarning.Location = new System.Drawing.Point(12, 15);
      this.labelVSTWarning.Name = "labelVSTWarning";
      this.labelVSTWarning.Size = new System.Drawing.Size(412, 13);
      this.labelVSTWarning.TabIndex = 21;
      this.labelVSTWarning.Text = "EXPERIMENTAL: Some plugins might be outdated or not working at all!";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(249, 277);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(176, 13);
      this.label5.TabIndex = 20;
      this.label5.Text = "Double click for plugin configuration";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(250, 42);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(181, 13);
      this.label4.TabIndex = 19;
      this.label4.Text = "Selected plugins in ascending priority";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(12, 42);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(86, 13);
      this.label3.TabIndex = 18;
      this.label3.Text = "Available plugins";
      // 
      // buttonPluginRemove
      // 
      this.buttonPluginRemove.Location = new System.Drawing.Point(218, 178);
      this.buttonPluginRemove.Name = "buttonPluginRemove";
      this.buttonPluginRemove.Size = new System.Drawing.Size(28, 23);
      this.buttonPluginRemove.TabIndex = 13;
      this.buttonPluginRemove.Text = "<";
      this.buttonPluginRemove.UseVisualStyleBackColor = true;
      this.buttonPluginRemove.Click += new System.EventHandler(this.buttonPluginRemove_Click);
      // 
      // buttonPluginAdd
      // 
      this.buttonPluginAdd.Location = new System.Drawing.Point(218, 127);
      this.buttonPluginAdd.Name = "buttonPluginAdd";
      this.buttonPluginAdd.Size = new System.Drawing.Size(28, 23);
      this.buttonPluginAdd.TabIndex = 12;
      this.buttonPluginAdd.Text = ">";
      this.buttonPluginAdd.UseVisualStyleBackColor = true;
      this.buttonPluginAdd.Click += new System.EventHandler(this.buttonPluginAdd_Click);
      // 
      // listBoxSelectedPlugins
      // 
      this.listBoxSelectedPlugins.FormattingEnabled = true;
      this.listBoxSelectedPlugins.HorizontalScrollbar = true;
      this.listBoxSelectedPlugins.Location = new System.Drawing.Point(253, 72);
      this.listBoxSelectedPlugins.Name = "listBoxSelectedPlugins";
      this.listBoxSelectedPlugins.Size = new System.Drawing.Size(195, 199);
      this.listBoxSelectedPlugins.TabIndex = 11;
      this.listBoxSelectedPlugins.DoubleClick += new System.EventHandler(this.listBoxSelectedPlugins_DoubleClick);
      // 
      // listBoxFoundPlugins
      // 
      this.listBoxFoundPlugins.FormattingEnabled = true;
      this.listBoxFoundPlugins.HorizontalScrollbar = true;
      this.listBoxFoundPlugins.Location = new System.Drawing.Point(11, 72);
      this.listBoxFoundPlugins.Name = "listBoxFoundPlugins";
      this.listBoxFoundPlugins.Size = new System.Drawing.Size(195, 199);
      this.listBoxFoundPlugins.Sorted = true;
      this.listBoxFoundPlugins.TabIndex = 10;
      // 
      // btFileselect
      // 
      this.btFileselect.Location = new System.Drawing.Point(416, 338);
      this.btFileselect.Name = "btFileselect";
      this.btFileselect.Size = new System.Drawing.Size(29, 23);
      this.btFileselect.TabIndex = 10;
      this.btFileselect.Text = "....";
      this.btFileselect.UseVisualStyleBackColor = true;
      this.btFileselect.Click += new System.EventHandler(this.btFileselect_Click);
      // 
      // textBoxMusicFile
      // 
      this.textBoxMusicFile.BorderColor = System.Drawing.Color.Empty;
      this.textBoxMusicFile.Location = new System.Drawing.Point(68, 340);
      this.textBoxMusicFile.Name = "textBoxMusicFile";
      this.textBoxMusicFile.Size = new System.Drawing.Size(342, 20);
      this.textBoxMusicFile.TabIndex = 9;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(15, 343);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(47, 13);
      this.label1.TabIndex = 8;
      this.label1.Text = "Test file:";
      // 
      // btStop
      // 
      this.btStop.Enabled = false;
      this.btStop.Location = new System.Drawing.Point(149, 366);
      this.btStop.Name = "btStop";
      this.btStop.Size = new System.Drawing.Size(75, 23);
      this.btStop.TabIndex = 7;
      this.btStop.Text = "Stop";
      this.btStop.UseVisualStyleBackColor = true;
      this.btStop.Click += new System.EventHandler(this.btStop_Click);
      // 
      // btPlay
      // 
      this.btPlay.Location = new System.Drawing.Point(68, 366);
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
      this.PluginTabPg.PerformLayout();
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
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPButton buttonPluginRemove;
    private MediaPortal.UserInterface.Controls.MPButton buttonPluginAdd;
    private System.Windows.Forms.ListBox listBoxSelectedPlugins;
    private System.Windows.Forms.ListBox listBoxFoundPlugins;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPLabel label22;
    private MediaPortal.UserInterface.Controls.MPLabel label13;
    private MediaPortal.UserInterface.Controls.MPLabel label11;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxCompressor;
    private MediaPortal.UserInterface.Controls.MPLabel labelCompThreshold;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxCompressor;
    private System.Windows.Forms.TrackBar trackBarCompressor;
    private System.Windows.Forms.ToolTip toolTip;
    private MediaPortal.UserInterface.Controls.MPLabel labelVSTWarning;

  }
}
