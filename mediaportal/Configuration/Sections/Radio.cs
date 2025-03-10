#region Copyright (C) 2005-2025 Team MediaPortal

// Copyright (C) 2005-2025 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#endregion

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class Radio : SectionSettings
  {
    #region Variables

    private bool _hideAllChannelsGroup = false;
    private bool _rememberLastGroup = true;
    private string _rootGroup = "(none)";
    private MPGroupBox groupBox3;
    private MPCheckBox byIndexCheckBox;
    private MPCheckBox showChannelNumberCheckBox;
    private MPNumericUpDown channelNumberMaxLengthNumUpDn;
    private MPLabel lblChanNumMaxLen;
    private bool _autoTurnOnRadio = false;

    #endregion

    #region Constructor

    public Radio()
      : this("Radio") { }

    public Radio(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

    }

    #endregion

    #region Public methods

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        _hideAllChannelsGroup = xmlreader.GetValueAsBool("myradio", "hideAllChannelsGroup", false);
        _rememberLastGroup = xmlreader.GetValueAsBool("myradio", "rememberlastgroup", true);
        _rootGroup = xmlreader.GetValueAsString("myradio", "rootgroup", "(none)");
        _autoTurnOnRadio = xmlreader.GetValueAsBool("myradio", "autoturnonradio", false);
        byIndexCheckBox.Checked = xmlreader.GetValueAsBool("myradio", "byindex", true);
        showChannelNumberCheckBox.Checked = xmlreader.GetValueAsBool("myradio", "showchannelnumber", false);
        channelNumberMaxLengthNumUpDn.Value = xmlreader.GetValueAsInt("myradio", "channelnumbermaxlength", 3);
      }

      cbTurnOnRadio.Checked = _autoTurnOnRadio;
      cbHideAllChannelsGroup.Checked = _hideAllChannelsGroup;
      cbRememberLastGroup.Checked = _rememberLastGroup;
      comboBoxGroups.Items.Clear();
      comboBoxGroups.Items.Add(_rootGroup);
      comboBoxGroups.SelectedIndex = 0;

      // Enable this Panel if the TvPlugin exists in the plug-in Directory
      Enabled = true;
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("myradio", "hideAllChannelsGroup", _hideAllChannelsGroup);
        xmlwriter.SetValueAsBool("myradio", "rememberlastgroup", _rememberLastGroup);
        xmlwriter.SetValue("myradio", "rootgroup", _rootGroup);
        xmlwriter.SetValueAsBool("myradio", "autoturnonradio", _autoTurnOnRadio);
        xmlwriter.SetValueAsBool("myradio", "byindex", byIndexCheckBox.Checked);
        xmlwriter.SetValueAsBool("myradio", "showchannelnumber", showChannelNumberCheckBox.Checked);
        xmlwriter.SetValue("myradio", "channelnumbermaxlength", channelNumberMaxLengthNumUpDn.Value);
      }
    }

    #endregion

    #region Designer generated code

    private MPGroupBox groupBoxChannelGroups;
    private MPCheckBox cbHideAllChannelsGroup;
    private MPCheckBox cbRememberLastGroup;
    private MPLabel label1;
    private MPComboBox comboBoxGroups;
    private MPGroupBox groupBoxRadioScreen;
    private MPCheckBox cbTurnOnRadio;

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.groupBoxRadioScreen = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.cbTurnOnRadio = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.groupBoxChannelGroups = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.cbRememberLastGroup = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.comboBoxGroups = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.cbHideAllChannelsGroup = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.byIndexCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.showChannelNumberCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.channelNumberMaxLengthNumUpDn = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
            this.lblChanNumMaxLen = new MediaPortal.UserInterface.Controls.MPLabel();
            this.groupBoxRadioScreen.SuspendLayout();
            this.groupBoxChannelGroups.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.channelNumberMaxLengthNumUpDn)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxRadioScreen
            // 
            this.groupBoxRadioScreen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxRadioScreen.Controls.Add(this.cbTurnOnRadio);
            this.groupBoxRadioScreen.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.groupBoxRadioScreen.Location = new System.Drawing.Point(245, 126);
            this.groupBoxRadioScreen.Name = "groupBoxRadioScreen";
            this.groupBoxRadioScreen.Size = new System.Drawing.Size(223, 94);
            this.groupBoxRadioScreen.TabIndex = 12;
            this.groupBoxRadioScreen.TabStop = false;
            this.groupBoxRadioScreen.Text = "When entering the Radio screen:";
            // 
            // cbTurnOnRadio
            // 
            this.cbTurnOnRadio.AutoSize = true;
            this.cbTurnOnRadio.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cbTurnOnRadio.Location = new System.Drawing.Point(13, 19);
            this.cbTurnOnRadio.Name = "cbTurnOnRadio";
            this.cbTurnOnRadio.Size = new System.Drawing.Size(92, 17);
            this.cbTurnOnRadio.TabIndex = 0;
            this.cbTurnOnRadio.Text = "Turn on Radio";
            this.cbTurnOnRadio.UseVisualStyleBackColor = true;
            this.cbTurnOnRadio.CheckedChanged += new System.EventHandler(this.cbTurnOnRadio_CheckedChanged);
            // 
            // groupBoxChannelGroups
            // 
            this.groupBoxChannelGroups.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxChannelGroups.Controls.Add(this.cbRememberLastGroup);
            this.groupBoxChannelGroups.Controls.Add(this.comboBoxGroups);
            this.groupBoxChannelGroups.Controls.Add(this.label1);
            this.groupBoxChannelGroups.Controls.Add(this.cbHideAllChannelsGroup);
            this.groupBoxChannelGroups.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.groupBoxChannelGroups.Location = new System.Drawing.Point(6, 0);
            this.groupBoxChannelGroups.Name = "groupBoxChannelGroups";
            this.groupBoxChannelGroups.Size = new System.Drawing.Size(462, 120);
            this.groupBoxChannelGroups.TabIndex = 11;
            this.groupBoxChannelGroups.TabStop = false;
            this.groupBoxChannelGroups.Text = "Channel groups";
            // 
            // cbRememberLastGroup
            // 
            this.cbRememberLastGroup.AutoSize = true;
            this.cbRememberLastGroup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cbRememberLastGroup.Location = new System.Drawing.Point(13, 42);
            this.cbRememberLastGroup.Name = "cbRememberLastGroup";
            this.cbRememberLastGroup.Size = new System.Drawing.Size(124, 17);
            this.cbRememberLastGroup.TabIndex = 1;
            this.cbRememberLastGroup.Text = "Remember last group";
            this.cbRememberLastGroup.UseVisualStyleBackColor = true;
            this.cbRememberLastGroup.CheckedChanged += new System.EventHandler(this.cbRememberLastGroup_CheckedChanged);
            // 
            // comboBoxGroups
            // 
            this.comboBoxGroups.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxGroups.BorderColor = System.Drawing.Color.Empty;
            this.comboBoxGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxGroups.FormattingEnabled = true;
            this.comboBoxGroups.Location = new System.Drawing.Point(13, 83);
            this.comboBoxGroups.Name = "comboBoxGroups";
            this.comboBoxGroups.Size = new System.Drawing.Size(431, 21);
            this.comboBoxGroups.TabIndex = 2;
            this.comboBoxGroups.DropDown += new System.EventHandler(this.comboBoxGroups_DropDown);
            this.comboBoxGroups.SelectedIndexChanged += new System.EventHandler(this.comboBoxGroups_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Group to show in root menu:";
            // 
            // cbHideAllChannelsGroup
            // 
            this.cbHideAllChannelsGroup.AutoSize = true;
            this.cbHideAllChannelsGroup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cbHideAllChannelsGroup.Location = new System.Drawing.Point(13, 19);
            this.cbHideAllChannelsGroup.Name = "cbHideAllChannelsGroup";
            this.cbHideAllChannelsGroup.Size = new System.Drawing.Size(164, 17);
            this.cbHideAllChannelsGroup.TabIndex = 0;
            this.cbHideAllChannelsGroup.Text = "Hide the \"All channels group\"";
            this.cbHideAllChannelsGroup.UseVisualStyleBackColor = true;
            this.cbHideAllChannelsGroup.Click += new System.EventHandler(this.cbHideAllChannelsGroup_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.byIndexCheckBox);
            this.groupBox3.Controls.Add(this.showChannelNumberCheckBox);
            this.groupBox3.Controls.Add(this.channelNumberMaxLengthNumUpDn);
            this.groupBox3.Controls.Add(this.lblChanNumMaxLen);
            this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.groupBox3.Location = new System.Drawing.Point(6, 126);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(233, 94);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Channel numbers";
            // 
            // byIndexCheckBox
            // 
            this.byIndexCheckBox.AutoSize = true;
            this.byIndexCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.byIndexCheckBox.Location = new System.Drawing.Point(17, 20);
            this.byIndexCheckBox.Name = "byIndexCheckBox";
            this.byIndexCheckBox.Size = new System.Drawing.Size(182, 17);
            this.byIndexCheckBox.TabIndex = 0;
            this.byIndexCheckBox.Text = "Select channel by index (non-US)";
            this.byIndexCheckBox.UseVisualStyleBackColor = true;
            // 
            // showChannelNumberCheckBox
            // 
            this.showChannelNumberCheckBox.AutoSize = true;
            this.showChannelNumberCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.showChannelNumberCheckBox.Location = new System.Drawing.Point(17, 40);
            this.showChannelNumberCheckBox.Name = "showChannelNumberCheckBox";
            this.showChannelNumberCheckBox.Size = new System.Drawing.Size(135, 17);
            this.showChannelNumberCheckBox.TabIndex = 1;
            this.showChannelNumberCheckBox.Text = "Show channel numbers";
            this.showChannelNumberCheckBox.UseVisualStyleBackColor = true;
            // 
            // channelNumberMaxLengthNumUpDn
            // 
            this.channelNumberMaxLengthNumUpDn.AutoSize = true;
            this.channelNumberMaxLengthNumUpDn.Location = new System.Drawing.Point(178, 60);
            this.channelNumberMaxLengthNumUpDn.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.channelNumberMaxLengthNumUpDn.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.channelNumberMaxLengthNumUpDn.Name = "channelNumberMaxLengthNumUpDn";
            this.channelNumberMaxLengthNumUpDn.Size = new System.Drawing.Size(42, 20);
            this.channelNumberMaxLengthNumUpDn.TabIndex = 2;
            this.channelNumberMaxLengthNumUpDn.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // lblChanNumMaxLen
            // 
            this.lblChanNumMaxLen.AutoSize = true;
            this.lblChanNumMaxLen.Location = new System.Drawing.Point(31, 62);
            this.lblChanNumMaxLen.Name = "lblChanNumMaxLen";
            this.lblChanNumMaxLen.Size = new System.Drawing.Size(141, 13);
            this.lblChanNumMaxLen.TabIndex = 2;
            this.lblChanNumMaxLen.Text = "Channel number max. length";
            // 
            // Radio
            // 
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBoxRadioScreen);
            this.Controls.Add(this.groupBoxChannelGroups);
            this.Name = "Radio";
            this.Size = new System.Drawing.Size(472, 408);
            this.groupBoxRadioScreen.ResumeLayout(false);
            this.groupBoxRadioScreen.PerformLayout();
            this.groupBoxChannelGroups.ResumeLayout(false);
            this.groupBoxChannelGroups.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.channelNumberMaxLengthNumUpDn)).EndInit();
            this.ResumeLayout(false);

    }

    #endregion

    #region Private methods

    private void cbHideAllChannelsGroup_Click(object sender, EventArgs e)
    {
      _hideAllChannelsGroup = cbHideAllChannelsGroup.Checked;
    }

    private void cbRememberLastGroup_CheckedChanged(object sender, EventArgs e)
    {
      _rememberLastGroup = cbRememberLastGroup.Checked;
    }

    private void comboBoxGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
      _rootGroup = comboBoxGroups.Text;
    }

    private void cbTurnOnRadio_CheckedChanged(object sender, EventArgs e)
    {
      _autoTurnOnRadio = cbTurnOnRadio.Checked;
    }

    private void comboBoxGroups_DropDown(object sender, EventArgs e)
    {
      // Save current cursor and display wait cursor
      Cursor currentCursor = Cursor.Current;
      Cursor.Current = Cursors.WaitCursor;

      // Fill comboBox with radio channel names
      comboBoxGroups.Items.Clear();
      comboBoxGroups.Items.Add("(none)");
      int selectedIdx = 0;

      if (string.IsNullOrEmpty(TVRadio.Hostname))
      {
        MessageBox.Show("Unable to get radio channel groups from the TV Server." +
          Environment.NewLine + "No valid hostname specified in the \"TV/Radio\" section.",
          "Radio Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      else
      {
        TvServerRemote.HostName = TVRadio.Hostname;
        List<string> groupNames = TvServerRemote.GetRadioChannelGroupNames();
        if (groupNames.Count == 0)
        {
          MessageBox.Show(string.Format("Unable to get radio channel groups from the TV Server on host \"{0}\".", TVRadio.Hostname) +
            Environment.NewLine + "Version incompatibility of MP Client and TV Server?",
            "Radio Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        else
        {
          foreach (string groupName in groupNames)
          {
            int idx = comboBoxGroups.Items.Add(groupName);
            if (groupName == _rootGroup)
              selectedIdx = idx;
          }
        }
      }
      comboBoxGroups.SelectedIndex = selectedIdx;

      // Reset cursor
      Cursor.Current = currentCursor;
    }

    #endregion
  }
}