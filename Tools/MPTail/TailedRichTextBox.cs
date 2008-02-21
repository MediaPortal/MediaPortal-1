using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace MPTail
{
  public class MyDateTime: IComparable
  {
    DateTime dt;
    int counter;

    public MyDateTime(int instanceId,DateTime dateTime)
    {
      counter = instanceId;
      dt=dateTime;
    }
    public MyDateTime(int instanceId,string dateTimeStr)
    {
      counter = instanceId;
      dt = DateTime.Parse(dateTimeStr);
    }

    public int CompareTo(object obj)
    {
      MyDateTime mdt = (MyDateTime)obj;
      int ret=DateTime.Compare(dt, mdt.dt);
      if (ret == 0)
        ret = 1;
      return ret;
    }
  }
  public class SearchParameters
  {
    public string searchStr = "";
    public bool caseSensitive = false;
    public System.Drawing.Color highlightColor = System.Drawing.Color.Yellow;
  }

  public enum LoggerCategory
  {
    MediaPortal,
    TvEngine,
    Custom
  }

  public class TailedRichTextBox: RichTextBox
  {
    #region Variables
    private bool followMe = true;
    private bool clearOnCreate = true;
    private string filename;
    private int maxBytes = 1024 * 16;
    private long previousSeekPosition;
    private long previousFileSize;
    private TabPage parentTab;
    private ContextMenuStrip ctxMenu;
    private SearchParameters searchParams;

    public LoggerCategory Category;
    #endregion

    public delegate void SaveSettingsHandler();
    public event SaveSettingsHandler OnSaveSettings;

    #region constructor
    public TailedRichTextBox(string filename,LoggerCategory loggerCategory,TabPage parentTabPage)
    {
      this.filename = filename;
      Category = loggerCategory;
      parentTab = parentTabPage;
      previousSeekPosition=0;
      previousFileSize = 0;
      ctxMenu = new ContextMenuStrip();
      this.ContextMenuStrip = ctxMenu;
      ToolStripItem item=ctxMenu.Items.Add("Configure search parameters");
      item.Click+=new EventHandler(item_Click);
      searchParams = new SearchParameters();
      LoadSettings();
    }

    void item_Click(object sender, EventArgs e)
    {
      frmSearchParams dlg = new frmSearchParams("Hightlight settings for [" + Path.GetFileName(filename) + "]",searchParams);
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        dlg.GetConfig(searchParams);
        if (Category != LoggerCategory.Custom) 
          OnSaveSettings();
        SelectAll();
        SelectionBackColor = System.Drawing.Color.White;
        SelectionFont = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Regular);
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
      get { return this.filename; }
    }
    public int MaxBytes
    {
      get { return this.maxBytes; }
      set { this.maxBytes = value; }
    }
    public bool FollowMe
    {
      get { return followMe; }
      set { followMe = value; }
    }
    public bool ClearLogOnCreate
    {
      get { return this.clearOnCreate; }
      set { clearOnCreate = value; }
    }
    #endregion

    #region Persistance
    private void LoadSettings()
    {
      if (Category == LoggerCategory.Custom) return;
      if (File.Exists("MPTailConfig.xml"))
      {
        XmlDocument doc = new XmlDocument();
        doc.Load("MPTailConfig.xml");
        string name = Path.GetFileNameWithoutExtension(filename);
        XmlNode node = doc.SelectSingleNode("/mptail/loggers/"+name.Replace(' ','_')+"/config");
        if (node == null) return;
        searchParams.searchStr = node.Attributes["search-string"].Value;
        string color = node.Attributes["search-highlight-color"].Value;
        searchParams.highlightColor = System.Drawing.Color.FromArgb(Int32.Parse(color));
        searchParams.caseSensitive = (node.Attributes["search-casesensitive"].Value == "1");
      }
    }
    public void SaveSettings(XmlDocument doc,XmlNode n_root)
    {
      if (Category == LoggerCategory.Custom) return;
      XmlNode n_logger = doc.CreateElement("loggers");
      string name = Path.GetFileNameWithoutExtension(filename);
      XmlNode n_name = doc.CreateElement(name.Replace(' ','_'));
      XmlNode n_config = doc.CreateElement("config");

      XmlAttribute searchString = n_config.OwnerDocument.CreateAttribute("search-string");
      searchString.InnerText = searchParams.searchStr;
      n_config.Attributes.Append(searchString);

      XmlAttribute hcolor = n_config.OwnerDocument.CreateAttribute("search-highlight-color");
      hcolor.InnerText = searchParams.highlightColor.ToArgb().ToString();
      n_config.Attributes.Append(hcolor);

      XmlAttribute csensitive = n_config.OwnerDocument.CreateAttribute("search-casesensitive");
      if (searchParams.caseSensitive)
        csensitive.InnerText = "1";
      else
        csensitive.InnerText = "0";
      n_config.Attributes.Append(csensitive);

      n_name.AppendChild(n_config);
      n_logger.AppendChild(n_name);
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
        this.Text = "";
      byte[] bytesRead = new byte[maxBytes];
      FileStream fs = new FileStream(this.filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
      if (fs.Length < previousFileSize)
      {
        // new file
        if (clearOnCreate)
          this.Text = "";
        previousSeekPosition = 0;
      }
      previousFileSize = fs.Length;
      if (previousFileSize == previousSeekPosition)
      {
        fs.Close();
        return previousFileSize;
      }
      if (fs.Length > maxBytes)
        this.previousSeekPosition = fs.Length - maxBytes;
      this.previousSeekPosition = (int)fs.Seek(this.previousSeekPosition, SeekOrigin.Begin);
      int numBytes = fs.Read(bytesRead, 0, maxBytes);
      fs.Close();
      this.previousSeekPosition += numBytes;

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < numBytes; i++)
        sb.Append((char)bytesRead[i]);
      long lastPos = this.TextLength;
      this.AppendText(sb.ToString());
      newText = sb.ToString();
      HighlightSearchTerms(lastPos);
      if (followMe)
        this.Focus();
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
      while ((lastPos = this.Text.IndexOf(searchParams.searchStr, (int)lastPos, comp)) != -1)
      {
        this.SelectionStart = (int)lastPos;
        this.SelectionLength = searchParams.searchStr.Length;
        this.SelectionFont = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold);
        this.SelectionBackColor = searchParams.highlightColor;
        lastPos += searchParams.searchStr.Length;
        if (lastPos >= this.Text.Length) break;
      }
      this.SelectionStart = this.TextLength;
    }
    #endregion
  }
}
