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
      } else
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
      if (edStreamURL.Text.Length == 0) { return false; }
      return true;
    }
  }
}
