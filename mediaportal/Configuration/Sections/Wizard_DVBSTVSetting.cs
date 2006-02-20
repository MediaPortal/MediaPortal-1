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
using System.IO;
using System.Xml;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
namespace MediaPortal.Configuration.Sections
{
  public class Wizard_DVBSTVSetup : MediaPortal.Configuration.SectionSettings
  {
    private System.ComponentModel.IContainer components = null;


    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPCheckBox useLNB4;
    private MediaPortal.UserInterface.Controls.MPCheckBox useLNB3;
    private MediaPortal.UserInterface.Controls.MPCheckBox useLNB2;
    private MediaPortal.UserInterface.Controls.MPCheckBox useLNB1;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbkind4;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbkind3;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbkind2;
    private MediaPortal.UserInterface.Controls.MPLabel label30;
    private MediaPortal.UserInterface.Controls.MPLabel label31;
    private MediaPortal.UserInterface.Controls.MPLabel label32;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbconfig4;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbconfig3;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbconfig2;
    private MediaPortal.UserInterface.Controls.MPComboBox diseqcd;
    private MediaPortal.UserInterface.Controls.MPComboBox diseqcc;
    private MediaPortal.UserInterface.Controls.MPComboBox diseqcb;
    private MediaPortal.UserInterface.Controls.MPComboBox diseqca;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbkind1;
    private MediaPortal.UserInterface.Controls.MPComboBox lnbconfig1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox4;
    private MediaPortal.UserInterface.Controls.MPTextBox circularMHZ;
    private MediaPortal.UserInterface.Controls.MPLabel label20;
    private MediaPortal.UserInterface.Controls.MPTextBox cbandMHZ;
    private MediaPortal.UserInterface.Controls.MPLabel label21;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox3;
    private MediaPortal.UserInterface.Controls.MPTextBox lnb1MHZ;
    private MediaPortal.UserInterface.Controls.MPLabel lnb1;
    private MediaPortal.UserInterface.Controls.MPTextBox lnbswMHZ;
    private MediaPortal.UserInterface.Controls.MPLabel switchMHZ;
    private MediaPortal.UserInterface.Controls.MPTextBox lnb0MHZ;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPLabel label22;
    //int m_currentDiseqc=1;

    public Wizard_DVBSTVSetup()
      : this("DVB-S TV")
    {
    }

    public Wizard_DVBSTVSetup(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
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

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBox1 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.useLNB4 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.useLNB3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.useLNB2 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.useLNB1 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.lnbkind4 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lnbkind3 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lnbkind2 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label30 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label31 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label32 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lnbconfig4 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lnbconfig3 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lnbconfig2 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.diseqcd = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.diseqcc = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.diseqcb = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.diseqca = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lnbkind1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lnbconfig1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.circularMHZ = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label20 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbandMHZ = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label21 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lnb1MHZ = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lnb1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lnbswMHZ = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.switchMHZ = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lnb0MHZ = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label22 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.checkBox1);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(32, 288);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(144, 56);
      this.groupBox1.TabIndex = 38;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Use SkyStar2 MD-Plugins";
      this.groupBox1.Visible = false;
      // 
      // checkBox1
      // 
      this.checkBox1.AutoSize = true;
      this.checkBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBox1.Location = new System.Drawing.Point(32, 24);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(80, 17);
      this.checkBox1.TabIndex = 0;
      this.checkBox1.Text = "Use Plugins";
      this.checkBox1.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.mpLabel4);
      this.groupBox2.Controls.Add(this.mpLabel3);
      this.groupBox2.Controls.Add(this.mpLabel2);
      this.groupBox2.Controls.Add(this.mpLabel1);
      this.groupBox2.Controls.Add(this.useLNB4);
      this.groupBox2.Controls.Add(this.useLNB3);
      this.groupBox2.Controls.Add(this.useLNB2);
      this.groupBox2.Controls.Add(this.useLNB1);
      this.groupBox2.Controls.Add(this.lnbkind4);
      this.groupBox2.Controls.Add(this.lnbkind3);
      this.groupBox2.Controls.Add(this.lnbkind2);
      this.groupBox2.Controls.Add(this.label30);
      this.groupBox2.Controls.Add(this.label31);
      this.groupBox2.Controls.Add(this.label32);
      this.groupBox2.Controls.Add(this.lnbconfig4);
      this.groupBox2.Controls.Add(this.lnbconfig3);
      this.groupBox2.Controls.Add(this.lnbconfig2);
      this.groupBox2.Controls.Add(this.diseqcd);
      this.groupBox2.Controls.Add(this.diseqcc);
      this.groupBox2.Controls.Add(this.diseqcb);
      this.groupBox2.Controls.Add(this.diseqca);
      this.groupBox2.Controls.Add(this.lnbkind1);
      this.groupBox2.Controls.Add(this.lnbconfig1);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(32, 136);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(431, 144);
      this.groupBox2.TabIndex = 37;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "LNB setup";
      this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
      // 
      // useLNB4
      // 
      this.useLNB4.AutoSize = true;
      this.useLNB4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useLNB4.Location = new System.Drawing.Point(361, 112);
      this.useLNB4.Name = "useLNB4";
      this.useLNB4.Size = new System.Drawing.Size(63, 17);
      this.useLNB4.TabIndex = 31;
      this.useLNB4.Text = "Enabled";
      this.useLNB4.UseVisualStyleBackColor = true;
      // 
      // useLNB3
      // 
      this.useLNB3.AutoSize = true;
      this.useLNB3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useLNB3.Location = new System.Drawing.Point(361, 88);
      this.useLNB3.Name = "useLNB3";
      this.useLNB3.Size = new System.Drawing.Size(63, 17);
      this.useLNB3.TabIndex = 30;
      this.useLNB3.Text = "Enabled";
      this.useLNB3.UseVisualStyleBackColor = true;
      this.useLNB3.CheckedChanged += new System.EventHandler(this.useLNB3_CheckedChanged);
      // 
      // useLNB2
      // 
      this.useLNB2.AutoSize = true;
      this.useLNB2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useLNB2.Location = new System.Drawing.Point(361, 64);
      this.useLNB2.Name = "useLNB2";
      this.useLNB2.Size = new System.Drawing.Size(63, 17);
      this.useLNB2.TabIndex = 29;
      this.useLNB2.Text = "Enabled";
      this.useLNB2.UseVisualStyleBackColor = true;
      this.useLNB2.CheckedChanged += new System.EventHandler(this.useLNB2_CheckedChanged);
      // 
      // useLNB1
      // 
      this.useLNB1.AutoSize = true;
      this.useLNB1.Checked = true;
      this.useLNB1.CheckState = System.Windows.Forms.CheckState.Checked;
      this.useLNB1.Enabled = false;
      this.useLNB1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useLNB1.Location = new System.Drawing.Point(361, 40);
      this.useLNB1.Name = "useLNB1";
      this.useLNB1.Size = new System.Drawing.Size(63, 17);
      this.useLNB1.TabIndex = 28;
      this.useLNB1.Text = "Enabled";
      this.useLNB1.UseVisualStyleBackColor = true;
      // 
      // lnbkind4
      // 
      this.lnbkind4.Items.AddRange(new object[] {
            "Ku-Band",
            "C-Band",
            "Circular"});
      this.lnbkind4.Location = new System.Drawing.Point(281, 109);
      this.lnbkind4.Name = "lnbkind4";
      this.lnbkind4.Size = new System.Drawing.Size(72, 21);
      this.lnbkind4.TabIndex = 26;
      // 
      // lnbkind3
      // 
      this.lnbkind3.Items.AddRange(new object[] {
            "Ku-Band",
            "C-Band",
            "Circular"});
      this.lnbkind3.Location = new System.Drawing.Point(281, 85);
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
      this.lnbkind2.Location = new System.Drawing.Point(281, 61);
      this.lnbkind2.Name = "lnbkind2";
      this.lnbkind2.Size = new System.Drawing.Size(72, 21);
      this.lnbkind2.TabIndex = 24;
      // 
      // label30
      // 
      this.label30.Location = new System.Drawing.Point(281, 21);
      this.label30.Name = "label30";
      this.label30.Size = new System.Drawing.Size(56, 16);
      this.label30.TabIndex = 22;
      this.label30.Text = "LNB:";
      // 
      // label31
      // 
      this.label31.Location = new System.Drawing.Point(185, 21);
      this.label31.Name = "label31";
      this.label31.Size = new System.Drawing.Size(64, 16);
      this.label31.TabIndex = 21;
      this.label31.Text = "LNBSelect:";
      // 
      // label32
      // 
      this.label32.Location = new System.Drawing.Point(65, 21);
      this.label32.Name = "label32";
      this.label32.Size = new System.Drawing.Size(80, 16);
      this.label32.TabIndex = 20;
      this.label32.Text = "DiSEqC:";
      // 
      // lnbconfig4
      // 
      this.lnbconfig4.Items.AddRange(new object[] {
            "0 KHz",
            "22 KHz",
            "33 Khz",
            "44 KHz"});
      this.lnbconfig4.Location = new System.Drawing.Point(185, 109);
      this.lnbconfig4.Name = "lnbconfig4";
      this.lnbconfig4.Size = new System.Drawing.Size(80, 21);
      this.lnbconfig4.TabIndex = 19;
      // 
      // lnbconfig3
      // 
      this.lnbconfig3.Items.AddRange(new object[] {
            "0 KHz",
            "22 KHz",
            "33 Khz",
            "44 KHz"});
      this.lnbconfig3.Location = new System.Drawing.Point(185, 85);
      this.lnbconfig3.Name = "lnbconfig3";
      this.lnbconfig3.Size = new System.Drawing.Size(80, 21);
      this.lnbconfig3.TabIndex = 18;
      // 
      // lnbconfig2
      // 
      this.lnbconfig2.Items.AddRange(new object[] {
            "0 KHz",
            "22 KHz",
            "33 Khz",
            "44 KHz"});
      this.lnbconfig2.Location = new System.Drawing.Point(185, 61);
      this.lnbconfig2.Name = "lnbconfig2";
      this.lnbconfig2.Size = new System.Drawing.Size(80, 21);
      this.lnbconfig2.TabIndex = 17;
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
      this.diseqcd.Location = new System.Drawing.Point(65, 109);
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
      this.diseqcc.Location = new System.Drawing.Point(65, 85);
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
      this.diseqcb.Location = new System.Drawing.Point(65, 61);
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
      this.diseqca.Location = new System.Drawing.Point(65, 37);
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
      this.lnbkind1.Location = new System.Drawing.Point(281, 37);
      this.lnbkind1.Name = "lnbkind1";
      this.lnbkind1.Size = new System.Drawing.Size(72, 21);
      this.lnbkind1.TabIndex = 27;
      // 
      // lnbconfig1
      // 
      this.lnbconfig1.Items.AddRange(new object[] {
            "0 KHz",
            "22 KHz",
            "33 Khz",
            "44 KHz"});
      this.lnbconfig1.Location = new System.Drawing.Point(185, 37);
      this.lnbconfig1.Name = "lnbconfig1";
      this.lnbconfig1.Size = new System.Drawing.Size(80, 21);
      this.lnbconfig1.TabIndex = 24;
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.circularMHZ);
      this.groupBox4.Controls.Add(this.label20);
      this.groupBox4.Controls.Add(this.cbandMHZ);
      this.groupBox4.Controls.Add(this.label21);
      this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox4.Location = new System.Drawing.Point(264, 16);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(176, 96);
      this.groupBox4.TabIndex = 35;
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
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.lnb1MHZ);
      this.groupBox3.Controls.Add(this.lnb1);
      this.groupBox3.Controls.Add(this.lnbswMHZ);
      this.groupBox3.Controls.Add(this.switchMHZ);
      this.groupBox3.Controls.Add(this.lnb0MHZ);
      this.groupBox3.Controls.Add(this.label22);
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox3.Location = new System.Drawing.Point(32, 16);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(176, 96);
      this.groupBox3.TabIndex = 34;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Ku-Band Config:";
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
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(6, 41);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(53, 16);
      this.mpLabel1.TabIndex = 8;
      this.mpLabel1.Text = "LNB#1";
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(6, 66);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(53, 16);
      this.mpLabel2.TabIndex = 32;
      this.mpLabel2.Text = "LNB#2";
      // 
      // mpLabel3
      // 
      this.mpLabel3.Location = new System.Drawing.Point(6, 90);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(53, 16);
      this.mpLabel3.TabIndex = 33;
      this.mpLabel3.Text = "LNB#3";
      this.mpLabel3.Click += new System.EventHandler(this.mpLabel3_Click);
      // 
      // mpLabel4
      // 
      this.mpLabel4.Location = new System.Drawing.Point(6, 113);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(53, 16);
      this.mpLabel4.TabIndex = 34;
      this.mpLabel4.Text = "LNB#4";
      // 
      // Wizard_DVBSTVSetup
      // 
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox4);
      this.Controls.Add(this.groupBox3);
      this.Name = "Wizard_DVBSTVSetup";
      this.Size = new System.Drawing.Size(488, 368);
      this.Load += new System.EventHandler(this.Wizard_DVBSTVSetup_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    TVCaptureDevice CaptureCard
    {
      get
      {
        TVCaptureCards cards = new TVCaptureCards();
        cards.LoadCaptureCards();
        foreach (TVCaptureDevice dev in cards.captureCards)
        {
          if (dev.Network == NetworkType.DVBS)
          {
            return dev;
          }
        }
        return null;
      }
    }


    public override void OnSectionActivated()
    {
      LoadDVBSSettings();
    }

    public override void SaveSettings()
    {
      SaveDVBSSettings();
    }


    void LoadDVBSSettings()
    {
      if (CaptureCard == null) return;
      string filename = String.Format(@"database\card_{0}.xml", CaptureCard.FriendlyName);

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        checkBox1.Checked = xmlreader.GetValueAsBool("dvb_ts_cards", "enablePlugins", false);
      }

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
      {
        lnb0MHZ.Text = xmlreader.GetValueAsInt("dvbs", "LNB0", 9750).ToString();
        lnb1MHZ.Text = xmlreader.GetValueAsInt("dvbs", "LNB1", 10600).ToString();
        lnbswMHZ.Text = xmlreader.GetValueAsInt("dvbs", "Switch", 11700).ToString();
        cbandMHZ.Text = xmlreader.GetValueAsInt("dvbs", "CBand", 5150).ToString();
        circularMHZ.Text = xmlreader.GetValueAsInt("dvbs", "Circular", 10750).ToString();
        useLNB1.Checked = true;
        useLNB2.Checked = xmlreader.GetValueAsBool("dvbs", "useLNB2", false);
        useLNB3.Checked = xmlreader.GetValueAsBool("dvbs", "useLNB3", false);
        useLNB4.Checked = xmlreader.GetValueAsBool("dvbs", "useLNB4", false);

        int lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb", 22);
        switch (lnbKhz)
        {
          case 0: lnbconfig1.SelectedItem = "0 KHz"; break;
          case 22: lnbconfig1.SelectedItem = "22 KHz"; break;
          case 33: lnbconfig1.SelectedItem = "33 KHz"; break;
          case 44: lnbconfig1.SelectedItem = "44 KHz"; break;
        }
        lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb2", 22);
        switch (lnbKhz)
        {
          case 0: lnbconfig2.SelectedItem = "0 KHz"; break;
          case 22: lnbconfig2.SelectedItem = "22 KHz"; break;
          case 33: lnbconfig2.SelectedItem = "33 KHz"; break;
          case 44: lnbconfig2.SelectedItem = "44 KHz"; break;
        }
        lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb3", 22);
        switch (lnbKhz)
        {
          case 0: lnbconfig3.SelectedItem = "0 KHz"; break;
          case 22: lnbconfig3.SelectedItem = "22 KHz"; break;
          case 33: lnbconfig3.SelectedItem = "33 KHz"; break;
          case 44: lnbconfig3.SelectedItem = "44 KHz"; break;
        }
        lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb4", 22);
        switch (lnbKhz)
        {
          case 0: lnbconfig4.SelectedItem = "0 KHz"; break;
          case 22: lnbconfig4.SelectedItem = "22 KHz"; break;
          case 33: lnbconfig4.SelectedItem = "33 KHz"; break;
          case 44: lnbconfig4.SelectedItem = "44 KHz"; break;
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
        int diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc", 1);
        if (diseqc == 0) diseqc = 1;
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
        diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc2", 1);
        if (diseqc == 0) diseqc = 1;
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
        diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc3", 1);
        if (diseqc == 0) diseqc = 1;
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
        diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc4", 1);
        if (diseqc == 0) diseqc = 1;
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


      }
    }

    void SaveDVBSSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("dvb_ts_cards", "enablePlugins", checkBox1.Checked);
      }

      TVCaptureCards cards = new TVCaptureCards();
      cards.LoadCaptureCards();
      foreach (TVCaptureDevice dev in cards.captureCards)
      {
        if (dev.Network == NetworkType.DVBS)
        {
          string filename = String.Format(@"database\card_{0}.xml", dev.FriendlyName);
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
            if (lnbconfig1.SelectedIndex >= 0)
            {
              switch (lnbconfig1.SelectedIndex)
              {
                case 0: xmlwriter.SetValue("dvbs", "lnb", 0); break;
                case 1: xmlwriter.SetValue("dvbs", "lnb", 22); break;
                case 2: xmlwriter.SetValue("dvbs", "lnb", 33); break;
                case 3: xmlwriter.SetValue("dvbs", "lnb", 44); break;
              }
            }
            if (lnbconfig2.SelectedIndex >= 0)
            {
              switch (lnbconfig2.SelectedIndex)
              {
                case 0: xmlwriter.SetValue("dvbs", "lnb2", 0); break;
                case 1: xmlwriter.SetValue("dvbs", "lnb2", 22); break;
                case 2: xmlwriter.SetValue("dvbs", "lnb2", 33); break;
                case 3: xmlwriter.SetValue("dvbs", "lnb2", 44); break;
              }
            }
            if (lnbconfig3.SelectedIndex >= 0)
            {
              switch (lnbconfig3.SelectedIndex)
              {
                case 0: xmlwriter.SetValue("dvbs", "lnb3", 0); break;
                case 1: xmlwriter.SetValue("dvbs", "lnb3", 22); break;
                case 2: xmlwriter.SetValue("dvbs", "lnb3", 33); break;
                case 3: xmlwriter.SetValue("dvbs", "lnb3", 44); break;
              }
            }
            if (lnbconfig4.SelectedIndex >= 0)
            {
              switch (lnbconfig4.SelectedIndex)
              {
                case 0: xmlwriter.SetValue("dvbs", "lnb4", 0); break;
                case 1: xmlwriter.SetValue("dvbs", "lnb4", 22); break;
                case 2: xmlwriter.SetValue("dvbs", "lnb4", 33); break;
                case 3: xmlwriter.SetValue("dvbs", "lnb4", 44); break;
              }
            }

            if (lnbkind1.SelectedIndex >= 0)
            {
              xmlwriter.SetValue("dvbs", "lnbKind", lnbkind1.SelectedIndex);
            }
            if (lnbkind2.SelectedIndex >= 0)
            {
              xmlwriter.SetValue("dvbs", "lnbKind2", lnbkind2.SelectedIndex);
            }
            if (lnbkind3.SelectedIndex >= 0)
            {
              xmlwriter.SetValue("dvbs", "lnbKind3", lnbkind3.SelectedIndex);
            }
            if (lnbkind4.SelectedIndex >= 0)
            {
              xmlwriter.SetValue("dvbs", "lnbKind4", lnbkind4.SelectedIndex);
            }
          }
        }
      }
    }

    private void useLNB2_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void useLNB3_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void Wizard_DVBSTVSetup_Load(object sender, EventArgs e)
    {

    }

    private void groupBox2_Enter(object sender, EventArgs e)
    {

    }

    private void mpLabel3_Click(object sender, EventArgs e)
    {

    }
  }
}
