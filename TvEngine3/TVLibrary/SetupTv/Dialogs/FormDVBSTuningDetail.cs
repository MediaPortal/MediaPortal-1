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
        textBox5.Text = TuningDetail.Frequency.ToString();
        textBox4.Text = TuningDetail.NetworkId.ToString();
        textBox3.Text = TuningDetail.TransportId.ToString();
        textBox2.Text = TuningDetail.ServiceId.ToString();
        textBox1.Text = TuningDetail.Symbolrate.ToString();
        textBoxSwitch.Text = TuningDetail.SwitchingFrequency.ToString();
        textBoxDVBSChannel.Text = TuningDetail.ChannelNumber.ToString();
        textBoxDVBSPmt.Text = TuningDetail.PmtPid.ToString();
        textBoxDVBSProvider.Text = TuningDetail.Provider;
        checkBoxDVBSfta.Checked = TuningDetail.FreeToAir;
        switch ((Polarisation)TuningDetail.Polarisation)
        {
          case Polarisation.LinearH:
            comboBoxPol.SelectedIndex = 0;
            break;
          case Polarisation.LinearV:
            comboBoxPol.SelectedIndex = 1;
            break;
          case Polarisation.CircularL:
            comboBoxPol.SelectedIndex = 2;
            break;
          case Polarisation.CircularR:
            comboBoxPol.SelectedIndex = 3;
            break;
        }
        comboBoxModulation.SelectedIndex = TuningDetail.Modulation + 1;
        comboBoxInnerFecRate.SelectedIndex = TuningDetail.InnerFecRate + 1;
        comboBoxPilot.SelectedIndex = TuningDetail.Pilot + 1;
        comboBoxRollOff.SelectedIndex = TuningDetail.RollOff + 1;
        comboBoxDisEqc.SelectedIndex = TuningDetail.Diseqc;
      } else
      {
        textBox5.Text = "";
        textBox4.Text = "";
        textBox3.Text = "";
        textBox2.Text = "";
        textBox1.Text = "";
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
      TuningDetail.Frequency = Int32.Parse(textBox5.Text);
      TuningDetail.NetworkId = Int32.Parse(textBox4.Text);
      TuningDetail.TransportId = Int32.Parse(textBox3.Text);
      TuningDetail.ServiceId = Int32.Parse(textBox2.Text);
      TuningDetail.Symbolrate = Int32.Parse(textBox1.Text);
      TuningDetail.SwitchingFrequency = Int32.Parse(textBoxSwitch.Text);
      TuningDetail.InnerFecRate = (int)(BinaryConvolutionCodeRate)(comboBoxInnerFecRate.SelectedIndex - 1);
      TuningDetail.Pilot = (int)(Pilot)(comboBoxPilot.SelectedIndex - 1);
      TuningDetail.RollOff = (int)(RollOff)(comboBoxRollOff.SelectedIndex - 1);
      TuningDetail.Modulation = (int)(ModulationType)(comboBoxModulation.SelectedIndex - 1);
      TuningDetail.ChannelNumber = Int32.Parse(textBoxDVBSChannel.Text);
      TuningDetail.PmtPid = Int32.Parse(textBoxDVBSPmt.Text);
      TuningDetail.Provider = textBoxDVBSProvider.Text;
      TuningDetail.FreeToAir = checkBoxDVBSfta.Checked;
      switch (comboBoxPol.SelectedIndex)
      {
        case 0:
          TuningDetail.Polarisation = (int)Polarisation.LinearH;
          break;
        case 1:
          TuningDetail.Polarisation = (int)Polarisation.LinearV;
          break;
        case 2:
          TuningDetail.Polarisation = (int)Polarisation.CircularL;
          break;
        case 3:
          TuningDetail.Polarisation = (int)Polarisation.CircularR;
          break;
      }
      TuningDetail.Diseqc = comboBoxDisEqc.SelectedIndex;
    }

    private bool ValidateInput()
    {
      if (textBox5.Text.Length == 0) { return false; }
      int lcn, freq, onid, tsid, sid, symbolrate, switchfreq, pmt;
      if (!Int32.TryParse(textBoxDVBSChannel.Text, out lcn)) { return false; }
      if (!Int32.TryParse(textBox5.Text, out freq)) { return false; }
      if (!Int32.TryParse(textBox4.Text, out onid)) { return false; }
      if (!Int32.TryParse(textBox3.Text, out tsid)) { return false; }
      if (!Int32.TryParse(textBox2.Text, out sid)) { return false; }
      if (!Int32.TryParse(textBox1.Text, out symbolrate)) { return false; }
      if (!Int32.TryParse(textBoxSwitch.Text, out switchfreq)) { return false; }
      if (!Int32.TryParse(textBoxDVBSPmt.Text, out pmt)) { return false; }
      if (onid <= 0 && tsid < 0 && sid < 0) { return false; }
      return true;
    }
  }
}
