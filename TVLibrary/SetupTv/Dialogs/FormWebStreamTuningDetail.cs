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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SetupTv.Dialogs
{
  public partial class FormWebStreamTuningDetail : SetupControls.FormTuningDetailCommon
  {
    public FormWebStreamTuningDetail()
    {
      InitializeComponent();
    }

    private void FormWebStreamTuningDetail_Load(object sender, EventArgs e)
    {
      if (TuningDetail != null)
      {
        edStreamURL.Text = TuningDetail.Url;
        nudStreamBitrate.Value = TuningDetail.Bitrate;
      }
      else
      {
        edStreamURL.Text = "";
        nudStreamBitrate.Value = 0;
      }
    }

    private void btnSearchSHOUTcast_Click(object sender, EventArgs e)
    {
      SearchSHOUTcast dlg = new SearchSHOUTcast();
      dlg.ShowDialog(this);
      if (dlg.Station == null) return;
      edStreamURL.Text = dlg.Station.url;
      nudStreamBitrate.Value = dlg.Station.bitrate;
    }

    private void mpButtonOk_Click(object sender, EventArgs e)
    {
      if (ValidateInput())
      {
        if (TuningDetail == null)
        {
          TuningDetail = CreateInitialTuningDetail();
        }
        UpdateTuningDetail();
        DialogResult = DialogResult.OK;
        Close();
      }
    }

    private void UpdateTuningDetail()
    {
      TuningDetail.ChannelType = 5;
      TuningDetail.Url = edStreamURL.Text;
      TuningDetail.Bitrate = (int)nudStreamBitrate.Value;
    }

    private bool ValidateInput()
    {
      if (edStreamURL.Text.Length == 0)
      {
        return false;
      }
      return true;
    }
  }
}