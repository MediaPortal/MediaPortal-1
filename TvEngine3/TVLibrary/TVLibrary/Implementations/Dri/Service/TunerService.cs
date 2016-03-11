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
using System.Collections.Generic;
using System.Linq;
using UPnP.Infrastructure.CP.DeviceTree;

namespace TvLibrary.Implementations.Dri.Service
{
  public sealed class DriTunerModulation
  {
    private readonly string _name;
    private static readonly IDictionary<string, DriTunerModulation> _values = new Dictionary<string, DriTunerModulation>();

    public static readonly DriTunerModulation Qam64 = new DriTunerModulation("QAM64");
    public static readonly DriTunerModulation Qam64_2 = new DriTunerModulation("QAM-64");
    public static readonly DriTunerModulation Qam256 = new DriTunerModulation("QAM256");
    public static readonly DriTunerModulation Qam256_2 = new DriTunerModulation("QAM-256");
    public static readonly DriTunerModulation Ntsc = new DriTunerModulation("NTSC");
    public static readonly DriTunerModulation NtscM = new DriTunerModulation("NTSC-M");
    public static readonly DriTunerModulation Vsb8 = new DriTunerModulation("8VSB");
    public static readonly DriTunerModulation Vsb8_2 = new DriTunerModulation("8-VSB");
    public static readonly DriTunerModulation All = new DriTunerModulation("ALL");

    private DriTunerModulation(string name)
    {
      _name = name;
      _values.Add(name, this);
    }

    public override string ToString()
    {
      return _name;
    }

    public override bool Equals(object obj)
    {
      DriTunerModulation mod = obj as DriTunerModulation;
      if (mod != null && this == mod)
      {
        return true;
      }
      return false;
    }

    public static ICollection<DriTunerModulation> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator DriTunerModulation(string name)
    {
      DriTunerModulation value = null;
      if (name == null || !_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(DriTunerModulation mod)
    {
      return mod._name;
    }
  }

  public class TunerService : BaseService
  {
    private CpAction _setTunerParametersAction = null;
    private CpAction _getTunerParametersAction = null;
    private CpAction _seekSignalAction = null;
    private CpAction _seekCancelAction = null;

    public TunerService(CpDevice device)
      : base(device, "urn:opencable-com:serviceId:urn:schemas-opencable-com:service:Tuner")
    {
      _service.Actions.TryGetValue("SetTunerParameters", out _setTunerParametersAction);
      _service.Actions.TryGetValue("GetTunerParameters", out _getTunerParametersAction);
      _service.Actions.TryGetValue("SeekSignal", out _seekSignalAction);
      _service.Actions.TryGetValue("SeekCancel", out _seekCancelAction);
    }

    /// <summary>
    /// The SetTunerParameters action SHALL return tuning status in less than 2s per modulations attempt.
    /// </summary>
    /// <param name="newFrequency">This argument set the Frequency state variable to the selected frequency. The unit is kHz.</param>
    /// <param name="newModulationList">This argument sets the Modulation state variable to the last in a list of selected
    ///   modulation types if no demodulation was achieved or to the modulation as reported by demodulator in case of
    ///   successful demodulation.</param>
    /// <param name="currentFrequency">This argument provides the value in Frequency state variable when the action response is created. The unit is kHz.</param>
    /// <param name="currentModulation">This argument provides the value of the Modulation state variable when the action response is created.</param>
    /// <param name="pcrLockStatus">This argument provides the value of the PCRLock state variable when the action response is created.</param>
    public void SetTunerParameters(UInt32 newFrequency, IList<DriTunerModulation> newModulationList,
                            out UInt32 currentFrequency, out DriTunerModulation currentModulation, out bool pcrLockStatus)
    {
      string newModulationListCsv = DriTunerModulation.All;
      if (newModulationList != null)
      {
        newModulationListCsv = string.Join(",", newModulationList.Select(x => x.ToString()).ToArray());
      }
      IList<object> outParams = _setTunerParametersAction.InvokeAction(new List<object> { newFrequency, newModulationListCsv });
      currentFrequency = (uint)outParams[0];
      currentModulation = (DriTunerModulation)(string)outParams[1];
      pcrLockStatus = (bool)outParams[2];
    }

    /// <summary>
    /// The GetTunerParameters action SHALL return tuning status in less than 1s.
    /// </summary>
    /// <param name="currentCarrierLock">This argument provides the value of the CarrierLock state variable when the action response is created.</param>
    /// <param name="currentFrequency">This argument provides the value of the Frequency state variable when the action response is created. The unit is kHz.</param>
    /// <param name="currentModulation">This argument provides the value of the Modulation state variable when the action response is created.</param>
    /// <param name="currentPcrLock">This argument provides the value of the PCRLock state variable when the action response is created.</param>
    /// <param name="currentSignalLevel">This argument provides the value of the SignalLevel state variable when the action response is created. The unit is dBmV.</param>
    /// <param name="currentSnr">This argument provides the value of the SNR state variable when the action response is created. The unit is dB.</param>
    public void GetTunerParameters(out bool currentCarrierLock, out UInt32 currentFrequency,
                                  out DriTunerModulation currentModulation, out bool currentPcrLock,
                                  out Int32 currentSignalLevel, out UInt32 currentSnr)
    {
      IList<object> outParams = _getTunerParametersAction.InvokeAction(null);
      currentCarrierLock = (bool)outParams[0];
      currentFrequency = (uint)outParams[1];
      currentModulation = (DriTunerModulation)(string)outParams[2];
      currentPcrLock = (bool)outParams[3];
      currentSignalLevel = (int)outParams[4];
      currentSnr = (uint)outParams[5];
    }

    /// <summary>
    /// Upon receipt of the SeekSignal action, the DRIT SHALL performa signal search according to the input arguments
    /// within the limit of the timeout period.
    /// </summary>
    /// <param name="startFrequency">This argument sets the Frequency state variable.</param>
    /// <param name="stopFrequency">This argument sets the A_ARG_TYPE_StopFrequency state variable.</param>
    /// <param name="newModulationList">This argument sets the A_ARG_TYPE_TuneModulationList state variable.</param>
    /// <param name="increment">This argument sets the A_ARG_TYPE_Increment state variable.</param>
    /// <param name="seekUp">This argument sets the A_ARG_TYPE_SeekUp state variable.</param>
    /// <param name="timeToBlock">This argument sets the A_ARGTYPE_TimeToBlock state variable.</param>
    /// <returns><c>true</c> if the action is executed, otherwise <c>false</c></returns>
    public bool SeekSignal(UInt32 startFrequency, UInt32 stopFrequency, List<DriTunerModulation> newModulationList,
                            UInt32 increment, bool seekUp, UInt16 timeToBlock)
    {
      if (_seekSignalAction == null)
      {
        Log.Log.Debug("DRI: device {0} does not implement a Tuner SeekSignal action", _device.UDN);
        return false;
      }

      string newModulationListCsv = DriTunerModulation.All;
      if (newModulationList != null)
      {
        newModulationListCsv = string.Join(",", newModulationList.Select(x => x.ToString()).ToArray());
      }
      _seekSignalAction.InvokeAction(new List<object> { startFrequency, stopFrequency, newModulationListCsv, increment, seekUp, timeToBlock });
      return true;
    }

    /// <summary>
    /// Upon receipt of the SeekCancel action, the DRIT SHALL cancel any SeekSignal action in less than 1s.
    /// </summary>
    /// <returns><c>true</c> if the action is executed, otherwise <c>false</c></returns>
    public bool SeekCancel()
    {
      if (_seekCancelAction == null)
      {
        Log.Log.Debug("DRI: device {0} does not implement a Tuner SeekCancel action", _device.UDN);
        return false;
      }
      _seekCancelAction.InvokeAction(null);
      return true;
    }
  }
}
