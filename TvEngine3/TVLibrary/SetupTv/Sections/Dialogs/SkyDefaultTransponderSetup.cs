using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvDatabase;
using TvLibrary.Implementations;
using TvLibrary.Implementations.Helper.Providers;

namespace SetupTv.Sections.Dialogs
{
  public partial class SkyDefaultTransponderSetup : Form
  {
    /// <summary>
    /// Sky country
    /// </summary>
    public enum eSkyCounntry
    {
      UK,
      Italy,
      Australia,
    }

    /// <summary>
    /// Sky country
    /// </summary>
    private readonly eSkyCounntry _skyCountry;

    /// <summary>
    /// Cstr
    /// </summary>
    /// <param name="country"></param>
    public SkyDefaultTransponderSetup(eSkyCounntry country)
    {
      _skyCountry = country;

      InitializeComponent();

      LoadValues();

      tbFrequency.Select(tbFrequency.Text.Length, tbFrequency.Text.Length);
    }

    /// <summary>
    /// Loads the values into the fields
    /// </summary>
    private void LoadValues()
    {
      if (_skyCountry == eSkyCounntry.UK)
      {
        tbFrequency.Text = SkyUK.Instance.DefaultTransponderFrequency.ToString();
        tbSymbolRate.Text = SkyUK.Instance.DefaultTransponderSymbolRate.ToString();
        tbNetworkId.Text = SkyUK.Instance.NetworkId.ToString("X");

        cbPolarisation.SelectedItem = SkyUK.Instance.DefaultTransponderPolarisationString;
        cbFEC.SelectedItem = SkyUK.Instance.DefaultTransponderInnerFECString;
      }

      if (_skyCountry == eSkyCounntry.Italy)
      {
        tbFrequency.Text = SkyItaly.Instance.DefaultTransponderFrequency.ToString();
        tbSymbolRate.Text = SkyItaly.Instance.DefaultTransponderSymbolRate.ToString();
        tbNetworkId.Text = SkyItaly.Instance.NetworkId.ToString("X");

        cbPolarisation.SelectedItem = SkyItaly.Instance.DefaultTransponderPolarisationString;
        cbFEC.SelectedItem = SkyItaly.Instance.DefaultTransponderInnerFECString;
      }
    }
    /// <summary>
    /// Fired when the save button is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btSave_Click(object sender, EventArgs e)
    {
      string frequencyString = tbFrequency.Text;
      int frequency = 0;

      if (!int.TryParse(frequencyString, out frequency))
      {
        MessageBox.Show("Please enter a valid number for frequency", "Validation");
        return;
      }

      string symbolRateString = tbSymbolRate.Text;
      int symbolRate = 0;

      if (!int.TryParse(symbolRateString, out symbolRate))
      {
        MessageBox.Show("Please enter a valid number for symbol rate", "Validation");
        return;
      }

      string networkIdString = tbNetworkId.Text;
      int networkId = 0;

      if (!int.TryParse(networkIdString, System.Globalization.NumberStyles.HexNumber, null, out networkId))
      {
        MessageBox.Show("Please enter a valid number for network id", "Validation");
        return;
      }

      if (_skyCountry == eSkyCounntry.UK)
      {
        SkyUK.Instance.DefaultTransponderFrequency = frequency;
        SkyUK.Instance.DefaultTransponderPolarisationString = cbPolarisation.SelectedItem as string;
        SkyUK.Instance.DefaultTransponderSymbolRate = symbolRate;
        SkyUK.Instance.DefaultTransponderInnerFECString = cbFEC.SelectedItem as string;
        SkyUK.Instance.NetworkId = networkId;
      }

      if (_skyCountry == eSkyCounntry.Italy)
      {
        SkyItaly.Instance.DefaultTransponderFrequency = frequency;
        SkyItaly.Instance.DefaultTransponderPolarisationString = cbPolarisation.SelectedItem as string;
        SkyItaly.Instance.DefaultTransponderSymbolRate = symbolRate;
        SkyItaly.Instance.DefaultTransponderInnerFECString = cbFEC.SelectedItem as string;
        SkyItaly.Instance.NetworkId = networkId;
      }

      DialogResult = DialogResult.OK;

      Close();
    }

    /// <summary>
    /// Fired when the cancel button is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;

      Close();
    }
  }
}
