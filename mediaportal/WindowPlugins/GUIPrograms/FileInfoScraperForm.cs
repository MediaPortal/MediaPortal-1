#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

using MediaPortal.GUI.Library;

using Programs.Utils;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for Form1.
  /// </summary>
  public class FileInfoScraperForm : Form
  {
    #region InitControls

    private AppItem m_CurApp;
    private MediaPortal.UserInterface.Controls.MPListView listViewFileList;
    private ColumnHeader FileTitle;
    private ColumnHeader status;
    private MediaPortal.UserInterface.Controls.MPButton buttonStartSearch;
    private MediaPortal.UserInterface.Controls.MPButton buttonSaveSearch;
    private MediaPortal.UserInterface.Controls.MPButton buttonClose;
    private LinkLabel linkLabelAllGame;
    private ToolTip toolTip1;
    private MediaPortal.UserInterface.Controls.MPComboBox filterComboBox;
    private MediaPortal.UserInterface.Controls.MPButton ResetFilterButton;
    private NumericUpDown MinRelevanceNum;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private Panel bottomPanel;
    private ContextMenu menuFileList;
    private MenuItem mnuCheckWithoutImages;
    private MenuItem mnuCheckWithoutOverview;
    private IContainer components;

    private ContextMenu menuSaveDetails;
    private MenuItem menuItem4;
    private MenuItem menuDataAndImages;
    private MenuItem menuData;
    private MenuItem menuImages;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxMatchList;
    private MediaPortal.UserInterface.Controls.MPListView listViewMatchList;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxFileList;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkboxFileList;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPLabel labelLaunchUrlInfo;

    #endregion

    #region Variables / Init

    int mStartTime = 0;
    private SplitContainer splitContainer1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxFilter;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel toolStripStatusLabel;
    private ToolStripProgressBar toolStripProgressBar;
    private MediaPortal.UserInterface.Controls.MPLabel labelPlatform;
    private ToolStripDropDownButton toolStripDropDownButton1;
    private ToolStripMenuItem cancelSearchToolStripMenuItem; // timer stuff

    bool isSearching = false;
    bool stopSearching = false;

    ScraperSaveType saveType = ScraperSaveType.DataAndImages;

    public AppItem CurApp
    {
      get
      {
        return m_CurApp;
      }
      set
      {
        SetCurApp(value);
      }
    }

    void SetCurApp(AppItem value)
    {
      m_CurApp = value;
      if (m_CurApp != null)
      {
        filterComboBox.Text = m_CurApp.SystemDefault;
      }
    }

    public FileInfoScraperForm()
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

    public void Setup()
    {
      SyncFileList();
      UpdateButtonStates();
    }

    #endregion

    #region Properties / Helper Routines

    private FileItem GetSelectedFileItem()
    {
      if (listViewFileList.FocusedItem == null) return null;
      if (listViewFileList.FocusedItem.Tag == null) return null;

      return listViewFileList.FocusedItem.Tag as FileItem;
    }

    private FileInfo GetSelectedMatchItem()
    {
      if (listViewMatchList.CheckedItems == null) return null;
      if (listViewMatchList.CheckedItems[0] == null) return null;
      if (listViewMatchList.CheckedItems[0].Tag == null) return null;

      return listViewMatchList.CheckedItems[0].Tag as FileInfo;
    }

    private bool IsGoodMatch(FileInfo info)
    {
      bool result = (filterComboBox.Text == "") || (info.Platform.ToLower().IndexOf(filterComboBox.Text.ToLower()) == 0);
      if (result)
      {
        result = (info.RelevanceNorm >= MinRelevanceNum.Value);
      }
      return result;
    }

    private void SelectBestMatch(ListViewItem curItem)
    {
      if (curItem.Tag == null) return;

      FileItem file = curItem.Tag as FileItem;
      if (file == null) return;
      if (file.FileInfoList == null) return;

      foreach (FileInfo info in file.FileInfoList)
      {
        // check if 
        //   - info is from platform, which is set in combobox
        //   - has minimum relevance
        if (!IsGoodMatch(info)) continue;

        // if file has no favourite yet
        if (file.FileInfoFavourite == null)
        {
          file.FileInfoFavourite = info;
          continue;
        }

        // prevously selected infoitem
        if (file.GameURL == info.GameURL)
        {
          file.FileInfoFavourite = info;
          continue;
        }

        // file has already a favourite
        // is info's relevance better than current favourite's relevance
        if (info.RelevanceNorm > file.FileInfoFavourite.RelevanceNorm)
          file.FileInfoFavourite = info;
      }

      if (file.FileInfoFavourite != null)
      {
        curItem.SubItems[1].Text = String.Format("best: {0}%", file.FileInfoFavourite.RelevanceNorm);
      }
      else
      {
        curItem.SubItems[1].Text = "no match";
      }
    }

    #endregion

    #region Display

    private void UpdateButtonStates()
    {
      // labels to update
      groupBoxFileList.Text = String.Format("Files ({0} of {1} selected)", listViewFileList.CheckedItems.Count, listViewFileList.Items.Count);

      // button states to update
      buttonStartSearch.Enabled = (listViewFileList.CheckedItems.Count > 0);

      if (!buttonStartSearch.Enabled)
        buttonSaveSearch.Enabled = false;

      // when search is active, only cancel button should be enabled
      groupBoxFileList.Enabled = (!isSearching);
      groupBoxMatchList.Enabled = (!isSearching);
      groupBoxFilter.Enabled = (!isSearching);
      buttonStartSearch.Enabled = (!isSearching);
      buttonSaveSearch.Enabled = (!isSearching);
      buttonClose.Enabled = (!isSearching);
      toolStripDropDownButton1.Enabled = isSearching;

    }

    private void ChangeFileSelection()
    {
      SyncMatchList(GetSelectedFileItem());
    }

    private void SyncFileList()
    {
      if (m_CurApp == null) return;
      
      listViewFileList.BeginUpdate();
      try
      {
        listViewFileList.Items.Clear();

        // add all files
        foreach (FileItem file in m_CurApp.Files)
        {
          ListViewItem curItem = new ListViewItem(file.Title);
          file.ToFileInfoFavourite();
          curItem.Tag = file;
          if (!file.IsFolder)
          {
            ListViewItem newItem = listViewFileList.Items.Add(curItem);
            newItem.SubItems.Add("<unknown>");
          }
        }
      }
      finally
      {
        listViewFileList.EndUpdate();
      }
    }

    private void SyncMatchList(FileItem file)
    {
      listViewMatchList.BeginUpdate();
      try
      {
        listViewMatchList.Items.Clear();

        if (file == null) return;
        if (file.FileInfoList == null) return;

        foreach (FileInfo item in file.FileInfoList)
        {
          if (!IsGoodMatch(item)) continue;

          ListViewItem curItem = new ListViewItem(String.Format("{0} ({1})", item.Title, item.Platform));
          curItem.SubItems.Add(String.Format("{0}%", item.RelevanceNorm));
          curItem.Tag = item;

          // selected item?
          if (file.FileInfoFavourite != null)
            if (file.FileInfoFavourite == item)
            {
              curItem.Checked = true;
            }

          listViewMatchList.Items.Add(curItem);
        }
      }
      finally
      {
        listViewMatchList.EndUpdate();
      }
    }

    private void InitProgressBar(string msg)
    {
      toolStripProgressBar.Value = 0;
      if (listViewFileList.CheckedItems.Count - 1 > 0)
      {
        toolStripProgressBar.Maximum = listViewFileList.CheckedItems.Count - 1;
      }
      else
      {
        toolStripProgressBar.Maximum = 1;
      }
      toolStripProgressBar.Step = 1;
      toolStripStatusLabel.Text = msg;
      mStartTime = (int)(DateTime.Now.Ticks / 10000); // reset timer!
    }

    private void StepProgressBar()
    {
      string strTimeRemaining = "";
      toolStripProgressBar.PerformStep();
      if (toolStripProgressBar.Value > 1)
      {
        int nTimeElapsed = ((int)(DateTime.Now.Ticks / 10000)) - mStartTime;
        double TimePerItem = nTimeElapsed / toolStripProgressBar.Value - 1;
        int nTotalTime = (int)(toolStripProgressBar.Maximum * TimePerItem);
        int nTimeRemaining = nTotalTime - nTimeElapsed;
        int nSecondsRemaining = nTimeRemaining / 1000;
        int nMinutesRemaining = nSecondsRemaining / 60;
        nSecondsRemaining = nSecondsRemaining - (nMinutesRemaining * 60);
        strTimeRemaining = String.Format(" ({0}m {1}s remaining)", nMinutesRemaining, nSecondsRemaining);

      }
      toolStripStatusLabel.Text = String.Format("Searching file {0} of {1} ", toolStripProgressBar.Value, toolStripProgressBar.Maximum + 1) + strTimeRemaining;
    }

    private void DeInitProgressBar(string msg)
    {
      toolStripStatusLabel.Text = msg;
    }

    #endregion

    #region ControlEvents

    #region Lists

    private void listViewFileList_SelectedIndexChanged(object sender, EventArgs e)
    {
      ChangeFileSelection();
    }

    private void listViewFileList_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      UpdateButtonStates();
    }

    private void listViewMatchList_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      if (e.NewValue == CheckState.Checked)
      {
        if (listViewMatchList.CheckedItems == null) return;
        foreach (ListViewItem item in listViewMatchList.CheckedItems)
          if (item.Index != e.Index)
            item.Checked = false;
      }
    }

    private void listViewMatchList_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      if (e.Item.Checked == false) return;

      FileItem file = GetSelectedFileItem();
      if (file == null) return;

      if (e.Item.Tag == null) return;
      FileInfo info = e.Item.Tag as FileInfo;
      if (info == null) return;

      file.FileInfoFavourite = info;
      buttonSaveSearch.Enabled = true;
      listViewFileList.FocusedItem.SubItems[1].Text = String.Format("best: {0}%", file.FileInfoFavourite.RelevanceNorm);
    }

    private void listViewMatchList_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listViewMatchList.SelectedItems.Count == 0) return;
      if (listViewMatchList.SelectedItems[0].Tag == null) return;
      FileInfo info = listViewMatchList.SelectedItems[0].Tag as FileInfo;
      if (info == null) return;

      info.LaunchURL();
    }

    private void checkboxFileList_CheckedChanged(object sender, EventArgs e)
    {
      if (checkboxFileList.Checked)
      {
        foreach (ListViewItem curItem in listViewFileList.Items)
          curItem.Checked = true;
      }
      else
      {
        foreach (ListViewItem curItem in listViewFileList.Items)
          curItem.Checked = false;
      }
      UpdateButtonStates();
    }

    #endregion

    #region Buttons

    private void btnStartSearch_Click(object sender, EventArgs e)
    {
      SearchStart();
    }

    private void btnSaveSearch_Click(object sender, EventArgs e)
    {
      menuSaveDetails.Show(buttonSaveSearch, new Point(0, buttonSaveSearch.Height));
    }

    private void cancelSearchToolStripMenuItem_Click(object sender, EventArgs e)
    {
      stopSearching = true;
    }

    private void buttonClose_Click(object sender, EventArgs e)
    {
      if ((m_CurApp != null) && (filterComboBox.Text != ""))
      {
        m_CurApp.SystemDefault = filterComboBox.Text;
        m_CurApp.Write();
      }
      this.Close();
    }

    private void allGameLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (linkLabelAllGame.Text == null) return;

      if (linkLabelAllGame.Text.Length > 0)
      {
        ProcessStartInfo sInfo = new ProcessStartInfo(linkLabelAllGame.Text);
        Process.Start(sInfo);
      }
    }

    #endregion

    private void filterComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      ChangeFileSelection();
    }

    private void filterComboBox_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        ChangeFileSelection();
      }
    }

    private void ResetFilterButton_Click(object sender, EventArgs e)
    {
      filterComboBox.Text = "";
      ChangeFileSelection();
    }

    private void MinRelevanceNum_ValueChanged(object sender, EventArgs e)
    {
      ChangeFileSelection();
    }

    #region Menus

    private void mnuCheckWithoutImages_Click(object sender, EventArgs e)
    {
      FileItem curFile;
      foreach (ListViewItem curItem in listViewFileList.Items)
      {
        curFile = (FileItem)curItem.Tag;
        if (curFile != null)
        {
          curItem.Checked = (curFile.Imagefile == "");
        }
        else
        {
          curItem.Checked = false;
        }
      }
      buttonStartSearch.Enabled = (listViewFileList.CheckedItems.Count > 0);
      UpdateButtonStates();
    }
    private void mnuCheckWithoutOverview_Click(object sender, EventArgs e)
    {
      FileItem curFile;
      foreach (ListViewItem curItem in listViewFileList.Items)
      {
        curFile = (FileItem)curItem.Tag;
        if (curFile != null)
        {
          curItem.Checked = (curFile.Overview == "");
        }
        else
        {
          curItem.Checked = false;
        }
      }
      buttonStartSearch.Enabled = (listViewFileList.CheckedItems.Count > 0);
      UpdateButtonStates();
    }
    
    private void menuDataAndImages_Click(object sender, EventArgs e)
    {
      saveType = ScraperSaveType.DataAndImages;
      SearchSave();
    }
    private void menuData_Click(object sender, EventArgs e)
    {
      saveType = ScraperSaveType.Data;
      SearchSave();
    }
    private void menuImages_Click(object sender, EventArgs e)
    {
      saveType = ScraperSaveType.Images;
      SearchSave();
    }

    #endregion

    #endregion

    #region Threads

    private void SearchStart()
    {
      Thread thread = new Thread(new ThreadStart(SearchStartThread));
      thread.Priority = ThreadPriority.BelowNormal;
      thread.Name = "MyPrograms SearchStart";
      thread.Start();
    }

    private void SearchStartThread()
    {
      isSearching = true;
      UpdateButtonStates();

      int numberOfSearches = 0;
      bool bSuccess = true;
      InitProgressBar("Starting search");
      foreach (ListViewItem curItem in listViewFileList.CheckedItems)
      {
        if (stopSearching) break;
        if (curItem.Tag == null) continue;

        FileItem file = curItem.Tag as FileItem;
        if (file == null) continue;

        ListViewItem nextItem = null;

        if (curItem.Index < listViewFileList.Items.Count - 1)
        {
          nextItem = listViewFileList.Items[curItem.Index + 1];
        }
        else
        {
          nextItem = curItem;
        }
        nextItem.EnsureVisible();
        //          if (!bSuccess)
        //          {
        //            curItem.SubItems[1].Text = String.Format("waiting for reconnection...");
        //            System.Threading.Thread.Sleep(5126);
        //          }
        numberOfSearches = numberOfSearches + 1;
        if (numberOfSearches > 20)
        {
          curItem.SubItems[1].Text = String.Format("waiting...");
          System.Threading.Thread.Sleep(20000);
          System.Windows.Forms.Application.DoEvents();
          numberOfSearches = 0;
        }
        curItem.SubItems[1].Text = String.Format("searching...");
        curItem.Font = new Font(curItem.Font, curItem.Font.Style | FontStyle.Bold);
        System.Windows.Forms.Application.DoEvents();
        bSuccess = file.FindFileInfo(myProgScraperType.ALLGAME);

        SelectBestMatch(curItem);

        StepProgressBar();
        System.Windows.Forms.Application.DoEvents();
      }
      ChangeFileSelection();
      if (stopSearching)
      {
        DeInitProgressBar("Search aborted");
      }
      else
      {
        DeInitProgressBar("Search finished");
      }

      stopSearching = false;
      isSearching = false;
      UpdateButtonStates();
    }

    private void SearchSave()
    {
      Thread thread = new Thread(new ThreadStart(SearchSaveThread));
      thread.Priority = ThreadPriority.BelowNormal;
      thread.Name = "MyPrograms SearchSave";
      thread.Start();
    }

    private void SearchSaveThread()
    {
      isSearching = true;
      UpdateButtonStates();

      int numberOfSearches = 0;
      InitProgressBar("Starting search");
      ListViewItem nextItem = null;

      foreach (ListViewItem curItem in listViewFileList.CheckedItems)
      {
        if (stopSearching)
          break;
        FileItem file = (FileItem)curItem.Tag;

        if (file == null)
          continue;

        if (curItem.Index < listViewFileList.Items.Count - 1)
        {
          nextItem = listViewFileList.Items[curItem.Index + 1];
        }
        else
        {
          nextItem = curItem;
        }
        nextItem.EnsureVisible();
        StepProgressBar();
        if (file.FileInfoFavourite != null)
        {
          numberOfSearches++;
          numberOfSearches = numberOfSearches + 1;
          if (numberOfSearches > 20)
          {
            curItem.SubItems[1].Text = String.Format("waiting...");
            System.Windows.Forms.Application.DoEvents();
            System.Threading.Thread.Sleep(20000);
            numberOfSearches = 0;
          }
          curItem.SubItems[1].Text = String.Format("<searching...>");
          System.Windows.Forms.Application.DoEvents();
          file.FindFileInfoDetail(m_CurApp, file.FileInfoFavourite, myProgScraperType.ALLGAME, saveType);
          if ((saveType == ScraperSaveType.DataAndImages) || (saveType == ScraperSaveType.Data))
          {
            file.SaveFromFileInfoFavourite();
          }
          curItem.SubItems[1].Text = String.Format("<saved>");
          System.Windows.Forms.Application.DoEvents();
        }
      }
      if (stopSearching)
      {
        DeInitProgressBar("Search aborted");
      }
      else
      {
        DeInitProgressBar("Search finished");
      }

      stopSearching = false;
      isSearching = false;
      UpdateButtonStates();
    }

    #endregion

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileInfoScraperForm));
      this.bottomPanel = new System.Windows.Forms.Panel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxFilter = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelPlatform = new MediaPortal.UserInterface.Controls.MPLabel();
      this.filterComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.ResetFilterButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.MinRelevanceNum = new System.Windows.Forms.NumericUpDown();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.linkLabelAllGame = new System.Windows.Forms.LinkLabel();
      this.buttonClose = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonSaveSearch = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonStartSearch = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewFileList = new MediaPortal.UserInterface.Controls.MPListView();
      this.FileTitle = new System.Windows.Forms.ColumnHeader();
      this.status = new System.Windows.Forms.ColumnHeader();
      this.menuFileList = new System.Windows.Forms.ContextMenu();
      this.mnuCheckWithoutImages = new System.Windows.Forms.MenuItem();
      this.mnuCheckWithoutOverview = new System.Windows.Forms.MenuItem();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.menuSaveDetails = new System.Windows.Forms.ContextMenu();
      this.menuDataAndImages = new System.Windows.Forms.MenuItem();
      this.menuItem4 = new System.Windows.Forms.MenuItem();
      this.menuData = new System.Windows.Forms.MenuItem();
      this.menuImages = new System.Windows.Forms.MenuItem();
      this.groupBoxMatchList = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelLaunchUrlInfo = new MediaPortal.UserInterface.Controls.MPLabel();
      this.listViewMatchList = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.groupBoxFileList = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkboxFileList = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.statusStrip = new System.Windows.Forms.StatusStrip();
      this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
      this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
      this.cancelSearchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.bottomPanel.SuspendLayout();
      this.groupBoxFilter.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.MinRelevanceNum)).BeginInit();
      this.groupBoxMatchList.SuspendLayout();
      this.groupBoxFileList.SuspendLayout();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.statusStrip.SuspendLayout();
      this.SuspendLayout();
      // 
      // bottomPanel
      // 
      this.bottomPanel.Controls.Add(this.label2);
      this.bottomPanel.Controls.Add(this.groupBoxFilter);
      this.bottomPanel.Controls.Add(this.linkLabelAllGame);
      this.bottomPanel.Controls.Add(this.buttonClose);
      this.bottomPanel.Controls.Add(this.buttonSaveSearch);
      this.bottomPanel.Controls.Add(this.buttonStartSearch);
      this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.bottomPanel.Location = new System.Drawing.Point(0, 399);
      this.bottomPanel.Name = "bottomPanel";
      this.bottomPanel.Size = new System.Drawing.Size(631, 86);
      this.bottomPanel.TabIndex = 0;
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(474, 4);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(154, 13);
      this.label2.TabIndex = 28;
      this.label2.Text = "The data is brought to you by:";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // groupBoxFilter
      // 
      this.groupBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.groupBoxFilter.Controls.Add(this.labelPlatform);
      this.groupBoxFilter.Controls.Add(this.filterComboBox);
      this.groupBoxFilter.Controls.Add(this.ResetFilterButton);
      this.groupBoxFilter.Controls.Add(this.MinRelevanceNum);
      this.groupBoxFilter.Controls.Add(this.label1);
      this.groupBoxFilter.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxFilter.Location = new System.Drawing.Point(3, 4);
      this.groupBoxFilter.Name = "groupBoxFilter";
      this.groupBoxFilter.Size = new System.Drawing.Size(247, 79);
      this.groupBoxFilter.TabIndex = 29;
      this.groupBoxFilter.TabStop = false;
      this.groupBoxFilter.Text = "Filter:";
      // 
      // labelPlatform
      // 
      this.labelPlatform.AutoSize = true;
      this.labelPlatform.Location = new System.Drawing.Point(6, 25);
      this.labelPlatform.Name = "labelPlatform";
      this.labelPlatform.Size = new System.Drawing.Size(51, 13);
      this.labelPlatform.TabIndex = 30;
      this.labelPlatform.Text = "Platform:";
      // 
      // filterComboBox
      // 
      this.filterComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.filterComboBox.BorderColor = System.Drawing.Color.Empty;
      this.filterComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.filterComboBox.Items.AddRange(new object[] {
            "Arcade",
            "Atari 5200",
            "Atari 7800",
            "Atari Lynx",
            "Atari ST",
            "Atari Video Computer System",
            "Commodore 64/128",
            "Commodore Amiga",
            "Game Boy",
            "Game Boy Advance",
            "Game Boy Color",
            "Neo Geo",
            "Nintendo 64",
            "Nintendo Entertainment System",
            "PlayStation",
            "Sega Dreamcast",
            "Sega Game Gear",
            "Sega Genesis",
            "Sega Master System",
            "Super NES",
            "TurboGrafx-16"});
      this.filterComboBox.Location = new System.Drawing.Point(63, 22);
      this.filterComboBox.Name = "filterComboBox";
      this.filterComboBox.Size = new System.Drawing.Size(132, 21);
      this.filterComboBox.TabIndex = 22;
      this.toolTip1.SetToolTip(this.filterComboBox, "Enter platform to filter results");
      this.filterComboBox.SelectedIndexChanged += new System.EventHandler(this.filterComboBox_SelectedIndexChanged);
      this.filterComboBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.filterComboBox_KeyUp);
      // 
      // ResetFilterButton
      // 
      this.ResetFilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.ResetFilterButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.ResetFilterButton.Location = new System.Drawing.Point(201, 20);
      this.ResetFilterButton.Name = "ResetFilterButton";
      this.ResetFilterButton.Size = new System.Drawing.Size(40, 23);
      this.ResetFilterButton.TabIndex = 23;
      this.ResetFilterButton.Text = "Clear";
      this.toolTip1.SetToolTip(this.ResetFilterButton, "Reset Filter");
      this.ResetFilterButton.UseVisualStyleBackColor = true;
      this.ResetFilterButton.Click += new System.EventHandler(this.ResetFilterButton_Click);
      // 
      // MinRelevanceNum
      // 
      this.MinRelevanceNum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.MinRelevanceNum.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.MinRelevanceNum.Location = new System.Drawing.Point(117, 49);
      this.MinRelevanceNum.Name = "MinRelevanceNum";
      this.MinRelevanceNum.Size = new System.Drawing.Size(78, 21);
      this.MinRelevanceNum.TabIndex = 24;
      this.MinRelevanceNum.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.toolTip1.SetToolTip(this.MinRelevanceNum, "This is the minimal RELEVANCE value to autoselect a match");
      this.MinRelevanceNum.Value = new decimal(new int[] {
            70,
            0,
            0,
            0});
      this.MinRelevanceNum.ValueChanged += new System.EventHandler(this.MinRelevanceNum_ValueChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(6, 51);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(84, 13);
      this.label1.TabIndex = 25;
      this.label1.Text = "Min. Relevance:";
      // 
      // linkLabelAllGame
      // 
      this.linkLabelAllGame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.linkLabelAllGame.AutoSize = true;
      this.linkLabelAllGame.Location = new System.Drawing.Point(502, 24);
      this.linkLabelAllGame.Name = "linkLabelAllGame";
      this.linkLabelAllGame.Size = new System.Drawing.Size(126, 13);
      this.linkLabelAllGame.TabIndex = 3;
      this.linkLabelAllGame.TabStop = true;
      this.linkLabelAllGame.Text = "http://www.allgame.com";
      this.linkLabelAllGame.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.linkLabelAllGame.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.allGameLink_LinkClicked);
      // 
      // buttonClose
      // 
      this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonClose.Location = new System.Drawing.Point(505, 49);
      this.buttonClose.Name = "buttonClose";
      this.buttonClose.Size = new System.Drawing.Size(114, 30);
      this.buttonClose.TabIndex = 2;
      this.buttonClose.Text = "Close";
      this.buttonClose.UseVisualStyleBackColor = true;
      this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
      // 
      // buttonSaveSearch
      // 
      this.buttonSaveSearch.Enabled = false;
      this.buttonSaveSearch.Location = new System.Drawing.Point(256, 49);
      this.buttonSaveSearch.Name = "buttonSaveSearch";
      this.buttonSaveSearch.Size = new System.Drawing.Size(160, 30);
      this.buttonSaveSearch.TabIndex = 1;
      this.buttonSaveSearch.Text = "3) Download && Save Details";
      this.toolTip1.SetToolTip(this.buttonSaveSearch, "Download selected matches and save results to MediaPortal!");
      this.buttonSaveSearch.UseVisualStyleBackColor = true;
      this.buttonSaveSearch.Click += new System.EventHandler(this.btnSaveSearch_Click);
      // 
      // buttonStartSearch
      // 
      this.buttonStartSearch.Enabled = false;
      this.buttonStartSearch.Location = new System.Drawing.Point(256, 17);
      this.buttonStartSearch.Name = "buttonStartSearch";
      this.buttonStartSearch.Size = new System.Drawing.Size(160, 30);
      this.buttonStartSearch.TabIndex = 0;
      this.buttonStartSearch.Text = "1) Start Search";
      this.toolTip1.SetToolTip(this.buttonStartSearch, "Search Details for all the checked files");
      this.buttonStartSearch.UseVisualStyleBackColor = true;
      this.buttonStartSearch.Click += new System.EventHandler(this.btnStartSearch_Click);
      // 
      // listViewFileList
      // 
      this.listViewFileList.AllowDrop = true;
      this.listViewFileList.AllowRowReorder = false;
      this.listViewFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewFileList.CheckBoxes = true;
      this.listViewFileList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FileTitle,
            this.status});
      this.listViewFileList.ContextMenu = this.menuFileList;
      this.listViewFileList.FullRowSelect = true;
      this.listViewFileList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.listViewFileList.HideSelection = false;
      this.listViewFileList.Location = new System.Drawing.Point(6, 20);
      this.listViewFileList.Name = "listViewFileList";
      this.listViewFileList.Size = new System.Drawing.Size(296, 344);
      this.listViewFileList.TabIndex = 13;
      this.listViewFileList.UseCompatibleStateImageBehavior = false;
      this.listViewFileList.View = System.Windows.Forms.View.Details;
      this.listViewFileList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewFileList_ItemChecked);
      this.listViewFileList.SelectedIndexChanged += new System.EventHandler(this.listViewFileList_SelectedIndexChanged);
      // 
      // FileTitle
      // 
      this.FileTitle.Text = "Title";
      this.FileTitle.Width = 188;
      // 
      // status
      // 
      this.status.Text = "Status";
      this.status.Width = 102;
      // 
      // menuFileList
      // 
      this.menuFileList.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuCheckWithoutImages,
            this.mnuCheckWithoutOverview});
      // 
      // mnuCheckWithoutImages
      // 
      this.mnuCheckWithoutImages.Index = 0;
      this.mnuCheckWithoutImages.Text = "Check all files without images";
      this.mnuCheckWithoutImages.Click += new System.EventHandler(this.mnuCheckWithoutImages_Click);
      // 
      // mnuCheckWithoutOverview
      // 
      this.mnuCheckWithoutOverview.Index = 1;
      this.mnuCheckWithoutOverview.Text = "Check all files without an overview";
      this.mnuCheckWithoutOverview.Click += new System.EventHandler(this.mnuCheckWithoutOverview_Click);
      // 
      // menuSaveDetails
      // 
      this.menuSaveDetails.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuDataAndImages,
            this.menuItem4,
            this.menuData,
            this.menuImages});
      // 
      // menuDataAndImages
      // 
      this.menuDataAndImages.Index = 0;
      this.menuDataAndImages.Text = "Save Data and download images";
      this.menuDataAndImages.Click += new System.EventHandler(this.menuDataAndImages_Click);
      // 
      // menuItem4
      // 
      this.menuItem4.Index = 1;
      this.menuItem4.Text = "-";
      // 
      // menuData
      // 
      this.menuData.Index = 2;
      this.menuData.Text = "Save Data only";
      this.menuData.Click += new System.EventHandler(this.menuData_Click);
      // 
      // menuImages
      // 
      this.menuImages.Index = 3;
      this.menuImages.Text = "Download images only";
      this.menuImages.Click += new System.EventHandler(this.menuImages_Click);
      // 
      // groupBoxMatchList
      // 
      this.groupBoxMatchList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxMatchList.Controls.Add(this.labelLaunchUrlInfo);
      this.groupBoxMatchList.Controls.Add(this.listViewMatchList);
      this.groupBoxMatchList.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxMatchList.Location = new System.Drawing.Point(3, 3);
      this.groupBoxMatchList.Name = "groupBoxMatchList";
      this.groupBoxMatchList.Size = new System.Drawing.Size(307, 393);
      this.groupBoxMatchList.TabIndex = 16;
      this.groupBoxMatchList.TabStop = false;
      this.groupBoxMatchList.Text = "MatchList";
      // 
      // labelLaunchUrlInfo
      // 
      this.labelLaunchUrlInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.labelLaunchUrlInfo.AutoSize = true;
      this.labelLaunchUrlInfo.Location = new System.Drawing.Point(10, 372);
      this.labelLaunchUrlInfo.Name = "labelLaunchUrlInfo";
      this.labelLaunchUrlInfo.Size = new System.Drawing.Size(153, 13);
      this.labelLaunchUrlInfo.TabIndex = 15;
      this.labelLaunchUrlInfo.Text = "Click on item to launch website";
      // 
      // listViewMatchList
      // 
      this.listViewMatchList.AllowDrop = true;
      this.listViewMatchList.AllowRowReorder = false;
      this.listViewMatchList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewMatchList.CheckBoxes = true;
      this.listViewMatchList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
      this.listViewMatchList.FullRowSelect = true;
      this.listViewMatchList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.listViewMatchList.Location = new System.Drawing.Point(6, 20);
      this.listViewMatchList.MultiSelect = false;
      this.listViewMatchList.Name = "listViewMatchList";
      this.listViewMatchList.Size = new System.Drawing.Size(295, 344);
      this.listViewMatchList.TabIndex = 14;
      this.listViewMatchList.UseCompatibleStateImageBehavior = false;
      this.listViewMatchList.View = System.Windows.Forms.View.Details;
      this.listViewMatchList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewMatchList_ItemChecked);
      this.listViewMatchList.SelectedIndexChanged += new System.EventHandler(this.listViewMatchList_SelectedIndexChanged);
      this.listViewMatchList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listViewMatchList_ItemCheck);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Title (Platform)";
      this.columnHeader1.Width = 179;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Relevance";
      this.columnHeader2.Width = 80;
      // 
      // groupBoxFileList
      // 
      this.groupBoxFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxFileList.Controls.Add(this.checkboxFileList);
      this.groupBoxFileList.Controls.Add(this.listViewFileList);
      this.groupBoxFileList.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxFileList.Location = new System.Drawing.Point(3, 3);
      this.groupBoxFileList.Name = "groupBoxFileList";
      this.groupBoxFileList.Size = new System.Drawing.Size(308, 393);
      this.groupBoxFileList.TabIndex = 16;
      this.groupBoxFileList.TabStop = false;
      this.groupBoxFileList.Text = "FileList";
      // 
      // checkboxFileList
      // 
      this.checkboxFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.checkboxFileList.AutoSize = true;
      this.checkboxFileList.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkboxFileList.Location = new System.Drawing.Point(6, 370);
      this.checkboxFileList.Name = "checkboxFileList";
      this.checkboxFileList.Size = new System.Drawing.Size(116, 17);
      this.checkboxFileList.TabIndex = 14;
      this.checkboxFileList.Text = "Select / deselect all";
      this.checkboxFileList.UseVisualStyleBackColor = true;
      this.checkboxFileList.CheckedChanged += new System.EventHandler(this.checkboxFileList_CheckedChanged);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.groupBoxFileList);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.groupBoxMatchList);
      this.splitContainer1.Size = new System.Drawing.Size(631, 399);
      this.splitContainer1.SplitterDistance = 314;
      this.splitContainer1.TabIndex = 17;
      // 
      // statusStrip
      // 
      this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripProgressBar,
            this.toolStripDropDownButton1});
      this.statusStrip.Location = new System.Drawing.Point(0, 485);
      this.statusStrip.Name = "statusStrip";
      this.statusStrip.Size = new System.Drawing.Size(631, 22);
      this.statusStrip.TabIndex = 18;
      this.statusStrip.Text = "statusStrip1";
      // 
      // toolStripStatusLabel
      // 
      this.toolStripStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.toolStripStatusLabel.Name = "toolStripStatusLabel";
      this.toolStripStatusLabel.Size = new System.Drawing.Size(285, 17);
      this.toolStripStatusLabel.Spring = true;
      this.toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // toolStripProgressBar
      // 
      this.toolStripProgressBar.Name = "toolStripProgressBar";
      this.toolStripProgressBar.Size = new System.Drawing.Size(300, 16);
      // 
      // toolStripDropDownButton1
      // 
      this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cancelSearchToolStripMenuItem});
      this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
      this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
      this.toolStripDropDownButton1.Size = new System.Drawing.Size(29, 20);
      this.toolStripDropDownButton1.Text = "toolStripDropDownButton1";
      // 
      // cancelSearchToolStripMenuItem
      // 
      this.cancelSearchToolStripMenuItem.Name = "cancelSearchToolStripMenuItem";
      this.cancelSearchToolStripMenuItem.ShowShortcutKeys = false;
      this.cancelSearchToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
      this.cancelSearchToolStripMenuItem.Text = "Cancel Search";
      this.cancelSearchToolStripMenuItem.Click += new System.EventHandler(this.cancelSearchToolStripMenuItem_Click);
      // 
      // FileInfoScraperForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
      this.ClientSize = new System.Drawing.Size(631, 507);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.bottomPanel);
      this.Controls.Add(this.statusStrip);
      this.Name = "FileInfoScraperForm";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Search fileinfo";
      this.bottomPanel.ResumeLayout(false);
      this.bottomPanel.PerformLayout();
      this.groupBoxFilter.ResumeLayout(false);
      this.groupBoxFilter.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.MinRelevanceNum)).EndInit();
      this.groupBoxMatchList.ResumeLayout(false);
      this.groupBoxMatchList.PerformLayout();
      this.groupBoxFileList.ResumeLayout(false);
      this.groupBoxFileList.PerformLayout();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.statusStrip.ResumeLayout(false);
      this.statusStrip.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion
  }
}
