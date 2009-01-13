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
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Recording;
using MediaPortal.UserInterface.Controls;
#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class RadioStations : SectionSettings
  {
    public class ComboCard
    {
      public string FriendlyName;
      public string VideoDevice;
      public int ID;

      public override string ToString()
      {
        return String.Format("{0} - {1}", FriendlyName, VideoDevice);
      }
    } ;

    private ColumnHeader columnHeader3;
    private MPButton deleteButton;
    private MPButton editButton;
    private MPButton addButton;
    private MPListView stationsListView;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private ColumnHeader columnHeader4;
    private ColumnHeader columnHeader5;
    private ColumnHeader columnHeader6;
    private IContainer components = null;
    private MPButton upButton;
    private MPButton downButton;

    //
    // Private members
    //
    //bool isDirty = false;
    private MPTabControl tabControl1;
    private MPTabPage tabPage1;
    private MPTabPage tabPage2;
    private MPButton btnMapChannelToCard;
    private MPButton btnUnmapChannelFromCard;
    private ColumnHeader columnHeader10;
    private ColumnHeader columnHeader11;
    private MPLabel label6;
    private MPComboBox comboBoxCard;
    private ListView listviewCardChannels;
    private ListView listViewRadioChannels;
    private ListViewItem currentlyCheckedItem = null;
    private static bool reloadList = false;
    private MPButton mpButtonClear;
    private Label lblCheckedHint;
    private ListViewColumnSorter _columnSorter;


    public RadioStations()
      : this("Stations")
    {
    }

    public RadioStations(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // Disable if TVE3
      if (File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"))
      {
        this.Enabled = false;
      }
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

    public override void OnSectionActivated()
    {
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.upButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.downButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.deleteButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.editButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.addButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.stationsListView = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.mpButtonClear = new MediaPortal.UserInterface.Controls.MPButton();
      this.tabPage2 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.btnMapChannelToCard = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnUnmapChannelFromCard = new MediaPortal.UserInterface.Controls.MPButton();
      this.listviewCardChannels = new System.Windows.Forms.ListView();
      this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
      this.listViewRadioChannels = new System.Windows.Forms.ListView();
      this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxCard = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lblCheckedHint = new System.Windows.Forms.Label();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.SuspendLayout();
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Frequency";
      this.columnHeader3.Width = 54;
      // 
      // upButton
      // 
      this.upButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.upButton.Enabled = false;
      this.upButton.Location = new System.Drawing.Point(222, 334);
      this.upButton.Name = "upButton";
      this.upButton.Size = new System.Drawing.Size(72, 22);
      this.upButton.TabIndex = 4;
      this.upButton.Text = "Up";
      this.upButton.UseVisualStyleBackColor = true;
      this.upButton.Click += new System.EventHandler(this.upButton_Click);
      // 
      // downButton
      // 
      this.downButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.downButton.Enabled = false;
      this.downButton.Location = new System.Drawing.Point(222, 357);
      this.downButton.Name = "downButton";
      this.downButton.Size = new System.Drawing.Size(72, 22);
      this.downButton.TabIndex = 5;
      this.downButton.Text = "Down";
      this.downButton.UseVisualStyleBackColor = true;
      this.downButton.Click += new System.EventHandler(this.downButton_Click);
      // 
      // deleteButton
      // 
      this.deleteButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.deleteButton.Enabled = false;
      this.deleteButton.Location = new System.Drawing.Point(94, 357);
      this.deleteButton.Name = "deleteButton";
      this.deleteButton.Size = new System.Drawing.Size(72, 22);
      this.deleteButton.TabIndex = 3;
      this.deleteButton.Text = "Delete";
      this.deleteButton.UseVisualStyleBackColor = true;
      this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
      // 
      // editButton
      // 
      this.editButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.editButton.Enabled = false;
      this.editButton.Location = new System.Drawing.Point(94, 334);
      this.editButton.Name = "editButton";
      this.editButton.Size = new System.Drawing.Size(72, 22);
      this.editButton.TabIndex = 2;
      this.editButton.Text = "Edit";
      this.editButton.UseVisualStyleBackColor = true;
      this.editButton.Click += new System.EventHandler(this.editButton_Click);
      // 
      // addButton
      // 
      this.addButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.addButton.Location = new System.Drawing.Point(16, 334);
      this.addButton.Name = "addButton";
      this.addButton.Size = new System.Drawing.Size(72, 22);
      this.addButton.TabIndex = 1;
      this.addButton.Text = "Add";
      this.addButton.UseVisualStyleBackColor = true;
      this.addButton.Click += new System.EventHandler(this.addButton_Click);
      // 
      // stationsListView
      // 
      this.stationsListView.AllowDrop = true;
      this.stationsListView.AllowRowReorder = true;
      this.stationsListView.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.stationsListView.CheckBoxes = true;
      this.stationsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                               {
                                                 this.columnHeader1,
                                                 this.columnHeader2,
                                                 this.columnHeader3,
                                                 this.columnHeader4,
                                                 this.columnHeader5,
                                                 this.columnHeader6
                                               });
      this.stationsListView.FullRowSelect = true;
      this.stationsListView.HideSelection = false;
      this.stationsListView.Location = new System.Drawing.Point(16, 37);
      this.stationsListView.Name = "stationsListView";
      this.stationsListView.Size = new System.Drawing.Size(432, 291);
      this.stationsListView.TabIndex = 0;
      this.stationsListView.UseCompatibleStateImageBehavior = false;
      this.stationsListView.View = System.Windows.Forms.View.Details;
      this.stationsListView.DoubleClick += new System.EventHandler(this.stationsListView_DoubleClick);
      this.stationsListView.SelectedIndexChanged += new System.EventHandler(this.stationsListView_SelectedIndexChanged);
      this.stationsListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.stationsListView_ItemCheck);
      this.stationsListView.ColumnClick +=
        new System.Windows.Forms.ColumnClickEventHandler(this.stationsListView_ColumnClick);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Type";
      this.columnHeader1.Width = 65;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Station name";
      this.columnHeader2.Width = 117;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Genre";
      this.columnHeader4.Width = 72;
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Bitrate";
      this.columnHeader5.Width = 42;
      // 
      // columnHeader6
      // 
      this.columnHeader6.Text = "Server";
      this.columnHeader6.Width = 78;
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(472, 408);
      this.tabControl1.TabIndex = 0;
      this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.lblCheckedHint);
      this.tabPage1.Controls.Add(this.mpButtonClear);
      this.tabPage1.Controls.Add(this.stationsListView);
      this.tabPage1.Controls.Add(this.deleteButton);
      this.tabPage1.Controls.Add(this.editButton);
      this.tabPage1.Controls.Add(this.addButton);
      this.tabPage1.Controls.Add(this.upButton);
      this.tabPage1.Controls.Add(this.downButton);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(464, 382);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Radio Stations";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpButtonClear
      // 
      this.mpButtonClear.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonClear.Location = new System.Drawing.Point(16, 357);
      this.mpButtonClear.Name = "mpButtonClear";
      this.mpButtonClear.Size = new System.Drawing.Size(72, 22);
      this.mpButtonClear.TabIndex = 6;
      this.mpButtonClear.Text = "Clear";
      this.mpButtonClear.UseVisualStyleBackColor = true;
      this.mpButtonClear.Click += new System.EventHandler(this.mpButtonClear_Click);
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.btnMapChannelToCard);
      this.tabPage2.Controls.Add(this.btnUnmapChannelFromCard);
      this.tabPage2.Controls.Add(this.listviewCardChannels);
      this.tabPage2.Controls.Add(this.listViewRadioChannels);
      this.tabPage2.Controls.Add(this.label6);
      this.tabPage2.Controls.Add(this.comboBoxCard);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(464, 382);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Radio Cards";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // btnMapChannelToCard
      // 
      this.btnMapChannelToCard.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.btnMapChannelToCard.Location = new System.Drawing.Point(212, 168);
      this.btnMapChannelToCard.Name = "btnMapChannelToCard";
      this.btnMapChannelToCard.Size = new System.Drawing.Size(40, 22);
      this.btnMapChannelToCard.TabIndex = 3;
      this.btnMapChannelToCard.Text = ">>";
      this.btnMapChannelToCard.UseVisualStyleBackColor = true;
      this.btnMapChannelToCard.Click += new System.EventHandler(this.btnMapChannelToCard_Click);
      // 
      // btnUnmapChannelFromCard
      // 
      this.btnUnmapChannelFromCard.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.btnUnmapChannelFromCard.Location = new System.Drawing.Point(212, 200);
      this.btnUnmapChannelFromCard.Name = "btnUnmapChannelFromCard";
      this.btnUnmapChannelFromCard.Size = new System.Drawing.Size(40, 22);
      this.btnUnmapChannelFromCard.TabIndex = 4;
      this.btnUnmapChannelFromCard.Text = "<<";
      this.btnUnmapChannelFromCard.UseVisualStyleBackColor = true;
      this.btnUnmapChannelFromCard.Click += new System.EventHandler(this.btnUnmapChannelFromCard_Click);
      // 
      // listviewCardChannels
      // 
      this.listviewCardChannels.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.listviewCardChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                                   {
                                                     this.columnHeader10
                                                   });
      this.listviewCardChannels.Location = new System.Drawing.Point(272, 56);
      this.listviewCardChannels.Name = "listviewCardChannels";
      this.listviewCardChannels.Size = new System.Drawing.Size(176, 304);
      this.listviewCardChannels.TabIndex = 5;
      this.listviewCardChannels.UseCompatibleStateImageBehavior = false;
      this.listviewCardChannels.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader10
      // 
      this.columnHeader10.Text = "Assigned Radio Stations";
      this.columnHeader10.Width = 154;
      // 
      // listViewRadioChannels
      // 
      this.listViewRadioChannels.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
           | System.Windows.Forms.AnchorStyles.Left)));
      this.listViewRadioChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                                    {
                                                      this.columnHeader11
                                                    });
      this.listViewRadioChannels.Location = new System.Drawing.Point(16, 56);
      this.listViewRadioChannels.Name = "listViewRadioChannels";
      this.listViewRadioChannels.Size = new System.Drawing.Size(176, 304);
      this.listViewRadioChannels.TabIndex = 2;
      this.listViewRadioChannels.UseCompatibleStateImageBehavior = false;
      this.listViewRadioChannels.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader11
      // 
      this.columnHeader11.Text = "Available Radio Stations";
      this.columnHeader11.Width = 154;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 24);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(144, 16);
      this.label6.TabIndex = 0;
      this.label6.Text = "Map stations to radio card:";
      // 
      // comboBoxCard
      // 
      this.comboBoxCard.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxCard.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxCard.Location = new System.Drawing.Point(160, 20);
      this.comboBoxCard.Name = "comboBoxCard";
      this.comboBoxCard.Size = new System.Drawing.Size(288, 21);
      this.comboBoxCard.TabIndex = 1;
      this.comboBoxCard.SelectedIndexChanged += new System.EventHandler(this.comboBoxCard_SelectedIndexChanged);
      // 
      // lblCheckedHint
      // 
      this.lblCheckedHint.AutoSize = true;
      this.lblCheckedHint.Location = new System.Drawing.Point(13, 12);
      this.lblCheckedHint.Name = "lblCheckedHint";
      this.lblCheckedHint.Size = new System.Drawing.Size(321, 13);
      this.lblCheckedHint.TabIndex = 7;
      this.lblCheckedHint.Text = "Note: If you select a station it will autostart everytime MP comes up";
      // 
      // RadioStations
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "RadioStations";
      this.Size = new System.Drawing.Size(472, 408);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.tabPage2.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    private void addButton_Click(object sender, EventArgs e)
    {
      //			isDirty = true;

      RadioStation newStation = new RadioStation();
      newStation.Type = "Radio";
      newStation.Frequency = new Frequency(0);
      EditRadioStationForm editStation = new EditRadioStationForm();
      editStation.Station = newStation;

      DialogResult dialogResult = editStation.ShowDialog(this);

      if (dialogResult == DialogResult.OK)
      {
        MediaPortal.Radio.Database.RadioStation station = new MediaPortal.Radio.Database.RadioStation();
        station.Scrambled = false;
        station.Name = editStation.Station.Name;
        station.Genre = editStation.Station.Genre;
        station.BitRate = editStation.Station.Bitrate;
        station.URL = editStation.Station.URL;
        station.Frequency = editStation.Station.Frequency.Hertz;
        if (station.Frequency < 1000)
        {
          station.Frequency *= 1000000L;
        }

        ListViewItem listItem = new ListViewItem(new string[]
                                                   {
                                                     editStation.Station.Type,
                                                     editStation.Station.Name,
                                                     editStation.Station.Frequency.ToString(Frequency.Format.MegaHertz),
                                                     editStation.Station.Genre,
                                                     editStation.Station.Bitrate.ToString(),
                                                     editStation.Station.URL
                                                   });


        listItem.Tag = editStation.Station;
        stationsListView.Items.Add(listItem);
        station.Channel = listItem.Index;
        editStation.Station.ID = RadioDatabase.AddStation(ref station);
      }
    }

    private void editButton_Click(object sender, EventArgs e)
    {
      //			isDirty = true;

      foreach (ListViewItem listItem in stationsListView.SelectedItems)
      {
        EditRadioStationForm editStation = new EditRadioStationForm();
        editStation.Station = listItem.Tag as RadioStation;

        DialogResult dialogResult = editStation.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          listItem.Tag = editStation.Station;

          //
          // Remove URL if we have a normal radio station
          //
          if (editStation.Station.Type.Equals("Radio"))
          {
            editStation.Station.URL = string.Empty;
          }

          listItem.SubItems[0].Text = editStation.Station.Type;
          listItem.SubItems[1].Text = editStation.Station.Name;
          listItem.SubItems[2].Text = editStation.Station.Frequency.ToString(Frequency.Format.MegaHertz);
          listItem.SubItems[3].Text = editStation.Station.Genre;
          listItem.SubItems[4].Text = editStation.Station.Bitrate.ToString();
          listItem.SubItems[5].Text = editStation.Station.URL;

          MediaPortal.Radio.Database.RadioStation station = new MediaPortal.Radio.Database.RadioStation();
          station.Scrambled = editStation.Station.Scrambled;
          station.ID = editStation.Station.ID;
          station.Name = editStation.Station.Name;
          station.Genre = editStation.Station.Genre;
          station.BitRate = editStation.Station.Bitrate;
          station.URL = editStation.Station.URL;
          station.Frequency = editStation.Station.Frequency.Hertz;
          if (station.Frequency < 1000)
          {
            station.Frequency *= 1000000L;
          }
          station.Channel = listItem.Index;
          RadioDatabase.UpdateStation(station);
        }
      }
    }

    private void deleteButton_Click(object sender, EventArgs e)
    {
      //			isDirty = true;

      int itemCount = stationsListView.SelectedItems.Count;

      for (int index = 0; index < itemCount; index++)
      {
        RadioStation station = stationsListView.SelectedItems[0].Tag as RadioStation;
        RadioDatabase.RemoveStation(station.Name);
        stationsListView.Items.RemoveAt(stationsListView.SelectedIndices[0]);
      }
    }

    public override void LoadSettings()
    {
      LoadRadioStations();
    }

    public override void SaveSettings()
    {
      SaveRadioStations();
    }

    private void SaveRadioStations()
    {
      //
      // Start by removing the currently available stations from the database
      //

      string strDefaultStation = "";
      foreach (ListViewItem listItem in stationsListView.Items)
      {
        RadioStation radioStation = listItem.Tag as RadioStation;
        if (listItem.Checked == true)
        {
          strDefaultStation = radioStation.Name;
        }
      }
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("myradio", "default", strDefaultStation);
      }
    }

    private void LoadRadioStations()
    {
      stationsListView.Items.Clear();
      string defaultStation = string.Empty;

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        defaultStation = xmlreader.GetValueAsString("myradio", "default", "");
      }

      ArrayList stations = new ArrayList();
      RadioDatabase.GetStations(ref stations);

      foreach (MediaPortal.Radio.Database.RadioStation station in stations)
      {
        RadioStation radioStation = new RadioStation();

        radioStation.ID = station.ID;
        radioStation.Type = station.URL.Length == 0 ? "Radio" : "Stream";
        radioStation.Name = station.Name;
        radioStation.Frequency = station.Frequency;
        radioStation.Genre = station.Genre;
        radioStation.Bitrate = station.BitRate;
        radioStation.URL = station.URL;
        radioStation.Scrambled = station.Scrambled;

        ListViewItem listItem = new ListViewItem(new string[]
                                                   {
                                                     radioStation.Type,
                                                     radioStation.Name,
                                                     radioStation.Frequency.ToString(Frequency.Format.MegaHertz),
                                                     radioStation.Genre,
                                                     radioStation.Bitrate.ToString(),
                                                     radioStation.URL
                                                   });

        //
        // Check default station
        //
        listItem.Checked = radioStation.Name.Equals(defaultStation);

        listItem.Tag = radioStation;

        stationsListView.Items.Add(listItem);
      }
    }

    private void stationsListView_DoubleClick(object sender, EventArgs e)
    {
      editButton_Click(sender, e);
    }

    private void MoveSelectionDown()
    {
      //      isDirty = true;

      for (int index = stationsListView.Items.Count - 1; index >= 0; index--)
      {
        if (stationsListView.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't greater than the highest index in the list view
          //
          if (index < stationsListView.Items.Count - 1)
          {
            ListViewItem listItem = stationsListView.Items[index];
            stationsListView.Items.RemoveAt(index);

            if (index + 1 < stationsListView.Items.Count)
            {
              stationsListView.Items.Insert(index + 1, listItem);
            }
            else
            {
              stationsListView.Items.Add(listItem);
            }
          }
        }
      }
    }

    private void MoveSelectionUp()
    {
      //      isDirty = true;

      for (int index = 0; index < stationsListView.Items.Count; index++)
      {
        if (stationsListView.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't smaller than the lowest index (0) in the list view
          //
          if (index > 0)
          {
            ListViewItem listItem = stationsListView.Items[index];
            stationsListView.Items.RemoveAt(index);
            stationsListView.Items.Insert(index - 1, listItem);
          }
        }
      }
    }

    private void upButton_Click(object sender, EventArgs e)
    {
      MoveSelectionUp();
    }

    private void downButton_Click(object sender, EventArgs e)
    {
      MoveSelectionDown();
    }

    private void stationsListView_SelectedIndexChanged(object sender, EventArgs e)
    {
      deleteButton.Enabled =
        editButton.Enabled = upButton.Enabled = downButton.Enabled = (stationsListView.SelectedItems.Count > 0);
    }

    private void stationsListView_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      //      isDirty = true;

      if (e.NewValue == CheckState.Checked)
      {
        //
        // Check if the new selected item is the same as the current one
        //
        if (stationsListView.Items[e.Index] != currentlyCheckedItem)
        {
          //
          // We have a new selection
          //
          if (currentlyCheckedItem != null)
          {
            currentlyCheckedItem.Checked = false;
          }
          currentlyCheckedItem = stationsListView.Items[e.Index];
        }
      }

      if (e.NewValue == CheckState.Unchecked)
      {
        //
        // Check if the new selected item is the same as the current one
        //
        if (stationsListView.Items[e.Index] == currentlyCheckedItem)
        {
          currentlyCheckedItem = null;
        }
      }
      SaveSettings();
    }

    private void btnMapChannelToCard_Click(object sender, EventArgs e)
    {
      if (listViewRadioChannels.SelectedItems == null)
      {
        return;
      }
      int card = 1;
      int index = comboBoxCard.SelectedIndex;
      if (index >= 0)
      {
        ComboCard combo = (ComboCard) comboBoxCard.Items[index];
        card = combo.ID;
      }

      for (int i = 0; i < listViewRadioChannels.SelectedItems.Count; ++i)
      {
        ListViewItem listItem = listViewRadioChannels.SelectedItems[i];
        MediaPortal.Radio.Database.RadioStation chan = (MediaPortal.Radio.Database.RadioStation) listItem.Tag;

        listItem = new ListViewItem(new string[] {chan.Name});
        listItem.Tag = chan;
        listviewCardChannels.Items.Add(listItem);
        if (chan != null)
        {
          RadioDatabase.MapChannelToCard(chan.ID, card);
        }
      }

      for (int i = listViewRadioChannels.SelectedItems.Count - 1; i >= 0; i--)
      {
        ListViewItem listItem = listViewRadioChannels.SelectedItems[i];

        listViewRadioChannels.Items.Remove(listItem);
      }
    }

    private void btnUnmapChannelFromCard_Click(object sender, EventArgs e)
    {
      int card = 1;
      int index = comboBoxCard.SelectedIndex;
      if (index >= 0)
      {
        ComboCard combo = (ComboCard) comboBoxCard.Items[index];
        card = combo.ID;
      }
      if (listviewCardChannels.SelectedItems == null)
      {
        return;
      }
      for (int i = 0; i < listviewCardChannels.SelectedItems.Count; ++i)
      {
        ListViewItem listItem = listviewCardChannels.SelectedItems[i];
        MediaPortal.Radio.Database.RadioStation chan = (MediaPortal.Radio.Database.RadioStation) listItem.Tag;

        listItem = new ListViewItem(new string[] {chan.Name});
        listItem.Tag = chan;
        listViewRadioChannels.Items.Add(listItem);
      }

      for (int i = listviewCardChannels.SelectedItems.Count - 1; i >= 0; --i)
      {
        ListViewItem listItem = listviewCardChannels.SelectedItems[i];
        MediaPortal.Radio.Database.RadioStation channel = listItem.Tag as MediaPortal.Radio.Database.RadioStation;
        if (channel != null)
        {
          RadioDatabase.UnmapChannelFromCard(channel, card);
        }
        listviewCardChannels.Items.Remove(listItem);
      }
    }

    private void comboBoxCard_SelectedIndexChanged(object sender, EventArgs e)
    {
      FillInChannelCardMappings();
    }

    private void LoadCards()
    {
      comboBoxCard.Items.Clear();
      if (File.Exists(Config.GetFile(Config.Dir.Config, "capturecards.xml")))
      {
        using (
          FileStream fileStream = new FileStream(Config.GetFile(Config.Dir.Config, "capturecards.xml"), FileMode.Open,
                                                 FileAccess.Read, FileShare.ReadWrite))
        {
          try
          {
            //
            // Create Soap Formatter
            //
            SoapFormatter formatter = new SoapFormatter();

            //
            // Serialize
            //
            ArrayList captureCards = new ArrayList();
            captureCards = (ArrayList) formatter.Deserialize(fileStream);
            for (int i = 0; i < captureCards.Count; i++)
            {
              ((TVCaptureDevice) captureCards[i]).ID = (i + 1);

              TVCaptureDevice device = (TVCaptureDevice) captureCards[i];
              ComboCard combo = new ComboCard();
              combo.FriendlyName = device.FriendlyName;
              combo.VideoDevice = device.VideoDevice;
              combo.ID = device.ID;
              comboBoxCard.Items.Add(combo);
            }
            //
            // Finally close our file stream
            //
            fileStream.Close();
          }
          catch
          {
            MessageBox.Show(
              "Failed to load previously configured capture card(s), you will need to re-configure your device(s).",
              "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Log.Error("Recorder: LoadCards()");
          }
        }
      }


      if (comboBoxCard.Items.Count != 0)
      {
        comboBoxCard.SelectedIndex = 0;
      }
      FillInChannelCardMappings();
    }

    private void FillInChannelCardMappings()
    {
      int card = 1;
      int index = comboBoxCard.SelectedIndex;
      if (index >= 0)
      {
        ComboCard combo = (ComboCard) comboBoxCard.Items[index];
        card = combo.ID;
      }

      listViewRadioChannels.Items.Clear();
      listviewCardChannels.Items.Clear();
      ArrayList cardChannels = new ArrayList();
      RadioDatabase.GetStationsForCard(ref cardChannels, card);

      ArrayList channels = new ArrayList();
      RadioDatabase.GetStations(ref channels);
      foreach (MediaPortal.Radio.Database.RadioStation chan in channels)
      {
        bool mapped = false;
        foreach (MediaPortal.Radio.Database.RadioStation chanCard in cardChannels)
        {
          if (chanCard.Name == chan.Name)
          {
            mapped = true;
            break;
          }
        }
        if (!mapped)
        {
          if (chan.URL == string.Empty)
          {
            ListViewItem newItem = new ListViewItem(chan.Name);
            newItem.Tag = chan;
            listViewRadioChannels.Items.Add(newItem);
          }
        }
      }

      foreach (MediaPortal.Radio.Database.RadioStation chanCard in cardChannels)
      {
        if (chanCard.URL == string.Empty)
        {
          ListViewItem newItemCard = new ListViewItem(chanCard.Name);
          newItemCard.Tag = chanCard;
          listviewCardChannels.Items.Add(newItemCard);
        }
      }
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      LoadCards();
    }

    public static void UpdateList()
    {
      reloadList = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      if (reloadList)
      {
        reloadList = false;
        LoadSettings();
        FillInChannelCardMappings();
      }
      base.OnPaint(e);
    }

    private void stationsListView_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (_columnSorter == null)
      {
        stationsListView.ListViewItemSorter = _columnSorter = new ListViewColumnSorter();
      }

      // Determine if clicked column is already the column that is being sorted.
      if (e.Column == _columnSorter.SortColumn)
      {
        _columnSorter.Order = _columnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        _columnSorter.SortColumn = e.Column;
        _columnSorter.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      stationsListView.Sort();
    }

    private void mpButtonClear_Click(object sender, EventArgs e)
    {
      stationsListView.Clear();
      RadioDatabase.ClearAll();
    }
  }
}