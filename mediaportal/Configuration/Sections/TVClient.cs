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
    private string _preferredLanguages;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBoxHostname;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxPrefAC3;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPListView mpListViewLanguages;
    private IList<string> _languagesAvail;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxPrefAudioOverLang;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private IList<string> _languageCodes;
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
        _preferredLanguages = xmlreader.GetValueAsString("tvservice", "preferredlanguages", "");
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
                    mpListViewLanguages.Items.Clear();
                    for (int i = 0; i < _languagesAvail.Count; i++)
                    {
                      ListViewItem item = new ListViewItem();
                      item.Text = _languagesAvail[i];
                      item.Tag = _languageCodes[i];
                      item.Checked = _preferredLanguages.Contains(_languageCodes[i]);
                      mpListViewLanguages.Items.Add(item);
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
        
        foreach (ListViewItem item in mpListViewLanguages.Items)
        {
          if (item.Checked)
            prefLangs += (string)item.Tag + ";";
        }
        xmlreader.SetValue("tvservice", "preferredlanguages", prefLangs);

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
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListViewLanguages = new MediaPortal.UserInterface.Controls.MPListView();
      this.mpCheckBoxPrefAC3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox2.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Controls.Add(this.mpTextBoxHostname);
      this.mpGroupBox2.Controls.Add(this.mpLabel3);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(8, 12);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(361, 53);
      this.mpGroupBox2.TabIndex = 10;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "TvServer";
      // 
      // mpTextBoxHostname
      // 
      this.mpTextBoxHostname.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBoxHostname.Location = new System.Drawing.Point(126, 25);
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
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.Controls.Add(this.mpListViewLanguages);
      this.mpGroupBox1.Controls.Add(this.mpCheckBoxPrefAC3);
      this.mpGroupBox1.Controls.Add(this.mpLabel2);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(10, 71);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(365, 268);
      this.mpGroupBox1.TabIndex = 9;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Audio preferences";
      // 
      // mpCheckBoxPrefAudioOverLang
      // 
      this.mpCheckBoxPrefAudioOverLang.AutoSize = true;
      this.mpCheckBoxPrefAudioOverLang.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.mpCheckBoxPrefAudioOverLang.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxPrefAudioOverLang.Location = new System.Drawing.Point(332, 230);
      this.mpCheckBoxPrefAudioOverLang.Name = "mpCheckBoxPrefAudioOverLang";
      this.mpCheckBoxPrefAudioOverLang.Size = new System.Drawing.Size(13, 12);
      this.mpCheckBoxPrefAudioOverLang.TabIndex = 11;
      this.mpCheckBoxPrefAudioOverLang.UseVisualStyleBackColor = false;
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(168, 229);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(158, 13);
      this.mpLabel4.TabIndex = 10;
      this.mpLabel4.Text = "Prefer audiotype over language:";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(20, 21);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(103, 13);
      this.mpLabel1.TabIndex = 9;
      this.mpLabel1.Text = "Prefered Languages";
      // 
      // mpListViewLanguages
      // 
      this.mpListViewLanguages.AllowDrop = true;
      this.mpListViewLanguages.AllowRowReorder = true;
      this.mpListViewLanguages.CheckBoxes = true;
      this.mpListViewLanguages.HideSelection = false;
      this.mpListViewLanguages.Location = new System.Drawing.Point(6, 40);
      this.mpListViewLanguages.Name = "mpListViewLanguages";
      this.mpListViewLanguages.Size = new System.Drawing.Size(353, 169);
      this.mpListViewLanguages.TabIndex = 8;
      this.mpListViewLanguages.UseCompatibleStateImageBehavior = false;
      this.mpListViewLanguages.View = System.Windows.Forms.View.List;
      // 
      // mpCheckBoxPrefAC3
      // 
      this.mpCheckBoxPrefAC3.AutoSize = true;
      this.mpCheckBoxPrefAC3.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.mpCheckBoxPrefAC3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxPrefAC3.Location = new System.Drawing.Point(124, 230);
      this.mpCheckBoxPrefAC3.Name = "mpCheckBoxPrefAC3";
      this.mpCheckBoxPrefAC3.Size = new System.Drawing.Size(13, 12);
      this.mpCheckBoxPrefAC3.TabIndex = 7;
      this.mpCheckBoxPrefAC3.UseVisualStyleBackColor = false;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(17, 229);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(61, 13);
      this.mpLabel2.TabIndex = 6;
      this.mpLabel2.Text = "Prefer AC3:";
      // 
      // TVClient
      // 
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "TVClient";
      this.Size = new System.Drawing.Size(399, 410);
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    
  }
}
