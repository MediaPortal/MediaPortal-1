using System;
using System.Collections;
using System.Xml;
using System.Threading;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Scanning;
using MediaPortal.Util;
using MediaPortal.GUI.Settings.Wizard;

namespace WindowPlugins.GUISettings.Wizard
{
  /// <summary>
  /// Summary description for GUIWizardDVBTCountry.
  /// </summary>
  public class GUIWizardScanBase : GUIWindow, AutoTuneCallback
  {
    [SkinControlAttribute(26)]
    protected GUILabelControl lblChannelsFound = null;
    [SkinControlAttribute(27)]
    protected GUILabelControl lblStatus = null;
    [SkinControlAttribute(24)]
    protected GUIListControl listChannelsFound = null;
    [SkinControlAttribute(5)]
    protected GUIButtonControl btnNext = null;
    [SkinControlAttribute(25)]
    protected GUIButtonControl btnBack = null;
    [SkinControlAttribute(20)]
    protected GUIProgressControl progressBar = null;

    int card = 0;
    bool updateList = false;

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

    protected virtual ITuning GetTuningInterface(TVCaptureDevice captureCard)
    {
      return null;
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
      Recorder.Paused = true;
      TVCaptureDevice captureCard = null;
      if (card >= 0 && card < Recorder.Count)
      {
        captureCard = Recorder.Get(card);
        captureCard.CreateGraph();
      }
      else
      {
        btnNext.Disabled = false;
        btnBack.Disabled = false;
        return;
      }
      updateList = false;
      ITuning tuning = GetTuningInterface(captureCard);
      tuning.Start();
      while (!tuning.IsFinished())
      {
        tuning.Next();
      }

      btnNext.Disabled = false;
      btnBack.Disabled = false;
      captureCard.DeleteGraph();
      progressBar.Percentage = 100;
      lblStatus.Label = "Press Next to continue the setup";
      MapTvToOtherCards(captureCard.ID);
      MapRadioToOtherCards(captureCard.ID);
      captureCard = null;
      GUIControl.FocusControl(GetID, btnNext.GetID);
      OnScanDone();
      Recorder.Paused = false;
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
      MediaPortal.Radio.Database.RadioDatabase.GetStationsForCard(ref radioChans, id);
      for (int i = 0; i < Recorder.Count; ++i)
      {
        TVCaptureDevice dev = Recorder.Get(i);

        if (dev.Network == Network() && dev.ID != id)
        {
          foreach (MediaPortal.Radio.Database.RadioStation chan in radioChans)
          {
            MediaPortal.Radio.Database.RadioDatabase.MapChannelToCard(chan.ID, dev.ID);
          }
        }
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == btnNext)
      {
        _log.Info("dvbt-scan:pressed next");
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
        string strLogo = MediaPortal.Util.Utils.GetCoverArt(Thumbs.TVChannel, chan.Name);
        if (!System.IO.File.Exists(strLogo))
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
