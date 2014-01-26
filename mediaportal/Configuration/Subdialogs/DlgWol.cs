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
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Profile;

namespace MediaPortal.Configuration
{
  public partial class DlgWol : MPConfigForm
  {
    public DlgWol()
    {
      InitializeComponent();

      using (Settings xmlreader = new MPSettings())
      {
        mpUpDownWolTimeout.Value = xmlreader.GetValueAsInt("WOL", "WolTimeout", 10);
        mpUpDownWolResend.Value = xmlreader.GetValueAsInt("WOL", "WolResendTime", 1);
        mpUpDownWaitTime.Value = xmlreader.GetValueAsInt("WOL", "WaitTimeAfterWOL", 0);
      }
    }

    private void mpButtonOK_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("WOL", "WolTimeout", mpUpDownWolTimeout.Value);
        xmlwriter.SetValue("WOL", "WolResendTime", mpUpDownWolResend.Value);
        xmlwriter.SetValue("WOL", "WaitTimeAfterWOL", mpUpDownWaitTime.Value);
      }
      this.Hide();
    }
  }
}
