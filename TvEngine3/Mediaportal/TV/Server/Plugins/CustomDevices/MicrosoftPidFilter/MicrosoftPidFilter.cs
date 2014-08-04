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

using System.Collections.Generic;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftPidFilter
{
  /// <summary>
  /// This class provides a base implementation of PID filtering for tuners that support Microsoft
  /// BDA interfaces and de-facto standards.
  /// </summary>
  public class MicrosoftPidFilter : BaseCustomDevice, IMpeg2PidFilter
  {
    #region variables

    private bool _isMicrosoftPidFilter = false;
    private IMPEG2PIDMap _interface = null;
    private HashSet<ushort> _pidsToRemove = new HashSet<ushort>();
    private HashSet<ushort> _pidsToAdd = new HashSet<ushort>();

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        // This implementation should only be used when more specialised interfaces are not available.
        return 1;
      }
    }

    /// <summary>
    /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
    /// even a model name and/or number.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Microsoft PID filter";
      }
    }

    /// <summary>
    /// Attempt to initialise the extension-specific interfaces used by the class. If
    /// initialisation fails, the <see ref="ICustomDevice"/> instance should be disposed
    /// immediately.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, CardType tunerType, object context)
    {
      this.LogDebug("Microsoft PID filter: initialising");

      if (_isMicrosoftPidFilter)
      {
        this.LogWarn("Microsoft PID filter: extension already initialised");
        return true;
      }

      IMPEG2PIDMap pidFilterInterface = context as IMPEG2PIDMap;
      if (pidFilterInterface == null)
      {
        this.LogDebug("Microsoft PID filter: interface not supported");
        return false;
      }

      this.LogInfo("Microsoft PID filter: extension supported");
      return true;
    }

    #endregion

    #region IMpeg2PidFilter member

    /// <summary>
    /// Should the filter be enabled for the current multiplex.
    /// </summary>
    /// <param name="tuningDetail">The current multiplex/transponder tuning parameters.</param>
    /// <returns><c>true</c> if the filter should be enabled, otherwise <c>false</c></returns>
    public bool ShouldEnableFilter(IChannel tuningDetail)
    {
      // If a tuner supports a PID filter then assume it is desirable to enable it.
      return true;
    }

    /// <summary>
    /// Disable the filter.
    /// </summary>
    /// <returns><c>true</c> if the filter is successfully disabled, otherwise <c>false</c></returns>
    public bool DisableFilter()
    {
      // Nothing to do here, based on two assumptions:
      // 1. Driver resets (ie. disables) the PID filter automatically on retune.
      // 2. Because the PID limit is not known (ie. the caller won't call this function on reaching
      //    a PID count threshold) and ShouldEnableFilter() returns true (ie. the filter would
      //    normally be enabled), this function would only be called when the user expressly
      //    disables the PID filter... which would be automatically handled by (1).
      // Note: according to the comments, IMPEG2PIDMap.EnumPIDMap() is not available. That means we
      // can't actually test that the driver disables the PID filter on retune.
      return true;
    }

    /// <summary>
    /// Get the maximum number of streams that the filter can allow.
    /// </summary>
    public int MaximumPidCount
    {
      get
      {
        return -1;  // maximum not known
      }
    }

    /// <summary>
    /// Configure the filter to allow one or more streams to pass through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    /// <returns><c>true</c> if the filter is successfully configured, otherwise <c>false</c></returns>
    public bool AllowStreams(ICollection<ushort> pids)
    {
      _pidsToAdd.UnionWith(pids);
      _pidsToRemove.ExceptWith(pids);
      return true;
    }

    /// <summary>
    /// Configure the filter to stop one or more streams from passing through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    /// <returns><c>true</c> if the filter is successfully configured, otherwise <c>false</c></returns>
    public bool BlockStreams(ICollection<ushort> pids)
    {
      _pidsToAdd.ExceptWith(pids);
      _pidsToRemove.UnionWith(pids);
      return true;
    }

    /// <summary>
    /// Apply the current filter configuration.
    /// </summary>
    /// <returns><c>true</c> if the filter configuration is successfully applied, otherwise <c>false</c></returns>
    public bool ApplyFilter()
    {
      if (_pidsToAdd.Count == 0 && _pidsToRemove.Count == 0)
      {
        return true;
      }

      this.LogDebug("Microsoft PID filter: apply PID filter");

      if (_pidsToRemove.Count > 0)
      {
        this.LogDebug("  unmap {0} current PID(s)", _pidsToRemove.Count);
        int[] pidArray = new int[_pidsToRemove.Count];
        int i = 0;
        foreach (ushort pid in _pidsToRemove)
        {
          pidArray[i++] = pid;
        }
        int hr = _interface.UnmapPID(pidArray.Length, pidArray);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("Microsoft PID filter: failed to unmap current PID(s), hr = 0x{0:x}", hr);
          return false;
        }
        _pidsToRemove.Clear();
      }
      if (_pidsToAdd.Count > 0)
      {
        this.LogDebug("  map {0} new PID(s)", _pidsToAdd.Count);
        int[] pidArray = new int[_pidsToAdd.Count];
        int i = 0;
        foreach (ushort pid in _pidsToAdd)
        {
          pidArray[i++] = pid;
        }
        int hr = _interface.MapPID(pidArray.Length, pidArray, MediaSampleContent.ElementaryStream);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("Microsoft PID filter: failed to map new PID(s), hr = 0x{0:x}", hr);
          return false;
        }
        _pidsToAdd.Clear();
      }

      this.LogDebug("Microsoft PID filter: result = success");
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      _interface = null;
      _isMicrosoftPidFilter = false;
    }

    #endregion
  }
}