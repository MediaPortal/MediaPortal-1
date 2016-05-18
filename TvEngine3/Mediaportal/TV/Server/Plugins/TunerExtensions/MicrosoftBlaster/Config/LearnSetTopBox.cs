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
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Config
{
  internal partial class LearnSetTopBox : Form
  {
    public delegate LearnResult LearnCommandDelegate(TimeSpan timeLimit, out string command);

    private LearnCommandDelegate _learnCommandDelegate = null;
    private SetTopBoxProfile _profile = new SetTopBoxProfile();

    public LearnSetTopBox(LearnCommandDelegate learnCommandDelegate)
    {
      _learnCommandDelegate = learnCommandDelegate;
      InitializeComponent();
    }

    public SetTopBoxProfile Profile
    {
      get
      {
        return _profile;
      }
    }

    private void UpdateOkayButtonState()
    {
      if (string.IsNullOrEmpty(textBoxName.Text))
      {
        buttonOkay.Enabled = false;
        return;
      }
      foreach (ListViewItem item in listViewCommands.Items)
      {
        if ((bool)item.Tag && item.SubItems[1].Tag == null)
        {
          buttonOkay.Enabled = false;
          return;
        }
      }
      buttonOkay.Enabled = true;
    }

    private void Learn_Shown(object sender, EventArgs e)
    {
      listViewCommands.Items.Clear();

      ListViewItem item;
      for (int i = 0; i < 10; i++)
      {
        item = listViewCommands.Items.Add(i.ToString());
        item.Tag = true;    // required
        item.UseItemStyleForSubItems = false;
        ListViewItem.ListViewSubItem subItem = item.SubItems.Add("");
        subItem.BackColor = Color.Red;
      }

      foreach (string command in new List<string> { "Pre-change (-/--)", "Separator (./-)", "Enter/Select/OK", "Power Toggle", "Power Off", "Power On" })
      {
        item = listViewCommands.Items.Add(command);
        item.Tag = false;   // optional
        item.UseItemStyleForSubItems = false;
        item.SubItems.Add("");
      }

      numericUpDownCommandDelay.Value = 200;
      numericUpDownPowerChangeDelay.Value = 3000;
      comboBoxDigitCount.SelectedIndex = 0;

      listViewCommands_SelectedIndexChanged(null, null);
    }

    private void textBoxName_TextChanged(object sender, EventArgs e)
    {
      UpdateOkayButtonState();
    }

    private void listViewCommands_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listViewCommands.SelectedItems != null && listViewCommands.SelectedItems.Count > 0)
      {
        buttonLearn.Enabled = listViewCommands.SelectedItems.Count == 1;
        foreach (ListViewItem item in listViewCommands.SelectedItems)
        {
          if (item.SubItems[1].Tag != null)
          {
            buttonForget.Enabled = true;
            return;
          }
        }
        buttonForget.Enabled = false;
      }
      else
      {
        buttonLearn.Enabled = false;
        buttonForget.Enabled = false;
      }
    }

    private void buttonLearn_Click(object sender, EventArgs e)
    {
      if (listViewCommands.SelectedItems == null || listViewCommands.SelectedItems.Count != 1)
      {
        return;
      }

      ListViewItem item = listViewCommands.SelectedItems[0];
      this.LogInfo("learn STB: learn command, name = {0}", item.SubItems[0].Text);

      NotifyForm dlg = new NotifyForm(SectionSettings.MESSAGE_CAPTION, "Hold the remote control approximately 5 centimeters from the receiver and press the target button once.");
      dlg.Show(this);
      dlg.WaitForDisplay();

      ThreadPool.QueueUserWorkItem(
        delegate
        {
          string command;
          LearnResult result = _learnCommandDelegate(new TimeSpan(0, 0, 10), out command);
          this.LogInfo("learn STB: learn result = {0}, command = {1}", result, command);
          this.Invoke((MethodInvoker)delegate
          {
            dlg.Close();
            dlg.Dispose();

            if (result == LearnResult.Success)
            {
              item.SubItems[1].Tag = command;
              item.SubItems[1].BackColor = Color.ForestGreen;
              buttonForget.Enabled = true;

              UpdateOkayButtonState();
              listViewCommands.Focus();
            }
            else if (result == LearnResult.Fail)
            {
              MessageBox.Show("Learning failed. Ensure the remote control batteries are not flat, and try again with the remote control slightly closer to or further from the receiver.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (result == LearnResult.NotOpen)
            {
              MessageBox.Show("The selected transceiver's interface is currently closed.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (result == LearnResult.TimeOut)
            {
              MessageBox.Show("Learning was incomplete. Please try again.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (result == LearnResult.Unavailable)
            {
              MessageBox.Show("The selected transceiver is currently not available. Please check that it is connected.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (result == LearnResult.Unsupported)
            {
              MessageBox.Show("The selected transceiver does not support learning.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
          });
        }
      );
    }

    private void buttonForget_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in listViewCommands.SelectedItems)
      {
        this.LogDebug("learn STB: forget command, name = {0}", item.SubItems[0].Text);
        item.SubItems[1].Tag = null;
        if ((bool)item.Tag)   // Is the command required?
        {
          item.SubItems[1].BackColor = Color.Red;
        }
        else
        {
          item.SubItems[1].BackColor = Color.White;
        }
      }

      UpdateOkayButtonState();
      listViewCommands.Focus();
    }

    private void buttonOkay_Click(object sender, EventArgs e)
    {
      _profile.Name = textBoxName.Text.Trim();
      _profile.Digit0 = listViewCommands.Items[0].SubItems[1].Tag as string;
      _profile.Digit1 = listViewCommands.Items[1].SubItems[1].Tag as string;
      _profile.Digit2 = listViewCommands.Items[2].SubItems[1].Tag as string;
      _profile.Digit3 = listViewCommands.Items[3].SubItems[1].Tag as string;
      _profile.Digit4 = listViewCommands.Items[4].SubItems[1].Tag as string;
      _profile.Digit5 = listViewCommands.Items[5].SubItems[1].Tag as string;
      _profile.Digit6 = listViewCommands.Items[6].SubItems[1].Tag as string;
      _profile.Digit7 = listViewCommands.Items[7].SubItems[1].Tag as string;
      _profile.Digit8 = listViewCommands.Items[8].SubItems[1].Tag as string;
      _profile.Digit9 = listViewCommands.Items[9].SubItems[1].Tag as string;
      _profile.PreChange = listViewCommands.Items[10].SubItems[1].Tag as string;
      _profile.Separator = listViewCommands.Items[11].SubItems[1].Tag as string;
      _profile.Enter = listViewCommands.Items[12].SubItems[1].Tag as string;
      _profile.PowerToggle = listViewCommands.Items[13].SubItems[1].Tag as string;
      _profile.PowerOff = listViewCommands.Items[14].SubItems[1].Tag as string;
      _profile.PowerOn = listViewCommands.Items[15].SubItems[1].Tag as string;

      _profile.CommandDelay = (int)numericUpDownCommandDelay.Value;
      _profile.PowerChangeDelay = (int)numericUpDownPowerChangeDelay.Value;
      _profile.DigitCount = comboBoxDigitCount.SelectedIndex;
      if (_profile.DigitCount != 0)
      {
        _profile.DigitCount++;
      }

      this.LogDebug("learn STB: configuration...");
      this.LogDebug("  name               = {0}", _profile.Name);
      this.LogDebug("  command delay      = {0} ms", _profile.CommandDelay);
      this.LogDebug("  power change delay = {0} ms", _profile.PowerChangeDelay);
      this.LogDebug("  digit count        = {0}", _profile.DigitCount);

      DialogResult = DialogResult.OK;
      Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }
  }
}