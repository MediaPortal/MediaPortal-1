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
using System.Collections.Generic;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.ServerBlaster
{
  public partial class BlasterSetup : SectionSettings
  {

    public BlasterSetup()
    {
      InitializeComponent();
    }

    public override void OnSectionDeActivated()
    {
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("SrvBlasterType", comboBoxType.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("SrvBlasterSpeed", comboBoxSpeed.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("SrvBlaster1Card", comboBoxBlaster1.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("SrvBlaster2Card", comboBoxBlaster2.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("SrvBlasterLog", Convert.ToString(checkBoxExtLog.Checked));
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("SrvBlasterSendSelect", Convert.ToString(checkSendSelect.Checked));            

      base.OnSectionDeActivated();
    }

    public override void OnSectionActivated()
    {
      
      comboBoxType.SelectedIndex = Convert.ToInt16(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("SrvBlasterType", "0").Value);
      comboBoxSpeed.SelectedIndex = Convert.ToInt16(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("SrvBlasterSpeed", "0").Value);
      comboBoxBlaster1.Items.Clear();
      comboBoxBlaster2.Items.Clear();
      comboBoxBlaster1.Items.Add("None");
      comboBoxBlaster2.Items.Add("None");

      IList<Card> cards = ServiceAgents.Instance.CardServiceAgent.ListAllCards(CardIncludeRelationEnum.None);

      foreach (Card card in cards)
      {
        comboBoxBlaster1.Items.Add(card.Name);
        comboBoxBlaster2.Items.Add(card.Name);
      }
      this.LogDebug("CB1Size {0}, CB2Size {1}, BT1 {2}, BT2 {3}", comboBoxBlaster1.Items.Count,
                    comboBoxBlaster1.Items.Count, Convert.ToInt16(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("SrvBlaster1Card", "0").Value),
                    Convert.ToInt16(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("SrvBlaster2Card", "0").Value));
      comboBoxBlaster1.SelectedIndex = Convert.ToInt16(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("SrvBlaster1Card", "0").Value);
      comboBoxBlaster2.SelectedIndex = Convert.ToInt16(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("SrvBlaster2Card", "0").Value);
      checkBoxExtLog.Checked = (ServiceAgents.Instance.SettingServiceAgent.GetSetting("SrvBlasterLog").Value == "True");
      checkSendSelect.Checked = (ServiceAgents.Instance.SettingServiceAgent.GetSetting("SrvBlasterSendSelect").Value == "True");
    }

    private void ComboBox1SelectedIndexChanged(object sender, EventArgs e)
    {
      bool enabled;

      switch (comboBoxType.SelectedIndex)
      {
        case 0: // Microsoft
          if (OSInfo.OSInfo.VistaOrLater())
          {
            enabled = false;
            checkBoxExtLog.Visible = false;
            mpLabelAdditionalNotes.Text =
              "Because of an architecture change in the driver handling, MCE blasting is no more available under Vista and newer operating systems";
          }
          else
          {
            enabled = true;
          }
          break;

        case 1: // SMK
          enabled = true;
          break;

        case 2: // Hauppauge blasting
          enabled = false;
          checkBoxExtLog.Visible = true;
          mpLabelAdditionalNotes.Text =
            "To configure the Hauppauge IR Blaster, use the original Hauppauge IR configuration software.";
          break;
        default:
          enabled = false;
          break;
      }
      if (enabled)
      {
        comboBoxSpeed.Visible = true;
        comboBoxBlaster1.Visible = true;
        comboBoxBlaster2.Visible = true;
        labelBlasterSpeed.Visible = true;
        labelUseBlaster1.Visible = true;
        labelUseBlaster2.Visible = true;
        checkBoxExtLog.Visible = true;
        checkSendSelect.Visible = true;
        mpLabelAdditionalNotes.Visible = false;
      }
      else
      {
        comboBoxSpeed.Visible = false;
        comboBoxBlaster1.Visible = false;
        comboBoxBlaster2.Visible = false;
        labelBlasterSpeed.Visible = false;
        labelUseBlaster1.Visible = false;
        labelUseBlaster2.Visible = false;
        checkSendSelect.Visible = false;
        mpLabelAdditionalNotes.Visible = true;
      }
    }
  }
}