#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using System.Collections.Generic;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class TV : SectionSettings
  {
    private MPRadioButton radioButton1;
    private MPGroupBox groupBox3;
    private MPCheckBox byIndexCheckBox;
    private MPCheckBox showChannelNumberCheckBox;
    private Label lblChanNumMaxLen;
    private NumericUpDown channelNumberMaxLengthNumUpDn;
    private bool _init;
    private MPGroupBox groupBox5;
    private MPCheckBox cbTurnOnTv;
    private MPCheckBox cbAutoFullscreen;
    private MPLabel label8;
    private MPComboBox cbDeinterlace;
    private MPGroupBox mpGroupBoxAdditional;
    private MPGroupBox gbRecording;
    private MPListView lvIncludeScheduleTypes;
    private MPCheckBox cbUseQuickRecordMenu;
    private ColumnHeader clIncludedSchedules;
    private MPButton btQuickRecordScheduleUp;
    private MPButton btQuickRecordScheduleDown;
    public int pluginVersion;
    private readonly Dictionary<ListViewItem, string> _listItemScheduleLookup = new Dictionary<ListViewItem, string>();

    public TV()
      : this("TV") {}

    public TV(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      if (_init == false)
      {
        _init = true;
        LoadSettings();
      }
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.radioButton1 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.byIndexCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.showChannelNumberCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.channelNumberMaxLengthNumUpDn = new System.Windows.Forms.NumericUpDown();
      this.lblChanNumMaxLen = new System.Windows.Forms.Label();
      this.groupBox5 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbAutoFullscreen = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbTurnOnTv = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbDeinterlace = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpGroupBoxAdditional = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.gbRecording = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.btQuickRecordScheduleDown = new MediaPortal.UserInterface.Controls.MPButton();
      this.btQuickRecordScheduleUp = new MediaPortal.UserInterface.Controls.MPButton();
      this.lvIncludeScheduleTypes = new MediaPortal.UserInterface.Controls.MPListView();
      this.clIncludedSchedules = new System.Windows.Forms.ColumnHeader();
      this.cbUseQuickRecordMenu = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberMaxLengthNumUpDn)).BeginInit();
      this.groupBox5.SuspendLayout();
      this.mpGroupBoxAdditional.SuspendLayout();
      this.gbRecording.SuspendLayout();
      this.SuspendLayout();
      
      // 
      // radioButton1
      // 
      this.radioButton1.AutoSize = true;
      this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButton1.Location = new System.Drawing.Point(0, 0);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(104, 24);
      this.radioButton1.TabIndex = 0;
      this.radioButton1.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.byIndexCheckBox);
      this.groupBox3.Controls.Add(this.showChannelNumberCheckBox);
      this.groupBox3.Controls.Add(this.channelNumberMaxLengthNumUpDn);
      this.groupBox3.Controls.Add(this.lblChanNumMaxLen);
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox3.Location = new System.Drawing.Point(6, 66);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(233, 94);
      this.groupBox3.TabIndex = 3;
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
      this.channelNumberMaxLengthNumUpDn.Maximum = new decimal(new int[]
                                                                 {
                                                                   5,
                                                                   0,
                                                                   0,
                                                                   0
                                                                 });
      this.channelNumberMaxLengthNumUpDn.Minimum = new decimal(new int[]
                                                                 {
                                                                   1,
                                                                   0,
                                                                   0,
                                                                   0
                                                                 });
      this.channelNumberMaxLengthNumUpDn.Name = "channelNumberMaxLengthNumUpDn";
      this.channelNumberMaxLengthNumUpDn.Size = new System.Drawing.Size(42, 20);
      this.channelNumberMaxLengthNumUpDn.TabIndex = 3;
      this.channelNumberMaxLengthNumUpDn.Value = new decimal(new int[]
                                                               {
                                                                 3,
                                                                 0,
                                                                 0,
                                                                 0
                                                               });
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
      // groupBox5
      // 
      this.groupBox5.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox5.Controls.Add(this.cbAutoFullscreen);
      this.groupBox5.Controls.Add(this.cbTurnOnTv);
      this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox5.Location = new System.Drawing.Point(248, 66);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(220, 94);
      this.groupBox5.TabIndex = 4;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "When entering the TV screen:";
      // 
      // cbAutoFullscreen
      // 
      this.cbAutoFullscreen.AutoSize = true;
      this.cbAutoFullscreen.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAutoFullscreen.Location = new System.Drawing.Point(17, 40);
      this.cbAutoFullscreen.Name = "cbAutoFullscreen";
      this.cbAutoFullscreen.Size = new System.Drawing.Size(152, 17);
      this.cbAutoFullscreen.TabIndex = 1;
      this.cbAutoFullscreen.Text = "Directly show fullscreen TV";
      this.cbAutoFullscreen.UseVisualStyleBackColor = true;
      this.cbAutoFullscreen.CheckedChanged += new System.EventHandler(this.cbAutoFullscreen_CheckedChanged);
      // 
      // cbTurnOnTv
      // 
      this.cbTurnOnTv.AutoSize = true;
      this.cbTurnOnTv.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbTurnOnTv.Location = new System.Drawing.Point(17, 20);
      this.cbTurnOnTv.Name = "cbTurnOnTv";
      this.cbTurnOnTv.Size = new System.Drawing.Size(78, 17);
      this.cbTurnOnTv.TabIndex = 0;
      this.cbTurnOnTv.Text = "Turn on TV";
      this.cbTurnOnTv.UseVisualStyleBackColor = true;
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(6, 23);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(146, 17);
      this.label8.TabIndex = 14;
      this.label8.Text = "Fallback de-interlace mode:";
      // 
      // cbDeinterlace
      // 
      this.cbDeinterlace.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.cbDeinterlace.BorderColor = System.Drawing.Color.Empty;
      this.cbDeinterlace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDeinterlace.Items.AddRange(new object[]
                                          {
                                            "None",
                                            "Bob",
                                            "Weave",
                                            "Best"
                                          });
      this.cbDeinterlace.Location = new System.Drawing.Point(166, 19);
      this.cbDeinterlace.Name = "cbDeinterlace";
      this.cbDeinterlace.Size = new System.Drawing.Size(290, 21);
      this.cbDeinterlace.TabIndex = 15;
      // 
      // mpGroupBoxAdditional
      // 
      this.mpGroupBoxAdditional.Controls.Add(this.cbDeinterlace);
      this.mpGroupBoxAdditional.Controls.Add(this.label8);
      this.mpGroupBoxAdditional.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxAdditional.Location = new System.Drawing.Point(6, 0);
      this.mpGroupBoxAdditional.Name = "mpGroupBoxAdditional";
      this.mpGroupBoxAdditional.Size = new System.Drawing.Size(462, 63);
      this.mpGroupBoxAdditional.TabIndex = 16;
      this.mpGroupBoxAdditional.TabStop = false;
      this.mpGroupBoxAdditional.Text = "Additional settings";
       
       // 
       // gbRecording
       // 
       this.gbRecording.Controls.Add(this.btQuickRecordScheduleDown);
       this.gbRecording.Controls.Add(this.btQuickRecordScheduleUp);
       this.gbRecording.Controls.Add(this.lvIncludeScheduleTypes);
       this.gbRecording.Controls.Add(this.cbUseQuickRecordMenu);
       this.gbRecording.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
       this.gbRecording.Location = new System.Drawing.Point(4, 181);
       this.gbRecording.Name = "gbRecording";
       this.gbRecording.Size = new System.Drawing.Size(267, 243);
       this.gbRecording.TabIndex = 17;
       this.gbRecording.TabStop = false;
       this.gbRecording.Text = "Recording";
       
       // 
       // btQuickRecordScheduleDown
       // 
       this.btQuickRecordScheduleDown.Location = new System.Drawing.Point(225, 130);
       this.btQuickRecordScheduleDown.Name = "btQuickRecordScheduleDown";
       this.btQuickRecordScheduleDown.Size = new System.Drawing.Size(34, 23);
       this.btQuickRecordScheduleDown.TabIndex = 3;
       this.btQuickRecordScheduleDown.Text = "Dw";
       this.btQuickRecordScheduleDown.UseVisualStyleBackColor = true;
       this.btQuickRecordScheduleDown.Click += new System.EventHandler(this.btQuickRecordScheduleDown_Click);
       
       // 
       // btQuickRecordScheduleUp
       // 
       this.btQuickRecordScheduleUp.Location = new System.Drawing.Point(225, 101);
       this.btQuickRecordScheduleUp.Name = "btQuickRecordScheduleUp";
       this.btQuickRecordScheduleUp.Size = new System.Drawing.Size(34, 23);
       this.btQuickRecordScheduleUp.TabIndex = 2;
       this.btQuickRecordScheduleUp.Text = "Up";
       this.btQuickRecordScheduleUp.UseVisualStyleBackColor = true;
       this.btQuickRecordScheduleUp.Click += new System.EventHandler(this.btQuickRecordScheduleUp_Click);
        
       // 
       // lvIncludeScheduleTypes
       // 
       this.lvIncludeScheduleTypes.AllowDrop = true;
       this.lvIncludeScheduleTypes.AllowRowReorder = true;
       this.lvIncludeScheduleTypes.CheckBoxes = true;
       this.lvIncludeScheduleTypes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {this.clIncludedSchedules});
       this.lvIncludeScheduleTypes.FullRowSelect = true;
       this.lvIncludeScheduleTypes.Location = new System.Drawing.Point(16, 42);
       this.lvIncludeScheduleTypes.MultiSelect = false;
       this.lvIncludeScheduleTypes.Name = "lvIncludeScheduleTypes";
       this.lvIncludeScheduleTypes.Size = new System.Drawing.Size(203, 182);
       this.lvIncludeScheduleTypes.TabIndex = 1;
       this.lvIncludeScheduleTypes.UseCompatibleStateImageBehavior = false;
       this.lvIncludeScheduleTypes.View = System.Windows.Forms.View.Details;
       
       // 
       // clIncludedSchedules
       // 
       this.clIncludedSchedules.Text = "Include schedule types";
       this.clIncludedSchedules.Width = 198;
       
       // 
       // cbUseQuickRecordMenu
       // 
       this.cbUseQuickRecordMenu.AutoSize = true;
       this.cbUseQuickRecordMenu.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
       this.cbUseQuickRecordMenu.Location = new System.Drawing.Point(16, 19);
       this.cbUseQuickRecordMenu.Name = "cbUseQuickRecordMenu";
       this.cbUseQuickRecordMenu.Size = new System.Drawing.Size(191, 17);
       this.cbUseQuickRecordMenu.TabIndex = 0;
       this.cbUseQuickRecordMenu.Text = "Use quick record menu in TV guide";
       this.cbUseQuickRecordMenu.UseVisualStyleBackColor = true; 
      // 
      // TV
      // 
      this.Controls.Add(this.gbRecording);
      this.Controls.Add(this.mpGroupBoxAdditional);
      this.Controls.Add(this.groupBox5);
      this.Controls.Add(this.groupBox3);
      this.Name = "TV";
      this.Size = new System.Drawing.Size(472, 427);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberMaxLengthNumUpDn)).EndInit();
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.mpGroupBoxAdditional.ResumeLayout(false);
      this.gbRecording.ResumeLayout(false);
      this.gbRecording.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion

    public override void LoadSettings()
    {
      if (_init == false)
      {
        return;
      }

      using (Settings xmlreader = new MPSettings())
      {
        cbTurnOnTv.Checked = xmlreader.GetValueAsBool("mytv", "autoturnontv", false);
        cbAutoFullscreen.Checked = xmlreader.GetValueAsBool("mytv", "autofullscreen", false);
        byIndexCheckBox.Checked = xmlreader.GetValueAsBool("mytv", "byindex", true);
        showChannelNumberCheckBox.Checked = xmlreader.GetValueAsBool("mytv", "showchannelnumber", false);

        int channelNumberMaxLen = xmlreader.GetValueAsInt("mytv", "channelnumbermaxlength", 3);
        channelNumberMaxLengthNumUpDn.Value = channelNumberMaxLen;


        int DeInterlaceMode = xmlreader.GetValueAsInt("mytv", "deinterlace", 0);
        if (DeInterlaceMode < 0 || DeInterlaceMode > 3)
        {
          DeInterlaceMode = 3;
        }
        cbDeinterlace.SelectedIndex = DeInterlaceMode;
        //  Load quick record menu options
        cbUseQuickRecordMenu.Checked = xmlreader.GetValueAsBool("mytv", "usequickrecordmenu", false);
 
        string selectedSchedulesValue = xmlreader.GetValueAsString("mytv", "selectedquickrecordschedules", string.Empty);
 
        if (selectedSchedulesValue == null)
        selectedSchedulesValue = string.Empty;

        string[] scheduleOrder = xmlreader.GetValueAsString("mytv", "quickrecordscheduleorder", string.Empty).Split(new char[] { ',' });
        List<string> scheduleOrderList = new List<string>(scheduleOrder);
 
        if (!scheduleOrderList.Contains("Once")
         || !scheduleOrderList.Contains("Daily")
         || !scheduleOrderList.Contains("Weekly")
         || !scheduleOrderList.Contains("EveryTimeOnThisChannel")
         || !scheduleOrderList.Contains("EveryTimeOnEveryChannel")
         || !scheduleOrderList.Contains("Weekends")
         || !scheduleOrderList.Contains("WorkingDays")
         || !scheduleOrderList.Contains("WeeklyEveryTimeOnThisChannel")
         || !scheduleOrderList.Contains("SeriesLink")
           )
         {
           scheduleOrderList = new List<string>() { "Once", "Daily", "Weekly", "EveryTimeOnThisChannel", "EveryTimeOnEveryChannel", "Weekends", "WorkingDays", "WeeklyEveryTimeOnThisChannel", "SeriesLink" };
         }
 
         _listItemScheduleLookup.Clear();
         lvIncludeScheduleTypes.Items.Clear();
 
         string[] selectedSchedules = xmlreader.GetValueAsString("mytv", "selectedquickrecordschedules", string.Empty).Split(new char[] { ',' });
         List<string> selectedSchedulesList = new List<string>(selectedSchedules);
 
        //  Populate the list
         foreach (string scheduleItem in scheduleOrderList)
         {
           string scheduleDescription = string.Empty;
 
           switch (scheduleItem)
           {
             case "Once":
               scheduleDescription = "Once";
               break;
             case "Daily":
               scheduleDescription = "Daily";
               break;
             case "Weekly":
               scheduleDescription = "Weekly";
               break;
             case "EveryTimeOnThisChannel":
               scheduleDescription = "Every time on this channel";
               break;
             case "EveryTimeOnEveryChannel":
               scheduleDescription = "Every time on every channel";
               break;
             case "Weekends":
               scheduleDescription = "Saturday/Sunday";
               break;
             case "WorkingDays":
               scheduleDescription = "Monday-Friday";
               break;
             case "WeeklyEveryTimeOnThisChannel":
               scheduleDescription = "Weekly every time on this channel";
               break;
             case "SeriesLink":
               scheduleDescription = "Series link";
               break;
           }

          if (scheduleDescription == string.Empty)
             continue;
 
           ListViewItem newItem = new ListViewItem();
 
          newItem.Text = scheduleDescription;
 
 
           if (selectedSchedulesList.Contains(scheduleItem))
             newItem.Checked = true;
           else
             newItem.Checked = false;
 
           lvIncludeScheduleTypes.Items.Add(newItem);
           _listItemScheduleLookup.Add(newItem, scheduleItem);
         }
 
      }
    }

    public override void SaveSettings()
    {
      if (_init == false)
      {
        return;
      }
      using (Settings xmlwriter = new MPSettings())
      {
        if (cbDeinterlace.SelectedIndex >= 0)
        {
          xmlwriter.SetValue("mytv", "deinterlace", cbDeinterlace.SelectedIndex.ToString());
        }

        xmlwriter.SetValueAsBool("mytv", "autoturnontv", cbTurnOnTv.Checked);
        xmlwriter.SetValueAsBool("mytv", "autofullscreen", cbAutoFullscreen.Checked);
        xmlwriter.SetValueAsBool("mytv", "byindex", byIndexCheckBox.Checked);
        xmlwriter.SetValueAsBool("mytv", "showchannelnumber", showChannelNumberCheckBox.Checked);
        xmlwriter.SetValue("mytv", "channelnumbermaxlength", channelNumberMaxLengthNumUpDn.Value);
        //  Save quick record menu options
         xmlwriter.SetValueAsBool("mytv", "usequickrecordmenu", cbUseQuickRecordMenu.Checked);
 
         //  Build a list of selected items
         string selectedItems = string.Empty;
         string scheduleOrder = string.Empty;
 
         foreach (ListViewItem item in lvIncludeScheduleTypes.Items)
         {
           string scheduleName = _listItemScheduleLookup[item];
           scheduleOrder += (scheduleOrder != string.Empty ? "," : string.Empty) + scheduleName;
 
           if (item.Checked)
             selectedItems += (selectedItems != string.Empty ? "," : string.Empty) + scheduleName;
 
         }
 
         xmlwriter.SetValue("mytv", "quickrecordscheduleorder", scheduleOrder);
         xmlwriter.SetValue("mytv", "selectedquickrecordschedules", selectedItems);
 
      }
    }

    private void cbAutoFullscreen_CheckedChanged(object sender, EventArgs e)
    {
      if (cbAutoFullscreen.Checked)
      {
        cbTurnOnTv.Checked = true;
      }
    }
 
private void btQuickRecordScheduleUp_Click(object sender, EventArgs e)
    {
      if (lvIncludeScheduleTypes.SelectedItems.Count != 1)
         return;
 
       ListViewItem selectedItem = lvIncludeScheduleTypes.SelectedItems[0];
 
       int selectedItemIndex = lvIncludeScheduleTypes.Items.IndexOf(selectedItem);
 
       lvIncludeScheduleTypes.Items.RemoveAt(selectedItemIndex);
 
       selectedItemIndex--;
       if (selectedItemIndex < 0)
         selectedItemIndex = 0;
 
       lvIncludeScheduleTypes.Items.Insert(selectedItemIndex, selectedItem);
       selectedItem.Selected = true;
       lvIncludeScheduleTypes.Select();
     }
 
     private void btQuickRecordScheduleDown_Click(object sender, EventArgs e)
     {
       if (lvIncludeScheduleTypes.SelectedItems.Count != 1)
         return;
 
       ListViewItem selectedItem = lvIncludeScheduleTypes.SelectedItems[0];
 
       int selectedItemIndex = lvIncludeScheduleTypes.Items.IndexOf(selectedItem);
 
       lvIncludeScheduleTypes.Items.RemoveAt(selectedItemIndex);
 
       selectedItemIndex++;
       if (selectedItemIndex > lvIncludeScheduleTypes.Items.Count)
         selectedItemIndex = lvIncludeScheduleTypes.Items.Count;
 
       lvIncludeScheduleTypes.Items.Insert(selectedItemIndex, selectedItem);
       selectedItem.Selected = true;
       lvIncludeScheduleTypes.Select();
     }

  }
}