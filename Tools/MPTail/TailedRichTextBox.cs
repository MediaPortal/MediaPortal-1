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
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MPTail
{
  public class TailedRichTextBox : RingBufferedRichTextBox
  {
    #region Variables

    private bool followMe = true;
    private bool clearOnCreate = true;
    private string filename;
    private int maxBytes = 1024*16;
    private long previousSeekPosition;
    private long previousFileSize;
    private readonly TabPage parentTab;
    private readonly SearchParameters searchParams;
    private readonly ContextMenuStrip ctxMenu;
    public LoggerCategory Category;

    #endregion

    #region constructor

    public TailedRichTextBox(string filename, LoggerCategory loggerCategory, TabPage parentTabPage)
    {
      this.filename = filename;
      ShortcutsEnabled = true;
      Category = loggerCategory;
      parentTab = parentTabPage;
      previousSeekPosition = 0;
      previousFileSize = 0;
      ctxMenu = new ContextMenuStrip();
      ContextMenuStrip = ctxMenu;
      if (Category != LoggerCategory.Custom)
      {
        ToolStripItem relocateItem = ctxMenu.Items.Add("Correct logfile location");
        relocateItem.Click += new EventHandler(relocateItem_Click);
      }
      ctxMenu.Items.Add("-");
      ToolStripItem searchItem = ctxMenu.Items.Add("Find");
      searchItem.Click += new EventHandler(searchItem_Click);
      ctxMenu.Items.Add("-");

      ToolStripItem cfgItem = ctxMenu.Items.Add("Configure search parameters");
      cfgItem.Click += new EventHandler(Config_Click);
      ctxMenu.Items.Add("-");
      ToolStripItem clearWindowItem = ctxMenu.Items.Add("Clear window");
      clearWindowItem.Click += new EventHandler(clearWindowItem_Click);
      ToolStripItem clearFileItem = ctxMenu.Items.Add("Delete file");
      clearFileItem.Click += new EventHandler(clearFileItem_Click);
      searchParams = new SearchParameters();
      LoadSettings();
    }

    #endregion

    #region Contextmenu handlers

    private void searchItem_Click(object sender, EventArgs e)
    {
      frmFindSettings dlg = new frmFindSettings();
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      Find(dlg.SearchString, dlg.Options);
    }

    private void clearFileItem_Click(object sender, EventArgs e)
    {
      if (
        MessageBox.Show("Do you really want to delete this file?", "Confirmation", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
      {
        File.Delete(filename);
        Text = "";
      }
    }

    private void clearWindowItem_Click(object sender, EventArgs e)
    {
      Text = "";
    }

    private void relocateItem_Click(object sender, EventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.CheckFileExists = true;
      dlg.CheckPathExists = true;
      dlg.Multiselect = false;
      dlg.Filter = Path.GetFileName(filename) + "|" + Path.GetFileName(filename);
      if (dlg.ShowDialog() == DialogResult.OK)
        filename = dlg.FileName;
    }

    private void Config_Click(object sender, EventArgs e)
    {
      frmSearchParams dlg = new frmSearchParams("Hightlight settings for [" + Path.GetFileName(filename) + "]",
                                                searchParams);
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        dlg.GetConfig(searchParams);
        SelectAll();
        SelectionBackColor = Color.White;
        SelectionFont = new Font(Font, FontStyle.Regular);
        HighlightSearchTerms(0);
      }
    }

    #endregion

    #region Properties

    public TabPage ParentTab
    {
      get { return parentTab; }
    }

    public string Filename
    {
      get { return filename; }
    }

    public int MaxBytes
    {
      get { return maxBytes; }
      set { maxBytes = value; }
    }

    public bool FollowMe
    {
      get { return followMe; }
      set { followMe = value; }
    }

    public bool ClearLogOnCreate
    {
      get { return clearOnCreate; }
      set { clearOnCreate = value; }
    }

    #endregion

    #region Persistance

    private void LoadSettings()
    {
      if (File.Exists("MPTailConfig.xml"))
      {
        XmlDocument doc = new XmlDocument();
        doc.Load("MPTailConfig.xml");
        string name = Path.GetFileNameWithoutExtension(filename);
        XmlNode node = null;
        try
        {
          node =
            doc.SelectSingleNode("/mptail/loggers/" + Category.ToString() + "/" + name.Replace(' ', '_') + "/config");
        }
        catch (Exception)
        {
          return;
        }
        if (node == null) return;
        string fname = node.Attributes["filename"].Value;
        if (filename != fname)
          filename = fname;
        searchParams.searchStr = node.Attributes["search-string"].Value;
        string color = node.Attributes["search-highlight-color"].Value;
        searchParams.highlightColor = Color.FromArgb(Int32.Parse(color));
        searchParams.caseSensitive = (node.Attributes["search-casesensitive"].Value == "1");
      }
    }

    public void SaveSettings(XmlDocument doc, XmlNode n_root)
    {
      XmlNode n_logger = doc.CreateElement("loggers");
      XmlNode n_category = doc.CreateElement(Category.ToString());
      string name = Path.GetFileNameWithoutExtension(filename);
      XmlNode n_name = doc.CreateElement(name.Replace(' ', '_'));
      XmlNode n_config = doc.CreateElement("config");

      XmlUtils.NewAttribute(n_config, "filename", filename);
      XmlUtils.NewAttribute(n_config, "search-string", searchParams.searchStr);
      XmlUtils.NewAttribute(n_config, "search-highlight-color", searchParams.highlightColor);
      XmlUtils.NewAttribute(n_config, "search-casesensitive", searchParams.caseSensitive);

      n_name.AppendChild(n_config);
      n_category.AppendChild(n_name);
      n_logger.AppendChild(n_category);
      n_root.AppendChild(n_logger);
    }

    #endregion

    #region public members

    public long Process(out string newText)
    {
      newText = "";
      if (!File.Exists(filename))
      {
        previousSeekPosition = 0;
        return 0;
      }
      if (previousSeekPosition == 0 && clearOnCreate)
        Text = "";
      byte[] bytesRead = new byte[maxBytes];
      FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
      if (fs.Length < previousFileSize)
      {
        // new file
        if (clearOnCreate)
          Text = "";
        previousSeekPosition = 0;
      }
      previousFileSize = fs.Length;
      if (previousFileSize == previousSeekPosition)
      {
        fs.Close();
        return previousFileSize;
      }
      if (fs.Length > maxBytes)
        previousSeekPosition = fs.Length - maxBytes;
      previousSeekPosition = (int) fs.Seek(previousSeekPosition, SeekOrigin.Begin);
      int numBytes = fs.Read(bytesRead, 0, maxBytes);
      fs.Close();
      previousSeekPosition += numBytes;

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < numBytes; i++)
        sb.Append((char) bytesRead[i]);
      long lastPos = TextLength;
      AppendText(sb.ToString());
      newText = sb.ToString();
      HighlightSearchTerms(lastPos);
      if (followMe)
        Focus();
      return previousFileSize;
    }

    #endregion

    #region private members

    private void HighlightSearchTerms(long lastPos)
    {
      if (searchParams.searchStr == "") return;
      StringComparison comp = StringComparison.InvariantCultureIgnoreCase;
      if (searchParams.caseSensitive)
        comp = StringComparison.InvariantCulture;
      while ((lastPos = Text.IndexOf(searchParams.searchStr, (int) lastPos, comp)) != -1)
      {
        SelectionStart = (int) lastPos;
        SelectionLength = searchParams.searchStr.Length;
        SelectionFont = new Font(Font, FontStyle.Bold);
        SelectionBackColor = searchParams.highlightColor;
        lastPos += searchParams.searchStr.Length;
        if (lastPos >= Text.Length) break;
      }
      SelectionStart = TextLength;
    }

    #endregion
  }
}