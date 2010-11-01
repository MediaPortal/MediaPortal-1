#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

#region usings

using System;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#endregion

namespace MediaPortal.ProcessPlugins.MusicDBReorg
{
  public partial class MusicDBReorgSettings : MPConfigForm
  {
    #region Ctor

    public MusicDBReorgSettings()
    {
      InitializeComponent();
      LoadSettings();
    }

    #endregion

    #region Serialization

    private void LoadSettings()
    {
      // load settings
      using (Settings reader = new MPSettings())
      {
        int hours, minutes;
        hours = reader.GetValueAsInt("musicdbreorg", "hours", 0);
        minutes = reader.GetValueAsInt("musicdbreorg", "minutes", 0);
        VerifySchedule(ref hours, ref minutes);
        hoursTextBox.Text = hours.ToString();
        minutesTextBox.Text = minutes.ToString();
        if (hoursTextBox.Text.Length == 1)
        {
          hoursTextBox.Text = "0" + hoursTextBox.Text;
        }
        if (minutesTextBox.Text.Length == 1)
        {
          minutesTextBox.Text = "0" + minutesTextBox.Text;
        }

        cbMonday.Checked = reader.GetValueAsBool("musicdbreorg", "monday", true);
        cbTuesday.Checked = reader.GetValueAsBool("musicdbreorg", "tuesday", true);
        cbWednesday.Checked = reader.GetValueAsBool("musicdbreorg", "wednesday", true);
        cbThursday.Checked = reader.GetValueAsBool("musicdbreorg", "thursday", true);
        cbFriday.Checked = reader.GetValueAsBool("musicdbreorg", "friday", true);
        cbSaturday.Checked = reader.GetValueAsBool("musicdbreorg", "saturday", true);
        cbSunday.Checked = reader.GetValueAsBool("musicdbreorg", "sunday", true);
      }
    }

    private void SaveSettings()
    {
      // save settings
      using (Settings writer = new MPSettings())
      {
        int hours, minutes;
        hours = Int32.Parse(hoursTextBox.Text);
        minutes = Int32.Parse(minutesTextBox.Text);
        VerifySchedule(ref hours, ref minutes);
        writer.SetValue("musicdbreorg", "hours", hours);
        writer.SetValue("musicdbreorg", "minutes", minutes);

        writer.SetValueAsBool("musicdbreorg", "monday", cbMonday.Checked);
        writer.SetValueAsBool("musicdbreorg", "tuesday", cbTuesday.Checked);
        writer.SetValueAsBool("musicdbreorg", "wednesday", cbWednesday.Checked);
        writer.SetValueAsBool("musicdbreorg", "thursday", cbThursday.Checked);
        writer.SetValueAsBool("musicdbreorg", "friday", cbFriday.Checked);
        writer.SetValueAsBool("musicdbreorg", "saturday", cbSaturday.Checked);
        writer.SetValueAsBool("musicdbreorg", "sunday", cbSunday.Checked);
      }
    }

    private void VerifySchedule(ref int hours, ref int minutes)
    {
      if (hours < 0 || hours > 23)
      {
        hours = 0;
      }
      if (minutes < 0 || minutes > 59)
      {
        minutes = 0;
      }
    }

    #endregion

    #region Event handlers

    private void hoursTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
      // Allow only numbers, and backspace.
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void minutesTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
      // Allow only numbers, and backspace.
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      // save settings
      SaveSettings();
      //close form
      Close();
    }


    private void btnCancel_Click(object sender, EventArgs e)
    {
      // restore settings
      LoadSettings();
      //close form
      Close();
    }

    #endregion
  }
}