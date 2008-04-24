using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using MediaPortal.Configuration;
using TvDatabase;


namespace TvPlugin
{
  public partial class RadioSetupForm : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    #region variables
    private bool _showAllChannelsGroup = true;
    private bool _rememberLastGroup = true;
    private string _rootGroup="(none)";
    #endregion

    #region Serialisation

    private void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _showAllChannelsGroup = xmlreader.GetValueAsBool("myradio", "showallchannelsgroup", true);
        _rememberLastGroup = xmlreader.GetValueAsBool("myradio", "rememberlastgroup", true);
        _rootGroup = xmlreader.GetValueAsString("myradio", "rootgroup", "(none)");
      }
    }

    private void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValueAsBool("myradio", "showallchannelsgroup", _showAllChannelsGroup);
        xmlreader.SetValueAsBool("myradio", "rememberlastgroup", _rememberLastGroup);
        xmlreader.SetValue("myradio", "rootgroup",_rootGroup);
      }
    }

    #endregion

    public RadioSetupForm()
    {
      InitializeComponent();
    }

    private void RadioSetupForm_Load(object sender, EventArgs e)
    {
      LoadSettings();
      cbShowAllChannelsGroup.Checked = _showAllChannelsGroup;
      cbRememberLastGroup.Checked = _rememberLastGroup;
      comboBoxGroups.Items.Clear();
      comboBoxGroups.Items.Add("(none)");
      int selectedIdx=0;
      IList groups = RadioChannelGroup.ListAll();
      foreach (RadioChannelGroup group in groups)
      {
        int idx = comboBoxGroups.Items.Add(group.GroupName);
        if (group.GroupName == _rootGroup)
          selectedIdx = idx;
      }
      comboBoxGroups.SelectedIndex = selectedIdx;
    }

    private void cbShowAllChannelsGroup_Click(object sender, EventArgs e)
    {
      _showAllChannelsGroup = cbShowAllChannelsGroup.Checked;
    }

    private void cbRememberLastGroup_CheckedChanged(object sender, EventArgs e)
    {
      _rememberLastGroup = cbRememberLastGroup.Checked;
    }

    private void comboBoxGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
      _rootGroup = comboBoxGroups.Text;
    }

    private void mpButtonOk_Click(object sender, EventArgs e)
    {
      SaveSettings();
    }
  }
}