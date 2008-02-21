using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MPTail
{
  public partial class frmMain : Form
  {
    private string mpLogPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)+@"\Team Mediaportal\Mediaportal\Log\";
    private string tveLogPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\MediaPortal TV Server\log\";
    private List<TailedRichTextBox> loggerCollection;

    public frmMain()
    {
      InitializeComponent();
      if (!Directory.Exists(mpLogPath))
        mpLogPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Team MediaPortal\MediaPortal\Log\";
      loggerCollection = new List<TailedRichTextBox>();
      LoadSettings();
      AddAllLoggers();
    }

    #region Persistance
    private void LoadSettings()
    {
      string font_family=this.Font.FontFamily.Name;
      float font_size=this.Font.Size;

      if (File.Exists("MPTailConfig.xml"))
      {
        XmlDocument doc = new XmlDocument();
        doc.Load("MPTailConfig.xml");
        XmlNode node = doc.SelectSingleNode("/mptail/config");
        font_family = node.Attributes["font-family"].Value;
        font_size = float.Parse(node.Attributes["font-size"].Value);
        cbClearOnCreate.Checked = (node.Attributes["clear_log_on_create"].Value == "1");
      }
      this.Font = new Font(new FontFamily(font_family), font_size);
    }
    private void SaveSettings()
    {
      XmlDocument doc = new XmlDocument();
      XmlNode root = doc.CreateElement("mptail");
      XmlNode config = doc.CreateElement("config");
      XmlAttribute font_family=config.OwnerDocument.CreateAttribute("font-family");
      font_family.InnerText=this.Font.FontFamily.Name;
      config.Attributes.Append(font_family);
      XmlAttribute font_size=config.OwnerDocument.CreateAttribute("font-size");
      font_size.InnerText=this.Font.Size.ToString();
      config.Attributes.Append(font_size);
      XmlAttribute clear_log = config.OwnerDocument.CreateAttribute("clear_log_on_create");
      if (cbClearOnCreate.Checked)
        clear_log.InnerText = "1";
      else
        clear_log.InnerText = "0";
      config.Attributes.Append(clear_log);

      foreach (TailedRichTextBox tr in loggerCollection)
        tr.SaveSettings(doc,root);
      root.AppendChild(config);
      doc.AppendChild(root);
      doc.Save("MPTailConfig.xml");
    }
    #endregion

    private void AddLogger(string filename, TabControl ctrl)
    {
      TabPage tab = new TabPage(Path.GetFileNameWithoutExtension(filename));
      TailedRichTextBox tr = new TailedRichTextBox(filename,tab);
      tr.OnSaveSettings += new TailedRichTextBox.SaveSettingsHandler(Logger_OnSaveSettings);
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

      AddLogger(tveLogPath + "TV.log", TVETabCtrl);
      AddLogger(tveLogPath + "Error.log", TVETabCtrl);
      AddLogger(tveLogPath + "TsWriter.log", TVETabCtrl);
      AddLogger(tveLogPath + "EPG.log", TVETabCtrl);
      AddLogger(tveLogPath + "Player.log", TVETabCtrl);
      AddLogger(tveLogPath + "Streaming Server.log", TVETabCtrl);
    }
    private string FormatFileSize(long fs)
    {
      if (fs < 1024)
        return " ("+fs.ToString() + " bytes)";
      if (fs < 1024 * 1024)
      {
        long kb = fs / 1024;
        return " (" + kb.ToString() + " kb)";
      }
      long mb = (fs / 1024) / 1024;
      return " (" + mb.ToString() + " MB)";
    }
    private void ProcessAllLoggers()
    {
      foreach (TailedRichTextBox tr in loggerCollection)
      {
        string currentCaption = tr.ParentTab.Text;
        string newCaption = Path.GetFileNameWithoutExtension(tr.Filename) + FormatFileSize(tr.Process());
        if (currentCaption != newCaption)
          tr.ParentTab.Text = newCaption;
      }
    }

    private void Form1_Shown(object sender, EventArgs e)
    {
      timer1.Enabled = true;
    }
    private void ScrollInView(TabPage tab)
    {
      RichTextBox tr = (RichTextBox)tab.Tag;
      if (tr != null)
      {
        tr.SelectionStart = tr.TextLength;
        tr.ScrollToCaret();
      }
    }

    #region Events
    void Logger_OnSaveSettings()
    {
      SaveSettings();
    }
    private void MPTabCtrl_Selected(object sender, TabControlEventArgs e)
    {
      ScrollInView(e.TabPage);
    }

    private void button1_Click(object sender, EventArgs e)
    {
      FontDialog dlg = new FontDialog();
      dlg.Font = this.Font;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        this.Font = dlg.Font;
        SaveSettings();
      }
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
      SaveSettings();
    }
    private void PageCtrlCategory_Selected(object sender, TabControlEventArgs e)
    {
      if (e.TabPage == tabPage1)
        ScrollInView(MPTabCtrl.TabPages[MPTabCtrl.SelectedIndex]);
      else
        ScrollInView(TVETabCtrl.TabPages[MPTabCtrl.SelectedIndex]);
    }
    #endregion

    private void timer1_Tick(object sender, EventArgs e)
    {
      timer1.Enabled = false;
      ProcessAllLoggers();
      timer1.Enabled = true;
    }
  }
}