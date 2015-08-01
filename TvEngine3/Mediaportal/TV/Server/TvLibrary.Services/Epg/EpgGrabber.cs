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
using System.Threading;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Services;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.TVLibrary.Epg
{
  /// <summary>
  /// Class which will continously grab EPG for all DVB channels.
  /// EPG is grabbed when:
  ///  - channel is a DVB channel
  ///  - if at least 2 hours have past since the previous time the EPG for the channel was grabbed
  ///  - if no cards are timeshifting or recording
  /// </summary>
  public class EpgGrabber : IDisposable
  {
    #region variables
    
    private int _epgReGrabAfter = 4 * 60; //hours
    private readonly System.Timers.Timer _epgTimer = new System.Timers.Timer();

    private bool _disposed;
    private bool _isRunning;
    private bool _reEntrant;    
    private List<EpgCard> _epgCards = new List<EpgCard>();

    #endregion

    #region ctor

    /// <summary>
    /// Constructor
    /// </summary>
    public EpgGrabber()
    {
      _epgTimer.Interval = 30000;
      _epgTimer.Elapsed += EpgTimerElapsed;
    }

    #endregion

    #region properties

    /// <summary>
    /// Property which returns true if EPG grabber is currently grabbing the epg
    /// or false is epg grabber is idle
    /// </summary>
    public bool IsRunning
    {
      get { return _isRunning; }
    }

    #endregion

    #region public members

    /// <summary>
    /// Start the EPG grabber.
    /// </summary>
    public void Start()
    {
      if (!SettingsManagement.GetValue("dvbEpgGrabberEnabledIdleTuners", true))
      {
        this.LogInfo("EPG grabber: idle EPG grabber disabled");
        return;
      }
      if (_isRunning)
      {
        return;
      }

      _epgReGrabAfter = SettingsManagement.GetValue("timeoutEPGRefresh", 240);

      TransponderList.Instance.RefreshTransponders();
      if (TransponderList.Instance.Count == 0)
      {
        return;
      }
      this.LogInfo("EPG: grabber initialized for {0} transmitter(s)..", TransponderList.Instance.Count);
      _isRunning = true;

      _epgTimer.Interval = 30000;
      _epgTimer.Enabled = true;
    }

    /// <summary>
    /// Stops the epg grabber
    /// </summary>
    public void Stop()
    {
      if (_isRunning == false)
      {
        return;
      }
      this.LogInfo("EPG: grabber stopped..");
      _epgTimer.Enabled = false;
      _isRunning = false;
      foreach (EpgCard epgCard in _epgCards)
      {
        epgCard.Stop();
      }
    }

    #endregion

    #region IDisposable Members    

    protected virtual void Dispose(bool disposing)
		{
		  if (disposing)
		  {
		    // get rid of managed resources
        if (!_disposed)
        {
          _epgTimer.SafeDispose();
          _epgCards.SafeDispose();
          _disposed = true;
        }
		  }
		  // get rid of unmanaged resources
		}
		
		
		/// <summary>
		/// Disposes the EPG card grabber
		/// </summary>    
		public void Dispose()
		{
		  Dispose(true);
		  GC.SuppressFinalize(this);
		}
		
		~EpgGrabber()
		{
		  Dispose(false);
		}

    #endregion

    #region private members

    private void EpgTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if (_reEntrant)
      {
        return;
      }
      _reEntrant = true;
      if (_epgTimer.Interval == 1000)
      {
        _epgTimer.Interval = 30000;
      }
      try
      {
        try
        {
          string threadname = Thread.CurrentThread.Name;
          if (string.IsNullOrEmpty(threadname))
          {
            Thread.CurrentThread.Name = "EPG grabber timer";
          }
        }
        catch (InvalidOperationException)
        {
        }

        foreach (EpgCard card in _epgCards)
        {
          if (!_isRunning)
          {
            return;
          }
          IUser ownerUser;
          if (card.IsGrabbing || !card.IsGrabbingEnabled || ServiceManager.Instance.InternalControllerService.IsCardInUse(card.IdTuner, out ownerUser))
          {
            continue;
          }
          GrabEpgWithCard(card);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
      finally
      {
        _reEntrant = false;
      }
    }

    private void GrabEpgWithCard(EpgCard epgCard)
    {
      do
      {
        Transponder transponder = TransponderList.Instance.GetNextTransponder();
        if (transponder == null)
        {
          // Reached the end of the list.
          TransponderList.Instance.RefreshTransponders();
          this.LogInfo("EPG grabber: refreshed transmitter list, count = {0}", TransponderList.Instance.Count);
          transponder = TransponderList.Instance.GetNextTransponder();
          if (transponder == null)
          {
            break;
          }
          break;
        }

        if (!ServiceManager.Instance.InternalControllerService.CanTune(epgCard.IdTuner, transponder.TuningChannel))
        {
          continue;
        }

        // Does the EPG for any of the channels on the transponder need updating?
        bool doGrab = false;
        foreach (Channel ch in transponder.Channels)
        {
          TimeSpan ts = DateTime.Now - ch.LastGrabTime.GetValueOrDefault(DateTime.MinValue);
          if (ts.TotalMinutes >= _epgReGrabAfter)
          {
            doGrab = true;
            break;
          }
        }

        if (doGrab)
        {
          epgCard.GrabEpg(transponder);
          return;
        }
      }
      while (true);
    }

    #endregion
  }
}