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
using System.ComponentModel;
using System.Windows.Forms;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// The setup form of this plugin
  /// </summary>
  public class SetupForm : Form
  {
    private IContainer components;

    private Label label1;
    private GroupBox groupBox1;
    private Label label2;
    private Label label3;
    private Label label4;
    private TextBox txtCols;
    private TextBox txtRows;
    private TextBox txtTim;
    private ComboBox cmbPort;
    private CheckBox cbLight;
    private Label label7;
    private ComboBox cmbType;
    private Label label8;
    private TextBox txtTimG;
    private TextBox txtRowsG;
    private TextBox txtColsG;
    private Label label9;
    private Label label10;
    private GroupBox gbTextMode;
    private GroupBox gbGraphMode;
    private CheckBox cbPropertyBrowser;
    private Button btnOK;
    private Button btnAdvanced;
    private TrackBar tbContrast;
    private Label label5;
    private ErrorProvider errorProvider;
    private CheckBox cbExtensiveLogging;
    private IDisplay lcd = null;

    public SetupForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      cmbPort.SelectedIndex = 0;
      cmbType.DataSource = Settings.Instance.Drivers;
      cmbType.DisplayMember = "Description";
      cmbType.DataBindings.Add("SelectedItem", Settings.Instance, "LCDType");
      cmbPort.DataBindings.Add("SelectedItem", Settings.Instance, "GUIPort");
      cbPropertyBrowser.DataBindings.Add("Checked", Settings.Instance, "ShowPropertyBrowser");
      cbExtensiveLogging.DataBindings.Add("Checked", Settings.Instance, "ExtensiveLogging");
      cbLight.DataBindings.Add("Checked", Settings.Instance, "BackLight");
      txtCols.DataBindings.Add("Text", Settings.Instance, "TextWidth");
      txtRows.DataBindings.Add("Text", Settings.Instance, "TextHeight");
      txtColsG.DataBindings.Add("Text", Settings.Instance, "GraphicWidth");
      txtRowsG.DataBindings.Add("Text", Settings.Instance, "GraphicHeight");
      txtTim.DataBindings.Add("Text", Settings.Instance, "TextComDelay");
      txtTimG.DataBindings.Add("Text", Settings.Instance, "GraphicComDelay");
      tbContrast.DataBindings.Add("Value", Settings.Instance, "Contrast");
      lcd = Settings.Instance.LCDType;
      cmbType.SelectedItem = lcd;
      VerifyLCDType();
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.btnAdvanced = new MediaPortal.UserInterface.Controls.MPButton();
      this.cmbPort = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbContrast = new System.Windows.Forms.TrackBar();
      this.gbGraphMode = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtTimG = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.txtRowsG = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.txtColsG = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label10 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.gbTextMode = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtTim = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.txtRows = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.txtCols = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cmbType = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.cbLight = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbPropertyBrowser = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.btnOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.cbExtensiveLogging = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tbContrast)).BeginInit();
      this.gbGraphMode.SuspendLayout();
      this.gbTextMode.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      this.SuspendLayout();
      // 
      // btnAdvanced
      // 
      this.btnAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAdvanced.Location = new System.Drawing.Point(226, 208);
      this.btnAdvanced.Name = "btnAdvanced";
      this.btnAdvanced.Size = new System.Drawing.Size(88, 23);
      this.btnAdvanced.TabIndex = 70;
      this.btnAdvanced.Text = "&Advanced";
      this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);
      // 
      // cmbPort
      // 
      this.cmbPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbPort.Items.AddRange(new object[] {
            "LPT1",
            "LPT2",
            "LPT3",
            "LPT4",
            "USB",
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "NONE",
            "localhost"});
      this.cmbPort.Location = new System.Drawing.Point(40, 48);
      this.cmbPort.Name = "cmbPort";
      this.cmbPort.Size = new System.Drawing.Size(64, 21);
      this.cmbPort.TabIndex = 20;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 48);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(32, 23);
      this.label1.TabIndex = 2;
      this.label1.Text = "Port";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.tbContrast);
      this.groupBox1.Controls.Add(this.gbGraphMode);
      this.groupBox1.Controls.Add(this.gbTextMode);
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.cmbType);
      this.groupBox1.Controls.Add(this.cbLight);
      this.groupBox1.Controls.Add(this.btnAdvanced);
      this.groupBox1.Controls.Add(this.cmbPort);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(328, 248);
      this.groupBox1.TabIndex = 3;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Configuration";
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(8, 168);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(56, 16);
      this.label5.TabIndex = 74;
      this.label5.Text = "Contrast:";
      // 
      // tbContrast
      // 
      this.tbContrast.Location = new System.Drawing.Point(16, 184);
      this.tbContrast.Maximum = 255;
      this.tbContrast.Name = "tbContrast";
      this.tbContrast.Size = new System.Drawing.Size(208, 45);
      this.tbContrast.TabIndex = 73;
      this.tbContrast.TickFrequency = 8;
      this.tbContrast.TickStyle = System.Windows.Forms.TickStyle.None;
      this.tbContrast.Value = 127;
      // 
      // gbGraphMode
      // 
      this.gbGraphMode.Controls.Add(this.label8);
      this.gbGraphMode.Controls.Add(this.txtTimG);
      this.gbGraphMode.Controls.Add(this.txtRowsG);
      this.gbGraphMode.Controls.Add(this.txtColsG);
      this.gbGraphMode.Controls.Add(this.label9);
      this.gbGraphMode.Controls.Add(this.label10);
      this.gbGraphMode.Location = new System.Drawing.Point(168, 72);
      this.gbGraphMode.Name = "gbGraphMode";
      this.gbGraphMode.Size = new System.Drawing.Size(152, 96);
      this.gbGraphMode.TabIndex = 72;
      this.gbGraphMode.TabStop = false;
      this.gbGraphMode.Text = "GraphMode";
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(8, 16);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(64, 23);
      this.label8.TabIndex = 3;
      this.label8.Text = "Columns";
      this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtTimG
      // 
      this.txtTimG.Location = new System.Drawing.Point(88, 64);
      this.txtTimG.Name = "txtTimG";
      this.txtTimG.Size = new System.Drawing.Size(48, 20);
      this.txtTimG.TabIndex = 50;
      this.txtTimG.Text = "1";
      // 
      // txtRowsG
      // 
      this.txtRowsG.Location = new System.Drawing.Point(88, 40);
      this.txtRowsG.Name = "txtRowsG";
      this.txtRowsG.Size = new System.Drawing.Size(48, 20);
      this.txtRowsG.TabIndex = 40;
      this.txtRowsG.Text = "240";
      // 
      // txtColsG
      // 
      this.txtColsG.Location = new System.Drawing.Point(88, 16);
      this.txtColsG.Name = "txtColsG";
      this.txtColsG.Size = new System.Drawing.Size(48, 20);
      this.txtColsG.TabIndex = 30;
      this.txtColsG.Text = "320";
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(8, 64);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(80, 23);
      this.label9.TabIndex = 5;
      this.label9.Text = "Comm. Delay";
      this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label10
      // 
      this.label10.Location = new System.Drawing.Point(8, 40);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(72, 23);
      this.label10.TabIndex = 4;
      this.label10.Text = "Rows";
      this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // gbTextMode
      // 
      this.gbTextMode.Controls.Add(this.label2);
      this.gbTextMode.Controls.Add(this.txtTim);
      this.gbTextMode.Controls.Add(this.txtRows);
      this.gbTextMode.Controls.Add(this.txtCols);
      this.gbTextMode.Controls.Add(this.label4);
      this.gbTextMode.Controls.Add(this.label3);
      this.gbTextMode.Location = new System.Drawing.Point(8, 72);
      this.gbTextMode.Name = "gbTextMode";
      this.gbTextMode.Size = new System.Drawing.Size(152, 96);
      this.gbTextMode.TabIndex = 71;
      this.gbTextMode.TabStop = false;
      this.gbTextMode.Text = "TextMode";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(8, 16);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(64, 23);
      this.label2.TabIndex = 3;
      this.label2.Text = "Columns";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtTim
      // 
      this.txtTim.Location = new System.Drawing.Point(88, 64);
      this.txtTim.Name = "txtTim";
      this.txtTim.Size = new System.Drawing.Size(48, 20);
      this.txtTim.TabIndex = 50;
      this.txtTim.Text = "1";
      // 
      // txtRows
      // 
      this.txtRows.Location = new System.Drawing.Point(88, 40);
      this.txtRows.Name = "txtRows";
      this.txtRows.Size = new System.Drawing.Size(48, 20);
      this.txtRows.TabIndex = 40;
      this.txtRows.Text = "2";
      // 
      // txtCols
      // 
      this.txtCols.Location = new System.Drawing.Point(88, 16);
      this.txtCols.Name = "txtCols";
      this.txtCols.Size = new System.Drawing.Size(48, 20);
      this.txtCols.TabIndex = 30;
      this.txtCols.Text = "16";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(8, 64);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(80, 23);
      this.label4.TabIndex = 5;
      this.label4.Text = "Comm. Delay";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(8, 40);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(72, 23);
      this.label3.TabIndex = 4;
      this.label3.Text = "Rows";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(8, 16);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(32, 23);
      this.label7.TabIndex = 11;
      this.label7.Text = "Type";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cmbType
      // 
      this.cmbType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbType.Location = new System.Drawing.Point(40, 16);
      this.cmbType.Name = "cmbType";
      this.cmbType.Size = new System.Drawing.Size(270, 21);
      this.cmbType.Sorted = true;
      this.cmbType.TabIndex = 10;
      this.cmbType.SelectionChangeCommitted += new System.EventHandler(this.cmbType_SelectionChangeCommitted);
      // 
      // cbLight
      // 
      this.cbLight.Location = new System.Drawing.Point(240, 168);
      this.cbLight.Name = "cbLight";
      this.cbLight.Size = new System.Drawing.Size(80, 24);
      this.cbLight.TabIndex = 60;
      this.cbLight.Text = "BackLight";
      // 
      // cbPropertyBrowser
      // 
      this.cbPropertyBrowser.Location = new System.Drawing.Point(8, 262);
      this.cbPropertyBrowser.Name = "cbPropertyBrowser";
      this.cbPropertyBrowser.Size = new System.Drawing.Size(168, 24);
      this.cbPropertyBrowser.TabIndex = 4;
      this.cbPropertyBrowser.Text = "Show property browser";
      // 
      // btnOK
      // 
      this.btnOK.Location = new System.Drawing.Point(261, 279);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(75, 23);
      this.btnOK.TabIndex = 5;
      this.btnOK.Text = "&OK";
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // errorProvider
      // 
      this.errorProvider.ContainerControl = this;
      // 
      // cbExtensiveLogging
      // 
      this.cbExtensiveLogging.AutoSize = true;
      this.cbExtensiveLogging.Location = new System.Drawing.Point(8, 285);
      this.cbExtensiveLogging.Name = "cbExtensiveLogging";
      this.cbExtensiveLogging.Size = new System.Drawing.Size(109, 17);
      this.cbExtensiveLogging.TabIndex = 6;
      this.cbExtensiveLogging.Text = "Extensive logging";
      // 
      // SetupForm
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(344, 305);
      this.Controls.Add(this.cbExtensiveLogging);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.cbPropertyBrowser);
      this.Controls.Add(this.groupBox1);
      this.Name = "SetupForm";
      this.Text = "ExternalDisplay Configuration";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tbContrast)).EndInit();
      this.gbGraphMode.ResumeLayout(false);
      this.gbGraphMode.PerformLayout();
      this.gbTextMode.ResumeLayout(false);
      this.gbTextMode.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private void btnAdvanced_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      try
      {
        lcd.Configure();
      }
      finally
      {
        Cursor = Cursors.Default;
      }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      Settings.Save();
      Close();
    }

    private void cmbType_SelectionChangeCommitted(object sender, EventArgs e)
    {
      VerifyLCDType();
    }

    private void VerifyLCDType()
    {
      lcd = cmbType.SelectedItem as IDisplay;
      if (lcd.IsDisabled)
      {
        errorProvider.SetError(cmbType, lcd.ErrorMessage);
        btnOK.Enabled = false;
      }
      else
      {
        errorProvider.SetError(cmbType, null);
        btnOK.Enabled = true;
      }
      gbGraphMode.Visible = lcd.SupportsGraphics;
      gbTextMode.Visible = lcd.SupportsText;
      Settings.Instance.LCDType = lcd;
    }
  }
}