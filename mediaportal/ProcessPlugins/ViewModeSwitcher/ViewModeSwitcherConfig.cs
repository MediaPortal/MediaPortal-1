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
using System.Diagnostics;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ViewModeSwitcher
{
  public partial class ViewModeSwitcherConfig : Form
  {
    private readonly ViewModeswitcherSettings currentSettings = new ViewModeswitcherSettings();

    public ViewModeSwitcherConfig()
    {
      InitializeComponent();
      LoadSettings();
    }

    public void RefreshFormComponents()
    {
      cmbFBViewMode.Items.Clear();
      foreach (String mode in ViewModeswitcherSettings.LoadMediaPortalXml())
      {
        cmbFBViewMode.Items.Add(mode);
      }
      cmbFBViewMode.SelectedItem = currentSettings.FallBackViewMode.ToString();

      cbVerboseLog.Checked = currentSettings.verboseLog;
      cbShowSwitchMsg.Checked = currentSettings.ShowSwitchMsg;
      cbUseFallbackRule.Checked = currentSettings.UseFallbackRule;
      cbDisableLBGlobaly.Checked = currentSettings.DisableLBGlobaly;
      numBlackLevel.Value = currentSettings.LBMaxBlackLevel;
      numBlackLevAve.Value = currentSettings.LBMinBlackLevel;
      fbosUpDown.Value = currentSettings.fboverScan;
      numBBdetWidth.Value = currentSettings.DetectWidthPercent;
      numBBdetHeight.Value = currentSettings.DetectHeightPercent;      
      cbDisableForVideo.Checked   = currentSettings.disableForVideo; 
      cbDisableLBForVideo.Checked = currentSettings.disableLBForVideo;     
      numMaxCropLimit.Value = currentSettings.LBMaxCropLimitPercent;
      numSymLimit.Value = currentSettings.LBSymLimitPercent;
      numDetectInterval.Value = currentSettings.LBdetectInterval;
      
      ReBuildDataGrid();
    }

    public void LoadSettings()
    {
      currentSettings.LoadSettings();
      RefreshFormComponents();
    }

    public void SaveSettings()
    {
      currentSettings.verboseLog = cbVerboseLog.Checked;
      currentSettings.ShowSwitchMsg = cbShowSwitchMsg.Checked;
      currentSettings.UseFallbackRule = cbUseFallbackRule.Checked;
      currentSettings.FallBackViewMode = ViewModeswitcherSettings.StringToViewMode(cmbFBViewMode.Text);
      currentSettings.DisableLBGlobaly = cbDisableLBGlobaly.Checked;
      currentSettings.LBMaxBlackLevel = numBlackLevel.Value;
      currentSettings.LBMinBlackLevel = numBlackLevAve.Value;

      currentSettings.fboverScan = (int)fbosUpDown.Value;
      currentSettings.DetectWidthPercent = numBBdetWidth.Value;
      currentSettings.DetectHeightPercent = numBBdetHeight.Value;
      currentSettings.LBMaxCropLimitPercent = numMaxCropLimit.Value;
      currentSettings.LBSymLimitPercent = numSymLimit.Value;
      currentSettings.LBdetectInterval = (int)numDetectInterval.Value;

      currentSettings.disableForVideo = cbDisableForVideo.Checked;
      currentSettings.disableLBForVideo = cbDisableLBForVideo.Checked;

      currentSettings.SaveSettings();
    }

    /// <summary>
    /// Returns the currently selected rule
    /// </summary>
    /// <returns>currently selected rule</returns>
    public Rule GetCurrentRule()
    {
      return currentSettings.ViewModeRules.Count > 0
               ? currentSettings.ViewModeRules[dg_RuleSets.CurrentRow.Index]
               : null;
    }

    public void ReBuildDataGrid()
    {
      dg_RuleSets.Rows.Clear();
      foreach (Rule tmpRule in currentSettings.ViewModeRules)
      {
        dg_RuleSets.Rows.Add(tmpRule.Enabled, 
                             tmpRule.Name, 
                             tmpRule.ARFrom.ToString(), 
                             tmpRule.ARTo.ToString(),
                             tmpRule.MinWidth.ToString(), 
                             tmpRule.MaxWidth.ToString(), 
                             tmpRule.MinHeight.ToString(),
                             tmpRule.MaxHeight.ToString(), 
                             tmpRule.ViewMode.ToString(),
                             tmpRule.OverScan.ToString(), 
                             tmpRule.EnableLBDetection,
                             tmpRule.AutoCrop, 
                             tmpRule.MaxCrop); 
      }
    }

    private void bOK_Click(object sender, EventArgs e)
    {
      SaveSettings();
      Close();
    }

    private void bAdd_Click(object sender, EventArgs e)
    {
      Rule tmpRule = new Rule();
      tmpRule.Enabled = true;
      tmpRule.Name = "New rule";
      tmpRule.ARFrom = 1.2;
      tmpRule.ARTo = 1.46;
      tmpRule.MinWidth = 200;
      tmpRule.MaxWidth = 2000;
      tmpRule.MinHeight = 200;
      tmpRule.MaxHeight = 2000;
      tmpRule.AutoCrop = false;
      tmpRule.ViewMode = Geometry.Type.Normal;
      tmpRule.MaxCrop = true;
      tmpRule.OverScan = 8;
      tmpRule.EnableLBDetection = false;

      currentSettings.ViewModeRules.Add(tmpRule);

      ReBuildDataGrid();
    }

    private void bDelete_Click(object sender, EventArgs e)
    {
      if (currentSettings.ViewModeRules.Count <= 0)
      {
        return;
      }
      currentSettings.ViewModeRules.RemoveAt(dg_RuleSets.CurrentRow.Index);
      ReBuildDataGrid();
    }

    private void bModify_Click(object sender, EventArgs e)
    {
      if (currentSettings.ViewModeRules.Count <= 0)
      {
        return;
      }
      ViewModeSwitcherRuleDetail frmRuleDetail = new ViewModeSwitcherRuleDetail();
      frmRuleDetail.MainForm = this;
      if (frmRuleDetail.ShowDialog() == DialogResult.OK)
      {
        ReBuildDataGrid();
      }
    }

    private void bCancel_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void linkLabelForum_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        Process.Start("http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/141_Configuration/MediaPortal_Configuration/95_Plugins/ViewModeSwitcher", "");
      }
      catch (Exception ex)
      {
        Log.Warn(ex.ToString());
      }
    }

    private void cbUseFallbackRule_CheckedChanged(object sender, EventArgs e)
    {
      cmbFBViewMode.Enabled = cbUseFallbackRule.Checked;
      fbosUpDown.Enabled = cbUseFallbackRule.Checked;
    }

    private void cbDisableForVideo_CheckedChanged(object sender, EventArgs e)
    {
      cbDisableLBForVideo.Checked = cbDisableForVideo.Checked;
    }

    private void cbEnableAdvanced_CheckedChanged(object sender, EventArgs e)
    {
      cbDisableLBForVideo.Visible = cbEnableAdvanced.Checked;
      cbDisableForVideo.Visible   = cbEnableAdvanced.Checked;
      cbVerboseLog.Visible        = cbEnableAdvanced.Checked;
      label12.Visible             = cbEnableAdvanced.Checked;
      numMaxCropLimit.Visible     = cbEnableAdvanced.Checked;
      label11.Visible             = cbEnableAdvanced.Checked;
      label10.Visible             = cbEnableAdvanced.Checked;
      label9.Visible              = cbEnableAdvanced.Checked;
      numBlackLevAve.Visible      = cbEnableAdvanced.Checked;
      label8.Visible              = cbEnableAdvanced.Checked;
      label7.Visible              = cbEnableAdvanced.Checked;
      numBBdetHeight.Visible      = cbEnableAdvanced.Checked;
      numBBdetWidth.Visible       = cbEnableAdvanced.Checked;
      label1.Visible              = cbEnableAdvanced.Checked;
      numBlackLevel.Visible       = cbEnableAdvanced.Checked;
      label18.Visible             = cbEnableAdvanced.Checked;
      label13.Visible             = cbEnableAdvanced.Checked;
      numSymLimit.Visible         = cbEnableAdvanced.Checked;
      label14.Visible             = cbEnableAdvanced.Checked;
      label15.Visible             = cbEnableAdvanced.Checked;
      label16.Visible             = cbEnableAdvanced.Checked;
      numDetectInterval.Visible   = cbEnableAdvanced.Checked;
    }

    
    private void bExport_Click(object sender, EventArgs e)
    {
      saveFileDialog.AddExtension = true;
      //saveFileDialog.DefaultExt = "VmsSettings";
      saveFileDialog.Filter = "ViewModeSwitcher config (*.VmsSettings)|*.VmsSettings|All files (*.*)|*.*";
      saveFileDialog.FilterIndex = 0;
      saveFileDialog.InitialDirectory = Config.GetSubFolder(Config.Dir.Plugins, "Process");
      if (saveFileDialog.ShowDialog() == DialogResult.OK)
      {
        SaveSettings(); // make sure all settings are stored before
        currentSettings.SaveSettings(saveFileDialog.FileName);
      }
    }

    private void bImport_Click(object sender, EventArgs e)
    {
      openFileDialog.AddExtension = true;
      openFileDialog.Filter = "ViewModeSwitcher config (*.VmsSettings)|*.VmsSettings|All files (*.*)|*.*";
      openFileDialog.FilterIndex = 0;
      openFileDialog.InitialDirectory = Config.GetSubFolder(Config.Dir.Plugins, "Process");
      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
        if (!currentSettings.LoadSettings(openFileDialog.FileName))
        {
          MessageBox.Show("Import Error!");
        }
        else
        {
          RefreshFormComponents(); // refresh the form
        }
      }
    }

    private void bLoadDefaults_Click(object sender, EventArgs e)
    {
      if (!currentSettings.LoadDefaultSettings())
      {
        MessageBox.Show("Import Error!");
      }
      else
      {
        RefreshFormComponents(); // refresh the form
      }
    }

    private void dg_RuleSets_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
      if (e.ColumnIndex == 0 && e.RowIndex >= 0)
      {
        bool isChecked = currentSettings.ViewModeRules[e.RowIndex].Enabled;         
        currentSettings.ViewModeRules[e.RowIndex].Enabled = !isChecked;
        dg_RuleSets[0, e.RowIndex].Value = !isChecked;
      }
    }   
    
  }
}