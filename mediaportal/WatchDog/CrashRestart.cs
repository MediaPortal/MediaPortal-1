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

using System;
using System.Windows.Forms;

namespace WatchDog
{
  public partial class CrashRestartDlg : MPForm
  {
    private int ticks = 10;

    public CrashRestartDlg(int cancelDelay)
    {
      InitializeComponent();
      ticks = cancelDelay;
      lDelay.Text = ticks.ToString() + " second(s)";
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      ticks--;
      lDelay.Text = ticks.ToString() + " second(s)";
      if (ticks == 0)
      {
        timer1.Enabled = false;
        this.DialogResult = DialogResult.OK;
        Close();
      }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      Close();
    }
  }
}