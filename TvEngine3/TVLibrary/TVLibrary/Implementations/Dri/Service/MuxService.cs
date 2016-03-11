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
  public class MuxService : BaseService
  {
    private CpAction _setProgramAction = null;
    private CpAction _addPidAction = null;
    private CpAction _removePidAction = null;

    public MuxService(CpDevice device)
      : base(device, "urn:opencable-com:serviceId:urn:schemas-opencable-com:service:Mux")
    {
      _service.Actions.TryGetValue("SetProgram", out _setProgramAction);
      _service.Actions.TryGetValue("AddPid", out _addPidAction);
      _service.Actions.TryGetValue("RemovePid", out _removePidAction);
    }

    /// <summary>
    /// Upon receipt of the SetProgramaction, the DRIT SHALL perform in sequence the following actions in less than 1s,
    /// only if a Card is not inserted.
    /// 1.  Clear all the previous program PID from the PidList state variable.
    /// 2.  Send a ca_pmt() APDU to the Card.
    /// 3.  Add all PID referenced in the new Program Map Table (PMT) and the PMT PID into the PidList state
    ///     variable.
    /// </summary>
    /// <param name="newProgram">This argument sets the ProgramNumber state variable.</param>
    public void SetProgram(UInt16 newProgram)
    {
      _setProgramAction.InvokeAction(new List<object> { newProgram });
    }

    /// <summary>
    /// Upon receipt of the AddPid action, the DRIT SHALL add all new PID values to the state variable in less than 1s.
    /// </summary>
    /// <param name="addPidList">This argument provides a list of PID values that need to be added to the PidList state variable.</param>
    public void AddPid(IList<UInt16> addPidList)
    {
      string addPidListHexCsv = string.Empty;
      if (addPidList != null)
      {
        addPidListHexCsv = string.Join(",", addPidList.Select(x => string.Format("{0:X}", x)).ToArray());
      }
      _addPidAction.InvokeAction(new List<object> { addPidListHexCsv });
    }

    /// <summary>
    /// Upon receipt of the RemovePid action, the DRIT SHALL remove all old PID values from the state variable in less
    /// than 1s.
    /// </summary>
    /// <param name="removePidList">This argument provides a list of PID values that need to be removed from the PidList state variable.</param>
    public void RemovePid(IList<UInt16> removePidList)
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
