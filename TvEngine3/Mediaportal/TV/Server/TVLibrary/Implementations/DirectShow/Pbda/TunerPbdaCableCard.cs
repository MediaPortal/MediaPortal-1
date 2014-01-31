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
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Pbda
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles North
  /// American CableCARD tuners with PBDA drivers.
  /// </summary>
  public class TunerPbdaCableCard : TunerBdaAtsc, IConditionalAccessMenuActions
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
    private Uri _caMenuUriBase = null;
    private object _caMenuCallBackLock = new object();
    private IConditionalAccessMenuCallBacks _caMenuCallBacks = null;
    private IList<string> _caMenuLinks = new List<string>();
    private Stack<string> _caMenuStack = new Stack<string>();

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

      _caInterface = _filterMain as IBDA_ConditionalAccess;
      if (_caInterface == null)
      {
        throw new TvException("Failed to find BDA conditional access interface on tuner filter.");
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

      SmartCardStatusType status;
      SmartCardAssociationType association;
      string error;
      bool isOutOfBandTunerLocked = false;
      int hr = _caInterface.get_SmartCardStatus(out status, out association, out error, out isOutOfBandTunerLocked);
      HResult.ThrowException(hr, "Failed to read smart card status.");
      if (status != SmartCardStatusType.CardInserted || (IsScanning && !isOutOfBandTunerLocked) || !string.IsNullOrEmpty(error))
      {
        this.LogError("PBDA CableCARD: smart card status");
        this.LogError("  status      = {0}", status);
        this.LogError("  association = {0}", association);
        this.LogError("  OOB locked  = {0}", isOutOfBandTunerLocked);
        this.LogError("  error       = {0}", error);
        throw new TvExceptionNoSignal();
      }

      if (!IsScanning)
      {
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
      if (atscChannel == null)
      {
        return false;
      }
      // Major channel holds the virtual channel number that we use for tuning.
      return atscChannel.MajorChannel > 0;
    }

    #endregion

    #region IConditionalAccessMenuActions members

    /// <summary>
    /// Set the menu call back delegate.
    /// </summary>
    /// <param name="callBacks">The call back delegate.</param>
    public void SetCallBacks(IConditionalAccessMenuCallBacks callBacks)
    {
      lock (_caMenuCallBackLock)
      {
        _caMenuCallBacks = callBacks;
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

      lock (_caMenuCallBackLock)
      {
        if (_caMenuCallBacks == null)
        {
          this.LogDebug("PBDA CableCARD: menu call backs are not set");
        }
        else
        {
          _caMenuStack.Clear();
          _caMenuLinks.Clear();
          _caMenuStack.Push("root");
          _caMenuCallBacks.OnCiMenu(cardName, cardManufacturer, string.Empty, appCount);
        }
        this.LogDebug("  application count = {0}", appCount);
        _caMenuUriBase = null;
        for (int i = 0; i < appCount; i++)
        {
          SmartCardApplication app = applicationDetails[i];
          this.LogDebug("  application #{0}", i + 1);
          this.LogDebug("    type            = {0}", app.ApplicationType);
          this.LogDebug("    version         = {0}", app.ApplicationVersion);
          this.LogDebug("    name            = {0}", app.pbstrApplicationName);

          string appUrl = app.pbstrApplicationURL;
          if (_caMenuUriBase == null && appUrl.StartsWith("http"))
          {
            _caMenuUriBase = new Uri(appUrl);
          }
          else if (_caMenuUriBase != null && !appUrl.StartsWith("http"))
          {
            Uri uri = new Uri(_caMenuUriBase, appUrl);
            appUrl = uri.AbsoluteUri;
          }
          this.LogDebug("    URL             = {0}", appUrl);

          if (_caMenuCallBacks != null)
          {
            _caMenuCallBacks.OnCiMenuChoice(i, app.pbstrApplicationName);
            _caMenuLinks.Add(appUrl);
          }
        }
      }

      this.LogDebug("PBDA CableCARD: result = success");
      return true;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseMenu()
    {
      this.LogDebug("PBDA CableCARD: close menu");

      if (_caInterface == null)
      {
        this.LogWarn("PBDA CableCARD: not initialised or interface not supported");
        return false;
      }

      int hr = _caInterface.InformUIClosed(0, UICloseReasonType.UserClosed);
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("PBDA CableCARD: result = success");
        return true;
      }

      this.LogError("PBDA CableCARD: failed to inform the CableCARD that the user interface has been closed, hr = 0x{0:x}");
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

      if (_caInterface == null)
      {
        this.LogWarn("PBDA CableCARD: not initialised or interface not supported");
        return false;
      }

      lock (_caMenuCallBackLock)
      {
        if (_caMenuCallBacks == null)
        {
          this.LogDebug("PBDA CableCARD: menu call backs are not set");
        }
        string url;
        if (choice == 0)    // Back up one level.
        {
          if (_caMenuStack.Count == 1)
          {
            this.LogDebug("PBDA CableCARD: closing root menu");
            _caMenuStack.Clear();
            _caMenuLinks.Clear();
            return true;
          }
          if (_caMenuStack.Count == 2)
          {
            this.LogDebug("PBDA CableCARD: return to root menu");
            if (_caMenuCallBacks == null)
            {
              _caMenuStack.Pop();
              return EnterMenu();
            }
            return true;
          }
          url = _caMenuStack.Peek();
        }
        else
        {
          url = _caMenuLinks[choice - 1];
        }

        if (string.IsNullOrEmpty(url))
        {
          this.LogDebug("PBDA CableCARD: selected non-link entry, nothing to do");
          return true;
        }

        this.LogDebug("PBDA CableCARD: retrieving menu from URI {0}", url);
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Timeout = 5000;
        HttpWebResponse response = null;
        try
        {
          response = (HttpWebResponse)request.GetResponse();
        }
        catch (Exception ex)
        {
          this.LogError(ex, "PBDA CableCARD: failed to send HTTP menu request to URI {0}", url);
          request.Abort();
          return false;
        }
        string content = string.Empty;
        try
        {
          using (System.IO.Stream s = response.GetResponseStream())
          {
            using (TextReader textReader = new StreamReader(s))
            {
              content = textReader.ReadToEnd();
              textReader.Close();
            }
            s.Close();
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex, "PBDA CableCARD: failed to receive HTTP menu response from URI {0}", url);
          return false;
        }
        finally
        {
          if (response != null)
          {
            response.Close();
          }
        }

        // Reformat from pure HTML into title and menu items. This is quite
        // hacky, but we have no way to render HTML in MediaPortal.
        this.LogDebug("PBDA CableCARD: retrieved raw menu HTML\r\n{0}", content);
        try
        {
          content = Regex.Replace(content, "(<\\/b>|<center>)", string.Empty, RegexOptions.IgnoreCase);
          content = Regex.Replace(content, "&nbsp;", " ", RegexOptions.IgnoreCase);
          content = Regex.Replace(content, "\\s+", " ");
          content = Regex.Replace(content, @".*<body( [^>]*)?>\s*(.*?)\s*</body>.*", "$2", RegexOptions.IgnoreCase | RegexOptions.Singleline);

          this.LogDebug("PBDA CableCARD: pre-split menu HTML\r\n{0}", content);
          string[] sections = content.Split(new string[] { "<b>", "<B>", "<br>", "<BR>" }, StringSplitOptions.RemoveEmptyEntries);

          // The split options should remove empty entries... but it won't remove whitespace entries.
          List<string> entries = new List<string>();
          foreach (string s in sections)
          {
            string trimmed = s.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
              entries.Add(trimmed);
            }
          }

          if (_caMenuCallBacks != null)
          {
            _caMenuLinks.Clear();
            _caMenuCallBacks.OnCiMenu(entries[0], string.Empty, string.Empty, entries.Count - 1);
          }
          this.LogDebug("  title  = {0}", entries[0]);
          for (int i = 1; i < entries.Count; i++)
          {
            string itemText = entries[i];
            string itemUrl = string.Empty;
            Match m = Regex.Match(itemText, @"<a href=""\s*([^""]+)\s*"">\s*(.*?)\s*</a>", RegexOptions.IgnoreCase);
            if (m.Success)
            {
              itemUrl = m.Groups[1].Captures[0].Value;
              if (_caMenuUriBase != null && !itemUrl.StartsWith("http"))
              {
                Uri uri = new Uri(_caMenuUriBase, itemUrl);
                itemUrl = uri.AbsoluteUri;
              }
              itemText = m.Groups[2].Captures[0].Value;
              this.LogDebug("  item {0} = {1} [{2}]", i, itemText, itemUrl);
            }
            else
            {
              this.LogDebug("  item {0} = {1}", i, itemText);
            }
            if (_caMenuCallBacks != null)
            {
              _caMenuLinks.Add(itemUrl);
              _caMenuCallBacks.OnCiMenuChoice(i - 1, itemText);
            }
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex, "PBDA CableCARD: failed to handle menu HTML\r\n{0}", content);
          return false;
        }

        if (_caMenuCallBacks != null)
        {
          if (choice == 0)
          {
            _caMenuStack.Pop();
          }
          else
          {
            _caMenuStack.Push(url);
          }
        }
      }

      this.LogDebug("PBDA CableCARD: result = success");
      return true;
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
    protected override void PerformSignalStatusUpdate(bool onlyUpdateLock)
    {
      if (IsScanning)
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