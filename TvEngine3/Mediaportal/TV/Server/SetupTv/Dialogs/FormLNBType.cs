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
using System.Windows.Forms;
using MediaPortal.Common.Utils.ExtensionMethods;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormLNBType : Form
  {
    public FormLNBType()
    {
      InitializeComponent();
      SetControlsStateBasedOnFrequencySelection();
    }

    public LnbType LnbType { get; set; }

    private void button2_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (ValidateInput())
      {
        UpdateLnbType();
        DialogResult = DialogResult.OK;
        Close(); 
      }      
    }

    private void UpdateLnbType()
    {
      if (LnbType == null)
      {
        LnbType = new LnbType();
      }

      LnbType.LowBandFrequency = GetCalculatedFrequency(txtFreq1.Text);
      LnbType.HighBandFrequency = GetCalculatedFrequency(txtFreq2.Text);
      LnbType.SwitchFrequency = GetCalculatedSwitchFrequency(txtSwitchFreq.Text);
      LnbType.IsBandStacked = mpRadioButtonBand.Checked;
      LnbType.IsToroidal = checkBoxTorodial.Checked;
      LnbType.ToneState = (int)mpComboBox22KhzControl.SelectedValue;
      LnbType.Name = txtLnbTypeName.Text;

      ServiceAgents.Instance.CardServiceAgent.SaveLnbType(LnbType);
    }


    private int GetCalculatedSwitchFrequency(string value)
    {
      int freq = Int32.Parse(value);
      if (mpRadioButtonDual.Checked)
      {        
        freq = freq * 1000;                
      }
      else
      {
        freq = 18000000;
      }
      return freq;
    }

    private int GetCalculatedFrequency(string value)
    {
      int freq = Int32.Parse(value);
      if (mpRadioButtonSingle.Checked)
      {
        if (mpComboBox22KhzControl.SelectedIndex == 2) //off
        {
          freq = freq*1000;
        }
        else if (mpComboBox22KhzControl.SelectedIndex == 1) //on
        {
          freq = ((freq - 500)*1000);
        }
      }
      else
      {
        freq = freq * 1000;
      }
      return freq;
    }

    private bool ValidateInput()
    {
      if (txtFreq1.Enabled)
      {
        int freq1;
        if (!Int32.TryParse(txtFreq1.Text, out freq1))
        {
          MessageBox.Show(this, "Please enter a valid frequency 1!", "Incorrect input");
          return false;
        }
        if (freq1 < 1)
        {
          MessageBox.Show(this, "Please enter frequency 1 with value larger than 0!", "Incorrect input");
          return false;
        }

      }

      if (txtFreq2.Enabled)
      {
        int freq2;
        if (!Int32.TryParse(txtFreq2.Text, out freq2))
        {
          MessageBox.Show(this, "Please enter a valid frequency 2!", "Incorrect input");
          return false;
        }
        if (freq2 < 1)
        {
          MessageBox.Show(this, "Please enter frequency 2 with value larger than 0!", "Incorrect input");
          return false;
        }
      }

      if (txtSwitchFreq.Enabled)
      {
        int sw;
        if (!Int32.TryParse(txtSwitchFreq.Text, out sw))
        {
          MessageBox.Show(this, "Please enter a valid switch!", "Incorrect input");
          return false;
        }
        if (sw < 1)
        {
          MessageBox.Show(this, "Please enter switch with value larger than 0!", "Incorrect input");
          return false;
        }
      }

      return true;

    }





    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void mpRadioButtonSingle_CheckedChanged(object sender, EventArgs e)
    {
      SetControlsStateBasedOnFrequencySelection();
    } 

    private void mpRadioButtonDual_CheckedChanged(object sender, EventArgs e)
    {
      SetControlsStateBasedOnFrequencySelection();
    }

    private void mpRadioButtonBand_CheckedChanged(object sender, EventArgs e)
    {
      SetControlsStateBasedOnFrequencySelection();
    }

    private void SetControlsStateBasedOnFrequencySelection()
    {
      txtFreq1.Enabled = true;
      checkBoxTorodial.Enabled = true;

      txtFreq2.Enabled = mpRadioButtonBand.Checked || mpRadioButtonDual.Checked;
      txtSwitchFreq.Enabled = mpRadioButtonDual.Checked;
      mpComboBox22KhzControl.Enabled = mpRadioButtonBand.Checked || mpRadioButtonSingle.Checked;


      if (mpRadioButtonDual.Checked)
      {
        txtFreq2.Text = "0";
      }

      mpComboBox22KhzControl.Items.Clear();
      if (mpRadioButtonDual.Checked)
      {
        mpComboBox22KhzControl.Items.Add(Tone22k.Auto);
      }
      else
      {
        mpComboBox22KhzControl.Items.Add(Tone22k.On);
        mpComboBox22KhzControl.Items.Add(Tone22k.Off);
      }
      mpComboBox22KhzControl.SelectedIndex = 0;
    }

    private void FormLNBType_Load(object sender, EventArgs e)
    {
      if (LnbType != null)
      {
        txtLnbTypeName.Text = LnbType.Name;

        txtFreq1.Text = Convert.ToString(LnbType.LowBandFrequency);
        txtFreq2.Text = Convert.ToString(LnbType.HighBandFrequency);
        txtSwitchFreq.Text = Convert.ToString(LnbType.SwitchFrequency);

        checkBoxTorodial.Checked = LnbType.IsToroidal;
        
        if (LnbType.IsBandStacked)
        {
          mpRadioButtonBand.Checked = true;
        }
        else if (LnbType.HighBandFrequency > 0)
        {
          mpRadioButtonDual.Checked = true;
        }
        else
        {
          mpRadioButtonSingle.Checked = true;
        }

        SetControlsStateBasedOnFrequencySelection();
        mpComboBox22KhzControl.SelectedItem = ((Tone22k)LnbType.ToneState).ToString();        
      }
      else
      {
        txtFreq1.Text = "0";
        txtFreq2.Text = "0";
        txtSwitchFreq.Text = "0";
        txtLnbTypeName.Text = "";

        checkBoxTorodial.Checked = false;                
        mpRadioButtonSingle.Checked = true;
        
        mpComboBox22KhzControl.SelectedIndex = 0;
      }
    }

  }
}