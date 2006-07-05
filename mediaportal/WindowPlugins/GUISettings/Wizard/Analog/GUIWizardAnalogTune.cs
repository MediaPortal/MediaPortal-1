using System;
using System.Collections;
using System.Xml;
using System.Threading;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.Util;
using MediaPortal.GUI.Settings.Wizard;
using DShowNET;
using DirectShowLib;
namespace WindowPlugins.GUISettings.Wizard.Analog
{
  /// <summary>
  /// Summary description for GUIWizardAnalogTune.
  /// </summary>
  public class GUIWizardAnalogTune : GUIWindow
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

    int currentFrequencyIndex = 0;
    bool updateList = false;
    int newChannels = 0;
    static ArrayList listTvChannels = new ArrayList();

    public GUIWizardAnalogTune()
    {
      GetID = (int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_TUNE;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_analog_scan.xml");
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
      listTvChannels.Clear();
      newChannels = 0; ;
      TVCaptureDevice captureCard = null;
      card = Int32.Parse(GUIPropertyManager.GetProperty("#WizardCard"));
      if (card >= 0 && card < Recorder.Count)
      {
        captureCard = Recorder.Get(card);
        captureCard.StartViewing("test");
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
        if (captureCard == null) return;
        currentFrequencyIndex = 4;
        while (true)
        {
          if (currentFrequencyIndex >= 200)
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
          currentFrequencyIndex++;
        }
      }
      catch (Exception ex)
      {
        _log.Error("ex:{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
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
    void ScanChannels(TVCaptureDevice captureCard)
    {
      _log.Info("Analog-scan:ScanChannels() {0}/{1}", currentFrequencyIndex, 200);
      if (currentFrequencyIndex < 0 || currentFrequencyIndex >= 200) return;


      System.Threading.Thread.Sleep(400);
      TVChannel chan = new TVChannel();
      chan.Name = String.Format("Channel{0}", currentFrequencyIndex);
      chan.Number = currentFrequencyIndex;
      listTvChannels.Add(chan);
      updateList = true;
      newChannels++;
    }

    void ScanNextFrequency(TVCaptureDevice captureCard, int offset)
    {
      _log.Info("Analog-scan:ScanNextFrequency() {0}/{1}", currentFrequencyIndex, 200);
      if (currentFrequencyIndex < 0) currentFrequencyIndex = 0;
      if (currentFrequencyIndex >= 200) return;


      TVChannel chan = new TVChannel();
      chan.Number = currentFrequencyIndex;
      chan.Country = captureCard.DefaultCountryCode;
      chan.TVStandard = AnalogVideoStandard.None;

      System.Threading.Thread.Sleep(400);
      if (!captureCard.SignalPresent())
        System.Threading.Thread.Sleep(400);

      _log.Info("Analog-scan:tune:{0}", currentFrequencyIndex);


      captureCard.ViewChannel(chan);
      _log.Info("Analog-scan:tuned");
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


    void UpdateList()
    {
      _log.Info("UpdateList()");
      listChannelsFound.Clear();
      if (listTvChannels.Count == 0)
      {
        GUIListItem item = new GUIListItem();
        item.Label = "No channels found";
        item.IsFolder = false;
        listChannelsFound.Add(item);
        _log.Info("UpdateList() done");
        return;

      }
      int count = 1;
      foreach (TVChannel chan in listTvChannels)
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
      _log.Info("UpdateList() done");
    }
    void UpdateStatus()
    {
      int currentFreq = currentFrequencyIndex;
      if (currentFrequencyIndex < 0) currentFreq = 0;
      float percent = ((float)currentFreq) / ((float)200);
      percent *= 100.0f;

      progressBar.Percentage = (int)percent;
      string description = String.Format("Channel:{0}", currentFreq);
      lblChannelsFound.Label = description;
      lblStatus.Label = String.Format("Found {0} tv channels", newChannels);
      _log.Info("Analog-scan:ScanChannels() done");
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == btnNext)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_RENAME);
        return;
      }
      base.OnClicked(controlId, control, actionType);
    }
    static public ArrayList TVChannelsFound
    {
      get { return listTvChannels; }
    }
  }
}
