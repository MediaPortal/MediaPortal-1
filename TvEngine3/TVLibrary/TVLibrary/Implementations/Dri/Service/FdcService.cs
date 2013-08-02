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
  public class FdcService : BaseService
  {
    private CpAction _getFdcStatusAction = null;
    private CpAction _requestTablesAction = null;
    private CpAction _addPidAction = null;
    private CpAction _removePidAction = null;

    public FdcService(CpDevice device)
      : base(device, "urn:opencable-com:serviceId:urn:schemas-opencable-com:service:FDC")
    {
      _service.Actions.TryGetValue("GetFDCStatus", out _getFdcStatusAction);
      _service.Actions.TryGetValue("RequestTables", out _requestTablesAction);
      _service.Actions.TryGetValue("AddPid", out _addPidAction);
      _service.Actions.TryGetValue("RemovePid", out _removePidAction);
    }

    /// <summary>
    /// Upon receipt of the GetFDCStatus action, the DRIT SHALL return tuning status in less than 1s.
    /// </summary>
    /// <param name="currentBitrate">This argument provides the value of the Bitrate state variable when the action response is created. The unit is kbps.</param>
    /// <param name="currentCarrierLock">This argument provides the value of the CarrierLock state variable when the action response is created.</param>
    /// <param name="currentFrequency">This argument provides the value of the Frequency state variable when the action response is created. The unit is kHz.</param>
    /// <param name="currentSpectrumInversion">This argument provides the value of the SpectrumInversion state variable when the action response is created.</param>
    /// <param name="currentPidList">This argument provides the value of the PidList state variable when the action response is created.</param>
    public void GetFdcStatus(out UInt32 currentBitrate, out bool currentCarrierLock, out UInt32 currentFrequency,
                            out bool currentSpectrumInversion, out IList<UInt16> currentPidList)
    {
      IList<object> outParams = _getFdcStatusAction.InvokeAction(null);
      currentBitrate = (uint)outParams[0];
      currentCarrierLock = (bool)outParams[1];
      currentFrequency = (uint)outParams[2];
      currentSpectrumInversion = (bool)outParams[3];
      currentPidList = outParams[4].ToString().Split(',').Select(x => Convert.ToUInt16(x, 16)).ToList<UInt16>();
    }

    /// <summary>
    /// Upon receipt of the RequestTables action, the DRIT SHALL event each cached table section filtered by the
    /// A_ARG_TYPE_TID state variables using the TableSection state variable in less than 1s per table.
    /// </summary>
    /// <param name="tid">This argument sets the A_ARG_TYPE_TID state variable to the selected TID values.</param>
    public void RequestTables(IList<byte> tid)
    {
      string tidHexCsv = "ALL"; // = no filtering
      if (tid != null)
      {
        tidHexCsv = string.Join(",", tid.Select(x => string.Format("{0:X}", x)).ToArray());
      }
      _requestTablesAction.InvokeAction(new List<object> { tidHexCsv });
    }

    /// <summary>
    /// Upon receipt of the AddPid action, the DRIT SHALL send a new_flow_request() APDU with an MPEG flow type
    /// to the Card for each additional PID value in less than 1s.
    /// </summary>
    /// <param name="addPidList">This argument provides a list of PID value that need to be added to the PidList state variable.</param>
    /// <param name="remainingPidFilter">This argument provides the value of the CCFreePidFilter when the action response is created.</param>
    public void AddPid(IList<UInt16> addPidList, out byte remainingPidFilter)
    {
      string addPidListHexCsv = string.Empty;
      if (addPidList != null)
      {
        addPidListHexCsv = string.Join(",", addPidList.Select(x => string.Format("{0:X}", x)).ToArray());
      }
      IList<object> outParams = _addPidAction.InvokeAction(new List<object> { addPidListHexCsv });
      remainingPidFilter = (byte)outParams[0];
    }

    /// <summary>
    /// Upon receipt of the RemovePid action,the DRIT SHALL send a delete_flow_request() APDU to the Card for each
    /// PID value in less than 1s.
    /// </summary>
    /// <param name="removePidList">This argument provides a list of PID value that need to be removed from the PidList state variable.</param>
    public void RemovidPid(IList<UInt16> removePidList)
    {
      string removePidListHexCsv = string.Empty;
      if (removePidList != null)
      {
        removePidListHexCsv = string.Join(",", removePidList.Select(x => string.Format("{0:X}", x)).ToArray());
      }
      _removePidAction.InvokeAction(new List<object> { removePidListHexCsv });
    }
  }
}
