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

#region Usings

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using DirectShowLib;
using DirectShowLib.BDA;

using Gentle.Framework;

using TvDatabase;
using TvControl;
using TvLibrary;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;

#endregion

namespace SetupTv.Sections
{
  public partial class TvRecording : SectionSettings
  {
    #region CardInfo class

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

    #endregion

    #region Example Format class

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
        strInput = Utils.ReplaceTag(strInput, "%startday%", example[recType].StartDate.ToString("dd"), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%startmonth%", example[recType].StartDate.ToString("MM"), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%startyear%", example[recType].StartDate.ToString("yyyy"), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%starthh%", example[recType].StartDate.ToString("HH"), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%startmm%", example[recType].StartDate.ToString("mm"), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%endday%", example[recType].EndDate.ToString("dd"), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%endmonth%", example[recType].EndDate.ToString("MM"), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%endyear%", example[recType].EndDate.ToString("yyyy"), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%endhh%", example[recType].EndDate.ToString("HH"), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%endmm%", example[recType].EndDate.ToString("mm"), "unknown");

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
      strReturn += strName + ".mpg";
      return strReturn;
    }

    #endregion

    #region Vars

    bool _needRestart = false;

    #endregion

    #region Constructors

    public TvRecording()
      : this("Recording settings")
    {
    }

    public TvRecording(string name)
      : base(name)
    {
      InitializeComponent();
    }

    #endregion

    #region Serialization

    public override void LoadSettings()
    {
      numericUpDownPreRec.Value = 5;
      numericUpDownPostRec.Value = 5;
      TvBusinessLayer layer = new TvBusinessLayer();
      checkBoxAutoDelete.Checked = (layer.GetSetting("autodeletewatchedrecordings", "no").Value == "yes");
      checkBoxCreateTagInfoXML.Checked = true; // (layer.GetSetting("createtaginfoxml", "yes").Value == "yes");
      checkboxSchedulerPriority.Checked = (layer.GetSetting("scheduleroverlivetv", "yes").Value == "yes");
      formatString[0] = "";
      formatString[1] = "";

      numericUpDownPreRec.Value = int.Parse(layer.GetSetting("preRecordInterval", "5").Value);
      numericUpDownPostRec.Value = int.Parse(layer.GetSetting("postRecordInterval", "5").Value);
      formatString[0] = layer.GetSetting("moviesformat", @"%title% - %channel% - %date%").Value;
      formatString[1] = layer.GetSetting("seriesformat", @"%title% - %channel%\%title% - %episode% - %date% - %start%").Value;

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

      enableDiskQuota.Checked = (layer.GetSetting("diskQuotaEnabled", "False").Value == "True");
      enableDiskQuotaControls();

      LoadComboBoxDrive();
    }

    public override void SaveSettings()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting;
      setting = layer.GetSetting("preRecordInterval", "5");
      setting.Value = numericUpDownPreRec.Value.ToString();
      setting.Persist();
      setting = layer.GetSetting("postRecordInterval", "5");
      setting.Value = numericUpDownPostRec.Value.ToString();
      setting.Persist();
      setting = layer.GetSetting("moviesformat", "");
      setting.Value = formatString[0];
      setting.Persist();
      setting = layer.GetSetting("seriesformat", "");
      setting.Value = formatString[1];
      setting.Persist();

      setting = layer.GetSetting("autodeletewatchedrecordings", "no");
      if (checkBoxAutoDelete.Checked)
      {
        setting.Value = "yes";
      }
      else
      {
        setting.Value = "no";
      }
      setting.Persist();

      setting = layer.GetSetting("createtaginfoxml", "yes");
      if (checkBoxCreateTagInfoXML.Checked)
      {
        setting.Value = "yes";
      }
      else
      {
        setting.Value = "no";
      }
      setting.Persist();

      setting = layer.GetSetting("scheduleroverlivetv", "yes");
      if (checkboxSchedulerPriority.Checked)
      {
        setting.Value = "yes";
      }
      else
      {
        setting.Value = "no";
      }
      setting.Persist();

      UpdateDriveInfo(true);
    }

    #endregion

    #region GUI-Events

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

    private void textBoxFormat_KeyPress(object sender, KeyPressEventArgs e)
    {
      if ((e.KeyChar == '/') || (e.KeyChar == ':') || (e.KeyChar == '*') ||
        (e.KeyChar == '?') || (e.KeyChar == '\"') || (e.KeyChar == '<') ||
        (e.KeyChar == '>') || (e.KeyChar == '|'))
      {
        e.Handled = true;
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

    private void comboBoxCards_SelectedIndexChanged(object sender, EventArgs e)
    {
      CardInfo info = (CardInfo)comboBoxCards.SelectedItem;
      textBoxFolder.Text = info.card.RecordingFolder;
      textBoxTimeShiftFolder.Text = info.card.TimeShiftFolder;
      if (textBoxFolder.Text == "")
      {
        textBoxFolder.Text = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        if (!Directory.Exists(textBoxFolder.Text))
          Directory.CreateDirectory(textBoxFolder.Text);
      }
      if (textBoxTimeShiftFolder.Text == "")
      {
        textBoxTimeShiftFolder.Text = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\timeshiftbuffer", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        if (!Directory.Exists(textBoxTimeShiftFolder.Text))
          Directory.CreateDirectory(textBoxTimeShiftFolder.Text);
      }
      switch (info.card.RecordingFormat)
      {
        case 0:
          comboBoxRecordingFormat.SelectedIndex = 0;
          break;
        case 1:
          comboBoxRecordingFormat.SelectedIndex = 1;
          break;
      }
    }

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.SelectedPath = textBoxFolder.Text;
      dlg.Description = "Specify recording folder";
      dlg.ShowNewFolderButton = true;
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        textBoxFolder.Text = dlg.SelectedPath;
        CardInfo info = (CardInfo)comboBoxCards.SelectedItem;
        if (info.card.RecordingFolder != textBoxFolder.Text)
        {
          _needRestart = true;
          info.card.RecordingFolder = textBoxFolder.Text;
          info.card.Persist();
          LoadComboBoxDrive();
        }
      }
    }

    public override void OnSectionActivated()
    {
      MatroskaTagHandler.OnTagLookupCompleted += new MatroskaTagHandler.TagLookupSuccessful(OnLookupCompleted);

      _needRestart = false;
      comboBoxCards.Items.Clear();
      IList cards = Card.ListAll();
      foreach (Card card in cards)
      {
        comboBoxCards.Items.Add(new CardInfo(card));
      }
      if (comboBoxCards.Items.Count > 0)
        comboBoxCards.SelectedIndex = 0;
      UpdateDriveInfo(false);

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();

      MatroskaTagHandler.OnTagLookupCompleted -= new MatroskaTagHandler.TagLookupSuccessful(OnLookupCompleted);

      SaveSettings();
      if (_needRestart)
      {
        RemoteControl.Instance.ClearCache();
        RemoteControl.Instance.Restart();
      }
    }

    private void textBoxFolder_TextChanged(object sender, EventArgs e)
    {
      CardInfo info = (CardInfo)comboBoxCards.SelectedItem;
      if (info.card.RecordingFolder != textBoxFolder.Text)
      {
        info.card.RecordingFolder = textBoxFolder.Text;
        info.card.Persist();
        _needRestart = true;
        LoadComboBoxDrive();
      }
    }

    private void buttonTimeShiftBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.SelectedPath = textBoxTimeShiftFolder.Text;
      dlg.Description = "Specify timeshift folder";
      dlg.ShowNewFolderButton = true;
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        textBoxTimeShiftFolder.Text = dlg.SelectedPath;
        CardInfo info = (CardInfo)comboBoxCards.SelectedItem;
        if (info.card.RecordingFolder != textBoxFolder.Text)
        {
          info.card.RecordingFolder = textBoxFolder.Text;
          info.card.TimeShiftFolder = textBoxTimeShiftFolder.Text;
          info.card.Persist();
          _needRestart = true;
          LoadComboBoxDrive();
        }
      }
    }

    private void textBoxTimeShiftFolder_TextChanged(object sender, EventArgs e)
    {
      CardInfo info = (CardInfo)comboBoxCards.SelectedItem;
      if (info.card.TimeShiftFolder != textBoxTimeShiftFolder.Text)
      {
        info.card.TimeShiftFolder = textBoxTimeShiftFolder.Text;
        info.card.Persist();
        _needRestart = true;
      }
    }

    private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
    {
      CardInfo info = (CardInfo)comboBoxCards.SelectedItem;
      if (info.card.RecordingFormat != comboBoxRecordingFormat.SelectedIndex)
      {
        info.card.RecordingFormat = comboBoxRecordingFormat.SelectedIndex;
        info.card.Persist();
        _needRestart = true;
      }
    }

    private void mpNumericTextBoxDiskQuota_Leave(object sender, EventArgs e)
    {
      UpdateDriveInfo(true);
    }

    private void enableDiskQuota_CheckedChanged(object sender, EventArgs e)
    {
      enableDiskQuotaControls();

      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("diskQuotaEnabled");
      setting.Value = ((CheckBox)sender).Checked.ToString();
      setting.Persist();
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (tabControl1.SelectedTab.Name == "tpRecordImport")
      {
        LoadDbImportSettings();
      }
    }

    #endregion

    #region Quota handling

    private void UpdateDriveInfo(bool save)
    {
      if (comboBoxDrive.SelectedItem == null) return;
      string drive = (string)comboBoxDrive.SelectedItem;
      ulong freeSpace = Utils.GetFreeDiskSpace(drive);
      long totalSpace = Utils.GetDiskSize(drive);

      labelFreeDiskspace.Text = Utils.GetSize((long)freeSpace);
      labelTotalDiskSpace.Text = Utils.GetSize((long)totalSpace);
      if (labelTotalDiskSpace.Text == "0")
        labelTotalDiskSpace.Text = "Not available - WMI service not available";
      if (save)
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Setting setting = layer.GetSetting("freediskspace" + drive[0].ToString());
        if (mpNumericTextBoxDiskQuota.Value < 500)
          mpNumericTextBoxDiskQuota.Value = 500;
        long quota = mpNumericTextBoxDiskQuota.Value * 1024;
        setting.Value = quota.ToString();
        setting.Persist();
      }
      else
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Setting setting = layer.GetSetting("freediskspace" + drive[0].ToString());
        try
        {
          long quota = Int64.Parse(setting.Value);
          mpNumericTextBoxDiskQuota.Value = (int)quota / 1024;
        }
        catch (Exception)
        {
          mpNumericTextBoxDiskQuota.Value = 0;
        }
        if (mpNumericTextBoxDiskQuota.Value < 500)
          mpNumericTextBoxDiskQuota.Value = 500;
      }
    }

    private void enableDiskQuotaControls()
    {
      if (enableDiskQuota.Checked)
      {
        //enable all controls
        label9.Enabled = true;
        comboBoxDrive.Enabled = true;
        label10.Enabled = true;
        labelTotalDiskSpace.Enabled = true;
        label11.Enabled = true;
        labelFreeDiskspace.Enabled = true;
        label14.Enabled = true;
        mpNumericTextBoxDiskQuota.Enabled = true;
        mpLabel5.Enabled = true;
      }
      else
      {
        //disable all controls
        label9.Enabled = false;
        comboBoxDrive.Enabled = false;
        label10.Enabled = false;
        labelTotalDiskSpace.Enabled = false;
        label11.Enabled = false;
        labelFreeDiskspace.Enabled = false;
        label14.Enabled = false;
        mpNumericTextBoxDiskQuota.Enabled = false;
        mpLabel5.Enabled = false;
      }
    }

    private void LoadComboBoxDrive()
    {
      comboBoxDrive.Items.Clear();
      IList cards = Card.ListAll();
      foreach (Card card in cards)
      {
        if (card.RecordingFolder.Length > 0)
        {
          string driveLetter = String.Format("{0}:", card.RecordingFolder[0]);
          if (Utils.getDriveType(driveLetter) == 3)
          {
            comboBoxDrive.Items.Add(driveLetter);
          }
        }
      }
      if (comboBoxDrive.Items.Count > 0)
      {
        comboBoxDrive.SelectedIndex = 0;
        UpdateDriveInfo(false);
      }
    }

    #endregion

    #region DB Imports

    #region RecordSorter

    // Create a sorter that implements the IComparer interface.
    public class RecordSorter : IComparer
    {
      // Compare the length of the strings, or the strings
      // themselves, if they are the same length.
      public int Compare(object x, object y)
      {
        int result = 0;
        try
        {
          TreeNode tx = x as TreeNode;
          TreeNode ty = y as TreeNode;

          result = string.Compare(tx.Text, ty.Text, System.StringComparison.CurrentCulture);
        }
        catch (Exception)
        {
        }

        return result;
      }
    }

    #endregion

    #region Delegates

    protected delegate void MethodTreeViewTags(Dictionary<string, MatroskaTagInfo> FoundTags);

    #endregion

    #region Fields

    private string fCurrentImportPath;
    public string CurrentImportPath
    {
      get { return fCurrentImportPath; }
      set
      {
        fCurrentImportPath = value;
      }
    }

    private List<TreeNode> tvDbRecs = new List<TreeNode>();

    #endregion

    #region Settings

    private void LoadDbImportSettings()
    {
      cbRecPaths.Items.Clear();
      GetRecordingsFromDb();
      try
      {
        IList allCards = Card.ListAll();
        foreach (Card tvCard in allCards)
        {
          if (!string.IsNullOrEmpty(tvCard.RecordingFolder) && !cbRecPaths.Items.Contains(tvCard.RecordingFolder))
            cbRecPaths.Items.Add(tvCard.RecordingFolder);
        }
        if (cbRecPaths.Items.Count > 0)
        {
          cbRecPaths.SelectedIndex = 0;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Error gathering recording folders of all tv cards: \n{0}", ex.Message));
      }
    }

    private void cbRecPaths_SelectedIndexChanged(object sender, EventArgs e)
    {
      try
      {
        CurrentImportPath = cbRecPaths.Text;
        GetTagFiles();
      }
      catch (Exception ex2)
      {
        MessageBox.Show(string.Format("Error gathering matroska tags: \n{0}", ex2.Message));
      }
    }

    private void tvTagRecs_AfterCheck(object sender, TreeViewEventArgs e)
    {
      SetImportButton();
    }

    private void SetImportButton()
    {
      bool shouldImportSomething = false;
      if (tvTagRecs.Nodes.Count > 0)
      {
        foreach (TreeNode rec in tvTagRecs.Nodes)
        {
          if (rec.Checked)
          {
            shouldImportSomething = true;
            break;
          }
        }
      }
      btnImport.Enabled = shouldImportSomething;
    }

    #endregion

    #region Recording retrieval

    private void GetRecordingsFromDb()
    {
      try
      {
        tvDbRecs.Clear();
        IList recordings = Recording.ListAll();
        foreach (Recording rec in recordings)
        {
          TreeNode RecNode = BuildNodeFromRecording(rec);
          if (RecNode != null)
            tvDbRecs.Add(RecNode);
        }
      }
      catch (Exception ex1)
      {
        MessageBox.Show(string.Format("Error retrieving recordings from database: \n{0}", ex1.Message));
      }
    }

    #endregion

    #region Tag retrieval

    private void GetTagFiles()
    {
      try
      {
        btnImport.Enabled = false;
        Dictionary<string, MatroskaTagInfo> importTags = new Dictionary<string, MatroskaTagInfo>();
        Thread lookupThread = new Thread(new ParameterizedThreadStart(MatroskaTagHandler.GetAllMatroskaTags));
        lookupThread.Name = "MatroskaTagHandler";
        lookupThread.Start((object)CurrentImportPath);
        lookupThread.IsBackground = true;
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void OnLookupCompleted(Dictionary<string, MatroskaTagInfo> FoundTags)
    {
      try
      {
        Invoke(new MethodTreeViewTags(AddTagFiles), new object[] { FoundTags });
      }
      catch (Exception) { }
    }

    /// <summary>
    /// Invoke method from MethodTreeViewTags delegate!!!
    /// </summary>
    /// <param name="FoundTags"></param>
    private void AddTagFiles(Dictionary<string, MatroskaTagInfo> FoundTags)
    {
      try
      {
        tvTagRecs.Nodes.Clear();
        tvTagRecs.BeginUpdate();
        foreach (KeyValuePair<string, MatroskaTagInfo> kvp in FoundTags)
        {
          Recording TagRec = BuildRecordingFromTag(kvp.Key, kvp.Value);
          if (TagRec != null)
          {
            TreeNode TagNode = BuildNodeFromRecording(TagRec);
            if (TagNode != null)
            {
              bool RecFileFound = false;
              foreach (TreeNode dbRec in tvDbRecs)
              {
                Recording currentDbRec = dbRec.Tag as Recording;
                if (currentDbRec != null)
                {
                  if (Path.GetFileNameWithoutExtension(currentDbRec.FileName) == Path.GetFileNameWithoutExtension(TagRec.FileName))
                  {
                    RecFileFound = true;
                    break;
                  }
                }
              }
              if (!RecFileFound)
              {
                // only add those tags which specify a still valid filename
                if (File.Exists(TagRec.FileName))
                {
                  if (TagRec.IdChannel == -1)
                  {
                    TagNode.ForeColor = SystemColors.GrayText;
                    TagNode.Checked = false;
                  }

                  tvTagRecs.Nodes.Add(TagNode);
                }
              }
            }
          }
        }
        //tvTagRecs.TreeViewNodeSorter = new RecordSorter();
        //try
        //{
        //  tvTagRecs.Sort();
        //}
        //catch (Exception ex)
        //{
        //  MessageBox.Show(string.Format("Error sorting tag recordings: \n{0}", ex.Message));
        //}
        tvTagRecs.EndUpdate();
        SetImportButton();
      }
      catch (Exception)
      {
        // just in case the GUI controls could be null due to timing problems on thread callback
        if (btnImport != null)
          btnImport.Enabled = false;
      }
    }

    #endregion

    #region Visualisation

    private TreeNode BuildNodeFromRecording(Recording aRec)
    {
      try
      {
        Channel lookupChannel = null;
        string channelId = "unknown";
        string channelName = "unknown";
        string startTime = SqlDateTime.MinValue.Value == aRec.StartTime ? "unknown" : aRec.StartTime.ToString();
        string endTime = SqlDateTime.MinValue.Value == aRec.EndTime ? "unknown" : aRec.EndTime.ToString();
        try
        {
          lookupChannel = (Channel)aRec.ReferencedChannel();
          if (lookupChannel != null)
          {
            channelName = lookupChannel.DisplayName;
            channelId = lookupChannel.IdChannel.ToString();
          }
        }
        catch (Exception)
        {
        }

        //TreeNode[] subitems = new TreeNode[] { 
        //                                       new TreeNode("Channel name: " + channelName), 
        //                                       new TreeNode("Channel ID: " + channelId), 
        //                                       new TreeNode("Genre: " + aRec.Genre), 
        //                                       new TreeNode("Description: " + aRec.Description), 
        //                                       new TreeNode("Start time: " + startTime), 
        //                                       new TreeNode("End time: " + endTime), 
        //                                       new TreeNode("Server ID: " + aRec.IdServer)
        //                                     };
        // /!\ TODO: need some code to disable the checkboxes for subnodes
        //foreach (TreeNode subItem in subitems)
        //{
        //  subItem.StateImageIndex = -1;
        //}
        //TreeNode recItem = new TreeNode(aRec.Title, subitems);

        string NodeTitle = string.Empty;
        if (startTime != "unknown" && endTime != "unknown")
          NodeTitle = string.Format("Title: {0} / Channel: {1} / Time: {2}-{3}", aRec.Title, channelName, startTime, endTime);
        else
          NodeTitle = string.Format("Title: {0} / Channel: {1} / Time: {2}", aRec.Title, channelName, startTime);

        TreeNode recItem = new TreeNode(NodeTitle);
        recItem.Tag = aRec;
        recItem.Checked = true;
        return recItem;
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Could not build TreeNode from recording: {0}\n{1}", aRec.Title, ex.Message));
        return null;
      }
    }

    #endregion

    #region Tag to recording conversion

    private Recording BuildRecordingFromTag(string aFileName, MatroskaTagInfo aTag)
    {
      Recording tagRec = null;
      try
      {
        string physicalFile = GetRecordingFilename(aFileName);
        tagRec = new Recording(GetChannelIdByDisplayName(aTag.channelName),
                                         GetRecordingStartTime(physicalFile),
                                         GetRecordingEndTime(physicalFile),
                                         aTag.title,
                                         aTag.description,
                                         aTag.genre,
                                         physicalFile,
                                         0,
                                         SqlDateTime.MaxValue.Value,
                                         0,
                                         GetServerId()
                                         );
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Could not build recording from tag: {0}\n{1}", aFileName, ex.Message));
      }
      return tagRec;
    }

    private DateTime GetRecordingStartTime(string aFileName)
    {
      DateTime startTime = SqlDateTime.MinValue.Value;
      if (File.Exists(aFileName))
      {
        FileInfo fi = new FileInfo(aFileName);
        startTime = fi.CreationTime;
      }
      return startTime;
    }

    private DateTime GetRecordingEndTime(string aFileName)
    {
      DateTime endTime = SqlDateTime.MinValue.Value;
      if (File.Exists(aFileName))
      {
        FileInfo fi = new FileInfo(aFileName);
        endTime = fi.LastWriteTime;
      }
      return endTime;
    }

    private string GetRecordingFilename(string aTagFilename)
    {
      string recordingFile = Path.ChangeExtension(aTagFilename, ".ts");
      try
      {
        string[] validExtensions = new string[] { ".ts", ".mpg" };
        foreach (string ext in validExtensions)
        {
          string[] lookupFiles = Directory.GetFiles(Path.GetDirectoryName(aTagFilename), string.Format("{0}{1}", Path.GetFileNameWithoutExtension(aTagFilename), ext), SearchOption.TopDirectoryOnly);
          if (lookupFiles.Length == 1)
          {
            recordingFile = lookupFiles[0];
            return recordingFile;
          }
        }
      }
      catch (Exception)
      {
      }
      return recordingFile;
    }

    private int GetServerId()
    {
      int serverId = 1;
      try
      {
        string localHost = System.Net.Dns.GetHostName();
        IList dbsServers = Server.ListAll();
        foreach (Server computer in dbsServers)
        {
          if (computer.HostName.ToLower() == localHost.ToLower())
            serverId = computer.IdServer;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Could not get ServerID for recording!\n{0}", ex.Message));
      }
      return serverId;
    }

    private int GetChannelIdByDisplayName(string aChannelName)
    {
      int channelId = -1;
      if (string.IsNullOrEmpty(aChannelName))
        return channelId;
      try
      {
        SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Channel));
        sb.AddConstraint(Operator.Like, "displayName", aChannelName);
        sb.SetRowLimit(1);
        SqlStatement stmt = sb.GetStatement(true);
        IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
        if (channels.Count > 0)
          channelId = ((Channel)channels[0]).IdChannel;
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Could not get ChannelID for DisplayName: {0}\n{1}", aChannelName, ex.Message));
      }
      return channelId;
    }

    #endregion

    #region Import

    private void btnImport_Click(object sender, EventArgs e)
    {
      foreach (TreeNode tagRec in tvTagRecs.Nodes)
      {
        if (tagRec.Checked) // only import the recordings which the user has selected
        {
          Recording currentTagRec = tagRec.Tag as Recording;
          if (currentTagRec != null && currentTagRec.IdChannel != -1)
          {
            //if (MessageBox.Show(this, string.Format("Import {0} now? \n{1}", currentTagRec.Title, currentTagRec.FileName), "Recording not found in DB", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            //{
            try
            {
              currentTagRec.Persist();
            }
            catch (Exception ex)
            {
              MessageBox.Show(string.Format("Importing failed: {0}", ex.Message), "Could not import", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //}
          }
        }
      }
      // Refresh the view
      GetRecordingsFromDb();
      GetTagFiles();
    }

    #endregion

    #region Cleanup

    private void btnRemoveInvalidFiles_Click(object sender, EventArgs e)
    {
      foreach (TreeNode dbRec in tvDbRecs)
      {
        Recording currentDbRec = dbRec.Tag as Recording;
        if (currentDbRec != null)
        {
          if (!File.Exists(currentDbRec.FileName))
          {
            if (MessageBox.Show(this, string.Format("Delete entry {0} now? \n{1}", currentDbRec.Title, currentDbRec.FileName), "Recording not found on disk!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
              try
              {
                currentDbRec.Delete();
              }
              catch (Exception ex)
              {
                MessageBox.Show(string.Format("Cleanup failed: {0}", ex.Message), "Could not delete entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
              }
            }
          }
        }
      }
    }

    #endregion

    #endregion
  }
}
