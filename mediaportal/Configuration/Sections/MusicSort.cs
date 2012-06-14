#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.ComponentModel;
using System.Globalization;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class MusicSort : SectionSettings
  {
    private IContainer components = null;

    private const string defaultTrackTag = "[%track%. ][%artist% - ][%title%]";
    private const string defaultAlbumTrackTag = "[%artist% - ][%title%]";
    private const string defaultFileTag = "[%filename%]";

    private string[] sortModes = {
                                   "Name", "Date", "Size", "Track", "Duration", "Title", "Artist", "Album", "Filename",
                                   "Rating", "Album Artist", "Year", "DiscID", "Composer", "Times Played"
                                 };

    private string[] defaultSortTags1 = {
                                          defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultTrackTag,
                                          defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultAlbumTrackTag,
                                          defaultFileTag, defaultTrackTag, defaultTrackTag, defaultTrackTag,
                                          defaultTrackTag, defaultTrackTag, defaultTrackTag
                                        };

    private string[] defaultSortTags2 = {
                                          "%duration%", "%date%", "%filesize%", "%duration%", "%duration%",
                                          "%duration%", "%duration%", "%album%", "%filesize%", "%rating%",
                                          "%duration%", "%year%", "%disc#%", "%duration%", "%timesplayed%"
                                        };

    private string[] sortTags1 = new string[20];
    private MPGroupBox groupBox3;
    private MPGroupBox groupBox4;
    private MPLabel label15;
    private MPLabel label18;
    private MPLabel label16;
    private MPLabel label17;
    private MPTextBox textBoxSample;
    private MPLabel label19;
    private MPTextBox tbSortRight;
    private MPLabel label20;
    private MPTextBox tbSortLeft;
    private MPLabel label21;
    private MPComboBox comboBox1;
    private MPLabel label38;
    private string[] sortTags2 = new string[20];

    /// <summary>
    /// 
    /// </summary>
    public MusicSort()
      : this("Music Track Format Masks") {}

    /// <summary>
    /// 
    /// </summary>
    public MusicSort(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      comboBox1.Items.Clear();
      using (Settings xmlreader = new MPSettings())
      {
        for (int i = 0; i < sortModes.Length; ++i)
        {
          sortTags1[i] = xmlreader.GetValueAsString("mymusic", sortModes[i] + "1", defaultSortTags1[i]);
          sortTags2[i] = xmlreader.GetValueAsString("mymusic", sortModes[i] + "2", defaultSortTags2[i]);
          comboBox1.Items.Add(sortModes[i]);
        }
      }
      comboBox1.SelectedIndex = 0;
      tbSortLeft.Text = sortTags1[comboBox1.SelectedIndex];
      tbSortRight.Text = sortTags2[comboBox1.SelectedIndex];
      ShowExample();
    }

    private void ShowExample()
    {
      string duration = "6:21";
      string fileSize = "3.2MB";
      string artist = "Pink Floyd";
      string albumartist = "Pink Floyd";
      string album = "The Wall";
      string title = "Comfortably Numb";
      string trackNr = "06";
      string year = "1979";
      string genre = "Rock";
      string date = DateTime.Now.ToShortDateString();
      string discNr = "2";
      string composer = "Roger Waters";
      string filename = "comfortably numb.mp3";
      string rating = "8.2";
      string timesPlayed = "3";

      string line1 = tbSortLeft.Text;
      string line2 = tbSortRight.Text;
      line1 = Util.Utils.ReplaceTag(line1, "%track%", trackNr);
      line2 = Util.Utils.ReplaceTag(line2, "%track%", trackNr);
      line1 = Util.Utils.ReplaceTag(line1, "%filesize%", fileSize);
      line2 = Util.Utils.ReplaceTag(line2, "%filesize%", fileSize);
      line1 = Util.Utils.ReplaceTag(line1, "%artist%", artist);
      line2 = Util.Utils.ReplaceTag(line2, "%artist%", artist);
      line1 = Util.Utils.ReplaceTag(line1, "%albumartist%", albumartist);
      line2 = Util.Utils.ReplaceTag(line2, "%albumartist%", albumartist);
      line1 = Util.Utils.ReplaceTag(line1, "%album%", album);
      line2 = Util.Utils.ReplaceTag(line2, "%album%", album);
      line1 = Util.Utils.ReplaceTag(line1, "%title%", title);
      line2 = Util.Utils.ReplaceTag(line2, "%title%", title);
      line1 = Util.Utils.ReplaceTag(line1, "%year%", year);
      line2 = Util.Utils.ReplaceTag(line2, "%year%", year);
      line1 = Util.Utils.ReplaceTag(line1, "%date%", date);
      line2 = Util.Utils.ReplaceTag(line2, "%date%", date);
      line1 = Util.Utils.ReplaceTag(line1, "%filename%", filename);
      line2 = Util.Utils.ReplaceTag(line2, "%filename%", filename);
      line1 = Util.Utils.ReplaceTag(line1, "%rating%", rating);
      line2 = Util.Utils.ReplaceTag(line2, "%rating%", rating);
      line1 = Util.Utils.ReplaceTag(line1, "%duration%", duration);
      line2 = Util.Utils.ReplaceTag(line2, "%duration%", duration);
      line1 = Util.Utils.ReplaceTag(line1, "%genre%", genre);
      line2 = Util.Utils.ReplaceTag(line2, "%genre%", genre);
      line1 = Util.Utils.ReplaceTag(line1, "%disc#%", discNr);
      line2 = Util.Utils.ReplaceTag(line2, "%disc#%", discNr);
      line1 = Util.Utils.ReplaceTag(line1, "%composer%", composer);
      line2 = Util.Utils.ReplaceTag(line2, "%composer%", composer);
      line1 = Util.Utils.ReplaceTag(line1, "%timesplayed%", timesPlayed);
      line2 = Util.Utils.ReplaceTag(line2, "%timesplayed%", timesPlayed);

      while (line1.Length < 25)
      {
        line1 += " ";
      }
      textBoxSample.Text = line1 + "\t" + line2;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        for (int i = 0; i < sortModes.Length; ++i)
        {
          xmlwriter.SetValue("mymusic", sortModes[i] + "1", sortTags1[i]);
          xmlwriter.SetValue("mymusic", sortModes[i] + "2", sortTags2[i]);
        }
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

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label15 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label18 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label16 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label17 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxSample = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label19 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbSortRight = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label20 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbSortLeft = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label21 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBox1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label38 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox3.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.groupBox4);
      this.groupBox3.Controls.Add(this.textBoxSample);
      this.groupBox3.Controls.Add(this.label19);
      this.groupBox3.Controls.Add(this.tbSortRight);
      this.groupBox3.Controls.Add(this.label20);
      this.groupBox3.Controls.Add(this.tbSortLeft);
      this.groupBox3.Controls.Add(this.label21);
      this.groupBox3.Controls.Add(this.comboBox1);
      this.groupBox3.Controls.Add(this.label38);
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox3.Location = new System.Drawing.Point(6, 0);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(462, 282);
      this.groupBox3.TabIndex = 3;
      this.groupBox3.TabStop = false;
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.label15);
      this.groupBox4.Controls.Add(this.label18);
      this.groupBox4.Controls.Add(this.label16);
      this.groupBox4.Controls.Add(this.label17);
      this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox4.Location = new System.Drawing.Point(19, 135);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(397, 132);
      this.groupBox4.TabIndex = 21;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Available Tags";
      // 
      // label15
      // 
      this.label15.AutoSize = true;
      this.label15.Location = new System.Drawing.Point(289, 24);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(94, 91);
      this.label15.TabIndex = 15;
      this.label15.Text = "filename\r\nfilesize of song\r\nduration of song\r\nsong rating\r\nfile date\r\nname of com" +
    "poser\r\ngenre of track";
      // 
      // label18
      // 
      this.label18.AutoSize = true;
      this.label18.Location = new System.Drawing.Point(211, 24);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(78, 91);
      this.label18.TabIndex = 14;
      this.label18.Text = "%filename% =\r\n%filesize% =\r\n%duration% =\r\n%rating% =\r\n%date% =\r\n%composer% =\r\n%ge" +
    "nre% =\r\n";
      this.label18.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // label16
      // 
      this.label16.AutoSize = true;
      this.label16.Location = new System.Drawing.Point(94, 24);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(189, 104);
      this.label16.TabIndex = 13;
      this.label16.Text = "name of artist\r\nsong title\r\nname of album artist\r\nname of album\r\ndisc number\r\ntra" +
    "cknumber\r\nyear of song\r\nnumber of times track has been played";
      // 
      // label17
      // 
      this.label17.AutoSize = true;
      this.label17.Location = new System.Drawing.Point(6, 24);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(87, 104);
      this.label17.TabIndex = 12;
      this.label17.Text = "%artist% =\r\n%title% =\r\n%albumartist% =\r\n%album% =\r\n%disc#% =\r\n%track% =\r\n%year% =" +
    "\r\n%timesplayed% =";
      this.label17.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // textBoxSample
      // 
      this.textBoxSample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxSample.BackColor = System.Drawing.SystemColors.ControlLight;
      this.textBoxSample.BorderColor = System.Drawing.Color.Empty;
      this.textBoxSample.Location = new System.Drawing.Point(96, 102);
      this.textBoxSample.Name = "textBoxSample";
      this.textBoxSample.ReadOnly = true;
      this.textBoxSample.Size = new System.Drawing.Size(350, 20);
      this.textBoxSample.TabIndex = 19;
      // 
      // label19
      // 
      this.label19.AutoSize = true;
      this.label19.Location = new System.Drawing.Point(16, 24);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(72, 13);
      this.label19.TabIndex = 18;
      this.label19.Text = "Sorting mode:";
      // 
      // tbSortRight
      // 
      this.tbSortRight.BorderColor = System.Drawing.Color.Empty;
      this.tbSortRight.Location = new System.Drawing.Point(345, 71);
      this.tbSortRight.Name = "tbSortRight";
      this.tbSortRight.Size = new System.Drawing.Size(71, 20);
      this.tbSortRight.TabIndex = 4;
      this.tbSortRight.Text = "%duration%";
      this.tbSortRight.TextChanged += new System.EventHandler(this.tbSortRight_TextChanged);
      // 
      // label20
      // 
      this.label20.AutoSize = true;
      this.label20.Location = new System.Drawing.Point(93, 51);
      this.label20.Name = "label20";
      this.label20.Size = new System.Drawing.Size(217, 13);
      this.label20.TabIndex = 0;
      this.label20.Text = "Use blockquotes [ ] to specify optional fields.";
      // 
      // tbSortLeft
      // 
      this.tbSortLeft.BorderColor = System.Drawing.Color.Empty;
      this.tbSortLeft.Location = new System.Drawing.Point(96, 71);
      this.tbSortLeft.Name = "tbSortLeft";
      this.tbSortLeft.Size = new System.Drawing.Size(248, 20);
      this.tbSortLeft.TabIndex = 3;
      this.tbSortLeft.Text = "%track%. %artist% - %title%";
      this.tbSortLeft.TextChanged += new System.EventHandler(this.tbSortLeft_TextChanged);
      // 
      // label21
      // 
      this.label21.AutoSize = true;
      this.label21.Location = new System.Drawing.Point(16, 105);
      this.label21.Name = "label21";
      this.label21.Size = new System.Drawing.Size(45, 13);
      this.label21.TabIndex = 5;
      this.label21.Text = "Sample:";
      // 
      // comboBox1
      // 
      this.comboBox1.BorderColor = System.Drawing.Color.Empty;
      this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox1.FormattingEnabled = true;
      this.comboBox1.Location = new System.Drawing.Point(96, 21);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(88, 21);
      this.comboBox1.TabIndex = 0;
      this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
      // 
      // label38
      // 
      this.label38.AutoSize = true;
      this.label38.Location = new System.Drawing.Point(16, 74);
      this.label38.Name = "label38";
      this.label38.Size = new System.Drawing.Size(42, 13);
      this.label38.TabIndex = 3;
      this.label38.Text = "Format:";
      // 
      // MusicSort
      // 
      this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.Controls.Add(this.groupBox3);
      this.DoubleBuffered = true;
      this.Name = "MusicSort";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      tbSortLeft.Text = sortTags1[comboBox1.SelectedIndex];
      tbSortRight.Text = sortTags2[comboBox1.SelectedIndex];
      ShowExample();
    }

    private void tbSortLeft_TextChanged(object sender, EventArgs e)
    {
      sortTags1[comboBox1.SelectedIndex] = tbSortLeft.Text;
      ShowExample();
    }

    private void tbSortRight_TextChanged(object sender, EventArgs e)
    {
      sortTags2[comboBox1.SelectedIndex] = tbSortRight.Text;
      ShowExample();
    }
  }
}