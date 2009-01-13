using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration.Sections
{
  public class TVClient : SectionSettings
  {
    #region variables

    private string _preferredAudioLanguages;
    private string _preferredSubLanguages;
    private IList<string> _languageCodes;

    private MPGroupBox mpGroupBox2;
    private MPTextBox mpTextBoxHostname;
    private MPLabel mpLabel3;
    private MPGroupBox mpGroupBox1;
    private MPCheckBox mpCheckBoxPrefAC3;
    private IList<string> _languagesAvail;
    private MPCheckBox mpCheckBoxPrefAudioOverLang;
    private MPTabControl tabControlTVGeneral;
    private MPTabPage tabPageGeneralSettings;
    private MPTabPage tabPageAudioLanguages;
    private MPGroupBox groupBox2;
    private MPTabPage tabPageSubtitles;
    private MPButton mpButtonAddAudioLang;
    private MPButton mpButtonRemoveAudioLang;
    private MPListView mpListViewPreferredAudioLang;
    private MPListView mpListViewAvailAudioLang;
    private MPButton mpButtonDownAudioLang;
    private MPButton mpButtonUpAudioLang;
    private MPLabel mpLabel5;
    private MPLabel mpLabel1;
    private MPGroupBox mpGroupBox3;
    private MPLabel mpLabel6;
    private MPLabel mpLabel7;
    private MPButton mpButtonDownSubLang;
    private MPButton mpButtonUpSubLang;
    private MPButton mpButtonAddSubLang;
    private MPButton mpButtonRemoveSubLang;
    private MPListView mpListViewPreferredSubLang;
    private MPListView mpListViewAvailSubLang;
    private MPGroupBox mpGroupBox4;
    private MPCheckBox mpCheckBoxEnableTTXTSub;
    private MPCheckBox mpCheckBoxEnableDVBSub;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private ColumnHeader columnHeader4;
    private MPGroupBox mpGroupBox5;
    private MPCheckBox cbHideAllChannels;
    private MPGroupBox mpGroupBox6;
    private MPCheckBox cbShowChannelStateIcons;
    private MPCheckBox enableAudioDualMonoModes;
    private ColumnHeader columnHeader3;
    private ColumnHeader columnHeader5;
    private ColumnHeader columnHeader6;
    private ColumnHeader columnHeader8;
    private TabPage tabPage1;
    private MPGroupBox mpGroupBox7;
    private MPCheckBox chkTVnotifications;
    private MPLabel mpLabel2;
    private MPTextBox txtNotifyBefore;
    private MPGroupBox mpGroupBox8;
    private MPCheckBox chkRecnotifications;
    private MPCheckBox checkBoxNotifyPlaySound;
    private MPTextBox txtNotifyAfter;
    private MPLabel labelNotifyTimeout;
    private ColumnHeader columnHeader7;
    private ListViewColumnSorter _columnSorter;

    #endregion

    public TVClient()
      : this("TV Client")
    {
    }

    public TVClient(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void LoadSettings()
    {
      //Load parameters from XML File
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        mpTextBoxHostname.Text = xmlreader.GetValueAsString("tvservice", "hostname", "");
        mpCheckBoxPrefAC3.Checked = xmlreader.GetValueAsBool("tvservice", "preferac3", false);
        mpCheckBoxPrefAudioOverLang.Checked = xmlreader.GetValueAsBool("tvservice", "preferAudioTypeOverLang", true);
        _preferredAudioLanguages = xmlreader.GetValueAsString("tvservice", "preferredaudiolanguages", "");
        _preferredSubLanguages = xmlreader.GetValueAsString("tvservice", "preferredsublanguages", "");

        mpCheckBoxEnableDVBSub.Checked = xmlreader.GetValueAsBool("tvservice", "dvbbitmapsubtitles", false);
        mpCheckBoxEnableTTXTSub.Checked = xmlreader.GetValueAsBool("tvservice", "dvbttxtsubtitles", false);
        enableAudioDualMonoModes.Checked = xmlreader.GetValueAsBool("tvservice", "audiodualmono", false);
        cbHideAllChannels.Checked = xmlreader.GetValueAsBool("mytv", "hideAllChannelsGroup", false);
        cbShowChannelStateIcons.Checked = xmlreader.GetValueAsBool("mytv", "showChannelStateIcons", true);

        chkRecnotifications.Checked = xmlreader.GetValueAsBool("mytv", "enableRecNotifier", false);
        chkTVnotifications.Checked = xmlreader.GetValueAsBool("mytv", "enableTvNotifier", false);
        txtNotifyBefore.Text = xmlreader.GetValueAsString("mytv", "notifyTVBefore", "300");
        txtNotifyAfter.Text = xmlreader.GetValueAsString("mytv", "notifyTVTimeout", "15");
        checkBoxNotifyPlaySound.Checked = xmlreader.GetValueAsBool("mytv", "notifybeep", true);
      }
      chkTVnotifications_CheckedChanged(null, null);
      if (File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"))
      {
        // Enable this Panel if the TvPlugin exists in the plug-in Directory
        this.Enabled = true;

        try
        {
          Assembly assem = Assembly.LoadFrom(Config.GetFolder(Config.Dir.Base) + "\\TvLibrary.Interfaces.dll");
          if (assem != null)
          {
            Type[] types = assem.GetExportedTypes();
            foreach (Type exportedType in types)
            {
              try
              {
                if (exportedType.Name == "Languages")
                {
                  // Load available languages into variables. 
                  Object languageObject = null;
                  languageObject = Activator.CreateInstance(exportedType);
                  MethodInfo methodInfo = exportedType.GetMethod("GetLanguages",
                                                                 BindingFlags.Public | BindingFlags.Instance);
                  _languagesAvail = methodInfo.Invoke(languageObject, null) as List<String>;
                  methodInfo = exportedType.GetMethod("GetLanguageCodes", BindingFlags.Public | BindingFlags.Instance);
                  _languageCodes = (List<String>) methodInfo.Invoke(languageObject, null);

                  if (_languagesAvail == null || _languageCodes == null)
                  {
                    Log.Debug("Failed to load languages");
                    return;
                  }
                  else
                  {
                    mpListViewAvailAudioLang.Items.Clear();
                    mpListViewPreferredAudioLang.Items.Clear();
                    for (int i = 0; i < _languagesAvail.Count; i++)
                    {
                      if (!_preferredAudioLanguages.Contains(_languagesAvail[i]))
                      {
                        ListViewItem item = new ListViewItem(new string[] {_languagesAvail[i], _languageCodes[i]});
                        item.Tag = _languageCodes[i];
                        mpListViewAvailAudioLang.Items.Add(item);
                      }
                    }
                    mpListViewAvailAudioLang.ListViewItemSorter = _columnSorter = new ListViewColumnSorter();
                    _columnSorter.SortColumn = 0;
                    _columnSorter.Order = SortOrder.Ascending;
                    mpListViewAvailAudioLang.Sort();

                    if (_preferredAudioLanguages.Length > 0)
                    {
                      string[] langArr = _preferredAudioLanguages.Split(';');

                      for (int i = 0; i < langArr.Length; i++)
                      {
                        string langStr = langArr[i];
                        if (langStr.Trim().Length > 0)
                        {
                          for (int j = 0; j < _languagesAvail.Count; j++)
                          {
                            if (_languageCodes[j].Contains(langStr))
                            {
                              ListViewItem item = new ListViewItem(new string[] {_languagesAvail[j], _languageCodes[j]});
                              item.Tag = _languageCodes[j];
                              mpListViewPreferredAudioLang.Items.Add(item);
                              break;
                            }
                          }
                        }
                      }
                    }

                    mpListViewAvailSubLang.Items.Clear();
                    mpListViewPreferredSubLang.Items.Clear();
                    for (int i = 0; i < _languagesAvail.Count; i++)
                    {
                      if (!_preferredSubLanguages.Contains(_languagesAvail[i]))
                      {
                        ListViewItem item = new ListViewItem(new string[] {_languagesAvail[i], _languageCodes[i]});
                        item.Tag = _languageCodes[i];
                        mpListViewAvailSubLang.Items.Add(item);
                      }
                    }
                    mpListViewAvailSubLang.ListViewItemSorter = _columnSorter = new ListViewColumnSorter();
                    _columnSorter.SortColumn = 0;
                    _columnSorter.Order = SortOrder.Ascending;
                    mpListViewAvailSubLang.Sort();

                    if (_preferredSubLanguages.Length > 0)
                    {
                      string[] langArr = _preferredSubLanguages.Split(';');

                      for (int i = 0; i < langArr.Length; i++)
                      {
                        string langStr = langArr[i];
                        if (langStr.Trim().Length > 0)
                        {
                          for (int j = 0; j < _languagesAvail.Count; j++)
                          {
                            if (_languageCodes[j].Contains(langStr))
                            {
                              ListViewItem item = new ListViewItem(new string[] {_languagesAvail[j], _languageCodes[j]});
                              item.Tag = _languageCodes[j];
                              mpListViewPreferredSubLang.Items.Add(item);
                              break;
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
              catch (TargetInvocationException ex)
              {
                Log.Warn("TVClient: Failed to load languages {0}", ex.ToString());
                continue;
              }
              catch (Exception gex)
              {
                Log.Warn("TVClient: Failed to load settings {0}", gex.Message);
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Debug("Configuration: Loading TVLibrary.Interface assembly");
          Log.Debug("Configuration: Exception: {0}", ex);
        }
      }
      else
      {
        this.Enabled = false;
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string prefLangs = "";
        xmlwriter.SetValue("tvservice", "hostname", mpTextBoxHostname.Text);
        xmlwriter.SetValueAsBool("tvservice", "preferac3", mpCheckBoxPrefAC3.Checked);
        xmlwriter.SetValueAsBool("tvservice", "preferAudioTypeOverLang", mpCheckBoxPrefAudioOverLang.Checked);

        xmlwriter.SetValueAsBool("tvservice", "dvbbitmapsubtitles", mpCheckBoxEnableDVBSub.Checked);
        xmlwriter.SetValueAsBool("tvservice", "dvbttxtsubtitles", mpCheckBoxEnableTTXTSub.Checked);
        xmlwriter.SetValueAsBool("tvservice", "audiodualmono", enableAudioDualMonoModes.Checked);
        xmlwriter.SetValueAsBool("mytv", "hideAllChannelsGroup", cbHideAllChannels.Checked);
        xmlwriter.SetValueAsBool("mytv", "showChannelStateIcons", cbShowChannelStateIcons.Checked);

        xmlwriter.SetValueAsBool("mytv", "enableRecNotifier", chkRecnotifications.Checked);
        xmlwriter.SetValueAsBool("mytv", "enableTvNotifier", chkTVnotifications.Checked);
        xmlwriter.SetValue("mytv", "notifyTVBefore", txtNotifyBefore.Text);
        xmlwriter.SetValue("mytv", "notifyTVTimeout", txtNotifyAfter.Text);
        xmlwriter.SetValueAsBool("mytv", "notifybeep", checkBoxNotifyPlaySound.Checked);

        foreach (ListViewItem item in mpListViewPreferredAudioLang.Items)
        {
          prefLangs += (string) item.Tag + ";";
        }
        xmlwriter.SetValue("tvservice", "preferredaudiolanguages", prefLangs);

        prefLangs = "";
        foreach (ListViewItem item in mpListViewPreferredSubLang.Items)
        {
          prefLangs += (string) item.Tag + ";";
        }
        xmlwriter.SetValue("tvservice", "preferredsublanguages", prefLangs);
      }
    }

    private void InitializeComponent()
    {
      this.mpGroupBox2 = new MPGroupBox();
      this.mpTextBoxHostname = new MPTextBox();
      this.mpLabel3 = new MPLabel();
      this.mpGroupBox1 = new MPGroupBox();
      this.enableAudioDualMonoModes = new MPCheckBox();
      this.mpCheckBoxPrefAudioOverLang = new MPCheckBox();
      this.mpCheckBoxPrefAC3 = new MPCheckBox();
      this.tabControlTVGeneral = new MPTabControl();
      this.tabPageGeneralSettings = new MPTabPage();
      this.mpGroupBox6 = new MPGroupBox();
      this.cbShowChannelStateIcons = new MPCheckBox();
      this.mpGroupBox5 = new MPGroupBox();
      this.cbHideAllChannels = new MPCheckBox();
      this.tabPageAudioLanguages = new MPTabPage();
      this.groupBox2 = new MPGroupBox();
      this.mpLabel5 = new MPLabel();
      this.mpLabel1 = new MPLabel();
      this.mpButtonDownAudioLang = new MPButton();
      this.mpButtonUpAudioLang = new MPButton();
      this.mpButtonAddAudioLang = new MPButton();
      this.mpButtonRemoveAudioLang = new MPButton();
      this.mpListViewPreferredAudioLang = new MPListView();
      this.columnHeader2 = new ColumnHeader();
      this.columnHeader6 = new ColumnHeader();
      this.mpListViewAvailAudioLang = new MPListView();
      this.columnHeader1 = new ColumnHeader();
      this.columnHeader5 = new ColumnHeader();
      this.tabPageSubtitles = new MPTabPage();
      this.mpGroupBox4 = new MPGroupBox();
      this.mpCheckBoxEnableTTXTSub = new MPCheckBox();
      this.mpCheckBoxEnableDVBSub = new MPCheckBox();
      this.mpGroupBox3 = new MPGroupBox();
      this.mpLabel6 = new MPLabel();
      this.mpLabel7 = new MPLabel();
      this.mpButtonDownSubLang = new MPButton();
      this.mpButtonUpSubLang = new MPButton();
      this.mpButtonAddSubLang = new MPButton();
      this.mpButtonRemoveSubLang = new MPButton();
      this.mpListViewPreferredSubLang = new MPListView();
      this.columnHeader4 = new ColumnHeader();
      this.columnHeader8 = new ColumnHeader();
      this.mpListViewAvailSubLang = new MPListView();
      this.columnHeader3 = new ColumnHeader();
      this.columnHeader7 = new ColumnHeader();
      this.tabPage1 = new TabPage();
      this.mpGroupBox8 = new MPGroupBox();
      this.chkRecnotifications = new MPCheckBox();
      this.mpGroupBox7 = new MPGroupBox();
      this.txtNotifyAfter = new MPTextBox();
      this.labelNotifyTimeout = new MPLabel();
      this.checkBoxNotifyPlaySound = new MPCheckBox();
      this.mpLabel2 = new MPLabel();
      this.txtNotifyBefore = new MPTextBox();
      this.chkTVnotifications = new MPCheckBox();
      this.mpGroupBox2.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.tabControlTVGeneral.SuspendLayout();
      this.tabPageGeneralSettings.SuspendLayout();
      this.mpGroupBox6.SuspendLayout();
      this.mpGroupBox5.SuspendLayout();
      this.tabPageAudioLanguages.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.tabPageSubtitles.SuspendLayout();
      this.mpGroupBox4.SuspendLayout();
      this.mpGroupBox3.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.mpGroupBox8.SuspendLayout();
      this.mpGroupBox7.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Controls.Add(this.mpTextBoxHostname);
      this.mpGroupBox2.Controls.Add(this.mpLabel3);
      this.mpGroupBox2.FlatStyle = FlatStyle.Popup;
      this.mpGroupBox2.Location = new Point(16, 16);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new Size(431, 53);
      this.mpGroupBox2.TabIndex = 10;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "TV-Server";
      // 
      // mpTextBoxHostname
      // 
      this.mpTextBoxHostname.BorderColor = Color.Empty;
      this.mpTextBoxHostname.Location = new Point(126, 22);
      this.mpTextBoxHostname.Name = "mpTextBoxHostname";
      this.mpTextBoxHostname.Size = new Size(229, 20);
      this.mpTextBoxHostname.TabIndex = 6;
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new Point(19, 25);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new Size(58, 13);
      this.mpLabel3.TabIndex = 5;
      this.mpLabel3.Text = "Hostname:";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((AnchorStyles) (((AnchorStyles.Bottom | AnchorStyles.Left)
                                                  | AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.enableAudioDualMonoModes);
      this.mpGroupBox1.Controls.Add(this.mpCheckBoxPrefAudioOverLang);
      this.mpGroupBox1.Controls.Add(this.mpCheckBoxPrefAC3);
      this.mpGroupBox1.FlatStyle = FlatStyle.Popup;
      this.mpGroupBox1.Location = new Point(16, 301);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new Size(432, 75);
      this.mpGroupBox1.TabIndex = 9;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Audio stream settings";
      // 
      // enableAudioDualMonoModes
      // 
      this.enableAudioDualMonoModes.AutoSize = true;
      this.enableAudioDualMonoModes.CheckAlign = ContentAlignment.TopLeft;
      this.enableAudioDualMonoModes.FlatStyle = FlatStyle.Popup;
      this.enableAudioDualMonoModes.Location = new Point(9, 41);
      this.enableAudioDualMonoModes.Name = "enableAudioDualMonoModes";
      this.enableAudioDualMonoModes.Size = new Size(386, 30);
      this.enableAudioDualMonoModes.TabIndex = 12;
      this.enableAudioDualMonoModes.Text =
        "Enable AudioDualMono mode switching\r\n(if 1 audio stream contains 2x mono channels" +
        ", you can switch between them)";
      this.enableAudioDualMonoModes.UseVisualStyleBackColor = true;
      // 
      // mpCheckBoxPrefAudioOverLang
      // 
      this.mpCheckBoxPrefAudioOverLang.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Left)));
      this.mpCheckBoxPrefAudioOverLang.AutoSize = true;
      this.mpCheckBoxPrefAudioOverLang.FlatStyle = FlatStyle.Popup;
      this.mpCheckBoxPrefAudioOverLang.Location = new Point(239, 18);
      this.mpCheckBoxPrefAudioOverLang.Name = "mpCheckBoxPrefAudioOverLang";
      this.mpCheckBoxPrefAudioOverLang.Size = new Size(172, 17);
      this.mpCheckBoxPrefAudioOverLang.TabIndex = 11;
      this.mpCheckBoxPrefAudioOverLang.Text = "Prefer audiotype over language";
      this.mpCheckBoxPrefAudioOverLang.UseVisualStyleBackColor = false;
      // 
      // mpCheckBoxPrefAC3
      // 
      this.mpCheckBoxPrefAC3.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Left)));
      this.mpCheckBoxPrefAC3.AutoSize = true;
      this.mpCheckBoxPrefAC3.FlatStyle = FlatStyle.Popup;
      this.mpCheckBoxPrefAC3.Location = new Point(9, 18);
      this.mpCheckBoxPrefAC3.Name = "mpCheckBoxPrefAC3";
      this.mpCheckBoxPrefAC3.Size = new Size(110, 17);
      this.mpCheckBoxPrefAC3.TabIndex = 7;
      this.mpCheckBoxPrefAC3.Text = "Prefer AC-3 sound";
      this.mpCheckBoxPrefAC3.UseVisualStyleBackColor = false;
      // 
      // tabControlTVGeneral
      // 
      this.tabControlTVGeneral.Controls.Add(this.tabPageGeneralSettings);
      this.tabControlTVGeneral.Controls.Add(this.tabPageAudioLanguages);
      this.tabControlTVGeneral.Controls.Add(this.tabPageSubtitles);
      this.tabControlTVGeneral.Controls.Add(this.tabPage1);
      this.tabControlTVGeneral.Location = new Point(0, 2);
      this.tabControlTVGeneral.Name = "tabControlTVGeneral";
      this.tabControlTVGeneral.SelectedIndex = 0;
      this.tabControlTVGeneral.Size = new Size(472, 408);
      this.tabControlTVGeneral.TabIndex = 11;
      // 
      // tabPageGeneralSettings
      // 
      this.tabPageGeneralSettings.Controls.Add(this.mpGroupBox6);
      this.tabPageGeneralSettings.Controls.Add(this.mpGroupBox5);
      this.tabPageGeneralSettings.Controls.Add(this.mpGroupBox2);
      this.tabPageGeneralSettings.Location = new Point(4, 22);
      this.tabPageGeneralSettings.Name = "tabPageGeneralSettings";
      this.tabPageGeneralSettings.Padding = new Padding(3);
      this.tabPageGeneralSettings.Size = new Size(464, 382);
      this.tabPageGeneralSettings.TabIndex = 0;
      this.tabPageGeneralSettings.Text = "General settings";
      this.tabPageGeneralSettings.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox6
      // 
      this.mpGroupBox6.Controls.Add(this.cbShowChannelStateIcons);
      this.mpGroupBox6.FlatStyle = FlatStyle.Popup;
      this.mpGroupBox6.Location = new Point(16, 161);
      this.mpGroupBox6.Name = "mpGroupBox6";
      this.mpGroupBox6.Size = new Size(431, 53);
      this.mpGroupBox6.TabIndex = 12;
      this.mpGroupBox6.TabStop = false;
      this.mpGroupBox6.Text = "Mini Guide";
      // 
      // cbShowChannelStateIcons
      // 
      this.cbShowChannelStateIcons.AutoSize = true;
      this.cbShowChannelStateIcons.Checked = true;
      this.cbShowChannelStateIcons.CheckState = CheckState.Checked;
      this.cbShowChannelStateIcons.FlatStyle = FlatStyle.Popup;
      this.cbShowChannelStateIcons.Location = new Point(22, 19);
      this.cbShowChannelStateIcons.Name = "cbShowChannelStateIcons";
      this.cbShowChannelStateIcons.Size = new Size(226, 17);
      this.cbShowChannelStateIcons.TabIndex = 0;
      this.cbShowChannelStateIcons.Text = "Show channel state icons (on supp. skins))";
      this.cbShowChannelStateIcons.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox5
      // 
      this.mpGroupBox5.Controls.Add(this.cbHideAllChannels);
      this.mpGroupBox5.FlatStyle = FlatStyle.Popup;
      this.mpGroupBox5.Location = new Point(16, 91);
      this.mpGroupBox5.Name = "mpGroupBox5";
      this.mpGroupBox5.Size = new Size(431, 53);
      this.mpGroupBox5.TabIndex = 11;
      this.mpGroupBox5.TabStop = false;
      this.mpGroupBox5.Text = "TV-Group";
      // 
      // cbHideAllChannels
      // 
      this.cbHideAllChannels.AutoSize = true;
      this.cbHideAllChannels.FlatStyle = FlatStyle.Popup;
      this.cbHideAllChannels.Location = new Point(22, 19);
      this.cbHideAllChannels.Name = "cbHideAllChannels";
      this.cbHideAllChannels.Size = new Size(149, 17);
      this.cbHideAllChannels.TabIndex = 0;
      this.cbHideAllChannels.Text = "Hide \"All Channels\" Group";
      this.cbHideAllChannels.UseVisualStyleBackColor = true;
      // 
      // tabPageAudioLanguages
      // 
      this.tabPageAudioLanguages.Controls.Add(this.groupBox2);
      this.tabPageAudioLanguages.Controls.Add(this.mpGroupBox1);
      this.tabPageAudioLanguages.Location = new Point(4, 22);
      this.tabPageAudioLanguages.Name = "tabPageAudioLanguages";
      this.tabPageAudioLanguages.Padding = new Padding(3);
      this.tabPageAudioLanguages.Size = new Size(464, 382);
      this.tabPageAudioLanguages.TabIndex = 3;
      this.tabPageAudioLanguages.Text = "Audio settings";
      this.tabPageAudioLanguages.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                | AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.mpLabel5);
      this.groupBox2.Controls.Add(this.mpLabel1);
      this.groupBox2.Controls.Add(this.mpButtonDownAudioLang);
      this.groupBox2.Controls.Add(this.mpButtonUpAudioLang);
      this.groupBox2.Controls.Add(this.mpButtonAddAudioLang);
      this.groupBox2.Controls.Add(this.mpButtonRemoveAudioLang);
      this.groupBox2.Controls.Add(this.mpListViewPreferredAudioLang);
      this.groupBox2.Controls.Add(this.mpListViewAvailAudioLang);
      this.groupBox2.FlatStyle = FlatStyle.Popup;
      this.groupBox2.Location = new Point(16, 16);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new Size(432, 284);
      this.groupBox2.TabIndex = 2;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Preferred audio languages";
      // 
      // mpLabel5
      // 
      this.mpLabel5.AutoSize = true;
      this.mpLabel5.Location = new Point(236, 21);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new Size(102, 13);
      this.mpLabel5.TabIndex = 7;
      this.mpLabel5.Text = "Preferred languages";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new Point(6, 21);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new Size(102, 13);
      this.mpLabel1.TabIndex = 6;
      this.mpLabel1.Text = "Available languages";
      // 
      // mpButtonDownAudioLang
      // 
      this.mpButtonDownAudioLang.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.mpButtonDownAudioLang.Location = new Point(289, 255);
      this.mpButtonDownAudioLang.Name = "mpButtonDownAudioLang";
      this.mpButtonDownAudioLang.Size = new Size(46, 20);
      this.mpButtonDownAudioLang.TabIndex = 5;
      this.mpButtonDownAudioLang.Text = "Down";
      this.mpButtonDownAudioLang.UseVisualStyleBackColor = true;
      this.mpButtonDownAudioLang.Click += new EventHandler(this.mpButtonDownAudioLang_Click);
      // 
      // mpButtonUpAudioLang
      // 
      this.mpButtonUpAudioLang.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.mpButtonUpAudioLang.Location = new Point(237, 255);
      this.mpButtonUpAudioLang.Name = "mpButtonUpAudioLang";
      this.mpButtonUpAudioLang.Size = new Size(46, 20);
      this.mpButtonUpAudioLang.TabIndex = 4;
      this.mpButtonUpAudioLang.Text = "Up";
      this.mpButtonUpAudioLang.UseVisualStyleBackColor = true;
      this.mpButtonUpAudioLang.Click += new EventHandler(this.mpButtonUpAudioLang_Click);
      // 
      // mpButtonAddAudioLang
      // 
      this.mpButtonAddAudioLang.Anchor = ((AnchorStyles) ((AnchorStyles.Top | AnchorStyles.Right)));
      this.mpButtonAddAudioLang.Location = new Point(202, 40);
      this.mpButtonAddAudioLang.Name = "mpButtonAddAudioLang";
      this.mpButtonAddAudioLang.Size = new Size(28, 20);
      this.mpButtonAddAudioLang.TabIndex = 3;
      this.mpButtonAddAudioLang.Text = ">";
      this.mpButtonAddAudioLang.UseVisualStyleBackColor = true;
      this.mpButtonAddAudioLang.Click += new EventHandler(this.mpButtonAddAudioLang_Click);
      // 
      // mpButtonRemoveAudioLang
      // 
      this.mpButtonRemoveAudioLang.Anchor = ((AnchorStyles) ((AnchorStyles.Top | AnchorStyles.Right)));
      this.mpButtonRemoveAudioLang.Location = new Point(202, 66);
      this.mpButtonRemoveAudioLang.Name = "mpButtonRemoveAudioLang";
      this.mpButtonRemoveAudioLang.Size = new Size(28, 20);
      this.mpButtonRemoveAudioLang.TabIndex = 2;
      this.mpButtonRemoveAudioLang.Text = "<";
      this.mpButtonRemoveAudioLang.UseVisualStyleBackColor = true;
      this.mpButtonRemoveAudioLang.Click += new EventHandler(this.mpButtonRemoveAudioLang_Click);
      // 
      // mpListViewPreferredAudioLang
      // 
      this.mpListViewPreferredAudioLang.AllowDrop = true;
      this.mpListViewPreferredAudioLang.AllowRowReorder = true;
      this.mpListViewPreferredAudioLang.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Bottom)
                                                                   | AnchorStyles.Right)));
      this.mpListViewPreferredAudioLang.Columns.AddRange(new ColumnHeader[]
                                                           {
                                                             this.columnHeader2,
                                                             this.columnHeader6
                                                           });
      this.mpListViewPreferredAudioLang.FullRowSelect = true;
      this.mpListViewPreferredAudioLang.HeaderStyle = ColumnHeaderStyle.Nonclickable;
      this.mpListViewPreferredAudioLang.HideSelection = false;
      this.mpListViewPreferredAudioLang.Location = new Point(239, 40);
      this.mpListViewPreferredAudioLang.Name = "mpListViewPreferredAudioLang";
      this.mpListViewPreferredAudioLang.Size = new Size(183, 209);
      this.mpListViewPreferredAudioLang.TabIndex = 1;
      this.mpListViewPreferredAudioLang.UseCompatibleStateImageBehavior = false;
      this.mpListViewPreferredAudioLang.View = View.Details;
      this.mpListViewPreferredAudioLang.SelectedIndexChanged += new EventHandler(this.mpListView2_SelectedIndexChanged);
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Language";
      this.columnHeader2.Width = 125;
      // 
      // columnHeader6
      // 
      this.columnHeader6.Text = "ID";
      this.columnHeader6.Width = 35;
      // 
      // mpListViewAvailAudioLang
      // 
      this.mpListViewAvailAudioLang.AllowDrop = true;
      this.mpListViewAvailAudioLang.AllowRowReorder = true;
      this.mpListViewAvailAudioLang.Anchor = ((AnchorStyles) ((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                                | AnchorStyles.Left)
                                                               | AnchorStyles.Right)));
      this.mpListViewAvailAudioLang.Columns.AddRange(new ColumnHeader[]
                                                       {
                                                         this.columnHeader1,
                                                         this.columnHeader5
                                                       });
      this.mpListViewAvailAudioLang.FullRowSelect = true;
      this.mpListViewAvailAudioLang.HeaderStyle = ColumnHeaderStyle.Nonclickable;
      this.mpListViewAvailAudioLang.HideSelection = false;
      this.mpListViewAvailAudioLang.Location = new Point(6, 40);
      this.mpListViewAvailAudioLang.Name = "mpListViewAvailAudioLang";
      this.mpListViewAvailAudioLang.Size = new Size(183, 209);
      this.mpListViewAvailAudioLang.TabIndex = 0;
      this.mpListViewAvailAudioLang.UseCompatibleStateImageBehavior = false;
      this.mpListViewAvailAudioLang.View = View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Language";
      this.columnHeader1.Width = 125;
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "ID";
      this.columnHeader5.Width = 35;
      // 
      // tabPageSubtitles
      // 
      this.tabPageSubtitles.Controls.Add(this.mpGroupBox4);
      this.tabPageSubtitles.Controls.Add(this.mpGroupBox3);
      this.tabPageSubtitles.Location = new Point(4, 22);
      this.tabPageSubtitles.Name = "tabPageSubtitles";
      this.tabPageSubtitles.Padding = new Padding(3);
      this.tabPageSubtitles.Size = new Size(464, 382);
      this.tabPageSubtitles.TabIndex = 2;
      this.tabPageSubtitles.Text = "Subtitle settings";
      this.tabPageSubtitles.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox4
      // 
      this.mpGroupBox4.Controls.Add(this.mpCheckBoxEnableTTXTSub);
      this.mpGroupBox4.Controls.Add(this.mpCheckBoxEnableDVBSub);
      this.mpGroupBox4.FlatStyle = FlatStyle.Popup;
      this.mpGroupBox4.Location = new Point(16, 308);
      this.mpGroupBox4.Name = "mpGroupBox4";
      this.mpGroupBox4.Size = new Size(432, 60);
      this.mpGroupBox4.TabIndex = 10;
      this.mpGroupBox4.TabStop = false;
      this.mpGroupBox4.Text = "Subtitle settings";
      // 
      // mpCheckBoxEnableTTXTSub
      // 
      this.mpCheckBoxEnableTTXTSub.AutoSize = true;
      this.mpCheckBoxEnableTTXTSub.FlatStyle = FlatStyle.Popup;
      this.mpCheckBoxEnableTTXTSub.Location = new Point(239, 28);
      this.mpCheckBoxEnableTTXTSub.Name = "mpCheckBoxEnableTTXTSub";
      this.mpCheckBoxEnableTTXTSub.Size = new Size(135, 17);
      this.mpCheckBoxEnableTTXTSub.TabIndex = 11;
      this.mpCheckBoxEnableTTXTSub.Text = "Enable teletext subtitles";
      this.mpCheckBoxEnableTTXTSub.UseVisualStyleBackColor = false;
      this.mpCheckBoxEnableTTXTSub.CheckedChanged += new EventHandler(this.mpCheckBox1_CheckedChanged);
      // 
      // mpCheckBoxEnableDVBSub
      // 
      this.mpCheckBoxEnableDVBSub.AutoSize = true;
      this.mpCheckBoxEnableDVBSub.FlatStyle = FlatStyle.Popup;
      this.mpCheckBoxEnableDVBSub.Location = new Point(9, 28);
      this.mpCheckBoxEnableDVBSub.Name = "mpCheckBoxEnableDVBSub";
      this.mpCheckBoxEnableDVBSub.Size = new Size(123, 17);
      this.mpCheckBoxEnableDVBSub.TabIndex = 7;
      this.mpCheckBoxEnableDVBSub.Text = "Enable DVB subtitles";
      this.mpCheckBoxEnableDVBSub.UseVisualStyleBackColor = false;
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                  | AnchorStyles.Right)));
      this.mpGroupBox3.Controls.Add(this.mpLabel6);
      this.mpGroupBox3.Controls.Add(this.mpLabel7);
      this.mpGroupBox3.Controls.Add(this.mpButtonDownSubLang);
      this.mpGroupBox3.Controls.Add(this.mpButtonUpSubLang);
      this.mpGroupBox3.Controls.Add(this.mpButtonAddSubLang);
      this.mpGroupBox3.Controls.Add(this.mpButtonRemoveSubLang);
      this.mpGroupBox3.Controls.Add(this.mpListViewPreferredSubLang);
      this.mpGroupBox3.Controls.Add(this.mpListViewAvailSubLang);
      this.mpGroupBox3.FlatStyle = FlatStyle.Popup;
      this.mpGroupBox3.Location = new Point(16, 16);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new Size(432, 286);
      this.mpGroupBox3.TabIndex = 3;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Preferred subtitle languages";
      // 
      // mpLabel6
      // 
      this.mpLabel6.AutoSize = true;
      this.mpLabel6.Location = new Point(236, 21);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new Size(102, 13);
      this.mpLabel6.TabIndex = 7;
      this.mpLabel6.Text = "Preferred languages";
      // 
      // mpLabel7
      // 
      this.mpLabel7.AutoSize = true;
      this.mpLabel7.Location = new Point(6, 21);
      this.mpLabel7.Name = "mpLabel7";
      this.mpLabel7.Size = new Size(102, 13);
      this.mpLabel7.TabIndex = 6;
      this.mpLabel7.Text = "Available languages";
      // 
      // mpButtonDownSubLang
      // 
      this.mpButtonDownSubLang.Location = new Point(289, 257);
      this.mpButtonDownSubLang.Name = "mpButtonDownSubLang";
      this.mpButtonDownSubLang.Size = new Size(46, 20);
      this.mpButtonDownSubLang.TabIndex = 5;
      this.mpButtonDownSubLang.Text = "Down";
      this.mpButtonDownSubLang.UseVisualStyleBackColor = true;
      this.mpButtonDownSubLang.Click += new EventHandler(this.mpButtonDownSubLang_Click);
      // 
      // mpButtonUpSubLang
      // 
      this.mpButtonUpSubLang.Location = new Point(237, 257);
      this.mpButtonUpSubLang.Name = "mpButtonUpSubLang";
      this.mpButtonUpSubLang.Size = new Size(46, 20);
      this.mpButtonUpSubLang.TabIndex = 4;
      this.mpButtonUpSubLang.Text = "Up";
      this.mpButtonUpSubLang.UseVisualStyleBackColor = true;
      this.mpButtonUpSubLang.Click += new EventHandler(this.mpButtonUpSubLang_Click);
      // 
      // mpButtonAddSubLang
      // 
      this.mpButtonAddSubLang.Location = new Point(202, 40);
      this.mpButtonAddSubLang.Name = "mpButtonAddSubLang";
      this.mpButtonAddSubLang.Size = new Size(28, 20);
      this.mpButtonAddSubLang.TabIndex = 3;
      this.mpButtonAddSubLang.Text = ">";
      this.mpButtonAddSubLang.UseVisualStyleBackColor = true;
      this.mpButtonAddSubLang.Click += new EventHandler(this.mpButtonAddSubLang_Click);
      // 
      // mpButtonRemoveSubLang
      // 
      this.mpButtonRemoveSubLang.Location = new Point(202, 66);
      this.mpButtonRemoveSubLang.Name = "mpButtonRemoveSubLang";
      this.mpButtonRemoveSubLang.Size = new Size(28, 20);
      this.mpButtonRemoveSubLang.TabIndex = 2;
      this.mpButtonRemoveSubLang.Text = "<";
      this.mpButtonRemoveSubLang.UseVisualStyleBackColor = true;
      this.mpButtonRemoveSubLang.Click += new EventHandler(this.mpButtonRemoveSubLang_Click);
      // 
      // mpListViewPreferredSubLang
      // 
      this.mpListViewPreferredSubLang.AllowDrop = true;
      this.mpListViewPreferredSubLang.AllowRowReorder = true;
      this.mpListViewPreferredSubLang.Columns.AddRange(new ColumnHeader[]
                                                         {
                                                           this.columnHeader4,
                                                           this.columnHeader8
                                                         });
      this.mpListViewPreferredSubLang.FullRowSelect = true;
      this.mpListViewPreferredSubLang.HeaderStyle = ColumnHeaderStyle.Nonclickable;
      this.mpListViewPreferredSubLang.HideSelection = false;
      this.mpListViewPreferredSubLang.Location = new Point(239, 40);
      this.mpListViewPreferredSubLang.Name = "mpListViewPreferredSubLang";
      this.mpListViewPreferredSubLang.Size = new Size(183, 211);
      this.mpListViewPreferredSubLang.TabIndex = 1;
      this.mpListViewPreferredSubLang.UseCompatibleStateImageBehavior = false;
      this.mpListViewPreferredSubLang.View = View.Details;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Language";
      this.columnHeader4.Width = 125;
      // 
      // columnHeader8
      // 
      this.columnHeader8.Text = "ID";
      this.columnHeader8.Width = 35;
      // 
      // mpListViewAvailSubLang
      // 
      this.mpListViewAvailSubLang.AllowDrop = true;
      this.mpListViewAvailSubLang.AllowRowReorder = true;
      this.mpListViewAvailSubLang.Columns.AddRange(new ColumnHeader[]
                                                     {
                                                       this.columnHeader3,
                                                       this.columnHeader7
                                                     });
      this.mpListViewAvailSubLang.FullRowSelect = true;
      this.mpListViewAvailSubLang.HeaderStyle = ColumnHeaderStyle.Nonclickable;
      this.mpListViewAvailSubLang.HideSelection = false;
      this.mpListViewAvailSubLang.Location = new Point(6, 40);
      this.mpListViewAvailSubLang.Name = "mpListViewAvailSubLang";
      this.mpListViewAvailSubLang.Size = new Size(183, 211);
      this.mpListViewAvailSubLang.TabIndex = 0;
      this.mpListViewAvailSubLang.UseCompatibleStateImageBehavior = false;
      this.mpListViewAvailSubLang.View = View.Details;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Language";
      this.columnHeader3.Width = 125;
      // 
      // columnHeader7
      // 
      this.columnHeader7.Text = "ID";
      this.columnHeader7.Width = 35;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpGroupBox8);
      this.tabPage1.Controls.Add(this.mpGroupBox7);
      this.tabPage1.Location = new Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new Padding(3);
      this.tabPage1.Size = new Size(464, 382);
      this.tabPage1.TabIndex = 4;
      this.tabPage1.Text = "Notifier";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox8
      // 
      this.mpGroupBox8.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                  | AnchorStyles.Right)));
      this.mpGroupBox8.Controls.Add(this.chkRecnotifications);
      this.mpGroupBox8.FlatStyle = FlatStyle.Popup;
      this.mpGroupBox8.Location = new Point(16, 166);
      this.mpGroupBox8.Name = "mpGroupBox8";
      this.mpGroupBox8.Size = new Size(431, 82);
      this.mpGroupBox8.TabIndex = 13;
      this.mpGroupBox8.TabStop = false;
      this.mpGroupBox8.Text = "Recording notifications";
      // 
      // chkRecnotifications
      // 
      this.chkRecnotifications.AutoSize = true;
      this.chkRecnotifications.FlatStyle = FlatStyle.Popup;
      this.chkRecnotifications.Location = new Point(22, 19);
      this.chkRecnotifications.Name = "chkRecnotifications";
      this.chkRecnotifications.Size = new Size(327, 17);
      this.chkRecnotifications.TabIndex = 0;
      this.chkRecnotifications.Text = "Enabled (shows a notification when a recording starts and stops)";
      this.chkRecnotifications.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox7
      // 
      this.mpGroupBox7.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                  | AnchorStyles.Right)));
      this.mpGroupBox7.Controls.Add(this.txtNotifyAfter);
      this.mpGroupBox7.Controls.Add(this.labelNotifyTimeout);
      this.mpGroupBox7.Controls.Add(this.checkBoxNotifyPlaySound);
      this.mpGroupBox7.Controls.Add(this.mpLabel2);
      this.mpGroupBox7.Controls.Add(this.txtNotifyBefore);
      this.mpGroupBox7.Controls.Add(this.chkTVnotifications);
      this.mpGroupBox7.FlatStyle = FlatStyle.Popup;
      this.mpGroupBox7.Location = new Point(16, 16);
      this.mpGroupBox7.Name = "mpGroupBox7";
      this.mpGroupBox7.Size = new Size(431, 135);
      this.mpGroupBox7.TabIndex = 12;
      this.mpGroupBox7.TabStop = false;
      this.mpGroupBox7.Text = "TV notifications";
      // 
      // txtNotifyAfter
      // 
      this.txtNotifyAfter.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                     | AnchorStyles.Right)));
      this.txtNotifyAfter.BorderColor = Color.Empty;
      this.txtNotifyAfter.Location = new Point(164, 73);
      this.txtNotifyAfter.Name = "txtNotifyAfter";
      this.txtNotifyAfter.Size = new Size(229, 20);
      this.txtNotifyAfter.TabIndex = 11;
      this.txtNotifyAfter.Text = "15";
      // 
      // labelNotifyTimeout
      // 
      this.labelNotifyTimeout.AutoSize = true;
      this.labelNotifyTimeout.Location = new Point(19, 76);
      this.labelNotifyTimeout.Name = "labelNotifyTimeout";
      this.labelNotifyTimeout.Size = new Size(139, 13);
      this.labelNotifyTimeout.TabIndex = 10;
      this.labelNotifyTimeout.Text = "Hide notification after (sec.):";
      // 
      // checkBoxNotifyPlaySound
      // 
      this.checkBoxNotifyPlaySound.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                              | AnchorStyles.Right)));
      this.checkBoxNotifyPlaySound.AutoSize = true;
      this.checkBoxNotifyPlaySound.Checked = true;
      this.checkBoxNotifyPlaySound.CheckState = CheckState.Checked;
      this.checkBoxNotifyPlaySound.FlatStyle = FlatStyle.Popup;
      this.checkBoxNotifyPlaySound.Location = new Point(22, 99);
      this.checkBoxNotifyPlaySound.Name = "checkBoxNotifyPlaySound";
      this.checkBoxNotifyPlaySound.Size = new Size(105, 17);
      this.checkBoxNotifyPlaySound.TabIndex = 9;
      this.checkBoxNotifyPlaySound.Text = "Play \"notify.wav\"";
      this.checkBoxNotifyPlaySound.UseVisualStyleBackColor = true;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new Point(19, 50);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new Size(96, 13);
      this.mpLabel2.TabIndex = 8;
      this.mpLabel2.Text = "Notify before (sec):";
      // 
      // txtNotifyBefore
      // 
      this.txtNotifyBefore.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                      | AnchorStyles.Right)));
      this.txtNotifyBefore.BorderColor = Color.Empty;
      this.txtNotifyBefore.Location = new Point(164, 47);
      this.txtNotifyBefore.Name = "txtNotifyBefore";
      this.txtNotifyBefore.Size = new Size(229, 20);
      this.txtNotifyBefore.TabIndex = 7;
      this.txtNotifyBefore.Text = "300";
      // 
      // chkTVnotifications
      // 
      this.chkTVnotifications.AutoSize = true;
      this.chkTVnotifications.FlatStyle = FlatStyle.Popup;
      this.chkTVnotifications.Location = new Point(22, 19);
      this.chkTVnotifications.Name = "chkTVnotifications";
      this.chkTVnotifications.Size = new Size(336, 17);
      this.chkTVnotifications.TabIndex = 0;
      this.chkTVnotifications.Text = "Enabled (shows a notification when a TV program is about to start)";
      this.chkTVnotifications.UseVisualStyleBackColor = true;
      this.chkTVnotifications.CheckedChanged += new EventHandler(this.chkTVnotifications_CheckedChanged);
      // 
      // TVClient
      // 
      this.Controls.Add(this.tabControlTVGeneral);
      this.Name = "TVClient";
      this.Size = new Size(510, 435);
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.tabControlTVGeneral.ResumeLayout(false);
      this.tabPageGeneralSettings.ResumeLayout(false);
      this.mpGroupBox6.ResumeLayout(false);
      this.mpGroupBox6.PerformLayout();
      this.mpGroupBox5.ResumeLayout(false);
      this.mpGroupBox5.PerformLayout();
      this.tabPageAudioLanguages.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.tabPageSubtitles.ResumeLayout(false);
      this.mpGroupBox4.ResumeLayout(false);
      this.mpGroupBox4.PerformLayout();
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.tabPage1.ResumeLayout(false);
      this.mpGroupBox8.ResumeLayout(false);
      this.mpGroupBox8.PerformLayout();
      this.mpGroupBox7.ResumeLayout(false);
      this.mpGroupBox7.PerformLayout();
      this.ResumeLayout(false);
    }

    private void mpListView2_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    private void mpCheckBox1_CheckedChanged(object sender, EventArgs e)
    {
    }

    private void mpButtonAddAudioLang_Click(object sender, EventArgs e)
    {
      if (mpListViewAvailAudioLang.SelectedItems.Count == 0)
      {
        return;
      }

      foreach (ListViewItem lang in mpListViewAvailAudioLang.SelectedItems)
      {
        mpListViewAvailAudioLang.Items.Remove(lang);
        mpListViewPreferredAudioLang.Items.Add(lang);
      }
    }

    private void mpButtonRemoveAudioLang_Click(object sender, EventArgs e)
    {
      if (mpListViewPreferredAudioLang.SelectedItems.Count == 0)
      {
        return;
      }

      foreach (ListViewItem lang in mpListViewPreferredAudioLang.SelectedItems)
      {
        mpListViewPreferredAudioLang.Items.Remove(lang);
        mpListViewAvailAudioLang.Items.Add(lang);
      }
    }

    private void mpButtonUpAudioLang_Click(object sender, EventArgs e)
    {
      moveItemUp(mpListViewPreferredAudioLang);
    }

    private void mpButtonDownAudioLang_Click(object sender, EventArgs e)
    {
      moveItemDown(mpListViewPreferredAudioLang);
    }

    private void mpButtonAddSubLang_Click(object sender, EventArgs e)
    {
      if (mpListViewAvailSubLang.SelectedItems.Count == 0)
      {
        return;
      }

      foreach (ListViewItem lang in mpListViewAvailSubLang.SelectedItems)
      {
        mpListViewAvailSubLang.Items.Remove(lang);
        mpListViewPreferredSubLang.Items.Add(lang);
      }
    }

    private void mpButtonRemoveSubLang_Click(object sender, EventArgs e)
    {
      if (mpListViewPreferredSubLang.SelectedItems.Count == 0)
      {
        return;
      }

      foreach (ListViewItem lang in mpListViewPreferredSubLang.SelectedItems)
      {
        mpListViewPreferredSubLang.Items.Remove(lang);
        mpListViewAvailSubLang.Items.Add(lang);
      }
    }

    private void mpButtonUpSubLang_Click(object sender, EventArgs e)
    {
      moveItemUp(mpListViewPreferredSubLang);
    }

    private void mpButtonDownSubLang_Click(object sender, EventArgs e)
    {
      moveItemDown(mpListViewPreferredSubLang);
    }

    private void moveItemDown(MPListView mplistView)
    {
      ListView.SelectedIndexCollection indexes = mplistView.SelectedIndices;
      if (indexes.Count == 0)
      {
        return;
      }
      if (mplistView.Items.Count < 2)
      {
        return;
      }
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        ListViewItem item = mplistView.Items[index];
        mplistView.Items.RemoveAt(index);
        if (index + 1 < mplistView.Items.Count)
        {
          mplistView.Items.Insert(index + 1, item);
        }
        else
        {
          mplistView.Items.Add(item);
        }
      }
    }

    private void moveItemUp(MPListView mplistView)
    {
      ListView.SelectedIndexCollection indexes = mplistView.SelectedIndices;
      if (indexes.Count == 0)
      {
        return;
      }
      for (int i = 0; i < indexes.Count; ++i)
      {
        int index = indexes[i];
        if (index > 0)
        {
          ListViewItem item = mplistView.Items[index];
          mplistView.Items.RemoveAt(index);
          mplistView.Items.Insert(index - 1, item);
        }
      }
    }

    private void chkTVnotifications_CheckedChanged(object sender, EventArgs e)
    {
      if (chkTVnotifications.Checked)
      {
        txtNotifyAfter.Enabled = true;
        txtNotifyBefore.Enabled = true;
        checkBoxNotifyPlaySound.Enabled = true;
        labelNotifyTimeout.Enabled = true;
        mpLabel2.Enabled = true;
      }
      else
      {
        txtNotifyAfter.Enabled = false;
        txtNotifyBefore.Enabled = false;
        checkBoxNotifyPlaySound.Enabled = false;
        labelNotifyTimeout.Enabled = false;
        mpLabel2.Enabled = false;
      }
    }
  }
}