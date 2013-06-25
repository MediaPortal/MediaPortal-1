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
      fbosUpDown.Value = currentSettings.fboverScan;

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
      currentSettings.fboverScan = (int)fbosUpDown.Value;
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
//        dg_RuleSets.Rows.Add(tmpRule.Enabled, tmpRule.Name, tmpRule.ARFrom.ToString(), tmpRule.ARTo.ToString(),
//                             tmpRule.MinWidth.ToString(), tmpRule.MaxWidth.ToString(), tmpRule.MinHeight.ToString(),
//                             tmpRule.MaxHeight.ToString(), tmpRule.AutoCrop, tmpRule.ViewMode.ToString(),
//                             tmpRule.MaxCrop, tmpRule.OverScan.ToString(), tmpRule.EnableLBDetection);

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
      tmpRule.ARFrom = 1.2f;
      tmpRule.ARTo = 1.46f;
      tmpRule.MinWidth = 200;
      tmpRule.MaxWidth = 2000;
      tmpRule.MinHeight = 200;
      tmpRule.MaxHeight = 2000;
      tmpRule.AutoCrop = false;
      tmpRule.ViewMode = Geometry.Type.Normal;
      tmpRule.MaxCrop = true;
      tmpRule.OverScan = 0;
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
        Process.Start("http://forum.team-mediaportal.com/threads/viewmodeswitcher-for-1-4-0.119781/", "");
      }
      catch (Exception ex)
      {
        Log.Warn(ex.ToString());
      }
    }

    private void cbUseFallbackRule_CheckedChanged(object sender, EventArgs e)
    {
      cmbFBViewMode.Enabled = cbUseFallbackRule.Checked;
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
  }
}