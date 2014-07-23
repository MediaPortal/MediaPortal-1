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
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Pbda
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles North
  /// American CableCARD tuners with PBDA drivers.
  /// </summary>
  internal class TunerPbdaCableCard : TunerBdaAtsc, IConditionalAccessMenuActions
  {
    #region constants

    private static readonly Guid PBDA_PT_FILTER_CLSID = new Guid(0x89c2e132, 0xc29b, 0x11db, 0x96, 0xfa, 0x00, 0x50, 0x56, 0xc0, 0x00, 0x08);

    #endregion

    #region variables

    /// <summary>
    /// The PBDA PT filter.
    /// </summary>
    private IBaseFilter _filterPbdaPt = null;

    /// <summary>
    /// The PBDA conditional access interface, used for tuning and conditional
    /// access menu interaction.
    /// </summary>
    private IBDA_ConditionalAccess _caInterface = null;

    // CA menu variables
    private object _caMenuCallBackLock = new object();
    private IConditionalAccessMenuCallBack _caMenuCallBack = null;
    private CableCardMmiHandler _caMenuHandler = null;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerPbdaCableCard"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    public TunerPbdaCableCard(DsDevice device)
      : base(device)
    {
      _caMenuHandler = new CableCardMmiHandler(EnterMenu, CloseDialog);

      // CableCARD tuners are limited to one channel per tuner, even for
      // non-encrypted channels:
      // The DRIT SHALL output the selected program content as a single program
      // MPEG-TS in RTP packets according to [RTSP] and [RTP].
      // - OpenCable DRI I04 specification, 10 September 2010
      _supportsSubChannels = false;
    }

    #endregion

    #region graph building

    /// <summary>
    /// Add and connect the appropriate BDA capture/receiver filter into the graph.
    /// </summary>
    /// <param name="lastFilter">The upstream filter to connect the capture filter to.</param>
    protected override void AddAndConnectCaptureFilterIntoGraph(ref IBaseFilter lastFilter)
    {
      // PBDA graphs don't require a capture filter. Expect problems if you try to connect anything
      // other than the PBDA PT filter to the tuner filter output.
      this.LogDebug("PBDA CableCARD: add PBDA PT filter");
      try
      {
        _filterPbdaPt = FilterGraphTools.AddFilterFromRegisteredClsid(_graph, PBDA_PT_FILTER_CLSID, "PBDA PT Filter");
      }
      catch (Exception ex)
      {
        throw new TvException("Failed to add PBDA PT filter, are you using Windows 7+?", ex);
      }

      FilterGraphTools.ConnectFilters(_graph, lastFilter, 0, _filterPbdaPt, 0);
      lastFilter = _filterPbdaPt;
    }

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    public override void PerformLoading()
    {
      this.LogDebug("PBDA CableCARD: perform loading");
      base.PerformLoading();

      // Connect the tuner filter OOB info into TsWriter.
      this.LogDebug("PBDA CableCARD: connect out-of-band stream");
      FilterGraphTools.ConnectFilters(_graph, _filterMain, 1, _filterTsWriter, 1);

      _caInterface = _filterMain as IBDA_ConditionalAccess;
      if (_caInterface == null)
      {
        throw new TvException("Failed to find BDA conditional access interface on tuner filter.");
      }
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    public override void PerformUnloading()
    {
      this.LogDebug("PBDA CableCARD: perform unloading");
      if (_graph != null)
      {
        _graph.RemoveFilter(_filterPbdaPt);
      }
      Release.ComObject("PBDA CableCARD tuner PBDA PT filter", ref _filterPbdaPt);

      base.PerformUnloading();
    }

    #endregion

    #region tuning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("PBDA CableCARD: perform tuning");
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      // Tuning without a CableCARD (clear QAM) is currently not supported.
      SmartCardStatusType status;
      SmartCardAssociationType association;
      string error;
      bool isOutOfBandTunerLocked = false;
      int hr = _caInterface.get_SmartCardStatus(out status, out association, out error, out isOutOfBandTunerLocked);
      HResult.ThrowException(hr, "Failed to read smart card status.");
      if (status != SmartCardStatusType.CardInserted || (_channelScanner.IsScanning && !isOutOfBandTunerLocked) || !string.IsNullOrEmpty(error))
      {
        this.LogError("PBDA CableCARD: smart card status");
        this.LogError("  status      = {0}", status);
        this.LogError("  association = {0}", association);
        this.LogError("  OOB locked  = {0}", isOutOfBandTunerLocked);
        this.LogError("  error       = {0}", error);
        throw new TvExceptionNoSignal();
      }

      if (!_channelScanner.IsScanning)
      {
        // Note: SDV not explicitly supported.
        hr = _caInterface.TuneByChannel((short)atscChannel.MajorChannel);
        HResult.ThrowException(hr, "Failed to tune channel.");
      }
    }

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      ATSCChannel atscChannel = channel as ATSCChannel;
      // Tuning without a CableCARD (clear QAM) is currently not supported.
      // Major channel holds the virtual channel number that we use for tuning.
      if (atscChannel == null || atscChannel.MajorChannel <= 0 || atscChannel.MinorChannel >= 0)
      {
        return false;
      }
      return true;
    }

    #endregion

    #region IConditionalAccessMenuActions members

    /// <summary>
    /// Set the menu call back delegate.
    /// </summary>
    /// <param name="callBack">The call back delegate.</param>
    public void SetMenuCallBack(IConditionalAccessMenuCallBack callBack)
    {
      lock (_caMenuCallBackLock)
      {
        _caMenuCallBack = callBack;
      }
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterMenu()
    {
      this.LogDebug("PBDA CableCARD: enter menu");

      if (_caInterface == null)
      {
        this.LogWarn("PBDA CableCARD: not initialised or interface not supported");
        return false;
      }

      // The root menu is the CableCARD application list. Each application is a
      // simple series of HTML pages.
      string cardName;
      string cardManufacturer;
      bool isDaylightSavings;
      byte ratingRegion;
      int timeOffset;
      string lang;
      EALocationCodeType locationCode;
      int hr = _caInterface.get_SmartCardInfo(out cardName, out cardManufacturer, out isDaylightSavings, out ratingRegion, out timeOffset, out lang, out locationCode);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("PBDA CableCARD: failed to read smart card information, hr = 0x{0:x}");
        return false;
      }
      this.LogDebug("  card name         = {0}", cardName);
      this.LogDebug("  card manufacturer = {0}", cardManufacturer);
      this.LogDebug("  is DS time        = {0}", isDaylightSavings);
      this.LogDebug("  rating region     = {0}", ratingRegion);
      this.LogDebug("  time offset       = {0}", timeOffset);
      this.LogDebug("  language          = {0}", lang);
      this.LogDebug("  location code...");
      this.LogDebug("    scheme          = {0}", locationCode.LocationCodeScheme);
      this.LogDebug("    state code      = {0}", locationCode.StateCode);
      this.LogDebug("    county sub div  = {0}", locationCode.CountySubdivision);
      this.LogDebug("    county code     = {0}", locationCode.CountyCode);

      int appCount = 0;
      int maxAppCount = 32;
      SmartCardApplication[] applicationDetails = new SmartCardApplication[33];
      hr = _caInterface.get_SmartCardApplications(out appCount, maxAppCount, applicationDetails);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("PBDA CableCARD: failed to read smart card application list, hr = 0x{0:x}");
        return false;
      }

      // Translate into standard DRI application list format.
      int byteCount = 1 + (7 * appCount);   // + 1 for app count, (1 + 2 + 1 + 1 + 2) for app type, version, name length, name NULL termination, URL length
      foreach (SmartCardApplication app in applicationDetails)
      {
        byteCount += app.pbstrApplicationName.Length + app.pbstrApplicationURL.Length;
      }
      byte[] applicationList = new byte[byteCount];
      applicationList[0] = (byte)appCount;
      int offset = 1;
      foreach (SmartCardApplication app in applicationDetails)
      {
        applicationList[offset++] = (byte)app.ApplicationType;
        applicationList[offset++] = (byte)(app.ApplicationVersion >> 8);
        applicationList[offset++] = (byte)(app.ApplicationVersion & 0xff);
        applicationList[offset++] = (byte)(app.pbstrApplicationName.Length + 1);    // + 1 for NULL termination
        Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes(app.pbstrApplicationName), 0, applicationList, offset, app.pbstrApplicationName.Length);
        offset += app.pbstrApplicationName.Length;
        applicationList[offset++] = 0;
        applicationList[offset++] = (byte)(app.pbstrApplicationURL.Length >> 8);
        applicationList[offset++] = (byte)(app.pbstrApplicationURL.Length & 0xff);
        Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes(app.pbstrApplicationURL), 0, applicationList, offset, app.pbstrApplicationURL.Length);
        offset += app.pbstrApplicationURL.Length;
      }
      lock (_caMenuCallBackLock)
      {
        return _caMenuHandler.EnterMenu(cardName, cardManufacturer, string.Empty, applicationList, _caMenuCallBack);
      }
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseMenu()
    {
      this.LogDebug("PBDA CableCARD: close menu");
      return CloseDialog(0);
    }

    /// <summary>
    /// Inform the CableCARD that the user has closed a dialog.
    /// </summary>
    /// <param name="dialogNumber">The identifier for the dialog that has been closed.</param>
    /// <returns><c>true</c> if the CableCARD is successfully notified, otherwise <c>false</c></returns>
    public bool CloseDialog(byte dialogNumber)
    {
      this.LogDebug("PBDA CableCARD: close dialog, dialog number = {0}", dialogNumber);

      if (_caInterface == null)
      {
        this.LogWarn("PBDA CableCARD: not initialised or interface not supported");
        return false;
      }

      int hr = _caInterface.InformUIClosed(dialogNumber, UICloseReasonType.UserClosed);
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("PBDA CableCARD: result = success");
        return true;
      }

      this.LogError("PBDA CableCARD: failed to inform the CableCARD that the user interface has been closed, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenuEntry(byte choice)
    {
      this.LogDebug("PBDA CableCARD: select menu entry, choice = {0}", choice);
      lock (_caMenuCallBackLock)
      {
        return _caMenuHandler.SelectEntry(choice, _caMenuCallBack);
      }
    }

    /// <summary>
    /// Send an answer to an enquiry from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the enquiry.</param>
    /// <param name="answer">The user's answer to the enquiry.</param>
    /// <returns><c>true</c> if the answer is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool AnswerEnquiry(bool cancel, string answer)
    {
      if (answer == null)
      {
        answer = string.Empty;
      }
      this.LogDebug("PBDA CableCARD: answer enquiry, answer = {0}, cancel = {1}", answer, cancel);

      // TODO I don't know how to implement this yet.
      return true;
    }

    #endregion

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    public override void PerformSignalStatusUpdate(bool onlyUpdateLock)
    {
      if (_channelScanner != null && _channelScanner.IsScanning)
      {
        // When scanning we need the OOB tuner to be locked. We already updated
        // the lock status when tuning, so only update again when monitoring.
        if (!onlyUpdateLock)
        {
          if (_caInterface == null)
          {
            return;
          }
          SmartCardStatusType status;
          SmartCardAssociationType association;
          string error;
          bool isSignalLocked;
          int hr = _caInterface.get_SmartCardStatus(out status, out association, out error, out isSignalLocked);
          if (hr != (int)HResult.Severity.Success)
          {
            this.LogWarn("PBDA CableCARD: potential error updating signal status, hr = 0x{0:x}", hr);
            return;
          }
          _isSignalLocked = isSignalLocked;
        }
        // We can only get lock status for the OOB tuner.
        _isSignalPresent = _isSignalLocked;
        _signalLevel = 0;
        _signalQuality = 0;
        return;
      }

      base.PerformSignalStatusUpdate(onlyUpdateLock);
    }
  }
}