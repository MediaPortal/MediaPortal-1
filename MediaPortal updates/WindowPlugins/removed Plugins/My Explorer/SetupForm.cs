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
using MediaPortal.GUI.Library;

namespace GUIExplorer
{
  /// <summary>
  /// Summary description for SetupForm.
  /// </summary>
  public class SetupForm : System.Windows.Forms.Form, ISetupForm, IShowPlugin
  {
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox2;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox3;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox4;
    private MediaPortal.UserInterface.Controls.MPTextBox textBox1;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPButton button1;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    private MediaPortal.UserInterface.Controls.MPButton button2;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox1;

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
      this.checkBox1 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox2 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBox3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox4 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.textBox1 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.button1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
      this.button2 = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(168, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Show only Shares (Destination)";
      // 
      // checkBox1
      // 
      this.checkBox1.Location = new System.Drawing.Point(264, 16);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(24, 24);
      this.checkBox1.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 48);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(152, 23);
      this.label2.TabIndex = 2;
      this.label2.Text = "Enable Delete Funktion";
      // 
      // checkBox2
      // 
      this.checkBox2.Location = new System.Drawing.Point(264, 40);
      this.checkBox2.Name = "checkBox2";
      this.checkBox2.Size = new System.Drawing.Size(24, 24);
      this.checkBox2.TabIndex = 3;
      // 
      // checkBox3
      // 
      this.checkBox3.Location = new System.Drawing.Point(264, 64);
      this.checkBox3.Name = "checkBox3";
      this.checkBox3.Size = new System.Drawing.Size(24, 24);
      this.checkBox3.TabIndex = 4;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 72);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(160, 24);
      this.label3.TabIndex = 5;
      this.label3.Text = "Delete Files immediately ";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 96);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(256, 23);
      this.label4.TabIndex = 6;
      this.label4.Text = "Delete moves files to Temp Folder (like Trashcan)";
      // 
      // checkBox4
      // 
      this.checkBox4.Location = new System.Drawing.Point(264, 88);
      this.checkBox4.Name = "checkBox4";
      this.checkBox4.Size = new System.Drawing.Size(24, 24);
      this.checkBox4.TabIndex = 7;
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(264, 120);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(216, 20);
      this.textBox1.TabIndex = 8;
      this.textBox1.Text = "";
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 120);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(176, 23);
      this.label5.TabIndex = 9;
      this.label5.Text = "Temp Folder";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(496, 120);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(32, 24);
      this.button1.TabIndex = 10;
      this.button1.Text = "...";
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(472, 176);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(56, 24);
      this.button2.TabIndex = 11;
      this.button2.Text = "OK";
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // SetupForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(552, 214);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.checkBox4);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.checkBox3);
      this.Controls.Add(this.checkBox2);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.checkBox1);
      this.Controls.Add(this.label1);
      this.Name = "SetupForm";
      this.Text = "SetupForm";
      this.ResumeLayout(false);

    }
    #endregion

    #region plugin vars

    public string PluginName()
    {
      return "My Explorer";
    }

    public string Description()
    {
      return "Browse your files with MediaPortal";
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
      return 770;
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
      strButtonText = GUILocalizeStrings.Get(2200);
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
      using (folderBrowserDialog1 = new FolderBrowserDialog())
      {
        folderBrowserDialog1.Description = "Select the folder where delete file will be stored";
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
        textBox1.Text = xmlreader.GetValueAsString("myexplorer", "temp_folder", "");
        checkBox1.Checked = xmlreader.GetValueAsBool("myexplorer", "show_only_shares", false);
        checkBox2.Checked = xmlreader.GetValueAsBool("myexplorer", "enable_delete", false);
        checkBox3.Checked = xmlreader.GetValueAsBool("myexplorer", "delete_immediately", false);
        checkBox4.Checked = xmlreader.GetValueAsBool("myexplorer", "delete_temp", false);
      }
    }

    private void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValue("myexplorer", "temp_folder", textBox1.Text);
        xmlwriter.SetValueAsBool("myexplorer", "show_only_shares", checkBox1.Checked);
        xmlwriter.SetValueAsBool("myexplorer", "enable_delete", checkBox2.Checked);
        if (checkBox2.Checked == true)
        {
          xmlwriter.SetValueAsBool("myexplorer", "delete_immediately", checkBox3.Checked);
          if (checkBox3.Checked == true)
          {
            xmlwriter.SetValueAsBool("myexplorer", "delete_temp", false);
          }
          else
          {
            xmlwriter.SetValueAsBool("myexplorer", "delete_temp", checkBox4.Checked);
          }
        }
        else
        {
          xmlwriter.SetValueAsBool("myexplorer", "delete_immediately", false);
          xmlwriter.SetValueAsBool("myexplorer", "delete_temp", false);
        }
      }
    }

    private void button2_Click(object sender, System.EventArgs e)
    {
      if (checkBox4.Checked == true)
      {
        if (textBox1.Text == "")
        {
          MessageBox.Show("Please select a temp path!");
        }
        else
        {
          SaveSettings();
          this.Visible = false;
        }
      }
      else
      {
        if (checkBox2.Checked == true && checkBox3.Checked == false && checkBox4.Checked == false)
        {
          MessageBox.Show("Please a Option: \n(Delete Files immediately)\n or \n(Delete moves files to Temp Folder)");
        }
        else
        {
          SaveSettings();
          this.Visible = false;
        }
      }
    }

  }
}
