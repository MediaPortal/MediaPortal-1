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

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

#endregion

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class TvTimeshifting : SectionSettings
  {
    #region CardInfo class

    public class CardInfo
    {
      public Card card;

      public CardInfo(Card newcard)
      {
        card = newcard;
      }

      public override string ToString()
      {
        return card.Name;
      }
    }

    #endregion

    #region Vars

    private bool _needRestart;

    #endregion

    #region Constructors

    public TvTimeshifting()
      : this("Timeshifting") {}

    public TvTimeshifting(string name)
      : base(name)
    {
      InitializeComponent();
    }

    #endregion

    #region Serialization

    public override void LoadSettings()
    {
      

      numericUpDownMinFiles.Value = ValueSanityCheck(ServiceAgents.Instance.SettingServiceAgent.GetValue("timeshiftMinFiles", 6), 3, 100);
      numericUpDownMaxFiles.Value = ValueSanityCheck(ServiceAgents.Instance.SettingServiceAgent.GetValue("timeshiftMaxFiles", 20), 3, 100);
      numericUpDownMaxFileSize.Value = ValueSanityCheck(ServiceAgents.Instance.SettingServiceAgent.GetValue("timeshiftMaxFileSize", 256), 20, 1024);
      numericUpDownWaitUnscrambled.Value = ValueSanityCheck(ServiceAgents.Instance.SettingServiceAgent.GetValue("timeshiftWaitForUnscrambled", 5), 1, 30);
      numericUpDownWaitTimeshifting.Value = ValueSanityCheck(ServiceAgents.Instance.SettingServiceAgent.GetValue("timeshiftWaitForTimeshifting", 15), 1, 30);
      numericUpDownMaxFreeCardsToTry.Value = ValueSanityCheck(ServiceAgents.Instance.SettingServiceAgent.GetValue("timeshiftMaxFreeCardsToTry", 0), 0, 100);
      numericParkedStreamTimeout.Value = ValueSanityCheck(ServiceAgents.Instance.SettingServiceAgent.GetValue("parkedStreamTimeout", 5), 1, 300);
    }

    public override void SaveSettings()
    {
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeshiftMinFiles", (int)numericUpDownMinFiles.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeshiftMaxFiles", (int)numericUpDownMaxFiles.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeshiftMaxFileSize", (int)numericUpDownMaxFileSize.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeshiftWaitForUnscrambled", (int)numericUpDownWaitUnscrambled.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeshiftWaitForTimeshifting", (int) numericUpDownWaitTimeshifting.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeshiftMaxFreeCardsToTry", (int) numericUpDownMaxFreeCardsToTry.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("parkedStreamTimeout", (int) numericParkedStreamTimeout.Value);
    }

    #endregion

    #region GUI-Events

    private void comboBoxCards_SelectedIndexChanged(object sender, EventArgs e)
    {
      CardInfo info = (CardInfo)comboBoxCards.SelectedItem;
      textBoxTimeShiftFolder.Text = info.card.TimeshiftingFolder;

      if (String.IsNullOrEmpty(textBoxTimeShiftFolder.Text))
      {
        textBoxTimeShiftFolder.Text = TVDatabase.TVBusinessLayer.Common.GetDefaultTimeshiftingFolder();
        if (!Directory.Exists(textBoxTimeShiftFolder.Text))
        {
          Directory.CreateDirectory(textBoxTimeShiftFolder.Text);
        }
        _needRestart = true;
      }
    }

  

    public override void OnSectionActivated()
    {
      _needRestart = false;
      comboBoxCards.Items.Clear();
      IList<Card> cards = ServiceAgents.Instance.CardServiceAgent.ListAllCards(CardIncludeRelationEnum.None);
      foreach (Card card in cards)
      {
        comboBoxCards.Items.Add(new CardInfo(card));
      }

      if (comboBoxCards.Items.Count > 0)
      {
        comboBoxCards.SelectedIndex = 0;
      }

      TimeshiftSpaceAdditionalInfo();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();

      SaveSettings();
      if (_needRestart)
      {        
        ServiceAgents.Instance.ControllerServiceAgent.Restart();
      }
    }

    // Browse TimeShift folder
    private void buttonTimeShiftBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.SelectedPath = textBoxTimeShiftFolder.Text;
      dlg.Description = "Specify timeshift folder";
      dlg.ShowNewFolderButton = true;
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        textBoxTimeShiftFolder.Text = dlg.SelectedPath;
      }
    }

    // When TimeShift folder has been changed
    private void textBoxTimeShiftFolder_TextChanged(object sender, EventArgs e)
    {
      CardInfo info = (CardInfo)comboBoxCards.SelectedItem;
      if (info.card.TimeshiftingFolder != textBoxTimeShiftFolder.Text)
      {
        info.card.TimeshiftingFolder = textBoxTimeShiftFolder.Text;
        ServiceAgents.Instance.CardServiceAgent.SaveCard(info.card);
        _needRestart = true;
      }
    }

    // Click on Same timeshift folder for all cards
    private void buttonSameTimeshiftFolder_Click(object sender, EventArgs e)
    {
      // Change timeshiftFolder for all cards
      for (int iIndex = 0; iIndex < comboBoxCards.Items.Count; iIndex++)
      {
        CardInfo info = (CardInfo)comboBoxCards.Items[iIndex];
        if (info.card.TimeshiftingFolder != textBoxTimeShiftFolder.Text)
        {
          info.card.TimeshiftingFolder = textBoxTimeShiftFolder.Text;
          ServiceAgents.Instance.CardServiceAgent.SaveCard(info.card);
          if (!_needRestart)
          {
            _needRestart = true;
          }
        }
      }
    }

    private static decimal ValueSanityCheck(decimal Value, int Min, int Max)
    {
      if (Value < Min)
        return Min;
      if (Value > Max)
        return Max;
      return Value;
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e) {}

    #endregion

    private void numericUpDownMaxFileSize_ValueChanged(object sender, EventArgs e)
    {
      TimeshiftSpaceAdditionalInfo();
    }

    private void numericUpDownMinFiles_ValueChanged(object sender, EventArgs e)
    {
      TimeshiftSpaceAdditionalInfo();
    }

    private void TimeshiftSpaceAdditionalInfo()
    {
      lblMinFileSizeNeeded.Text = "Minimum drive space needed           : " +
                                  (3 * numericUpDownMaxFileSize.Value).ToString().PadLeft(8) + " MByte";
      lblFileSizeNeeded.Text = "Drive space needed                         : " +
                               ((numericUpDownMinFiles.Value * numericUpDownMaxFileSize.Value) +
                                numericUpDownMaxFileSize.Value).ToString().PadLeft(7) + " MByte";
      lblOverhead.Text = "Drive space overhead needed         : " + numericUpDownMaxFileSize.Value.ToString().PadLeft(8) +
                         " MByte";
      lblTimeSD.Text = "Maximum timeshifting for SD content: approx. " +
                       ((float)(numericUpDownMinFiles.Value * numericUpDownMaxFileSize.Value) / 100f * 2.75f) +
                       " Minutes";
      lblTimeHD.Text = "Maximum timeshifting for HD content: approx. " +
                       ((float)(numericUpDownMinFiles.Value * numericUpDownMaxFileSize.Value) / 100f * 1.00f) +
                       " Minutes";
    }
  }
}