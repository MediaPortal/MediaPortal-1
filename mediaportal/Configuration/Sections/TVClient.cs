using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration.Sections
{
  public class TVClient : MediaPortal.Configuration.SectionSettings
  {
    #region variables
    private string _preferredAudioLanguages;
		private string _preferredSubLanguages;
		private IList<string> _languageCodes;

    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBoxHostname;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxPrefAC3;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
		private IList<string> _languagesAvail;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxPrefAudioOverLang;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
		private MediaPortal.UserInterface.Controls.MPTabControl tabControlTVGeneral;
		private MediaPortal.UserInterface.Controls.MPTabPage tabPageImportSettings;
		private MediaPortal.UserInterface.Controls.MPTabPage tabPageFilenames;
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
		private MediaPortal.UserInterface.Controls.MPTabPage tabPageEncoderSettings;
		private MediaPortal.UserInterface.Controls.MPButton mpButtonAddAudioLang;
		private MediaPortal.UserInterface.Controls.MPButton mpButtonRemoveAudioLang;
		private MediaPortal.UserInterface.Controls.MPListView mpListViewPreferredAudioLang;
		private MediaPortal.UserInterface.Controls.MPListView mpListViewAvailAudioLang;
		private MediaPortal.UserInterface.Controls.MPButton mpButtonDownAudioLang;
		private MediaPortal.UserInterface.Controls.MPButton mpButtonUpAudioLang;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabel5;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox3;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabel6;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabel7;
		private MediaPortal.UserInterface.Controls.MPButton mpButtonDownSubLang;
		private MediaPortal.UserInterface.Controls.MPButton mpButtonUpSubLang;
		private MediaPortal.UserInterface.Controls.MPButton mpButtonAddSubLang;
		private MediaPortal.UserInterface.Controls.MPButton mpButtonRemoveSubLang;
		private MediaPortal.UserInterface.Controls.MPListView mpListViewPreferredSubLang;
		private MediaPortal.UserInterface.Controls.MPListView mpListViewAvailSubLang;
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox4;
		private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxEnableTTXTSub;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabel8;
		private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxEnableDVBSub;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabel9;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
		private ColumnHeader columnHeader4;
		private ColumnHeader columnHeader3;    
		
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


    private void TVClient_Load(object sender, EventArgs e)
    {
      LoadSettings();
      //mpTextBoxHostname.Text = _serverHostName;
      //mpCheckBoxPrefAC3.Checked = _preferAC3;
      //mpCheckBoxavoidSeekingonChannelChange.Checked = _avoidSeeking;
      
      
      //mpListViewLanguages.Items.Clear();
      /*for (int i = 0; i < languages.Count; i++)
      {
        ListViewItem item = new ListViewItem();
        item.Text = languages[i];
        item.Tag = languageCodes[i];
        item.Checked = preferredLanguages.Contains(languageCodes[i]);
        mpListViewLanguages.Items.Add(item);
      }*/
    }

    bool MyInterfaceFilter(Type typeObj, Object criteriaObj)
    {
      return (typeObj.ToString().Equals(criteriaObj.ToString()));
    }

    public override void LoadSettings()
    {
      //Load parameters from XML File
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        mpTextBoxHostname.Text = xmlreader.GetValueAsString("tvservice", "hostname", "");
        mpCheckBoxPrefAC3.Checked = xmlreader.GetValueAsBool("tvservice", "preferac3", false);
        mpCheckBoxPrefAudioOverLang.Checked = xmlreader.GetValueAsBool("tvservice", "preferAudioTypeOverLang", true);
				_preferredAudioLanguages = xmlreader.GetValueAsString("tvservice", "preferredaudiolanguages", "");
				_preferredSubLanguages = xmlreader.GetValueAsString("tvservice", "preferredsublanguages", "");

				mpCheckBoxEnableDVBSub.Checked = xmlreader.GetValueAsBool("tvservice", "dvbbitmapsubtitles", false);
				mpCheckBoxEnableTTXTSub.Checked = xmlreader.GetValueAsBool("tvservice", "dvbttxtsubtitles", false);
      }

      if (System.IO.File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"))
      {
        // Enable this Panel if the TvPlugin exists in the plug-in Directory
        this.Enabled = true;

        // Load the TVLibraryInterfaces so we can lookup available languages. 
        Type[] foundInterfaces = null;

        try
        {
          Assembly assem = Assembly.LoadFrom(Config.GetFolder(Config.Dir.Base) + "\\TvLibrary.Interfaces.dll");
          if (assem != null)
          {
            Type[] types = assem.GetExportedTypes();
            foreach (Type t in types)
            {
              try
              {
                if (t.Name == "Languages")
                {
                  // Load available languages into variables. 
                  Object newObj = null;
                  newObj = Activator.CreateInstance(t);
                  MethodInfo inf = t.GetMethod("GetLanguages", BindingFlags.Public | BindingFlags.Instance);
                  _languagesAvail = inf.Invoke(newObj, null) as List<String>;
                  inf = t.GetMethod("GetLanguageCodes", BindingFlags.Public | BindingFlags.Instance);
									_languageCodes = (List<String>)inf.Invoke(newObj, null);									

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
												ListViewItem item = new ListViewItem();
												item.Text = _languagesAvail[i];
												item.Tag = _languageCodes[i];                      
												mpListViewAvailAudioLang.Items.Add(item);
											}
                    }

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
															ListViewItem item = new ListViewItem();
															item.Text = _languagesAvail[j];
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
												ListViewItem item = new ListViewItem();
												item.Text = _languagesAvail[i];
												item.Tag = _languageCodes[i];
												mpListViewAvailSubLang.Items.Add(item);
											}											
										}

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
															ListViewItem item = new ListViewItem();
															item.Text = _languagesAvail[j];
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
              catch (System.Reflection.TargetInvocationException ex)
              {
                 Log.Debug("Failed to load languages {0}", ex.ToString());
                 continue;
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
      else this.Enabled = false;
    }
    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string prefLangs = "";
        xmlreader.SetValue("tvservice", "hostname", mpTextBoxHostname.Text);
        xmlreader.SetValueAsBool("tvservice", "preferac3", mpCheckBoxPrefAC3.Checked);
        xmlreader.SetValueAsBool("tvservice", "preferAudioTypeOverLang", mpCheckBoxPrefAudioOverLang.Checked);

				xmlreader.SetValueAsBool("tvservice", "dvbbitmapsubtitles", mpCheckBoxEnableDVBSub.Checked);
				xmlreader.SetValueAsBool("tvservice", "dvbttxtsubtitles", mpCheckBoxEnableTTXTSub.Checked);
				        
				
        foreach (ListViewItem item in mpListViewPreferredAudioLang.Items)
        {        
					prefLangs += (string)item.Tag + ";";
        }
				xmlreader.SetValue("tvservice", "preferredaudiolanguages", prefLangs);

				prefLangs = "";
				foreach (ListViewItem item in mpListViewPreferredSubLang.Items)
				{					
					prefLangs += (string)item.Tag + ";";
				}
				xmlreader.SetValue("tvservice", "preferredsublanguages", prefLangs);
				
      }
    }

    private void InitializeComponent()
    {
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpTextBoxHostname = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpCheckBoxPrefAudioOverLang = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpCheckBoxPrefAC3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControlTVGeneral = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageImportSettings = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.tabPageFilenames = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonDownAudioLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonUpAudioLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonAddAudioLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonRemoveAudioLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpListViewPreferredAudioLang = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.mpListViewAvailAudioLang = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.tabPageEncoderSettings = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.mpGroupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpCheckBoxEnableTTXTSub = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpCheckBoxEnableDVBSub = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonDownSubLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonUpSubLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonAddSubLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonRemoveSubLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpListViewPreferredSubLang = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.mpListViewAvailSubLang = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.mpGroupBox2.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.tabControlTVGeneral.SuspendLayout();
      this.tabPageImportSettings.SuspendLayout();
      this.tabPageFilenames.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.tabPageEncoderSettings.SuspendLayout();
      this.mpGroupBox4.SuspendLayout();
      this.mpGroupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Controls.Add(this.mpTextBoxHostname);
      this.mpGroupBox2.Controls.Add(this.mpLabel3);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(16, 16);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(432, 53);
      this.mpGroupBox2.TabIndex = 10;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "TvServer";
      // 
      // mpTextBoxHostname
      // 
      this.mpTextBoxHostname.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBoxHostname.Location = new System.Drawing.Point(126, 22);
      this.mpTextBoxHostname.Name = "mpTextBoxHostname";
      this.mpTextBoxHostname.Size = new System.Drawing.Size(229, 20);
      this.mpTextBoxHostname.TabIndex = 6;
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(19, 25);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(58, 13);
      this.mpLabel3.TabIndex = 5;
      this.mpLabel3.Text = "Hostname:";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.mpCheckBoxPrefAudioOverLang);
      this.mpGroupBox1.Controls.Add(this.mpLabel4);
      this.mpGroupBox1.Controls.Add(this.mpCheckBoxPrefAC3);
      this.mpGroupBox1.Controls.Add(this.mpLabel2);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(16, 270);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(432, 98);
      this.mpGroupBox1.TabIndex = 9;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Audio Stream Settings";
      // 
      // mpCheckBoxPrefAudioOverLang
      // 
      this.mpCheckBoxPrefAudioOverLang.AutoSize = true;
      this.mpCheckBoxPrefAudioOverLang.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.mpCheckBoxPrefAudioOverLang.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxPrefAudioOverLang.Location = new System.Drawing.Point(321, 27);
      this.mpCheckBoxPrefAudioOverLang.Name = "mpCheckBoxPrefAudioOverLang";
      this.mpCheckBoxPrefAudioOverLang.Size = new System.Drawing.Size(13, 12);
      this.mpCheckBoxPrefAudioOverLang.TabIndex = 11;
      this.mpCheckBoxPrefAudioOverLang.UseVisualStyleBackColor = false;
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(157, 26);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(165, 13);
      this.mpLabel4.TabIndex = 10;
      this.mpLabel4.Text = "Prefer Audiotype Over Language:";
      // 
      // mpCheckBoxPrefAC3
      // 
      this.mpCheckBoxPrefAC3.AutoSize = true;
      this.mpCheckBoxPrefAC3.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.mpCheckBoxPrefAC3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxPrefAC3.Location = new System.Drawing.Point(113, 27);
      this.mpCheckBoxPrefAC3.Name = "mpCheckBoxPrefAC3";
      this.mpCheckBoxPrefAC3.Size = new System.Drawing.Size(13, 12);
      this.mpCheckBoxPrefAC3.TabIndex = 7;
      this.mpCheckBoxPrefAC3.UseVisualStyleBackColor = false;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(6, 26);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(61, 13);
      this.mpLabel2.TabIndex = 6;
      this.mpLabel2.Text = "Prefer AC3:";
      // 
      // tabControlTVGeneral
      // 
      this.tabControlTVGeneral.Controls.Add(this.tabPageImportSettings);
      this.tabControlTVGeneral.Controls.Add(this.tabPageFilenames);
      this.tabControlTVGeneral.Controls.Add(this.tabPageEncoderSettings);
      this.tabControlTVGeneral.Location = new System.Drawing.Point(0, 2);
      this.tabControlTVGeneral.Name = "tabControlTVGeneral";
      this.tabControlTVGeneral.SelectedIndex = 0;
      this.tabControlTVGeneral.Size = new System.Drawing.Size(472, 400);
      this.tabControlTVGeneral.TabIndex = 11;
      // 
      // tabPageImportSettings
      // 
      this.tabPageImportSettings.Controls.Add(this.mpGroupBox2);
      this.tabPageImportSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageImportSettings.Name = "tabPageImportSettings";
      this.tabPageImportSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageImportSettings.Size = new System.Drawing.Size(464, 374);
      this.tabPageImportSettings.TabIndex = 0;
      this.tabPageImportSettings.Text = "General Settings";
      this.tabPageImportSettings.UseVisualStyleBackColor = true;
      // 
      // tabPageFilenames
      // 
      this.tabPageFilenames.Controls.Add(this.groupBox2);
      this.tabPageFilenames.Controls.Add(this.mpGroupBox1);
      this.tabPageFilenames.Location = new System.Drawing.Point(4, 22);
      this.tabPageFilenames.Name = "tabPageFilenames";
      this.tabPageFilenames.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageFilenames.Size = new System.Drawing.Size(464, 374);
      this.tabPageFilenames.TabIndex = 3;
      this.tabPageFilenames.Text = "Audio Settings";
      this.tabPageFilenames.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.mpLabel5);
      this.groupBox2.Controls.Add(this.mpLabel1);
      this.groupBox2.Controls.Add(this.mpButtonDownAudioLang);
      this.groupBox2.Controls.Add(this.mpButtonUpAudioLang);
      this.groupBox2.Controls.Add(this.mpButtonAddAudioLang);
      this.groupBox2.Controls.Add(this.mpButtonRemoveAudioLang);
      this.groupBox2.Controls.Add(this.mpListViewPreferredAudioLang);
      this.groupBox2.Controls.Add(this.mpListViewAvailAudioLang);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(16, 16);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(432, 248);
      this.groupBox2.TabIndex = 2;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Preferred Audio Languages";
      // 
      // mpLabel5
      // 
      this.mpLabel5.AutoSize = true;
      this.mpLabel5.Location = new System.Drawing.Point(236, 21);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(106, 13);
      this.mpLabel5.TabIndex = 7;
      this.mpLabel5.Text = "Preferred Languages";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(6, 21);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(106, 13);
      this.mpLabel1.TabIndex = 6;
      this.mpLabel1.Text = "Available Languages";
      // 
      // mpButtonDownAudioLang
      // 
      this.mpButtonDownAudioLang.Location = new System.Drawing.Point(291, 215);
      this.mpButtonDownAudioLang.Name = "mpButtonDownAudioLang";
      this.mpButtonDownAudioLang.Size = new System.Drawing.Size(46, 23);
      this.mpButtonDownAudioLang.TabIndex = 5;
      this.mpButtonDownAudioLang.Text = "Down";
      this.mpButtonDownAudioLang.UseVisualStyleBackColor = true;
      this.mpButtonDownAudioLang.Click += new System.EventHandler(this.mpButtonDownAudioLang_Click);
      // 
      // mpButtonUpAudioLang
      // 
      this.mpButtonUpAudioLang.Location = new System.Drawing.Point(239, 215);
      this.mpButtonUpAudioLang.Name = "mpButtonUpAudioLang";
      this.mpButtonUpAudioLang.Size = new System.Drawing.Size(46, 23);
      this.mpButtonUpAudioLang.TabIndex = 4;
      this.mpButtonUpAudioLang.Text = "Up";
      this.mpButtonUpAudioLang.UseVisualStyleBackColor = true;
      this.mpButtonUpAudioLang.Click += new System.EventHandler(this.mpButtonUpAudioLang_Click);
      // 
      // mpButtonAddAudioLang
      // 
      this.mpButtonAddAudioLang.Location = new System.Drawing.Point(200, 40);
      this.mpButtonAddAudioLang.Name = "mpButtonAddAudioLang";
      this.mpButtonAddAudioLang.Size = new System.Drawing.Size(28, 23);
      this.mpButtonAddAudioLang.TabIndex = 3;
      this.mpButtonAddAudioLang.Text = ">>";
      this.mpButtonAddAudioLang.UseVisualStyleBackColor = true;
      this.mpButtonAddAudioLang.Click += new System.EventHandler(this.mpButtonAddAudioLang_Click);
      // 
      // mpButtonRemoveAudioLang
      // 
      this.mpButtonRemoveAudioLang.Location = new System.Drawing.Point(200, 69);
      this.mpButtonRemoveAudioLang.Name = "mpButtonRemoveAudioLang";
      this.mpButtonRemoveAudioLang.Size = new System.Drawing.Size(28, 23);
      this.mpButtonRemoveAudioLang.TabIndex = 2;
      this.mpButtonRemoveAudioLang.Text = "<<";
      this.mpButtonRemoveAudioLang.UseVisualStyleBackColor = true;
      this.mpButtonRemoveAudioLang.Click += new System.EventHandler(this.mpButtonRemoveAudioLang_Click);
      // 
      // mpListViewPreferredAudioLang
      // 
      this.mpListViewPreferredAudioLang.AllowDrop = true;
      this.mpListViewPreferredAudioLang.AllowRowReorder = true;
      this.mpListViewPreferredAudioLang.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
      this.mpListViewPreferredAudioLang.FullRowSelect = true;
      this.mpListViewPreferredAudioLang.HideSelection = false;
      this.mpListViewPreferredAudioLang.Location = new System.Drawing.Point(239, 40);
      this.mpListViewPreferredAudioLang.Name = "mpListViewPreferredAudioLang";
      this.mpListViewPreferredAudioLang.Size = new System.Drawing.Size(183, 169);
      this.mpListViewPreferredAudioLang.TabIndex = 1;
      this.mpListViewPreferredAudioLang.UseCompatibleStateImageBehavior = false;
      this.mpListViewPreferredAudioLang.View = System.Windows.Forms.View.Details;
      this.mpListViewPreferredAudioLang.SelectedIndexChanged += new System.EventHandler(this.mpListView2_SelectedIndexChanged);
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Language";
      this.columnHeader2.Width = 150;
      // 
      // mpListViewAvailAudioLang
      // 
      this.mpListViewAvailAudioLang.AllowDrop = true;
      this.mpListViewAvailAudioLang.AllowRowReorder = false;
      this.mpListViewAvailAudioLang.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
      this.mpListViewAvailAudioLang.FullRowSelect = true;
      this.mpListViewAvailAudioLang.HideSelection = false;
      this.mpListViewAvailAudioLang.Location = new System.Drawing.Point(9, 40);
      this.mpListViewAvailAudioLang.Name = "mpListViewAvailAudioLang";
      this.mpListViewAvailAudioLang.Size = new System.Drawing.Size(183, 169);
      this.mpListViewAvailAudioLang.TabIndex = 0;
      this.mpListViewAvailAudioLang.UseCompatibleStateImageBehavior = false;
      this.mpListViewAvailAudioLang.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Language";
      this.columnHeader1.Width = 150;
      // 
      // tabPageEncoderSettings
      // 
      this.tabPageEncoderSettings.Controls.Add(this.mpGroupBox4);
      this.tabPageEncoderSettings.Controls.Add(this.mpGroupBox3);
      this.tabPageEncoderSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageEncoderSettings.Name = "tabPageEncoderSettings";
      this.tabPageEncoderSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageEncoderSettings.Size = new System.Drawing.Size(464, 374);
      this.tabPageEncoderSettings.TabIndex = 2;
      this.tabPageEncoderSettings.Text = "Subtitle Settings";
      this.tabPageEncoderSettings.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox4
      // 
      this.mpGroupBox4.Controls.Add(this.mpCheckBoxEnableTTXTSub);
      this.mpGroupBox4.Controls.Add(this.mpLabel8);
      this.mpGroupBox4.Controls.Add(this.mpCheckBoxEnableDVBSub);
      this.mpGroupBox4.Controls.Add(this.mpLabel9);
      this.mpGroupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox4.Location = new System.Drawing.Point(16, 270);
      this.mpGroupBox4.Name = "mpGroupBox4";
      this.mpGroupBox4.Size = new System.Drawing.Size(432, 98);
      this.mpGroupBox4.TabIndex = 10;
      this.mpGroupBox4.TabStop = false;
      this.mpGroupBox4.Text = "Subtitle Settings";
      // 
      // mpCheckBoxEnableTTXTSub
      // 
      this.mpCheckBoxEnableTTXTSub.AutoSize = true;
      this.mpCheckBoxEnableTTXTSub.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.mpCheckBoxEnableTTXTSub.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxEnableTTXTSub.Location = new System.Drawing.Point(321, 27);
      this.mpCheckBoxEnableTTXTSub.Name = "mpCheckBoxEnableTTXTSub";
      this.mpCheckBoxEnableTTXTSub.Size = new System.Drawing.Size(13, 12);
      this.mpCheckBoxEnableTTXTSub.TabIndex = 11;
      this.mpCheckBoxEnableTTXTSub.UseVisualStyleBackColor = false;
      this.mpCheckBoxEnableTTXTSub.CheckedChanged += new System.EventHandler(this.mpCheckBox1_CheckedChanged);
      // 
      // mpLabel8
      // 
      this.mpLabel8.AutoSize = true;
      this.mpLabel8.Location = new System.Drawing.Point(157, 26);
      this.mpLabel8.Name = "mpLabel8";
      this.mpLabel8.Size = new System.Drawing.Size(127, 13);
      this.mpLabel8.TabIndex = 10;
      this.mpLabel8.Text = "Enable Teletext Subtitles:";
      // 
      // mpCheckBoxEnableDVBSub
      // 
      this.mpCheckBoxEnableDVBSub.AutoSize = true;
      this.mpCheckBoxEnableDVBSub.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.mpCheckBoxEnableDVBSub.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxEnableDVBSub.Location = new System.Drawing.Point(123, 27);
      this.mpCheckBoxEnableDVBSub.Name = "mpCheckBoxEnableDVBSub";
      this.mpCheckBoxEnableDVBSub.Size = new System.Drawing.Size(13, 12);
      this.mpCheckBoxEnableDVBSub.TabIndex = 7;
      this.mpCheckBoxEnableDVBSub.UseVisualStyleBackColor = false;
      // 
      // mpLabel9
      // 
      this.mpLabel9.AutoSize = true;
      this.mpLabel9.Location = new System.Drawing.Point(6, 26);
      this.mpLabel9.Name = "mpLabel9";
      this.mpLabel9.Size = new System.Drawing.Size(111, 13);
      this.mpLabel9.TabIndex = 6;
      this.mpLabel9.Text = "Enable DVB Subtitles:";
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox3.Controls.Add(this.mpLabel6);
      this.mpGroupBox3.Controls.Add(this.mpLabel7);
      this.mpGroupBox3.Controls.Add(this.mpButtonDownSubLang);
      this.mpGroupBox3.Controls.Add(this.mpButtonUpSubLang);
      this.mpGroupBox3.Controls.Add(this.mpButtonAddSubLang);
      this.mpGroupBox3.Controls.Add(this.mpButtonRemoveSubLang);
      this.mpGroupBox3.Controls.Add(this.mpListViewPreferredSubLang);
      this.mpGroupBox3.Controls.Add(this.mpListViewAvailSubLang);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(16, 16);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(432, 248);
      this.mpGroupBox3.TabIndex = 3;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Preferred Subtitle Languages";
      // 
      // mpLabel6
      // 
      this.mpLabel6.AutoSize = true;
      this.mpLabel6.Location = new System.Drawing.Point(236, 21);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new System.Drawing.Size(106, 13);
      this.mpLabel6.TabIndex = 7;
      this.mpLabel6.Text = "Preferred Languages";
      // 
      // mpLabel7
      // 
      this.mpLabel7.AutoSize = true;
      this.mpLabel7.Location = new System.Drawing.Point(6, 21);
      this.mpLabel7.Name = "mpLabel7";
      this.mpLabel7.Size = new System.Drawing.Size(106, 13);
      this.mpLabel7.TabIndex = 6;
      this.mpLabel7.Text = "Available Languages";
      // 
      // mpButtonDownSubLang
      // 
      this.mpButtonDownSubLang.Location = new System.Drawing.Point(291, 215);
      this.mpButtonDownSubLang.Name = "mpButtonDownSubLang";
      this.mpButtonDownSubLang.Size = new System.Drawing.Size(46, 23);
      this.mpButtonDownSubLang.TabIndex = 5;
      this.mpButtonDownSubLang.Text = "Down";
      this.mpButtonDownSubLang.UseVisualStyleBackColor = true;
      this.mpButtonDownSubLang.Click += new System.EventHandler(this.mpButtonDownSubLang_Click);
      // 
      // mpButtonUpSubLang
      // 
      this.mpButtonUpSubLang.Location = new System.Drawing.Point(239, 215);
      this.mpButtonUpSubLang.Name = "mpButtonUpSubLang";
      this.mpButtonUpSubLang.Size = new System.Drawing.Size(46, 23);
      this.mpButtonUpSubLang.TabIndex = 4;
      this.mpButtonUpSubLang.Text = "Up";
      this.mpButtonUpSubLang.UseVisualStyleBackColor = true;
      this.mpButtonUpSubLang.Click += new System.EventHandler(this.mpButtonUpSubLang_Click);
      // 
      // mpButtonAddSubLang
      // 
      this.mpButtonAddSubLang.Location = new System.Drawing.Point(200, 40);
      this.mpButtonAddSubLang.Name = "mpButtonAddSubLang";
      this.mpButtonAddSubLang.Size = new System.Drawing.Size(28, 23);
      this.mpButtonAddSubLang.TabIndex = 3;
      this.mpButtonAddSubLang.Text = ">>";
      this.mpButtonAddSubLang.UseVisualStyleBackColor = true;
      this.mpButtonAddSubLang.Click += new System.EventHandler(this.mpButtonAddSubLang_Click);
      // 
      // mpButtonRemoveSubLang
      // 
      this.mpButtonRemoveSubLang.Location = new System.Drawing.Point(200, 69);
      this.mpButtonRemoveSubLang.Name = "mpButtonRemoveSubLang";
      this.mpButtonRemoveSubLang.Size = new System.Drawing.Size(28, 23);
      this.mpButtonRemoveSubLang.TabIndex = 2;
      this.mpButtonRemoveSubLang.Text = "<<";
      this.mpButtonRemoveSubLang.UseVisualStyleBackColor = true;
      this.mpButtonRemoveSubLang.Click += new System.EventHandler(this.mpButtonRemoveSubLang_Click);
      // 
      // mpListViewPreferredSubLang
      // 
      this.mpListViewPreferredSubLang.AllowDrop = true;
      this.mpListViewPreferredSubLang.AllowRowReorder = true;
      this.mpListViewPreferredSubLang.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
      this.mpListViewPreferredSubLang.FullRowSelect = true;
      this.mpListViewPreferredSubLang.HideSelection = false;
      this.mpListViewPreferredSubLang.Location = new System.Drawing.Point(239, 40);
      this.mpListViewPreferredSubLang.Name = "mpListViewPreferredSubLang";
      this.mpListViewPreferredSubLang.Size = new System.Drawing.Size(183, 169);
      this.mpListViewPreferredSubLang.TabIndex = 1;
      this.mpListViewPreferredSubLang.UseCompatibleStateImageBehavior = false;
      this.mpListViewPreferredSubLang.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Language";
      this.columnHeader4.Width = 150;
      // 
      // mpListViewAvailSubLang
      // 
      this.mpListViewAvailSubLang.AllowDrop = true;
      this.mpListViewAvailSubLang.AllowRowReorder = true;
      this.mpListViewAvailSubLang.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3});
      this.mpListViewAvailSubLang.FullRowSelect = true;
      this.mpListViewAvailSubLang.HideSelection = false;
      this.mpListViewAvailSubLang.Location = new System.Drawing.Point(9, 40);
      this.mpListViewAvailSubLang.Name = "mpListViewAvailSubLang";
      this.mpListViewAvailSubLang.Size = new System.Drawing.Size(183, 169);
      this.mpListViewAvailSubLang.TabIndex = 0;
      this.mpListViewAvailSubLang.UseCompatibleStateImageBehavior = false;
      this.mpListViewAvailSubLang.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Language";
      this.columnHeader3.Width = 150;
      // 
      // TVClient
      // 
      this.Controls.Add(this.tabControlTVGeneral);
      this.Name = "TVClient";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.tabControlTVGeneral.ResumeLayout(false);
      this.tabPageImportSettings.ResumeLayout(false);
      this.tabPageFilenames.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.tabPageEncoderSettings.ResumeLayout(false);
      this.mpGroupBox4.ResumeLayout(false);
      this.mpGroupBox4.PerformLayout();
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
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
			if (mpListViewAvailAudioLang.SelectedItems.Count == 0) return;

			foreach (ListViewItem lang in mpListViewAvailAudioLang.SelectedItems)
			{
				mpListViewAvailAudioLang.Items.Remove(lang);
				mpListViewPreferredAudioLang.Items.Add(lang);
			}

		}

		private void mpButtonRemoveAudioLang_Click(object sender, EventArgs e)
		{
			if (mpListViewPreferredAudioLang.SelectedItems.Count == 0) return;

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
			if (mpListViewAvailSubLang.SelectedItems.Count == 0) return;

			foreach (ListViewItem lang in mpListViewAvailSubLang.SelectedItems)
			{
				mpListViewAvailSubLang.Items.Remove(lang);
				mpListViewPreferredSubLang.Items.Add(lang);
			}
		}

		private void mpButtonRemoveSubLang_Click(object sender, EventArgs e)
		{
			if (mpListViewPreferredSubLang.SelectedItems.Count == 0) return;

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

		private void moveItemDown(MediaPortal.UserInterface.Controls.MPListView mplistView)
		{
			ListView.SelectedIndexCollection indexes = mplistView.SelectedIndices;
			if (indexes.Count == 0) return;
			if (mplistView.Items.Count < 2) return;
			for (int i = indexes.Count - 1; i >= 0; i--)
			{
				int index = indexes[i];
				ListViewItem item = mplistView.Items[index];
				mplistView.Items.RemoveAt(index);
				if (index + 1 < mplistView.Items.Count)
					mplistView.Items.Insert(index + 1, item);
				else
					mplistView.Items.Add(item);
			}
		}

		private void moveItemUp(MediaPortal.UserInterface.Controls.MPListView mplistView)
		{

			ListView.SelectedIndexCollection indexes = mplistView.SelectedIndices;
			if (indexes.Count == 0) return;
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
    
  }
}
