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
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using MediaPortal.GUI.Library;

namespace home
{
  /// <summary>
  /// Summary description for SetupForm.
  /// </summary>
  public class SetupForm : System.Windows.Forms.Form, ISetupForm
  {
    private int NodeCount, FolderCount;
    private string NodeMap;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox3;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPLabel label7;
    private MediaPortal.UserInterface.Controls.MPLabel label8;
    private MediaPortal.UserInterface.Controls.MPTextBox textBox5;
    private MediaPortal.UserInterface.Controls.MPTextBox textBox6;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPTextBox textBox3;
    private MediaPortal.UserInterface.Controls.MPTextBox textBox4;
    private MediaPortal.UserInterface.Controls.MPTextBox textBox2;
    private MediaPortal.UserInterface.Controls.MPTextBox textBox1;
    private System.Windows.Forms.ListBox listBox;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkBoxScrolling;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButton2;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButton1;
    private System.Windows.Forms.TreeView treeView;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    enum TagTypes
    {
      PLUGIN = 0,
      MENU_TAG = 1,
      EMPTY = 3
    };

    class ItemTag
    {
      public string DLLName;
      public string FullPath;
      public string author;
      public string buttonText;
      public string pluginName;
      public string description;
      public string picture;
      public int windowId = -1;
      public TagTypes tagType;
    };

    private ArrayList availablePlugins = new ArrayList();
    private MediaPortal.UserInterface.Controls.MPCheckBox chkBoxFixed;
    private ArrayList loadedPlugins = new ArrayList();
    bool addPicture = false;
    bool addScript = false;
    string skinName;

    private MediaPortal.UserInterface.Controls.MPButton MakeMenu;
    private MediaPortal.UserInterface.Controls.MPButton SaveAll;
    private MediaPortal.UserInterface.Controls.MPButton CopyItem;
    private MediaPortal.UserInterface.Controls.MPButton DeleteItem;
    private MediaPortal.UserInterface.Controls.MPButton AddMenu;
    private MediaPortal.UserInterface.Controls.MPCheckBox NoScrollSubs;
    private MediaPortal.UserInterface.Controls.MPButton AddPicture;
    private MediaPortal.UserInterface.Controls.MPButton SearchPicture;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private MediaPortal.UserInterface.Controls.MPButton deletePicture;
    private System.Windows.Forms.PictureBox pictureBox1;
    private MediaPortal.UserInterface.Controls.MPLabel label9;
    private System.Windows.Forms.PictureBox pictureBox2;
    private MediaPortal.UserInterface.Controls.MPLabel label10;
    private MediaPortal.UserInterface.Controls.MPLabel label11;
    private System.Windows.Forms.PictureBox pictureBox3;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControl1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage2;
    private MediaPortal.UserInterface.Controls.MPRadioButton useMyPlugins;
    private MediaPortal.UserInterface.Controls.MPRadioButton useMenus;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPButton button1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox4;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButton3;
    private MediaPortal.UserInterface.Controls.MPLabel label12;
    private MediaPortal.UserInterface.Controls.MPLabel label13;
    private MediaPortal.UserInterface.Controls.MPLabel label14;
    private MediaPortal.UserInterface.Controls.MPLabel label15;
    private MediaPortal.UserInterface.Controls.MPLabel label16;
    private MediaPortal.UserInterface.Controls.MPLabel label18;
    private MediaPortal.UserInterface.Controls.MPLabel label19;
    private MediaPortal.UserInterface.Controls.MPLabel label17;
    private MediaPortal.UserInterface.Controls.MPLabel label20;
    private MediaPortal.UserInterface.Controls.MPTextBox OwnDate;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox5;
    private MediaPortal.UserInterface.Controls.MPTextBox DateTest;
    private MediaPortal.UserInterface.Controls.MPButton TestDate;
    private MediaPortal.UserInterface.Controls.MPLabel label21;
    private MediaPortal.UserInterface.Controls.MPButton AddSpecial;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage3;
    private System.Windows.Forms.ListBox SpecialFunctions;
    private MediaPortal.UserInterface.Controls.MPCheckBox BackButtons;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage4;
    private System.Windows.Forms.ListView listView;
    private System.Windows.Forms.ColumnHeader Plugin;
    private System.Windows.Forms.ColumnHeader Key;
    private System.Windows.Forms.ColumnHeader Fullscreen;
    private System.Windows.Forms.ColumnHeader Type;
    private MediaPortal.UserInterface.Controls.MPLabel label22;
    private MediaPortal.UserInterface.Controls.MPLabel label23;
    private MediaPortal.UserInterface.Controls.MPTabPage TopBar;
    private MediaPortal.UserInterface.Controls.MPButton button2;
    private MediaPortal.UserInterface.Controls.MPButton button3;
    private MediaPortal.UserInterface.Controls.MPCheckBox useTopBarSub;
    private MediaPortal.UserInterface.Controls.MPCheckBox useMenuShortcuts;
    private MediaPortal.UserInterface.Controls.MPCheckBox NoTopBar;
    private MediaPortal.UserInterface.Controls.MPLabel label24;
    private MediaPortal.UserInterface.Controls.MPCheckBox ActivateSpecial;
    private MediaPortal.UserInterface.Controls.MPLabel label25;
    private System.Windows.Forms.PictureBox pictureBox4;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox6;
    private MediaPortal.UserInterface.Controls.MPCheckBox StartScript;
    private MediaPortal.UserInterface.Controls.MPCheckBox EndScript;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox2;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox3;
    private MediaPortal.UserInterface.Controls.MPButton addConfig;

    #region plugin vars
    public bool CanEnable()		// Indicates whether plugin can be enabled/disabled
    {
      return false;
    }

    public int GetWindowId()
    {
      return (int)GUIWindow.Window.WINDOW_HOME;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = "";
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return false;
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public string PluginName()
    {
      return "Home";
    }

    public string Description()
    {
      return "Configures the appearance of the MediaPortal home screen";
    }

    public string Author()
    {
      return "Gucky62/Frodo";
    }

    public void ShowPlugin()
    {
      ShowDialog();
    }

    public bool HasSetup()
    {
      return true;
    }
    #endregion

    public SetupForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupForm));
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBox1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.SearchPicture = new MediaPortal.UserInterface.Controls.MPButton();
      this.MakeMenu = new MediaPortal.UserInterface.Controls.MPButton();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBox5 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBox6 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBox3 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBox4 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBox2 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBox1 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.SaveAll = new MediaPortal.UserInterface.Controls.MPButton();
      this.DeleteItem = new MediaPortal.UserInterface.Controls.MPButton();
      this.AddMenu = new MediaPortal.UserInterface.Controls.MPButton();
      this.CopyItem = new MediaPortal.UserInterface.Controls.MPButton();
      this.listBox = new System.Windows.Forms.ListBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.NoScrollSubs = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.chkBoxFixed = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.chkBoxScrolling = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.radioButton2 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButton1 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.treeView = new System.Windows.Forms.TreeView();
      this.AddPicture = new MediaPortal.UserInterface.Controls.MPButton();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.deletePicture = new MediaPortal.UserInterface.Controls.MPButton();
      this.label9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.pictureBox2 = new System.Windows.Forms.PictureBox();
      this.label10 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label11 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.pictureBox3 = new System.Windows.Forms.PictureBox();
      this.addConfig = new MediaPortal.UserInterface.Controls.MPButton();
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.TopBar = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox6 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBox3 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBox2 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.EndScript = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.StartScript = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label24 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox5 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.NoTopBar = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.useMenuShortcuts = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.useTopBarSub = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.BackButtons = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label21 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.TestDate = new MediaPortal.UserInterface.Controls.MPButton();
      this.DateTest = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label20 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label17 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label19 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label18 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label16 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label15 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label14 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label13 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label12 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.OwnDate = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.radioButton3 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.button1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.tabPage2 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.AddSpecial = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label25 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.pictureBox4 = new System.Windows.Forms.PictureBox();
      this.label23 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label22 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.useMyPlugins = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.useMenus = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPage3 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.ActivateSpecial = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.button2 = new MediaPortal.UserInterface.Controls.MPButton();
      this.SpecialFunctions = new System.Windows.Forms.ListBox();
      this.tabPage4 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.button3 = new MediaPortal.UserInterface.Controls.MPButton();
      this.listView = new System.Windows.Forms.ListView();
      this.Type = new System.Windows.Forms.ColumnHeader();
      this.Plugin = new System.Windows.Forms.ColumnHeader();
      this.Key = new System.Windows.Forms.ColumnHeader();
      this.Fullscreen = new System.Windows.Forms.ColumnHeader();
      this.groupBox3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.groupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
      this.tabControl1.SuspendLayout();
      this.TopBar.SuspendLayout();
      this.groupBox6.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
      this.tabPage3.SuspendLayout();
      this.tabPage4.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.pictureBox1);
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox3.Location = new System.Drawing.Point(272, 8);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(216, 192);
      this.groupBox3.TabIndex = 27;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Button Image (for Menu Items)";
      // 
      // pictureBox1
      // 
      this.pictureBox1.Location = new System.Drawing.Point(8, 16);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(200, 168);
      this.pictureBox1.TabIndex = 0;
      this.pictureBox1.TabStop = false;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.comboBox1);
      this.groupBox2.Controls.Add(this.SearchPicture);
      this.groupBox2.Controls.Add(this.MakeMenu);
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Controls.Add(this.label8);
      this.groupBox2.Controls.Add(this.textBox5);
      this.groupBox2.Controls.Add(this.textBox6);
      this.groupBox2.Controls.Add(this.label5);
      this.groupBox2.Controls.Add(this.label6);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.textBox3);
      this.groupBox2.Controls.Add(this.textBox4);
      this.groupBox2.Controls.Add(this.textBox2);
      this.groupBox2.Controls.Add(this.textBox1);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(496, 8);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(320, 192);
      this.groupBox2.TabIndex = 26;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Tag Info";
      // 
      // comboBox1
      // 
      this.comboBox1.Enabled = false;
      this.comboBox1.Location = new System.Drawing.Point(96, 16);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(168, 21);
      this.comboBox1.TabIndex = 13;
      this.comboBox1.Visible = false;
      // 
      // SearchPicture
      // 
      this.SearchPicture.Location = new System.Drawing.Point(272, 16);
      this.SearchPicture.Name = "SearchPicture";
      this.SearchPicture.Size = new System.Drawing.Size(32, 24);
      this.SearchPicture.TabIndex = 12;
      this.SearchPicture.Text = "...";
      this.SearchPicture.UseVisualStyleBackColor = true;
      this.SearchPicture.Visible = false;
      this.SearchPicture.Click += new System.EventHandler(this.SearchPicture_Click);
      // 
      // MakeMenu
      // 
      this.MakeMenu.Location = new System.Drawing.Point(112, 160);
      this.MakeMenu.Name = "MakeMenu";
      this.MakeMenu.Size = new System.Drawing.Size(80, 24);
      this.MakeMenu.TabIndex = 1;
      this.MakeMenu.Text = "Make Menu";
      this.MakeMenu.UseVisualStyleBackColor = true;
      this.MakeMenu.Click += new System.EventHandler(this.MakeMenu_Click);
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(16, 120);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(80, 16);
      this.label7.TabIndex = 11;
      this.label7.Text = "Author";
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(16, 144);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(80, 16);
      this.label8.TabIndex = 10;
      this.label8.Text = "Plugin Name";
      // 
      // textBox5
      // 
      this.textBox5.Location = new System.Drawing.Point(96, 112);
      this.textBox5.Name = "textBox5";
      this.textBox5.Size = new System.Drawing.Size(168, 20);
      this.textBox5.TabIndex = 9;
      // 
      // textBox6
      // 
      this.textBox6.Location = new System.Drawing.Point(96, 136);
      this.textBox6.Name = "textBox6";
      this.textBox6.Size = new System.Drawing.Size(168, 20);
      this.textBox6.TabIndex = 8;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 72);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(80, 16);
      this.label5.TabIndex = 7;
      this.label5.Text = "DLL Name";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 96);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(80, 16);
      this.label6.TabIndex = 6;
      this.label6.Text = "Plugin Name";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 48);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(80, 16);
      this.label4.TabIndex = 5;
      this.label4.Text = "Button Text";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 24);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(80, 16);
      this.label3.TabIndex = 4;
      this.label3.Text = "Tag Type";
      // 
      // textBox3
      // 
      this.textBox3.Location = new System.Drawing.Point(96, 64);
      this.textBox3.Name = "textBox3";
      this.textBox3.Size = new System.Drawing.Size(168, 20);
      this.textBox3.TabIndex = 3;
      // 
      // textBox4
      // 
      this.textBox4.Location = new System.Drawing.Point(96, 88);
      this.textBox4.Name = "textBox4";
      this.textBox4.Size = new System.Drawing.Size(168, 20);
      this.textBox4.TabIndex = 2;
      // 
      // textBox2
      // 
      this.textBox2.Location = new System.Drawing.Point(96, 40);
      this.textBox2.Name = "textBox2";
      this.textBox2.Size = new System.Drawing.Size(168, 20);
      this.textBox2.TabIndex = 1;
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(96, 16);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(168, 20);
      this.textBox1.TabIndex = 0;
      this.textBox1.Text = "textBox1";
      // 
      // SaveAll
      // 
      this.SaveAll.Location = new System.Drawing.Point(368, 456);
      this.SaveAll.Name = "SaveAll";
      this.SaveAll.Size = new System.Drawing.Size(88, 24);
      this.SaveAll.TabIndex = 25;
      this.SaveAll.Text = "Save";
      this.SaveAll.UseVisualStyleBackColor = true;
      this.SaveAll.Click += new System.EventHandler(this.SaveAll_Click);
      // 
      // DeleteItem
      // 
      this.DeleteItem.Location = new System.Drawing.Point(368, 248);
      this.DeleteItem.Name = "DeleteItem";
      this.DeleteItem.Size = new System.Drawing.Size(88, 24);
      this.DeleteItem.TabIndex = 22;
      this.DeleteItem.Text = "Delete Item";
      this.DeleteItem.UseVisualStyleBackColor = true;
      this.DeleteItem.Click += new System.EventHandler(this.DeleteItem_Click);
      // 
      // AddMenu
      // 
      this.AddMenu.Location = new System.Drawing.Point(368, 312);
      this.AddMenu.Name = "AddMenu";
      this.AddMenu.Size = new System.Drawing.Size(88, 24);
      this.AddMenu.TabIndex = 21;
      this.AddMenu.Text = "Add Menu";
      this.AddMenu.UseVisualStyleBackColor = true;
      this.AddMenu.Click += new System.EventHandler(this.AddMenu_Click);
      // 
      // CopyItem
      // 
      this.CopyItem.Location = new System.Drawing.Point(368, 216);
      this.CopyItem.Name = "CopyItem";
      this.CopyItem.Size = new System.Drawing.Size(88, 24);
      this.CopyItem.TabIndex = 19;
      this.CopyItem.Text = "<---";
      this.CopyItem.UseVisualStyleBackColor = true;
      this.CopyItem.Click += new System.EventHandler(this.CopyItem_Click);
      // 
      // listBox
      // 
      this.listBox.Location = new System.Drawing.Point(472, 216);
      this.listBox.Name = "listBox";
      this.listBox.Size = new System.Drawing.Size(344, 264);
      this.listBox.TabIndex = 17;
      this.listBox.SelectedIndexChanged += new System.EventHandler(this.listBox_SelectedIndexChanged);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 200);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(104, 16);
      this.label1.TabIndex = 16;
      this.label1.Text = "Menu Structure";
      // 
      // NoScrollSubs
      // 
      this.NoScrollSubs.AutoSize = true;
      this.NoScrollSubs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.NoScrollSubs.Location = new System.Drawing.Point(24, 72);
      this.NoScrollSubs.Name = "NoScrollSubs";
      this.NoScrollSubs.Size = new System.Drawing.Size(118, 17);
      this.NoScrollSubs.TabIndex = 6;
      this.NoScrollSubs.Text = "NoScroll SubMenus";
      this.NoScrollSubs.UseVisualStyleBackColor = true;
      // 
      // chkBoxFixed
      // 
      this.chkBoxFixed.AutoSize = true;
      this.chkBoxFixed.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkBoxFixed.Location = new System.Drawing.Point(24, 48);
      this.chkBoxFixed.Name = "chkBoxFixed";
      this.chkBoxFixed.Size = new System.Drawing.Size(85, 17);
      this.chkBoxFixed.TabIndex = 3;
      this.chkBoxFixed.Text = "Fix Scroll Bar";
      this.chkBoxFixed.UseVisualStyleBackColor = true;
      // 
      // chkBoxScrolling
      // 
      this.chkBoxScrolling.AutoSize = true;
      this.chkBoxScrolling.Checked = true;
      this.chkBoxScrolling.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkBoxScrolling.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkBoxScrolling.Location = new System.Drawing.Point(24, 24);
      this.chkBoxScrolling.Name = "chkBoxScrolling";
      this.chkBoxScrolling.Size = new System.Drawing.Size(106, 17);
      this.chkBoxScrolling.TabIndex = 2;
      this.chkBoxScrolling.Text = "Scroll menu items";
      this.chkBoxScrolling.UseVisualStyleBackColor = true;
      // 
      // radioButton2
      // 
      this.radioButton2.AutoSize = true;
      this.radioButton2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButton2.Location = new System.Drawing.Point(16, 56);
      this.radioButton2.Name = "radioButton2";
      this.radioButton2.Size = new System.Drawing.Size(95, 17);
      this.radioButton2.TabIndex = 1;
      this.radioButton2.Text = "Day Month DD";
      this.radioButton2.UseVisualStyleBackColor = true;
      this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
      // 
      // radioButton1
      // 
      this.radioButton1.AutoSize = true;
      this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButton1.Location = new System.Drawing.Point(16, 24);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(98, 17);
      this.radioButton1.TabIndex = 0;
      this.radioButton1.Text = "Day DD. Month";
      this.radioButton1.UseVisualStyleBackColor = true;
      this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
      // 
      // treeView
      // 
      this.treeView.AllowDrop = true;
      this.treeView.LabelEdit = true;
      this.treeView.Location = new System.Drawing.Point(8, 216);
      this.treeView.Name = "treeView";
      this.treeView.Size = new System.Drawing.Size(344, 264);
      this.treeView.TabIndex = 14;
      this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
      // 
      // AddPicture
      // 
      this.AddPicture.Location = new System.Drawing.Point(368, 344);
      this.AddPicture.Name = "AddPicture";
      this.AddPicture.Size = new System.Drawing.Size(88, 24);
      this.AddPicture.TabIndex = 28;
      this.AddPicture.Text = "Link Picture";
      this.AddPicture.UseVisualStyleBackColor = true;
      this.AddPicture.Click += new System.EventHandler(this.AddPicture_Click);
      // 
      // deletePicture
      // 
      this.deletePicture.Location = new System.Drawing.Point(368, 280);
      this.deletePicture.Name = "deletePicture";
      this.deletePicture.Size = new System.Drawing.Size(88, 24);
      this.deletePicture.TabIndex = 29;
      this.deletePicture.Text = "Delete Picture";
      this.deletePicture.UseVisualStyleBackColor = true;
      this.deletePicture.Click += new System.EventHandler(this.deletePicture_Click);
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(8, 64);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(208, 32);
      this.label9.TabIndex = 30;
      this.label9.Text = "You can move any Menu Item with drag and drop";
      // 
      // pictureBox2
      // 
      this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
      this.pictureBox2.Location = new System.Drawing.Point(8, 96);
      this.pictureBox2.Name = "pictureBox2";
      this.pictureBox2.Size = new System.Drawing.Size(16, 24);
      this.pictureBox2.TabIndex = 31;
      this.pictureBox2.TabStop = false;
      // 
      // label10
      // 
      this.label10.Location = new System.Drawing.Point(32, 96);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(168, 16);
      this.label10.TabIndex = 32;
      this.label10.Text = "is a Menu Item ";
      // 
      // label11
      // 
      this.label11.Location = new System.Drawing.Point(32, 120);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(88, 16);
      this.label11.TabIndex = 34;
      this.label11.Text = "is a Plugin Item";
      // 
      // pictureBox3
      // 
      this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
      this.pictureBox3.Location = new System.Drawing.Point(8, 120);
      this.pictureBox3.Name = "pictureBox3";
      this.pictureBox3.Size = new System.Drawing.Size(16, 24);
      this.pictureBox3.TabIndex = 33;
      this.pictureBox3.TabStop = false;
      // 
      // addConfig
      // 
      this.addConfig.Location = new System.Drawing.Point(368, 376);
      this.addConfig.Name = "addConfig";
      this.addConfig.Size = new System.Drawing.Size(88, 32);
      this.addConfig.TabIndex = 35;
      this.addConfig.Text = "Add Configuration";
      this.addConfig.UseVisualStyleBackColor = true;
      this.addConfig.Click += new System.EventHandler(this.addConfig_Click);
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.TopBar);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Location = new System.Drawing.Point(8, 8);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(840, 520);
      this.tabControl1.TabIndex = 36;
      // 
      // TopBar
      // 
      this.TopBar.Controls.Add(this.groupBox6);
      this.TopBar.Controls.Add(this.label24);
      this.TopBar.Controls.Add(this.groupBox5);
      this.TopBar.Controls.Add(this.groupBox4);
      this.TopBar.Controls.Add(this.button1);
      this.TopBar.Location = new System.Drawing.Point(4, 22);
      this.TopBar.Name = "TopBar";
      this.TopBar.Size = new System.Drawing.Size(832, 494);
      this.TopBar.TabIndex = 0;
      this.TopBar.Text = "Home Design";
      this.TopBar.UseVisualStyleBackColor = true;
      // 
      // groupBox6
      // 
      this.groupBox6.Controls.Add(this.comboBox3);
      this.groupBox6.Controls.Add(this.comboBox2);
      this.groupBox6.Controls.Add(this.EndScript);
      this.groupBox6.Controls.Add(this.StartScript);
      this.groupBox6.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox6.Location = new System.Drawing.Point(312, 296);
      this.groupBox6.Name = "groupBox6";
      this.groupBox6.Size = new System.Drawing.Size(456, 72);
      this.groupBox6.TabIndex = 30;
      this.groupBox6.TabStop = false;
      this.groupBox6.Text = "Special Scripts";
      this.groupBox6.Visible = false;
      // 
      // comboBox3
      // 
      this.comboBox3.Location = new System.Drawing.Point(160, 40);
      this.comboBox3.Name = "comboBox3";
      this.comboBox3.Size = new System.Drawing.Size(240, 21);
      this.comboBox3.TabIndex = 3;
      // 
      // comboBox2
      // 
      this.comboBox2.Location = new System.Drawing.Point(160, 16);
      this.comboBox2.Name = "comboBox2";
      this.comboBox2.Size = new System.Drawing.Size(240, 21);
      this.comboBox2.TabIndex = 2;
      // 
      // EndScript
      // 
      this.EndScript.AutoSize = true;
      this.EndScript.Enabled = false;
      this.EndScript.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.EndScript.Location = new System.Drawing.Point(16, 40);
      this.EndScript.Name = "EndScript";
      this.EndScript.Size = new System.Drawing.Size(101, 17);
      this.EndScript.TabIndex = 1;
      this.EndScript.Text = "Script at MP exit";
      this.EndScript.UseVisualStyleBackColor = true;
      // 
      // StartScript
      // 
      this.StartScript.AutoSize = true;
      this.StartScript.Enabled = false;
      this.StartScript.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.StartScript.Location = new System.Drawing.Point(16, 16);
      this.StartScript.Name = "StartScript";
      this.StartScript.Size = new System.Drawing.Size(117, 17);
      this.StartScript.TabIndex = 0;
      this.StartScript.Text = "Script at MP startup";
      this.StartScript.UseVisualStyleBackColor = true;
      // 
      // label24
      // 
      this.label24.Location = new System.Drawing.Point(32, 248);
      this.label24.Name = "label24";
      this.label24.Size = new System.Drawing.Size(240, 16);
      this.label24.TabIndex = 29;
      this.label24.Text = "*Topbar2 is with Navigation Buttons";
      // 
      // groupBox5
      // 
      this.groupBox5.Controls.Add(this.NoTopBar);
      this.groupBox5.Controls.Add(this.useMenuShortcuts);
      this.groupBox5.Controls.Add(this.useTopBarSub);
      this.groupBox5.Controls.Add(this.BackButtons);
      this.groupBox5.Controls.Add(this.NoScrollSubs);
      this.groupBox5.Controls.Add(this.chkBoxFixed);
      this.groupBox5.Controls.Add(this.chkBoxScrolling);
      this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox5.Location = new System.Drawing.Point(32, 24);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(232, 216);
      this.groupBox5.TabIndex = 28;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "Home Settings";
      // 
      // NoTopBar
      // 
      this.NoTopBar.AutoSize = true;
      this.NoTopBar.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.NoTopBar.Location = new System.Drawing.Point(24, 160);
      this.NoTopBar.Name = "NoTopBar";
      this.NoTopBar.Size = new System.Drawing.Size(139, 17);
      this.NoTopBar.TabIndex = 10;
      this.NoTopBar.Text = "No Topbar in Submenus";
      this.NoTopBar.UseVisualStyleBackColor = true;
      this.NoTopBar.CheckedChanged += new System.EventHandler(this.NoTopBar_CheckedChanged);
      // 
      // useMenuShortcuts
      // 
      this.useMenuShortcuts.AutoSize = true;
      this.useMenuShortcuts.Enabled = false;
      this.useMenuShortcuts.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useMenuShortcuts.Location = new System.Drawing.Point(24, 184);
      this.useMenuShortcuts.Name = "useMenuShortcuts";
      this.useMenuShortcuts.Size = new System.Drawing.Size(121, 17);
      this.useMenuShortcuts.TabIndex = 9;
      this.useMenuShortcuts.Text = "Use Menu Shortcuts";
      this.useMenuShortcuts.UseVisualStyleBackColor = true;
      this.useMenuShortcuts.Visible = false;
      // 
      // useTopBarSub
      // 
      this.useTopBarSub.AutoSize = true;
      this.useTopBarSub.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useTopBarSub.Location = new System.Drawing.Point(24, 136);
      this.useTopBarSub.Name = "useTopBarSub";
      this.useTopBarSub.Size = new System.Drawing.Size(135, 17);
      this.useTopBarSub.TabIndex = 8;
      this.useTopBarSub.Text = "Topbar2*  in Submenus";
      this.useTopBarSub.UseVisualStyleBackColor = true;
      this.useTopBarSub.CheckedChanged += new System.EventHandler(this.useTopBarSub_CheckedChanged);
      // 
      // BackButtons
      // 
      this.BackButtons.AutoSize = true;
      this.BackButtons.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.BackButtons.Location = new System.Drawing.Point(24, 112);
      this.BackButtons.Name = "BackButtons";
      this.BackButtons.Size = new System.Drawing.Size(147, 17);
      this.BackButtons.TabIndex = 7;
      this.BackButtons.Text = "Back Button in Submenus";
      this.BackButtons.UseVisualStyleBackColor = true;
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.label21);
      this.groupBox4.Controls.Add(this.TestDate);
      this.groupBox4.Controls.Add(this.DateTest);
      this.groupBox4.Controls.Add(this.label20);
      this.groupBox4.Controls.Add(this.label17);
      this.groupBox4.Controls.Add(this.label19);
      this.groupBox4.Controls.Add(this.label18);
      this.groupBox4.Controls.Add(this.label16);
      this.groupBox4.Controls.Add(this.label15);
      this.groupBox4.Controls.Add(this.label14);
      this.groupBox4.Controls.Add(this.label13);
      this.groupBox4.Controls.Add(this.label12);
      this.groupBox4.Controls.Add(this.OwnDate);
      this.groupBox4.Controls.Add(this.radioButton3);
      this.groupBox4.Controls.Add(this.radioButton2);
      this.groupBox4.Controls.Add(this.radioButton1);
      this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox4.Location = new System.Drawing.Point(312, 24);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(456, 256);
      this.groupBox4.TabIndex = 27;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Date & Time Settings";
      // 
      // label21
      // 
      this.label21.Location = new System.Drawing.Point(16, 232);
      this.label21.Name = "label21";
      this.label21.Size = new System.Drawing.Size(392, 16);
      this.label21.TabIndex = 16;
      this.label21.Text = "* When MP runs the  Date will be converted in your language !";
      // 
      // TestDate
      // 
      this.TestDate.Location = new System.Drawing.Point(224, 192);
      this.TestDate.Name = "TestDate";
      this.TestDate.Size = new System.Drawing.Size(120, 24);
      this.TestDate.TabIndex = 15;
      this.TestDate.Text = "Test selected Date *";
      this.TestDate.UseVisualStyleBackColor = true;
      this.TestDate.Click += new System.EventHandler(this.TestDate_Click);
      // 
      // DateTest
      // 
      this.DateTest.Location = new System.Drawing.Point(16, 192);
      this.DateTest.Name = "DateTest";
      this.DateTest.Size = new System.Drawing.Size(184, 20);
      this.DateTest.TabIndex = 14;
      // 
      // label20
      // 
      this.label20.Location = new System.Drawing.Point(224, 168);
      this.label20.Name = "label20";
      this.label20.Size = new System.Drawing.Size(216, 16);
      this.label20.TabIndex = 13;
      this.label20.Text = "shows-> Monday 1. March";
      // 
      // label17
      // 
      this.label17.Location = new System.Drawing.Point(224, 152);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(216, 16);
      this.label17.TabIndex = 12;
      this.label17.Text = "e.g.  Day DD. Month";
      // 
      // label19
      // 
      this.label19.Location = new System.Drawing.Point(224, 128);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(224, 16);
      this.label19.TabIndex = 11;
      this.label19.Text = "Year = Year as a long number ( e.g. 2005)";
      // 
      // label18
      // 
      this.label18.Location = new System.Drawing.Point(224, 112);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(208, 16);
      this.label18.TabIndex = 9;
      this.label18.Text = "YY = Year as a short number ( e.g.  05)";
      // 
      // label16
      // 
      this.label16.Location = new System.Drawing.Point(224, 88);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(192, 16);
      this.label16.TabIndex = 8;
      this.label16.Text = "Month = Month as a text ( e.g.  July)";
      // 
      // label15
      // 
      this.label15.Location = new System.Drawing.Point(224, 72);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(192, 16);
      this.label15.TabIndex = 7;
      this.label15.Text = "MM = Month as a number ( e.g.  10)";
      // 
      // label14
      // 
      this.label14.Location = new System.Drawing.Point(224, 48);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(192, 16);
      this.label14.TabIndex = 6;
      this.label14.Text = "Day = Day as a text ( e.g.  Monday)";
      // 
      // label13
      // 
      this.label13.Location = new System.Drawing.Point(224, 32);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(192, 16);
      this.label13.TabIndex = 5;
      this.label13.Text = "DD = Day as a number  ( e.g. 23)";
      // 
      // label12
      // 
      this.label12.Location = new System.Drawing.Point(224, 16);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(192, 16);
      this.label12.TabIndex = 4;
      this.label12.Text = "You can use the following tags:";
      // 
      // OwnDate
      // 
      this.OwnDate.Enabled = false;
      this.OwnDate.Location = new System.Drawing.Point(16, 160);
      this.OwnDate.Name = "OwnDate";
      this.OwnDate.Size = new System.Drawing.Size(184, 20);
      this.OwnDate.TabIndex = 3;
      this.OwnDate.Text = "Day DD. Month";
      // 
      // radioButton3
      // 
      this.radioButton3.AutoSize = true;
      this.radioButton3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButton3.Location = new System.Drawing.Point(16, 88);
      this.radioButton3.Name = "radioButton3";
      this.radioButton3.Size = new System.Drawing.Size(128, 17);
      this.radioButton3.TabIndex = 2;
      this.radioButton3.Text = "Build your own Format";
      this.radioButton3.UseVisualStyleBackColor = true;
      this.radioButton3.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(40, 448);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(88, 24);
      this.button1.TabIndex = 26;
      this.button1.Text = "Save";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.SaveAll_Click);
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.AddSpecial);
      this.tabPage2.Controls.Add(this.listBox);
      this.tabPage2.Controls.Add(this.groupBox2);
      this.tabPage2.Controls.Add(this.AddPicture);
      this.tabPage2.Controls.Add(this.CopyItem);
      this.tabPage2.Controls.Add(this.DeleteItem);
      this.tabPage2.Controls.Add(this.AddMenu);
      this.tabPage2.Controls.Add(this.deletePicture);
      this.tabPage2.Controls.Add(this.addConfig);
      this.tabPage2.Controls.Add(this.groupBox3);
      this.tabPage2.Controls.Add(this.treeView);
      this.tabPage2.Controls.Add(this.groupBox1);
      this.tabPage2.Controls.Add(this.label1);
      this.tabPage2.Controls.Add(this.SaveAll);
      this.tabPage2.Controls.Add(this.label2);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(832, 470);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Menu Structure";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // AddSpecial
      // 
      this.AddSpecial.Location = new System.Drawing.Point(368, 416);
      this.AddSpecial.Name = "AddSpecial";
      this.AddSpecial.Size = new System.Drawing.Size(88, 32);
      this.AddSpecial.TabIndex = 36;
      this.AddSpecial.Text = "Add Script Function";
      this.AddSpecial.UseVisualStyleBackColor = true;
      this.AddSpecial.Click += new System.EventHandler(this.AddSpecial_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label25);
      this.groupBox1.Controls.Add(this.pictureBox4);
      this.groupBox1.Controls.Add(this.label23);
      this.groupBox1.Controls.Add(this.label22);
      this.groupBox1.Controls.Add(this.useMyPlugins);
      this.groupBox1.Controls.Add(this.useMenus);
      this.groupBox1.Controls.Add(this.label9);
      this.groupBox1.Controls.Add(this.pictureBox2);
      this.groupBox1.Controls.Add(this.label10);
      this.groupBox1.Controls.Add(this.label11);
      this.groupBox1.Controls.Add(this.pictureBox3);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(256, 192);
      this.groupBox1.TabIndex = 15;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Menu settings";
      // 
      // label25
      // 
      this.label25.Location = new System.Drawing.Point(32, 144);
      this.label25.Name = "label25";
      this.label25.Size = new System.Drawing.Size(88, 16);
      this.label25.TabIndex = 38;
      this.label25.Text = "is a Script Item";
      // 
      // pictureBox4
      // 
      this.pictureBox4.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox4.Image")));
      this.pictureBox4.Location = new System.Drawing.Point(8, 144);
      this.pictureBox4.Name = "pictureBox4";
      this.pictureBox4.Size = new System.Drawing.Size(16, 24);
      this.pictureBox4.TabIndex = 37;
      this.pictureBox4.TabStop = false;
      // 
      // label23
      // 
      this.label23.Location = new System.Drawing.Point(8, 168);
      this.label23.Name = "label23";
      this.label23.Size = new System.Drawing.Size(88, 16);
      this.label23.TabIndex = 36;
      this.label23.Text = "(MENU NAME)";
      // 
      // label22
      // 
      this.label22.Location = new System.Drawing.Point(104, 168);
      this.label22.Name = "label22";
      this.label22.Size = new System.Drawing.Size(112, 16);
      this.label22.TabIndex = 35;
      this.label22.Text = "{PICTURE NAME}";
      // 
      // useMyPlugins
      // 
      this.useMyPlugins.AutoSize = true;
      this.useMyPlugins.Checked = true;
      this.useMyPlugins.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useMyPlugins.Location = new System.Drawing.Point(16, 16);
      this.useMyPlugins.Name = "useMyPlugins";
      this.useMyPlugins.Size = new System.Drawing.Size(198, 17);
      this.useMyPlugins.TabIndex = 1;
      this.useMyPlugins.TabStop = true;
      this.useMyPlugins.Text = "Use MyPlugins (2-level home screen)";
      this.useMyPlugins.UseVisualStyleBackColor = true;
      // 
      // useMenus
      // 
      this.useMenus.AutoSize = true;
      this.useMenus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useMenus.Location = new System.Drawing.Point(16, 40);
      this.useMenus.Name = "useMenus";
      this.useMenus.Size = new System.Drawing.Size(155, 17);
      this.useMenus.TabIndex = 0;
      this.useMenus.Text = "Use Menus (custom menus)";
      this.useMenus.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(472, 200);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(104, 16);
      this.label2.TabIndex = 37;
      this.label2.Text = "Available Plugins";
      // 
      // tabPage3
      // 
      this.tabPage3.Controls.Add(this.ActivateSpecial);
      this.tabPage3.Controls.Add(this.button2);
      this.tabPage3.Controls.Add(this.SpecialFunctions);
      this.tabPage3.Location = new System.Drawing.Point(4, 22);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Size = new System.Drawing.Size(832, 494);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Script Functions";
      this.tabPage3.UseVisualStyleBackColor = true;
      // 
      // ActivateSpecial
      // 
      this.ActivateSpecial.AutoSize = true;
      this.ActivateSpecial.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ActivateSpecial.Location = new System.Drawing.Point(32, 32);
      this.ActivateSpecial.Name = "ActivateSpecial";
      this.ActivateSpecial.Size = new System.Drawing.Size(137, 17);
      this.ActivateSpecial.TabIndex = 29;
      this.ActivateSpecial.Text = "Activate script funktions";
      this.ActivateSpecial.UseVisualStyleBackColor = true;
      this.ActivateSpecial.CheckedChanged += new System.EventHandler(this.ActivateSpecial_CheckedChanged);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(40, 448);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(88, 24);
      this.button2.TabIndex = 27;
      this.button2.Text = "Save";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.SaveAll_Click);
      // 
      // SpecialFunctions
      // 
      this.SpecialFunctions.Enabled = false;
      this.SpecialFunctions.Location = new System.Drawing.Point(408, 32);
      this.SpecialFunctions.Name = "SpecialFunctions";
      this.SpecialFunctions.Size = new System.Drawing.Size(392, 407);
      this.SpecialFunctions.TabIndex = 0;
      // 
      // tabPage4
      // 
      this.tabPage4.Controls.Add(this.button3);
      this.tabPage4.Controls.Add(this.listView);
      this.tabPage4.Location = new System.Drawing.Point(4, 22);
      this.tabPage4.Name = "tabPage4";
      this.tabPage4.Size = new System.Drawing.Size(832, 494);
      this.tabPage4.TabIndex = 3;
      this.tabPage4.Text = "Menu Shortcuts";
      this.tabPage4.UseVisualStyleBackColor = true;
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(40, 448);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(88, 24);
      this.button3.TabIndex = 27;
      this.button3.Text = "Save";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.SaveAll_Click);
      // 
      // listView
      // 
      this.listView.AutoArrange = false;
      this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Type,
            this.Plugin,
            this.Key,
            this.Fullscreen});
      this.listView.FullRowSelect = true;
      this.listView.GridLines = true;
      this.listView.LabelEdit = true;
      this.listView.Location = new System.Drawing.Point(240, 16);
      this.listView.MultiSelect = false;
      this.listView.Name = "listView";
      this.listView.Size = new System.Drawing.Size(552, 432);
      this.listView.TabIndex = 19;
      this.listView.UseCompatibleStateImageBehavior = false;
      this.listView.View = System.Windows.Forms.View.Details;
      // 
      // Type
      // 
      this.Type.Text = "Type";
      this.Type.Width = 72;
      // 
      // Plugin
      // 
      this.Plugin.Text = "Plugin";
      this.Plugin.Width = 282;
      // 
      // Key
      // 
      this.Key.Text = "Key";
      this.Key.Width = 77;
      // 
      // Fullscreen
      // 
      this.Fullscreen.Text = "Full Screen";
      this.Fullscreen.Width = 82;
      // 
      // SetupForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(856, 538);
      this.Controls.Add(this.tabControl1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "SetupForm";
      this.Text = "Home Setup";
      this.Load += new System.EventHandler(this.SetupForm_Load);
      this.groupBox3.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
      this.tabControl1.ResumeLayout(false);
      this.TopBar.ResumeLayout(false);
      this.groupBox6.ResumeLayout(false);
      this.groupBox6.PerformLayout();
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.tabPage2.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
      this.tabPage3.ResumeLayout(false);
      this.tabPage3.PerformLayout();
      this.tabPage4.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private void AddMenu_Click(object sender, System.EventArgs e)
    {
      MakeMenu.Visible = true;
      MakeMenu.Text = "Make Menu";
      label3.Text = "Menu Name";
      textBox1.Text = "";
      textBox2.Text = "";
      textBox3.Text = "";
      textBox4.Text = "";
      textBox5.Text = "";
      textBox6.Text = "";
      label4.Visible = false;
      textBox2.Visible = false;
      label5.Visible = false;
      textBox3.Visible = false;
      label6.Visible = false;
      textBox4.Visible = false;
      label7.Visible = false;
      textBox5.Visible = false;
      label8.Visible = false;
      textBox6.Visible = false;
      textBox1.Focus();
    }

    private void MakeMenu_Click(object sender, System.EventArgs e)
    {
      comboBox1.Visible = false;
      textBox1.Visible = true;
      MakeMenu.Visible = false;
      textBox2.Visible = true;
      label4.Visible = true;
      textBox2.Visible = true;
      label5.Visible = true;
      textBox3.Visible = true;
      label6.Visible = true;
      textBox4.Visible = true;
      label7.Visible = true;
      textBox5.Visible = true;
      label8.Visible = true;
      textBox6.Visible = true;
      if (addPicture == true)
      {
        addPicture = false;
        TreeNode tn = new TreeNode();
        if (treeView.SelectedNode != null)
        {
          tn = treeView.SelectedNode;
          if (tn.Text.IndexOf("{", 0) > 0)
          {
            int l = tn.Text.IndexOf(" {", 0);
            tn.Text = tn.Text.Substring(0, l);
            tn.Text = tn.Text + " {" + textBox1.Text + "}";
          }
          else
          {
            tn.Text = tn.Text + " {" + textBox1.Text + "}";
          }
        }
        SearchPicture.Visible = false;
        UpdateTagInfo(textBox1.Text);
      }
      else if (addScript == true)
      {

        ++this.FolderCount;
        string mName = "[" + comboBox1.Items[comboBox1.SelectedIndex] + "]";
        this.treeView.Nodes.Add(new TreeNode(mName, 2, 2));
        UpdateTagInfo(mName);
      }
      else
      {
        ++this.FolderCount;
        string mName = "(" + textBox1.Text + ")";
        this.treeView.Nodes.Add(new TreeNode(mName, 0, 0));
        UpdateTagInfo(mName);
      }
    }

    private void treeView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
    {
      UpdateTagInfo(treeView.SelectedNode.Text);
    }

    private void CopyItem_Click(object sender, System.EventArgs e)
    {
      ++this.NodeCount;
      this.treeView.Nodes.Add(new TreeNode(listBox.SelectedItem.ToString(), 1, 1));
    }

    private void SaveAll_Click(object sender, System.EventArgs e)
    {
      saveTree(treeView, Application.StartupPath + @"\menu2.bin");
      using (MediaPortal.Profile.Settings xmlWriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        int iLayout = 0;
        if (radioButton2.Checked) iLayout = 1;
        if (radioButton3.Checked) iLayout = 2;
        xmlWriter.SetValue("home", "datelayout", iLayout.ToString());
        xmlWriter.SetValueAsBool("home", "scroll", chkBoxScrolling.Checked);
        xmlWriter.SetValueAsBool("home", "scrollfixed", chkBoxFixed.Checked);
        xmlWriter.SetValueAsBool("home", "usemenus", useMenus.Checked);
        xmlWriter.SetValueAsBool("home", "usemyplugins", useMyPlugins.Checked);
        xmlWriter.SetValueAsBool("home", "noScrollsubs", NoScrollSubs.Checked);
        xmlWriter.SetValueAsBool("home", "backbuttons", BackButtons.Checked);
        xmlWriter.SetValueAsBool("home", "useTopBarSub", useTopBarSub.Checked);
        xmlWriter.SetValueAsBool("home", "noTopBarSub", NoTopBar.Checked);
        xmlWriter.SetValueAsBool("home", "useMenuShortcuts", useMenuShortcuts.Checked);
        xmlWriter.SetValue("home", "ownDate", OwnDate.Text);
        xmlWriter.SetValueAsBool("home", "activateSpecial", ActivateSpecial.Checked);
      }
      this.Close();
    }

    private void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        int iLayout = xmlreader.GetValueAsInt("home", "datelayout", 0);
        if (iLayout == 0) radioButton1.Checked = true;
        if (iLayout == 1) radioButton2.Checked = true;
        if (iLayout == 2) radioButton3.Checked = true;
        chkBoxScrolling.Checked = xmlreader.GetValueAsBool("home", "scroll", true);
        chkBoxFixed.Checked = xmlreader.GetValueAsBool("home", "scrollfixed", false);
        useMenus.Checked = xmlreader.GetValueAsBool("home", "usemenus", false);
        useMyPlugins.Checked = xmlreader.GetValueAsBool("home", "usemyplugins", true);
        NoScrollSubs.Checked = xmlreader.GetValueAsBool("home", "noScrollsubs", false);
        BackButtons.Checked = xmlreader.GetValueAsBool("home", "backbuttons", false);
        useTopBarSub.Checked = xmlreader.GetValueAsBool("home", "useTopBarSub", false);
        NoTopBar.Checked = xmlreader.GetValueAsBool("home", "noTopBarSub", false);
        useMenuShortcuts.Checked = xmlreader.GetValueAsBool("home", "useMenuShortcuts", false);
        skinName = xmlreader.GetValueAsString("skin", "name", "BlueTwo");
        OwnDate.Text = xmlreader.GetValueAsString("home", "ownDate", "Day DD. Month");
        ActivateSpecial.Checked = xmlreader.GetValueAsBool("home", "activateSpecial", false);
        if (ActivateSpecial.Checked == true)
        {
          AddSpecial.Enabled = true;
          SpecialFunctions.Enabled = true;
        }
        else
        {
          AddSpecial.Enabled = false;
          SpecialFunctions.Enabled = true;
        }
      }
      treeView.Nodes.Clear();
      if (System.IO.File.Exists(Application.StartupPath + @"\menu2.bin"))
      {
        loadTree(treeView, Application.StartupPath + @"\menu2.bin");
      }
      else
      {
        loadTree(treeView, Application.StartupPath + @"\menu.bin");
      }
    }

    private void UpdateTagInfo(string name)
    {
      try
      {
        if (name.StartsWith("("))
        {
          string strBtnFile = "";
          int l1 = name.IndexOf("{", 0);
          int l2 = name.IndexOf("}", 0);
          string strBtnText = name.Substring(1, name.IndexOf(")", 0) - 1);
          if (l1 > 0)
          {
            strBtnFile = name.Substring(l1 + 1, (l2 - l1) - 1);
            pictureBox1.Image = Image.FromFile(System.IO.Directory.GetCurrentDirectory() + "\\skin\\" + skinName + "\\media\\" + strBtnFile, true);
            groupBox3.Text = strBtnFile;
          }
          label3.Text = "Tag Type";
          textBox1.Text = "Menu Tag";
          label4.Text = "Button Text";
          textBox2.Text = strBtnText;
          label5.Text = "Button Picture";
          textBox3.Text = strBtnFile;
          label6.Text = "";
          textBox4.Text = "";
          label7.Text = "";
          textBox5.Text = "";
          label8.Text = "";
          textBox6.Text = "";
        }
        else
        {
          foreach (ItemTag tag in loadedPlugins)
          {
            if (name == tag.pluginName)
            {
              if (tag.tagType == TagTypes.PLUGIN)
              {
                pictureBox1.Image = null;
                label3.Text = "Tag Type";
                textBox1.Text = "Plugin";
                label4.Text = "Button Text";
                textBox2.Text = tag.buttonText;
                label5.Text = "DLL Name";
                textBox3.Text = tag.DLLName;
                label6.Text = "Plugin Name";
                textBox4.Text = tag.pluginName;
                label7.Text = "Author";
                textBox5.Text = tag.author;
                label8.Text = "Description";
                textBox6.Text = tag.description;
                groupBox3.Text = tag.picture;
                pictureBox1.Image = Image.FromFile(System.IO.Directory.GetCurrentDirectory() + "\\skin\\" + skinName + "\\media\\" + tag.picture, true);
              }
              break;
            }
          }
        }
      }
      catch (Exception)
      {
      }
    }

    private void DeleteItem_Click(object sender, System.EventArgs e)
    {
      TreeNode tn = new TreeNode();
      if (treeView.SelectedNode != null)
      {
        tn = treeView.SelectedNode;
        treeView.Nodes.Remove(tn);
      }
      else
      {
        MessageBox.Show("Please select a Item in the Menu Structure!");
      }
    }

    private void listBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      UpdateTagInfo(listBox.SelectedItem.ToString());
    }

    private void PopulateListBox()
    {
      foreach (ItemTag tag in loadedPlugins)
      {
        listBox.Items.Add(tag.pluginName);
        ListViewItem item = new ListViewItem("Plugin");
        item.SubItems.Add(tag.pluginName);
        item.SubItems.Add(" ");
        item.SubItems.Add(" ");
        item.SubItems.Add("no");
        listView.Items.Add(item);
      }
    }

    private void PopulateListViewBox()
    {
      foreach (ItemTag tag in loadedPlugins)
      {
        if (tag.tagType == TagTypes.MENU_TAG)
        {
          ListViewItem item = new ListViewItem("Menu");
          item.SubItems.Add(tag.pluginName);
          item.SubItems.Add(" ");
          item.SubItems.Add(" ");
          item.SubItems.Add("no");
          listView.Items.Add(item);
        }
      }
    }

    private void EnumeratePluginDirectory(string directory)
    {
      if (Directory.Exists(directory))
      {
        string[] files = Directory.GetFiles(directory, "*.dll");
        foreach (string file in files)
        {
          availablePlugins.Add(file);
        }
      }
    }

    private void EnumeratePlugins()
    {
      EnumeratePluginDirectory(@"plugins\windows");
      EnumeratePluginDirectory(@"plugins\subtitle");
      EnumeratePluginDirectory(@"plugins\tagreaders");
      EnumeratePluginDirectory(@"plugins\externalplayers");
      EnumeratePluginDirectory(@"plugins\process");
    }

    private void LoadPlugins()
    {
      string buttontxt, buttonimage, buttonimagefocus, picture;
      foreach (string pluginFile in availablePlugins)
      {
        try
        {
          Assembly pluginAssembly = Assembly.LoadFrom(pluginFile);
          if (pluginAssembly != null)
          {
            Type[] exportedTypes = pluginAssembly.GetExportedTypes();
            foreach (Type type in exportedTypes)
            {
              // an abstract class cannot be instanciated
              if (type.IsAbstract) continue;
              //
              // Try to locate the interface we're interested in
              //
              if (type.GetInterface("MediaPortal.GUI.Library.ISetupForm") != null)
              {
                if (type.FullName != "GUIHomeMenu.SetupForm" && type.FullName != "home.SetupForm")
                {
                  try
                  {
                    //
                    // Create instance of the current type
                    //
                    object pluginObject = (object)Activator.CreateInstance(type);
                    ISetupForm pluginForm = pluginObject as ISetupForm;

                    if (pluginForm != null)
                    {
                      ItemTag tag = new ItemTag();
                      tag.pluginName = pluginForm.PluginName();
                      tag.FullPath = pluginFile;
                      tag.author = pluginForm.Author();
                      pluginForm.GetHome(out buttontxt, out buttonimage, out buttonimagefocus, out picture);
                      tag.buttonText = buttontxt;
                      tag.picture = picture;
                      tag.description = pluginForm.Description();
                      tag.DLLName = pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1);
                      tag.windowId = pluginForm.GetWindowId();
                      tag.tagType = TagTypes.PLUGIN;
                      if (pluginForm.CanEnable() || pluginForm.DefaultEnabled())
                      {
                        loadedPlugins.Add(tag);
                      }
                    }
                  }
                  catch (Exception setupFormException)
                  {
                    Log.Write("Exception in plugin SetupForm loading :{0}", setupFormException.Message);
                    Log.Write("Current class is :{0}", type.FullName);
                  }
                }
              }
            }
          }
        }
        catch (Exception unknownException)
        {
          Log.Write("Exception in plugin loading :{0}", unknownException.Message);
        }
      }
    }

    #region Load and Save Tree

    private static int saveTree(TreeView tree, string filename)
    {
      ArrayList al = new ArrayList();
      foreach (TreeNode tn in tree.Nodes)
      {
        saveNode(tn, al);
      }
      Stream file = File.Open(filename, FileMode.Create);
      BinaryFormatter bf = new BinaryFormatter();
      try
      {
        bf.Serialize(file, al);
      }
      catch (System.Runtime.Serialization.SerializationException e)
      {
        MessageBox.Show("Serialization failed : {0}", e.Message);
        return -1;
      }
      file.Close();

      return 0;
    }

    private static void saveNode(TreeNode tn, ArrayList al)
    {
      Hashtable ht = new Hashtable();
      ht.Add("Tag", tn.Tag);
      ht.Add("Text", (object)tn.Text);
      ht.Add("FullPath", (object)tn.FullPath);
      ht.Add("SelectedImageIndex", (object)tn.SelectedImageIndex);
      ht.Add("ImageIndex", (object)tn.ImageIndex);
      al.Add((object)ht);

      foreach (TreeNode n in tn.Nodes)
      {
        saveNode(n, al);
      }
    }

    private int loadTree(TreeView tree, string filename)
    {
      if (File.Exists(filename))
      {
        Stream file = File.Open(filename, FileMode.Open);
        BinaryFormatter bf = new BinaryFormatter();
        object obj = null;
        try
        {
          obj = bf.Deserialize(file);
        }
        catch (System.Runtime.Serialization.SerializationException e)
        {
          MessageBox.Show("De-Serialization failed : {0}", e.Message);
          return -1;
        }
        file.Close();

        ArrayList alist = obj as ArrayList;
        foreach (object item in alist)
        {
          Hashtable ht = item as Hashtable;
          string name = ht["Text"].ToString();
          TreeNode tn = new TreeNode(name);

          if (name.StartsWith("("))
          {
            ItemTag tag = new ItemTag();
            tag.pluginName = name;
            tag.tagType = TagTypes.MENU_TAG;
            loadedPlugins.Add(tag);
          }

          tn.Tag = ht["Tag"];
          tn.ImageIndex = Convert.ToInt32(ht["SelectedImageIndex"].ToString());
          tn.SelectedImageIndex = Convert.ToInt32(ht["SelectedImageIndex"].ToString());
          string fPath = ht["FullPath"].ToString();
          string[] parts = fPath.Split(tree.PathSeparator.ToCharArray());
          if (parts.Length > 1)
          {
            TreeNode parentNode = null;
            TreeNodeCollection nodes = tree.Nodes;
            searchNode(parts, ref parentNode, nodes);

            if (parentNode != null)
            {
              parentNode.Nodes.Add(tn);
            }
          }
          else tree.Nodes.Add(tn);
        }
        return 0;
      }
      else
      {
        tree.Nodes.Add("Menu");
        return -2;
      }
    }

    private static void searchNode(string[] parts, ref TreeNode parentNode, TreeNodeCollection nodes)
    {
      foreach (TreeNode n in nodes)
      {
        if (n.Text.Equals(parts[parts.Length - 2].ToString()))
        {
          parentNode = n;
          return;
        }
        else searchNode(parts, ref parentNode, n.Nodes);
      }
    }
    #endregion

    private void AddPicture_Click(object sender, System.EventArgs e)
    {
      TreeNode tn = new TreeNode();
      if (treeView.SelectedNode != null)
      {
        tn = treeView.SelectedNode;
        if (tn.Text.StartsWith("(") || tn.Text.StartsWith("["))
        {
          SearchPicture.Visible = true;
          addPicture = true;
          MakeMenu.Visible = true;
          MakeMenu.Text = "Make Picture";
          label3.Text = "Picture Name";
          textBox1.Text = "";
          textBox2.Text = "";
          textBox3.Text = "";
          textBox4.Text = "";
          textBox5.Text = "";
          textBox6.Text = "";
          label4.Visible = false;
          textBox2.Visible = false;
          label5.Visible = false;
          textBox3.Visible = false;
          label6.Visible = false;
          textBox4.Visible = false;
          label7.Visible = false;
          textBox5.Visible = false;
          label8.Visible = false;
          textBox6.Visible = false;
        }
        else
        {
          MessageBox.Show("Pictures can only attach to a Menu Item");
        }
      }
      else
      {
        MessageBox.Show("Please select a Menu Item in the Menu Structure!");
      }
    }

    private void SearchPicture_Click(object sender, System.EventArgs e)
    {
      openFileDialog1.RestoreDirectory = true;
      openFileDialog1.DefaultExt = ".png";
      openFileDialog1.FileName = "hover*";
      openFileDialog1.InitialDirectory = System.IO.Directory.GetCurrentDirectory() + "\\skin\\" + skinName + "\\media";
      if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        string appName = openFileDialog1.FileName;
        pictureBox1.Image = Image.FromFile(appName, true);
        appName = System.IO.Path.GetFileName(appName);
        textBox1.Text = appName;
      }
    }

    private void deletePicture_Click(object sender, System.EventArgs e)
    {
      TreeNode tn = new TreeNode();
      if (treeView.SelectedNode != null)
      {
        tn = treeView.SelectedNode;
        if (tn.Text.IndexOf("{", 0) > 0)
        {
          int l = tn.Text.IndexOf(" {", 0);
          tn.Text = tn.Text.Substring(0, l);
        }
      }
      else
      {
        MessageBox.Show("Please select a Menu Item in the Menu Structure!");
      }
    }

    private void addConfig_Click(object sender, System.EventArgs e)
    {
      TreeNode tn = new TreeNode();
      if (treeView.SelectedNode != null)
      {
        tn = treeView.SelectedNode;
        if (tn.Text.StartsWith("("))
        {
          MessageBox.Show("You can attach configurations only to a Plugin Item");
        }
        else
        {
          foreach (ItemTag tag in loadedPlugins)
          {
            if (tag.pluginName == tn.Text)
            {
              try
              {
                Assembly pluginAssembly = Assembly.LoadFrom(tag.FullPath);
                if (pluginAssembly != null)
                {
                  Type[] exportedTypes = pluginAssembly.GetExportedTypes();
                  foreach (Type type in exportedTypes)
                  {
                    // an abstract class cannot be instanciated
                    if (type.IsAbstract) continue;
                    //
                    // Try to locate the interface we're interested in
                    //
                    if (type.GetInterface("MediaPortal.GUI.Library.ISetupForm") != null)
                    {
                      if (type.FullName != "GUIHomeMenu.SetupForm" && type.FullName != "home.SetupForm")
                      {
                        try
                        {
                          //
                          // Create instance of the current type
                          //
                          object pluginObject = (object)Activator.CreateInstance(type);
                          ISetupForm pluginForm = pluginObject as ISetupForm;

                          if (pluginForm != null)
                          {
                            if (pluginForm.PluginName() == tn.Text)
                            {
                              if (pluginForm.CanEnable() || pluginForm.DefaultEnabled())
                              {
                                if (pluginForm.HasSetup() == false)
                                {
                                  MessageBox.Show("Plugin has no Setup!");
                                }
                                else
                                {
                                  pluginForm.ShowPlugin();
                                }
                              }
                            }
                          }
                        }
                        catch (Exception setupFormException)
                        {
                          Log.Write("Exception in plugin SetupForm loading :{0}", setupFormException.Message);
                          Log.Write("Current class is :{0}", type.FullName);
                        }
                      }
                    }
                  }
                }
              }
              catch (Exception unknownException)
              {
                Log.Write("Exception in plugin loading :{0}", unknownException.Message);
              }

            }
          }
        }
      }
      else
      {
        MessageBox.Show("Please select a Menu Item in the Menu Structure!");
      }
    }

    #region Drag and Drop TreeView

    private void treeView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      this.treeView.SelectedNode = this.treeView.GetNodeAt(e.X, e.Y);
    }
    private void treeView_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
    {
      DoDragDrop(e.Item, DragDropEffects.Move);
    }

    private void treeView_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
    {
      e.Effect = DragDropEffects.Move;
    }
    private void treeView_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
    {
      if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false) && this.NodeMap != "")
      {
        TreeNode MovingNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
        string[] NodeIndexes = this.NodeMap.Split('|');
        TreeNodeCollection InsertCollection = this.treeView.Nodes;
        for (int i = 0; i < NodeIndexes.Length - 1; i++)
        {
          InsertCollection = InsertCollection[Int32.Parse(NodeIndexes[i])].Nodes;
        }

        if (InsertCollection != null)
        {
          InsertCollection.Insert(Int32.Parse(NodeIndexes[NodeIndexes.Length - 1]), (TreeNode)MovingNode.Clone());
          this.treeView.SelectedNode = InsertCollection[Int32.Parse(NodeIndexes[NodeIndexes.Length - 1])];
          MovingNode.Remove();
        }
      }
    }

    private void treeView_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
    {
      TreeNode NodeOver = this.treeView.GetNodeAt(this.treeView.PointToClient(Cursor.Position));
      TreeNode NodeMoving = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
      if (NodeOver != null && (NodeOver != NodeMoving || (NodeOver.Parent != null && NodeOver.Index == (NodeOver.Parent.Nodes.Count - 1))))
      {
        int OffsetY = this.treeView.PointToClient(Cursor.Position).Y - NodeOver.Bounds.Top;
        int NodeOverImageWidth = this.treeView.ImageList.Images[NodeOver.ImageIndex].Size.Width + 8;
        Graphics g = this.treeView.CreateGraphics();

        if (NodeOver.ImageIndex == 1)
        {
          if (OffsetY < (NodeOver.Bounds.Height / 2))
          {
            TreeNode tnParadox = NodeOver;
            while (tnParadox.Parent != null)
            {
              if (tnParadox.Parent == NodeMoving)
              {
                this.NodeMap = "";
                return;
              }

              tnParadox = tnParadox.Parent;
            }
            TreeNode tnPlaceholderInfo = NodeOver;
            string NewNodeMap = ((int)NodeOver.Index).ToString();
            while (tnPlaceholderInfo.Parent != null)
            {
              tnPlaceholderInfo = tnPlaceholderInfo.Parent;
              NewNodeMap = tnPlaceholderInfo.Index + "|" + NewNodeMap;
            }
            if (NewNodeMap == this.NodeMap)
              return;
            else
              this.NodeMap = NewNodeMap;
            this.Refresh();
            int LeftPos, RightPos;
            LeftPos = NodeOver.Bounds.Left - NodeOverImageWidth;
            RightPos = this.treeView.Width - 4;

            Point[] LeftTriangle = new Point[5]{
																								 new Point(LeftPos, NodeOver.Bounds.Top - 4),
																								 new Point(LeftPos, NodeOver.Bounds.Top + 4),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Y),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Top - 1),
																								 new Point(LeftPos, NodeOver.Bounds.Top - 5)};

            Point[] RightTriangle = new Point[5]{
																									new Point(RightPos, NodeOver.Bounds.Top - 4),
																									new Point(RightPos, NodeOver.Bounds.Top + 4),
																									new Point(RightPos - 4, NodeOver.Bounds.Y),
																									new Point(RightPos - 4, NodeOver.Bounds.Top - 1),
																									new Point(RightPos, NodeOver.Bounds.Top - 5)};


            g.FillPolygon(System.Drawing.Brushes.Black, LeftTriangle);
            g.FillPolygon(System.Drawing.Brushes.Black, RightTriangle);
            g.DrawLine(new System.Drawing.Pen(Color.Black, 2), new Point(LeftPos, NodeOver.Bounds.Top), new Point(RightPos, NodeOver.Bounds.Top));
          }
          else
          {
            TreeNode tnParadox = NodeOver;
            while (tnParadox.Parent != null)
            {
              if (tnParadox.Parent == NodeMoving)
              {
                this.NodeMap = "";
                return;
              }

              tnParadox = tnParadox.Parent;
            }
            TreeNode ParentDragDrop = null;
            if (NodeOver.Parent != null && NodeOver.Index == (NodeOver.Parent.Nodes.Count - 1))
            {
              int XPos = this.treeView.PointToClient(Cursor.Position).X;
              if (XPos < NodeOver.Bounds.Left)
              {
                ParentDragDrop = NodeOver.Parent;
                while (true)
                {
                  if (XPos > (ParentDragDrop.Bounds.Left - this.treeView.ImageList.Images[ParentDragDrop.ImageIndex].Size.Width))
                    break;

                  if (ParentDragDrop.Parent != null)
                    ParentDragDrop = ParentDragDrop.Parent;
                  else
                    break;
                }
              }
            }
            TreeNode tnPlaceholderInfo;
            if (ParentDragDrop != null)
              tnPlaceholderInfo = ParentDragDrop;
            else
              tnPlaceholderInfo = NodeOver;

            string NewNodeMap = ((int)tnPlaceholderInfo.Index + 1).ToString();
            while (tnPlaceholderInfo.Parent != null)
            {
              tnPlaceholderInfo = tnPlaceholderInfo.Parent;
              NewNodeMap = tnPlaceholderInfo.Index + "|" + NewNodeMap;
            }
            if (NewNodeMap == this.NodeMap)
              return;
            else
              this.NodeMap = NewNodeMap;
            this.Refresh();
            int LeftPos, RightPos;
            if (ParentDragDrop != null)
              LeftPos = ParentDragDrop.Bounds.Left - (this.treeView.ImageList.Images[ParentDragDrop.ImageIndex].Size.Width + 8);
            else
              LeftPos = NodeOver.Bounds.Left - NodeOverImageWidth;
            RightPos = this.treeView.Width - 4;

            Point[] LeftTriangle = new Point[5]{
																								 new Point(LeftPos, NodeOver.Bounds.Bottom - 4),
																								 new Point(LeftPos, NodeOver.Bounds.Bottom + 4),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Bottom),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Bottom - 1),
																								 new Point(LeftPos, NodeOver.Bounds.Bottom - 5)};

            Point[] RightTriangle = new Point[5]{
																									new Point(RightPos, NodeOver.Bounds.Bottom - 4),
																									new Point(RightPos, NodeOver.Bounds.Bottom + 4),
																									new Point(RightPos - 4, NodeOver.Bounds.Bottom),
																									new Point(RightPos - 4, NodeOver.Bounds.Bottom - 1),
																									new Point(RightPos, NodeOver.Bounds.Bottom - 5)};


            g.FillPolygon(System.Drawing.Brushes.Black, LeftTriangle);
            g.FillPolygon(System.Drawing.Brushes.Black, RightTriangle);
            g.DrawLine(new System.Drawing.Pen(Color.Black, 2), new Point(LeftPos, NodeOver.Bounds.Bottom), new Point(RightPos, NodeOver.Bounds.Bottom));
          }
        }
        else
        {
          if (OffsetY < (NodeOver.Bounds.Height / 3))
          {
            TreeNode tnParadox = NodeOver;
            while (tnParadox.Parent != null)
            {
              if (tnParadox.Parent == NodeMoving)
              {
                this.NodeMap = "";
                return;
              }

              tnParadox = tnParadox.Parent;
            }
            TreeNode tnPlaceholderInfo = NodeOver;
            string NewNodeMap = ((int)NodeOver.Index).ToString();
            while (tnPlaceholderInfo.Parent != null)
            {
              tnPlaceholderInfo = tnPlaceholderInfo.Parent;
              NewNodeMap = tnPlaceholderInfo.Index + "|" + NewNodeMap;
            }
            if (NewNodeMap == this.NodeMap)
              return;
            else
              this.NodeMap = NewNodeMap;
            this.Refresh();
            int LeftPos, RightPos;
            LeftPos = NodeOver.Bounds.Left - NodeOverImageWidth;
            RightPos = this.treeView.Width - 4;

            Point[] LeftTriangle = new Point[5]{
																								 new Point(LeftPos, NodeOver.Bounds.Top - 4),
																								 new Point(LeftPos, NodeOver.Bounds.Top + 4),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Y),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Top - 1),
																								 new Point(LeftPos, NodeOver.Bounds.Top - 5)};

            Point[] RightTriangle = new Point[5]{
																									new Point(RightPos, NodeOver.Bounds.Top - 4),
																									new Point(RightPos, NodeOver.Bounds.Top + 4),
																									new Point(RightPos - 4, NodeOver.Bounds.Y),
																									new Point(RightPos - 4, NodeOver.Bounds.Top - 1),
																									new Point(RightPos, NodeOver.Bounds.Top - 5)};


            g.FillPolygon(System.Drawing.Brushes.Black, LeftTriangle);
            g.FillPolygon(System.Drawing.Brushes.Black, RightTriangle);
            g.DrawLine(new System.Drawing.Pen(Color.Black, 2), new Point(LeftPos, NodeOver.Bounds.Top), new Point(RightPos, NodeOver.Bounds.Top));
          }
          else if ((NodeOver.Parent != null && NodeOver.Index == 0) && (OffsetY > (NodeOver.Bounds.Height - (NodeOver.Bounds.Height / 3))))
          {
            TreeNode tnParadox = NodeOver;
            while (tnParadox.Parent != null)
            {
              if (tnParadox.Parent == NodeMoving)
              {
                this.NodeMap = "";
                return;
              }

              tnParadox = tnParadox.Parent;
            }
            TreeNode tnPlaceholderInfo = NodeOver;
            string NewNodeMap = ((int)NodeOver.Index + 1).ToString();
            while (tnPlaceholderInfo.Parent != null)
            {
              tnPlaceholderInfo = tnPlaceholderInfo.Parent;
              NewNodeMap = tnPlaceholderInfo.Index + "|" + NewNodeMap;
            }
            if (NewNodeMap == this.NodeMap)
              return;
            else
              this.NodeMap = NewNodeMap;
            this.Refresh();
            int LeftPos, RightPos;
            LeftPos = NodeOver.Bounds.Left - NodeOverImageWidth;
            RightPos = this.treeView.Width - 4;

            Point[] LeftTriangle = new Point[5]{
																								 new Point(LeftPos, NodeOver.Bounds.Bottom - 4),
																								 new Point(LeftPos, NodeOver.Bounds.Bottom + 4),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Bottom),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Bottom - 1),
																								 new Point(LeftPos, NodeOver.Bounds.Bottom - 5)};

            Point[] RightTriangle = new Point[5]{
																									new Point(RightPos, NodeOver.Bounds.Bottom - 4),
																									new Point(RightPos, NodeOver.Bounds.Bottom + 4),
																									new Point(RightPos - 4, NodeOver.Bounds.Bottom),
																									new Point(RightPos - 4, NodeOver.Bounds.Bottom - 1),
																									new Point(RightPos, NodeOver.Bounds.Bottom - 5)};


            g.FillPolygon(System.Drawing.Brushes.Black, LeftTriangle);
            g.FillPolygon(System.Drawing.Brushes.Black, RightTriangle);
            g.DrawLine(new System.Drawing.Pen(Color.Black, 2), new Point(LeftPos, NodeOver.Bounds.Bottom), new Point(RightPos, NodeOver.Bounds.Bottom));
          }
          else
          {
            if (NodeOver.Nodes.Count > 0)
            {
              NodeOver.Expand();
            }
            else
            {
              if (NodeMoving == NodeOver)
                return;
              TreeNode tnParadox = NodeOver;
              while (tnParadox.Parent != null)
              {
                if (tnParadox.Parent == NodeMoving)
                {
                  this.NodeMap = "";
                  return;
                }
                tnParadox = tnParadox.Parent;
              }
              TreeNode tnPlaceholderInfo = NodeOver;
              string NewNodeMap = ((int)NodeOver.Index).ToString();
              while (tnPlaceholderInfo.Parent != null)
              {
                tnPlaceholderInfo = tnPlaceholderInfo.Parent;
                NewNodeMap = tnPlaceholderInfo.Index + "|" + NewNodeMap;
              }
              NewNodeMap = NewNodeMap + "|0";
              if (NewNodeMap == this.NodeMap)
                return;
              else
                this.NodeMap = NewNodeMap;
              this.Refresh();
              int RightPos = NodeOver.Bounds.Right + 6;
              Point[] RightTriangle = new Point[5]{
																										new Point(RightPos, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2) + 4),
																										new Point(RightPos, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2) + 4),
																										new Point(RightPos - 4, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2)),
																										new Point(RightPos - 4, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2) - 1),
																										new Point(RightPos, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2) - 5)};

              this.Refresh();
              g.FillPolygon(System.Drawing.Brushes.Black, RightTriangle);
            }
          }
        }
      }
    }
    #endregion

    private void radioButton3_CheckedChanged(object sender, System.EventArgs e)
    {
      OwnDate.Enabled = true;
    }

    private void radioButton2_CheckedChanged(object sender, System.EventArgs e)
    {
      OwnDate.Enabled = false;
    }

    private void radioButton1_CheckedChanged(object sender, System.EventArgs e)
    {
      OwnDate.Enabled = false;
    }

    protected string ConvertOwnDate(string own, string day, string month)
    {
      StringBuilder cown = new StringBuilder(own);
      string s;

      DateTime cur = DateTime.Now;
      s = cown.ToString();
      s = s.ToUpper();
      int inx = s.IndexOf("MM", 0);
      if (inx >= 0)
      {
        cown.Remove(inx, 2);
        cown.Insert(inx, cur.Month.ToString());
      }
      s = cown.ToString();
      s = s.ToUpper();
      inx = s.IndexOf("DD", 0);
      if (inx >= 0)
      {
        cown.Remove(inx, 2);
        cown.Insert(inx, cur.Day.ToString());
      }
      s = cown.ToString();
      s = s.ToUpper();
      inx = s.IndexOf("MONTH", 0);
      if (inx >= 0)
      {
        cown.Remove(inx, 5);
        cown.Insert(inx, month);
      }
      s = cown.ToString();
      s = s.ToUpper();
      inx = s.IndexOf("DAY", 0);
      if (inx >= 0)
      {
        cown.Remove(inx, 3);
        cown.Insert(inx, day);
      }
      s = cown.ToString();
      s = s.ToUpper();
      inx = s.IndexOf("YY", 0);
      if (inx >= 0)
      {
        cown.Remove(inx, 2);
        int sy = cur.Year - 2000;
        if (sy < 10)
        {
          cown.Insert(inx, "0" + sy.ToString());
        }
        else
        {
          cown.Insert(inx, sy.ToString());
        }
      }
      s = cown.ToString();
      s = s.ToUpper();
      inx = s.IndexOf("YEAR", 0);
      if (inx >= 0)
      {
        cown.Remove(inx, 4);
        cown.Insert(inx, cur.Year.ToString());
      }
      return (cown.ToString());
    }

    protected string GetDate()
    {
      string strDate = "";
      DateTime cur = DateTime.Now;
      string day;
      switch (cur.DayOfWeek)
      {
        case DayOfWeek.Monday: day = "Monday"; break;
        case DayOfWeek.Tuesday: day = "Tuesday"; break;
        case DayOfWeek.Wednesday: day = "Wednesday"; break;
        case DayOfWeek.Thursday: day = "Thursday"; break;
        case DayOfWeek.Friday: day = "Friday"; break;
        case DayOfWeek.Saturday: day = "Saturday"; break;
        default: day = "Sunday"; break;
      }

      string month;
      switch (cur.Month)
      {
        case 1: month = "January"; break;
        case 2: month = "February"; break;
        case 3: month = "March"; break;
        case 4: month = "April"; break;
        case 5: month = "May"; break;
        case 6: month = "June"; break;
        case 7: month = "July"; break;
        case 8: month = "August"; break;
        case 9: month = "September"; break;
        case 10: month = "October"; break;
        case 11: month = "November"; break;
        default: month = "December"; break;
      }

      if (radioButton1.Checked == true)
      {
        strDate = String.Format("{0} {1}. {2}", day, cur.Day, month);
      }
      if (radioButton2.Checked == true)
      {
        strDate = String.Format("{0} {1} {2}", day, month, cur.Day);
      }
      if (radioButton3.Checked == true)
      {
        strDate = ConvertOwnDate(OwnDate.Text, day, month);
      }
      return strDate;
    }

    private void TestDate_Click(object sender, System.EventArgs e)
    {
      DateTest.Text = GetDate();
    }

    private void NoTopBar_CheckedChanged(object sender, System.EventArgs e)
    {
      useTopBarSub.Checked = false;
    }

    private void useTopBarSub_CheckedChanged(object sender, System.EventArgs e)
    {
      NoTopBar.Checked = false;
    }

    private void ActivateSpecial_CheckedChanged(object sender, System.EventArgs e)
    {
      if (ActivateSpecial.Checked == true)
      {
        AddSpecial.Enabled = true;
        SpecialFunctions.Enabled = true;
        string scriptdir = System.IO.Directory.GetCurrentDirectory() + "\\" + "scripts";
        if (!Directory.Exists(scriptdir))
        {
          SpecialFunctions.Items.Clear();
          AddSpecial.Enabled = false;
          SpecialFunctions.Enabled = true;
          ActivateSpecial.Checked = false;
        }
        else
        {
          DirectoryInfo scDir = new DirectoryInfo(scriptdir);
          foreach (FileInfo fi in scDir.GetFiles())
          {
            Log.Write(fi.Name + "  " + fi.Extension);
            if (fi.Extension.ToLower() == ".mps")
            {
              string fl = fi.Name;
              fl = fl.Substring(0, fl.Length - 4);
              SpecialFunctions.Items.Add(fl);
              comboBox1.Items.Add(fl);
              comboBox2.Items.Add(fl);
              comboBox3.Items.Add(fl);
            }
          }
        }
      }
      else
      {
        SpecialFunctions.Items.Clear();
        AddSpecial.Enabled = false;
        SpecialFunctions.Enabled = true;
      }
    }

    private void AddSpecial_Click(object sender, System.EventArgs e)
    {
      addScript = true;
      SearchPicture.Visible = false;
      MakeMenu.Visible = true;
      MakeMenu.Text = "Add Script";
      label3.Text = "Script Name";
      textBox1.Visible = false;
      label4.Text = "";
      label4.Visible = false;
      textBox2.Visible = true;
      comboBox1.Visible = true;
      comboBox1.Enabled = true;
      textBox1.Text = "";
      textBox2.Text = "";
      textBox3.Text = "";
      textBox4.Text = "";
      textBox5.Text = "";
      textBox6.Text = "";
      label5.Visible = false;
      textBox3.Visible = false;
      label6.Visible = false;
      textBox4.Visible = false;
      label7.Visible = false;
      textBox5.Visible = false;
      label8.Visible = false;
      textBox6.Visible = false;
      comboBox1.Focus();
    }

    private void SetupForm_Load(object sender, System.EventArgs e)
    {
      this.NodeCount = 0;
      this.FolderCount = 0;

      ImageList TreeviewIL = new ImageList();
      TreeviewIL.Images.Add(System.Drawing.Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("WindowPlugins.Home.resources.folder.png")));
      TreeviewIL.Images.Add(System.Drawing.Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("WindowPlugins.Home.resources.node.png")));
      TreeviewIL.Images.Add(System.Drawing.Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("WindowPlugins.Home.resources.script.png")));

      this.treeView.ImageList = TreeviewIL;
      this.treeView.HideSelection = false;
      this.treeView.ItemHeight = this.treeView.ItemHeight + 3;
      this.treeView.Indent = this.treeView.Indent + 3;

      availablePlugins.Clear();
      loadedPlugins.Clear();
      listBox.Items.Clear();
      listView.Items.Clear();

      EnumeratePlugins();
      LoadPlugins();
      PopulateListBox();
      LoadSettings();
      PopulateListViewBox();
      MakeMenu.Visible = false;

      textBox1.Text = "";
      textBox2.Text = "";
      textBox3.Text = "";
      textBox4.Text = "";
      textBox5.Text = "";
      textBox6.Text = "";

      this.treeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseDown);
      this.treeView.DragOver += new System.Windows.Forms.DragEventHandler(this.treeView_DragOver);
      this.treeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeView_DragEnter);
      this.treeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_ItemDrag);
      this.treeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView_DragDrop);
    }
  }
}
