using System;
using System.Collections;
using System.Xml;
using System.Threading;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.Util;
using MediaPortal.GUI.Settings.Wizard;
namespace WindowPlugins.GUISettings.Wizard.DVBT
{
  /// <summary>
  /// Summary description for GUIWizardDVBTCountry.
  /// </summary>
  public class GUIWizardDVBTScan : GUIWindow
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
    int scanOffset = 0;
    ArrayList frequencies = null;
    int currentFrequencyIndex = 0;
    bool updateList = false;
    int newChannels = 0, updatedChannels = 0, newRadioChannels = 0, updatedRadioChannels = 0;

    public GUIWizardDVBTScan()
    {
      GetID = (int)GUIWindow.Window.WINDOW_WIZARD_DVBT_SCAN;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_dvbt_scan.xml");
    }
    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
      frequencies = null;
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

    void LoadFrequencies()
    {
      currentFrequencyIndex = -1;
      frequencies = new ArrayList();
      card = Int32.Parse(GUIPropertyManager.GetProperty("#WizardCard"));
      string country = GUIPropertyManager.GetProperty("#WizardCountry");
      XmlDocument doc = new XmlDocument();
      doc.Load("Tuningparameters/dvbt.xml");
      XmlNodeList countryList = doc.DocumentElement.SelectNodes("/dvbt/country");
      foreach (XmlNode nodeCountry in countryList)
      {
        string name = nodeCountry.Attributes.GetNamedItem(@"name").InnerText;
        if (name == country)
        {
          try
          {
            scanOffset = XmlConvert.ToInt32(nodeCountry.Attributes.GetNamedItem(@"offset").InnerText);
            Log.WriteFile(Log.LogType.Capture, "dvbt-scan:scanoffset: {0} ", scanOffset);
          }
          catch (Exception) { }
          XmlNodeList frequencyList = nodeCountry.SelectNodes("carrier");
          Log.WriteFile(Log.LogType.Capture, "dvbt-scan:number of carriers:{0}", frequencyList.Count);
          int[] carrier;
          foreach (XmlNode node in frequencyList)
          {
            carrier = new int[2];
            carrier[0] = XmlConvert.ToInt32(node.Attributes.GetNamedItem(@"frequency").InnerText);
            try
            {
              carrier[1] = XmlConvert.ToInt32(node.Attributes.GetNamedItem(@"bandwidth").InnerText);
            }
            catch (Exception) { }

            if (carrier[1] == 0) carrier[1] = 8;
            frequencies.Add(carrier);
            Log.WriteFile(Log.LogType.Capture, "dvbt-scan:added:{0}", carrier[0]);
          }
          break;
        }
      }
    }

    public void ScanThread()
    {
      newChannels = 0;
      updatedChannels = 0;
      newRadioChannels = 0;
      updatedRadioChannels = 0;
      LoadFrequencies();
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
      try
      {
        updateList = false;
        if (captureCard == null) return;
        currentFrequencyIndex = 0;
        while (true)
        {
          if (currentFrequencyIndex >= frequencies.Count)
          {
            btnNext.Disabled = false;
            btnBack.Disabled = false;
            return;
          }

          UpdateStatus();
          ScanNextFrequency(captureCard, 0);
          captureCard.Process();

          if (captureCard.SignalPresent() || captureCard.SignalQuality > 50)
          {
            ScanChannels(captureCard);
          }

          if (scanOffset != 0)
          {
            ScanNextFrequency(captureCard, -scanOffset);
            captureCard.Process();

            if (captureCard.SignalPresent() || captureCard.SignalQuality > 50)
            {
              ScanChannels(captureCard);
            }
            ScanNextFrequency(captureCard, scanOffset);
            captureCard.Process();

            if (captureCard.SignalPresent() || captureCard.SignalQuality > 50)
            {
              ScanChannels(captureCard);
            }
          }
          currentFrequencyIndex++;
        }
      }
      finally
      {
        captureCard.DeleteGraph();
        progressBar.Percentage = 100;
        lblChannelsFound.Label = String.Format("Finished, found {0} new tv channels, {1} new radio stations", newChannels, newRadioChannels);
        lblStatus.Label = "Press Next to continue the setup";
        MapTvToOtherCards(captureCard.ID);
        MapRadioToOtherCards(captureCard.ID);
        captureCard = null;
        GUIControl.FocusControl(GetID, btnNext.GetID);
        GUIPropertyManager.SetProperty("#Wizard.DVBT.Done", "yes");

        Recorder.Paused = false;
      }
    }
    void ScanChannels(TVCaptureDevice captureCard)
    {
      Log.Write("dvbt-scan:ScanChannels() {0}/{1}", currentFrequencyIndex, frequencies.Count);
      if (currentFrequencyIndex < 0 || currentFrequencyIndex >= frequencies.Count) return;
      int[] tmp;
      tmp = (int[])frequencies[currentFrequencyIndex];
      string description = String.Format("Found signal at frequency:{0:###.##} MHz. Scanning channels", tmp[0] / 1000);
      lblChannelsFound.Label = description;
      System.Threading.Thread.Sleep(400);
      captureCard.Process();

      Log.Write("ScanChannels() {0} {1}", captureCard.SignalStrength, captureCard.SignalQuality);
      captureCard.StoreTunedChannels(false, true, ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
      updateList = true;
      lblStatus.Label = String.Format("Found {0} tv channels, {1} radio stations", newChannels, newRadioChannels);
      Log.Write("dvbt-scan:ScanChannels() done");
    }

    void ScanNextFrequency(TVCaptureDevice captureCard, int offset)
    {
      Log.Write("dvbt-scan:ScanNextFrequency() {0}/{1}", currentFrequencyIndex, frequencies.Count);
      if (currentFrequencyIndex < 0) currentFrequencyIndex = 0;
      if (currentFrequencyIndex >= frequencies.Count) return;

      DVBChannel chan = new DVBChannel();
      int[] tmp;
      tmp = (int[])frequencies[currentFrequencyIndex];
      chan.Frequency = tmp[0];
      chan.Bandwidth = tmp[1];
      chan.Frequency += offset;

      float frequency = ((float)chan.Frequency) / 1000f;
      string description = String.Format("frequency:{0:###.##} MHz. Bandwidth:{1} MHz", frequency, tmp[1]);

      Log.WriteFile(Log.LogType.Capture, "dvbt-scan:tune:{0} bandwidth:{1} offset:{2}", chan.Frequency, chan.Bandwidth, offset);
      captureCard.Tune(chan, 0);
      captureCard.Process();

      System.Threading.Thread.Sleep(500);
      captureCard.Process();

      System.Threading.Thread.Sleep(500);
      Log.WriteFile(Log.LogType.Capture, "DVBT-scan:tuned locked:{0} level:{1} quality:{2}",
                        captureCard.SignalPresent(), captureCard.SignalStrength, captureCard.SignalQuality);
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
    void UpdateStatus()
    {
      int currentFreq = currentFrequencyIndex;
      if (currentFrequencyIndex < 0) currentFreq = 0;
      float percent = ((float)currentFreq) / ((float)frequencies.Count);
      percent *= 100.0f;

      progressBar.Percentage = (int)percent;
      int[] tmp = frequencies[currentFreq] as int[];
      float frequency = tmp[0];
      frequency /= 1000;
      string description = String.Format("frequency:{0:###.##} MHz. Bandwidth:{1} MHz", frequency, tmp[1]);
      lblChannelsFound.Label = description;
    }
    void MapTvToOtherCards(int id)
    {
      ArrayList tvchannels = new ArrayList();
      TVDatabase.GetChannelsForCard(ref tvchannels, id);
      for (int i = 0; i < Recorder.Count; ++i)
      {
        TVCaptureDevice dev = Recorder.Get(i);
        if (dev.Network == NetworkType.DVBT && dev.ID != id)
        {
          foreach (TVChannel chan in tvchannels)
          {
            TVDatabase.MapChannelToCard(chan.ID, dev.ID);
          }
        }
      }
    }
    void MapRadioToOtherCards(int id)
    {
      ArrayList radioChans = new ArrayList();
      MediaPortal.Radio.Database.RadioDatabase.GetStationsForCard(ref radioChans, id);
      for (int i = 0; i < Recorder.Count; ++i)
      {
        TVCaptureDevice dev = Recorder.Get(i);

        if (dev.Network == NetworkType.DVBT && dev.ID != id)
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
        Log.Write("dvbt-scan:pressed next");
        GUIWizardCardsDetected.ScanNextCardType();
        return;
      }
      base.OnClicked(controlId, control, actionType);
    }
  }
}
