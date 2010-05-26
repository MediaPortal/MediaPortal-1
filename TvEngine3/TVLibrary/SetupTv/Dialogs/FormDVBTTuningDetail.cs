using System;
using System.Windows.Forms;

namespace SetupTv.Dialogs
{
  public partial class FormDVBTTuningDetail : SetupControls.FormTuningDetailCommon
  {
    public FormDVBTTuningDetail()
    {
      InitializeComponent();
    }

    private void FormDVBTTuningDetail_Load(object sender, EventArgs e)
    {
      if (TuningDetail != null)
      {
        textBoxDVBTChannel.Text = TuningDetail.ChannelNumber.ToString();
        textBoxDVBTfreq.Text = TuningDetail.Frequency.ToString();
        textBox8.Text = TuningDetail.NetworkId.ToString();
        textBox7.Text = TuningDetail.TransportId.ToString();
        textBox6.Text = TuningDetail.ServiceId.ToString();
        textBoxDVBTProvider.Text = TuningDetail.Provider;
        checkBoxDVBTfta.Checked = TuningDetail.FreeToAir;
        comboBoxBandWidth.SelectedIndex = TuningDetail.Bandwidth == 7 ? 0 : 1;
        textBoxPmt.Text = TuningDetail.PmtPid.ToString();
      } else
      {
        textBoxDVBTChannel.Text = "";
        textBoxDVBTfreq.Text = "";
        textBox8.Text = "";
        textBox7.Text = "";
        textBox6.Text = "";
        textBoxDVBTProvider.Text = "";
        checkBoxDVBTfta.Checked = false;
        comboBoxBandWidth.SelectedIndex = -1;
        textBoxPmt.Text = "";
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
      TuningDetail.ChannelType = 4;
      TuningDetail.ChannelNumber = Int32.Parse(textBoxDVBTChannel.Text);
      TuningDetail.Frequency = Int32.Parse(textBoxDVBTfreq.Text);
      TuningDetail.NetworkId = Int32.Parse(textBox8.Text);
      TuningDetail.TransportId = Int32.Parse(textBox7.Text);
      TuningDetail.ServiceId = Int32.Parse(textBox6.Text);
      TuningDetail.Provider = textBoxDVBTProvider.Text;
      TuningDetail.FreeToAir = checkBoxDVBTfta.Checked;
      TuningDetail.Bandwidth = comboBoxBandWidth.SelectedIndex == 0 ? 7 : 8;
      TuningDetail.PmtPid = Int32.Parse(textBoxPmt.Text);
    }

    private bool ValidateInput()
    {
      if (textBoxDVBTfreq.Text.Length == 0) { return false; }
      int lcn, freq, onid, tsid, sid, pmt;
      if (!Int32.TryParse(textBoxDVBTChannel.Text, out lcn)) { return false; }
      if (!Int32.TryParse(textBoxDVBTfreq.Text, out freq)) { return false; }
      if (!Int32.TryParse(textBox8.Text, out onid)) { return false; }
      if (!Int32.TryParse(textBox7.Text, out tsid)) { return false; }
      if (!Int32.TryParse(textBox6.Text, out sid)) { return false; }
      if (!Int32.TryParse(textBoxPmt.Text, out pmt)) { return false; }
      if (onid <= 0 && tsid < 0 && sid < 0) { return false; }
      return true;
    }

  }
}
