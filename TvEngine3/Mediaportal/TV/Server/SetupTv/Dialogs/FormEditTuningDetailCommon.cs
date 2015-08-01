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
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEditTuningDetailCommon : Form
  {
    private TuningDetail _tuningDetail;

    protected FormEditTuningDetailCommon()
    {
      InitializeComponent();
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TuningDetail TuningDetail
    {
      get
      {
        return _tuningDetail;
      }
      set
      {
        _tuningDetail = value;
      }
    }

    private void FormEditTuningDetailCommon_Load(object sender, EventArgs e)
    {
      if (_tuningDetail != null)
      {
        this.LogInfo("tuning detail: start edit, ID = {0}", _tuningDetail.IdTuning);
        if (_tuningDetail.IdTuning > 0)
        {
          _tuningDetail = ServiceAgents.Instance.ChannelServiceAgent.GetTuningDetail(_tuningDetail.IdTuning);
        }
        textBoxName.Text = _tuningDetail.Name;
        textBoxNumber.Text = _tuningDetail.LogicalChannelNumber;
        textBoxProvider.Text = _tuningDetail.Provider;
        checkBoxIsEncrypted.Checked = _tuningDetail.IsEncrypted;
        checkBoxIsHighDefinition.Checked = _tuningDetail.IsHighDefinition;
        checkBoxIsThreeDimensional.Checked = _tuningDetail.IsThreeDimensional;
      }
      else
      {
        // Comment this line to enable the control to be viewed in the designer.
        //this.LogInfo("tuning detail: create new");
        textBoxName.Text = string.Empty;
        textBoxNumber.Text = string.Empty;
        textBoxProvider.Text = string.Empty;
        checkBoxIsEncrypted.Checked = false;
        checkBoxIsHighDefinition.Checked = false;
        checkBoxIsThreeDimensional.Checked = false;
      }

      LoadProperties(_tuningDetail);
    }

    private void buttonOkay_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrWhiteSpace(textBoxName.Text))
      {
        MessageBox.Show("Please enter a name.", SectionSettings.MESSAGE_CAPTION);
        return;
      }

      int intChannelNumber;
      float floatChannelNumber;
      if (
        string.IsNullOrWhiteSpace(textBoxNumber.Text) ||
        (
          !int.TryParse(textBoxNumber.Text, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out intChannelNumber) &&
          !float.TryParse(textBoxNumber.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out floatChannelNumber)
        )
      )
      {
        MessageBox.Show("Please enter a number in the form ### or #.#. For example, 123 or 1.10.", SectionSettings.MESSAGE_CAPTION);
        return;
      }

      if (!CheckPropertyValues())
      {
        return;
      }

      if (_tuningDetail == null)
      {
        this.LogInfo("tuning detail: save new");
        _tuningDetail = new TuningDetail
        {
          BroadcastStandard = 0,
          Name = textBoxName.Text,
          Provider = textBoxProvider.Text,
          LogicalChannelNumber = textBoxNumber.Text,
          IsEncrypted = checkBoxIsEncrypted.Checked,
          IsHighDefinition = checkBoxIsHighDefinition.Checked,
          IsThreeDimensional = checkBoxIsThreeDimensional.Checked,
          OriginalNetworkId = -1,
          TransportStreamId = -1,
          ServiceId = -1,
          FreesatChannelId = -1,
          OpenTvChannelId = -1,
          EpgOriginalNetworkId = -1,
          EpgTransportStreamId = -1,
          EpgServiceId = -1,
          SourceId = -1,
          PmtPid = -1,
          PhysicalChannelNumber = -1,
          Frequency = -1,
          CountryId = -1,
          Modulation = -1,
          Polarisation = -1,
          SymbolRate = -1,
          DiSEqC = -1,
          Bandwidth = -1,
          VideoSource = -1,
          AudioSource = -1,
          TuningSource = -1,
          SatIndex = -1,
          FecCodeRate = -1,
          PilotTonesState = -1,
          RollOffFactor = -1,
          StreamId = -1,
          Url = string.Empty,
          IsVcrSignal = false,
          IdLnbType = null
        };
      }
      else
      {
        this.LogInfo("tuning detail: save changes, ID = {0}", _tuningDetail.IdTuning);
      }
      UpdateProperties(_tuningDetail);
      if (_tuningDetail.IdTuning > 0)
      {
        ServiceAgents.Instance.ChannelServiceAgent.SaveTuningDetail(_tuningDetail);
      }

      DialogResult = DialogResult.OK;
      Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      if (_tuningDetail == null)
      {
        this.LogInfo("tuning detail: cancel new");
      }
      else
      {
        this.LogInfo("tuning detail: cancel changes, ID = {0}", _tuningDetail.IdTuning);
      }
      DialogResult = DialogResult.Cancel;
      Close();
    }

    protected virtual void LoadProperties(TuningDetail tuningDetail)
    {
      // Comment this line to enable the control to be viewed in the designer.
      //throw new NotImplementedException();
    }

    protected virtual bool CheckPropertyValues()
    {
      return true;
    }

    protected virtual void UpdateProperties(TuningDetail tuningDetail)
    {
      // Comment this line to enable the control to be viewed in the designer.
      //throw new NotImplementedException();
    }
  }
}