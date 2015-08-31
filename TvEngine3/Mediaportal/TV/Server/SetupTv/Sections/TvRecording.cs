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
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.SetupTV.Dialogs;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class TvRecording : SectionSettings
  {
    [DllImport("kernel32.dll")]
    private static extern long GetDriveType(string driveLetter);

    [DllImport("kernel32.dll")]
    private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out UInt64 lpFreeBytesAvailable,
                                                  out UInt64 lpTotalNumberOfBytes, out UInt64 lpTotalNumberOfFreeBytes);

    #region constants

    private const int MIN_DISK_QUOTA_MB = 500;

    #endregion

    #region Example Format class

    private readonly int[] _formatIndex = new int[2];
    private readonly string[][] _formatString = new string[2][];
    private readonly string[] _customFormat = new string[2];

    private class Example
    {
      public readonly string Channel;
      public readonly string Title;
      public readonly string Episode;
      public readonly string SeriesNum;
      public readonly string EpisodeNum;
      public readonly string EpisodePart;
      public DateTime StartDate;
      public DateTime EndDate;
      public readonly string Genre;

      public Example(string channel, string title, string episode, string seriesNum, string episodeNum,
                     string episodePart, string genre, DateTime startDate, DateTime endDate)
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

    private static string ShowExample(string strInput, int recType)
    {
      string strName = string.Empty;
      string strDirectory = string.Empty;
      Example[] example = new Example[2];
      example[0] = new Example("ProSieben", "Philadelphia", "unknown", "unknown", "unknown", "unknown", "Drama",
                               new DateTime(2005, 12, 23, 20, 15, 0), new DateTime(2005, 12, 23, 22, 45, 0));
      example[1] = new Example("ABC", "Friends", "Joey's Birthday", "4", "32", "part 1 of 1", "Comedy",
                               new DateTime(2005, 12, 23, 20, 15, 0), new DateTime(2005, 12, 23, 20, 45, 0));
      string strDefaultName = String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}p{7}{8}",
                                            example[recType].Channel, example[recType].Title,
                                            example[recType].StartDate.Year, example[recType].StartDate.Month,
                                            example[recType].StartDate.Day,
                                            example[recType].StartDate.Hour,
                                            example[recType].StartDate.Minute,
                                            DateTime.Now.Minute, DateTime.Now.Second);
      if (String.IsNullOrEmpty(strInput))
      {
        return string.Empty;
      }

      strInput = ReplaceTag(strInput, "%channel%", example[recType].Channel, "unknown");
      strInput = ReplaceTag(strInput, "%title%", example[recType].Title, "unknown");
      strInput = ReplaceTag(strInput, "%name%", example[recType].Episode, "unknown");
      strInput = ReplaceTag(strInput, "%series%", example[recType].SeriesNum, "unknown");
      strInput = ReplaceTag(strInput, "%episode%", example[recType].EpisodeNum, "unknown");
      strInput = ReplaceTag(strInput, "%part%", example[recType].EpisodePart, "unknown");
      strInput = ReplaceTag(strInput, "%date%", example[recType].StartDate.ToShortDateString(), "unknown");
      strInput = ReplaceTag(strInput, "%start%", example[recType].StartDate.ToShortTimeString(), "unknown");
      strInput = ReplaceTag(strInput, "%end%", example[recType].EndDate.ToShortTimeString(), "unknown");
      strInput = ReplaceTag(strInput, "%genre%", example[recType].Genre, "unknown");
      strInput = ReplaceTag(strInput, "%startday%", example[recType].StartDate.ToString("dd"), "unknown");
      strInput = ReplaceTag(strInput, "%startmonth%", example[recType].StartDate.ToString("MM"), "unknown");
      strInput = ReplaceTag(strInput, "%startyear%", example[recType].StartDate.ToString("yyyy"), "unknown");
      strInput = ReplaceTag(strInput, "%starthh%", example[recType].StartDate.ToString("HH"), "unknown");
      strInput = ReplaceTag(strInput, "%startmm%", example[recType].StartDate.ToString("mm"), "unknown");
      strInput = ReplaceTag(strInput, "%endday%", example[recType].EndDate.ToString("dd"), "unknown");
      strInput = ReplaceTag(strInput, "%endmonth%", example[recType].EndDate.ToString("MM"), "unknown");
      strInput = ReplaceTag(strInput, "%endyear%", example[recType].EndDate.ToString("yyyy"), "unknown");
      strInput = ReplaceTag(strInput, "%endhh%", example[recType].EndDate.ToString("HH"), "unknown");
      strInput = ReplaceTag(strInput, "%endmm%", example[recType].EndDate.ToString("mm"), "unknown");

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

      strDirectory = MakeDirectoryPath(strDirectory);
      strName = MakeFileName(strName);

      if (strName == string.Empty)
        strName = strDefaultName;
      string strReturn = strDirectory;
      if (strDirectory != string.Empty)
        strReturn += "\\";
      strReturn += strName + ".ts";
      return strReturn;
    }

    #endregion

    #region Vars

    private bool _needRestart;

    #endregion

    public TvRecording(ServerConfigurationChangedEventHandler handler)
      : base("Recording", handler)
    {
      InitializeComponent();
    }

    #region Serialization

    public override void OnSectionActivated()
    {
      MatroskaTagHandler.OnTagLookupCompleted += OnLookupCompleted;

      _needRestart = false;
      comboBoxCards.Items.Clear();
      IList<Tuner> tuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerIncludeRelationEnum.None);
      foreach (Tuner tuner in tuners)
      {
        comboBoxCards.Items.Add(tuner);
      }
      if (comboBoxCards.Items.Count > 0)
        comboBoxCards.SelectedIndex = 0;
      UpdateDriveInfo(false);

      numericUpDownMaxFreeCardsToTry.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("recordMaxFreeCardsToTry", 0);

      comboBoxWeekend.SelectedIndex = ServiceAgents.Instance.SettingServiceAgent.GetValue("FirstDayOfWeekend", 0);  // 0 = Saturday

      checkBoxPreventDupes.Checked = (ServiceAgents.Instance.SettingServiceAgent.GetValue("PreventDuplicates", false));
      comboBoxEpisodeKey.SelectedIndex = ServiceAgents.Instance.SettingServiceAgent.GetValue("EpisodeKey", 0);
      // default EpisodeName
      //checkBoxCreateTagInfoXML.Checked = true; // (ServiceAgents.Instance.SettingServiceAgent.GetValue("createtaginfoxml", "yes").value == "yes");

      numericUpDownPreRec.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("preRecordInterval", 7);
      numericUpDownPostRec.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("postRecordInterval", 10);

      // Movies formats
      _formatString[0] = new string[4];
      _formatString[0][0] = @"%title% - %channel% - %date%";
      _formatString[0][1] = @"%title% - %channel% - %date% - %start%";
      _formatString[0][2] = @"%title%\%title% - %channel% - %date% - %start%";
      _formatString[0][3] = @"[User custom value]"; // Must be the last one in the array list

      // Series formats
      _formatString[1] = new string[5];
      _formatString[1][0] = @"%channel%\%title%\%title% - %date%[ - S%series%][ - E%episode%][ - %name%]";
      _formatString[1][1] = @"%channel%\%title% (%starthh%%startmm% - %endhh%%endmm% %date%)\%title%";
      _formatString[1][2] = @"%title%\%title% - S%series%E%episode% - %name%";
      _formatString[1][3] = @"%title% - %channel%\%title% - %date% - %start%";
      _formatString[1][4] = @"[User custom value]"; // Must be the last one in the array list

      _formatIndex[0]= ServiceAgents.Instance.SettingServiceAgent.GetValue("moviesformatindex", 0);
      _formatIndex[1] = ServiceAgents.Instance.SettingServiceAgent.GetValue("seriesformatindex", 0);

      _customFormat[0] = ServiceAgents.Instance.SettingServiceAgent.GetValue("moviesformat", "");
      _customFormat[1] = ServiceAgents.Instance.SettingServiceAgent.GetValue("seriesformat", "");

      comboBoxMovies.SelectedIndex = 0;
      UpdateFieldDisplay();

      enableDiskQuota.Checked = (ServiceAgents.Instance.SettingServiceAgent.GetValue("diskQuotaEnabled", false));
      enableDiskQuotaControls();

      LoadComboBoxDrive();

      // thumbnails
      checkBoxThumbnailerEnable.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("thumbnailerEnabled", true);
      if (comboBoxThumbnailerQualitySpeed.Items.Count == 0)
      {
        comboBoxThumbnailerQualitySpeed.Items.AddRange(typeof(RecordingThumbnailQuality).GetDescriptions());
      }
      comboBoxThumbnailerQualitySpeed.SelectedItem = ((RecordingThumbnailQuality)ServiceAgents.Instance.SettingServiceAgent.GetValue("thumbnailerQuality", (int)RecordingThumbnailQuality.Highest)).GetDescription();
      numericUpDownThumbnailerColumnCount.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("thumbnailerColumnCount", 1);
      numericUpDownThumbnailerRowCount.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("thumbnailerRowCount", 1);
      numericUpDownThumbnailerTimeOffset.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("thumbnailerTimeOffset", 3);
      checkBoxThumbnailerCopyToRecordingFolder.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("thumbnailerCopyToRecordingFolder", false);

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      MatroskaTagHandler.OnTagLookupCompleted -= OnLookupCompleted;

      ISettingService s = ServiceAgents.Instance.SettingServiceAgent;
      s.SaveValue("preRecordInterval", (int) numericUpDownPreRec.Value);
      s.SaveValue("postRecordInterval", (int) numericUpDownPostRec.Value);


      s.SaveValue("moviesformat", _formatIndex[0] == (_formatString[0].Length - 1)
                        ? _customFormat[0]
                        : _formatString[0][_formatIndex[0]]);
      
      s.SaveValue("moviesformatindex",_formatIndex[0]);
      
      s.SaveValue("seriesformat", _formatIndex[1] == (_formatString[1].Length - 1)
                        ? _customFormat[1]
                        : _formatString[1][_formatIndex[1]]);

      s.SaveValue("seriesformatindex", _formatIndex[1]);
      s.SaveValue("FirstDayOfWeekend", comboBoxWeekend.SelectedIndex); //default is Saturday=0      
      s.SaveValue("PreventDuplicates", checkBoxPreventDupes.Checked);
      s.SaveValue("EpisodeKey", comboBoxEpisodeKey.SelectedIndex);
      s.SaveValue("recordMaxFreeCardsToTry", (int) numericUpDownMaxFreeCardsToTry.Value);      

      UpdateDriveInfo(true);

      // thumbnails
      s.SaveValue("thumbnailerEnabled", checkBoxThumbnailerEnable.Checked);
      s.SaveValue("thumbnailerQuality", Convert.ToInt32(typeof(RecordingThumbnailQuality).GetEnumFromDescription((string)comboBoxThumbnailerQualitySpeed.SelectedItem)));
      s.SaveValue("thumbnailerColumnCount", (int)numericUpDownThumbnailerColumnCount.Value);
      s.SaveValue("thumbnailerRowCount", (int)numericUpDownThumbnailerRowCount.Value);
      s.SaveValue("thumbnailerTimeOffset", (int)numericUpDownThumbnailerTimeOffset.Value);
      s.SaveValue("thumbnailerCopyToRecordingFolder", checkBoxThumbnailerCopyToRecordingFolder.Checked);

      if (_needRestart)
      {
        OnServerConfigurationChanged(this, true, false, null);
      }

      base.OnSectionDeActivated();
    }

    #endregion

    #region GUI-Events

    private void comboBoxMovies_SelectedIndexChanged(object sender, EventArgs e)
    {
      comboBoxFormat.Items.Clear();
      comboBoxFormat.Items.AddRange(_formatString[comboBoxMovies.SelectedIndex]);
      comboBoxFormat.SelectedIndex = _formatIndex[comboBoxMovies.SelectedIndex];
      UpdateFieldDisplay();
    }

    private void comboBoxFormat_SelectedIndexChanged(object sender, EventArgs e)
    {
      _formatIndex[comboBoxMovies.SelectedIndex] = comboBoxFormat.SelectedIndex;
      UpdateFieldDisplay();
    }

    private void UpdateFieldDisplay()
    {
      bool isCustom = comboBoxFormat.SelectedIndex == _formatString[comboBoxMovies.SelectedIndex].Length - 1;
      string frm = isCustom ? textBoxCustomFormat.Text : comboBoxFormat.Items[comboBoxFormat.SelectedIndex].ToString();
      textBoxSample.Text = ShowExample(frm, comboBoxMovies.SelectedIndex);
      labelCustomFormat.Visible = isCustom;
      textBoxCustomFormat.Visible = isCustom;
      textBoxCustomFormat.Text = _customFormat[comboBoxMovies.SelectedIndex];
    }

    private void textBoxCustomFormat_TextChanged(object sender, EventArgs e)
    {
      textBoxSample.Text = ShowExample(textBoxCustomFormat.Text, comboBoxMovies.SelectedIndex);
      _customFormat[comboBoxMovies.SelectedIndex] = textBoxCustomFormat.Text;
    }

    private void comboBoxDrive_SelectedIndexChanged(object sender, EventArgs e)
    {
      UpdateDriveInfo(false);
    }

    private void textBoxCustomFormat_KeyPress(object sender, KeyPressEventArgs e)
    {
      if ((e.KeyChar == '/') || (e.KeyChar == ':') || (e.KeyChar == '*') ||
          (e.KeyChar == '?') || (e.KeyChar == '\"') || (e.KeyChar == '<') ||
          (e.KeyChar == '>') || (e.KeyChar == '|'))
      {
        e.Handled = true;
      }
    }

    private void comboBoxCards_SelectedIndexChanged(object sender, EventArgs e)
    {
      Tuner tuner = (Tuner)comboBoxCards.SelectedItem;
      textBoxFolder.Text = tuner.RecordingFolder;
    }   

    // Browse Recording folder
    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.SelectedPath = textBoxFolder.Text;
      dlg.Description = "Specify recording folder";
      dlg.ShowNewFolderButton = true;
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        textBoxFolder.Text = dlg.SelectedPath;
      }
    }

    private void textBoxFolder_TextChanged(object sender, EventArgs e)
    {
      Tuner tuner = (Tuner)comboBoxCards.SelectedItem;
      if (tuner.RecordingFolder != textBoxFolder.Text)
      {
        tuner.RecordingFolder = textBoxFolder.Text;
        ServiceAgents.Instance.TunerServiceAgent.SaveTuner(tuner);
        _needRestart = true;
        LoadComboBoxDrive();
      }
    }

    // Click on Same recording folder for all cards
    private void buttonSameRecFolder_Click(object sender, EventArgs e)
    {
      // Change RecordingFolder for all cards
      for (int iIndex = 0; iIndex < comboBoxCards.Items.Count; iIndex++)
      {
        Tuner tuner = (Tuner)comboBoxCards.Items[iIndex];
        if (tuner.RecordingFolder != textBoxFolder.Text)
        {
          tuner.RecordingFolder = textBoxFolder.Text;
          ServiceAgents.Instance.TunerServiceAgent.SaveTuner(tuner);
          _needRestart = true;
        }
      }
    }

    private void mpNumericTextBoxDiskQuota_Leave(object sender, EventArgs e)
    {
      UpdateDriveInfo(true);
    }

    private void enableDiskQuota_CheckedChanged(object sender, EventArgs e)
    {
      enableDiskQuotaControls();

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("diskQuotaEnabled", ((MPCheckBox)sender).Checked.ToString());      
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
      if (comboBoxDrive.SelectedItem == null)
        return;
      string drive = (string)comboBoxDrive.SelectedItem;
      string settingName = "freediskspace" + drive;
      if (!drive.StartsWith(@"\"))
      {
        settingName = "freediskspace" + drive[0];
      }

      ulong freeBytesAvailable = 0;
      ulong totalNumberOfBytes = 0;
      ulong totalNumberOfFreeBytes = 0;
      try
      {
        bool result = GetDiskFreeSpaceEx(
          Path.GetPathRoot(drive),
          out freeBytesAvailable,
          out totalNumberOfBytes,
          out totalNumberOfFreeBytes);
        if (!result)
        {
          Log.Warn("utils: failed to determine disk size, error code = {0}, disk = {1}", Marshal.GetLastWin32Error(), drive);
          freeBytesAvailable = 0;
          totalNumberOfBytes = 0;
        }
      }
      catch (Exception ex)
      {
        Log.Warn(ex, "utils: failed to determine disk size, disk = {0}", drive);
      }

      labelFreeDiskspace.Text = GetSize((long)freeBytesAvailable);
      labelTotalDiskSpace.Text = GetSize((long)totalNumberOfBytes);
      if (totalNumberOfBytes == 0)
        labelTotalDiskSpace.Text = "Not available - WMI service not available";
      if (save)
      {
        if (mpNumericTextBoxDiskQuota.Value < MIN_DISK_QUOTA_MB)
          mpNumericTextBoxDiskQuota.Value = MIN_DISK_QUOTA_MB;

        ServiceAgents.Instance.SettingServiceAgent.SaveValue(settingName, mpNumericTextBoxDiskQuota.Value * 1024);
        if (enableDiskQuota.Checked)
        {
          this.LogDebug("SetupTV: Disk Quota for {0} is enabled and set to {1} MB", drive, mpNumericTextBoxDiskQuota.Value);
        }
        else
        {
          this.LogDebug("SetupTV: Disk Quota for {0} is disabled", drive);
        }
      }
      else
      {
        try
        {
          long quota = ServiceAgents.Instance.SettingServiceAgent.GetValue(settingName, MIN_DISK_QUOTA_MB * 1024);
          mpNumericTextBoxDiskQuota.Value = (int)(quota / 1024);
        }
        catch
        {
          mpNumericTextBoxDiskQuota.Value = 0;
        }
        if (mpNumericTextBoxDiskQuota.Value < MIN_DISK_QUOTA_MB)
          mpNumericTextBoxDiskQuota.Value = MIN_DISK_QUOTA_MB;
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
      IList<Tuner> tuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerIncludeRelationEnum.None);
      foreach (Tuner tuner in tuners)
      {
        if (tuner.RecordingFolder.Length > 0)
        {
          string driveLetter = String.Format("{0}:", tuner.RecordingFolder[0]);
          if (tuner.RecordingFolder.StartsWith(@"\"))
          {
            if (!comboBoxDrive.Items.Contains(driveLetter))
            {
              comboBoxDrive.Items.Add(tuner.RecordingFolder);
            }
          }
          else if (GetDriveType(driveLetter) == 3)
          {
            if (!comboBoxDrive.Items.Contains(driveLetter))
            {
              comboBoxDrive.Items.Add(driveLetter);
            }
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
    public class RecordSorter : IComparer<TreeNode>
    {
      // Compare the length of the strings, or the strings
      // themselves, if they are the same length.
      public int Compare(TreeNode tx, TreeNode ty)
      {
        int result = 0;
        try
        {
          result = string.Compare(tx.Text, ty.Text, StringComparison.CurrentCulture);
        }
        catch {}

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
      set { fCurrentImportPath = value; }
    }

    private readonly List<TreeNode> tvDbRecs = new List<TreeNode>();

    #endregion

    #region Settings

    private void LoadDbImportSettings()
    {
      cbRecPaths.Items.Clear();
      GetRecordingsFromDb();
      try
      {
        IList<Tuner> allTuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerIncludeRelationEnum.None);
        foreach (Tuner tuner in allTuners)
        {
          if (!string.IsNullOrEmpty(tuner.RecordingFolder) && !cbRecPaths.Items.Contains(tuner.RecordingFolder))
            cbRecPaths.Items.Add(tuner.RecordingFolder);
        }
        if (cbRecPaths.Items.Count > 0)
        {
          cbRecPaths.SelectedIndex = 0;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Error gathering recording folders of all tuners: \n{0}", ex.Message));
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

    private void checkBoxPreventDupes_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBoxPreventDupes.Checked)
      {
        comboBoxEpisodeKey.Enabled = true;
      }
      else
      {
        comboBoxEpisodeKey.Enabled = false;
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
        IList<Recording> recordings = ServiceAgents.Instance.RecordingServiceAgent.ListAllRecordingsByMediaType(MediaType.Television);
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
        Thread lookupThread = new Thread(MatroskaTagHandler.GetAllMatroskaTags);
        lookupThread.Name = "MatroskaTagHandler";
        lookupThread.Start(CurrentImportPath);
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
        Invoke(new MethodTreeViewTags(AddTagFiles), new object[] {FoundTags});
      }
      catch {}
    }

    /// <summary>
    /// Invoke method from MethodTreeViewTags delegate!!!
    /// </summary>
    /// <param name="FoundTags"></param>
    private void AddTagFiles(Dictionary<string, MatroskaTagInfo> FoundTags)
    {
      tvTagRecs.BeginUpdate();
      try
      {
        tvTagRecs.Nodes.Clear();
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
                  if (Path.GetFileNameWithoutExtension(currentDbRec.FileName) ==
                      Path.GetFileNameWithoutExtension(TagRec.FileName))
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
        SetImportButton();
      }
      catch
      {
        // just in case the GUI controls could be null due to timing problems on thread callback
        if (btnImport != null)
          btnImport.Enabled = false;
      }
      finally
      {
        tvTagRecs.EndUpdate();
      }
    }

    #endregion

    #region Visualisation

    private static TreeNode BuildNodeFromRecording(Recording aRec)
    {
      try
      {
        Channel lookupChannel;
        string channelName = "unknown";
        string startTime = SqlDateTime.MinValue.Value == aRec.StartTime ? "unknown" : aRec.StartTime.ToString();
        string endTime = SqlDateTime.MinValue.Value == aRec.EndTime ? "unknown" : aRec.EndTime.ToString();
        try
        {
          lookupChannel = aRec.Channel;
          if (lookupChannel != null)
          {
            channelName = lookupChannel.Name;
            lookupChannel.IdChannel.ToString();
          }
        }
        catch {}

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
        //TreeNode recItem = new TreeNode(aRec.title, subitems);

        string NodeTitle;
        if (startTime != "unknown" && endTime != "unknown")
          NodeTitle = string.Format("Title: {0} / Channel: {1} / Time: {2}-{3}", aRec.Title, channelName, startTime,
                                    endTime);
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


    private void tvTagRecs_AfterSelect(object sender, TreeViewEventArgs e)
    {
      try
      {
        if (e.Node != null)
        {
          e.Node.Checked = e.Node.IsSelected;
        }
      }
      catch {}
    }

    #endregion

    #region Tag to recording conversion

    private static Recording BuildRecordingFromTag(string aFileName, MatroskaTagInfo aTag)
    {
      Recording tagRec = null;
      try
      {
        string physicalFile = GetRecordingFilename(aFileName);
        if (aTag.startTime.Equals(SqlDateTime.MinValue.Value))
        {
          GetRecordingTimes(physicalFile, out aTag.startTime, out aTag.endTime);
        }

        ProgramCategory category =  ServiceAgents.Instance.ProgramServiceAgent.GetProgramCategoryByName(aTag.genre);

        Channel channel = GetChannelByDisplayName(aTag.channelName);
        int channelId = -1;

        if (channel != null)
        {
          channelId = channel.IdChannel;
        }

        tagRec = RecordingFactory.CreateRecording(channelId,
                               null,
                               false,
                               aTag.startTime,
                               aTag.endTime,
                               aTag.title,
                               aTag.description,
                               category,
                               physicalFile,
                               0,
                               SqlDateTime.MaxValue.Value,
                               0,                               
                               aTag.episodeName,
                               aTag.seriesNum,
                               aTag.episodeNum,
                               aTag.episodePart);                               

        tagRec.MediaType = Convert.ToInt32(aTag.mediaType);
        tagRec.Channel = channel;
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Could not build recording from tag: {0}\n{1}", aFileName, ex.Message));
      }
      return tagRec;
    }

    private static void GetRecordingTimes(string aFileName, out DateTime startTime, out DateTime endTime)
    {
      startTime = SqlDateTime.MinValue.Value;
      endTime = SqlDateTime.MinValue.Value;
      if (File.Exists(aFileName))
      {
        FileInfo fi = new FileInfo(aFileName);
        startTime = fi.CreationTime;
        endTime = fi.LastWriteTime;
      }
    }

    /*
     * Mantis #0001991: disable mpg recording  (part I: force TS recording format)
     * edit: morpheus_xx: function still needed to get valid extension of EXISTING recordings
     */

    private static string GetRecordingFilename(string aTagFilename)
    {
      string recordingFile = Path.ChangeExtension(aTagFilename, ".ts");
      try
      {
        string[] validExtensions = new string[] {".ts", ".mpg"};
        foreach (string ext in validExtensions)
        {
          string[] lookupFiles = Directory.GetFiles(Path.GetDirectoryName(aTagFilename),
                                                    string.Format("{0}{1}",
                                                                  Path.GetFileNameWithoutExtension(aTagFilename), ext),
                                                    SearchOption.TopDirectoryOnly);
          if (lookupFiles.Length == 1)
          {
            recordingFile = lookupFiles[0];
            return recordingFile;
          }
        }
      }
      catch {}
      return recordingFile;
    }

    private static Channel GetChannelByDisplayName(string aChannelName)
    {
      Channel channel = null;
      if (string.IsNullOrEmpty(aChannelName))
      {
        return channel;
      }
      try
      {       
        channel = ServiceAgents.Instance.ChannelServiceAgent.GetChannelByName(aChannelName, ChannelIncludeRelationEnum.None);        
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Could not get ChannelID for DisplayName: {0}\n{1}", aChannelName, ex.Message));
      }
      return channel;
    }

    #endregion

    #region Change channel

    private void buttonChangeChannel_Click(object sender, EventArgs e)
    {
      try
      {
        // TODO: Just change the channel in the xml file - do not import immediately
        // uninitialized
        int newId = -2;
        foreach (TreeNode node in tvTagRecs.Nodes)
        {
          if (node.Checked)
          {
            // first time = ask for channel
            if (newId == -2)
            {
              FormSelectListChannel idSelection = new FormSelectListChannel();
              newId = idSelection.ShowFormModal();
            }
            // If the user chose a proper channel
            if (newId > -1)
            {
              Recording currentTagRec = node.Tag as Recording;
              if (currentTagRec != null)
              {
                try
                {
                  currentTagRec.IdChannel = newId;                  
                  ServiceAgents.Instance.RecordingServiceAgent.SaveRecording(currentTagRec);
                }
                catch (Exception ex)
                {
                  MessageBox.Show(string.Format("Importing failed: \n{0}", ex.Message), "Could not import",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Changing the recording's channel failed: \n{0}", ex.Message),
                        "Change channel failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      // Refresh the view
      GetRecordingsFromDb();
      GetTagFiles();
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
            //if (MessageBox.Show(this, string.Format("Import {0} now? \n{1}", currentTagRec.title, currentTagRec.FileName), "Recording not found in DB", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            //{
            try
            {              
              ServiceAgents.Instance.RecordingServiceAgent.SaveRecording(currentTagRec);
            }
            catch (Exception ex)
            {
              MessageBox.Show(string.Format("Importing failed: \n{0}", ex.Message), "Could not import",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (
              MessageBox.Show(this,
                              string.Format("Delete entry {0} now? \n{1}", currentDbRec.Title, currentDbRec.FileName),
                              "Recording not found on disk!", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                              MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
              try
              {
                ServiceAgents.Instance.RecordingServiceAgent.DeleteRecording(currentDbRec.IdRecording);                
              }
              catch (Exception ex)
              {
                MessageBox.Show(string.Format("Cleanup failed: {0}", ex.Message), "Could not delete entry",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
              }
            }
          }
        }
      }
    }

    #endregion

    #endregion

    #region thumbnails

    private void checkBoxThumbnailerEnable_CheckedChanged(object sender, EventArgs e)
    {
      groupBoxThumbnailer.Enabled = checkBoxThumbnailerEnable.Checked;
    }

    private void buttonThumbnailerCreateMissing_Click(object sender, EventArgs e)
    {
      this.LogInfo("recordings: create missing thumbnails");
      ServiceAgents.Instance.ControllerServiceAgent.CreateMissingThumbnails();
    }

    private void buttonThumbnailerDeleteExisting_Click(object sender, EventArgs e)
    {
      this.LogInfo("recordings: delete existing thumbnails");
      ServiceAgents.Instance.ControllerServiceAgent.DeleteExistingThumbnails();
    }

    #endregion

    private static string GetSize(long dwFileSize)
    {
      if (dwFileSize < 0)
        return "0";
      string szTemp;
      // file < 1 kbyte?
      if (dwFileSize < 1024)
      {
        //  substract the integer part of the float value
        float fRemainder = (dwFileSize / 1024.0f) - (dwFileSize / 1024.0f);
        float fToAdd = 0.0f;
        if (fRemainder < 0.01f)
          fToAdd = 0.1f;
        szTemp = String.Format("{0:f} KB", (dwFileSize / 1024.0f) + fToAdd);
        return szTemp;
      }
      const long iOneMeg = 1024 * 1024;

      // file < 1 megabyte?
      if (dwFileSize < iOneMeg)
      {
        szTemp = String.Format("{0:f} KB", dwFileSize / 1024.0f);
        return szTemp;
      }

      // file < 1 GByte?
      long iOneGigabyte = iOneMeg;
      iOneGigabyte *= 1000;
      if (dwFileSize < iOneGigabyte)
      {
        szTemp = String.Format("{0:f} MB", dwFileSize / ((float)iOneMeg));
        return szTemp;
      }
      //file > 1 GByte
      int iGigs = 0;
      while (dwFileSize >= iOneGigabyte)
      {
        dwFileSize -= iOneGigabyte;
        iGigs++;
      }
      float fMegs = dwFileSize / ((float)iOneMeg);
      fMegs /= 1000.0f;
      fMegs += iGigs;
      szTemp = String.Format("{0:f} GB", fMegs);
      return szTemp;
    }

    private static string MakeFileName(string strText)
    {
      if (string.IsNullOrEmpty(strText))
      {
        return string.Empty;
      }
      foreach (char c in Path.GetInvalidFileNameChars())
      {
        strText = strText.Replace(c, '_');
      }
      return strText;
    }

    private static string MakeDirectoryPath(string strText)
    {
      if (string.IsNullOrEmpty(strText))
      {
        return string.Empty;
      }
      foreach (char c in Path.GetInvalidPathChars())
      {
        strText = strText.Replace(c, '_');
      }
      return strText;
    }

    private static string ReplaceTag(string line, string tag, string value, string empty)
    {
      if (line == null)
        return String.Empty;
      if (line.Length == 0)
        return String.Empty;
      if (tag == null)
        return line;
      if (tag.Length == 0)
        return line;

      Regex r = new Regex(String.Format(@"\[[^%]*{0}[^\]]*[\]]", tag));
      if (value == empty)
      {
        Match match = r.Match(line);
        if (match != null && match.Length > 0)
        {
          line = line.Remove(match.Index, match.Length);
        }
      }
      else
      {
        Match match = r.Match(line);
        if (match != null && match.Length > 0)
        {
          line = line.Remove(match.Index, match.Length);
          string m = match.Value.Substring(1, match.Value.Length - 2);
          line = line.Insert(match.Index, m);
        }
      }
      return line.Replace(tag, value);
    }
  }
}