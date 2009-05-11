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

using System;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.Util;

namespace MediaPortal.Configuration.Sections
{
  public class Wizard_DVBSTV : Wizard_ScanBase
  {
    class Transponder : IComparable
    {
      public string SatName;
      public string FileName;
      public override string ToString()
      {
        return SatName;
      }
      public int CompareTo(object o)
      {
        Transponder k = (Transponder)o;
        return SatName.CompareTo(k.SatName);
      }

    }
#if EDIT
    protected MediaPortal.UserInterface.Controls.MPLabel lblStatus;
    protected MediaPortal.UserInterface.Controls.MPLabel lblStatus2;
    protected ProgressBar progressBarQuality;
    protected ProgressBar progressBarStrength;
    protected ProgressBar progressBarProgress;
    protected void buttonScan_Click(object sender, System.EventArgs e)
    {
    }
    protected MediaPortal.UserInterface.Controls.MPButton buttonScan;
#endif
    private MediaPortal.UserInterface.Controls.MPComboBox lnbTone4;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbTone3;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbTone2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbTone1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox3;
    private MediaPortal.UserInterface.Controls.MPComboBox cbTransponder;
    private MediaPortal.UserInterface.Controls.MPComboBox cbTransponder2;
    private MediaPortal.UserInterface.Controls.MPComboBox cbTransponder3;
    private MediaPortal.UserInterface.Controls.MPComboBox cbTransponder4;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPLabel label31;
    private MediaPortal.UserInterface.Controls.MPCheckBox useLNB4;
    private MediaPortal.UserInterface.Controls.MPCheckBox useLNB3;
    private MediaPortal.UserInterface.Controls.MPCheckBox useLNB2;
    private MediaPortal.UserInterface.Controls.MPCheckBox useLNB1;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbkind4;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbkind3;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbkind2;
    private MediaPortal.UserInterface.Controls.MPLabel label30;
    private MediaPortal.UserInterface.Controls.MPLabel label32;
    private MediaPortal.UserInterface.Controls.MPComboBox diseqcd;
    private MediaPortal.UserInterface.Controls.MPComboBox diseqcc;
    private MediaPortal.UserInterface.Controls.MPComboBox diseqcb;
    private MediaPortal.UserInterface.Controls.MPComboBox diseqca;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbkind1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox4;
    private MediaPortal.UserInterface.Controls.MPTextBox circularMHZ;
    private MediaPortal.UserInterface.Controls.MPLabel label20;
    private MediaPortal.UserInterface.Controls.MPTextBox cbandMHZ;
    private MediaPortal.UserInterface.Controls.MPLabel label21;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPTextBox lnb1MHZ;
    private MediaPortal.UserInterface.Controls.MPLabel lnb1;
    private MediaPortal.UserInterface.Controls.MPTextBox lnbswMHZ;
    private MediaPortal.UserInterface.Controls.MPLabel switchMHZ;
    private MediaPortal.UserInterface.Controls.MPTextBox lnb0MHZ;
    private MediaPortal.UserInterface.Controls.MPLabel label22;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;


    public Wizard_DVBSTV()
      : this("DVB-S TV")
    {
      _card = null;
    }

    public Wizard_DVBSTV(string name)
      : base(name)
    {

      _card = null;
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
    }

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBarQuality = new System.Windows.Forms.ProgressBar();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBarStrength = new System.Windows.Forms.ProgressBar();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label31 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.useLNB4 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.useLNB3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbTransponder4 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.useLNB2 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbTransponder3 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.useLNB1 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbTransponder2 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lnbkind4 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.cbTransponder = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lnbkind3 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lnbkind2 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label30 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label32 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.diseqcd = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.diseqcc = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.diseqcb = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.diseqca = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lnbkind1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.circularMHZ = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label20 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbandMHZ = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label21 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lnb1MHZ = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lnb1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lnbswMHZ = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.switchMHZ = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lnb0MHZ = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label22 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblStatus = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblStatus2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBarProgress = new System.Windows.Forms.ProgressBar();
      this.buttonScan = new MediaPortal.UserInterface.Controls.MPButton();
      this.lnbTone4 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lnbTone3 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lnbTone2 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lnbTone1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBox3.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.mpLabel3);
      this.groupBox3.Controls.Add(this.progressBarQuality);
      this.groupBox3.Controls.Add(this.mpLabel2);
      this.groupBox3.Controls.Add(this.mpLabel1);
      this.groupBox3.Controls.Add(this.progressBarStrength);
      this.groupBox3.Controls.Add(this.groupBox2);
      this.groupBox3.Controls.Add(this.groupBox4);
      this.groupBox3.Controls.Add(this.mpGroupBox1);
      this.groupBox3.Controls.Add(this.lblStatus);
      this.groupBox3.Controls.Add(this.lblStatus2);
      this.groupBox3.Controls.Add(this.progressBarProgress);
      this.groupBox3.Controls.Add(this.buttonScan);
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox3.Location = new System.Drawing.Point(5, 5);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(532, 391);
      this.groupBox3.TabIndex = 0;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Setup Digital TV (DVB-S Satellite)";
      // 
      // mpLabel3
      // 
      this.mpLabel3.Location = new System.Drawing.Point(18, 271);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(67, 17);
      this.mpLabel3.TabIndex = 45;
      this.mpLabel3.Text = "Progress:";
      // 
      // progressBarQuality
      // 
      this.progressBarQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarQuality.Location = new System.Drawing.Point(120, 310);
      this.progressBarQuality.Name = "progressBarQuality";
      this.progressBarQuality.Size = new System.Drawing.Size(320, 16);
      this.progressBarQuality.Step = 1;
      this.progressBarQuality.TabIndex = 44;
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(18, 313);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(100, 17);
      this.mpLabel2.TabIndex = 43;
      this.mpLabel2.Text = "Signal quality:";
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(18, 291);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(100, 17);
      this.mpLabel1.TabIndex = 42;
      this.mpLabel1.Text = "Signal strength:";
      // 
      // progressBarStrength
      // 
      this.progressBarStrength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarStrength.Location = new System.Drawing.Point(120, 290);
      this.progressBarStrength.Name = "progressBarStrength";
      this.progressBarStrength.Size = new System.Drawing.Size(320, 16);
      this.progressBarStrength.Step = 1;
      this.progressBarStrength.TabIndex = 41;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.lnbTone4);
      this.groupBox2.Controls.Add(this.lnbTone3);
      this.groupBox2.Controls.Add(this.lnbTone2);
      this.groupBox2.Controls.Add(this.mpLabel4);
      this.groupBox2.Controls.Add(this.lnbTone1);
      this.groupBox2.Controls.Add(this.label31);
      this.groupBox2.Controls.Add(this.useLNB4);
      this.groupBox2.Controls.Add(this.useLNB3);
      this.groupBox2.Controls.Add(this.cbTransponder4);
      this.groupBox2.Controls.Add(this.useLNB2);
      this.groupBox2.Controls.Add(this.cbTransponder3);
      this.groupBox2.Controls.Add(this.useLNB1);
      this.groupBox2.Controls.Add(this.cbTransponder2);
      this.groupBox2.Controls.Add(this.lnbkind4);
      this.groupBox2.Controls.Add(this.cbTransponder);
      this.groupBox2.Controls.Add(this.lnbkind3);
      this.groupBox2.Controls.Add(this.lnbkind2);
      this.groupBox2.Controls.Add(this.label30);
      this.groupBox2.Controls.Add(this.label32);
      this.groupBox2.Controls.Add(this.diseqcd);
      this.groupBox2.Controls.Add(this.diseqcc);
      this.groupBox2.Controls.Add(this.diseqcb);
      this.groupBox2.Controls.Add(this.diseqca);
      this.groupBox2.Controls.Add(this.lnbkind1);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(20, 120);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(506, 144);
      this.groupBox2.TabIndex = 40;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "LNB setup";
      this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
      // 
      // label31
      // 
      this.label31.Location = new System.Drawing.Point(328, 16);
      this.label31.Name = "label31";
      this.label31.Size = new System.Drawing.Size(64, 16);
      this.label31.TabIndex = 21;
      this.label31.Text = "Sattelite:";
      // 
      // useLNB4
      // 
      this.useLNB4.AutoSize = true;
      this.useLNB4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useLNB4.Location = new System.Drawing.Point(6, 110);
      this.useLNB4.Name = "useLNB4";
      this.useLNB4.Size = new System.Drawing.Size(58, 17);
      this.useLNB4.TabIndex = 31;
      this.useLNB4.Text = "LNB#4";
      this.useLNB4.UseVisualStyleBackColor = true;
      this.useLNB4.CheckedChanged += new System.EventHandler(this.useLNB4_CheckedChanged);
      // 
      // useLNB3
      // 
      this.useLNB3.AutoSize = true;
      this.useLNB3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useLNB3.Location = new System.Drawing.Point(6, 86);
      this.useLNB3.Name = "useLNB3";
      this.useLNB3.Size = new System.Drawing.Size(58, 17);
      this.useLNB3.TabIndex = 30;
      this.useLNB3.Text = "LNB#3";
      this.useLNB3.UseVisualStyleBackColor = true;
      this.useLNB3.CheckedChanged += new System.EventHandler(this.useLNB3_CheckedChanged);
      // 
      // cbTransponder4
      // 
      this.cbTransponder4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbTransponder4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTransponder4.Location = new System.Drawing.Point(328, 112);
      this.cbTransponder4.Name = "cbTransponder4";
      this.cbTransponder4.Size = new System.Drawing.Size(160, 21);
      this.cbTransponder4.TabIndex = 8;
      // 
      // useLNB2
      // 
      this.useLNB2.AutoSize = true;
      this.useLNB2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useLNB2.Location = new System.Drawing.Point(6, 62);
      this.useLNB2.Name = "useLNB2";
      this.useLNB2.Size = new System.Drawing.Size(58, 17);
      this.useLNB2.TabIndex = 29;
      this.useLNB2.Text = "LNB#2";
      this.useLNB2.UseVisualStyleBackColor = true;
      this.useLNB2.CheckedChanged += new System.EventHandler(this.useLNB2_CheckedChanged);
      // 
      // cbTransponder3
      // 
      this.cbTransponder3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbTransponder3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTransponder3.Location = new System.Drawing.Point(328, 88);
      this.cbTransponder3.Name = "cbTransponder3";
      this.cbTransponder3.Size = new System.Drawing.Size(160, 21);
      this.cbTransponder3.TabIndex = 6;
      // 
      // useLNB1
      // 
      this.useLNB1.AutoSize = true;
      this.useLNB1.Checked = true;
      this.useLNB1.CheckState = System.Windows.Forms.CheckState.Checked;
      this.useLNB1.Enabled = false;
      this.useLNB1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useLNB1.Location = new System.Drawing.Point(6, 38);
      this.useLNB1.Name = "useLNB1";
      this.useLNB1.Size = new System.Drawing.Size(58, 17);
      this.useLNB1.TabIndex = 28;
      this.useLNB1.Text = "LNB#1";
      this.useLNB1.UseVisualStyleBackColor = true;
      this.useLNB1.CheckedChanged += new System.EventHandler(this.useLNB1_CheckedChanged);
      // 
      // cbTransponder2
      // 
      this.cbTransponder2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbTransponder2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTransponder2.Location = new System.Drawing.Point(328, 64);
      this.cbTransponder2.Name = "cbTransponder2";
      this.cbTransponder2.Size = new System.Drawing.Size(160, 21);
      this.cbTransponder2.TabIndex = 4;
      // 
      // lnbkind4
      // 
      this.lnbkind4.Items.AddRange(new object[] {
            "Ku-Band",
            "C-Band",
            "Circular"});
      this.lnbkind4.Location = new System.Drawing.Point(187, 112);
      this.lnbkind4.Name = "lnbkind4";
      this.lnbkind4.Size = new System.Drawing.Size(72, 21);
      this.lnbkind4.TabIndex = 26;
      // 
      // cbTransponder
      // 
      this.cbTransponder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbTransponder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTransponder.Location = new System.Drawing.Point(328, 40);
      this.cbTransponder.Name = "cbTransponder";
      this.cbTransponder.Size = new System.Drawing.Size(160, 21);
      this.cbTransponder.TabIndex = 2;
      // 
      // lnbkind3
      // 
      this.lnbkind3.Items.AddRange(new object[] {
            "Ku-Band",
            "C-Band",
            "Circular"});
      this.lnbkind3.Location = new System.Drawing.Point(187, 88);
      this.lnbkind3.Name = "lnbkind3";
      this.lnbkind3.Size = new System.Drawing.Size(72, 21);
      this.lnbkind3.TabIndex = 25;
      // 
      // lnbkind2
      // 
      this.lnbkind2.Items.AddRange(new object[] {
            "Ku-Band",
            "C-Band",
            "Circular"});
      this.lnbkind2.Location = new System.Drawing.Point(187, 64);
      this.lnbkind2.Name = "lnbkind2";
      this.lnbkind2.Size = new System.Drawing.Size(72, 21);
      this.lnbkind2.TabIndex = 24;
      // 
      // label30
      // 
      this.label30.Location = new System.Drawing.Point(203, 23);
      this.label30.Name = "label30";
      this.label30.Size = new System.Drawing.Size(56, 16);
      this.label30.TabIndex = 22;
      this.label30.Text = "LNB:";
      // 
      // label32
      // 
      this.label32.Location = new System.Drawing.Point(80, 23);
      this.label32.Name = "label32";
      this.label32.Size = new System.Drawing.Size(80, 16);
      this.label32.TabIndex = 20;
      this.label32.Text = "DiSEqC:";
      // 
      // diseqcd
      // 
      this.diseqcd.Items.AddRange(new object[] {
            "None",
            "Simple A",
            "Simple B",
            "Level 1 A/A",
            "Level 1 B/A",
            "Level 1 A/B",
            "Level 1 B/B"});
      this.diseqcd.Location = new System.Drawing.Point(71, 112);
      this.diseqcd.Name = "diseqcd";
      this.diseqcd.Size = new System.Drawing.Size(104, 21);
      this.diseqcd.TabIndex = 14;
      this.diseqcd.Text = "None";
      // 
      // diseqcc
      // 
      this.diseqcc.Items.AddRange(new object[] {
            "None",
            "Simple A",
            "Simple B",
            "Level 1 A/A",
            "Level 1 B/A",
            "Level 1 A/B",
            "Level 1 B/B"});
      this.diseqcc.Location = new System.Drawing.Point(71, 88);
      this.diseqcc.Name = "diseqcc";
      this.diseqcc.Size = new System.Drawing.Size(104, 21);
      this.diseqcc.TabIndex = 13;
      this.diseqcc.Text = "None";
      // 
      // diseqcb
      // 
      this.diseqcb.Items.AddRange(new object[] {
            "None",
            "Simple A",
            "Simple B",
            "Level 1 A/A",
            "Level 1 B/A",
            "Level 1 A/B",
            "Level 1 B/B"});
      this.diseqcb.Location = new System.Drawing.Point(71, 64);
      this.diseqcb.Name = "diseqcb";
      this.diseqcb.Size = new System.Drawing.Size(104, 21);
      this.diseqcb.TabIndex = 12;
      this.diseqcb.Text = "None";
      // 
      // diseqca
      // 
      this.diseqca.Items.AddRange(new object[] {
            "None",
            "Simple A",
            "Simple B",
            "Level 1 A/A",
            "Level 1 B/A",
            "Level 1 A/B",
            "Level 1 B/B"});
      this.diseqca.Location = new System.Drawing.Point(71, 40);
      this.diseqca.Name = "diseqca";
      this.diseqca.Size = new System.Drawing.Size(104, 21);
      this.diseqca.TabIndex = 1;
      this.diseqca.Text = "None";
      // 
      // lnbkind1
      // 
      this.lnbkind1.Items.AddRange(new object[] {
            "Ku-Band",
            "C-Band",
            "Circular"});
      this.lnbkind1.Location = new System.Drawing.Point(187, 40);
      this.lnbkind1.Name = "lnbkind1";
      this.lnbkind1.Size = new System.Drawing.Size(72, 21);
      this.lnbkind1.TabIndex = 27;
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.circularMHZ);
      this.groupBox4.Controls.Add(this.label20);
      this.groupBox4.Controls.Add(this.cbandMHZ);
      this.groupBox4.Controls.Add(this.label21);
      this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox4.Location = new System.Drawing.Point(252, 19);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(176, 96);
      this.groupBox4.TabIndex = 39;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "C-Band / Circular Config:";
      // 
      // circularMHZ
      // 
      this.circularMHZ.Location = new System.Drawing.Point(96, 45);
      this.circularMHZ.Name = "circularMHZ";
      this.circularMHZ.Size = new System.Drawing.Size(64, 20);
      this.circularMHZ.TabIndex = 3;
      this.circularMHZ.Text = "10750";
      // 
      // label20
      // 
      this.label20.Location = new System.Drawing.Point(16, 48);
      this.label20.Name = "label20";
      this.label20.Size = new System.Drawing.Size(80, 16);
      this.label20.TabIndex = 2;
      this.label20.Text = "Circular (MHz)";
      // 
      // cbandMHZ
      // 
      this.cbandMHZ.Location = new System.Drawing.Point(96, 21);
      this.cbandMHZ.Name = "cbandMHZ";
      this.cbandMHZ.Size = new System.Drawing.Size(64, 20);
      this.cbandMHZ.TabIndex = 1;
      this.cbandMHZ.Text = "5150";
      // 
      // label21
      // 
      this.label21.Location = new System.Drawing.Point(16, 24);
      this.label21.Name = "label21";
      this.label21.Size = new System.Drawing.Size(80, 16);
      this.label21.TabIndex = 0;
      this.label21.Text = "C-Band (MHz)";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.lnb1MHZ);
      this.mpGroupBox1.Controls.Add(this.lnb1);
      this.mpGroupBox1.Controls.Add(this.lnbswMHZ);
      this.mpGroupBox1.Controls.Add(this.switchMHZ);
      this.mpGroupBox1.Controls.Add(this.lnb0MHZ);
      this.mpGroupBox1.Controls.Add(this.label22);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(20, 19);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(176, 96);
      this.mpGroupBox1.TabIndex = 38;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Ku-Band Config:";
      // 
      // lnb1MHZ
      // 
      this.lnb1MHZ.Location = new System.Drawing.Point(104, 69);
      this.lnb1MHZ.Name = "lnb1MHZ";
      this.lnb1MHZ.Size = new System.Drawing.Size(56, 20);
      this.lnb1MHZ.TabIndex = 7;
      this.lnb1MHZ.Text = "10600";
      // 
      // lnb1
      // 
      this.lnb1.Location = new System.Drawing.Point(16, 72);
      this.lnb1.Name = "lnb1";
      this.lnb1.Size = new System.Drawing.Size(72, 16);
      this.lnb1.TabIndex = 6;
      this.lnb1.Text = "LNB1 (Mhz)";
      // 
      // lnbswMHZ
      // 
      this.lnbswMHZ.Location = new System.Drawing.Point(104, 45);
      this.lnbswMHZ.Name = "lnbswMHZ";
      this.lnbswMHZ.Size = new System.Drawing.Size(56, 20);
      this.lnbswMHZ.TabIndex = 3;
      this.lnbswMHZ.Text = "11700";
      // 
      // switchMHZ
      // 
      this.switchMHZ.Location = new System.Drawing.Point(16, 48);
      this.switchMHZ.Name = "switchMHZ";
      this.switchMHZ.Size = new System.Drawing.Size(80, 16);
      this.switchMHZ.TabIndex = 2;
      this.switchMHZ.Text = "Switch (MHz)";
      // 
      // lnb0MHZ
      // 
      this.lnb0MHZ.Location = new System.Drawing.Point(104, 21);
      this.lnb0MHZ.Name = "lnb0MHZ";
      this.lnb0MHZ.Size = new System.Drawing.Size(56, 20);
      this.lnb0MHZ.TabIndex = 1;
      this.lnb0MHZ.Text = "9750";
      // 
      // label22
      // 
      this.label22.Location = new System.Drawing.Point(16, 24);
      this.label22.Name = "label22";
      this.label22.Size = new System.Drawing.Size(72, 16);
      this.label22.TabIndex = 0;
      this.label22.Text = "LNB0 (Mhz)";
      // 
      // lblStatus
      // 
      this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblStatus.Location = new System.Drawing.Point(18, 337);
      this.lblStatus.Name = "lblStatus";
      this.lblStatus.Size = new System.Drawing.Size(423, 17);
      this.lblStatus.TabIndex = 10;
      // 
      // lblStatus2
      // 
      this.lblStatus2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblStatus2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblStatus2.Location = new System.Drawing.Point(17, 359);
      this.lblStatus2.Name = "lblStatus2";
      this.lblStatus2.Size = new System.Drawing.Size(423, 17);
      this.lblStatus2.TabIndex = 46;
      // 
      // progressBarProgress
      // 
      this.progressBarProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarProgress.Location = new System.Drawing.Point(120, 270);
      this.progressBarProgress.Name = "progressBarProgress";
      this.progressBarProgress.Size = new System.Drawing.Size(320, 16);
      this.progressBarProgress.Step = 1;
      this.progressBarProgress.TabIndex = 9;
      // 
      // buttonScan
      // 
      this.buttonScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonScan.Location = new System.Drawing.Point(454, 273);
      this.buttonScan.Name = "buttonScan";
      this.buttonScan.Size = new System.Drawing.Size(72, 22);
      this.buttonScan.TabIndex = 11;
      this.buttonScan.Text = "Scan";
      this.buttonScan.UseVisualStyleBackColor = true;
      this.buttonScan.Click += new System.EventHandler(this.buttonScan_Click);
      // 
      // lnbTone4
      // 
      this.lnbTone4.Items.AddRange(new object[] {
            "None",
            "22 kHz",
            "33 kHz",
            "44 kHz"});
      this.lnbTone4.Location = new System.Drawing.Point(265, 112);
      this.lnbTone4.Name = "lnbTone4";
      this.lnbTone4.Size = new System.Drawing.Size(57, 21);
      this.lnbTone4.TabIndex = 35;
      this.lnbTone4.SelectedIndexChanged += new System.EventHandler(this.mpComboBox1_SelectedIndexChanged);
      // 
      // lnbTone3
      // 
      this.lnbTone3.Items.AddRange(new object[] {
            "None",
            "22 kHz",
            "33 kHz",
            "44 kHz"});
      this.lnbTone3.Location = new System.Drawing.Point(265, 88);
      this.lnbTone3.Name = "lnbTone3";
      this.lnbTone3.Size = new System.Drawing.Size(57, 21);
      this.lnbTone3.TabIndex = 34;
      this.lnbTone3.SelectedIndexChanged += new System.EventHandler(this.mpComboBox2_SelectedIndexChanged);
      // 
      // lnbTone2
      // 
      this.lnbTone2.Items.AddRange(new object[] {
            "None",
            "22 kHz",
            "33 kHz",
            "44 kHz"});
      this.lnbTone2.Location = new System.Drawing.Point(265, 64);
      this.lnbTone2.Name = "lnbTone2";
      this.lnbTone2.Size = new System.Drawing.Size(57, 21);
      this.lnbTone2.TabIndex = 33;
      this.lnbTone2.SelectedIndexChanged += new System.EventHandler(this.mpComboBox3_SelectedIndexChanged);
      // 
      // mpLabel4
      // 
      this.mpLabel4.Location = new System.Drawing.Point(265, 16);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(41, 16);
      this.mpLabel4.TabIndex = 32;
      this.mpLabel4.Text = "Tone";
      // 
      // lnbTone1
      // 
      this.lnbTone1.Items.AddRange(new object[] {
            "None",
            "22 kHz",
            "33 kHz",
            "44 kHz",
            ""});
      this.lnbTone1.Location = new System.Drawing.Point(265, 40);
      this.lnbTone1.Name = "lnbTone1";
      this.lnbTone1.Size = new System.Drawing.Size(57, 21);
      this.lnbTone1.TabIndex = 36;
      // 
      // Wizard_DVBSTV
      // 
      this.Controls.Add(this.groupBox3);
      this.Name = "Wizard_DVBSTV";
      this.Size = new System.Drawing.Size(545, 408);
      this.groupBox3.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    Transponder LoadTransponderName(string fileName)
    {
      Transponder ts = new Transponder();
      ts.FileName = fileName;
      ts.SatName = fileName;

      string line;
      System.IO.TextReader tin = System.IO.File.OpenText(Config.GetFile(Config.Dir.Base, "Tuningparameters" , fileName));
      while (true)
      {
        line = tin.ReadLine();
        if (line == null) break;
        string search = line.ToLower();
        int pos = search.IndexOf("satname");
        if (pos >= 0)
        {
          pos = search.IndexOf("=");
          if (pos > 0)
          {
            ts.SatName = line.Substring(pos + 1);
            ts.SatName = ts.SatName.Trim();
            break;
          }
        }
      }
      tin.Close();

      return ts;
    }
    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      if (_card == null)
      {
        TVCaptureCards cards = new TVCaptureCards();
        cards.LoadCaptureCards();
        foreach (TVCaptureDevice dev in cards.captureCards)
        {
          if (dev.Network == NetworkType.DVBS)
            /*||
            // HACK: This is a nasty hack until I can figure out a way
            // around this - Drak
              dev.CardType == TVCapture.CardTypes.Digital_TTPremium)*/
          {
            _card = dev;
            break;
          }
        }
      }
      cbTransponder.Items.Clear();
      cbTransponder2.Items.Clear();
      cbTransponder3.Items.Clear();
      cbTransponder4.Items.Clear();
      string[] files = System.IO.Directory.GetFiles(Config.GetSubFolder(Config.Dir.Base, "Tuningparameters"), "*.tpl");
      Transponder[] transponders = new Transponder[files.Length];
      int trans = 0;
      foreach (string file in files)
      {
        string fileName = System.IO.Path.GetFileName(file);
        Transponder ts = LoadTransponderName(fileName);
        if (ts != null)
        {
          transponders[trans++] = ts;
        }
      }
      Array.Sort(transponders);
      foreach (Transponder ts in transponders)
      {
        cbTransponder.Items.Add(ts);
        cbTransponder2.Items.Add(ts);
        cbTransponder3.Items.Add(ts);
        cbTransponder4.Items.Add(ts);
      }
      if (cbTransponder.Items.Count > 0)
        cbTransponder.SelectedIndex = 0;
      if (cbTransponder2.Items.Count > 0)
        cbTransponder2.SelectedIndex = 0;
      if (cbTransponder3.Items.Count > 0)
        cbTransponder3.SelectedIndex = 0;
      if (cbTransponder4.Items.Count > 0)
        cbTransponder4.SelectedIndex = 0;


      useLNB1.Checked = true;
      useLNB2.Checked = false;
      useLNB3.Checked = false;
      useLNB4.Checked = false;
      this.OnScanFinished += new MediaPortal.Configuration.Sections.Wizard_ScanBase.ScanFinishedHandler(this.dlg_OnScanFinished);
      this.OnScanStarted += new MediaPortal.Configuration.Sections.Wizard_ScanBase.ScanStartedHandler(this.dlg_OnScanStarted);
      LoadSettings();
    }
    protected override String[] GetScanParameters()
    {
      int m_diseqcLoops = 1;
      string filename = String.Format(Config.GetFile(Config.Dir.Database, "card_{0}.xml"), _card.FriendlyName);
      if (useLNB2.Checked) m_diseqcLoops++;
      if (useLNB3.Checked) m_diseqcLoops++;
      if (useLNB4.Checked) m_diseqcLoops++;
      String[] parameters = new String[m_diseqcLoops];
      Transponder ts = (Transponder)cbTransponder.SelectedItem;
      parameters[0] = Config.GetFile(Config.Dir.Base, @"Tuningparameters\" + ts.FileName);

      if (useLNB2.Checked)
      {
        ts = (Transponder)cbTransponder2.SelectedItem;
        parameters[1] = Config.GetFile(Config.Dir.Base, @"Tuningparameters\" , ts.FileName);
      }
      if (useLNB3.Checked)
      {
        ts = (Transponder)cbTransponder3.SelectedItem;
        parameters[2] = Config.GetFile(Config.Dir.Base, @"Tuningparameters\" , ts.FileName);
      }
      if (useLNB4.Checked)
      {
        ts = (Transponder)cbTransponder4.SelectedItem;
        parameters[3] = Config.GetFile(Config.Dir.Base, @"Tuningparameters\", ts.FileName);
      }
      return parameters;
    }

    void dlg_OnScanFinished(object sender, EventArgs args)
    {
      this.groupBox2.Enabled = true;
      this.groupBox4.Enabled = true;
      this.mpGroupBox1.Enabled = true;
      WizardForm wizard = WizardForm.Form;
      if (wizard != null)
      {
        wizard.DisableBack(false);
        wizard.DisableNext(false);
      }

    }

    void dlg_OnScanStarted(object sender, EventArgs args)
    {
      this.groupBox2.Enabled = false;
      this.groupBox4.Enabled = false;
      this.mpGroupBox1.Enabled = false;
      WizardForm wizard = WizardForm.Form;
      if (wizard != null)
      {
        wizard.DisableBack(true);
        wizard.DisableNext(true);
      }

    }
    public override void LoadSettings()
    {
      if (_card == null)
      {
        Log.Info("load DVBS:no card");
        return;
      }
      Log.Info("load DVBS:{0}", _card.FriendlyName);
      string filename = String.Format(Config.GetFile(Config.Dir.Database, "card_{0}.xml"), _card.FriendlyName);


      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
      {
        lnb0MHZ.Text = xmlreader.GetValueAsInt("dvbs", "LNB0", 9750).ToString();
        lnb1MHZ.Text = xmlreader.GetValueAsInt("dvbs", "LNB1", 10600).ToString();
        lnbswMHZ.Text = xmlreader.GetValueAsInt("dvbs", "Switch", 11700).ToString();
        cbandMHZ.Text = xmlreader.GetValueAsInt("dvbs", "CBand", 5150).ToString();
        circularMHZ.Text = xmlreader.GetValueAsInt("dvbs", "Circular", 10750).ToString();
        useLNB1.Checked = true;
        useLNB1.Enabled = false;
        useLNB2.Checked = xmlreader.GetValueAsBool("dvbs", "useLNB2", false);
        useLNB3.Checked = xmlreader.GetValueAsBool("dvbs", "useLNB3", false);
        useLNB4.Checked = xmlreader.GetValueAsBool("dvbs", "useLNB4", false);
        useLNB2_CheckedChanged(null, null);
        useLNB3_CheckedChanged(null, null);
        useLNB4_CheckedChanged(null, null);

        int lnbTone = xmlreader.GetValueAsInt("dvbs", "lnb", 22);
        switch (lnbTone)
        {
          case 0: lnbTone1.SelectedIndex = 0; break;
          case 22: lnbTone1.SelectedIndex = 1; break;
          case 33: lnbTone1.SelectedIndex = 2; break;
          case 44: lnbTone1.SelectedIndex = 3; break;
        }
        lnbTone = xmlreader.GetValueAsInt("dvbs", "lnb2", 22);
        switch (lnbTone)
        {
          case 0: lnbTone2.SelectedIndex = 0; break;
          case 22: lnbTone2.SelectedIndex = 1; break;
          case 33: lnbTone2.SelectedIndex = 2; break;
          case 44: lnbTone2.SelectedIndex = 3; break;
        }
        lnbTone = xmlreader.GetValueAsInt("dvbs", "lnb3", 22);
        switch (lnbTone)
        {
          case 0: lnbTone3.SelectedIndex = 0; break;
          case 22: lnbTone3.SelectedIndex = 1; break;
          case 33: lnbTone3.SelectedIndex = 2; break;
          case 44: lnbTone3.SelectedIndex = 3; break;
        }
        lnbTone = xmlreader.GetValueAsInt("dvbs", "lnb4", 22);
        switch (lnbTone)
        {
          case 0: lnbTone4.SelectedIndex = 0; break;
          case 22: lnbTone4.SelectedIndex = 1; break;
          case 33: lnbTone4.SelectedIndex = 2; break;
          case 44: lnbTone4.SelectedIndex = 3; break;
        }
        int lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind", 0);
        switch (lnbKind)
        {
          case 0: lnbkind1.SelectedItem = "Ku-Band"; break;
          case 1: lnbkind1.SelectedItem = "C-Band"; break;
          case 2: lnbkind1.SelectedItem = "Circular"; break;
        }
        lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind2", 0);
        switch (lnbKind)
        {
          case 0: lnbkind2.SelectedItem = "Ku-Band"; break;
          case 1: lnbkind2.SelectedItem = "C-Band"; break;
          case 2: lnbkind2.SelectedItem = "Circular"; break;
        }
        lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind3", 0);
        switch (lnbKind)
        {
          case 0: lnbkind3.SelectedItem = "Ku-Band"; break;
          case 1: lnbkind3.SelectedItem = "C-Band"; break;
          case 2: lnbkind3.SelectedItem = "Circular"; break;
        }
        lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind4", 0);
        switch (lnbKind)
        {
          case 0: lnbkind4.SelectedItem = "Ku-Band"; break;
          case 1: lnbkind4.SelectedItem = "C-Band"; break;
          case 2: lnbkind4.SelectedItem = "Circular"; break;
        }
        int diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc", 0);
        switch (diseqc)
        {
          case 0: diseqca.SelectedItem = "None"; break;
          case 1: diseqca.SelectedItem = "Simple A"; break;
          case 2: diseqca.SelectedItem = "Simple B"; break;
          case 3: diseqca.SelectedItem = "Level 1 A/A"; break;
          case 4: diseqca.SelectedItem = "Level 1 B/A"; break;
          case 5: diseqca.SelectedItem = "Level 1 A/B"; break;
          case 6: diseqca.SelectedItem = "Level 1 B/B"; break;

        }
        diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc2", 0);
        switch (diseqc)
        {
          case 0: diseqcb.SelectedItem = "None"; break;
          case 1: diseqcb.SelectedItem = "Simple A"; break;
          case 2: diseqcb.SelectedItem = "Simple B"; break;
          case 3: diseqcb.SelectedItem = "Level 1 A/A"; break;
          case 4: diseqcb.SelectedItem = "Level 1 B/A"; break;
          case 5: diseqcb.SelectedItem = "Level 1 A/B"; break;
          case 6: diseqcb.SelectedItem = "Level 1 B/B"; break;

        }
        diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc3", 0);
        switch (diseqc)
        {
          case 0: diseqcc.SelectedItem = "None"; break;
          case 1: diseqcc.SelectedItem = "Simple A"; break;
          case 2: diseqcc.SelectedItem = "Simple B"; break;
          case 3: diseqcc.SelectedItem = "Level 1 A/A"; break;
          case 4: diseqcc.SelectedItem = "Level 1 B/A"; break;
          case 5: diseqcc.SelectedItem = "Level 1 A/B"; break;
          case 6: diseqcc.SelectedItem = "Level 1 B/B"; break;

        }
        diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc4", 0);
        switch (diseqc)
        {
          case 0: diseqcd.SelectedItem = "None"; break;
          case 1: diseqcd.SelectedItem = "Simple A"; break;
          case 2: diseqcd.SelectedItem = "Simple B"; break;
          case 3: diseqcd.SelectedItem = "Level 1 A/A"; break;
          case 4: diseqcd.SelectedItem = "Level 1 B/A"; break;
          case 5: diseqcd.SelectedItem = "Level 1 A/B"; break;
          case 6: diseqcd.SelectedItem = "Level 1 B/B"; break;

        }
        string transponder = xmlreader.GetValueAsString("dvbs", "transponder1", "");
        Log.Info("1:{0}", transponder);
        if (transponder != "")
        {
          for (int i = 0; i < cbTransponder.Items.Count; ++i)
          {
            Transponder tp = (Transponder)cbTransponder.Items[i];
            if (tp.SatName == transponder)
            {
              cbTransponder.SelectedIndex = i;
              break;
            }
          }
        }
        transponder = xmlreader.GetValueAsString("dvbs", "transponder2", "");

        for (int i = 0; i < cbTransponder2.Items.Count; ++i)
        {
          Transponder tp = (Transponder)cbTransponder2.Items[i];
          if (tp.SatName == transponder)
          {
            cbTransponder2.SelectedIndex = i;
            break;
          }
        }
        transponder = xmlreader.GetValueAsString("dvbs", "transponder3", "");

        for (int i = 0; i < cbTransponder3.Items.Count; ++i)
        {
          Transponder tp = (Transponder)cbTransponder3.Items[i];
          if (tp.SatName == transponder)
          {
            cbTransponder3.SelectedIndex = i;
            break;
          }
        }
        transponder = xmlreader.GetValueAsString("dvbs", "transponder4", "");

        for (int i = 0; i < cbTransponder4.Items.Count; ++i)
        {
          Transponder tp = (Transponder)cbTransponder4.Items[i];
          if (tp.SatName == transponder)
          {
            cbTransponder4.SelectedIndex = i;
            break;
          }
        }


      }
    }

    public override void SaveSettings()
    {
      Log.Info("Save DVBS:{0}", _card.FriendlyName);
      string filename = String.Format(Config.GetFile(Config.Dir.Database,"card_{0}.xml"), _card.FriendlyName);
      // save settings

      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(filename))
      {
        xmlwriter.SetValue("dvbs", "LNB0", lnb0MHZ.Text);
        xmlwriter.SetValue("dvbs", "LNB1", lnb1MHZ.Text);
        xmlwriter.SetValue("dvbs", "Switch", lnbswMHZ.Text);
        xmlwriter.SetValue("dvbs", "CBand", cbandMHZ.Text);
        xmlwriter.SetValue("dvbs", "Circular", circularMHZ.Text);
        xmlwriter.SetValueAsBool("dvbs", "useLNB1", true);
        xmlwriter.SetValueAsBool("dvbs", "useLNB2", useLNB2.Checked);
        xmlwriter.SetValueAsBool("dvbs", "useLNB3", useLNB3.Checked);
        xmlwriter.SetValueAsBool("dvbs", "useLNB4", useLNB4.Checked);
        int lnbTone = lnbTone1.SelectedIndex;
        switch (lnbTone)
        {
          case 0: xmlwriter.SetValue("dvbs", "lnb", "0"); break;
          case 1: xmlwriter.SetValue("dvbs", "lnb", "22"); break;
          case 2: xmlwriter.SetValue("dvbs", "lnb", "33"); break;
          case 3: xmlwriter.SetValue("dvbs", "lnb", "44"); break;
        }
        lnbTone = lnbTone2.SelectedIndex;
        switch (lnbTone)
        {
          case 0: xmlwriter.SetValue("dvbs", "lnb2", "0"); break;
          case 1: xmlwriter.SetValue("dvbs", "lnb2", "22"); break;
          case 2: xmlwriter.SetValue("dvbs", "lnb2", "33"); break;
          case 3: xmlwriter.SetValue("dvbs", "lnb2", "44"); break;
        }
        lnbTone = lnbTone3.SelectedIndex;
        switch (lnbTone)
        {
          case 0: xmlwriter.SetValue("dvbs", "lnb3", "0"); break;
          case 1: xmlwriter.SetValue("dvbs", "lnb3", "22"); break;
          case 2: xmlwriter.SetValue("dvbs", "lnb3", "33"); break;
          case 3: xmlwriter.SetValue("dvbs", "lnb3", "44"); break;
        }
        lnbTone = lnbTone4.SelectedIndex;
        switch (lnbTone)
        {
          case 0: xmlwriter.SetValue("dvbs", "lnb4", "0"); break;
          case 1: xmlwriter.SetValue("dvbs", "lnb4", "22"); break;
          case 2: xmlwriter.SetValue("dvbs", "lnb4", "33"); break;
          case 3: xmlwriter.SetValue("dvbs", "lnb4", "44"); break;
        }
        if (diseqca.SelectedIndex >= 0)
        {
          xmlwriter.SetValue("dvbs", "diseqc", diseqca.SelectedIndex);
        }
        if (diseqcb.SelectedIndex >= 0)
        {
          xmlwriter.SetValue("dvbs", "diseqc2", diseqcb.SelectedIndex);
        }
        if (diseqcc.SelectedIndex >= 0)
        {
          xmlwriter.SetValue("dvbs", "diseqc3", diseqcc.SelectedIndex);
        }
        if (diseqcd.SelectedIndex >= 0)
        {
          xmlwriter.SetValue("dvbs", "diseqc4", diseqcd.SelectedIndex);
        }
        if (lnbkind1.SelectedIndex >= 0)
        {
          xmlwriter.SetValue("dvbs", "lnbKind", lnbkind1.SelectedIndex);
          xmlwriter.SetValue("dvbs", "transponder1", ((Transponder)cbTransponder.SelectedItem).SatName);
        }
        if (lnbkind2.SelectedIndex >= 0)
        {
          xmlwriter.SetValue("dvbs", "lnbKind2", lnbkind2.SelectedIndex);
          xmlwriter.SetValue("dvbs", "transponder2", ((Transponder)cbTransponder2.SelectedItem).SatName);
        }
        if (lnbkind3.SelectedIndex >= 0)
        {
          xmlwriter.SetValue("dvbs", "lnbKind3", lnbkind3.SelectedIndex);
          xmlwriter.SetValue("dvbs", "transponder3", ((Transponder)cbTransponder3.SelectedItem).SatName);
        }
        if (lnbkind4.SelectedIndex >= 0)
        {
          xmlwriter.SetValue("dvbs", "lnbKind4", lnbkind4.SelectedIndex);
          xmlwriter.SetValue("dvbs", "transponder4", ((Transponder)cbTransponder4.SelectedItem).SatName);
        }
      }
      MediaPortal.Profile.Settings.SaveCache();
    }

    private void useLNB1_CheckedChanged(object sender, EventArgs e)
    {
      cbTransponder.Enabled = useLNB1.Checked;
      diseqca.Enabled = useLNB1.Checked;
      lnbkind1.Enabled = useLNB1.Checked;
      lnbTone1.Enabled = useLNB1.Checked;
    }

    private void useLNB2_CheckedChanged(object sender, EventArgs e)
    {
      cbTransponder2.Enabled = useLNB2.Checked;
      diseqcb.Enabled = useLNB2.Checked;
      lnbkind2.Enabled = useLNB2.Checked;
      lnbTone2.Enabled = useLNB2.Checked;
    }

    private void useLNB3_CheckedChanged(object sender, EventArgs e)
    {
      cbTransponder3.Enabled = useLNB3.Checked;
      diseqcc.Enabled = useLNB3.Checked;
      lnbkind3.Enabled = useLNB3.Checked;
      lnbTone3.Enabled = useLNB3.Checked;
    }

    private void useLNB4_CheckedChanged(object sender, EventArgs e)
    {
      cbTransponder4.Enabled = useLNB4.Checked;
      diseqcd.Enabled = useLNB4.Checked;
      lnbkind4.Enabled = useLNB4.Checked;
      lnbTone4.Enabled = useLNB4.Checked;
    }

    private void groupBox2_Enter(object sender, EventArgs e)
    {

    }

    private void mpComboBox3_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void mpComboBox2_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void mpComboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

  }
}

