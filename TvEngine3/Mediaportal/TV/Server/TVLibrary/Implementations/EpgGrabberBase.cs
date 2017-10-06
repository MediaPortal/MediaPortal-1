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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// A base class for all <see cref="IEpgGrabber"/> implementations,
  /// independent of tuner type and stream format.
  /// </summary>
  internal abstract class EpgGrabberBase : IEpgGrabberInternal
  {
    #region variables

    /// <summary>
    /// The controller for the tuner's electronic programme guide data grabber.
    /// </summary>
    private IEpgGrabberController _controller = null;

    /// <summary>
    /// A delegate to notify about grabber events.
    /// </summary>
    protected IEpgGrabberCallBack _callBack = null;

    /// <summary>
    /// Indicator: is electronic programme guide data grabbing enabled?
    /// </summary>
    private bool _isGrabbingEnabled = false;

    /// <summary>
    /// Indicator: is the tuner allowed to grab electronic programme guide data?
    /// </summary>
    private bool _isTunerAllowedToGrab = false;

    /// <summary>
    /// The set of electronic programme guide data protocols that the grabber
    /// is configured to grab.
    /// </summary>
    protected TunerEpgGrabberProtocol _protocols = TunerEpgGrabberProtocol.None;

    /// <summary>
    /// The current tuning details.
    /// </summary>
    protected IChannel _tuningDetail = null;

    /// <summary>
    /// Indicator: is the grabber grabbing electronic programme guide data?
    /// </summary>
    private bool _isGrabbing = false;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="EpgGrabberBase"/> class.
    /// </summary>
    /// <param name="controller">The controller for a tuner's EPG grabber.</param>
    public EpgGrabberBase(IEpgGrabberController controller)
    {
      _controller = controller;
    }

    private bool IsGrabbingAllowed
    {
      get
      {
        if (
          _isGrabbingEnabled &&
          _isTunerAllowedToGrab &&
          _tuningDetail != null && _tuningDetail.GrabEpg &&
          _protocols != TunerEpgGrabberProtocol.None
        )
        {
          return true;
        }
        return false;
      }
    }

    private void Start(TunerEpgGrabberProtocol? newProtocols = null)
    {
      if (!newProtocols.HasValue)
      {
        this.LogDebug("EPG base: start, protocols = [{0}]", _protocols);
      }
      else
      {
        this.LogDebug("EPG base: start");
        this.LogDebug("    current = [{0}]", _protocols);
        this.LogDebug("    new     = [{0}]", newProtocols.Value);
      }

      if (!_controller.IsEnabled())
      {
        _controller.Enable();
      }

      OnStart(newProtocols);

      if (_callBack != null)
      {
        _callBack.OnGrabbingStarted();
      }

      _isGrabbing = true;
    }

    protected abstract void OnStart(TunerEpgGrabberProtocol? newProtocols = null);

    private void Stop()
    {
      this.LogDebug("EPG base: stop");
      this.LogDebug("  enabled?          = {0}", _isGrabbingEnabled);
      this.LogDebug("  tuned?            = {0}", _tuningDetail != null);
      this.LogDebug("  grab transmitter? = {0}", _tuningDetail != null && _tuningDetail.GrabEpg);
      this.LogDebug("  grab tuner?       = {0}", _isTunerAllowedToGrab);
      this.LogDebug("  protocols         = [{0}]", _protocols);

      if (_controller.IsEnabled())
      {
        _controller.Disable();
      }

      OnStop();

      if (_callBack != null)
      {
        _callBack.OnGrabbingStopped();
      }

      _isGrabbing = false;
    }

    protected abstract void OnStop();

    protected static string TidyString(string s)
    {
      if (s == null)
      {
        return string.Empty;
      }
      return s.Trim();
    }

    #region IEpgGrabber members

    /// <summary>
    /// Set the grabber's call-back.
    /// </summary>
    /// <param name="callBack">The delegate to notify about grabber events.</param>
    public void SetCallBack(IEpgGrabberCallBack callBack)
    {
      _callBack = callBack;
    }

    /// <summary>
    /// Enable or disable grabbing.
    /// </summary>
    /// <value><c>true</c> if grabbing is enabled, otherwise <c>false</c></value>
    public bool IsEnabled
    {
      get
      {
        return _isGrabbingEnabled;
      }
      set
      {
        bool wasGrabbingAllowed = IsGrabbingAllowed;
        _isGrabbingEnabled = value;
        if (wasGrabbingAllowed != IsGrabbingAllowed)
        {
          if (wasGrabbingAllowed)
          {
            Stop();
            return;
          }
          Start();
        }
      }
    }

    /// <summary>
    /// Get/set the EPG data protocols supported by the tuner hardware and/or enabled for use.
    /// </summary>
    public TunerEpgGrabberProtocol SupportedProtocols
    {
      get
      {
        return _protocols;
      }
      set
      {
        bool wasGrabbingAllowed = IsGrabbingAllowed;
        TunerEpgGrabberProtocol formerProtocols = _protocols;
        TunerEpgGrabberProtocol newProtocols = value & PossibleProtocols;

        _protocols = newProtocols;  // Important for IsGrabbingAllowed.
        bool isGrabbingAllowed = IsGrabbingAllowed;
        if (
          wasGrabbingAllowed != isGrabbingAllowed ||
          formerProtocols != _protocols
        )
        {
          // Both Start() and Stop() need _protocols to have its former value.
          _protocols = formerProtocols;
          if (!isGrabbingAllowed)
          {
            Stop();
          }
          else
          {
            Start(value);
          }
          _protocols = newProtocols;
        }
      }
    }

    /// <summary>
    /// Get the EPG data protocols supported by the grabber code/class/type implementation.
    /// </summary>
    public abstract TunerEpgGrabberProtocol PossibleProtocols
    {
      get;
    }

    /// <summary>
    /// Get the grabber's current status.
    /// </summary>
    /// <value><c>true</c> if the grabber is grabbing, otherwise <c>false</c></value>
    public bool IsGrabbing
    {
      get
      {
        return _isGrabbing;
      }
    }

    /// <summary>
    /// Get all available EPG data.
    /// </summary>
    /// <returns>the data, grouped by channel</returns>
    public abstract IList<Tuple<IChannel, IList<EpgProgram>>> GetData();

    #endregion

    #region IEpgGrabberInternal members

    /// <summary>
    /// Reload the grabber's configuration.
    /// </summary>
    /// <param name="configuration">The configuration of the associated tuner.</param>
    public void ReloadConfiguration(Tuner configuration)
    {
      bool wasGrabbingAllowed = IsGrabbingAllowed;
      _isTunerAllowedToGrab = configuration.UseForEpgGrabbing;
      if (wasGrabbingAllowed != IsGrabbingAllowed)
      {
        if (wasGrabbingAllowed)
        {
          Stop();
          return;
        }
        Start();
      }
    }

    /// <summary>
    /// The tuner implementation invokes this method when it tunes to a
    /// different transmitter.
    /// </summary>
    /// <param name="tuningDetail">The transmitter tuning detail.</param>
    public void OnTune(IChannel tuningDetail)
    {
      bool wasGrabbingAllowed = IsGrabbingAllowed;
      _tuningDetail = tuningDetail;
      if (wasGrabbingAllowed != IsGrabbingAllowed)
      {
        if (wasGrabbingAllowed)
        {
          Stop();
          return;
        }
        Start();
      }
    }

    #endregion
  }
}