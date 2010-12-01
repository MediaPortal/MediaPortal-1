using System;
using System.Windows.Forms;
using DirectShowLib.BDA;
using TvDatabase;

namespace SetupTv.Dialogs
{
  public partial class FormDVBSTuningDetail : SetupControls.FormTuningDetailCommon
  {
    public FormDVBSTuningDetail()
    {
      InitializeComponent();
    }

    private void FormDVBSTuningDetail_Load(object sender, EventArgs e)
    {
      if (TuningDetail != null)
      {
        textBoxFrequency.Text = TuningDetail.Frequency.ToString();
        textBoxNetworkId.Text = TuningDetail.NetworkId.ToString();
        textBoxTransportId.Text = TuningDetail.TransportId.ToString();
        textBoxServiceId.Text = TuningDetail.ServiceId.ToString();
        textBoxSymbolRate.Text = TuningDetail.Symbolrate.ToString();
        textBoxSwitch.Text = TuningDetail.SwitchingFrequency.ToString();
        textBoxDVBSChannel.Text = TuningDetail.ChannelNumber.ToString();
        textBoxDVBSPmt.Text = TuningDetail.PmtPid.ToString();
        textBoxDVBSProvider.Text = TuningDetail.Provider;
        checkBoxDVBSfta.Checked = TuningDetail.FreeToAir;
        comboBoxPol.SelectedIndex = TuningDetail.Polarisation + 1;
        comboBoxModulation.SelectedIndex = TuningDetail.Modulation + 1;
        comboBoxInnerFecRate.SelectedIndex = TuningDetail.InnerFecRate + 1;
        comboBoxPilot.SelectedIndex = TuningDetail.Pilot + 1;
        comboBoxRollOff.SelectedIndex = TuningDetail.RollOff + 1;
        comboBoxDisEqc.SelectedIndex = TuningDetail.Diseqc;
      } else
      {
        textBoxFrequency.Text = "";
        textBoxNetworkId.Text = "";
        textBoxTransportId.Text = "";
        textBoxServiceId.Text = "";
        textBoxSymbolRate.Text = "";
        textBoxSwitch.Text = "";
        textBoxDVBSChannel.Text = "";
        textBoxDVBSPmt.Text = "";
        textBoxDVBSProvider.Text = "";
        checkBoxDVBSfta.Checked = false;
        comboBoxPol.SelectedIndex = -1;
        comboBoxModulation.SelectedIndex = -1;
        comboBoxInnerFecRate.SelectedIndex = -1;
        comboBoxPilot.SelectedIndex = -1;
        comboBoxRollOff.SelectedIndex = -1;
        comboBoxDisEqc.SelectedIndex = -1;
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
      TuningDetail.ChannelType = 3;
      TuningDetail.Frequency = Int32.Parse(textBoxFrequency.Text);
      TuningDetail.NetworkId = Int32.Parse(textBoxNetworkId.Text);
      TuningDetail.TransportId = Int32.Parse(textBoxTransportId.Text);
      TuningDetail.ServiceId = Int32.Parse(textBoxServiceId.Text);
      TuningDetail.Symbolrate = Int32.Parse(textBoxSymbolRate.Text);
      TuningDetail.SwitchingFrequency = Int32.Parse(textBoxSwitch.Text);
      TuningDetail.Polarisation = (int)(Polarisation)(comboBoxPol.SelectedIndex - 1);
      TuningDetail.InnerFecRate = (int)(BinaryConvolutionCodeRate)(comboBoxInnerFecRate.SelectedIndex - 1);
      TuningDetail.Pilot = (int)(Pilot)(comboBoxPilot.SelectedIndex - 1);
      TuningDetail.RollOff = (int)(RollOff)(comboBoxRollOff.SelectedIndex - 1);
      TuningDetail.Modulation = (int)(ModulationType)(comboBoxModulation.SelectedIndex - 1);
      TuningDetail.ChannelNumber = Int32.Parse(textBoxDVBSChannel.Text);
      TuningDetail.PmtPid = Int32.Parse(textBoxDVBSPmt.Text);
      TuningDetail.Provider = textBoxDVBSProvider.Text;
      TuningDetail.FreeToAir = checkBoxDVBSfta.Checked;
      TuningDetail.Diseqc = comboBoxDisEqc.SelectedIndex;
    }

    private bool ValidateInput()
    {
      if (textBoxFrequency.Text.Length == 0)
      {
        MessageBox.Show(this, "Please enter a valid frequency!", "Incorrect input");
        return false;
      }
      int lcn, freq, onid, tsid, sid, symbolrate, switchfreq, pmt;
      if (!Int32.TryParse(textBoxDVBSChannel.Text, out lcn))
      {
        MessageBox.Show(this, "Please enter a valid channel number!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxFrequency.Text, out freq))
      {
        MessageBox.Show(this, "Please enter a valid frequency!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxNetworkId.Text, out onid))
      {
        MessageBox.Show(this, "Please enter a valid network id!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxTransportId.Text, out tsid))
      {
        MessageBox.Show(this, "Please enter a valid transport id!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxServiceId.Text, out sid))
      {
        MessageBox.Show(this, "Please enter a valid service id!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxSymbolRate.Text, out symbolrate))
      {
        MessageBox.Show(this, "Please enter a valid symbol rate!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxSwitch.Text, out switchfreq))
      {
        MessageBox.Show(this, "Please enter a valid LNB switch!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBSPmt.Text, out pmt))
      {
        MessageBox.Show(this, "Please enter a valid pmt id!", "Incorrect input");
        return false;
      }
      if (onid <= 0 && tsid < 0 && sid < 0)
      {
        MessageBox.Show(this, "Please enter a valid network, transport and service id!", "Incorrect input");
        return false;
      }
      return true;
    }
  }
}
