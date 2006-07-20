using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using DirectShowLib;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

using TvDatabase;

using TvControl;
using TvLibrary;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using DirectShowLib.BDA;

namespace SetupTv.Sections
{
  public partial class TvRecording : SectionSettings
  {
    public class CardInfo
    {
      public Card card;
      public CardInfo(Card newcard)
      {
        card = newcard;
      }
      public override string ToString()
      {
        return card.Name;
      }
    }
    private string[] formatString = { string.Empty, string.Empty };
    private class Example
    {
      public string Channel;
      public string Title;
      public string Episode;
      public string SeriesNum;
      public string EpisodeNum;
      public string EpisodePart;
      public DateTime StartDate;
      public DateTime EndDate;
      public string Genre;

      public Example(string channel, string title, string episode, string seriesNum, string episodeNum, string episodePart, string genre, DateTime startDate, DateTime endDate)
      {
        Channel = channel;
        Title = title;
        Episode = episode;
        SeriesNum = seriesNum;
        EpisodeNum = episodeNum;
        EpisodePart = episodePart;
        Genre = genre;
        StartDate = startDate;
        EndDate = endDate;
      }
    }


    private string ShowExample(string strInput, int recType)
    {
      string strName = string.Empty;
      string strDirectory = string.Empty;
      Example[] example = new Example[2];
      example[0] = new Example("ProSieben", "Philadelphia", "unknown", "unknown", "unknown", "unknown", "Drama", new DateTime(2005, 12, 23, 20, 15, 0), new DateTime(2005, 12, 23, 22, 45, 0));
      example[1] = new Example("ABC", "Friends", "Joey's Birthday", "4", "32", "part 1 of 1", "Comedy", new DateTime(2005, 12, 23, 20, 15, 0), new DateTime(2005, 12, 23, 20, 45, 0));
      string strDefaultName = String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}p{7}{8}",
                                  example[recType].Channel, example[recType].Title,
                                  example[recType].StartDate.Year, example[recType].StartDate.Month, example[recType].StartDate.Day,
                                  example[recType].StartDate.Hour,
                                  example[recType].StartDate.Minute,
                                  DateTime.Now.Minute, DateTime.Now.Second);
      if (strInput != string.Empty)
      {
        strInput = Utils.ReplaceTag(strInput, "%channel%", example[recType].Channel, "unknown");
        strInput = Utils.ReplaceTag(strInput, "%title%", example[recType].Title, "unknown");
        strInput = Utils.ReplaceTag(strInput, "%name%", example[recType].Episode, "unknown");
        strInput = Utils.ReplaceTag(strInput, "%series%", example[recType].SeriesNum, "unknown");
        strInput = Utils.ReplaceTag(strInput, "%episode%", example[recType].EpisodeNum, "unknown");
        strInput = Utils.ReplaceTag(strInput, "%part%", example[recType].EpisodePart, "unknown");
        strInput = Utils.ReplaceTag(strInput, "%date%", example[recType].StartDate.ToShortDateString(), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%start%", example[recType].StartDate.ToShortTimeString(), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%end%", example[recType].EndDate.ToShortTimeString(), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%genre%", example[recType].Genre, "unknown");
        strInput = Utils.ReplaceTag(strInput, "%startday%", example[recType].StartDate.Day.ToString(), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%startmonth%", example[recType].StartDate.Month.ToString(), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%startyear%", example[recType].StartDate.Year.ToString(), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%starthh%", example[recType].StartDate.Hour.ToString(), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%startmm%", example[recType].StartDate.Minute.ToString(), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%endday%", example[recType].EndDate.Day.ToString(), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%endmonth%", example[recType].EndDate.Month.ToString(), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%startyear%", example[recType].EndDate.Year.ToString(), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%endhh%", example[recType].EndDate.Hour.ToString(), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%endmm%", example[recType].EndDate.Minute.ToString(), "unknown");

        int index = strInput.LastIndexOf('\\');
        switch (index)
        {
          case -1:
            strName = strInput;
            break;
          case 0:
            strName = strInput.Substring(1);
            break;
          default:
            {
              strDirectory = "\\" + strInput.Substring(0, index);
              strName = strInput.Substring(index + 1);
            }
            break;
        }

        strDirectory = Utils.MakeDirectoryPath(strDirectory);
        strName = Utils.MakeFileName(strName);
      }
      if (strName == string.Empty)
        strName = strDefaultName;
      string strReturn = strDirectory;
      if (strDirectory != string.Empty)
        strReturn += "\\";
      strReturn += strName + ".dvr-ms";
      return strReturn;
    }

    public TvRecording()
      : this("Recording")
    {
    }

    public TvRecording(string name)
      : base(name)
    {
      InitializeComponent();
    }

    private void textBoxFormat_TextChanged(object sender, EventArgs e)
    {
      formatString[comboBoxMovies.SelectedIndex] = textBoxFormat.Text;
      textBoxSample.Text = ShowExample(textBoxFormat.Text, comboBoxMovies.SelectedIndex);
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {

      textBoxFormat.Text = formatString[comboBoxMovies.SelectedIndex];
    }

    private void comboBoxDrive_SelectedIndexChanged(object sender, EventArgs e)
    {
      UpdateDriveInfo(false);
    }

    private void trackBarDisk_Scroll(object sender, EventArgs e)
    {
      UpdateDriveInfo(true);
    }

    private void textBoxFormat_KeyPress(object sender, KeyPressEventArgs e)
    {
      if ((e.KeyChar == '/') || (e.KeyChar == ':') || (e.KeyChar == '*') ||
        (e.KeyChar == '?') || (e.KeyChar == '\"') || (e.KeyChar == '<') ||
        (e.KeyChar == '>') || (e.KeyChar == '|'))
      {
        e.Handled = true;
      }
    }
    void UpdateDriveInfo(bool save)
    {
      string drive = (string)comboBoxDrive.SelectedItem;
      ulong freeSpace = Utils.GetFreeDiskSpace(drive);
      long totalSpace = Utils.GetDiskSize(drive);

      labelFreeDiskspace.Text = Utils.GetSize((long)freeSpace);
      labelTotalDiskSpace.Text = Utils.GetSize((long)totalSpace);
      if (labelTotalDiskSpace.Text == "0")
        labelTotalDiskSpace.Text = "Not available - WMI service not available";

      if (save)
      {
        float percent = (float)trackBarDisk.Value;
        percent /= 100f;
        float quota = percent * ((float)totalSpace);
        if (quota < (52428800f)) //50MB
        {
          quota = (52428800f);//50MB
          percent = (quota / ((float)totalSpace)) * 100f;
          try
          {
            trackBarDisk.Value = (int)percent;
          }
          catch (ArgumentOutOfRangeException)
          {
            trackBarDisk.Value = 0;
          }
        }
        labelQuota.Text = Utils.GetSize((long)quota);

        long longQuota = (long)quota;
        longQuota /= 1024; // kbyte
        TvBusinessLayer layer = new TvBusinessLayer();
        layer.GetSetting("freediskspace" + drive[0].ToString()).Value = longQuota.ToString();
        DatabaseManager.Instance.SaveChanges();
        //using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        //{
        //  long longQuota = (long)quota;
        //  longQuota /= 1024; // kbyte
        //  xmlwriter.SetValue("freediskspace", drive[0].ToString(), longQuota.ToString());
        //}
      }
      else
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        string quotaText = layer.GetSetting("freediskspace" + drive[0].ToString(), "51200").Value;
        //using (MediaPortal.Profile.Settings xmlReader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          //string quotaText = xmlReader.GetValueAsString("freediskspace", drive[0].ToString(), "51200");
          float quota = (float)Int32.Parse(quotaText);
          if (quota < 51200) quota = 51200f;
          quota *= 1024f;//kbyte
          labelQuota.Text = Utils.GetSize((long)quota);

          float percent = (quota / ((float)totalSpace)) * 100f;
          try
          {
            trackBarDisk.Value = (int)percent;
          }
          catch (ArgumentOutOfRangeException)
          {
            trackBarDisk.Value = 0;
          }
        }
      }
    }

    private void textBoxPreInterval_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void textBoxPostInterval_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }
    public override void LoadSettings()
    {
      textBoxPreInterval.Text = "5";
      textBoxPostInterval.Text = "5";
      checkBoxAutoDelete.Checked = true;
      checkBoxAddToDatabase.Checked = false;
      formatString[0] = "";
      formatString[1] = "";

      TvBusinessLayer layer = new TvBusinessLayer();
      textBoxPreInterval.Text = layer.GetSetting("preRecordInterval", "5").Value;
      textBoxPostInterval.Text = layer.GetSetting("postRecordInterval", "5").Value;
      formatString[0] = layer.GetSetting("moviesformat", "").Value;
      formatString[1] = layer.GetSetting("seriesformat", "").Value;

      checkBoxComSkipEnabled.Checked = (layer.GetSetting("comskipEnabled", "yes").Value == "yes");
      textBoxComSkip.Text = layer.GetSetting("comskipLocation", "").Value;
      /*using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        startTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("capture", "prerecord", 5));
        endTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("capture", "postrecord", 5));
        cbDeleteWatchedShows.Checked = xmlreader.GetValueAsBool("capture", "deletewatchedshows", false);
        cbAddRecordingsToMovie.Checked = xmlreader.GetValueAsBool("capture", "addrecordingstomoviedatabase", true);
        formatString[0] = xmlreader.GetValueAsString("capture", "moviesformat", string.Empty);
        formatString[1] = xmlreader.GetValueAsString("capture", "seriesformat", string.Empty);
      }*/
      comboBoxMovies.SelectedIndex = 0;
      textBoxSample.Text = ShowExample(formatString[comboBoxMovies.SelectedIndex], comboBoxMovies.SelectedIndex);


      comboBoxDrive.Items.Clear();
      for (char drive = 'a'; drive <= 'z'; drive++)
      {
        string driveLetter = String.Format("{0}:", drive);
        if (Utils.getDriveType(driveLetter) == 3)
        {
          comboBoxDrive.Items.Add(driveLetter);
        }
      }
      comboBoxDrive.SelectedIndex = 0;
      UpdateDriveInfo(false);
    }

    public override void SaveSettings()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      layer.GetSetting("preRecordInterval", "5").Value = textBoxPreInterval.Text;
      layer.GetSetting("postRecordInterval", "5").Value = textBoxPostInterval.Text;
      layer.GetSetting("moviesformat", "").Value = formatString[0];
      layer.GetSetting("seriesformat", "").Value = formatString[1];

      if (checkBoxComSkipEnabled.Checked)
        layer.GetSetting("comskipEnabled", "yes").Value = "yes";
      else
        layer.GetSetting("comskipEnabled", "yes").Value = "no";

      layer.GetSetting("comskipLocation", "").Value = textBoxComSkip.Text;
      DatabaseManager.Instance.SaveChanges();
      /*
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValue("capture", "prerecord", startTextBox.Text);
        xmlwriter.SetValue("capture", "postrecord", endTextBox.Text);

        xmlwriter.SetValueAsBool("capture", "deletewatchedshows", cbDeleteWatchedShows.Checked);
        xmlwriter.SetValueAsBool("capture", "addrecordingstomoviedatabase", cbAddRecordingsToMovie.Checked);

        xmlwriter.SetValue("capture", "moviesformat", formatString[0]);
        xmlwriter.SetValue("capture", "seriesformat", formatString[1]);
      }
      UpdateDriveInfo(true);
      */
    }

    private void comboBoxCards_SelectedIndexChanged(object sender, EventArgs e)
    {
      CardInfo info = (CardInfo)comboBoxCards.SelectedItem;
      textBoxFolder.Text = info.card.RecordingFolder;
      if (textBoxFolder.Text == "")
      {
        textBoxFolder.Text = System.IO.Directory.GetCurrentDirectory();
      }
    }

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.SelectedPath = textBoxFolder.Text;
      dlg.Description = "Specify recording folder";
      dlg.ShowNewFolderButton = true;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        textBoxFolder.Text = dlg.SelectedPath;
        CardInfo info = (CardInfo)comboBoxCards.SelectedItem;
        info.card.RecordingFolder = textBoxFolder.Text;
        DatabaseManager.Instance.SaveChanges();
      }
    }

    public override void OnSectionActivated()
    {
      comboBoxCards.Items.Clear();
      EntityList<Card> cards = DatabaseManager.Instance.GetEntities<Card>();
      foreach (Card card in cards)
      {
        comboBoxCards.Items.Add(new CardInfo(card));
      }
      if (comboBoxCards.Items.Count > 0)
        comboBoxCards.SelectedIndex = 0;
      base.OnSectionActivated();
    }

    private void checkBoxComSkipEnabled_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void buttonLocateComSkip_Click(object sender, EventArgs e)
    {
      openFileDialog1.CheckFileExists = true;
      openFileDialog1.CheckPathExists = true;
      openFileDialog1.DefaultExt = ".bat";
      openFileDialog1.Filter = "*.bat";
      openFileDialog1.RestoreDirectory = true;
      openFileDialog1.Title = "Select batch file to run comskip/comclean";
      DialogResult result = openFileDialog1.ShowDialog(this);
      if (result != DialogResult.OK) return;
      textBoxComSkip.Text = openFileDialog1.FileName;
    }

    private void label18_Click(object sender, EventArgs e)
    {

    }

  }
}