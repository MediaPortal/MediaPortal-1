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

#region Usings
using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using XPBurn;
#endregion

namespace GUIBurner
{
  /// <summary>
  /// Summary description for SetupForm.
  /// </summary>
  public class SetupForm : System.Windows.Forms.Form, ISetupForm, IShowPlugin
  {
    #region Private Variables
    private XPBurn.XPBurnCD burnClass;
    private int selIndx = 0;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox1;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPButton button1;
    private MediaPortal.UserInterface.Controls.MPTextBox textBox1;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPButton button2;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox1;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox2;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox3;
    private MediaPortal.UserInterface.Controls.MPLabel label7;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox4;
    private MediaPortal.UserInterface.Controls.MPLabel label8;
    private MediaPortal.UserInterface.Controls.MPLabel label9;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox5;
    private MediaPortal.UserInterface.Controls.MPLabel label10;
    private MediaPortal.UserInterface.Controls.MPLabel label11;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox6;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControl1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage2;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage3;
    private MediaPortal.UserInterface.Controls.MPButton button3;
    private MediaPortal.UserInterface.Controls.MPButton button5;
    private System.Windows.Forms.ListBox listBox1;
    private MediaPortal.UserInterface.Controls.MPLabel label12;
    private MediaPortal.UserInterface.Controls.MPButton button6;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage4;
    private MediaPortal.UserInterface.Controls.MPTextBox textBox2;
    private MediaPortal.UserInterface.Controls.MPButton button7;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private MediaPortal.UserInterface.Controls.MPCheckBox DateTimeFolder;
    private MediaPortal.UserInterface.Controls.MPButton button8;
    private MediaPortal.UserInterface.Controls.MPTextBox textBox3;
    private MediaPortal.UserInterface.Controls.MPButton button9;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox7;
    private MediaPortal.UserInterface.Controls.MPLabel label13;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox8;
    private MediaPortal.UserInterface.Controls.MPLabel label14;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;
    #endregion

    #region SetupForm
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
    #endregion

    #region Overtides
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
    #endregion

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.comboBox1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.button1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.textBox1 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.button2 = new MediaPortal.UserInterface.Controls.MPButton();
      this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
      this.checkBox1 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox2 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox4 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox5 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label10 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label11 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox6 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.button3 = new MediaPortal.UserInterface.Controls.MPButton();
      this.tabPage2 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.checkBox8 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label14 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox7 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label13 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.button9 = new MediaPortal.UserInterface.Controls.MPButton();
      this.textBox3 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tabPage4 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.tabPage3 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.button8 = new MediaPortal.UserInterface.Controls.MPButton();
      this.DateTimeFolder = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.button7 = new MediaPortal.UserInterface.Controls.MPButton();
      this.textBox2 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.button6 = new MediaPortal.UserInterface.Controls.MPButton();
      this.label12 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.listBox1 = new System.Windows.Forms.ListBox();
      this.button5 = new MediaPortal.UserInterface.Controls.MPButton();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.SuspendLayout();
      // 
      // comboBox1
      // 
      this.comboBox1.Enabled = false;
      this.comboBox1.Location = new System.Drawing.Point(224, 64);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(272, 21);
      this.comboBox1.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 64);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(80, 24);
      this.label1.TabIndex = 1;
      this.label1.Text = "Select Drive";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(520, 352);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(88, 24);
      this.button1.TabIndex = 2;
      this.button1.Text = "OK";
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // textBox1
      // 
      this.textBox1.Enabled = false;
      this.textBox1.Location = new System.Drawing.Point(224, 96);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(272, 20);
      this.textBox1.TabIndex = 3;
      this.textBox1.Text = "";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 96);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(96, 24);
      this.label2.TabIndex = 4;
      this.label2.Text = "Select Temp Path";
      // 
      // button2
      // 
      this.button2.Enabled = false;
      this.button2.Location = new System.Drawing.Point(512, 96);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(32, 24);
      this.button2.TabIndex = 5;
      this.button2.Text = "...";
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // checkBox1
      // 
      this.checkBox1.Checked = true;
      this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBox1.Enabled = false;
      this.checkBox1.Location = new System.Drawing.Point(224, 136);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(16, 16);
      this.checkBox1.TabIndex = 6;
      this.checkBox1.Text = "checkBox1";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 136);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(112, 16);
      this.label3.TabIndex = 7;
      this.label3.Text = "CD/RW Fast Format";
      // 
      // checkBox2
      // 
      this.checkBox2.Location = new System.Drawing.Point(224, 72);
      this.checkBox2.Name = "checkBox2";
      this.checkBox2.Size = new System.Drawing.Size(16, 16);
      this.checkBox2.TabIndex = 9;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 72);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(184, 24);
      this.label4.TabIndex = 10;
      this.label4.Text = "Delete DVR-MS File after Convert";
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 184);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(184, 24);
      this.label5.TabIndex = 12;
      this.label5.Text = "Automatic convert DVR-MS Files";
      // 
      // checkBox3
      // 
      this.checkBox3.Location = new System.Drawing.Point(224, 184);
      this.checkBox3.Name = "checkBox3";
      this.checkBox3.Size = new System.Drawing.Size(16, 16);
      this.checkBox3.TabIndex = 11;
      this.checkBox3.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(16, 48);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(184, 24);
      this.label7.TabIndex = 17;
      this.label7.Text = "Convert  DVR-MS ";
      // 
      // checkBox4
      // 
      this.checkBox4.Location = new System.Drawing.Point(224, 40);
      this.checkBox4.Name = "checkBox4";
      this.checkBox4.Size = new System.Drawing.Size(24, 24);
      this.checkBox4.TabIndex = 16;
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(8, 320);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(424, 24);
      this.label8.TabIndex = 18;
      this.label8.Text = "If you want to convert DVR-MS in MPEG Files you must instal the Cyberlink Filters" +
        ". ";
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(16, 32);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(184, 24);
      this.label9.TabIndex = 20;
      this.label9.Text = "Burn CD/DVD";
      // 
      // checkBox5
      // 
      this.checkBox5.Location = new System.Drawing.Point(224, 24);
      this.checkBox5.Name = "checkBox5";
      this.checkBox5.Size = new System.Drawing.Size(24, 24);
      this.checkBox5.TabIndex = 19;
      this.checkBox5.CheckedChanged += new System.EventHandler(this.checkBox5_CheckedChanged);
      // 
      // label10
      // 
      this.label10.Location = new System.Drawing.Point(248, 32);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(328, 16);
      this.label10.TabIndex = 22;
      this.label10.Text = "If you want to Burn you must  select the Burn CD/DVD Button";
      // 
      // label11
      // 
      this.label11.Location = new System.Drawing.Point(16, 96);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(208, 24);
      this.label11.TabIndex = 24;
      this.label11.Text = "Change TVDatabase entry after Convert";
      // 
      // checkBox6
      // 
      this.checkBox6.Location = new System.Drawing.Point(224, 96);
      this.checkBox6.Name = "checkBox6";
      this.checkBox6.Size = new System.Drawing.Size(16, 16);
      this.checkBox6.TabIndex = 23;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(248, 184);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(368, 32);
      this.label6.TabIndex = 25;
      this.label6.Text = "This Option converts automatic all TV-Record Files! TV-Database entry will be cha" +
        "nged. Attention, after convert DVR-MS Files will be deleted!!!";
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Controls.Add(this.tabPage4);
      this.tabControl1.Controls.Add(this.tabPage3);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(640, 416);
      this.tabControl1.TabIndex = 26;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.button3);
      this.tabPage1.Controls.Add(this.checkBox1);
      this.tabPage1.Controls.Add(this.label3);
      this.tabPage1.Controls.Add(this.label9);
      this.tabPage1.Controls.Add(this.checkBox5);
      this.tabPage1.Controls.Add(this.label10);
      this.tabPage1.Controls.Add(this.comboBox1);
      this.tabPage1.Controls.Add(this.label1);
      this.tabPage1.Controls.Add(this.textBox1);
      this.tabPage1.Controls.Add(this.label2);
      this.tabPage1.Controls.Add(this.button2);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(632, 390);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Burner settings";
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(520, 352);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(88, 24);
      this.button3.TabIndex = 23;
      this.button3.Text = "OK";
      this.button3.Click += new System.EventHandler(this.button1_Click);
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.checkBox8);
      this.tabPage2.Controls.Add(this.label14);
      this.tabPage2.Controls.Add(this.checkBox7);
      this.tabPage2.Controls.Add(this.label13);
      this.tabPage2.Controls.Add(this.button9);
      this.tabPage2.Controls.Add(this.textBox3);
      this.tabPage2.Controls.Add(this.checkBox2);
      this.tabPage2.Controls.Add(this.label4);
      this.tabPage2.Controls.Add(this.label6);
      this.tabPage2.Controls.Add(this.label7);
      this.tabPage2.Controls.Add(this.checkBox4);
      this.tabPage2.Controls.Add(this.checkBox6);
      this.tabPage2.Controls.Add(this.label11);
      this.tabPage2.Controls.Add(this.label5);
      this.tabPage2.Controls.Add(this.checkBox3);
      this.tabPage2.Controls.Add(this.label8);
      this.tabPage2.Controls.Add(this.button1);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(632, 390);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "DVR-MS Convert";
      this.tabPage2.Click += new System.EventHandler(this.tabPage2_Click);
      // 
      // checkBox8
      // 
      this.checkBox8.Location = new System.Drawing.Point(224, 152);
      this.checkBox8.Name = "checkBox8";
      this.checkBox8.Size = new System.Drawing.Size(16, 16);
      this.checkBox8.TabIndex = 30;
      this.checkBox8.CheckedChanged += new System.EventHandler(this.checkBox8_CheckedChanged);
      // 
      // label14
      // 
      this.label14.Location = new System.Drawing.Point(16, 152);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(184, 24);
      this.label14.TabIndex = 31;
      this.label14.Text = "Delete File from Database";
      // 
      // checkBox7
      // 
      this.checkBox7.Location = new System.Drawing.Point(224, 128);
      this.checkBox7.Name = "checkBox7";
      this.checkBox7.Size = new System.Drawing.Size(16, 16);
      this.checkBox7.TabIndex = 28;
      this.checkBox7.CheckedChanged += new System.EventHandler(this.checkBox7_CheckedChanged);
      // 
      // label13
      // 
      this.label13.Location = new System.Drawing.Point(16, 128);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(208, 24);
      this.label13.TabIndex = 29;
      this.label13.Text = "Copy MPEG after convert";
      // 
      // button9
      // 
      this.button9.Enabled = false;
      this.button9.Location = new System.Drawing.Point(528, 128);
      this.button9.Name = "button9";
      this.button9.Size = new System.Drawing.Size(32, 24);
      this.button9.TabIndex = 27;
      this.button9.Text = "...";
      this.button9.Click += new System.EventHandler(this.button9_Click);
      // 
      // textBox3
      // 
      this.textBox3.Enabled = false;
      this.textBox3.Location = new System.Drawing.Point(248, 128);
      this.textBox3.Name = "textBox3";
      this.textBox3.Size = new System.Drawing.Size(264, 20);
      this.textBox3.TabIndex = 26;
      this.textBox3.Text = "";
      this.textBox3.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
      // 
      // tabPage4
      // 
      this.tabPage4.Location = new System.Drawing.Point(4, 22);
      this.tabPage4.Name = "tabPage4";
      this.tabPage4.Size = new System.Drawing.Size(632, 390);
      this.tabPage4.TabIndex = 3;
      this.tabPage4.Text = "Video settings";
      // 
      // tabPage3
      // 
      this.tabPage3.Controls.Add(this.button8);
      this.tabPage3.Controls.Add(this.DateTimeFolder);
      this.tabPage3.Controls.Add(this.button7);
      this.tabPage3.Controls.Add(this.textBox2);
      this.tabPage3.Controls.Add(this.button6);
      this.tabPage3.Controls.Add(this.label12);
      this.tabPage3.Controls.Add(this.listBox1);
      this.tabPage3.Controls.Add(this.button5);
      this.tabPage3.Location = new System.Drawing.Point(4, 22);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Size = new System.Drawing.Size(632, 390);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Backup settings";
      // 
      // button8
      // 
      this.button8.Location = new System.Drawing.Point(40, 296);
      this.button8.Name = "button8";
      this.button8.Size = new System.Drawing.Size(104, 24);
      this.button8.TabIndex = 10;
      this.button8.Text = "Delete File";
      this.button8.Click += new System.EventHandler(this.button8_Click);
      // 
      // DateTimeFolder
      // 
      this.DateTimeFolder.Location = new System.Drawing.Point(40, 344);
      this.DateTimeFolder.Name = "DateTimeFolder";
      this.DateTimeFolder.Size = new System.Drawing.Size(408, 32);
      this.DateTimeFolder.TabIndex = 9;
      this.DateTimeFolder.Text = "Create new Folder with Date/Time   e.G.     backup 12-03-05 1250";
      // 
      // button7
      // 
      this.button7.Location = new System.Drawing.Point(416, 264);
      this.button7.Name = "button7";
      this.button7.Size = new System.Drawing.Size(32, 24);
      this.button7.TabIndex = 8;
      this.button7.Text = "...";
      this.button7.Click += new System.EventHandler(this.button7_Click);
      // 
      // textBox2
      // 
      this.textBox2.Location = new System.Drawing.Point(168, 264);
      this.textBox2.Name = "textBox2";
      this.textBox2.Size = new System.Drawing.Size(232, 20);
      this.textBox2.TabIndex = 7;
      this.textBox2.Text = "";
      // 
      // button6
      // 
      this.button6.Location = new System.Drawing.Point(40, 264);
      this.button6.Name = "button6";
      this.button6.Size = new System.Drawing.Size(104, 24);
      this.button6.TabIndex = 6;
      this.button6.Text = "Add File";
      this.button6.Click += new System.EventHandler(this.button6_Click);
      // 
      // label12
      // 
      this.label12.Location = new System.Drawing.Point(40, 24);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(320, 16);
      this.label12.TabIndex = 5;
      this.label12.Text = "Files to Backup";
      // 
      // listBox1
      // 
      this.listBox1.Items.AddRange(new object[] {
																								"database\\*.*",
																								"thumbs\\*.*",
																								"xmltv\\*.*",
																								"weather\\*.*",
																								"*.xml",
																								"menu.bin"});
      this.listBox1.Location = new System.Drawing.Point(40, 40);
      this.listBox1.Name = "listBox1";
      this.listBox1.Size = new System.Drawing.Size(512, 212);
      this.listBox1.TabIndex = 4;
      // 
      // button5
      // 
      this.button5.Location = new System.Drawing.Point(520, 352);
      this.button5.Name = "button5";
      this.button5.Size = new System.Drawing.Size(88, 24);
      this.button5.TabIndex = 3;
      this.button5.Text = "OK";
      this.button5.Click += new System.EventHandler(this.button1_Click);
      // 
      // SetupForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(640, 414);
      this.Controls.Add(this.tabControl1);
      this.Name = "SetupForm";
      this.Text = "SetupForm";
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tabPage3.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    #region plugin vars

    public string PluginName()
    {
      return "My Burner";
    }

    public string Description()
    {
      return "Burn CDs in MediaPortal";
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
      return 760;
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
      strButtonText = GUILocalizeStrings.Get(2100);
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

    #region Private Methods
    private void GetRecorder()
    {
      //Fill The Combobox with available drives
      string name;

      for (int i = 0; i < burnClass.NumberOfDrives; i++)
      {
        burnClass.BurnerDrive = burnClass.RecorderDrives[i].ToString();
        name = burnClass.Vendor + " " + burnClass.ProductID + " " + burnClass.Revision;
        comboBox1.Items.Add(name);
        comboBox1.SelectedIndex = 0;
      }
    }

    private void button1_Click(object sender, System.EventArgs e)
    {
      SaveSettings();
      this.Visible = false;
    }

    private void button2_Click(object sender, System.EventArgs e)
    {
      using (folderBrowserDialog1 = new FolderBrowserDialog())
      {
        folderBrowserDialog1.Description = "Select the folder where recorder temp file will be stored";
        folderBrowserDialog1.ShowNewFolderButton = true;
        folderBrowserDialog1.SelectedPath = textBox1.Text;
        DialogResult dialogResult = folderBrowserDialog1.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          textBox1.Text = folderBrowserDialog1.SelectedPath;
        }
      }
    }

    private void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        string tempPath = Path.GetTempPath();
        textBox1.Text = xmlreader.GetValueAsString("burner", "temp_folder", tempPath);
        if (textBox1.Text == "") textBox1.Text = tempPath;

        selIndx = xmlreader.GetValueAsInt("burner", "recorder", 0);
        checkBox5.Checked = xmlreader.GetValueAsBool("burner", "burn", false);
        checkBox1.Checked = xmlreader.GetValueAsBool("burner", "fastformat", true);
        checkBox4.Checked = xmlreader.GetValueAsBool("burner", "convertdvr", true);
        checkBox2.Checked = xmlreader.GetValueAsBool("burner", "deletedvrsource", false);
        checkBox3.Checked = xmlreader.GetValueAsBool("burner", "convertautomatic", false);
        checkBox6.Checked = xmlreader.GetValueAsBool("burner", "changetvdatabase", false);
        checkBox7.Checked = xmlreader.GetValueAsBool("burner", "mpegpath", false);
        checkBox8.Checked = xmlreader.GetValueAsBool("burner", "deletetvdatabase", false);
        textBox3.Text = xmlreader.GetValueAsString("burner", "mpeg_folder", "");
        DateTimeFolder.Checked = xmlreader.GetValueAsBool("burner", "dateTimeFolder", false);
        int count = xmlreader.GetValueAsInt("burner", "backuplines", 0);
        if (count != 0)
        {
          listBox1.Items.Clear();
          for (int i = 0; i <= count; i++)
          {
            listBox1.Items.Add(xmlreader.GetValueAsString("burner", "backupline#" + i.ToString(), ""));
          }
        }
      }
    }

    private void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValue("burner", "temp_folder", textBox1.Text);
        if (checkBox5.Checked == true)
        {
          xmlwriter.SetValue("burner", "recorder", comboBox1.SelectedIndex);
        }
        xmlwriter.SetValueAsBool("burner", "burn", checkBox5.Checked);
        xmlwriter.SetValueAsBool("burner", "fastformat", checkBox1.Checked);
        xmlwriter.SetValueAsBool("burner", "convertdvr", checkBox4.Checked);
        xmlwriter.SetValueAsBool("burner", "deletedvrsource", checkBox2.Checked);
        xmlwriter.SetValueAsBool("burner", "convertautomatic", checkBox3.Checked);
        xmlwriter.SetValueAsBool("burner", "changetvdatabase", checkBox6.Checked);
        xmlwriter.SetValueAsBool("burner", "mpegpath", checkBox7.Checked);
        xmlwriter.SetValueAsBool("burner", "changetvdatabase", checkBox8.Checked);
        xmlwriter.SetValueAsBool("burner", "deletetvdatabase", DateTimeFolder.Checked);
        xmlwriter.SetValue("burner", "mpeg_folder", textBox3.Text);
        int count = 0;
        foreach (string text in listBox1.Items)
        {
          if (text != "")
          {
            xmlwriter.SetValue("burner", "backupline#" + count.ToString(), text);
            count++;
          }
        }
        xmlwriter.SetValue("burner", "backuplines", count);
      }
    }


    private void checkBox5_CheckedChanged(object sender, System.EventArgs e)
    {
      try
      {
        burnClass = new XPBurn.XPBurnCD();
        GetRecorder();
        checkBox5.Checked = true;
        comboBox1.Enabled = true;
        textBox1.Enabled = true;
        button2.Enabled = true;
        checkBox1.Enabled = true;
        comboBox1.SelectedIndex = selIndx;
      }
      catch (Exception)
      {
        checkBox5.Checked = false;
      }
    }

    private void button7_Click(object sender, System.EventArgs e)
    {
      openFileDialog1.RestoreDirectory = true;
      openFileDialog1.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
      if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        string file = openFileDialog1.FileName;
        textBox2.Text = file.Substring(System.IO.Directory.GetCurrentDirectory().Length + 1);
      }
    }

    private void button6_Click(object sender, System.EventArgs e)
    {
      if (textBox2.Text == "")
      {
        MessageBox.Show("You must enter a valid file name");
      }
      else
      {
        listBox1.Items.Add(textBox2.Text);
        textBox2.Text = "";
      }
    }

    private void button8_Click(object sender, System.EventArgs e)
    {
      listBox1.Items.RemoveAt(listBox1.SelectedIndex);
    }

    private void tabPage2_Click(object sender, System.EventArgs e)
    {

    }

    private void textBox3_TextChanged(object sender, System.EventArgs e)
    {

    }

    private void button9_Click(object sender, System.EventArgs e)
    {
      using (folderBrowserDialog1 = new FolderBrowserDialog())
      {
        folderBrowserDialog1.Description = "Select the folder where MPEG temp file will be stored";
        folderBrowserDialog1.ShowNewFolderButton = true;
        folderBrowserDialog1.SelectedPath = textBox3.Text;
        DialogResult dialogResult = folderBrowserDialog1.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          textBox3.Text = folderBrowserDialog1.SelectedPath;
        }
      }
    }

    private void checkBox7_CheckedChanged(object sender, System.EventArgs e)
    {
      if (checkBox7.Checked == true)
      {
        textBox3.Enabled = true;
        button9.Enabled = true;
      }
      else
      {
        button9.Enabled = false;
        textBox3.Enabled = false;
      }
    }

    private void checkBox8_CheckedChanged(object sender, System.EventArgs e)
    {
      if (checkBox8.Checked == true)
      {
        checkBox6.Checked = false;
      }
    }

    private void checkBox3_CheckedChanged(object sender, System.EventArgs e)
    {
      if (checkBox8.Checked == true)
      {
        checkBox2.Checked = true;
        checkBox6.Checked = true;
        checkBox7.Checked = false;
        checkBox8.Checked = false;
      }
    }
    #endregion

  }
}
