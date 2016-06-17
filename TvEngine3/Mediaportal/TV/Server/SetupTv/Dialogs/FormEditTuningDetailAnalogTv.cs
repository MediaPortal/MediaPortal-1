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
using Mediaportal.TV.Server.Common.Types.Country;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEditTuningDetailAnalogTv : FormEditTuningDetailCommon
  {
    public FormEditTuningDetailAnalogTv()
    {
      InitializeComponent();
    }

    protected override void LoadProperties(TuningDetail tuningDetail)
    {
      comboBoxCountry.Items.Clear();
      comboBoxCountry.Items.AddRange(CountryCollection.Instance.Countries);

      comboBoxSource.Items.Clear();
      comboBoxSource.Items.AddRange(typeof(AnalogTunerSource).GetDescriptions());

      if (tuningDetail != null)
      {
        Text = "Edit Analog TV Tuning Detail";
        numericTextBoxPhysicalChannelNumber.Value = tuningDetail.Frequency;
        numericTextBoxFrequency.Value = tuningDetail.Frequency;
        foreach (Country country in CountryCollection.Instance.Countries)
        {
          if (country.Id == tuningDetail.CountryId)
          {
            comboBoxCountry.SelectedItem = country;
            break;
          }
        }
        comboBoxSource.SelectedItem = ((AnalogTunerSource)tuningDetail.TuningSource).GetDescription();
      }
      else
      {
        Text = "Add Analog TV Tuning Detail";
        numericTextBoxPhysicalChannelNumber.Value = 1;
        numericTextBoxFrequency.Value = 0;
        Country country = CountryCollection.Instance.GetCountryByName(System.Globalization.RegionInfo.CurrentRegion.EnglishName);
        if (country == null)
        {
          comboBoxCountry.SelectedIndex = 0;
        }
        else
        {
          comboBoxCountry.SelectedItem = country;
        }
        comboBoxSource.SelectedItem = AnalogTunerSource.Cable.GetDescription();
      }
    }

    protected override void UpdateProperties(TuningDetail tuningDetail)
    {
      tuningDetail.BroadcastStandard = (int)BroadcastStandard.AnalogTelevision;
      tuningDetail.PhysicalChannelNumber = numericTextBoxPhysicalChannelNumber.Value;
      tuningDetail.Frequency = numericTextBoxFrequency.Value;
      tuningDetail.CountryId = ((Country)comboBoxCountry.SelectedItem).Id;
      tuningDetail.TuningSource = Convert.ToInt32(typeof(AnalogTunerSource).GetEnumFromDescription((string)comboBoxSource.SelectedItem));
      tuningDetail.GrabEpg = false;
    }
  }
}