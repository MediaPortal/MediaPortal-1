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
using System.Globalization;
using System.Text;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster
{
  internal class IrCommand
  {
    #region constants

    public const int CARRIER_FREQUENCY_DC_MODE = 0;
    public const int CARRIER_FREQUENCY_DEFAULT = 36000;   // 36 kHz = RC-5 and RC-6 carrier frequency
    public const int CARRIER_FREQUENCY_UNKNOWN = -1;

    #endregion

    #region variables

    private readonly int _carrierFrequency = CARRIER_FREQUENCY_UNKNOWN;
    private readonly int[] _timingData = null;

    #endregion

    public IrCommand(int carrierFrequency, int[] timingData)
    {
      if (timingData == null || timingData.Length == 0)
      {
        throw new TvException("IR command timing data must always be supplied.");
      }
      _carrierFrequency = carrierFrequency;
      _timingData = timingData;
    }

    #region properties

    /// <summary>
    /// Get the command's carrier frequency. The unit is Hertz (Hz).
    /// </summary>
    public int CarrierFrequency
    {
      get
      {
        return _carrierFrequency;
      }
    }

    /// <summary>
    /// Get the command's timing data.
    /// </summary>
    public int[] TimingData
    {
      get
      {
        return _timingData;
      }
    }

    #endregion

    #region string conversion

    public string ToProntoString()
    {
      StringBuilder output = new StringBuilder();

      ushort[] prontoData = Pronto.ConvertIrCommandToProntoRaw(this);
      for (int index = 0; index < prontoData.Length; index++)
      {
        if (index != 0)
        {
          output.Append(' ');
        }
        output.Append(prontoData[index].ToString("X4"));
      }
      return output.ToString();
    }

    public static IrCommand FromProntoString(string command)
    {
      try
      {
        if (string.IsNullOrEmpty(command))
        {
          return null;
        }

        string[] temp = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        ushort[] prontoData = new ushort[temp.Length];
        for (int i = 0; i < temp.Length; i++)
        {
          prontoData[i] = ushort.Parse(temp[i], NumberStyles.HexNumber);
        }

        return Pronto.ConvertProntoDataToIrCommand(prontoData);
      }
      catch (Exception ex)
      {
        Log.Error(ex, "Microsoft blaster: failed to parse command string");
        return null;
      }
    }

    #endregion
  }
}