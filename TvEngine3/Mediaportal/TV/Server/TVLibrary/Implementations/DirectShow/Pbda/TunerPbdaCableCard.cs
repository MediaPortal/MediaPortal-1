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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Pbda
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles North
  /// American CableCARD tuners with PBDA drivers.
  /// </summary>
  public class TunerPbdaCableCard : TunerBdaAtsc
  {
    #region constants

    private static readonly Guid PBDA_PT_FILTER_CLSID = new Guid(0x89c2e132, 0xc29b, 0x11db, 0x96, 0xfa, 0x00, 0x50, 0x56, 0xc0, 0x00, 0x08);

    #endregion

    #region variables

    /// <summary>
    /// The PBDA PT filter.
    /// </summary>
    private IBaseFilter _filterPbdaPt = null;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerPbdaCableCard"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    public TunerPbdaCableCard(DsDevice device)
      : base(device)
    {
      _tunerType = CardType.Atsc;
    }

    #endregion

    #region graph building

    /// <summary>
    /// Add and connect the appropriate BDA capture/receiver filter into the graph.
    /// </summary>
    /// <param name="lastFilter">The upstream filter to connect the capture filter to.</param>
    protected virtual void AddAndConnectCaptureFilterIntoGraph(ref IBaseFilter lastFilter)
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

      int hr = _captureGraphBuilder.RenderStream(null, null, lastFilter, null, _filterPbdaPt);
      HResult.ThrowException(hr, "Failed to render into the PBDA PT filter.");
      lastFilter = _filterPbdaPt;
    }

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    protected override void PerformLoading()
    {
      this.LogDebug("PBDA CableCARD: perform loading");
      base.PerformLoading();

      // Connect the tuner filter OOB info into TsWriter.
      int hr = _captureGraphBuilder.RenderStream(null, null, _filterMain, null, _filterTsWriter);
      HResult.ThrowException(hr, "Failed to render out-of-band connection from the tuner filter into the TS writer/analyser filter.");

      ReadCableCardInfo();
    }

    private void ReadCableCardInfo()
    {
      this.LogDebug("PBDA CableCARD: read CableCARD information");

      IBDA_ConditionalAccess bdaCa = _filterMain as IBDA_ConditionalAccess;
      if (bdaCa == null)
      {
        throw new TvException("Failed to find BDA conditional access interface on tuner filter.");
      }
      int hr;

      string cardName;
      string cardManufacturer;
      bool isDaylightSavings;
      byte ratingRegion;
      int timeOffset;
      string lang;
      EALocationCodeType locationCode;
      try
      {
        hr = bdaCa.get_SmartCardInfo(out cardName, out cardManufacturer, out isDaylightSavings, out ratingRegion, out timeOffset, out lang, out locationCode);
        HResult.ThrowException(hr, "Failed to read smart card information.");
        this.LogDebug("PBDA CableCARD: smart card information");
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
      }
      catch (Exception ex)
      {
        this.LogError(ex, "PBDA CableCARD: failed to read smart card information");
      }

      SmartCardStatusType status;
      SmartCardAssociationType association;
      string error;
      bool isOutOfBandTunerLocked;
      try
      {
        hr = bdaCa.get_SmartCardStatus(out status, out association, out error, out isOutOfBandTunerLocked);
        HResult.ThrowException(hr, "Failed to read smart card status.");
        this.LogDebug("PBDA CableCARD: smart card status");
        this.LogDebug("  status      = {0}", status);
        this.LogDebug("  association = {0}", association);
        this.LogDebug("  OOB locked  = {0}", isOutOfBandTunerLocked);
        this.LogDebug("  error       = {0}", error);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "PBDA CableCARD: failed to read smart card status");
      }

      // Smart card applications.
      int scAppCount = 0;
      int maxAppCount = 32;
      SmartCardApplication[] applicationDetails = new SmartCardApplication[33];
      try
      {
        hr = bdaCa.get_SmartCardApplications(out scAppCount, maxAppCount, applicationDetails);
        HResult.ThrowException(hr, "Failed to read smart card application information.");
        this.LogDebug("PBDA CC: got smart card application info, application count = {0}", scAppCount);
        for (int i = 0; i < scAppCount; i++)
        {
          SmartCardApplication applicationDetail = applicationDetails[i];
          this.LogDebug("  application #{0}", i + 1);
          this.LogDebug("    type    = {0}", applicationDetail.ApplicationType);
          this.LogDebug("    version = {0}", applicationDetail.ApplicationVersion);
          this.LogDebug("    name    = {0}", applicationDetail.pbstrApplicationName);
          this.LogDebug("    URL     = {0}", applicationDetail.pbstrApplicationURL);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "PBDA CableCARD: failed to read smart card application information");
      }
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    protected override void PerformUnloading()
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
    protected override void PerformTuning(IChannel channel)
    {
      this.LogDebug("PBDA CableCARD: perform tuning");
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      IBDA_ConditionalAccess bdaCa = _filterMain as IBDA_ConditionalAccess;
      if (bdaCa == null)
      {
        throw new TvException("Failed to find BDA conditional access interface on tuner filter.");
      }

      SmartCardStatusType status;
      SmartCardAssociationType association;
      string error;
      bool isOutOfBandTunerLocked = false;
      int hr = bdaCa.get_SmartCardStatus(out status, out association, out error, out isOutOfBandTunerLocked);
      HResult.ThrowException(hr, "Failed to read smart card status.");
      this.LogDebug("PBDA CableCARD: smart card status");
      this.LogDebug("  status      = {0}", status);
      this.LogDebug("  association = {0}", association);
      this.LogDebug("  OOB locked  = {0}", isOutOfBandTunerLocked);
      this.LogDebug("  error       = {0}", error);
      if (status != SmartCardStatusType.CardInserted || !isOutOfBandTunerLocked)
      {
        throw new TvExceptionNoSignal();
      }

      hr = bdaCa.TuneByChannel((short)atscChannel.MajorChannel);
      HResult.ThrowException(hr, "Failed to tune channel.");
    }

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        return false;
      }
      // Major channel holds the virtual channel number that we use for tuning.
      return atscChannel.MajorChannel > 0;
    }

    #endregion

    /// <summary>
    /// Stop the tuner. The actual result of this function depends on tuner configuration.
    /// </summary>
    public override void Stop()
    {
      // CableCARD tuners don't forget the tuned channel after they're stopped,
      // and they reject tune requests for the current channel.
      IChannel savedTuningDetail = _currentTuningDetail;
      base.Stop();
      _currentTuningDetail = savedTuningDetail;
    }
  }
}