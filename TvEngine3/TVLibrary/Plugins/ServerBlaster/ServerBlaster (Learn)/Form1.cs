#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using MediaPortal.Devices;
using MediaPortal.GUI.Library;
using XPListview;
using System.Threading;

namespace BlasterTest
{
  /// <summary>
  /// Summary description for Form1.
  /// </summary>
  public class Form1 : System.Windows.Forms.Form
  {
    #region Form members

    #endregion Form members

    private System.Windows.Forms.Button buttonStart;
    private System.Windows.Forms.Button buttonTest;
    private System.Windows.Forms.Timer timerLearn;
    private System.Windows.Forms.PropertyGrid propertyGrid;
    private System.Windows.Forms.ImageList imageList;
    private System.Windows.Forms.ColumnHeader columnButton;
    private XPListView listButtons;
    private System.ComponentModel.IContainer components;

    public Form1()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

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
      this.components = new System.ComponentModel.Container();
      this.imageList = new System.Windows.Forms.ImageList(this.components);
      this.buttonStart = new System.Windows.Forms.Button();
      this.buttonTest = new System.Windows.Forms.Button();
      this.timerLearn = new System.Windows.Forms.Timer(this.components);
      this.propertyGrid = new System.Windows.Forms.PropertyGrid();
      this.listButtons = new XPListview.XPListView();
      this.columnButton = new System.Windows.Forms.ColumnHeader();
      this.SuspendLayout();
      // 
      // imageList
      // 
      this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
      this.imageList.ImageSize = new System.Drawing.Size(16, 16);
      this.imageList.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // buttonStart
      // 
      this.buttonStart.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonStart.Location = new System.Drawing.Point(8, 392);
      this.buttonStart.Name = "buttonStart";
      this.buttonStart.Size = new System.Drawing.Size(75, 23);
      this.buttonStart.TabIndex = 1;
      this.buttonStart.Text = "&Learn";
      this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
      // 
      // buttonTest
      // 
      this.buttonTest.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonTest.Location = new System.Drawing.Point(144, 392);
      this.buttonTest.Name = "buttonTest";
      this.buttonTest.Size = new System.Drawing.Size(75, 23);
      this.buttonTest.TabIndex = 6;
      this.buttonTest.Text = "&Test";
      this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
      // 
      // timerLearn
      // 
      this.timerLearn.Interval = 1000;
      this.timerLearn.Tick += new System.EventHandler(this.timerLearn_Tick);
      // 
      // propertyGrid
      // 
      this.propertyGrid.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.propertyGrid.LineColor = System.Drawing.SystemColors.ScrollBar;
      this.propertyGrid.Location = new System.Drawing.Point(224, 8);
      this.propertyGrid.Name = "propertyGrid";
      this.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
      this.propertyGrid.Size = new System.Drawing.Size(368, 408);
      this.propertyGrid.TabIndex = 9;
      this.propertyGrid.ToolbarVisible = false;
      this.propertyGrid.PropertyValueChanged +=
        new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
      // 
      // listButtons
      // 
      this.listButtons.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.listButtons.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                          {
                                            this.columnButton
                                          });
      this.listButtons.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.listButtons.HideSelection = false;
      this.listButtons.LabelEdit = true;
      this.listButtons.Location = new System.Drawing.Point(8, 8);
      this.listButtons.MultiSelect = false;
      this.listButtons.Name = "listButtons";
      this.listButtons.Size = new System.Drawing.Size(208, 376);
      this.listButtons.SmallImageList = this.imageList;
      this.listButtons.TabIndex = 0;
      this.listButtons.UseCompatibleStateImageBehavior = false;
      this.listButtons.View = System.Windows.Forms.View.Details;
      this.listButtons.DoubleClick += new System.EventHandler(this.listButtons_DoubleClick);
      this.listButtons.SelectedIndexChanged += new System.EventHandler(this.listButtons_SelectedIndexChanged);
      // 
      // columnButton
      // 
      this.columnButton.Text = "Button";
      this.columnButton.Width = 175;
      // 
      // Form1
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(600, 421);
      this.Controls.Add(this.listButtons);
      this.Controls.Add(this.propertyGrid);
      this.Controls.Add(this.buttonTest);
      this.Controls.Add(this.buttonStart);
      this.Name = "Form1";
      this.Text = "ServerBlaster Learn";
      this.Load += new System.EventHandler(this.Form1_Load);
      this.ResumeLayout(false);
    }

    #endregion

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      Thread.CurrentThread.Name = "BlasterTest";
      Form1 form = new Form1();

      foreach (string arg in args)
      {
        switch (arg.ToLowerInvariant())
        {
          case "/dump":
          case "-dump":
            Device.Dump = true;
            break;
          case "/gumbo":
          case "-gumbo":
            form._gumboTest = true;
            break;
          default:
            break;
        }
      }

      Application.EnableVisualStyles();
      Application.DoEvents();
      Application.Run(form);
    }

    private void Form1_Load(object sender, System.EventArgs e)
    {
      Blaster.DeviceArrival += new DeviceEventHandler(OnDeviceArrival);
      Blaster.DeviceRemoval += new DeviceEventHandler(OnDeviceRemoval);

      LoadConfig();

      if (listButtons.Items.Count == 0)
      {
        listButtons.ShowInGroups = true;
        listButtons.Groups.Add("My Set-top box", 0);
        listButtons.Groups.Add("My Set-top box (2)", 1);

        listButtons.Items.Add("0", 0, 0);
        listButtons.Items.Add("1", 0, 0);
        listButtons.Items.Add("2", 0, 0);
        listButtons.Items.Add("3", 0, 0);
        listButtons.Items.Add("4", 0, 0);
        listButtons.Items.Add("5", 0, 0);
        listButtons.Items.Add("6", 0, 0);
        listButtons.Items.Add("7", 0, 0);
        listButtons.Items.Add("8", 0, 0);
        listButtons.Items.Add("9", 0, 0);
        listButtons.Items.Add("Select", 0, 0);

        listButtons.Items.Add("0", 0, 1);
        listButtons.Items.Add("1", 0, 1);
        listButtons.Items.Add("2", 0, 1);
        listButtons.Items.Add("3", 0, 1);
        listButtons.Items.Add("4", 0, 1);
        listButtons.Items.Add("5", 0, 1);
        listButtons.Items.Add("6", 0, 1);
        listButtons.Items.Add("7", 0, 1);
        listButtons.Items.Add("8", 0, 1);
        listButtons.Items.Add("9", 0, 1);
        listButtons.Items.Add("Select", 0, 1);

        foreach (XPListViewItem item in listButtons.Items) item.Tag = new BlasterCommand(item.Text);
        foreach (XPListViewItem item in listButtons.Items)
          if (item.GroupIndex == 1) ((BlasterCommand)item.Tag).Port = Port.Two;
      }

      listButtons.Items[0].Selected = true;
      listButtons.Focus();

      // pre-allocate the sub items
      foreach (XPListViewItem item in listButtons.Items) item.SubItems.Add("");

      if (_gumboTest)
      {
        listButtons.Items[0].Tag = new BlasterCommand("0",
                                                      new byte[]
                                                        {
                                                          0x9E, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05, 0x84,
                                                          0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84, 0x05, 0x84,
                                                          0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83, 0x06, 0x83,
                                                          0x05, 0x9E, 0x84, 0x05, 0x84, 0x05, 0x84, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x16, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05,
                                                          0x84, 0x08, 0x9E, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84,
                                                          0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83,
                                                          0x06, 0x83, 0x05, 0x84, 0x05, 0x84, 0x05, 0x84, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x9E, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x16, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C,
                                                          0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05,
                                                          0x84, 0x05, 0x84, 0x05, 0x9E, 0x84, 0x0C, 0x83, 0x09, 0x83,
                                                          0x0D, 0x83, 0x06, 0x83, 0x05, 0x84, 0x05, 0x84, 0x05, 0x84,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x16, 0x9E, 0x88, 0x06, 0x83, 0x06,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D,
                                                          0x83, 0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x09,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x05, 0x94, 0x83, 0x06, 0x83,
                                                          0x06, 0x83, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x10, 0x80
                                                        });
        listButtons.Items[1].Tag = new BlasterCommand("1",
                                                      new byte[]
                                                        {
                                                          0x9E, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05, 0x84,
                                                          0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84, 0x05, 0x84,
                                                          0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83, 0x06, 0x83,
                                                          0x05, 0x9E, 0x84, 0x05, 0x84, 0x09, 0x83, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x13, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05,
                                                          0x84, 0x08, 0x9E, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84,
                                                          0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83,
                                                          0x06, 0x83, 0x05, 0x84, 0x05, 0x84, 0x09, 0x83, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x9E, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x13, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C,
                                                          0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05,
                                                          0x84, 0x05, 0x84, 0x05, 0x9E, 0x84, 0x0C, 0x83, 0x09, 0x83,
                                                          0x0D, 0x83, 0x06, 0x83, 0x05, 0x84, 0x05, 0x84, 0x09, 0x83,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x13, 0x9E, 0x88, 0x06, 0x83, 0x06,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D,
                                                          0x83, 0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x09,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x05, 0x94, 0x83, 0x06, 0x83,
                                                          0x09, 0x84, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x0C, 0x80
                                                        });
        listButtons.Items[2].Tag = new BlasterCommand("2",
                                                      new byte[]
                                                        {
                                                          0x9E, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05, 0x84,
                                                          0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84, 0x05, 0x84,
                                                          0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83, 0x06, 0x83,
                                                          0x05, 0x9E, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x10, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05,
                                                          0x84, 0x08, 0x9E, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84,
                                                          0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83,
                                                          0x06, 0x83, 0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x9E, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x10, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C,
                                                          0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05,
                                                          0x84, 0x05, 0x84, 0x05, 0x9E, 0x84, 0x0C, 0x83, 0x09, 0x83,
                                                          0x0D, 0x83, 0x06, 0x83, 0x05, 0x84, 0x05, 0x84, 0x0C, 0x83,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x10, 0x9E, 0x88, 0x06, 0x83, 0x06,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D,
                                                          0x83, 0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x09,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x05, 0x94, 0x83, 0x06, 0x83,
                                                          0x0D, 0x83, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x09, 0x80
                                                        });
        listButtons.Items[3].Tag = new BlasterCommand("3",
                                                      new byte[]
                                                        {
                                                          0x9E, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05, 0x84,
                                                          0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84, 0x05, 0x84,
                                                          0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83, 0x06, 0x83,
                                                          0x05, 0x9E, 0x84, 0x05, 0x84, 0x0F, 0x84, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x0C, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05,
                                                          0x84, 0x08, 0x9E, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84,
                                                          0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83,
                                                          0x06, 0x83, 0x05, 0x84, 0x05, 0x84, 0x0F, 0x84, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x9E, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x0C, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C,
                                                          0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05,
                                                          0x84, 0x05, 0x84, 0x05, 0x9E, 0x84, 0x0C, 0x83, 0x09, 0x83,
                                                          0x0D, 0x83, 0x06, 0x83, 0x05, 0x84, 0x05, 0x84, 0x0F, 0x84,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x0C, 0x9E, 0x88, 0x06, 0x83, 0x06,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D,
                                                          0x83, 0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x09,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x05, 0x94, 0x83, 0x06, 0x83,
                                                          0x10, 0x83, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x06, 0x80
                                                        });
        listButtons.Items[4].Tag = new BlasterCommand("4",
                                                      new byte[]
                                                        {
                                                          0x9E, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05, 0x84,
                                                          0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84, 0x05, 0x84,
                                                          0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83, 0x06, 0x83,
                                                          0x05, 0x9E, 0x84, 0x09, 0x83, 0x06, 0x83, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x13, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05,
                                                          0x84, 0x08, 0x9E, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84,
                                                          0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83,
                                                          0x06, 0x83, 0x05, 0x84, 0x09, 0x83, 0x06, 0x83, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x9E, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x13, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C,
                                                          0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05,
                                                          0x84, 0x05, 0x84, 0x05, 0x9E, 0x84, 0x0C, 0x83, 0x09, 0x83,
                                                          0x0D, 0x83, 0x06, 0x83, 0x05, 0x84, 0x09, 0x83, 0x06, 0x83,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x13, 0x9E, 0x88, 0x06, 0x83, 0x06,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D,
                                                          0x83, 0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x09,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x05, 0x94, 0x83, 0x09, 0x84,
                                                          0x05, 0x84, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x0C, 0x80
                                                        });
        listButtons.Items[5].Tag = new BlasterCommand("5",
                                                      new byte[]
                                                        {
                                                          0x9E, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05, 0x84,
                                                          0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84, 0x05, 0x84,
                                                          0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83, 0x06, 0x83,
                                                          0x05, 0x9E, 0x84, 0x09, 0x83, 0x09, 0x83, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x10, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05,
                                                          0x84, 0x08, 0x9E, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84,
                                                          0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83,
                                                          0x06, 0x83, 0x05, 0x84, 0x09, 0x83, 0x09, 0x83, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x9E, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x10, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C,
                                                          0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05,
                                                          0x84, 0x05, 0x84, 0x05, 0x9E, 0x84, 0x0C, 0x83, 0x09, 0x83,
                                                          0x0D, 0x83, 0x06, 0x83, 0x05, 0x84, 0x09, 0x83, 0x09, 0x83,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x10, 0x9E, 0x88, 0x06, 0x83, 0x06,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D,
                                                          0x83, 0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x09,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x05, 0x94, 0x83, 0x09, 0x84,
                                                          0x09, 0x83, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x09, 0x80
                                                        });
        listButtons.Items[6].Tag = new BlasterCommand("6",
                                                      new byte[]
                                                        {
                                                          0x9E, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05, 0x84,
                                                          0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84, 0x05, 0x84,
                                                          0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83, 0x06, 0x83,
                                                          0x05, 0x9E, 0x84, 0x09, 0x83, 0x0C, 0x84, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x0C, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05,
                                                          0x84, 0x08, 0x9E, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84,
                                                          0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83,
                                                          0x06, 0x83, 0x05, 0x84, 0x09, 0x83, 0x0C, 0x84, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x9E, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x0C, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C,
                                                          0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05,
                                                          0x84, 0x05, 0x84, 0x05, 0x9E, 0x84, 0x0C, 0x83, 0x09, 0x83,
                                                          0x0D, 0x83, 0x06, 0x83, 0x05, 0x84, 0x09, 0x83, 0x0C, 0x84,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x0C, 0x9E, 0x88, 0x06, 0x83, 0x06,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D,
                                                          0x83, 0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x09,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x05, 0x94, 0x83, 0x09, 0x84,
                                                          0x0C, 0x83, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x06, 0x80
                                                        });
        listButtons.Items[7].Tag = new BlasterCommand("7",
                                                      new byte[]
                                                        {
                                                          0x9E, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05, 0x84,
                                                          0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84, 0x05, 0x84,
                                                          0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83, 0x06, 0x83,
                                                          0x05, 0x9E, 0x84, 0x09, 0x83, 0x10, 0x83, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x09, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05,
                                                          0x84, 0x08, 0x9E, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84,
                                                          0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83,
                                                          0x06, 0x83, 0x05, 0x84, 0x09, 0x83, 0x10, 0x83, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x9E, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x09, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C,
                                                          0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05,
                                                          0x84, 0x05, 0x84, 0x05, 0x9E, 0x84, 0x0C, 0x83, 0x09, 0x83,
                                                          0x0D, 0x83, 0x06, 0x83, 0x05, 0x84, 0x09, 0x83, 0x10, 0x83,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x09, 0x9E, 0x88, 0x06, 0x83, 0x06,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D,
                                                          0x83, 0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x09,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x05, 0x94, 0x83, 0x09, 0x84,
                                                          0x0F, 0x84, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x02, 0x80
                                                        });
        listButtons.Items[8].Tag = new BlasterCommand("8",
                                                      new byte[]
                                                        {
                                                          0x9E, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05, 0x84,
                                                          0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84, 0x05, 0x84,
                                                          0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83, 0x06, 0x83,
                                                          0x05, 0x9E, 0x84, 0x0C, 0x83, 0x06, 0x83, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x10, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05,
                                                          0x84, 0x08, 0x9E, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84,
                                                          0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83,
                                                          0x06, 0x83, 0x05, 0x84, 0x0C, 0x83, 0x06, 0x83, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x9E, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x10, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C,
                                                          0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05,
                                                          0x84, 0x05, 0x84, 0x05, 0x9E, 0x84, 0x0C, 0x83, 0x09, 0x83,
                                                          0x0D, 0x83, 0x06, 0x83, 0x05, 0x84, 0x0C, 0x83, 0x06, 0x83,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x10, 0x9E, 0x88, 0x06, 0x83, 0x06,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D,
                                                          0x83, 0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x09,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x05, 0x94, 0x83, 0x0D, 0x83,
                                                          0x06, 0x83, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x09, 0x80
                                                        });
        listButtons.Items[9].Tag = new BlasterCommand("9",
                                                      new byte[]
                                                        {
                                                          0x9E, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05, 0x84,
                                                          0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84, 0x05, 0x84,
                                                          0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83, 0x06, 0x83,
                                                          0x05, 0x9E, 0x84, 0x0C, 0x83, 0x09, 0x84, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x0C, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C, 0x84, 0x05,
                                                          0x84, 0x08, 0x9E, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05, 0x84,
                                                          0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x09, 0x83, 0x0D, 0x83,
                                                          0x06, 0x83, 0x05, 0x84, 0x0C, 0x83, 0x09, 0x84, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x9E, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x0C, 0x88, 0x06, 0x83, 0x06, 0x83, 0x0C,
                                                          0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x05,
                                                          0x84, 0x05, 0x84, 0x05, 0x9E, 0x84, 0x0C, 0x83, 0x09, 0x83,
                                                          0x0D, 0x83, 0x06, 0x83, 0x05, 0x84, 0x0C, 0x83, 0x09, 0x84,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x0C, 0x9E, 0x88, 0x06, 0x83, 0x06,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x08, 0x84, 0x0C, 0x83, 0x0D,
                                                          0x83, 0x05, 0x84, 0x05, 0x84, 0x0C, 0x83, 0x0D, 0x83, 0x09,
                                                          0x83, 0x0C, 0x84, 0x05, 0x84, 0x05, 0x94, 0x83, 0x0D, 0x83,
                                                          0x09, 0x83, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F,
                                                          0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x06, 0x80
                                                        });

//				Save();
      }

      if (Blaster.Connected == false)
      {
        buttonStart.Enabled = false;
        buttonTest.Enabled = false;

        return;
      }

      buttonTest.Enabled = _selectedItem != null && _selectedItem.Tag != null;

      propertyGrid.ToolbarVisible = false;
      propertyGrid.SelectedObject = listButtons.Items[0].Tag;

//			ListView1.Groups.Add(New ListViewGroup("Group 1", _ HorizontalAlignment.Left))
    }

    private void listButtons_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (listButtons.SelectedItems.Count == 0) return;

      _selectedItem = listButtons.SelectedItems[0];

      if (_selectedItem == null) return;

      _selectedItem.EnsureVisible();

      buttonTest.Enabled = Blaster.Connected && _selectedItem.Tag != null;
      propertyGrid.SelectedObject = _selectedItem.Tag;
    }

    private void buttonStart_Click(object sender, System.EventArgs e)
    {
      if (_selectedItem == null) return;

      _timeoutRemaining = _timeoutMax;
      _selectedItem.SubItems[0].Text = _timeoutRemaining.ToString() + " seconds remaining...";

      buttonStart.Text = "&Stop";
      buttonTest.Enabled = false;
      timerLearn.Enabled = true;
      listButtons.Focus();

      Blaster.BeginLearn(new LearnCallback(OnLearnComplete));
    }

    private void buttonStop_Click(object sender, System.EventArgs e)
    {
//			Blaster.Cancel();
    }

    private void listButtons_DoubleClick(object sender, System.EventArgs e)
    {
      Test();
    }

    private void buttonTest_Click(object sender, System.EventArgs e)
    {
      Test();

      listButtons.Focus();
    }

    private void timerLearn_Tick(object sender, System.EventArgs e)
    {
      if (sender != timerLearn) return;
      if (_selectedItem == null) return;
      if (_selectedItem.Tag == null) return;
      if (_selectedItem.Tag.GetType() != typeof (BlasterCommand)) return;

      if (--_timeoutRemaining <= 0)
      {
        _selectedItem.SubItems[0].Text = ((BlasterCommand)_selectedItem.Tag).Name;

        buttonStart.Text = "&Learn";
        buttonTest.Enabled = false;
      }
      else
      {
        _selectedItem.SubItems[0].Text = _timeoutRemaining.ToString() + " seconds remaining...";
      }
    }

    //////////////
    ///
    private void Test()
    {
      if (Blaster.Connected == false) return;
      if (buttonTest.Enabled == false) return;
      if (_selectedItem == null || _selectedItem.Tag == null) return;
      if (_selectedItem.Tag.GetType() != typeof (BlasterCommand)) return;

      BlasterCommand command = _selectedItem.Tag as BlasterCommand;

      if (command.RawData == null) return;

      switch (command.Speed)
      {
        case Speed.Fast:
          Blaster.Speed = 0;
          break;
        case Speed.Medium:
          Blaster.Speed = 1;
          break;
        case Speed.Slow:
          Blaster.Speed = 2;
          break;
      }

      switch (command.Port)
      {
        case Port.Both:
          Blaster.Send(0, command.RawData);
          break;
        case Port.One:
          Blaster.Send(1, command.RawData);
          break;
        case Port.Two:
          Blaster.Send(2, command.RawData);
          break;
      }
    }

    //////////////
    ///
    private void OnDeviceArrival()
    {
      buttonStart.Text = "&Learn";
      buttonStart.Enabled = true;
      buttonTest.Enabled = _selectedItem != null && _selectedItem.Tag != null;
    }

    private void OnDeviceRemoval()
    {
      Log.Write("Device removal");

//			// are we currently learning?
//			if(buttonStart.Enabled) return;

      timerLearn.Enabled = false;

      buttonStart.Text = "&Learn";
      buttonStart.Enabled = false;
      buttonTest.Enabled = false;
    }

    private void OnLearnComplete(byte[] packet)
    {
      if (_selectedItem == null) return;

      timerLearn.Enabled = false;

      buttonStart.Text = "&Learn";
      buttonStart.Enabled = true;
      buttonTest.Enabled = true;

      BlasterCommand command = _selectedItem.Tag as BlasterCommand;

      command.RawData = packet;
      command.Status = packet == null ? Status.Failed : Status.Success;

      _selectedItem.SubItems[0].Text = command.Name;

      if (packet != null)
      {
        // quick and dirty way to make sure that the property grid is updated
        listButtons.Items[Math.Min(_selectedItem.Index + 1, listButtons.Items.Count - 1)].Selected = false;
        listButtons.Items[Math.Min(_selectedItem.Index + 1, listButtons.Items.Count - 1)].Selected = true;
      }

      Save2();
    }

    #region Members

    private ListViewItem _selectedItem;
    private int _timeoutRemaining;
    private int _timeoutMax = 10;
    public bool _gumboTest = false;

    #endregion Members

    private void propertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
    {
      if (_selectedItem == null) return;

      BlasterCommand command = _selectedItem.Tag as BlasterCommand;

      _selectedItem.SubItems[0].Text = command.Name;

      propertyGrid.Validate();

//			e.ChangedItem.PropertyDescriptor = PropertyDescriptor

      Console.WriteLine("Desc: {0}", command.Description);

      Save2();
    }

    private void Save()
    {
      Hashtable hash = new Hashtable();

      foreach (XPListViewItem item in listButtons.Items)
      {
        if (item.GroupIndex != 0)
          continue;

        if (item.Tag == null)
          continue;

        if (((BlasterCommand)item.Tag).RawData == null)
          continue;

        hash[item.Text] = ((BlasterCommand)item.Tag).RawData;
      }

      if (hash.Count > 0)
      {
        try
        {
          using (FileStream fs = new FileStream("serverblaster.dat", FileMode.Create, FileAccess.Write))
          {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, hash);
          }
        }
        catch
        {
          Console.WriteLine("Failed to save");
        }
      }
    }

    private void Save2()
    {
      Save();
/*			Hashtable hashRoot = new Hashtable();

			foreach(XPListViewGroup group in listButtons.Groups)
			{
				foreach(ListViewItem item in listButtons.Items) hash[item.Text] = item.Tag;
//				((HashTable)hashRoot[group.GroupText])
			}
			
			foreach(ListViewItem item in listButtons.Items) hash[item.Text] = item.Tag;

			if(hash.Count > 0)
			{
				try
				{
					using(FileStream fs = new FileStream("myblaster.xml", FileMode.Create, FileAccess.Write))
					{
						//						IFormatter formatter = new BinaryFormatter();
						IFormatter formatter = new SoapFormatter();
						formatter.Serialize(fs, hash);
					}
				}
				catch
				{
					Console.WriteLine("Failed to save");
				}
			}
*/
    }

    private void LoadConfig()
    {
      try
      {
        using (FileStream fs = new FileStream("serverblaster.dat", FileMode.Open, FileAccess.Read))
        {
          BinaryFormatter bf = new BinaryFormatter();
          object objectRoot = bf.Deserialize(fs);

          if (objectRoot is Hashtable)
          {
            Hashtable hashTable = objectRoot as Hashtable;

            foreach (object objectKey in hashTable.Keys)
            {
              if (objectKey is string)
              {
                object objectValue = hashTable[objectKey];

                if (objectValue is byte[])
                {
                  hashTable[objectKey] = new BlasterCommand(objectKey as string, objectValue as byte[]);
                }
                else if (objectValue is Hashtable)
                {
                  bool b = true;
                }
                else
                {
                  Log.Write("Form1.LoadConfig: Unexpected value type '{0}'", objectValue.GetType());
                }
              }
              else
              {
                Log.Write("Form1.LoadConfig: Unexpected key type '{0}'", objectKey.GetType());
              }
            }
          }
          else
          {
            Log.Write("Form1.LoadConfig: Unexpected root object");
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write("Form1.LoadConfig: {0}", ex.Message);
      }
    }
  }
}