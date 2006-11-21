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
using System.Diagnostics;
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
    private XPBurn.XPBurnCD CDBurner;
    private int selIndx = 0;
    private MediaPortal.UserInterface.Controls.MPButton buttonOK;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private TextBox textBox1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBoxBurnerDriver;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxTempPath;
    private MediaPortal.UserInterface.Controls.MPLabel labelSelectTempPath;
    private MediaPortal.UserInterface.Controls.MPButton buttonSelectTempPathLocation;
    private RadioButton radioButtonTvFormatNtsc;
    private Label label15;
    private CheckBox checkBoxLeaveFileForDebug;
    private ComboBox comboBox1;
    private Label label3;
    private RadioButton radioButtonTvFormatPal;
    private TextBox textBoxDVDBurnExePath;
    private MediaPortal.UserInterface.Controls.MPButton buttonSelectDvdBurnPathLocation;
    private Label label4;
    private CheckBox checkBoxDontBurnDVD;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private Label label2;
    private Label label5;
    private TextBox textBox2;
    private Label label6;
    private Label label7;
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

      CDBurner = new XPBurn.XPBurnCD();
      GetRecorder();
      
      comboBox1.Enabled = true;
      textBoxTempPath.Enabled = true;
      buttonSelectTempPathLocation.Enabled = true;
      comboBox1.SelectedIndex = selIndx;
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
      this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.radioButtonTvFormatNtsc = new System.Windows.Forms.RadioButton();
      this.label15 = new System.Windows.Forms.Label();
      this.checkBoxLeaveFileForDebug = new System.Windows.Forms.CheckBox();
      this.comboBox1 = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.radioButtonTvFormatPal = new System.Windows.Forms.RadioButton();
      this.textBoxDVDBurnExePath = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.checkBoxDontBurnDVD = new System.Windows.Forms.CheckBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonSelectDvdBurnPathLocation = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTextBoxBurnerDriver = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxTempPath = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelSelectTempPath = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonSelectTempPathLocation = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.textBox2 = new System.Windows.Forms.TextBox();
      this.label6 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(15, 189);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(595, 20);
      this.textBox1.TabIndex = 40;
      this.textBox1.Text = "http://www.microsoft.com/downloads/details.aspx?FamilyID=9D467A69-57FF-4AE7-96EE-" +
          "B18C4790CFFD&displaylang=en";
      // 
      // radioButtonTvFormatNtsc
      // 
      this.radioButtonTvFormatNtsc.AutoSize = true;
      this.radioButtonTvFormatNtsc.Location = new System.Drawing.Point(297, 332);
      this.radioButtonTvFormatNtsc.Name = "radioButtonTvFormatNtsc";
      this.radioButtonTvFormatNtsc.Size = new System.Drawing.Size(54, 17);
      this.radioButtonTvFormatNtsc.TabIndex = 2;
      this.radioButtonTvFormatNtsc.TabStop = true;
      this.radioButtonTvFormatNtsc.Text = "NTSC";
      this.radioButtonTvFormatNtsc.UseVisualStyleBackColor = true;
      // 
      // label15
      // 
      this.label15.AutoSize = true;
      this.label15.Location = new System.Drawing.Point(12, 380);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(159, 13);
      this.label15.TabIndex = 43;
      this.label15.Text = "Keep Temp Files For Debugging";
      // 
      // checkBoxLeaveFileForDebug
      // 
      this.checkBoxLeaveFileForDebug.AutoSize = true;
      this.checkBoxLeaveFileForDebug.Location = new System.Drawing.Point(220, 380);
      this.checkBoxLeaveFileForDebug.Name = "checkBoxLeaveFileForDebug";
      this.checkBoxLeaveFileForDebug.Size = new System.Drawing.Size(15, 14);
      this.checkBoxLeaveFileForDebug.TabIndex = 42;
      this.checkBoxLeaveFileForDebug.UseVisualStyleBackColor = true;
      // 
      // comboBox1
      // 
      this.comboBox1.FormattingEnabled = true;
      this.comboBox1.Location = new System.Drawing.Point(220, 28);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(270, 21);
      this.comboBox1.TabIndex = 44;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(12, 332);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(65, 13);
      this.label3.TabIndex = 46;
      this.label3.Text = "DVD Format";
      // 
      // radioButtonTvFormatPal
      // 
      this.radioButtonTvFormatPal.AutoSize = true;
      this.radioButtonTvFormatPal.Checked = true;
      this.radioButtonTvFormatPal.Location = new System.Drawing.Point(220, 332);
      this.radioButtonTvFormatPal.Name = "radioButtonTvFormatPal";
      this.radioButtonTvFormatPal.Size = new System.Drawing.Size(45, 17);
      this.radioButtonTvFormatPal.TabIndex = 1;
      this.radioButtonTvFormatPal.TabStop = true;
      this.radioButtonTvFormatPal.Text = "PAL";
      this.radioButtonTvFormatPal.UseVisualStyleBackColor = true;
      // 
      // textBoxDVDBurnExePath
      // 
      this.textBoxDVDBurnExePath.Location = new System.Drawing.Point(220, 214);
      this.textBoxDVDBurnExePath.Name = "textBoxDVDBurnExePath";
      this.textBoxDVDBurnExePath.Size = new System.Drawing.Size(272, 20);
      this.textBoxDVDBurnExePath.TabIndex = 47;
      this.textBoxDVDBurnExePath.Text = "C:\\Program Files\\Windows Resource Kits\\Tools";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(12, 400);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(98, 13);
      this.label4.TabIndex = 49;
      this.label4.Text = "Dont burn the DVD";
      // 
      // checkBoxDontBurnDVD
      // 
      this.checkBoxDontBurnDVD.AutoSize = true;
      this.checkBoxDontBurnDVD.Location = new System.Drawing.Point(220, 400);
      this.checkBoxDontBurnDVD.Name = "checkBoxDontBurnDVD";
      this.checkBoxDontBurnDVD.Size = new System.Drawing.Size(15, 14);
      this.checkBoxDontBurnDVD.TabIndex = 50;
      this.checkBoxDontBurnDVD.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(14, 162);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(549, 13);
      this.label2.TabIndex = 52;
      this.label2.Text = "Please download and install the below file. This is a publicly available download" +
          " from Microsoft that contains an app";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(12, 175);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(244, 13);
      this.label5.TabIndex = 53;
      this.label5.Text = " called dvdburn.exe which is required by MyBurner";
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(12, 217);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(200, 24);
      this.mpLabel2.TabIndex = 51;
      this.mpLabel2.Text = "Select Path to dvdburn.exe ";
      // 
      // buttonSelectDvdBurnPathLocation
      // 
      this.buttonSelectDvdBurnPathLocation.Location = new System.Drawing.Point(508, 210);
      this.buttonSelectDvdBurnPathLocation.Name = "buttonSelectDvdBurnPathLocation";
      this.buttonSelectDvdBurnPathLocation.Size = new System.Drawing.Size(32, 24);
      this.buttonSelectDvdBurnPathLocation.TabIndex = 48;
      this.buttonSelectDvdBurnPathLocation.Text = "...";
      this.buttonSelectDvdBurnPathLocation.UseVisualStyleBackColor = true;
      this.buttonSelectDvdBurnPathLocation.Click += new System.EventHandler(this.buttonSelectDvdBurnPathLocation_Click);
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(318, 65);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(222, 29);
      this.mpLabel1.TabIndex = 38;
      this.mpLabel1.Text = "Enter in your Burner\'s drive letter. E.g. \"D:\" but without the speechmarks";
      // 
      // mpTextBoxBurnerDriver
      // 
      this.mpTextBoxBurnerDriver.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBoxBurnerDriver.Location = new System.Drawing.Point(220, 65);
      this.mpTextBoxBurnerDriver.Name = "mpTextBoxBurnerDriver";
      this.mpTextBoxBurnerDriver.Size = new System.Drawing.Size(92, 20);
      this.mpTextBoxBurnerDriver.TabIndex = 37;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(12, 28);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(80, 24);
      this.label1.TabIndex = 33;
      this.label1.Text = "Select Burner Drive";
      // 
      // textBoxTempPath
      // 
      this.textBoxTempPath.BorderColor = System.Drawing.Color.Empty;
      this.textBoxTempPath.Enabled = false;
      this.textBoxTempPath.Location = new System.Drawing.Point(220, 97);
      this.textBoxTempPath.Name = "textBoxTempPath";
      this.textBoxTempPath.Size = new System.Drawing.Size(272, 20);
      this.textBoxTempPath.TabIndex = 34;
      // 
      // labelSelectTempPath
      // 
      this.labelSelectTempPath.Location = new System.Drawing.Point(12, 97);
      this.labelSelectTempPath.Name = "labelSelectTempPath";
      this.labelSelectTempPath.Size = new System.Drawing.Size(96, 24);
      this.labelSelectTempPath.TabIndex = 35;
      this.labelSelectTempPath.Text = "Select Temp Path";
      // 
      // buttonSelectTempPathLocation
      // 
      this.buttonSelectTempPathLocation.Enabled = false;
      this.buttonSelectTempPathLocation.Location = new System.Drawing.Point(508, 97);
      this.buttonSelectTempPathLocation.Name = "buttonSelectTempPathLocation";
      this.buttonSelectTempPathLocation.Size = new System.Drawing.Size(32, 24);
      this.buttonSelectTempPathLocation.TabIndex = 36;
      this.buttonSelectTempPathLocation.Text = "...";
      this.buttonSelectTempPathLocation.UseVisualStyleBackColor = true;
      this.buttonSelectTempPathLocation.Click += new System.EventHandler(this.buttonSelectTempPathLocation_Click);
      // 
      // buttonOK
      // 
      this.buttonOK.Location = new System.Drawing.Point(522, 380);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(88, 24);
      this.buttonOK.TabIndex = 2;
      this.buttonOK.Text = "OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
      // 
      // textBox2
      // 
      this.textBox2.Location = new System.Drawing.Point(12, 291);
      this.textBox2.Name = "textBox2";
      this.textBox2.Size = new System.Drawing.Size(595, 20);
      this.textBox2.TabIndex = 55;
      this.textBox2.Text = "http://www.team-mediaportal.com/files/Download/SystemUtilities/BurnerSupportFiles" +
          ".rar/";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(12, 121);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(466, 13);
      this.label6.TabIndex = 56;
      this.label6.Text = "This is where all the MyBurner temp files will be stored. This may be upto 5 gb f" +
          "or a complete DVD";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(14, 275);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(381, 13);
      this.label7.TabIndex = 57;
      this.label7.Text = "Please download and install the below file. These are files required by MyBurner";
      // 
      // SetupForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(640, 427);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.textBox2);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.mpLabel2);
      this.Controls.Add(this.checkBoxDontBurnDVD);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.buttonSelectDvdBurnPathLocation);
      this.Controls.Add(this.textBoxDVDBurnExePath);
      this.Controls.Add(this.radioButtonTvFormatNtsc);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.radioButtonTvFormatPal);
      this.Controls.Add(this.comboBox1);
      this.Controls.Add(this.label15);
      this.Controls.Add(this.checkBoxLeaveFileForDebug);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.mpTextBoxBurnerDriver);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.textBoxTempPath);
      this.Controls.Add(this.labelSelectTempPath);
      this.Controls.Add(this.buttonSelectTempPathLocation);
      this.Controls.Add(this.buttonOK);
      this.Name = "SetupForm";
      this.Text = "My Burner Setup";
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    #region plugin vars

    public string PluginName()
    {
      return "My Burner";
    }

    public string Description()
    {
      return "Burn CD's and DVD's in MediaPortal";
    }

    public string Author()
    {
      return "EgonSpenglerUk improving Gucky62s work";
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

      for (int i = 0; i < CDBurner.NumberOfDrives; i++)
      {
        CDBurner.BurnerDrive = CDBurner.RecorderDrives[i].ToString();
        name = CDBurner.Vendor + " " + CDBurner.ProductID + " " + CDBurner.Revision;
        comboBox1.Items.Add(name);
        comboBox1.SelectedIndex = 0;
      }
    }

    private void buttonOK_Click(object sender, System.EventArgs e)
    {
      if (textBoxTempPath.Text == "")
      {
        MessageBox.Show("Please select a Temp folder");
      }
      else
      {
        SaveSettings();
        this.Close();
      }
    }

    private void buttonSelectTempPathLocation_Click(object sender, System.EventArgs e)
    {
      using (folderBrowserDialog1 = new FolderBrowserDialog())
      {
        folderBrowserDialog1.Description = "Select a temporary file folder";
        folderBrowserDialog1.ShowNewFolderButton = true;
        folderBrowserDialog1.SelectedPath = textBoxTempPath.Text;
        DialogResult dialogResult = folderBrowserDialog1.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          textBoxTempPath.Text = folderBrowserDialog1.SelectedPath;
        }
      }
    }

    private void buttonSelectDvdBurnPathLocation_Click(object sender, EventArgs e)
    {
      using (folderBrowserDialog1 = new FolderBrowserDialog())
      {
        folderBrowserDialog1.Description = "Select where DVDBurn.exe is installed";
        folderBrowserDialog1.ShowNewFolderButton = true;
        folderBrowserDialog1.SelectedPath = textBoxDVDBurnExePath.Text;

        DialogResult dialogResult = folderBrowserDialog1.ShowDialog(this);
        if (dialogResult == DialogResult.OK)
        {
          textBoxDVDBurnExePath.Text = folderBrowserDialog1.SelectedPath;
        }
      }
    }

    private void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        textBoxTempPath.Text = xmlreader.GetValueAsString("burner", "temp_folder", Path.GetDirectoryName(Path.GetTempPath()));
        textBoxDVDBurnExePath.Text = xmlreader.GetValueAsString("burner", "dvdburnexe_folder", "C:\\Program Files\\Windows Resource Kits\\Tools");

        selIndx = xmlreader.GetValueAsInt("burner", "recorder", 0);
        mpTextBoxBurnerDriver.Text = xmlreader.GetValueAsString("burner", "recorderdrive", "D:");
        
        radioButtonTvFormatPal.Checked = xmlreader.GetValueAsBool("burner", "PalTvFormat", true);
        radioButtonTvFormatNtsc.Checked = !radioButtonTvFormatPal.Checked;

        checkBoxLeaveFileForDebug.Checked = xmlreader.GetValueAsBool("burner", "leavedebugfiles", true);
        checkBoxDontBurnDVD.Checked = xmlreader.GetValueAsBool("burner", "dummyburn", false);
      }
    }

    private void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValue("burner", "temp_folder", textBoxTempPath.Text);
        xmlwriter.SetValue("burner", "dvdburnexe_folder", textBoxDVDBurnExePath.Text);

        xmlwriter.SetValue("burner", "recorder", comboBox1.SelectedIndex);
        xmlwriter.SetValue("burner", "recorderdrive", mpTextBoxBurnerDriver.Text);

        xmlwriter.SetValueAsBool("burner", "PalTvFormat", radioButtonTvFormatPal.Checked);
        
        xmlwriter.SetValueAsBool("burner", "leavedebugfiles", checkBoxLeaveFileForDebug.Checked);
        xmlwriter.SetValueAsBool("burner", "dummyburn", checkBoxDontBurnDVD.Checked);
      }
    }
    #endregion

  }
}
