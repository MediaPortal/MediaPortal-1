#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.IO;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Settings.Wizard;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Scanning;
using MediaPortal.Util;

namespace WindowPlugins.GUISettings.Wizard
{
  /// <summary>
  /// Summary description for GUIWizardDVBTCountry.
  /// </summary>
  public class GUIWizardScanBase : GUIWindow, AutoTuneCallback
  {
    [SkinControl(26)] protected GUILabelControl lblChannelsFound = null;
    [SkinControl(27)] protected GUILabelControl lblStatus = null;
    [SkinControl(24)] protected GUIListControl listChannelsFound = null;
    [SkinControl(5)] protected GUIButtonControl btnNext = null;
    [SkinControl(25)] protected GUIButtonControl btnBack = null;
    [SkinControl(20)] protected GUIProgressControl progressBar = null;

    private int card = 0;
    private bool updateList = false;
    private bool _autoTuneRunning = false;

    public GUIWizardScanBase()
    {
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
      GUIGraphicsContext.VMR9Allowed = true;
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIGraphicsContext.VMR9Allowed = false;
      btnNext.Disabled = true;
      btnBack.Disabled = true;
      progressBar.Percentage = 0;
      progressBar.Disabled = false;
      progressBar.IsVisible = true;
      DoUpdateList();
      Thread WorkerThread = new Thread(new ThreadStart(ScanThread));
      WorkerThread.SetApartmentState(ApartmentState.STA);
      //WorkerThread.IsBackground=true;
      WorkerThread.Start();
    }


    protected virtual void OnScanDone()
    {
    }

    protected virtual NetworkType Network()
    {
      return NetworkType.Unknown;
    }

    public void ScanThread()
    {
      card = Int32.Parse(GUIPropertyManager.GetProperty("#WizardCard"));
      string country = GUIPropertyManager.GetProperty("#WizardCountry");
      //Recorder.Paused = true;

      updateList = false;
      Recorder.StartAutoTune(Network(), card, this);
      _autoTuneRunning = true;
      while (_autoTuneRunning && (GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.STOPPING))
      {
        Thread.Sleep(500);
      }

      btnNext.Disabled = false;
      btnBack.Disabled = false;
      progressBar.Percentage = 100;
      lblStatus.Label = "Press Next to continue the setup";
      MapTvToOtherCards(card);
      MapRadioToOtherCards(card);

      GUIControl.FocusControl(GetID, btnNext.GetID);
      OnScanDone();
      //Recorder.Paused = false;
    }

    public override void Process()
    {
      if (updateList)
      {
        DoUpdateList();
        updateList = false;
      }

      base.Process();
    }

    protected void MapTvToOtherCards(int id)
    {
      ArrayList tvchannels = new ArrayList();
      TVDatabase.GetChannelsForCard(ref tvchannels, id);
      for (int i = 0; i < Recorder.Count; ++i)
      {
        TVCaptureDevice dev = Recorder.Get(i);
        if (dev.Network == Network() && dev.ID != id)
        {
          foreach (TVChannel chan in tvchannels)
          {
            TVDatabase.MapChannelToCard(chan.ID, dev.ID);
          }
        }
      }
    }

    protected void MapRadioToOtherCards(int id)
    {
      ArrayList radioChans = new ArrayList();
      RadioDatabase.GetStationsForCard(ref radioChans, id);
      for (int i = 0; i < Recorder.Count; ++i)
      {
        TVCaptureDevice dev = Recorder.Get(i);

        if (dev.Network == Network() && dev.ID != id)
        {
          foreach (RadioStation chan in radioChans)
          {
            RadioDatabase.MapChannelToCard(chan.ID, dev.ID);
          }
        }
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnNext)
      {
        Log.Info("dvbt-scan:pressed next");
        GUIWizardCardsDetected.ScanNextCardType();
        return;
      }
      base.OnClicked(controlId, control, actionType);
    }

    #region AutoTuneCallback

    public void OnNewChannel()
    {
    }

    public void OnStatus(string description)
    {
      lblStatus.Label = description;
    }

    public void OnStatus2(string description)
    {
      lblChannelsFound.Label = description;
    }

    public void OnProgress(int percentDone)
    {
      progressBar.Percentage = percentDone;
    }

    public void OnEnded()
    {
      _autoTuneRunning = false;
    }

    public void OnSignal(int quality, int strength)
    {
    }

    public void UpdateList()
    {
      updateList = true;
    }

    protected void DoUpdateList()
    {
      listChannelsFound.Clear();
      ArrayList channels = new ArrayList();
      TVDatabase.GetChannels(ref channels);
      if (channels.Count == 0)
      {
        GUIListItem item = new GUIListItem();
        item.Label = "No channels found";
        item.IsFolder = false;
        listChannelsFound.Add(item);
        return;
      }
      int count = 1;
      foreach (TVChannel chan in channels)
      {
        GUIListItem item = new GUIListItem();
        item.Label = String.Format("{0}. {1}", count, chan.Name);
        item.IsFolder = false;
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, chan.Name);
        if (!File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
        item.ThumbnailImage = strLogo;
        item.IconImage = strLogo;
        item.IconImageBig = strLogo;
        listChannelsFound.Add(item);
        count++;
      }
      listChannelsFound.ScrollToEnd();
    }

    #endregion
  }
}