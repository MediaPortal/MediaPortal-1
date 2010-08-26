using System;
using System.Windows.Forms;
using DirectShowLib.BDA;

namespace SetupTv.Dialogs
{
  public partial class FormDVBCTuningDetail : SetupControls.FormTuningDetailCommon
  {
    public FormDVBCTuningDetail()
    {
      InitializeComponent();
    }

    private void FormDVBCTuningDetail_Load(object sender, System.EventArgs e)
    {
      comboBoxDvbCModulation.Items.Clear();
      foreach (ModulationType modValue in Enum.GetValues(typeof (ModulationType)))
      {
        comboBoxDvbCModulation.Items.Add(modValue);
      }

      if (TuningDetail != null)
      {
        //Editing
        textboxFreq.Text = TuningDetail.Frequency.ToString();
        textBoxONID.Text = TuningDetail.NetworkId.ToString();
        textBoxTSID.Text = TuningDetail.TransportId.ToString();
        textBoxSID.Text = TuningDetail.ServiceId.ToString();
        textBoxSymbolRate.Text = TuningDetail.Symbolrate.ToString();
        textBoxDVBCPmt.Text = TuningDetail.PmtPid.ToString();
        textBoxDVBCProvider.Text = TuningDetail.Provider;
        checkBoxDVBCfta.Checked = TuningDetail.FreeToAir;
        comboBoxDvbCModulation.SelectedItem = (ModulationType)TuningDetail.Modulation;
      } else
      {
        textboxFreq.Text = "";
        textBoxONID.Text = "";
        textBoxTSID.Text = "";
        textBoxSID.Text = "";
        textBoxSymbolRate.Text = "";
        textBoxDVBCPmt.Text = "";
        textBoxDVBCProvider.Text = "";
        checkBoxDVBCfta.Checked = false;
        comboBoxDvbCModulation.SelectedIndex = -1;
      }
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
      TuningDetail.Frequency = Convert.ToInt32(textboxFreq.Text);
      TuningDetail.NetworkId = Convert.ToInt32(textBoxONID.Text);
      TuningDetail.TransportId = Convert.ToInt32(textBoxTSID.Text);
      TuningDetail.ServiceId = Convert.ToInt32(textBoxSID.Text);
      TuningDetail.Symbolrate = Convert.ToInt32(textBoxSymbolRate.Text);
      TuningDetail.PmtPid = Convert.ToInt32(textBoxDVBCPmt.Text);
      TuningDetail.Provider = textBoxDVBCProvider.Text;
      TuningDetail.FreeToAir = checkBoxDVBCfta.Checked;
      TuningDetail.Modulation = (int)comboBoxDvbCModulation.SelectedItem;
      TuningDetail.ChannelType = 2;
    }

    private bool ValidateInput()
    {
      int freq, onid, tsid, sid, symbolrate, pmt;
      if (!Int32.TryParse(textboxFreq.Text, out freq)) { return false; }
      if (!Int32.TryParse(textBoxONID.Text, out onid)) { return false; }
      if (!Int32.TryParse(textBoxTSID.Text, out tsid)) { return false; }
      if (!Int32.TryParse(textBoxSID.Text, out sid)) { return false; }
      if (!Int32.TryParse(textBoxSymbolRate.Text, out symbolrate)) { return false; }
      if (!Int32.TryParse(textBoxDVBCPmt.Text, out pmt)) { return false; }
      if (onid <= 0 || tsid < 0 || sid < 0) { return false; }
      return true;
    }

  }
}
