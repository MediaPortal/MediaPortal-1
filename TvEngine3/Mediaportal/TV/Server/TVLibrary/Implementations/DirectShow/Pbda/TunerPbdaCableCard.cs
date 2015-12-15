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
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Pbda
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for North American CableCARD
  /// tuners with PBDA drivers.
  /// </summary>
  internal class TunerPbdaCableCard : TunerBdaAtsc, IConditionalAccessMenuActions
  {
    #region variables

    /// <summary>
    /// The PBDA conditional access interface, used for tuning and conditional
    /// access menu interaction.
    /// </summary>
    private IBDA_ConditionalAccess _caInterface = null;

    /// <summary>
    /// The tuner's sub-channel manager.
    /// </summary>
    private ISubChannelManager _subChannelManager = null;

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
      : base(device, BroadcastStandard.Scte)
    {
      _caMenuHandler = new CableCardMmiHandler(EnterMenu, CloseDialog);
    }

    #endregion

    #region ITunerInternal members

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading(StreamFormat streamFormat = StreamFormat.Default)
    {
      this.LogDebug("PBDA CableCARD: perform loading");

      if (streamFormat == StreamFormat.Default)
      {
        streamFormat = StreamFormat.Scte;
      }
      IList<ITunerExtension> extensions = base.PerformLoading(streamFormat);

      // Connect the tuner filter OOB info into TsWriter.
      this.LogDebug("PBDA CableCARD: connect out-of-band stream");
      FilterGraphTools.ConnectFilters(Graph, MainFilter, 1, TsWriter, 1);

      _caInterface = MainFilter as IBDA_ConditionalAccess;
      if (_caInterface == null)
      {
        throw new TvException("Failed to find BDA conditional access interface on tuner filter.");
      }

      // CableCARD tuners are limited to one channel per tuner, even for
      // non-encrypted channels:
      // The DRIT SHALL output the selected program content as a single program
      // MPEG-TS in RTP packets according to [RTSP] and [RTP].
      // - OpenCable DRI I04 specification, 10 September 2010
      _subChannelManager = new SubChannelManagerMpeg2Ts(TsWriter as ITsWriter, false);
      return extensions;
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformUnloading(bool isFinalising = false)
    {
      this.LogDebug("PBDA CableCARD: perform unloading");
      if (!isFinalising)
      {
        _subChannelManager = null;
      }
      base.PerformUnloading(isFinalising);
    }

    #endregion

    #region tuning

    /// <summary>
    /// Get the broadcast standards supported by the tuner code/class/type implementation.
    /// </summary>
    public override BroadcastStandard PossibleBroadcastStandards
    {
      get
      {
        return BroadcastStandard.Scte;
      }
    }

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      // Tuning without a CableCARD (ATSC or clear QAM) is currently not supported.
      ChannelScte scteChannel = channel as ChannelScte;
      short virtualChannelNumber;
      if (
        scteChannel == null ||
        !SupportedBroadcastStandards.HasFlag(BroadcastStandard.Scte) ||
        (
          (!short.TryParse(scteChannel.LogicalChannelNumber, out virtualChannelNumber) || virtualChannelNumber <= 0) &&
          scteChannel.Frequency != -1   // special case: CableCARD scanning
        )
      )
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("PBDA CableCARD: perform tuning");
      ChannelScte scteChannel = channel as ChannelScte;
      short virtualChannelNumber;
      if (scteChannel == null || !short.TryParse(scteChannel.LogicalChannelNumber, out virtualChannelNumber))
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      // Tuning without a CableCARD (clear QAM) is currently not supported.
      SmartCardStatusType status;
      SmartCardAssociationType association;
      string error;
      bool isOutOfBandTunerLocked = false;
      int hr = _caInterface.get_SmartCardStatus(out status, out association, out error, out isOutOfBandTunerLocked);
      TvExceptionDirectShowError.Throw(hr, "Failed to read smart card status.");
      if (status != SmartCardStatusType.CardInserted || (ChannelScanningInterface.IsScanning && !isOutOfBandTunerLocked) || !string.IsNullOrEmpty(error))
      {
        this.LogError("PBDA CableCARD: smart card status");
        this.LogError("  status      = {0}", status);
        this.LogError("  association = {0}", association);
        this.LogError("  OOB locked  = {0}", isOutOfBandTunerLocked);
        this.LogError("  error       = {0}", error);
        throw new TvException("CableCARD or out-of-band tuner error.");
      }

      if (!ChannelScanningInterface.IsScanning)
      {
        // Note: SDV not explicitly supported. We can't request the program
        // number from the tuner like we can when interacting directly with the
        // DRI interface. Therefore tuning SDV channels will fail if the
        // channel program number is not correct.
        hr = _caInterface.TuneByChannel(virtualChannelNumber);
        TvExceptionDirectShowError.Throw(hr, "Failed to tune channel.");
      }
    }

    #endregion

    #region signal

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    public override void GetSignalStatus(out bool isLocked, out bool isPresent, out int strength, out int quality, bool onlyGetLock)
    {
      if (ChannelScanningInterface != null && ChannelScanningInterface.IsScanning)
      {
        isLocked = false;
        try
        {
          // When scanning we need the OOB tuner to be locked. We already
          // updated the lock status when tuning, so only update again when
          // monitoring.
          if (onlyGetLock)
          {
            isLocked = true;
          }
          else
          {
            if (_caInterface != null)
            {
              SmartCardStatusType status;
              SmartCardAssociationType association;
              string error;
              int hr = _caInterface.get_SmartCardStatus(out status, out association, out error, out isLocked);
              if (hr != (int)NativeMethods.HResult.S_OK)
              {
                this.LogWarn("PBDA CableCARD: potential error updating signal status, hr = 0x{0:x}", hr);
              }
            }
          }
          return;
        }
        finally
        {
          // We can only get lock status for the OOB tuner.
          isPresent = isLocked;
          if (isLocked)
          {
            strength = 100;
            quality = 100;
          }
          else
          {
            strength = 0;
            quality = 0;
          }
        }
      }

      base.GetSignalStatus(out isLocked, out isPresent, out strength, out quality, onlyGetLock);
    }

    #endregion

    #region interfaces

    /// <summary>
    /// Get the tuner's sub-channel manager.
    /// </summary>
    public override ISubChannelManager SubChannelManager
    {
      get
      {
        return _subChannelManager;
      }
    }

    #endregion

    #endregion

    #region IConditionalAccessMenuActions members

    /// <summary>
    /// Set the menu call back delegate.
    /// </summary>
    /// <param name="callBack">The call back delegate.</param>
    void IConditionalAccessMenuActions.SetCallBack(IConditionalAccessMenuCallBack callBack)
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
    bool IConditionalAccessMenuActions.Enter()
    {
      return EnterMenu();
    }

    private bool EnterMenu()
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
      if (hr != (int)NativeMethods.HResult.S_OK)
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
      if (hr != (int)NativeMethods.HResult.S_OK)
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
    bool IConditionalAccessMenuActions.Close()
    {
      this.LogDebug("PBDA CableCARD: close menu");
      return CloseDialog(0);
    }

    /// <summary>
    /// Inform the CableCARD that the user has closed a dialog.
    /// </summary>
    /// <param name="dialogNumber">The identifier for the dialog that has been closed.</param>
    /// <returns><c>true</c> if the CableCARD is successfully notified, otherwise <c>false</c></returns>
    private bool CloseDialog(byte dialogNumber)
    {
      this.LogDebug("PBDA CableCARD: close dialog, dialog number = {0}", dialogNumber);

      if (_caInterface == null)
      {
        this.LogWarn("PBDA CableCARD: not initialised or interface not supported");
        return false;
      }

      int hr = _caInterface.InformUIClosed(dialogNumber, UICloseReasonType.UserClosed);
      if (hr == (int)NativeMethods.HResult.S_OK)
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
    bool IConditionalAccessMenuActions.SelectEntry(byte choice)
    {
      this.LogDebug("PBDA CableCARD: select menu entry, choice = {0}", choice);
      lock (_caMenuCallBackLock)
      {
        return _caMenuHandler.SelectMenuEntry(choice, _caMenuCallBack);
      }
    }

    /// <summary>
    /// Send an answer to an enquiry from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the enquiry.</param>
    /// <param name="answer">The user's answer to the enquiry.</param>
    /// <returns><c>true</c> if the answer is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.AnswerEnquiry(bool cancel, string answer)
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
  }
}