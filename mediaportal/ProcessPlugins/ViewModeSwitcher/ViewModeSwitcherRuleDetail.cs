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
using System.Windows.Forms;

namespace ProcessPlugins.ViewModeSwitcher
{
  public partial class ViewModeSwitcherRuleDetail : Form
  {
    private Rule currentRule;
    public ViewModeSwitcherConfig MainForm;

    public ViewModeSwitcherRuleDetail()
    {
      InitializeComponent();
    }

    private void ViewModeSwitcherRuleDetail_Load(object sender, EventArgs e)
    {
      cmbViewMode.Items.Clear();
      foreach (String mode in ViewModeswitcherSettings.LoadMediaPortalXml())
      {
        cmbViewMode.Items.Add(mode);
      }

      currentRule = MainForm.GetCurrentRule();
      cbEnabled.Checked = currentRule.Enabled;
      txbName.Text = currentRule.Name;
      txbARFrom.Text = currentRule.ARFrom.ToString();
      txbARTo.Text = currentRule.ARTo.ToString();
      txbMinWidth.Text = currentRule.MinWidth.ToString();
      txbMaxWidth.Text = currentRule.MaxWidth.ToString();
      txbMinHeight.Text = currentRule.MinHeight.ToString();
      txbMaxHeight.Text = currentRule.MaxHeight.ToString();
      cbAutoCropEnabled.Checked = currentRule.AutoCrop;
      cbMaxCropEnabled.Checked = currentRule.MaxCrop;
      txbOverScan.Text = currentRule.OverScan.ToString();
      cmbViewMode.SelectedItem = currentRule.ViewMode.ToString();
      cb_EnableLBDetection.Checked = currentRule.EnableLBDetection;
    }

    private void bCancel_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void bOK_Click(object sender, EventArgs e)
    {
      try {
        currentRule.Enabled = cbEnabled.Checked;
        currentRule.Name = txbName.Text;
        currentRule.ARFrom = Math.Max(Math.Min(Convert.ToDouble(txbARFrom.Text), 20.0), -20.0);
        currentRule.ARTo   = Math.Max(Math.Min(Convert.ToDouble(txbARTo.Text), 20.0), -20.0);
        currentRule.MinWidth = Convert.ToInt16(txbMinWidth.Text);
        currentRule.MaxWidth = Convert.ToInt16(txbMaxWidth.Text);
        currentRule.MinHeight = Convert.ToInt16(txbMinHeight.Text);
        currentRule.MaxHeight = Convert.ToInt16(txbMaxHeight.Text);
  
        currentRule.AutoCrop = cbAutoCropEnabled.Checked;
  
        String tmpViewMode = cmbViewMode.Text;
  
        currentRule.ViewMode = ViewModeswitcherSettings.StringToViewMode(tmpViewMode);
  
        currentRule.MaxCrop = cbMaxCropEnabled.Checked;
        currentRule.OverScan = Convert.ToInt16(txbOverScan.Text);
        currentRule.EnableLBDetection = cb_EnableLBDetection.Checked;
      } catch (Exception) {}
  
      Close();
    }

    private void cbAutoCropEnabled_CheckedChanged(object sender, EventArgs e)
    {
      if (cbAutoCropEnabled.Checked)
      {
        cb_EnableLBDetection.Checked = true; //Tick this box as well
      }
    }
    
    private void cb_EnableLBDetection_CheckedChanged(object sender, EventArgs e)
    {
      if (!cb_EnableLBDetection.Checked)
      {
        cbAutoCropEnabled.Checked = false;
      }
    }
  }
}