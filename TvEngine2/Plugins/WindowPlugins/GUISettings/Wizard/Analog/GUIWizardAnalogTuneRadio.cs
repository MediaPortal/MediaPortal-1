#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using MediaPortal.Radio.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Util;

namespace WindowPlugins.GUISettings.Wizard.Analog
{
  /// <summary>
  /// Summary description for GUIWizardAnalogTuneRadio.
  /// </summary>
  public class GUIWizardAnalogTuneRadio : GUIWindow
  {
    [SkinControl(26)] protected GUILabelControl lblChannelsFound = null;
    [SkinControl(27)] protected GUILabelControl lblStatus = null;
    [SkinControl(24)] protected GUIListControl listChannelsFound = null;
    [SkinControl(5)] protected GUIButtonControl btnNext = null;
    [SkinControl(25)] protected GUIButtonControl btnBack = null;
    [SkinControl(20)] protected GUIProgressControl progressBar = null;

    private int card = 0;

    private int currentFrequencyIndex = 0;
    private bool updateList = false;
    private int newChannels = 0;
    private static ArrayList listRadioChannels = new ArrayList();

    private const int minFreq = 87500000;
    private const int maxFreq = 108000000;
    private const int stepFreq = 100000;

    public GUIWizardAnalogTuneRadio()
    {
      GetID = (int) Window.WINDOW_WIZARD_ANALOG_SCAN_RADIO;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_analog_scanradio_TVE2.xml");
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
      UpdateList();
      Thread WorkerThread = new Thread(new ThreadStart(ScanThread));
      WorkerThread.SetApartmentState(ApartmentState.STA);
      //WorkerThread.IsBackground=true;
      WorkerThread.Start();
    }

    public void ScanThread()
    {
      Recorder.Paused = true;
      listRadioChannels.Clear();
      newChannels = 0;
      ;
      TVCaptureDevice captureCard = null;
      card = Int32.Parse(GUIPropertyManager.GetProperty("#WizardCard"));
      if (card >= 0 && card < Recorder.Count)
      {
        captureCard = Recorder.Get(card);
      }
      else
      {
        btnNext.Disabled = false;
        btnBack.Disabled = false;
        return;
      }
      try
      {
        updateList = false;
        if (captureCard == null)
        {
          return;
        }
        currentFrequencyIndex = minFreq;
        while (true)
        {
          if (currentFrequencyIndex >= maxFreq)
          {
            btnNext.Disabled = false;
            btnBack.Disabled = false;
            return;
          }

          UpdateStatus();
          ScanNextFrequency(captureCard, 0);
          if (captureCard.SignalPresent())
          {
            ScanChannels(captureCard);
          }
          currentFrequencyIndex += stepFreq;
        }
      }
      catch (Exception ex)
      {
        Log.Error("ex:{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
      finally
      {
        captureCard.DeleteGraph();
        progressBar.Percentage = 100;
        lblChannelsFound.Label = String.Format("Finished, found {0} tv channels", newChannels);
        lblStatus.Label = "Press Next to continue the setup";
        GUIControl.FocusControl(GetID, btnNext.GetID);
        captureCard = null;

        Recorder.Paused = false;
      }
    }

    private void ScanChannels(TVCaptureDevice captureCard)
    {
      Log.Info("Analog-scan:ScanChannels() {0}/{1}", currentFrequencyIndex, 200);
      if (currentFrequencyIndex < 0 || currentFrequencyIndex >= 200)
      {
        return;
      }


      Thread.Sleep(400);
      RadioStation chan = new RadioStation();
      chan.Name = String.Format("Channel{0}", currentFrequencyIndex);
      chan.Frequency = currentFrequencyIndex;
      listRadioChannels.Add(chan);
      updateList = true;
      newChannels++;
    }

    private void ScanNextFrequency(TVCaptureDevice captureCard, int offset)
    {
      Log.Info("Analog-scan:ScanNextFrequency() {0}/{1}", currentFrequencyIndex, maxFreq);
      if (currentFrequencyIndex < minFreq)
      {
        currentFrequencyIndex = minFreq;
      }
      if (currentFrequencyIndex >= maxFreq)
      {
        return;
      }


      Thread.Sleep(400);
      if (!captureCard.SignalPresent())
      {
        Thread.Sleep(400);
      }

      Log.Info("Analog-scan:tune:{0}", currentFrequencyIndex);

      RadioStation station = new RadioStation();
      float freq = (float) currentFrequencyIndex;
      freq /= 1000000f;
      station.Name = String.Format("{0} FM", freq.ToString("f2"));
      station.Frequency = currentFrequencyIndex;
      captureCard.StartRadio(station);
      Log.Info("Analog-scan:tuned");
      return;
    }

    public override void Process()
    {
      if (updateList)
      {
        UpdateList();
        updateList = false;
      }

      base.Process();
    }


    private void UpdateList()
    {
      Log.Info("UpdateList()");
      listChannelsFound.Clear();
      if (listRadioChannels.Count == 0)
      {
        GUIListItem item = new GUIListItem();
        item.Label = "No stations found";
        item.IsFolder = false;
        listChannelsFound.Add(item);
        Log.Info("UpdateList() done");
        return;
      }
      int count = 1;
      foreach (RadioStation chan in listRadioChannels)
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
      Log.Info("UpdateList() done");
    }

    private void UpdateStatus()
    {
      int currentFreq = currentFrequencyIndex;
      if (currentFrequencyIndex < minFreq)
      {
        currentFreq = minFreq;
      }
      float percent = ((float) (currentFreq - minFreq))/((float) (maxFreq - minFreq));
      percent *= 100.0f;

      progressBar.Percentage = (int) percent;
      float freq = (float) currentFreq;
      freq /= 1000000f;
      string description = String.Format("{0} FM", freq.ToString("f2"));
      lblChannelsFound.Label = description;
      lblStatus.Label = String.Format("Found {0} radio stations", newChannels);
      Log.Info("Analog-scan:ScanChannels() done");
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnNext)
      {
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_WIZARD_ANALOG_RENAME_RADIO);
        return;
      }
      base.OnClicked(controlId, control, actionType);
    }

    public static ArrayList RadioStationsFound
    {
      get { return listRadioChannels; }
    }
  }
}