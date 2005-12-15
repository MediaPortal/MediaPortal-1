/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Yeti.MMedia;
using Yeti.MMedia.Mp3;
using WaveLib;
using Yeti.Lame;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.Configuration.Sections
{
  public class MusicSort : MediaPortal.Configuration.SectionSettings
  {
    private System.ComponentModel.IContainer components = null;

    private GroupBox groupBox1;
    private Label label1;
    private ComboBox comboBox1;
    private TextBox tbSortRight;
    private TextBox tbSortLeft;
    private Label label2;
    private Label labelSample;
    private Label label3;
    private GroupBox groupBox2;
    private Label label6;
    private Label label5;
    private Label label4;
    private Label label11;
    private Label label10;
    private Label label9;
    private Label label8;
    private Label label7;


    const string defaultTrackTag = "[%track%. ][%artist% - ][%title%]";
    const string albumTrackTag = "[%track%. ][%artist% - ][%title%]"; 
    string[] sortModes = { "Name", "Date", "Size", "Track", "Duration", "Title", "Artist", "Album", "Filename", "Rating" };
    string[] defaultSortTags1 = { defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultTrackTag, albumTrackTag, defaultTrackTag, defaultTrackTag };
    string[] defaultSortTags2 = { "%duration%", "%year%", "%filesize%", "%duration%", "%duration%", "%duration%", "%duration%", "%duration%", "%filesize%", "%rating%" };

    string[] sortTags1 = new string[20];
    private Label label12;
    private Label label13;
    private Label label14;
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
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
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
    void ShowExample()
    {
      string duration = "3:51";
      string fileSize = "3.2MB";
      string artist = "Queen";
      string album = "Greatest hits";
      string title = "Barcelona";
      string trackNr = "03";
      string year = "1973";
      string date = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat); ;

      string filename = "barcelona.mp3";
      string rating = "8.2";

      string line1 = tbSortLeft.Text;
      string line2 = tbSortRight.Text;
      line1 = Utils.ReplaceTag(line1, "%track%", trackNr); line2 = Utils.ReplaceTag(line2, "%track%", trackNr);
      line1 = Utils.ReplaceTag(line1, "%filesize%", fileSize); line2 = Utils.ReplaceTag(line2, "%filesize%", fileSize);
      line1 = Utils.ReplaceTag(line1, "%artist%", artist); line2 = Utils.ReplaceTag(line2, "%artist%", artist);
      line1 = Utils.ReplaceTag(line1, "%album%", album); line2 = Utils.ReplaceTag(line2, "%album%", album);
      line1 = Utils.ReplaceTag(line1, "%title%", title); line2 = Utils.ReplaceTag(line2, "%title%", title);
      line1 = Utils.ReplaceTag(line1, "%year%", year); line2 = Utils.ReplaceTag(line2, "%year%", year);
      line1 = Utils.ReplaceTag(line1, "%date%", date); line2 = Utils.ReplaceTag(line2, "%date%", date);
      line1 = Utils.ReplaceTag(line1, "%filename%", filename); line2 = Utils.ReplaceTag(line2, "%filename%", filename);
      line1 = Utils.ReplaceTag(line1, "%rating%", rating); line2 = Utils.ReplaceTag(line2, "%rating%", rating);
      line1 = Utils.ReplaceTag(line1, "%duration%", duration); line2 = Utils.ReplaceTag(line2, "%duration%", duration);

      while (line1.Length < 25) line1 += " ";
      labelSample.Text = line1+line2;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.label12 = new System.Windows.Forms.Label();
      this.label11 = new System.Windows.Forms.Label();
      this.label10 = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.labelSample = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.tbSortRight = new System.Windows.Forms.TextBox();
      this.tbSortLeft = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.comboBox1 = new System.Windows.Forms.ComboBox();
      this.label13 = new System.Windows.Forms.Label();
      this.label14 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.groupBox2);
      this.groupBox1.Controls.Add(this.labelSample);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.tbSortRight);
      this.groupBox1.Controls.Add(this.tbSortLeft);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.comboBox1);
      this.groupBox1.Location = new System.Drawing.Point(3, 3);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(466, 392);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Music display mode:";
      this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.label14);
      this.groupBox2.Controls.Add(this.label13);
      this.groupBox2.Controls.Add(this.label12);
      this.groupBox2.Controls.Add(this.label11);
      this.groupBox2.Controls.Add(this.label10);
      this.groupBox2.Controls.Add(this.label9);
      this.groupBox2.Controls.Add(this.label8);
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Controls.Add(this.label6);
      this.groupBox2.Controls.Add(this.label5);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Location = new System.Drawing.Point(26, 190);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(420, 186);
      this.groupBox2.TabIndex = 7;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Tags:";
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(12, 126);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(211, 13);
      this.label12.TabIndex = 8;
      this.label12.Text = "Use blockquotes [] to specify optional fields";
      this.label12.Click += new System.EventHandler(this.label12_Click);
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(12, 83);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(113, 13);
      this.label11.TabIndex = 7;
      this.label11.Text = "%year% = year of song";
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(250, 54);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(113, 13);
      this.label10.TabIndex = 6;
      this.label10.Text = "%rating% = song rating";
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(12, 70);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(156, 13);
      this.label9.TabIndex = 5;
      this.label9.Text = "%track% = tracknumber of song";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(250, 28);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(135, 13);
      this.label8.TabIndex = 4;
      this.label8.Text = "%filesize% = filesize of song";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(250, 41);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(149, 13);
      this.label7.TabIndex = 3;
      this.label7.Text = "%duration% = duration of song";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(12, 57);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(132, 13);
      this.label6.TabIndex = 2;
      this.label6.Text = "%album% = name of album";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(12, 44);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(90, 13);
      this.label5.TabIndex = 1;
      this.label5.Text = "%title%= song title";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(12, 28);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(120, 13);
      this.label4.TabIndex = 0;
      this.label4.Text = "%artist% = name of artist";
      // 
      // labelSample
      // 
      this.labelSample.AutoSize = true;
      this.labelSample.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelSample.Location = new System.Drawing.Point(35, 161);
      this.labelSample.Name = "labelSample";
      this.labelSample.Size = new System.Drawing.Size(51, 16);
      this.labelSample.TabIndex = 6;
      this.labelSample.Text = "label4";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(23, 141);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(50, 13);
      this.label3.TabIndex = 5;
      this.label3.Text = "Example:";
      // 
      // tbSortRight
      // 
      this.tbSortRight.Location = new System.Drawing.Point(300, 105);
      this.tbSortRight.Name = "tbSortRight";
      this.tbSortRight.Size = new System.Drawing.Size(111, 20);
      this.tbSortRight.TabIndex = 4;
      this.tbSortRight.Text = "%duration%";
      this.tbSortRight.TextChanged += new System.EventHandler(this.tbSortRight_TextChanged);
      // 
      // tbSortLeft
      // 
      this.tbSortLeft.Location = new System.Drawing.Point(38, 105);
      this.tbSortLeft.Name = "tbSortLeft";
      this.tbSortLeft.Size = new System.Drawing.Size(175, 20);
      this.tbSortLeft.TabIndex = 3;
      this.tbSortLeft.Text = "%track%. %artist% - %title%";
      this.tbSortLeft.TextChanged += new System.EventHandler(this.tbSortLeft_TextChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(23, 89);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(76, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Display format:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(23, 29);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(72, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Sorting mode:";
      // 
      // comboBox1
      // 
      this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox1.FormattingEnabled = true;
      this.comboBox1.Location = new System.Drawing.Point(38, 46);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(263, 21);
      this.comboBox1.TabIndex = 0;
      this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(12, 96);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(113, 13);
      this.label13.TabIndex = 9;
      this.label13.Text = "%filename% = filename";
      // 
      // label14
      // 
      this.label14.AutoSize = true;
      this.label14.Location = new System.Drawing.Point(250, 67);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(93, 13);
      this.label14.TabIndex = 10;
      this.label14.Text = "%date% = file date";
      // 
      // MusicSort
      // 
      this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.Controls.Add(this.groupBox1);
      this.DoubleBuffered = true;
      this.Name = "MusicSort";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion


    private void groupBox1_Enter(object sender, EventArgs e)
    {

    }

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

    private void label12_Click(object sender, EventArgs e)
    {

    }
  }
}