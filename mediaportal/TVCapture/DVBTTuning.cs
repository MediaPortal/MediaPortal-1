/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.GUI.Library;
using System.Xml;

namespace MediaPortal.TV.Recording
{

  /// <summary>
  /// Summary description for DVBTTuning.
  /// </summary>
  public class DVBTTuning : ITuning
  {
    TVCaptureDevice captureCard;
    AutoTuneCallback callback = null;
    ArrayList frequencies = new ArrayList();
    int currentFrequencyIndex = 0;
    int scanOffset = 0;
    private System.Windows.Forms.Timer timer1;
    int newChannels, updatedChannels;
    int newRadioChannels, updatedRadioChannels;

    bool reentrant = false;
    public DVBTTuning()
    {
    }
    #region ITuning Members
    public void Stop()
    {
      timer1.Enabled = false;
      captureCard.DeleteGraph();
    }
    public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
    {
      newRadioChannels = 0;
      updatedRadioChannels = 0;
      newChannels = 0;
      updatedChannels = 0;

      captureCard = card;
      callback = statusCallback;

      frequencies.Clear();
      currentFrequencyIndex = -1;
      String countryCode = String.Empty;

      Log.WriteFile(Log.LogType.Capture, "dvbt-scan:Opening dvbt.xml");
      XmlDocument doc = new XmlDocument();
      doc.Load("Tuningparameters/dvbt.xml");

      FormCountry formCountry = new FormCountry();
      XmlNodeList countryList = doc.DocumentElement.SelectNodes("/dvbt/country");
      foreach (XmlNode nodeCountry in countryList)
      {
        string name = nodeCountry.Attributes.GetNamedItem(@"name").InnerText;
        formCountry.AddCountry(name);
      }
      formCountry.ShowDialog();
      string countryName = formCountry.countryName;
      if (countryName == String.Empty) return;
      Log.WriteFile(Log.LogType.Capture, "dvbt-scan:auto tune for {0}", countryName);
      frequencies.Clear();

      countryList = doc.DocumentElement.SelectNodes("/dvbt/country");
      foreach (XmlNode nodeCountry in countryList)
      {
        string name = nodeCountry.Attributes.GetNamedItem(@"name").InnerText;
        if (name != countryName) continue;
        Log.WriteFile(Log.LogType.Capture, "dvbt-scan:found country {0} in dvbt.xml", countryName);
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
      }
      if (frequencies.Count == 0) return;

      Log.WriteFile(Log.LogType.Capture, "dvbt-scan:loaded:{0} frequencies", frequencies.Count);
      Log.WriteFile(Log.LogType.Capture, "dvbt-scan:{0} has a scan offset of {1}KHz", countryCode, scanOffset);
      this.timer1 = new System.Windows.Forms.Timer();
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      return;
    }

    public void Start()
    {
      currentFrequencyIndex = -1;
      timer1.Interval = 100;
      timer1.Enabled = true;
      callback.OnProgress(0);
    }

    public void Next()
    {
      if (currentFrequencyIndex + 1 >= frequencies.Count) return;
      currentFrequencyIndex++;
      UpdateStatus();

      ScanNextFrequency(0);
      if (captureCard.SignalPresent())
      {
        ScanChannels();
      }
    }
    public void Previous()
    {
      if (currentFrequencyIndex >= 1)
      {
        currentFrequencyIndex--;

        UpdateStatus();
        ScanNextFrequency(0);
        if (captureCard.SignalPresent() )
        {
          ScanChannels();
        }
      }
    }
    public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback callback)
    {
      // TODO:  Add DVBTTuning.AutoTuneRadio implementation
    }

    public void Continue()
    {
      // TODO:  Add DVBTTuning.Continue implementation
    }

    public int MapToChannel(string channel)
    {
      // TODO:  Add DVBTTuning.MapToChannel implementation
      return 0;
    }

    void UpdateStatus()
    {
      int currentFreq = currentFrequencyIndex;
      if (currentFrequencyIndex < 0) currentFreq = 0;
      float percent = ((float)currentFreq) / ((float)frequencies.Count);
      percent *= 100.0f;
      callback.OnProgress((int)percent);
      int[] tmp = frequencies[currentFreq] as int[];
      //Log.WriteFile(Log.LogType.Capture,"dvbt-scan:FREQ: {0} BWDTH: {1}", tmp[0], tmp[1]);
      float frequency = tmp[0];
      frequency /= 1000;
      string description = String.Format("frequency:{0:###.##} MHz. Bandwidth:{1} MHz", frequency, tmp[1]);
      callback.OnStatus(description);
    }
    private void timer1_Tick(object sender, System.EventArgs e)
    {
      if (reentrant) return;
      try
      {
        reentrant = true;
        if (currentFrequencyIndex >= frequencies.Count)
        {
          Log.Write("dvbt-scan:finished");
          callback.OnProgress(100);
          callback.OnEnded();
          callback.OnStatus("Finished");
          return;
        }

        UpdateStatus();
        ScanNextFrequency(0);
        if (captureCard.SignalPresent())
        {
          ScanChannels();
        }

        if (scanOffset != 0)
        {
          ScanNextFrequency(-scanOffset);
          if (captureCard.SignalPresent() )
          {
            ScanChannels();
          }
          ScanNextFrequency(scanOffset);
          if (captureCard.SignalPresent() )
          {
            ScanChannels();
          }
        }
        currentFrequencyIndex++;
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"Exception:{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
      finally
      {
        if (currentFrequencyIndex >= frequencies.Count)
        {
          timer1.Enabled = false;
        }
        reentrant = false;
      }
    }

    void ScanChannels()
    {
      Log.Write("dvbt-scan:ScanChannels() {0}/{1}", currentFrequencyIndex, frequencies.Count);
      if (currentFrequencyIndex < 0 || currentFrequencyIndex >= frequencies.Count) return;
      int[] tmp;
      tmp = (int[])frequencies[currentFrequencyIndex];
      captureCard.Process();
      string description = String.Format("Found signal at frequency:{0:###.##} MHz. Scanning channels", tmp[0] / 1000);
      callback.OnStatus(description);
      System.Threading.Thread.Sleep(400);
      captureCard.Process();
      callback.OnSignal(captureCard.SignalQuality, captureCard.SignalStrength);
      Log.Write("ScanChannels() {0} {1}", captureCard.SignalStrength, captureCard.SignalQuality);
      callback.OnStatus2(String.Format("new tv:{0} new radio:{1}", newChannels, newRadioChannels));
      captureCard.StoreTunedChannels(false, true, ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
      callback.OnStatus2(String.Format("new tv:{0} new radio:{1}", newChannels, newRadioChannels));
      callback.UpdateList();
      Log.Write("dvbt-scan:ScanChannels() done");
    }

    void ScanNextFrequency(int offset)
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
      chan.NetworkID = -1;
      chan.TransportStreamID = -1;
      chan.ProgramNumber = -1;

      float frequency = ((float)chan.Frequency) / 1000f;
      string description = String.Format("frequency:{0:###.##} MHz. Bandwidth:{1} MHz", frequency, tmp[1]);
      callback.OnStatus(description);

      Log.WriteFile(Log.LogType.Capture, "dvbt-scan:tune to freq:{0} bandwidth:{1} offset:{2}", chan.Frequency, chan.Bandwidth, offset);
      captureCard.Tune(chan, 0);
      System.Threading.Thread.Sleep(500);
      captureCard.Process();
        System.Threading.Thread.Sleep(500);
      captureCard.Process();
      callback.OnSignal(captureCard.SignalQuality, captureCard.SignalStrength);
      captureCard.Process();
      Log.WriteFile(Log.LogType.Capture, "dvbt-scan:locked:{0} tuned quality:{1} strength:{2}",
              captureCard.SignalPresent(),captureCard.SignalQuality, captureCard.SignalStrength);
      return;
    }

    #endregion
  }
}
