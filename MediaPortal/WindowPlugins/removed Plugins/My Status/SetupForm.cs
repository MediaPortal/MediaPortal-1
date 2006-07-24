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
using System.Management;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using mbm5.MBMInfo;

namespace GUIStatus
{
  /// <summary>
  /// Summary description for SetupForm.
  /// </summary>
  public class SetupForm : System.Windows.Forms.Form, ISetupForm, IShowPlugin
  {
    private int[] aFan = { 0, 0, 0, 0 };
    private int[] aMhz = { 0, 0, 0, 0 };
    private int[] aPercentage = { 0, 0, 0, 0 };
    private int[] aTemperature = { 0, 0, 0, 0 };
    private int[] aVolt = { 0, 0, 0, 0, 0, 0, 0, 0 };
    private int statusVer = 1;
    private bool isMbm = false;
    private mbmSharedData MBMInfo = new mbmSharedData();
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPLabel label9;
    private MediaPortal.UserInterface.Controls.MPLabel label10;
    private MediaPortal.UserInterface.Controls.MPLabel label11;
    private MediaPortal.UserInterface.Controls.MPLabel label12;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPButton button1;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox2;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox3;
    private MediaPortal.UserInterface.Controls.MPLabel label7;
    private MediaPortal.UserInterface.Controls.MPLabel label8;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox4;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox5;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox6;
    private MediaPortal.UserInterface.Controls.MPLabel label13;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox7;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox8;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox9;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox10;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox11;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox12;
    private MediaPortal.UserInterface.Controls.MPLabel label14;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox15;
    private MediaPortal.UserInterface.Controls.MPLabel label15;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox13;
    private MediaPortal.UserInterface.Controls.MPButton button2;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox14;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox16;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox17;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPTextBox textBox1;
    private MediaPortal.UserInterface.Controls.MPLabel label16;
    private MediaPortal.UserInterface.Controls.MPButton button3;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    private MediaPortal.UserInterface.Controls.MPLabel label17;
    private MediaPortal.UserInterface.Controls.MPTextBox textBox2;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox18;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox19;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox20;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox21;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox22;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox23;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox24;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox25;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox26;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox27;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox28;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox29;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox30;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox31;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox32;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox33;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox34;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox35;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox36;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox37;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox38;
    private MediaPortal.UserInterface.Controls.MPLabel label18;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox39;
    private MediaPortal.UserInterface.Controls.MPLabel label19;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox40;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox41;
    private MediaPortal.UserInterface.Controls.MPLabel label20;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox3;
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
      LoadSettings();
      //		
      // TODO: Add any constructor code after InitializeComponent call
      //
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
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label10 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label11 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label12 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.button1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBox1 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox2 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox4 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox5 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox6 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label13 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox7 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox8 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox9 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox10 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox11 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox12 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label14 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox15 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label15 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox13 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.button2 = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBox14 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox16 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox17 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.textBox1 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label16 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.button3 = new MediaPortal.UserInterface.Controls.MPButton();
      this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
      this.label17 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBox2 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.checkBox18 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox19 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox20 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox21 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox22 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox23 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox24 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox25 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox26 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox27 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox28 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox29 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox30 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox31 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox32 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox33 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox34 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox35 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox36 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox37 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox38 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label18 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox39 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label19 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox40 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox41 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label20 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBox1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label1.Location = new System.Drawing.Point(24, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(96, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "MBM Version:";
      // 
      // label2
      // 
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label2.Location = new System.Drawing.Point(120, 16);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(232, 16);
      this.label2.TabIndex = 1;
      // 
      // label3
      // 
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label3.Location = new System.Drawing.Point(120, 64);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(544, 16);
      this.label3.TabIndex = 3;
      // 
      // label4
      // 
      this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label4.Location = new System.Drawing.Point(24, 64);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(88, 16);
      this.label4.TabIndex = 2;
      this.label4.Text = "MBM Path:";
      // 
      // label5
      // 
      this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label5.Location = new System.Drawing.Point(120, 40);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(232, 16);
      this.label5.TabIndex = 5;
      // 
      // label6
      // 
      this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label6.Location = new System.Drawing.Point(24, 40);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(104, 16);
      this.label6.TabIndex = 4;
      this.label6.Text = "MBM started at:";
      // 
      // label9
      // 
      this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label9.Location = new System.Drawing.Point(480, 40);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(184, 16);
      this.label9.TabIndex = 9;
      // 
      // label10
      // 
      this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label10.Location = new System.Drawing.Point(368, 40);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(80, 16);
      this.label10.TabIndex = 8;
      this.label10.Text = "SMB chip:";
      // 
      // label11
      // 
      this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label11.Location = new System.Drawing.Point(480, 16);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(184, 16);
      this.label11.TabIndex = 7;
      // 
      // label12
      // 
      this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label12.Location = new System.Drawing.Point(368, 16);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(104, 16);
      this.label12.TabIndex = 6;
      this.label12.Text = "MBM last update:";
      // 
      // groupBox1
      // 
      this.groupBox1.Location = new System.Drawing.Point(304, 280);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(376, 8);
      this.groupBox1.TabIndex = 10;
      this.groupBox1.TabStop = false;
      // 
      // button1
      // 
      this.button1.Enabled = false;
      this.button1.Location = new System.Drawing.Point(584, 392);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(88, 24);
      this.button1.TabIndex = 11;
      this.button1.Text = "OK";
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // checkBox1
      // 
      this.checkBox1.Enabled = false;
      this.checkBox1.Location = new System.Drawing.Point(32, 136);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(104, 16);
      this.checkBox1.TabIndex = 13;
      // 
      // checkBox2
      // 
      this.checkBox2.Enabled = false;
      this.checkBox2.Location = new System.Drawing.Point(32, 160);
      this.checkBox2.Name = "checkBox2";
      this.checkBox2.Size = new System.Drawing.Size(104, 16);
      this.checkBox2.TabIndex = 14;
      // 
      // checkBox3
      // 
      this.checkBox3.Enabled = false;
      this.checkBox3.Location = new System.Drawing.Point(32, 184);
      this.checkBox3.Name = "checkBox3";
      this.checkBox3.Size = new System.Drawing.Size(104, 16);
      this.checkBox3.TabIndex = 15;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(16, 112);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(72, 16);
      this.label7.TabIndex = 16;
      this.label7.Text = "Temperature";
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(136, 112);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(48, 16);
      this.label8.TabIndex = 20;
      this.label8.Text = "Fan";
      // 
      // checkBox4
      // 
      this.checkBox4.Enabled = false;
      this.checkBox4.Location = new System.Drawing.Point(136, 184);
      this.checkBox4.Name = "checkBox4";
      this.checkBox4.Size = new System.Drawing.Size(104, 16);
      this.checkBox4.TabIndex = 19;
      // 
      // checkBox5
      // 
      this.checkBox5.Enabled = false;
      this.checkBox5.Location = new System.Drawing.Point(136, 160);
      this.checkBox5.Name = "checkBox5";
      this.checkBox5.Size = new System.Drawing.Size(104, 16);
      this.checkBox5.TabIndex = 18;
      // 
      // checkBox6
      // 
      this.checkBox6.Enabled = false;
      this.checkBox6.Location = new System.Drawing.Point(136, 136);
      this.checkBox6.Name = "checkBox6";
      this.checkBox6.Size = new System.Drawing.Size(104, 16);
      this.checkBox6.TabIndex = 17;
      // 
      // label13
      // 
      this.label13.Location = new System.Drawing.Point(288, 112);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(48, 16);
      this.label13.TabIndex = 24;
      this.label13.Text = "Voltage";
      // 
      // checkBox7
      // 
      this.checkBox7.Enabled = false;
      this.checkBox7.Location = new System.Drawing.Point(240, 184);
      this.checkBox7.Name = "checkBox7";
      this.checkBox7.Size = new System.Drawing.Size(96, 16);
      this.checkBox7.TabIndex = 23;
      // 
      // checkBox8
      // 
      this.checkBox8.Enabled = false;
      this.checkBox8.Location = new System.Drawing.Point(240, 160);
      this.checkBox8.Name = "checkBox8";
      this.checkBox8.Size = new System.Drawing.Size(96, 16);
      this.checkBox8.TabIndex = 22;
      // 
      // checkBox9
      // 
      this.checkBox9.Enabled = false;
      this.checkBox9.Location = new System.Drawing.Point(240, 136);
      this.checkBox9.Name = "checkBox9";
      this.checkBox9.Size = new System.Drawing.Size(96, 16);
      this.checkBox9.TabIndex = 21;
      // 
      // checkBox10
      // 
      this.checkBox10.Enabled = false;
      this.checkBox10.Location = new System.Drawing.Point(336, 184);
      this.checkBox10.Name = "checkBox10";
      this.checkBox10.Size = new System.Drawing.Size(112, 16);
      this.checkBox10.TabIndex = 27;
      // 
      // checkBox11
      // 
      this.checkBox11.Enabled = false;
      this.checkBox11.Location = new System.Drawing.Point(336, 160);
      this.checkBox11.Name = "checkBox11";
      this.checkBox11.Size = new System.Drawing.Size(112, 16);
      this.checkBox11.TabIndex = 26;
      // 
      // checkBox12
      // 
      this.checkBox12.Enabled = false;
      this.checkBox12.Location = new System.Drawing.Point(336, 136);
      this.checkBox12.Name = "checkBox12";
      this.checkBox12.Size = new System.Drawing.Size(112, 16);
      this.checkBox12.TabIndex = 25;
      // 
      // label14
      // 
      this.label14.Location = new System.Drawing.Point(440, 112);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(64, 16);
      this.label14.TabIndex = 31;
      this.label14.Text = "Percentage";
      // 
      // checkBox15
      // 
      this.checkBox15.Enabled = false;
      this.checkBox15.Location = new System.Drawing.Point(448, 136);
      this.checkBox15.Name = "checkBox15";
      this.checkBox15.Size = new System.Drawing.Size(96, 16);
      this.checkBox15.TabIndex = 28;
      // 
      // label15
      // 
      this.label15.Location = new System.Drawing.Point(544, 112);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(64, 16);
      this.label15.TabIndex = 33;
      this.label15.Text = "mHz";
      // 
      // checkBox13
      // 
      this.checkBox13.Enabled = false;
      this.checkBox13.Location = new System.Drawing.Point(552, 136);
      this.checkBox13.Name = "checkBox13";
      this.checkBox13.Size = new System.Drawing.Size(136, 16);
      this.checkBox13.TabIndex = 32;
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(480, 392);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(88, 24);
      this.button2.TabIndex = 34;
      this.button2.Text = "Load Sensors";
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // checkBox14
      // 
      this.checkBox14.Enabled = false;
      this.checkBox14.Location = new System.Drawing.Point(32, 224);
      this.checkBox14.Name = "checkBox14";
      this.checkBox14.Size = new System.Drawing.Size(56, 24);
      this.checkBox14.TabIndex = 35;
      this.checkBox14.Text = "HD C:";
      // 
      // checkBox16
      // 
      this.checkBox16.Enabled = false;
      this.checkBox16.Location = new System.Drawing.Point(32, 248);
      this.checkBox16.Name = "checkBox16";
      this.checkBox16.Size = new System.Drawing.Size(56, 24);
      this.checkBox16.TabIndex = 36;
      this.checkBox16.Text = "HD D:";
      // 
      // checkBox17
      // 
      this.checkBox17.Enabled = false;
      this.checkBox17.Location = new System.Drawing.Point(32, 272);
      this.checkBox17.Name = "checkBox17";
      this.checkBox17.Size = new System.Drawing.Size(56, 24);
      this.checkBox17.TabIndex = 37;
      this.checkBox17.Text = "HD E:";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.groupBox3);
      this.groupBox2.Location = new System.Drawing.Point(24, 208);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(640, 8);
      this.groupBox2.TabIndex = 38;
      this.groupBox2.TabStop = false;
      // 
      // groupBox3
      // 
      this.groupBox3.Location = new System.Drawing.Point(0, 0);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(640, 8);
      this.groupBox3.TabIndex = 39;
      this.groupBox3.TabStop = false;
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(456, 224);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(200, 20);
      this.textBox1.TabIndex = 39;
      this.textBox1.Text = "C:\\WINDOWS\\Media";
      // 
      // label16
      // 
      this.label16.Location = new System.Drawing.Point(296, 232);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(144, 16);
      this.label16.TabIndex = 40;
      this.label16.Text = "Status Alarm Sound Folder";
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(664, 224);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(24, 23);
      this.button3.TabIndex = 41;
      this.button3.Text = "...";
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // label17
      // 
      this.label17.Location = new System.Drawing.Point(296, 256);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(176, 16);
      this.label17.TabIndex = 42;
      this.label17.Text = "Alarm  threshold for HD (in MB)";
      // 
      // textBox2
      // 
      this.textBox2.Location = new System.Drawing.Point(456, 248);
      this.textBox2.Name = "textBox2";
      this.textBox2.Size = new System.Drawing.Size(72, 20);
      this.textBox2.TabIndex = 43;
      this.textBox2.Text = "200";
      // 
      // checkBox18
      // 
      this.checkBox18.Enabled = false;
      this.checkBox18.Location = new System.Drawing.Point(32, 344);
      this.checkBox18.Name = "checkBox18";
      this.checkBox18.Size = new System.Drawing.Size(56, 24);
      this.checkBox18.TabIndex = 46;
      this.checkBox18.Text = "HD H:";
      // 
      // checkBox19
      // 
      this.checkBox19.Enabled = false;
      this.checkBox19.Location = new System.Drawing.Point(32, 320);
      this.checkBox19.Name = "checkBox19";
      this.checkBox19.Size = new System.Drawing.Size(56, 24);
      this.checkBox19.TabIndex = 45;
      this.checkBox19.Text = "HD G:";
      // 
      // checkBox20
      // 
      this.checkBox20.Enabled = false;
      this.checkBox20.Location = new System.Drawing.Point(32, 296);
      this.checkBox20.Name = "checkBox20";
      this.checkBox20.Size = new System.Drawing.Size(56, 24);
      this.checkBox20.TabIndex = 44;
      this.checkBox20.Text = "HD F:";
      // 
      // checkBox21
      // 
      this.checkBox21.Enabled = false;
      this.checkBox21.Location = new System.Drawing.Point(96, 344);
      this.checkBox21.Name = "checkBox21";
      this.checkBox21.Size = new System.Drawing.Size(64, 24);
      this.checkBox21.TabIndex = 52;
      this.checkBox21.Text = "HD N:";
      // 
      // checkBox22
      // 
      this.checkBox22.Enabled = false;
      this.checkBox22.Location = new System.Drawing.Point(96, 320);
      this.checkBox22.Name = "checkBox22";
      this.checkBox22.Size = new System.Drawing.Size(64, 24);
      this.checkBox22.TabIndex = 51;
      this.checkBox22.Text = "HD M:";
      // 
      // checkBox23
      // 
      this.checkBox23.Enabled = false;
      this.checkBox23.Location = new System.Drawing.Point(96, 296);
      this.checkBox23.Name = "checkBox23";
      this.checkBox23.Size = new System.Drawing.Size(64, 24);
      this.checkBox23.TabIndex = 50;
      this.checkBox23.Text = "HD L:";
      // 
      // checkBox24
      // 
      this.checkBox24.Enabled = false;
      this.checkBox24.Location = new System.Drawing.Point(96, 272);
      this.checkBox24.Name = "checkBox24";
      this.checkBox24.Size = new System.Drawing.Size(64, 24);
      this.checkBox24.TabIndex = 49;
      this.checkBox24.Text = "HD K:";
      // 
      // checkBox25
      // 
      this.checkBox25.Enabled = false;
      this.checkBox25.Location = new System.Drawing.Point(96, 248);
      this.checkBox25.Name = "checkBox25";
      this.checkBox25.Size = new System.Drawing.Size(64, 24);
      this.checkBox25.TabIndex = 48;
      this.checkBox25.Text = "HD J:";
      // 
      // checkBox26
      // 
      this.checkBox26.Enabled = false;
      this.checkBox26.Location = new System.Drawing.Point(96, 224);
      this.checkBox26.Name = "checkBox26";
      this.checkBox26.Size = new System.Drawing.Size(64, 24);
      this.checkBox26.TabIndex = 47;
      this.checkBox26.Text = "HD I:";
      // 
      // checkBox27
      // 
      this.checkBox27.Enabled = false;
      this.checkBox27.Location = new System.Drawing.Point(160, 344);
      this.checkBox27.Name = "checkBox27";
      this.checkBox27.Size = new System.Drawing.Size(64, 24);
      this.checkBox27.TabIndex = 58;
      this.checkBox27.Text = "HD T:";
      // 
      // checkBox28
      // 
      this.checkBox28.Enabled = false;
      this.checkBox28.Location = new System.Drawing.Point(160, 320);
      this.checkBox28.Name = "checkBox28";
      this.checkBox28.Size = new System.Drawing.Size(64, 24);
      this.checkBox28.TabIndex = 57;
      this.checkBox28.Text = "HD S:";
      // 
      // checkBox29
      // 
      this.checkBox29.Enabled = false;
      this.checkBox29.Location = new System.Drawing.Point(160, 296);
      this.checkBox29.Name = "checkBox29";
      this.checkBox29.Size = new System.Drawing.Size(64, 24);
      this.checkBox29.TabIndex = 56;
      this.checkBox29.Text = "HD R:";
      // 
      // checkBox30
      // 
      this.checkBox30.Enabled = false;
      this.checkBox30.Location = new System.Drawing.Point(160, 272);
      this.checkBox30.Name = "checkBox30";
      this.checkBox30.Size = new System.Drawing.Size(64, 24);
      this.checkBox30.TabIndex = 55;
      this.checkBox30.Text = "HD Q:";
      // 
      // checkBox31
      // 
      this.checkBox31.Enabled = false;
      this.checkBox31.Location = new System.Drawing.Point(160, 248);
      this.checkBox31.Name = "checkBox31";
      this.checkBox31.Size = new System.Drawing.Size(64, 24);
      this.checkBox31.TabIndex = 54;
      this.checkBox31.Text = "HD P:";
      // 
      // checkBox32
      // 
      this.checkBox32.Enabled = false;
      this.checkBox32.Location = new System.Drawing.Point(160, 224);
      this.checkBox32.Name = "checkBox32";
      this.checkBox32.Size = new System.Drawing.Size(64, 24);
      this.checkBox32.TabIndex = 53;
      this.checkBox32.Text = "HD O :";
      // 
      // checkBox33
      // 
      this.checkBox33.Enabled = false;
      this.checkBox33.Location = new System.Drawing.Point(224, 344);
      this.checkBox33.Name = "checkBox33";
      this.checkBox33.Size = new System.Drawing.Size(64, 24);
      this.checkBox33.TabIndex = 64;
      this.checkBox33.Text = "HD Z:";
      // 
      // checkBox34
      // 
      this.checkBox34.Enabled = false;
      this.checkBox34.Location = new System.Drawing.Point(224, 320);
      this.checkBox34.Name = "checkBox34";
      this.checkBox34.Size = new System.Drawing.Size(64, 24);
      this.checkBox34.TabIndex = 63;
      this.checkBox34.Text = "HD Y:";
      // 
      // checkBox35
      // 
      this.checkBox35.Enabled = false;
      this.checkBox35.Location = new System.Drawing.Point(224, 296);
      this.checkBox35.Name = "checkBox35";
      this.checkBox35.Size = new System.Drawing.Size(64, 24);
      this.checkBox35.TabIndex = 62;
      this.checkBox35.Text = "HD X:";
      // 
      // checkBox36
      // 
      this.checkBox36.Enabled = false;
      this.checkBox36.Location = new System.Drawing.Point(224, 272);
      this.checkBox36.Name = "checkBox36";
      this.checkBox36.Size = new System.Drawing.Size(64, 24);
      this.checkBox36.TabIndex = 61;
      this.checkBox36.Text = "HD W:";
      // 
      // checkBox37
      // 
      this.checkBox37.Enabled = false;
      this.checkBox37.Location = new System.Drawing.Point(224, 248);
      this.checkBox37.Name = "checkBox37";
      this.checkBox37.Size = new System.Drawing.Size(64, 24);
      this.checkBox37.TabIndex = 60;
      this.checkBox37.Text = "HD U:";
      // 
      // checkBox38
      // 
      this.checkBox38.Enabled = false;
      this.checkBox38.Location = new System.Drawing.Point(224, 224);
      this.checkBox38.Name = "checkBox38";
      this.checkBox38.Size = new System.Drawing.Size(64, 24);
      this.checkBox38.TabIndex = 59;
      this.checkBox38.Text = "HD U :";
      // 
      // label18
      // 
      this.label18.Location = new System.Drawing.Point(296, 304);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(152, 16);
      this.label18.TabIndex = 65;
      this.label18.Text = "Enable statusbar";
      // 
      // checkBox39
      // 
      this.checkBox39.Enabled = false;
      this.checkBox39.Location = new System.Drawing.Point(456, 296);
      this.checkBox39.Name = "checkBox39";
      this.checkBox39.Size = new System.Drawing.Size(48, 24);
      this.checkBox39.TabIndex = 66;
      this.checkBox39.CheckedChanged += new System.EventHandler(this.checkBox39_CheckedChanged);
      // 
      // label19
      // 
      this.label19.Location = new System.Drawing.Point(296, 328);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(152, 16);
      this.label19.TabIndex = 67;
      this.label19.Text = "Statusbar positon";
      // 
      // checkBox40
      // 
      this.checkBox40.Enabled = false;
      this.checkBox40.Location = new System.Drawing.Point(456, 328);
      this.checkBox40.Name = "checkBox40";
      this.checkBox40.Size = new System.Drawing.Size(48, 16);
      this.checkBox40.TabIndex = 68;
      this.checkBox40.Text = "Top";
      this.checkBox40.CheckedChanged += new System.EventHandler(this.checkBox40_CheckedChanged);
      // 
      // checkBox41
      // 
      this.checkBox41.Enabled = false;
      this.checkBox41.Location = new System.Drawing.Point(520, 328);
      this.checkBox41.Name = "checkBox41";
      this.checkBox41.Size = new System.Drawing.Size(64, 16);
      this.checkBox41.TabIndex = 69;
      this.checkBox41.Text = "Bottom";
      this.checkBox41.CheckedChanged += new System.EventHandler(this.checkBox41_CheckedChanged);
      // 
      // label20
      // 
      this.label20.Location = new System.Drawing.Point(296, 352);
      this.label20.Name = "label20";
      this.label20.Size = new System.Drawing.Size(152, 16);
      this.label20.TabIndex = 70;
      this.label20.Text = "Select statusbar sensor";
      // 
      // comboBox1
      // 
      this.comboBox1.Enabled = false;
      this.comboBox1.Location = new System.Drawing.Point(456, 352);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(216, 21);
      this.comboBox1.TabIndex = 71;
      this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
      // 
      // SetupForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(704, 430);
      this.Controls.Add(this.comboBox1);
      this.Controls.Add(this.label20);
      this.Controls.Add(this.checkBox41);
      this.Controls.Add(this.checkBox40);
      this.Controls.Add(this.label19);
      this.Controls.Add(this.checkBox39);
      this.Controls.Add(this.label18);
      this.Controls.Add(this.checkBox33);
      this.Controls.Add(this.checkBox34);
      this.Controls.Add(this.checkBox35);
      this.Controls.Add(this.checkBox36);
      this.Controls.Add(this.checkBox37);
      this.Controls.Add(this.checkBox38);
      this.Controls.Add(this.checkBox27);
      this.Controls.Add(this.checkBox28);
      this.Controls.Add(this.checkBox29);
      this.Controls.Add(this.checkBox30);
      this.Controls.Add(this.checkBox31);
      this.Controls.Add(this.checkBox32);
      this.Controls.Add(this.checkBox21);
      this.Controls.Add(this.checkBox22);
      this.Controls.Add(this.checkBox23);
      this.Controls.Add(this.checkBox24);
      this.Controls.Add(this.checkBox25);
      this.Controls.Add(this.checkBox26);
      this.Controls.Add(this.checkBox18);
      this.Controls.Add(this.checkBox19);
      this.Controls.Add(this.checkBox20);
      this.Controls.Add(this.textBox2);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.label17);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.label16);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.checkBox17);
      this.Controls.Add(this.checkBox16);
      this.Controls.Add(this.checkBox14);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.label15);
      this.Controls.Add(this.checkBox13);
      this.Controls.Add(this.label14);
      this.Controls.Add(this.checkBox15);
      this.Controls.Add(this.checkBox10);
      this.Controls.Add(this.checkBox11);
      this.Controls.Add(this.checkBox12);
      this.Controls.Add(this.label13);
      this.Controls.Add(this.checkBox7);
      this.Controls.Add(this.checkBox8);
      this.Controls.Add(this.checkBox9);
      this.Controls.Add(this.label8);
      this.Controls.Add(this.checkBox4);
      this.Controls.Add(this.checkBox5);
      this.Controls.Add(this.checkBox6);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.checkBox3);
      this.Controls.Add(this.checkBox2);
      this.Controls.Add(this.checkBox1);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.label9);
      this.Controls.Add(this.label10);
      this.Controls.Add(this.label11);
      this.Controls.Add(this.label12);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Name = "SetupForm";
      this.Text = "SetupForm";
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    #region plugin vars

    public string PluginName()
    {
      return "My Status";
    }

    public string Description()
    {
      return "Displays the system status of your HTPC";
    }

    public string Author()
    {
      return "Gucky62";
    }

    public void ShowPlugin()
    {
      ShowDialog();
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public bool CanEnable()
    {
      return true;
    }

    public bool HasSetup()
    {
      return true;
    }

    public int GetWindowId()
    {
      return 755;
    }

    /// <summary>
    /// If the plugin should have its own button on the home screen then it
    /// should return true to this method, otherwise if it should not be on home
    /// it should return false
    /// </summary>
    /// <param name="strButtonText">text the button should have</param>
    /// <param name="strButtonImage">image for the button, or empty for default</param>
    /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
    /// <param name="strPictureImage">subpicture for the button or empty for none</param>
    /// <returns>true  : plugin needs its own button on home
    ///          false : plugin does not need its own button on home</returns>
    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {

      strButtonText = GUILocalizeStrings.Get(1950);
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return true;
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion

    private void button1_Click(object sender, System.EventArgs e)
    {
      SaveSettings();
      this.Visible = false;
    }

    #region Private Methods
    /// <summary>
    /// Saves my status settings to the profile xml.
    /// </summary>
    private void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("status", "status_is_mbm", isMbm);
        xmlwriter.SetValueAsBool("status", "status_bar_show", checkBox39.Checked);
        xmlwriter.SetValueAsBool("status", "status_bar_top", checkBox40.Checked);
        xmlwriter.SetValueAsBool("status", "status_bar_bottom", checkBox41.Checked);
        if (checkBox39.Checked == true) xmlwriter.SetValue("status", "status_bar_sensor", comboBox1.Items[comboBox1.SelectedIndex]);
        xmlwriter.SetValue("status", "status_sound_folder", textBox1.Text);
        xmlwriter.SetValue("status", "status_hd_threshold", textBox2.Text);
        xmlwriter.SetValueAsBool("status", "status_temp1", checkBox1.Checked);
        xmlwriter.SetValue("status", "status_temp1i", aTemperature[1]);
        xmlwriter.SetValueAsBool("status", "status_temp2", checkBox2.Checked);
        xmlwriter.SetValue("status", "status_temp2i", aTemperature[2]);
        xmlwriter.SetValueAsBool("status", "status_temp3", checkBox3.Checked);
        xmlwriter.SetValue("status", "status_temp3i", aTemperature[3]);

        xmlwriter.SetValueAsBool("status", "status_fan1", checkBox6.Checked);
        xmlwriter.SetValue("status", "status_fan1i", aFan[1]);
        xmlwriter.SetValueAsBool("status", "status_fan2", checkBox5.Checked);
        xmlwriter.SetValue("status", "status_fan2i", aFan[2]);
        xmlwriter.SetValueAsBool("status", "status_fan3", checkBox4.Checked);
        xmlwriter.SetValue("status", "status_fan3i", aFan[3]);

        xmlwriter.SetValueAsBool("status", "status_volt1", checkBox9.Checked);
        xmlwriter.SetValue("status", "status_volt1i", aVolt[1]);
        xmlwriter.SetValueAsBool("status", "status_volt2", checkBox8.Checked);
        xmlwriter.SetValue("status", "status_volt2i", aVolt[2]);
        xmlwriter.SetValueAsBool("status", "status_volt3", checkBox7.Checked);
        xmlwriter.SetValue("status", "status_volt3i", aVolt[3]);
        xmlwriter.SetValueAsBool("status", "status_volt4", checkBox12.Checked);
        xmlwriter.SetValue("status", "status_volt4i", aVolt[4]);
        xmlwriter.SetValueAsBool("status", "status_volt5", checkBox11.Checked);
        xmlwriter.SetValue("status", "status_volt5i", aVolt[5]);
        xmlwriter.SetValueAsBool("status", "status_volt6", checkBox11.Checked);
        xmlwriter.SetValue("status", "status_volt6i", aVolt[6]);

        xmlwriter.SetValueAsBool("status", "status_mhz1", checkBox13.Checked);
        xmlwriter.SetValue("status", "status_mhz1i", aMhz[1]);

        xmlwriter.SetValueAsBool("status", "status_perc1", checkBox15.Checked);
        xmlwriter.SetValue("status", "status_perc1i", aPercentage[1]);

        xmlwriter.SetValueAsBool("status", "status_lwc", checkBox14.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwd", checkBox16.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwe", checkBox17.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwf", checkBox20.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwg", checkBox19.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwh", checkBox18.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwi", checkBox26.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwj", checkBox25.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwk", checkBox24.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwl", checkBox23.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwm", checkBox22.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwn", checkBox21.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwo", checkBox32.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwp", checkBox31.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwq", checkBox30.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwr", checkBox29.Checked);
        xmlwriter.SetValueAsBool("status", "status_lws", checkBox28.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwt", checkBox27.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwu", checkBox38.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwv", checkBox37.Checked);
        xmlwriter.SetValueAsBool("status", "status_lww", checkBox36.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwx", checkBox35.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwy", checkBox34.Checked);
        xmlwriter.SetValueAsBool("status", "status_lwz", checkBox33.Checked);
      }
    }

    /// <summary>
    /// Loads my status settings from the profile xml.
    /// </summary>
    private void LoadSettings()
    {
      bool isver = false;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        int ver = 0;
        ver = xmlreader.GetValueAsInt("status", "status_ver", 0);
        if (ver != statusVer)
        {
          isver = false;
        }
        else
        {
          isver = true;
          textBox2.Text = xmlreader.GetValueAsString("status", "status_hd_threshold", "100");
          textBox1.Text = xmlreader.GetValueAsString("status", "status_sound_folder", "C:\\windows\\media");
          checkBox1.Checked = xmlreader.GetValueAsBool("status", "status_temp1", false);
          checkBox2.Checked = xmlreader.GetValueAsBool("status", "status_temp2", false);
          checkBox3.Checked = xmlreader.GetValueAsBool("status", "status_temp3", false);
          checkBox6.Checked = xmlreader.GetValueAsBool("status", "status_fan1", false);
          checkBox5.Checked = xmlreader.GetValueAsBool("status", "status_fan2", false);
          checkBox4.Checked = xmlreader.GetValueAsBool("status", "status_fan3", false);
          checkBox9.Checked = xmlreader.GetValueAsBool("status", "status_volt1", false);
          checkBox8.Checked = xmlreader.GetValueAsBool("status", "status_volt2", false);
          checkBox7.Checked = xmlreader.GetValueAsBool("status", "status_volt3", false);
          checkBox12.Checked = xmlreader.GetValueAsBool("status", "status_volt4", false);
          checkBox11.Checked = xmlreader.GetValueAsBool("status", "status_volt5", false);
          checkBox10.Checked = xmlreader.GetValueAsBool("status", "status_volt6", false);
          checkBox13.Checked = xmlreader.GetValueAsBool("status", "status_mhz1", false);
          checkBox15.Checked = xmlreader.GetValueAsBool("status", "status_perc1", false);
          checkBox14.Checked = xmlreader.GetValueAsBool("status", "status_lwc", false);
          checkBox16.Checked = xmlreader.GetValueAsBool("status", "status_lwd", false);
          checkBox17.Checked = xmlreader.GetValueAsBool("status", "status_lwe", false);
          checkBox20.Checked = xmlreader.GetValueAsBool("status", "status_lwf", false);
          checkBox19.Checked = xmlreader.GetValueAsBool("status", "status_lwg", false);
          checkBox18.Checked = xmlreader.GetValueAsBool("status", "status_lwh", false);
          checkBox26.Checked = xmlreader.GetValueAsBool("status", "status_lwi", false);
          checkBox25.Checked = xmlreader.GetValueAsBool("status", "status_lwj", false);
          checkBox24.Checked = xmlreader.GetValueAsBool("status", "status_lwk", false);
          checkBox23.Checked = xmlreader.GetValueAsBool("status", "status_lwl", false);
          checkBox22.Checked = xmlreader.GetValueAsBool("status", "status_lwm", false);
          checkBox21.Checked = xmlreader.GetValueAsBool("status", "status_lwn", false);
          checkBox32.Checked = xmlreader.GetValueAsBool("status", "status_lwo", false);
          checkBox31.Checked = xmlreader.GetValueAsBool("status", "status_lwp", false);
          checkBox30.Checked = xmlreader.GetValueAsBool("status", "status_lwq", false);
          checkBox29.Checked = xmlreader.GetValueAsBool("status", "status_lwr", false);
          checkBox28.Checked = xmlreader.GetValueAsBool("status", "status_lws", false);
          checkBox27.Checked = xmlreader.GetValueAsBool("status", "status_lwt", false);
          checkBox38.Checked = xmlreader.GetValueAsBool("status", "status_lwu", false);
          checkBox37.Checked = xmlreader.GetValueAsBool("status", "status_lwv", false);
          checkBox36.Checked = xmlreader.GetValueAsBool("status", "status_lww", false);
          checkBox35.Checked = xmlreader.GetValueAsBool("status", "status_lwx", false);
          checkBox34.Checked = xmlreader.GetValueAsBool("status", "status_lwy", false);
          checkBox33.Checked = xmlreader.GetValueAsBool("status", "status_lwz", false);
        }
      }
      if (isver == false)
      {
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          xmlwriter.SetValue("status", "status_ver", statusVer);
        }
      }
    }

    #endregion

    /// <summary>
    /// Returns the Type of given Drive. 3=HD 5=CD
    /// </summary>
    private int GetDriveType(string lw)
    {
      ManagementObjectSearcher query;
      ManagementObjectCollection queryCollection;
      System.Management.ObjectQuery oq;
      string stringMachineName = "localhost";
      int m = 0;
      //Connect to the remote computer
      ConnectionOptions co = new ConnectionOptions();

      //Point to machine
      System.Management.ManagementScope ms = new System.Management.ManagementScope("\\\\" + stringMachineName + "\\root\\cimv2", co);

      oq = new System.Management.ObjectQuery("SELECT * FROM Win32_LogicalDisk WHERE DeviceID = '" + lw + "'");
      query = new ManagementObjectSearcher(ms, oq);
      queryCollection = query.Get();
      foreach (ManagementObject mo in queryCollection)
      {
        m = Convert.ToInt32(mo["DriveType"]);
      }
      if (m == 4) m = 3; // shows Netdrives
      if (m == 2) m = 3; // shows Cardreader
      return m;
    }

    private void button2_Click(object sender, System.EventArgs e)
    {
      int nFan = 0;
      int nMhz = 0;
      int nPercentage = 0;
      int nTemperature = 0;
      int nVoltage = 0;
      checkBox39.Enabled = true;
      if (GetDriveType("C:") == 3)  // 3 = hd 5 = cd
      {
        checkBox14.Enabled = true;
        comboBox1.Items.Add("C:");
      }
      else checkBox14.Checked = false;
      if (GetDriveType("D:") == 3)  // 3 = hd 5 = cd
      {
        checkBox16.Enabled = true;
        comboBox1.Items.Add("D:");
      }
      else checkBox16.Checked = false;
      if (GetDriveType("E:") == 3)  // 3 = hd 5 = cd
      {
        checkBox17.Enabled = true;
        comboBox1.Items.Add("E:");
      }
      else checkBox17.Checked = false;
      if (GetDriveType("F:") == 3)  // 3 = hd 5 = cd
      {
        checkBox20.Enabled = true;
        comboBox1.Items.Add("F:");
      }
      else checkBox20.Checked = false;
      if (GetDriveType("G:") == 3)  // 3 = hd 5 = cd
      {
        checkBox19.Enabled = true;
        comboBox1.Items.Add("G:");
      }
      else checkBox19.Checked = false;
      if (GetDriveType("H:") == 3)  // 3 = hd 5 = cd
      {
        checkBox18.Enabled = true;
        comboBox1.Items.Add("H:");
      }
      else checkBox18.Checked = false;
      if (GetDriveType("I:") == 3)  // 3 = hd 5 = cd
      {
        checkBox26.Enabled = true;
        comboBox1.Items.Add("I:");
      }
      else checkBox26.Checked = false;
      if (GetDriveType("J:") == 3)  // 3 = hd 5 = cd
      {
        checkBox25.Enabled = true;
        comboBox1.Items.Add("J:");
      }
      else checkBox25.Checked = false;
      if (GetDriveType("K:") == 3)  // 3 = hd 5 = cd
      {
        checkBox24.Enabled = true;
        comboBox1.Items.Add("K:");
      }
      else checkBox24.Checked = false;
      if (GetDriveType("L:") == 3)  // 3 = hd 5 = cd
      {
        checkBox23.Enabled = true;
        comboBox1.Items.Add("L:");
      }
      else checkBox23.Checked = false;
      if (GetDriveType("M:") == 3)  // 3 = hd 5 = cd
      {
        checkBox22.Enabled = true;
        comboBox1.Items.Add("M:");
      }
      else checkBox22.Checked = false;
      if (GetDriveType("N:") == 3)  // 3 = hd 5 = cd
      {
        checkBox21.Enabled = true;
        comboBox1.Items.Add("N:");
      }
      else checkBox21.Checked = false;
      if (GetDriveType("O:") == 3)  // 3 = hd 5 = cd
      {
        checkBox32.Enabled = true;
        comboBox1.Items.Add("O:");
      }
      else checkBox32.Checked = false;
      if (GetDriveType("P:") == 3)  // 3 = hd 5 = cd
      {
        checkBox31.Enabled = true;
        comboBox1.Items.Add("P:");
      }
      else checkBox31.Checked = false;
      if (GetDriveType("Q:") == 3)  // 3 = hd 5 = cd
      {
        checkBox30.Enabled = true;
        comboBox1.Items.Add("Q:");
      }
      else checkBox30.Checked = false;
      if (GetDriveType("R:") == 3)  // 3 = hd 5 = cd
      {
        checkBox29.Enabled = true;
        comboBox1.Items.Add("R:");
      }
      else checkBox29.Checked = false;
      if (GetDriveType("S:") == 3)  // 3 = hd 5 = cd
      {
        checkBox28.Enabled = true;
        comboBox1.Items.Add("S:");
      }
      else checkBox28.Checked = false;
      if (GetDriveType("T:") == 3)  // 3 = hd 5 = cd
      {
        checkBox27.Enabled = true;
        comboBox1.Items.Add("T:");
      }
      else checkBox27.Checked = false;
      if (GetDriveType("U:") == 3)  // 3 = hd 5 = cd
      {
        checkBox38.Enabled = true;
        comboBox1.Items.Add("U:");
      }
      else checkBox38.Checked = false;
      if (GetDriveType("V:") == 3)  // 3 = hd 5 = cd
      {
        checkBox37.Enabled = true;
        comboBox1.Items.Add("V:");
      }
      else checkBox37.Checked = false;
      if (GetDriveType("W:") == 3)  // 3 = hd 5 = cd
      {
        checkBox36.Enabled = true;
        comboBox1.Items.Add("W:");
      }
      else checkBox36.Checked = false;
      if (GetDriveType("X:") == 3)  // 3 = hd 5 = cd
      {
        checkBox35.Enabled = true;
        comboBox1.Items.Add("X:");
      }
      else checkBox35.Checked = false;
      if (GetDriveType("Y:") == 3)  // 3 = hd 5 = cd
      {
        checkBox34.Enabled = true;
        comboBox1.Items.Add("Y:");
      }
      else checkBox34.Checked = false;
      if (GetDriveType("Z:") == 3)  // 3 = hd 5 = cd
      {
        checkBox33.Enabled = true;
        comboBox1.Items.Add("Z:");
      }
      else checkBox33.Checked = false;

      MBMInfo.Refresh();
      button1.Enabled = true;
      if (MBMInfo.Version < 1.0)
      {
        MessageBox.Show("Can not found MBM! You can load MBM from http://mbm.livewiredev.com", "My Status",
          MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
        checkBox6.Enabled = false;
        checkBox6.Checked = false;
        checkBox5.Enabled = false;
        checkBox5.Checked = false;
        checkBox4.Enabled = false;
        checkBox4.Checked = false;
        checkBox13.Enabled = false;
        checkBox13.Checked = false;
        checkBox15.Enabled = false;
        checkBox15.Checked = false;
        checkBox1.Enabled = false;
        checkBox1.Checked = false;
        checkBox2.Enabled = false;
        checkBox2.Checked = false;
        checkBox3.Enabled = false;
        checkBox3.Checked = false;
        checkBox7.Enabled = false;
        checkBox7.Checked = false;
        checkBox8.Enabled = false;
        checkBox8.Checked = false;
        checkBox9.Enabled = false;
        checkBox9.Checked = false;
        checkBox10.Enabled = false;
        checkBox10.Checked = false;
        checkBox11.Enabled = false;
        checkBox11.Checked = false;
        checkBox12.Enabled = false;
        checkBox12.Checked = false;
        isMbm = false;
      }
      else
      {
        isMbm = true;
        label2.Text = MBMInfo.Version.ToString();
        label3.Text = MBMInfo.sdPath.ToString();
        label5.Text = MBMInfo.sdStart.ToString();
        label11.Text = MBMInfo.sdCurrent.ToString();
        label9.Text = MBMInfo.sdInfo().siSmb_Name.ToString();
        for (int i = 1; i <= 100; i++)
        {
          switch (MBMInfo.Sensor(i).ssType)
          {
            case mbmSensorType.stFan:
              if (MBMInfo.Sensor(i).ssCurrent > 260)
              {
                nFan++;
                switch (nFan)
                {
                  case 1:
                    checkBox6.Enabled = true;
                    checkBox6.Text = String.Format("{0}: {1:0.##}upm", MBMInfo.Sensor(i).ssName, MBMInfo.Sensor(i).ssCurrent);
                    aFan[1] = i;
                    comboBox1.Items.Add(MBMInfo.Sensor(i).ssName);
                    break;
                  case 2:
                    checkBox5.Enabled = true;
                    checkBox5.Text = String.Format("{0}: {1:0.##}upm", MBMInfo.Sensor(i).ssName, MBMInfo.Sensor(i).ssCurrent);
                    aFan[2] = i;
                    comboBox1.Items.Add(MBMInfo.Sensor(i).ssName);
                    break;
                  case 3:
                    checkBox4.Enabled = true;
                    checkBox4.Text = String.Format("{0}: {1:0.##}upm", MBMInfo.Sensor(i).ssName, MBMInfo.Sensor(i).ssCurrent);
                    aFan[3] = i;
                    comboBox1.Items.Add(MBMInfo.Sensor(i).ssName);
                    break;
                }
              }
              break;
            case mbmSensorType.stMhz:
              nMhz++;
              if (nMhz == 1)
              {
                checkBox13.Enabled = true;
                checkBox13.Text = String.Format("{0}: {1:0.##}Mhz", MBMInfo.Sensor(i).ssName, MBMInfo.Sensor(i).ssCurrent);
                aMhz[1] = i;
              }
              break;
            case mbmSensorType.stPercentage:
              nPercentage++;
              if (nPercentage == 1)
              {
                checkBox15.Enabled = true;
                checkBox15.Text = String.Format("{0}: {1:0.##}%", MBMInfo.Sensor(i).ssName, MBMInfo.Sensor(i).ssCurrent);
                aPercentage[1] = i;
                comboBox1.Items.Add(MBMInfo.Sensor(i).ssName);
              }
              break;
            case mbmSensorType.stTemperature:
              if (MBMInfo.Sensor(i).ssCurrent < 150)
              {
                nTemperature++;
                switch (nTemperature)
                {
                  case 1:
                    checkBox1.Enabled = true;
                    checkBox1.Text = String.Format("{0}: {1:0.##}C", MBMInfo.Sensor(i).ssName, MBMInfo.Sensor(i).ssCurrent);
                    aTemperature[1] = i;
                    comboBox1.Items.Add(MBMInfo.Sensor(i).ssName);
                    break;
                  case 2:
                    checkBox2.Enabled = true;
                    checkBox2.Text = String.Format("{0}: {1:0.##}C", MBMInfo.Sensor(i).ssName, MBMInfo.Sensor(i).ssCurrent);
                    aTemperature[2] = i;
                    comboBox1.Items.Add(MBMInfo.Sensor(i).ssName);
                    break;
                  case 3:
                    checkBox3.Enabled = true;
                    checkBox3.Text = String.Format("{0}: {1:0.##}C", MBMInfo.Sensor(i).ssName, MBMInfo.Sensor(i).ssCurrent);
                    aTemperature[3] = i;
                    comboBox1.Items.Add(MBMInfo.Sensor(i).ssName);
                    break;
                }
              }
              break;
            case mbmSensorType.stVoltage:
              if (MBMInfo.Sensor(i).ssCurrent < 255)
              {
                nVoltage++;
                switch (nVoltage)
                {
                  case 1:
                    checkBox9.Enabled = true;
                    checkBox9.Text = String.Format("{0}: {1:0.##}V", MBMInfo.Sensor(i).ssName, MBMInfo.Sensor(i).ssCurrent);
                    aVolt[1] = i;
                    comboBox1.Items.Add(MBMInfo.Sensor(i).ssName);
                    break;
                  case 2:
                    checkBox8.Enabled = true;
                    checkBox8.Text = String.Format("{0}: {1:0.##}V", MBMInfo.Sensor(i).ssName, MBMInfo.Sensor(i).ssCurrent);
                    aVolt[2] = i;
                    comboBox1.Items.Add(MBMInfo.Sensor(i).ssName);
                    break;
                  case 3:
                    checkBox7.Enabled = true;
                    checkBox7.Text = String.Format("{0}: {1:0.##}V", MBMInfo.Sensor(i).ssName, MBMInfo.Sensor(i).ssCurrent);
                    aVolt[3] = i;
                    comboBox1.Items.Add(MBMInfo.Sensor(i).ssName);
                    break;
                  case 4:
                    checkBox12.Enabled = true;
                    checkBox12.Text = String.Format("{0}: {1:0.##}V", MBMInfo.Sensor(i).ssName, MBMInfo.Sensor(i).ssCurrent);
                    aVolt[4] = i;
                    comboBox1.Items.Add(MBMInfo.Sensor(i).ssName);
                    break;
                  case 5:
                    checkBox11.Enabled = true;
                    checkBox11.Text = String.Format("{0}: {1:0.##}V", MBMInfo.Sensor(i).ssName, MBMInfo.Sensor(i).ssCurrent);
                    aVolt[5] = i;
                    comboBox1.Items.Add(MBMInfo.Sensor(i).ssName);
                    break;
                  case 6:
                    checkBox10.Enabled = true;
                    checkBox10.Text = String.Format("{0}: {1:0.##}V", MBMInfo.Sensor(i).ssName, MBMInfo.Sensor(i).ssCurrent);
                    aVolt[6] = i;
                    comboBox1.Items.Add(MBMInfo.Sensor(i).ssName);
                    break;
                }
              }
              break;
            case mbmSensorType.stUnknown:
              break;
          }
        }
      }
    }

    private void button3_Click(object sender, System.EventArgs e)
    {
      using (folderBrowserDialog1 = new FolderBrowserDialog())
      {
        folderBrowserDialog1.Description = "Select the folder where alarm sounds will be stored";
        folderBrowserDialog1.ShowNewFolderButton = true;
        folderBrowserDialog1.SelectedPath = textBox1.Text;
        DialogResult dialogResult = folderBrowserDialog1.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          textBox1.Text = folderBrowserDialog1.SelectedPath;
        }
      }
    }

    private void checkBox39_CheckedChanged(object sender, System.EventArgs e)
    {
      comboBox1.Enabled = true;
      checkBox40.Enabled = true;
      checkBox41.Enabled = true;
    }

    private void checkBox40_CheckedChanged(object sender, System.EventArgs e)
    {
      checkBox41.Checked = false;
    }

    private void checkBox41_CheckedChanged(object sender, System.EventArgs e)
    {
      checkBox40.Checked = false;
    }

    private void comboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
    {

    }

  }
}
