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
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Channel;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
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
        this.LogInfo("tuning detail: start edit, ID = {0}", _tuningDetail.IdTuningDetail);
        if (_tuningDetail.IdTuningDetail > 0)
        {
          _tuningDetail = ServiceAgents.Instance.ChannelServiceAgent.GetTuningDetail(_tuningDetail.IdTuningDetail, TuningDetailRelation.None);
        }
        textBoxName.Text = _tuningDetail.Name;
        channelNumberUpDownNumber.Text = _tuningDetail.LogicalChannelNumber;
        textBoxProvider.Text = _tuningDetail.Provider;
        checkBoxIsEncrypted.Checked = _tuningDetail.IsEncrypted;
        checkBoxIsHighDefinition.Checked = _tuningDetail.IsHighDefinition;
        checkBoxIsThreeDimensional.Checked = _tuningDetail.IsThreeDimensional;
      }
      else
      {
        if (!DesignMode)
        {
          this.LogInfo("tuning detail: create new");
        }
        textBoxName.Text = string.Empty;
        channelNumberUpDownNumber.Text = LogicalChannelNumber.GLOBAL_DEFAULT;
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

      string logicalChannelNumber;
      if (!LogicalChannelNumber.Create(channelNumberUpDownNumber.Text, out logicalChannelNumber))
      {
        MessageBox.Show("Please enter a channel number in the form ### or #.#. For example, 123 or 1.23.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK);
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
          Name = textBoxName.Text,
          Provider = textBoxProvider.Text,
          LogicalChannelNumber = logicalChannelNumber,
          IsEncrypted = checkBoxIsEncrypted.Checked,
          IsHighDefinition = checkBoxIsHighDefinition.Checked,
          IsThreeDimensional = checkBoxIsThreeDimensional.Checked
        };
      }
      UpdateProperties(_tuningDetail);
      if (_tuningDetail.IdTuningDetail > 0)
      {
        this.LogInfo("tuning detail: save changes, ID = {0}", _tuningDetail.IdTuningDetail);
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
        this.LogInfo("tuning detail: cancel changes, ID = {0}", _tuningDetail.IdTuningDetail);
      }
      DialogResult = DialogResult.Cancel;
      Close();
    }

    protected virtual void LoadProperties(TuningDetail tuningDetail)
    {
      if (!DesignMode)
      {
        throw new NotImplementedException();
      }
    }

    protected virtual bool CheckPropertyValues()
    {
      return true;
    }

    protected virtual void UpdateProperties(TuningDetail tuningDetail)
    {
      if (!DesignMode)
      {
        throw new NotImplementedException();
      }
    }
  }
}