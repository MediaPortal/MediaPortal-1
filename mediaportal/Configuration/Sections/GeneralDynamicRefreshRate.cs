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
using System.Collections.Generic;
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
    private MPButton buttonRemove;
    private MPButton buttonAdd;
    private DataGridViewTextBoxColumn gridColType;
    private DataGridViewTextBoxColumn gridColFramerates;
    private DataGridViewTextBoxColumn gridColRR;
    private DataGridViewTextBoxColumn gridColAction;
    private MPButton mpButtonDefault;
    private new IContainer components = null;
    private bool _ignoreCellValueChangedEvent = false;

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
        String[] p = null;
        DataGridViewRow row = new DataGridViewRow();
        /*
        // example
        <entry name="refreshrate01_ext">C:\\Program Files\\displaychanger\\dc.exe -refreshrate=60 -apply -quiet</entry>
        <entry name="refreshrate01_name">NTSC</entry>
        <entry name="ntsc_fps">29.97;30</entry>
        <entry name="ntsc_hz">60</entry>
        */

        //System.Diagnostics.Debugger.Launch();

        for (int i = 1; i < 100; i++)
        {
          string extCmd = xmlreader.GetValueAsString("general", "refreshrate0" + Convert.ToString (i) + "_ext", "");
          string name = xmlreader.GetValueAsString("general", "refreshrate0" + Convert.ToString(i) + "_name", "");          

          if (string.IsNullOrEmpty(name))
          {
            continue;
          }          

          string fps  = xmlreader.GetValueAsString("general", name + "_fps", "");
          string hz = xmlreader.GetValueAsString("general", name + "_hz", "");

          p = new String[4];
          p[0] = name;
          p[1] = fps; // fps
          p[2] = hz; //hz
          p[3] = extCmd; //action
          dataGridViewRR.Rows.Add((object[])p);
          row = dataGridViewRR.Rows[dataGridViewRR.Rows.Count - 1];          

          if (name.ToLower().IndexOf("tv") > -1)
          {            
            row.Cells[0].ReadOnly = true;
          }
        }
        
        if (dataGridViewRR.Rows.Count == 0)
        {
          InsertDefaultValues();          
        }                        
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

        /*
        // example
        <entry name="refreshrate01_ext">C:\\Program Files\\displaychanger\\dc.exe -refreshrate=60 -apply -quiet</entry>
        <entry name="refreshrate01_name">NTSC</entry>
        <entry name="ntsc_fps">29.97;30</entry>
        <entry name="ntsc_hz">60</entry>
        */

        //delete all refreshrate entries, then re-add them.
        Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));
        for (int i = 1; i < 100; i++)
        {          
          string name = xmlreader.GetValueAsString("general", "refreshrate0" + Convert.ToString(i) + "_name", "");

          if (string.IsNullOrEmpty(name))
          {
            continue;
          }

          xmlwriter.RemoveEntry("general", name + "_fps");
          xmlwriter.RemoveEntry("general", name + "_hz");
          xmlwriter.RemoveEntry("general", "refreshrate0" + Convert.ToString(i) + "_ext");
          xmlwriter.RemoveEntry("general", "refreshrate0" + Convert.ToString(i) + "_name");
        }       

        int j = 1;
        foreach (DataGridViewRow row in dataGridViewRR.Rows)
        {          
          string name = (string)row.Cells[0].Value;
          string fps = (string)row.Cells[1].Value;
          string hz = (string)row.Cells[2].Value;
          string extCmd = (string)row.Cells[3].Value;
          
          xmlwriter.SetValue("general", name + "_fps", fps);
          xmlwriter.SetValue("general", name + "_hz", hz);
          xmlwriter.SetValue("general", "refreshrate0" + Convert.ToString(j) + "_ext", extCmd);
          xmlwriter.SetValue("general", "refreshrate0" + Convert.ToString(j) + "_name", name);
          j++;
        }        
               
        
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GeneralDynamicRefreshRate));
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      this.groupBoxRR = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpButtonDefault = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonRemove = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonAdd = new MediaPortal.UserInterface.Controls.MPButton();
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
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRR)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxRR
      // 
      this.groupBoxRR.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxRR.Controls.Add(this.mpButtonDefault);
      this.groupBoxRR.Controls.Add(this.buttonRemove);
      this.groupBoxRR.Controls.Add(this.buttonAdd);
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
      // mpButtonDefault
      // 
      this.mpButtonDefault.Location = new System.Drawing.Point(147, 314);
      this.mpButtonDefault.Name = "mpButtonDefault";
      this.mpButtonDefault.Size = new System.Drawing.Size(58, 23);
      this.mpButtonDefault.TabIndex = 22;
      this.mpButtonDefault.Text = "Default";
      this.mpButtonDefault.UseVisualStyleBackColor = true;
      this.mpButtonDefault.Click += new System.EventHandler(this.mpButtonDefault_Click);
      // 
      // buttonRemove
      // 
      this.buttonRemove.Location = new System.Drawing.Point(83, 314);
      this.buttonRemove.Name = "buttonRemove";
      this.buttonRemove.Size = new System.Drawing.Size(58, 23);
      this.buttonRemove.TabIndex = 21;
      this.buttonRemove.Text = "Remove";
      this.buttonRemove.UseVisualStyleBackColor = true;
      this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
      // 
      // buttonAdd
      // 
      this.buttonAdd.Location = new System.Drawing.Point(19, 314);
      this.buttonAdd.Name = "buttonAdd";
      this.buttonAdd.Size = new System.Drawing.Size(58, 23);
      this.buttonAdd.TabIndex = 20;
      this.buttonAdd.Text = "Add";
      this.buttonAdd.UseVisualStyleBackColor = true;
      this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
      // 
      // txtDefaultHz
      // 
      this.txtDefaultHz.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
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
      this.dataGridViewRR.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridViewRR.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.gridColType,
            this.gridColFramerates,
            this.gridColRR,
            this.gridColAction});
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.dataGridViewRR.DefaultCellStyle = dataGridViewCellStyle1;
      this.dataGridViewRR.Location = new System.Drawing.Point(19, 193);
      this.dataGridViewRR.Name = "dataGridViewRR";
      this.dataGridViewRR.RowHeadersVisible = false;
      this.dataGridViewRR.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridViewRR.Size = new System.Drawing.Size(434, 115);
      this.dataGridViewRR.TabIndex = 17;
      this.dataGridViewRR.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewRR_CellValueChanged);
      
      // 
      // gridColType
      // 
      this.gridColType.Frozen = true;
      this.gridColType.HeaderText = "Name";
      this.gridColType.Name = "gridColType";
      this.gridColType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // gridColFramerates
      // 
      this.gridColFramerates.HeaderText = "Framerate(s)";
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
      this.gridColAction.Width = 130;
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
      this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
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
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRR)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion
   
    private void InsertDefaultValues()
    {
      Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));
      //first time mp config is run, no refreshrate settings available, create the default ones.
      string[] p = new String[4];
      p[0] = "CINEMA";
      p[1] = "23.976;24"; // fps
      p[2] = "24"; //hz
      p[3] = ""; //action
      dataGridViewRR.Rows.Add((object[])p);

      p = new String[4];
      p[0] = "PAL";
      p[1] = "25"; // fps
      p[2] = "50"; //hz
      p[3] = ""; //action
      dataGridViewRR.Rows.Add((object[])p);

      p = new String[4];
      p[0] = "NTSC";
      p[1] = "29.97;30"; // fps
      p[2] = "60"; //hz
      p[3] = ""; //action
      dataGridViewRR.Rows.Add((object[])p);

      //tv section is not editable, it's static.
      string tvExtCmd = xmlreader.GetValueAsString("general", "refreshrateTV_ext", "");
      string tvName = xmlreader.GetValueAsString("general", "refreshrateTV_name", "PAL");
      string tvFPS = xmlreader.GetValueAsString("general", "tv_fps", "25");
      string tvHz = xmlreader.GetValueAsString("general", "tv_hz", "50");

      String[] parameters = new String[4];
      parameters = new String[4];
      parameters[0] = "TV";
      parameters[1] = tvFPS; // fps
      parameters[2] = tvHz; //hz
      parameters[3] = tvExtCmd; //action
      dataGridViewRR.Rows.Add((object[])parameters);
      DataGridViewRow row = dataGridViewRR.Rows[dataGridViewRR.Rows.Count - 1];
      row.Cells[0].ReadOnly = true;
    }

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

    private void buttonAdd_Click(object sender, EventArgs e)
    {
      DataGridViewRow row = new DataGridViewRow();

      row.ReadOnly = false;            
      dataGridViewRR.Rows.Add(row);      
    }

    private void buttonRemove_Click(object sender, EventArgs e)
    {
      if (dataGridViewRR.Rows.Count == 0)
      {
        return;
      }

      DataGridViewSelectedRowCollection sel = dataGridViewRR.SelectedRows;
      if (sel.Count == 0)
      {
        return;
      }

      foreach (DataGridViewRow row in sel)
      {        
        if (row.Cells[0].Value.ToString().ToLower().IndexOf("tv") > -1)
        {
          continue;
        }        

        dataGridViewRR.Rows.Remove(row);
      }      

    }

    private void mpButtonDefault_Click(object sender, EventArgs e)
    {
      dataGridViewRR.Rows.Clear();
      InsertDefaultValues();
    }

    

    private void dataGridViewRR_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {

      if (_ignoreCellValueChangedEvent)
      {
        return;
      }

      if (dataGridViewRR.CurrentCell == null || dataGridViewRR.CurrentCell.Value == null)
      {
        return;
      }

      string currentValue = dataGridViewRR.CurrentCell.Value.ToString();

      int i = 0;
      foreach (DataGridViewRow row in dataGridViewRR.Rows)
      {
        if (row.Cells[0].Value != null && row.Cells[0].Value.ToString().ToLower().Equals(currentValue) && i != dataGridViewRR.CurrentCell.RowIndex)
        {
          MessageBox.Show("Please do not add the same name twice. They most be unique.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          _ignoreCellValueChangedEvent = true;
          dataGridViewRR.Rows.Remove(dataGridViewRR.Rows[dataGridViewRR.CurrentCell.RowIndex]);
          _ignoreCellValueChangedEvent = false;
        }
        i++;
      }      
    }    
   
  }
}