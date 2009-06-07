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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace MPTail
{
  public partial class frmMain : Form
  {
    private readonly string mpLogPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                        @"\Team Mediaportal\Mediaportal\Log\";

    private readonly string tveLogPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                         @"\Team Mediaportal\MediaPortal TV Server\log\";

    private LastSettings lastSettings;
    private readonly List<string> customFiles;
    private readonly List<TailedRichTextBox> loggerCollection;

    public frmMain()
    {
      InitializeComponent();
      loggerCollection = new List<TailedRichTextBox>();
      customFiles = new List<string>();

      lastSettings.left = Left;
      lastSettings.top = Top;
      lastSettings.width = Width;
      lastSettings.height = Height;
      lastSettings.categoryIndex = 0;
      lastSettings.tabIndex = 0;

      LoadSettings();
      AddAllLoggers();
    }

    #region Persistance

    private void LoadSettings()
    {
      string font_family = Font.FontFamily.Name;
      float font_size = Font.Size;

      if (File.Exists("MPTailConfig.xml"))
      {
        XmlDocument doc = new XmlDocument();
        doc.Load("MPTailConfig.xml");
        XmlNode node = doc.SelectSingleNode("/mptail/config");
        font_family = node.Attributes["font-family"].Value;
        font_size = float.Parse(node.Attributes["font-size"].Value);
        cbClearOnCreate.Checked = (node.Attributes["clear-log-on-create"].Value == "1");

        string windowStateStr = node.Attributes["WindowState"].Value;
        switch (windowStateStr)
        {
          case "Normal":
            lastSettings.windowState = FormWindowState.Normal;
            break;
          case "Maximized":
            lastSettings.windowState = FormWindowState.Maximized;
            break;
          case "Minimized":
            lastSettings.windowState = FormWindowState.Minimized;
            break;
        }
        if (lastSettings.windowState == FormWindowState.Normal)
        {
          lastSettings.left = Int32.Parse(node.Attributes["WindowPosX"].Value);
          lastSettings.top = Int32.Parse(node.Attributes["WindowPosY"].Value);
          lastSettings.height = Int32.Parse(node.Attributes["WindowHeight"].Value);
          lastSettings.width = Int32.Parse(node.Attributes["WindowWidth"].Value);
        }
        else
          WindowState = lastSettings.windowState;
        lastSettings.categoryIndex = Int32.Parse(node.Attributes["CategoryTabIndex"].Value);
        lastSettings.tabIndex = Int32.Parse(node.Attributes["LoggerIndex"].Value);

        XmlNodeList customNodes = doc.SelectNodes("/mptail/loggers/Custom");
        foreach (XmlNode cnode in customNodes)
          customFiles.Add(cnode.ChildNodes[0].ChildNodes[0].Attributes["filename"].Value);
      }
      Font = new Font(new FontFamily(font_family), font_size);
    }

    private void SaveSettings()
    {
      XmlDocument doc = new XmlDocument();
      XmlNode root = doc.CreateElement("mptail");
      XmlNode config = doc.CreateElement("config");

      XmlUtils.NewAttribute(config, "font-family", Font.FontFamily.Name);
      XmlUtils.NewAttribute(config, "font-size", Font.Size);
      XmlUtils.NewAttribute(config, "clear-log-on-create", cbClearOnCreate.Checked);

      if (WindowState == FormWindowState.Normal)
      {
        XmlUtils.NewAttribute(config, "WindowPosX", Left);
        XmlUtils.NewAttribute(config, "WindowPosY", Top);
        XmlUtils.NewAttribute(config, "WindowHeight", Height);
        XmlUtils.NewAttribute(config, "WindowWidth", Width);
      }
      XmlUtils.NewAttribute(config, "WindowState", WindowState.ToString());

      XmlUtils.NewAttribute(config, "CategoryTabIndex", PageCtrlCategory.SelectedIndex);
      switch (PageCtrlCategory.SelectedIndex)
      {
        case 0:
          XmlUtils.NewAttribute(config, "LoggerIndex", MPTabCtrl.SelectedIndex);
          break;
        case 1:
          XmlUtils.NewAttribute(config, "LoggerIndex", TVETabCtrl.SelectedIndex);
          break;
        case 2:
          XmlUtils.NewAttribute(config, "LoggerIndex", CustomTabCtrl.SelectedIndex);
          break;
      }

      foreach (TailedRichTextBox tr in loggerCollection)
        tr.SaveSettings(doc, root);
      root.AppendChild(config);
      doc.AppendChild(root);
      doc.Save("MPTailConfig.xml");
    }

    #endregion

    private void AddLogger(string filename, TabControl ctrl)
    {
      TabPage tab = new TabPage(Path.GetFileNameWithoutExtension(filename));
      LoggerCategory cat = LoggerCategory.MediaPortal;
      if (ctrl == TVETabCtrl)
        cat = LoggerCategory.TvEngine;
      else if (ctrl == CustomTabCtrl)
        cat = LoggerCategory.Custom;
      TailedRichTextBox tr = new TailedRichTextBox(filename, cat, tab);
      tr.WordWrap = false;
      tab.Controls.Add(tr);
      tr.Dock = DockStyle.Fill;
      ctrl.TabPages.Add(tab);
      tab.Tag = tr;
      loggerCollection.Add(tr);
    }

    private void AddAllLoggers()
    {
      AddLogger(mpLogPath + "MediaPortal.log", MPTabCtrl);
      AddLogger(mpLogPath + "Configuration.log", MPTabCtrl);
      AddLogger(mpLogPath + "TsReader.log", MPTabCtrl);
      AddLogger(mpLogPath + "Error.log", MPTabCtrl);
      AddLogger(mpLogPath + "Recorder.log", MPTabCtrl);
      AddLogger(mpLogPath + "EVR.log", MPTabCtrl);
      AddLogger(mpLogPath + "VMR9.log", MPTabCtrl);

      AddLogger(tveLogPath + "TV.log", TVETabCtrl);
      AddLogger(tveLogPath + "Error.log", TVETabCtrl);
      AddLogger(tveLogPath + "TsWriter.log", TVETabCtrl);
      AddLogger(tveLogPath + "EPG.log", TVETabCtrl);
      AddLogger(tveLogPath + "Player.log", TVETabCtrl);
      AddLogger(tveLogPath + "Streaming Server.log", TVETabCtrl);

      foreach (string logfile in customFiles)
        AddLogger(logfile, CustomTabCtrl);
    }

    private string FormatFileSize(long fs)
    {
      if (fs < 1024)
        return " (" + fs.ToString() + " bytes)";
      if (fs < 1024*1024)
      {
        long kb = fs/1024;
        return " (" + kb.ToString() + " kb)";
      }
      long mb = (fs/1024)/1024;
      return " (" + mb.ToString() + " MB)";
    }

    private string FormatCombinedLogLine(string logger, DateTime dt, string line, int maxLoggerSize)
    {
      string s = dt.ToShortDateString() + " " + dt.ToLongTimeString() + ".";
      string milli = dt.Millisecond.ToString();
      while (milli.Length < 3)
        milli += "0";
      s += milli;
      while (logger.Length < maxLoggerSize)
        logger = " " + logger;
      s += " [" + logger + "] " + line;
      return s;
    }

    private void ProcessAllLoggers()
    {
      SortedDictionary<MyDateTime, string> mpCombined = new SortedDictionary<MyDateTime, string>();
      SortedDictionary<MyDateTime, string> tveCombined = new SortedDictionary<MyDateTime, string>();
      foreach (TailedRichTextBox tr in loggerCollection)
      {
        string currentCaption = tr.ParentTab.Text;
        string newText;
        string newCaption = Path.GetFileNameWithoutExtension(tr.Filename) + FormatFileSize(tr.Process(out newText));
        if (currentCaption == newCaption) continue;

        tr.ParentTab.Text = newCaption;
        if (tr.Category == LoggerCategory.Custom) continue;

        string[] lines = newText.Split(new char[] {'\n'});
        foreach (string line in lines)
        {
          if (line == "") continue;
          if (line.Length < 13) continue;
          int idx = line.IndexOf(' ', 13);
          if (idx == -1) continue;
          string dtStr = line.Substring(0, line.IndexOf(' ', 13));
          string nline = line.Remove(0, line.IndexOf(' ', 13) + 1);
          DateTime dt;
          if (!DateTime.TryParse(dtStr, out dt))
            continue;
          if (tr.Category == LoggerCategory.TvEngine)
            tveCombined.Add(new MyDateTime(dt),
                            FormatCombinedLogLine(Path.GetFileNameWithoutExtension(tr.Filename), dt, nline, 16));
          else
            mpCombined.Add(new MyDateTime(dt),
                           FormatCombinedLogLine(Path.GetFileNameWithoutExtension(tr.Filename), dt, nline, 13));
        }
      }
      foreach (string cline in tveCombined.Values)
        richTextBoxTvEngine.AppendText(cline);
      if (cbFollowTail.Checked)
        richTextBoxTvEngine.Focus();
      foreach (string cline in mpCombined.Values)
        richTextBoxMP.AppendText(cline);
      if (cbFollowTail.Checked)
        richTextBoxMP.Focus();
    }

    private void Form1_Shown(object sender, EventArgs e)
    {
      Left = lastSettings.left;
      Top = lastSettings.top;
      Width = lastSettings.width;
      Height = lastSettings.height;
      PageCtrlCategory.SelectedIndex = lastSettings.categoryIndex;
      if (lastSettings.tabIndex != -1)
      {
        switch (PageCtrlCategory.SelectedIndex)
        {
          case 0:
            MPTabCtrl.SelectedIndex = lastSettings.tabIndex;
            break;
          case 1:
            TVETabCtrl.SelectedIndex = lastSettings.tabIndex;
            break;
          case 2:
            CustomTabCtrl.SelectedIndex = lastSettings.tabIndex;
            break;
        }
      }
      timer1.Enabled = true;
    }

    private void ScrollInView(TabPage tab)
    {
      RichTextBox tr = (RichTextBox) tab.Tag;
      if (tr != null)
      {
        tr.SelectionStart = tr.TextLength;
        tr.ScrollToCaret();
      }
    }

    #region Events

    private void MPTabCtrl_Selected(object sender, TabControlEventArgs e)
    {
      ScrollInView(e.TabPage);
    }

    private void button1_Click(object sender, EventArgs e)
    {
      FontDialog dlg = new FontDialog();
      dlg.Font = Font;
      if (dlg.ShowDialog() == DialogResult.OK)
        Font = dlg.Font;
    }

    private void cbFollowTail_CheckedChanged(object sender, EventArgs e)
    {
      foreach (TailedRichTextBox tr in loggerCollection)
        tr.FollowMe = cbFollowTail.Checked;
    }

    private void cbClearOnCreate_CheckedChanged(object sender, EventArgs e)
    {
      foreach (TailedRichTextBox tr in loggerCollection)
        tr.ClearLogOnCreate = cbClearOnCreate.Checked;
    }

    private void PageCtrlCategory_Selected(object sender, TabControlEventArgs e)
    {
      btnAddLogfile.Visible = false;
      btnRemoveLog.Visible = false;
      if (e.TabPage == tabPage1)
        ScrollInView(MPTabCtrl.TabPages[MPTabCtrl.SelectedIndex]);
      else if (e.TabPage == tabPage2)
        ScrollInView(TVETabCtrl.TabPages[TVETabCtrl.SelectedIndex]);
      else
      {
        if (CustomTabCtrl.TabCount > 0)
          ScrollInView(CustomTabCtrl.TabPages[CustomTabCtrl.SelectedIndex]);
        btnAddLogfile.Visible = true;
        btnRemoveLog.Visible = true;
      }
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      timer1.Enabled = false;
      ProcessAllLoggers();
      timer1.Enabled = true;
    }

    private void btnAddLogfile_Click(object sender, EventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.CheckFileExists = true;
      dlg.CheckPathExists = true;
      dlg.Multiselect = false;
      dlg.RestoreDirectory = true;
      dlg.Filter = "All files (*.*)|*.*";
      if (dlg.ShowDialog() == DialogResult.OK)
        AddLogger(dlg.FileName, CustomTabCtrl);
    }

    private void btnRemoveLog_Click(object sender, EventArgs e)
    {
      TabPage tab = CustomTabCtrl.SelectedTab;
      if (tab == null) return;
      TailedRichTextBox tr = (TailedRichTextBox) tab.Tag;
      if (
        MessageBox.Show("Do you really want to remove the logfile [" + tr.Filename + "] ?", "Confirmation",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
      {
        loggerCollection.Remove(tr);
        CustomTabCtrl.TabPages.Remove(tab);
      }
    }

    private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
      RichTextBox rtb = richTextBoxMP;
      if (richTextBoxTvEngine.Focused)
        rtb = richTextBoxTvEngine;
      SaveFileDialog dlg = new SaveFileDialog();
      dlg.CheckPathExists = true;
      dlg.Filter = "All files (*.*)|*.*";
      dlg.OverwritePrompt = true;
      dlg.RestoreDirectory = true;
      if (dlg.ShowDialog() == DialogResult.OK)
        rtb.SaveFile(dlg.FileName);
    }

    private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
      SaveSettings();
    }

    #endregion

    private void searchToolStripMenuItem_Click(object sender, EventArgs e)
    {
      frmFindSettings dlg = new frmFindSettings();
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      RingBufferedRichTextBox rb = richTextBoxMP;
      if (richTextBoxTvEngine.Focused)
        rb = richTextBoxTvEngine;
      rb.Find(dlg.SearchString, dlg.Options);
    }
  }
}