using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using MediaPortal.Util;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.WebEPG
{
  public partial class WebEPGGrabberSettings : Form
  {
    public WebEPGGrabberSettings()
    {
      InitializeComponent();
      LoadSettings();
    }

    #region Serialization

    private void LoadSettings()
    {
      // load settings
      using (Settings reader = new Settings(Config.Get(Config.Dir.Config) + "mediaportal.xml"))
      {
        int hours, minutes;
        hours = reader.GetValueAsInt("webepggrabber", "hours", 0);
        minutes = reader.GetValueAsInt("webepggrabber", "minutes", 0);
        VerifySchedule(ref hours, ref minutes);
        hoursTextBox.Text = hours.ToString();
        minutesTextBox.Text = minutes.ToString() ;
        if (hoursTextBox.Text.Length == 1)
          hoursTextBox.Text = "0" + hoursTextBox.Text;
        if (minutesTextBox.Text.Length == 1)
          minutesTextBox.Text = "0" + minutesTextBox.Text;

        cbMonday.Checked = reader.GetValueAsBool("webepggrabber", "monday", true);
        cbTuesday.Checked = reader.GetValueAsBool("webepggrabber", "tuesday", true);
        cbWednesday.Checked = reader.GetValueAsBool("webepggrabber", "wednesday", true);
        cbThursday.Checked = reader.GetValueAsBool("webepggrabber", "thursday", true);
        cbFriday.Checked = reader.GetValueAsBool("webepggrabber", "friday", true);
        cbSaturday.Checked = reader.GetValueAsBool("webepggrabber", "saturday", true);
        cbSunday.Checked = reader.GetValueAsBool("webepggrabber", "sunday", true);
      }
    }

    private void SaveSettings()
    {
      // save settings
      using (Settings writer = new Settings(Config.Get(Config.Dir.Config) + "mediaportal.xml"))
      {
        int hours, minutes;
        hours = Int32.Parse(hoursTextBox.Text);
        minutes = Int32.Parse(minutesTextBox.Text);
        VerifySchedule(ref hours, ref minutes);
        writer.SetValue("webepggrabber", "hours", hours);
        writer.SetValue("webepggrabber", "minutes", minutes);

        writer.SetValueAsBool("webepggrabber", "monday", cbMonday.Checked);
        writer.SetValueAsBool("webepggrabber", "tuesday", cbTuesday.Checked);
        writer.SetValueAsBool("webepggrabber", "wednesday", cbWednesday.Checked);
        writer.SetValueAsBool("webepggrabber", "thursday", cbThursday.Checked);
        writer.SetValueAsBool("webepggrabber", "friday", cbFriday.Checked);
        writer.SetValueAsBool("webepggrabber", "saturday", cbSaturday.Checked);
        writer.SetValueAsBool("webepggrabber", "sunday", cbSunday.Checked);
      }
    }

    private void VerifySchedule(ref int hours, ref int minutes)
    {
      if (hours < 0 || hours > 23)
        hours = 0;
      if (minutes < 0 || minutes > 59)
        minutes = 0;
    }

    #endregion
    
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

    private void mpButton1_Click(object sender, EventArgs e)
    {
      // save settings
      SaveSettings();
      //close form
      Close();
    }

  }
}