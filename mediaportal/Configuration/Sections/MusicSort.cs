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
using System.Globalization;
using System.Windows.Forms;
using MediaPortal.Util;
#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class MusicSort : MediaPortal.Configuration.SectionSettings
  {
    private System.ComponentModel.IContainer components = null;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox1;
    private MediaPortal.UserInterface.Controls.MPTextBox tbSortRight;
    private MediaPortal.UserInterface.Controls.MPTextBox tbSortLeft;

    const string defaultTrackTag = "[%track%. ][%artist% - ][%title%]";
    const string defaultFileTag = "[%filename%]";
    const string albumTrackTag = "[%track%. ][%artist% - ][%title%]";
    string[] sortModes =        { "Name",        "Date",           "Size",          "Track",         "Duration",      "Title",         "Artist",        "Album",       "Filename",     "Rating" };
    string[] defaultSortTags1 = { defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultTrackTag, albumTrackTag, defaultFileTag, defaultTrackTag };
    string[] defaultSortTags2 = { "%duration%", "%year%", "%filesize%", "%duration%", "%duration%", "%duration%", "%duration%", "%duration%", "%filesize%", "%rating%" };

    string[] sortTags1 = new string[20];
    private MediaPortal.UserInterface.Controls.MPTabControl tabControl1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox3;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox4;
    private MediaPortal.UserInterface.Controls.MPLabel label15;
    private MediaPortal.UserInterface.Controls.MPLabel label16;
    private MediaPortal.UserInterface.Controls.MPLabel label17;
    private MediaPortal.UserInterface.Controls.MPLabel label18;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxSample;
    private MediaPortal.UserInterface.Controls.MPLabel label19;
    private MediaPortal.UserInterface.Controls.MPLabel label20;
    private MediaPortal.UserInterface.Controls.MPLabel label21;
    private MediaPortal.UserInterface.Controls.MPLabel label38;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxShowSort;
    string[] sortTags2 = new string[20];
    /// <summary>
    /// 
    /// </summary>
    public MusicSort()
      : this("Music Sort")
    {
    }

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
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        checkBoxShowSort.Checked = xmlreader.GetValueAsBool("musicfiles", "showSortButton", true);

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
    void ShowExample()
    {
      string duration = "3:51";
      string fileSize = "3.2MB";
      string artist = "Queen";
      string album = "Greatest Hits";
      string title = "Barcelona";
      string trackNr = "03";
      string year = "1973";
      string genre = "Pop";
      string date = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat); ;

      string filename = "barcelona.mp3";
      string rating = "8.2";

      string line1 = tbSortLeft.Text;
      string line2 = tbSortRight.Text;
      line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%track%", trackNr); line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%track%", trackNr);
      line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%filesize%", fileSize); line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%filesize%", fileSize);
      line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%artist%", artist); line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%artist%", artist);
      line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%album%", album); line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%album%", album);
      line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%title%", title); line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%title%", title);
      line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%year%", year); line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%year%", year);
      line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%date%", date); line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%date%", date);
      line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%filename%", filename); line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%filename%", filename);
      line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%rating%", rating); line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%rating%", rating);
      line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%duration%", duration); line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%duration%", duration);
      line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%genre%", genre); line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%genre%", genre);

      while (line1.Length < 25) line1 += " ";
      textBoxSample.Text = line1 + line2;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("musicfiles", "showSortButton", checkBoxShowSort.Checked);
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
      this.tbSortRight = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbSortLeft = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.comboBox1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label15 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label18 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label16 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label17 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxSample = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label19 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label20 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label21 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label38 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxShowSort = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.SuspendLayout();
      // 
      // tbSortRight
      // 
      this.tbSortRight.BorderColor = System.Drawing.Color.Empty;
      this.tbSortRight.Location = new System.Drawing.Point(345, 98);
      this.tbSortRight.Name = "tbSortRight";
      this.tbSortRight.Size = new System.Drawing.Size(71, 20);
      this.tbSortRight.TabIndex = 4;
      this.tbSortRight.Text = "%duration%";
      this.tbSortRight.TextChanged += new System.EventHandler(this.tbSortRight_TextChanged);
      // 
      // tbSortLeft
      // 
      this.tbSortLeft.BorderColor = System.Drawing.Color.Empty;
      this.tbSortLeft.Location = new System.Drawing.Point(96, 98);
      this.tbSortLeft.Name = "tbSortLeft";
      this.tbSortLeft.Size = new System.Drawing.Size(248, 20);
      this.tbSortLeft.TabIndex = 3;
      this.tbSortLeft.Text = "%track%. %artist% - %title%";
      this.tbSortLeft.TextChanged += new System.EventHandler(this.tbSortLeft_TextChanged);
      // 
      // comboBox1
      // 
      this.comboBox1.BorderColor = System.Drawing.Color.Empty;
      this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox1.FormattingEnabled = true;
      this.comboBox1.Location = new System.Drawing.Point(96, 48);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(88, 21);
      this.comboBox1.TabIndex = 0;
      this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Location = new System.Drawing.Point(0, 8);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(472, 400);
      this.tabControl1.TabIndex = 1;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.groupBox3);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(464, 374);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Music sort";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.checkBoxShowSort);
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
      this.groupBox3.Location = new System.Drawing.Point(16, 16);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(432, 282);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.label15);
      this.groupBox4.Controls.Add(this.label18);
      this.groupBox4.Controls.Add(this.label16);
      this.groupBox4.Controls.Add(this.label17);
      this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox4.Location = new System.Drawing.Point(96, 162);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(320, 104);
      this.groupBox4.TabIndex = 21;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Available Tags";
      // 
      // label15
      // 
      this.label15.AutoSize = true;
      this.label15.Location = new System.Drawing.Point(229, 24);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(83, 65);
      this.label15.TabIndex = 15;
      this.label15.Text = "filename\r\nfilesize of song\r\nduration of song\r\nsong rating\r\nfile date";
      // 
      // label18
      // 
      this.label18.AutoSize = true;
      this.label18.Location = new System.Drawing.Point(151, 24);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(71, 65);
      this.label18.TabIndex = 14;
      this.label18.Text = "%filename% =\r\n%filesize% =\r\n%duration% =\r\n%rating% =\r\n%date% =\r\n";
      this.label18.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // label16
      // 
      this.label16.AutoSize = true;
      this.label16.Location = new System.Drawing.Point(66, 24);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(76, 65);
      this.label16.TabIndex = 13;
      this.label16.Text = "name of artist\r\nsong title\r\nname of album\r\ntracknumber\r\nyear of song";
      // 
      // label17
      // 
      this.label17.AutoSize = true;
      this.label17.Location = new System.Drawing.Point(1, 24);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(60, 65);
      this.label17.TabIndex = 12;
      this.label17.Text = "%artist% =\r\n%title% =\r\n%album% =\r\n%track% =\r\n%year% =\r\n";
      this.label17.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // textBoxSample
      // 
      this.textBoxSample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxSample.BackColor = System.Drawing.SystemColors.ControlLight;
      this.textBoxSample.BorderColor = System.Drawing.Color.Empty;
      this.textBoxSample.Location = new System.Drawing.Point(96, 129);
      this.textBoxSample.Name = "textBoxSample";
      this.textBoxSample.ReadOnly = true;
      this.textBoxSample.Size = new System.Drawing.Size(320, 20);
      this.textBoxSample.TabIndex = 19;
      // 
      // label19
      // 
      this.label19.AutoSize = true;
      this.label19.Location = new System.Drawing.Point(16, 51);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(72, 13);
      this.label19.TabIndex = 18;
      this.label19.Text = "Sorting mode:";
      // 
      // label20
      // 
      this.label20.AutoSize = true;
      this.label20.Location = new System.Drawing.Point(93, 78);
      this.label20.Name = "label20";
      this.label20.Size = new System.Drawing.Size(217, 13);
      this.label20.TabIndex = 0;
      this.label20.Text = "Use blockquotes [ ] to specify optional fields.";
      // 
      // label21
      // 
      this.label21.AutoSize = true;
      this.label21.Location = new System.Drawing.Point(16, 132);
      this.label21.Name = "label21";
      this.label21.Size = new System.Drawing.Size(45, 13);
      this.label21.TabIndex = 5;
      this.label21.Text = "Sample:";
      // 
      // label38
      // 
      this.label38.AutoSize = true;
      this.label38.Location = new System.Drawing.Point(16, 101);
      this.label38.Name = "label38";
      this.label38.Size = new System.Drawing.Size(42, 13);
      this.label38.TabIndex = 3;
      this.label38.Text = "Format:";
      // 
      // checkBoxShowSort
      // 
      this.checkBoxShowSort.AutoSize = true;
      this.checkBoxShowSort.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxShowSort.Location = new System.Drawing.Point(19, 19);
      this.checkBoxShowSort.Name = "checkBoxShowSort";
      this.checkBoxShowSort.Size = new System.Drawing.Size(183, 17);
      this.checkBoxShowSort.TabIndex = 22;
      this.checkBoxShowSort.Text = "Show button to switch sort modes";
      this.checkBoxShowSort.UseVisualStyleBackColor = true;
      // 
      // MusicSort
      // 
      this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.Controls.Add(this.tabControl1);
      this.DoubleBuffered = true;
      this.Name = "MusicSort";
      this.Size = new System.Drawing.Size(472, 408);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
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