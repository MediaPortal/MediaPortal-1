#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Diagnostics;
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class GeneralDynamicRefreshRate : SectionSettings
  {
    private MPGroupBox groupBoxRR;
    private LinkLabel linkLabel1;
    private MPCheckBox chkEnableDynamicRR;
    private MPCheckBox chkNotifyOnRR;
    private MPCheckBox chkForceRR;
    private MPCheckBox chkUseDeviceReset;
    private MPCheckBox chkUseDefaultRR;
    private DataGridView dataGridViewRR;
    private MPLabel lblDescription;
    private MPTextBox txtDefaultHz;
    private DataGridViewTextBoxColumn gridColType;
    private DataGridViewTextBoxColumn gridColFramerates;
    private DataGridViewTextBoxColumn gridColRR;
    private DataGridViewTextBoxColumn gridColAction;
    private new IContainer components = null;

    public GeneralDynamicRefreshRate()
      : this("Dynamic Refresh Rate")
    {
    }

    public GeneralDynamicRefreshRate(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      this.linkLabel1.Links.Add(0, linkLabel1.Text.Length, "http://wiki.team-mediaportal.com/");
      dataGridViewRR.Rows.Clear();
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

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        chkEnableDynamicRR.Checked = xmlreader.GetValueAsBool("general", "autochangerefreshrate", false);
        chkNotifyOnRR.Checked = xmlreader.GetValueAsBool("general", "notify_on_refreshrate", false);
        chkUseDefaultRR.Checked = xmlreader.GetValueAsBool("general", "use_default_hz", false);
        chkUseDeviceReset.Checked = xmlreader.GetValueAsBool("general", "devicereset", false);
        chkForceRR.Checked = xmlreader.GetValueAsBool("general", "force_refresh_rate", false);

        txtDefaultHz.Text = xmlreader.GetValueAsString("general", "default_hz", "");

        string cinemaFPS = xmlreader.GetValueAsString("general", "cinema_fps", "23.976;24");
        string palFPS = xmlreader.GetValueAsString("general", "pal_fps", "25");
        string ntscFPS = xmlreader.GetValueAsString("general", "ntsc_fps", "29.97;30");
        string tvFPS = xmlreader.GetValueAsString("general", "tv_fps", "25");

        string cinemaExtCmd = xmlreader.GetValueAsString("general", "cinema_ext", "");
        string palExtCmd = xmlreader.GetValueAsString("general", "pal_ext", "");
        string ntscExtCmd = xmlreader.GetValueAsString("general", "ntsc_ext", "");
        string tvExtCmd = xmlreader.GetValueAsString("general", "tv_ext", "");

        string cinemaHz = xmlreader.GetValueAsString("general", "cinema_hz", "24");
        string palHz = xmlreader.GetValueAsString("general", "pal_hz", "50");
        string ntscHz = xmlreader.GetValueAsString("general", "ntsc_hz", "60");
        string tvHz = xmlreader.GetValueAsString("general", "tv_hz", "50");

        String[] parameters = new String[4];
        parameters[0] = "CINEMA";
        parameters[1] = cinemaFPS; // fps
        parameters[2] = cinemaHz; //hz
        parameters[3] = cinemaExtCmd; //action
        dataGridViewRR.Rows.Add((object[]) parameters);

        parameters = new String[4];
        parameters[0] = "PAL";
        parameters[1] = palFPS; // fps
        parameters[2] = palHz; //hz
        parameters[3] = palExtCmd; //action
        dataGridViewRR.Rows.Add((object[]) parameters);

        parameters = new String[4];
        parameters[0] = "NTSC";
        parameters[1] = ntscFPS; // fps
        parameters[2] = ntscHz; //hz
        parameters[3] = ntscExtCmd; //action
        dataGridViewRR.Rows.Add((object[]) parameters);

        parameters = new String[4];
        parameters[0] = "TV";
        parameters[1] = tvFPS; // fps
        parameters[2] = tvHz; //hz
        parameters[3] = tvExtCmd; //action
        dataGridViewRR.Rows.Add((object[]) parameters);
      }

      updateStates();
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("general", "autochangerefreshrate", chkEnableDynamicRR.Checked);
        xmlwriter.SetValueAsBool("general", "notify_on_refreshrate", chkNotifyOnRR.Checked);
        xmlwriter.SetValueAsBool("general", "use_default_hz", chkUseDefaultRR.Checked);
        xmlwriter.SetValueAsBool("general", "devicereset", chkUseDeviceReset.Checked);
        xmlwriter.SetValueAsBool("general", "force_refresh_rate", chkForceRR.Checked);
        xmlwriter.SetValue("general", "default_hz", txtDefaultHz.Text);

        string cinemaFPS = "";
        string palFPS = "";
        string ntscFPS = "";
        string tvFPS = "";

        string cinemaExtCmd = "";
        string palExtCmd = "";
        string ntscExtCmd = "";
        string tvExtCmd = "";

        string cinemaHz = "";
        string palHz = "";
        string ntscHz = "";
        string tvHz = "";

        cinemaFPS = (string) dataGridViewRR.Rows[0].Cells[1].Value;
        cinemaHz = (string) dataGridViewRR.Rows[0].Cells[2].Value;
        cinemaExtCmd = (string) dataGridViewRR.Rows[0].Cells[3].Value;

        palFPS = (string) dataGridViewRR.Rows[1].Cells[1].Value;
        palHz = (string) dataGridViewRR.Rows[1].Cells[2].Value;
        palExtCmd = (string) dataGridViewRR.Rows[1].Cells[3].Value;

        ntscFPS = (string) dataGridViewRR.Rows[2].Cells[1].Value;
        ntscHz = (string) dataGridViewRR.Rows[2].Cells[2].Value;
        ntscExtCmd = (string) dataGridViewRR.Rows[2].Cells[3].Value;

        tvFPS = (string) dataGridViewRR.Rows[3].Cells[1].Value;
        tvHz = (string) dataGridViewRR.Rows[3].Cells[2].Value;
        tvExtCmd = (string) dataGridViewRR.Rows[3].Cells[3].Value;


        xmlwriter.SetValue("general", "cinema_fps", cinemaFPS);
        xmlwriter.SetValue("general", "cinema_hz", cinemaHz);
        xmlwriter.SetValue("general", "cinema_ext", cinemaExtCmd);

        xmlwriter.SetValue("general", "pal_fps", palFPS);
        xmlwriter.SetValue("general", "pal_hz", palHz);
        xmlwriter.SetValue("general", "pal_ext", palExtCmd);

        xmlwriter.SetValue("general", "ntsc_fps", ntscFPS);
        xmlwriter.SetValue("general", "ntsc_hz", ntscHz);
        xmlwriter.SetValue("general", "ntsc_ext", ntscExtCmd);

        xmlwriter.SetValue("general", "tv_fps", tvFPS);
        xmlwriter.SetValue("general", "tv_ext", tvExtCmd);
        xmlwriter.SetValue("general", "tv_hz", tvHz);
      }
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start((string) e.Link.LinkData);
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources =
        new System.ComponentModel.ComponentResourceManager(typeof (GeneralDynamicRefreshRate));
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 =
        new System.Windows.Forms.DataGridViewCellStyle();
      this.groupBoxRR = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.txtDefaultHz = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblDescription = new MediaPortal.UserInterface.Controls.MPLabel();
      this.dataGridViewRR = new System.Windows.Forms.DataGridView();
      this.gridColType = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.gridColFramerates = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.gridColRR = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.gridColAction = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.chkUseDefaultRR = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.chkForceRR = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.chkUseDeviceReset = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.chkNotifyOnRR = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.chkEnableDynamicRR = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.groupBoxRR.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (this.dataGridViewRR)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxRR
      // 
      this.groupBoxRR.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxRR.Controls.Add(this.txtDefaultHz);
      this.groupBoxRR.Controls.Add(this.lblDescription);
      this.groupBoxRR.Controls.Add(this.dataGridViewRR);
      this.groupBoxRR.Controls.Add(this.chkUseDefaultRR);
      this.groupBoxRR.Controls.Add(this.chkForceRR);
      this.groupBoxRR.Controls.Add(this.chkUseDeviceReset);
      this.groupBoxRR.Controls.Add(this.chkNotifyOnRR);
      this.groupBoxRR.Controls.Add(this.chkEnableDynamicRR);
      this.groupBoxRR.Controls.Add(this.linkLabel1);
      this.groupBoxRR.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxRR.Location = new System.Drawing.Point(3, 3);
      this.groupBoxRR.Name = "groupBoxRR";
      this.groupBoxRR.Size = new System.Drawing.Size(460, 402);
      this.groupBoxRR.TabIndex = 7;
      this.groupBoxRR.TabStop = false;
      this.groupBoxRR.Text = "Dynamic Refresh Rate Setup";
      // 
      // txtDefaultHz
      // 
      this.txtDefaultHz.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.txtDefaultHz.BorderColor = System.Drawing.Color.Empty;
      this.txtDefaultHz.Location = new System.Drawing.Point(245, 350);
      this.txtDefaultHz.Name = "txtDefaultHz";
      this.txtDefaultHz.Size = new System.Drawing.Size(208, 20);
      this.txtDefaultHz.TabIndex = 19;
      // 
      // lblDescription
      // 
      this.lblDescription.AutoSize = true;
      this.lblDescription.Location = new System.Drawing.Point(16, 25);
      this.lblDescription.Name = "lblDescription";
      this.lblDescription.Size = new System.Drawing.Size(386, 117);
      this.lblDescription.TabIndex = 18;
      this.lblDescription.Text = resources.GetString("lblDescription.Text");
      // 
      // dataGridViewRR
      // 
      this.dataGridViewRR.AllowUserToAddRows = false;
      this.dataGridViewRR.AllowUserToDeleteRows = false;
      this.dataGridViewRR.ColumnHeadersHeightSizeMode =
        System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridViewRR.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
                                             {
                                               this.gridColType,
                                               this.gridColFramerates,
                                               this.gridColRR,
                                               this.gridColAction
                                             });
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F,
                                                            System.Drawing.FontStyle.Regular,
                                                            System.Drawing.GraphicsUnit.Point, ((byte) (0)));
      dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.dataGridViewRR.DefaultCellStyle = dataGridViewCellStyle1;
      this.dataGridViewRR.Location = new System.Drawing.Point(19, 193);
      this.dataGridViewRR.Name = "dataGridViewRR";
      this.dataGridViewRR.Size = new System.Drawing.Size(434, 147);
      this.dataGridViewRR.TabIndex = 17;
      // 
      // gridColType
      // 
      this.gridColType.Frozen = true;
      this.gridColType.HeaderText = "Type";
      this.gridColType.Name = "gridColType";
      this.gridColType.ReadOnly = true;
      this.gridColType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // gridColFramerates
      // 
      this.gridColFramerates.HeaderText = "Framerates";
      this.gridColFramerates.Name = "gridColFramerates";
      this.gridColFramerates.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // gridColRR
      // 
      this.gridColRR.HeaderText = "Refreshrate";
      this.gridColRR.Name = "gridColRR";
      this.gridColRR.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // gridColAction
      // 
      this.gridColAction.HeaderText = "Action";
      this.gridColAction.Name = "gridColAction";
      this.gridColAction.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // chkUseDefaultRR
      // 
      this.chkUseDefaultRR.AutoSize = true;
      this.chkUseDefaultRR.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkUseDefaultRR.Location = new System.Drawing.Point(19, 350);
      this.chkUseDefaultRR.Name = "chkUseDefaultRR";
      this.chkUseDefaultRR.Size = new System.Drawing.Size(134, 17);
      this.chkUseDefaultRR.TabIndex = 15;
      this.chkUseDefaultRR.Text = "Use default refreshrate.";
      this.chkUseDefaultRR.UseVisualStyleBackColor = true;
      this.chkUseDefaultRR.CheckedChanged += new System.EventHandler(this.chkUseDefaultRR_CheckedChanged);
      // 
      // chkForceRR
      // 
      this.chkForceRR.AutoSize = true;
      this.chkForceRR.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkForceRR.Location = new System.Drawing.Point(237, 170);
      this.chkForceRR.Name = "chkForceRR";
      this.chkForceRR.Size = new System.Drawing.Size(146, 17);
      this.chkForceRR.TabIndex = 14;
      this.chkForceRR.Text = "Force refreshrate change.";
      this.chkForceRR.UseVisualStyleBackColor = true;
      // 
      // chkUseDeviceReset
      // 
      this.chkUseDeviceReset.AutoSize = true;
      this.chkUseDeviceReset.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkUseDeviceReset.Location = new System.Drawing.Point(237, 147);
      this.chkUseDeviceReset.Name = "chkUseDeviceReset";
      this.chkUseDeviceReset.Size = new System.Drawing.Size(107, 17);
      this.chkUseDeviceReset.TabIndex = 13;
      this.chkUseDeviceReset.Text = "Use device reset.";
      this.chkUseDeviceReset.UseVisualStyleBackColor = true;
      // 
      // chkNotifyOnRR
      // 
      this.chkNotifyOnRR.AutoSize = true;
      this.chkNotifyOnRR.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkNotifyOnRR.Location = new System.Drawing.Point(19, 170);
      this.chkNotifyOnRR.Name = "chkNotifyOnRR";
      this.chkNotifyOnRR.Size = new System.Drawing.Size(200, 17);
      this.chkNotifyOnRR.TabIndex = 12;
      this.chkNotifyOnRR.Text = "Notify (popup) on refreshrate change.";
      this.chkNotifyOnRR.UseVisualStyleBackColor = true;
      // 
      // chkEnableDynamicRR
      // 
      this.chkEnableDynamicRR.AutoSize = true;
      this.chkEnableDynamicRR.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkEnableDynamicRR.Location = new System.Drawing.Point(19, 147);
      this.chkEnableDynamicRR.Name = "chkEnableDynamicRR";
      this.chkEnableDynamicRR.Size = new System.Drawing.Size(206, 17);
      this.chkEnableDynamicRR.TabIndex = 11;
      this.chkEnableDynamicRR.Text = "Enable Dynamic Refresh Rate Control.";
      this.chkEnableDynamicRR.UseVisualStyleBackColor = true;
      this.chkEnableDynamicRR.CheckedChanged += new System.EventHandler(this.chkEnableDynamicRR_CheckedChanged);
      // 
      // linkLabel1
      // 
      this.linkLabel1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(16, 375);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(112, 13);
      this.linkLabel1.TabIndex = 10;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "more info in the wiki ...";
      // 
      // GeneralDynamicRefreshRate
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.groupBoxRR);
      this.Name = "GeneralDynamicRefreshRate";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxRR.ResumeLayout(false);
      this.groupBoxRR.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) (this.dataGridViewRR)).EndInit();
      this.ResumeLayout(false);
    }

    #endregion

    private void chkEnableDynamicRR_CheckedChanged(object sender, EventArgs e)
    {
      updateStates();
    }

    private void chkUseDefaultRR_CheckedChanged(object sender, EventArgs e)
    {
      updateStates();
    }

    private void updateStates()
    {
      chkUseDeviceReset.Enabled = chkEnableDynamicRR.Checked;
      chkForceRR.Enabled = chkEnableDynamicRR.Checked;
      chkNotifyOnRR.Enabled = chkEnableDynamicRR.Checked;
      chkUseDefaultRR.Enabled = chkEnableDynamicRR.Checked;
      dataGridViewRR.Enabled = chkEnableDynamicRR.Checked;
      txtDefaultHz.Enabled = chkEnableDynamicRR.Checked;

      if (chkEnableDynamicRR.Checked)
      {
        txtDefaultHz.Enabled = chkUseDefaultRR.Checked;
      }
    }
  }
}