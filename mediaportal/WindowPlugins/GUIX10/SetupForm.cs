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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.GUI.X10Plugin
{
  /// <summary>
  /// Summary description for SetupForm.
  /// </summary>
  public class SetupForm : System.Windows.Forms.Form
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public SetupForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
    }

    private MediaPortal.UserInterface.Controls.MPButton buttonSave;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private System.Windows.Forms.ListView listLocations;
    public System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private MediaPortal.UserInterface.Controls.MPButton buttonNew;
    private MediaPortal.UserInterface.Controls.MPButton buttonEdit;
    private MediaPortal.UserInterface.Controls.MPButton buttonDelete;
    private MediaPortal.UserInterface.Controls.MPLabel label2;

    private MediaPortal.UserInterface.Controls.MPTextBox txtCM1xHost;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButton1;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButton2;
    private MediaPortal.UserInterface.Controls.MPTextBox txtComPort;
    private MediaPortal.UserInterface.Controls.MPLabel label3;

    private ApplianceConfiguration appConfig;

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

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.buttonSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonDelete = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonEdit = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonNew = new MediaPortal.UserInterface.Controls.MPButton();
      this.listLocations = new System.Windows.Forms.ListView();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtCM1xHost = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButton1 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButton2 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.txtComPort = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonSave
      // 
      this.buttonSave.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonSave.Location = new System.Drawing.Point(344, 296);
      this.buttonSave.Name = "buttonSave";
      this.buttonSave.Size = new System.Drawing.Size(64, 24);
      this.buttonSave.TabIndex = 1;
      this.buttonSave.Text = "Save";
      this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.buttonDelete);
      this.groupBox1.Controls.Add(this.buttonEdit);
      this.groupBox1.Controls.Add(this.buttonNew);
      this.groupBox1.Controls.Add(this.listLocations);
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(400, 208);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "X10 Appliances";
      // 
      // buttonDelete
      // 
      this.buttonDelete.Location = new System.Drawing.Point(240, 168);
      this.buttonDelete.Name = "buttonDelete";
      this.buttonDelete.Size = new System.Drawing.Size(64, 24);
      this.buttonDelete.TabIndex = 3;
      this.buttonDelete.Text = "Delete";
      this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
      // 
      // buttonEdit
      // 
      this.buttonEdit.Location = new System.Drawing.Point(160, 168);
      this.buttonEdit.Name = "buttonEdit";
      this.buttonEdit.Size = new System.Drawing.Size(64, 24);
      this.buttonEdit.TabIndex = 2;
      this.buttonEdit.Text = "Edit";
      this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
      // 
      // buttonNew
      // 
      this.buttonNew.Location = new System.Drawing.Point(80, 168);
      this.buttonNew.Name = "buttonNew";
      this.buttonNew.Size = new System.Drawing.Size(64, 24);
      this.buttonNew.TabIndex = 1;
      this.buttonNew.Text = "New";
      this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
      // 
      // listLocations
      // 
      this.listLocations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this.columnHeader2,
																							this.columnHeader3,
																							this.columnHeader1});
      this.listLocations.FullRowSelect = true;
      this.listLocations.HideSelection = false;
      this.listLocations.Location = new System.Drawing.Point(16, 24);
      this.listLocations.MultiSelect = false;
      this.listLocations.Name = "listLocations";
      this.listLocations.Size = new System.Drawing.Size(376, 136);
      this.listLocations.TabIndex = 0;
      this.listLocations.View = System.Windows.Forms.View.Details;
      this.listLocations.SelectedIndexChanged += new System.EventHandler(this.listLocations_SelectedIndexChanged);
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Code";
      this.columnHeader2.Width = 103;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Description";
      this.columnHeader3.Width = 165;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Location";
      this.columnHeader1.Width = 103;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 296);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(208, 24);
      this.label2.TabIndex = 3;
      this.label2.Text = "version 0.2 - Designed by Nopap";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtCM1xHost
      // 
      this.txtCM1xHost.Location = new System.Drawing.Point(304, 232);
      this.txtCM1xHost.Name = "txtCM1xHost";
      this.txtCM1xHost.Size = new System.Drawing.Size(96, 20);
      this.txtCM1xHost.TabIndex = 4;
      this.txtCM1xHost.Text = "";
      this.txtCM1xHost.TextChanged += new System.EventHandler(this.txtCM1xHost_TextChanged);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(88, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(208, 24);
      this.label1.TabIndex = 5;
      this.label1.Text = "CM11 Host, running xAP CM11 service. Computer name only, no domain:";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.txtComPort);
      this.groupBox2.Controls.Add(this.radioButton2);
      this.groupBox2.Controls.Add(this.radioButton1);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Location = new System.Drawing.Point(8, 216);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(400, 72);
      this.groupBox2.TabIndex = 6;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "CM Device";
      // 
      // radioButton1
      // 
      this.radioButton1.Location = new System.Drawing.Point(8, 16);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(72, 24);
      this.radioButton1.TabIndex = 6;
      this.radioButton1.Text = "CM11";
      this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
      // 
      // radioButton2
      // 
      this.radioButton2.Location = new System.Drawing.Point(8, 40);
      this.radioButton2.Name = "radioButton2";
      this.radioButton2.Size = new System.Drawing.Size(69, 24);
      this.radioButton2.TabIndex = 7;
      this.radioButton2.Text = "CM17";
      this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
      // 
      // txtComPort
      // 
      this.txtComPort.Location = new System.Drawing.Point(296, 48);
      this.txtComPort.Name = "txtComPort";
      this.txtComPort.Size = new System.Drawing.Size(96, 20);
      this.txtComPort.TabIndex = 8;
      this.txtComPort.Text = "";
      this.txtComPort.TextChanged += new System.EventHandler(this.txtComPort_TextChanged);
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(88, 48);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(208, 16);
      this.label3.TabIndex = 9;
      this.label3.Text = "CM17 COM port";
      // 
      // SetupForm
      // 
      this.AcceptButton = this.buttonSave;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.buttonSave;
      this.ClientSize = new System.Drawing.Size(416, 326);
      this.Controls.Add(this.txtCM1xHost);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.buttonSave);
      this.Controls.Add(this.groupBox2);
      this.Name = "SetupForm";
      this.Text = "X10 Plugin - Settings";
      this.Load += new System.EventHandler(this.SetupForm_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private void SetupForm_Load(object sender, System.EventArgs e)
    {
      appConfig = new ApplianceConfiguration();
      appConfig.LoadSettings();

      foreach (X10Appliance sx10 in appConfig.m_X10Appliances)
      {
        ListViewItem item = null;
        item = listLocations.Items.Add(sx10.m_strCode);
        item.SubItems.Add(sx10.m_strDescription);
        item.SubItems.Add(sx10.m_location);
      }
      listLocations_SelectedIndexChanged(sender, e);

      txtCM1xHost.Text = appConfig.m_CM1xHost;

      if (appConfig.m_CMDevice == (int)SendX10.CMDevices.CM17)
      {
        radioButton2.Checked = true;
        radioButton1.Checked = false;
      }
      else
      {
        radioButton2.Checked = false;
        radioButton1.Checked = true;
      }
      txtComPort.Text = appConfig.m_CM17COMPort.ToString();
    }

    private void listLocations_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (listLocations.SelectedItems.Count == 0)
      {
        buttonEdit.Enabled = false;
        buttonDelete.Enabled = false;

        return;
      }
      int iItem = listLocations.SelectedIndices[0];
      buttonEdit.Enabled = true;
      buttonDelete.Enabled = true;
    }

    private void buttonSave_Click(object sender, System.EventArgs e)
    {
      appConfig.SaveSettings();
      this.Close();
    }

    private void buttonNew_Click(object sender, System.EventArgs e)
    {
      ApplianceForm appForm = new ApplianceForm();
      if (appForm.ShowDialog() == DialogResult.OK)
      {
        ListViewItem item = null;

        item = listLocations.Items.Add(appForm.txtCode.Text);
        item.SubItems.Add(appForm.txtDescription.Text);
        item.SubItems.Add(appForm.txtLocation.Text);

        X10Appliance sx10 = new X10Appliance();
        sx10.m_strCode = appForm.txtCode.Text;
        sx10.m_strDescription = appForm.txtDescription.Text;
        sx10.m_location = appForm.txtLocation.Text;
        appConfig.m_X10Appliances.Add(sx10);
      }
      appForm.Dispose();
    }

    private void buttonEdit_Click(object sender, System.EventArgs e)
    {
      if (listLocations.SelectedItems.Count >= 0)
      {
        ListViewItem item = listLocations.Items[(int)listLocations.SelectedIndices[0]];

        ApplianceForm appForm = new ApplianceForm();
        X10Appliance sx10 = (X10Appliance)appConfig.m_X10Appliances[(int)listLocations.SelectedIndices[0]];
        appForm.txtCode.Text = sx10.m_strCode;
        appForm.txtDescription.Text = sx10.m_strDescription;
        appForm.txtLocation.Text = sx10.m_location;

        if (appForm.ShowDialog() == DialogResult.OK)
        {
          item.SubItems[0].Text = appForm.txtCode.Text;
          item.SubItems[1].Text = appForm.txtDescription.Text;
          item.SubItems[2].Text = appForm.txtLocation.Text;

          sx10.m_strCode = appForm.txtCode.Text;
          sx10.m_strDescription = appForm.txtDescription.Text;
          sx10.m_location = appForm.txtLocation.Text;
        }
        appForm.Dispose();
      }
    }

    private void buttonDelete_Click(object sender, System.EventArgs e)
    {
      if (listLocations.SelectedItems.Count == 0)
      {
        listLocations.Items.RemoveAt((int)listLocations.SelectedIndices[0]);
        appConfig.m_X10Appliances.RemoveAt((int)listLocations.SelectedIndices[0]);

        listLocations.Refresh();
      }
    }

    private void txtCM1xHost_TextChanged(object sender, System.EventArgs e)
    {
      appConfig.m_CM1xHost = txtCM1xHost.Text;
    }

    private void radioButton1_CheckedChanged(object sender, System.EventArgs e)
    {
      radioButton2.Checked = !radioButton1.Checked;
      if (radioButton1.Checked == true)
        appConfig.m_CMDevice = (int)SendX10.CMDevices.CM11;
    }

    private void radioButton2_CheckedChanged(object sender, System.EventArgs e)
    {
      radioButton1.Checked = !radioButton2.Checked;
      if (radioButton2.Checked == true)
        appConfig.m_CMDevice = (int)SendX10.CMDevices.CM17;
    }

    private void txtComPort_TextChanged(object sender, System.EventArgs e)
    {
      try
      {
        appConfig.m_CM17COMPort = System.Convert.ToInt32(txtComPort.Text);
      }
      catch { }
    }

  }
}
