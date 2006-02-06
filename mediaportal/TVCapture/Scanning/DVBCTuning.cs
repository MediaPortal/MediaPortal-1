/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using System.Xml;
using DirectShowLib;
using DirectShowLib.BDA;


namespace MediaPortal.TV.Scanning
{

  /// <summary>
  /// Summary description for DVBCTuning.
  /// </summary>
  public class DVBCTuning : ITuning
  {
    struct DVBCList
    {
      public int frequency;		 // frequency
      public int modulation;	 // modulation
      public string modstr;      // modulation string for display purpose
      public int symbolrate;	 // symbol rate
    }

    enum State
    {
      ScanStart,
      ScanFrequencies,
      ScanChannels
    }
    TVCaptureDevice captureCard;
    AutoTuneCallback callback = null;
    int currentIndex = -1;
    private System.Windows.Forms.Timer timer1;
    DVBCList[] dvbcChannels = new DVBCList[1000];
    int count = 0;

    int newChannels, updatedChannels;
    int newRadioChannels, updatedRadioChannels;
    public DVBCTuning()
    {
    }
    #region ITuning Members
    public void Stop()
    {
      timer1.Enabled = false;
      captureCard.DeleteGraph();
    }
    public void Start()
    {
      currentIndex = -1;
      timer1.Interval = 100;
      timer1.Enabled = true;
      callback.OnProgress(0);
    }

    public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
    {
      newRadioChannels = 0;
      updatedRadioChannels = 0;
      newChannels = 0;
      updatedChannels = 0;
      captureCard = card;
      callback = statusCallback;

      currentIndex = -1;

      OpenFileDialog ofd = new OpenFileDialog();
      ofd.RestoreDirectory = true;
      ofd.InitialDirectory = System.IO.Directory.GetCurrentDirectory() + @"\TuningParameters";
      ofd.Filter = "DVBC-Listings (*.dvbc)|*.dvbc";
      ofd.Title = "Choose DVB-C Listing Files";
      DialogResult res = ofd.ShowDialog();
      if (res != DialogResult.OK) return;

      count = 0;
      string line;
      string[] tpdata;
      Log.WriteFile(Log.LogType.Capture, "dvbc-scan:Opening {0}", ofd.FileName);
      // load dvbcChannelsList list and start scan
      System.IO.TextReader tin = System.IO.File.OpenText(ofd.FileName);

      int LineNr = 0;
      do
      {
        line = null;
        line = tin.ReadLine();
        if (line != null)
        {
          LineNr++;
          if (line.Length > 0)
          {
            if (line.StartsWith(";"))
              continue;
            tpdata = line.Split(new char[] { ',' });
            if (tpdata.Length != 3)
              tpdata = line.Split(new char[] { ';' });
            if (tpdata.Length == 3)
            {
              try
              {
                dvbcChannels[count].frequency = Int32.Parse(tpdata[0]);
                string mod = tpdata[1].ToUpper();
                dvbcChannels[count].modstr = mod;
                switch (mod)
                {
                    case "1024QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_1024QAM;
                        break;
                    case "112QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_112QAM;
                        break;
                    case "128QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_128QAM;
                        break;
                    case "160QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_160QAM;
                        break;
                    case "16QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_16QAM;
                        break;
                    case "16VSB":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_16VSB;
                        break;
                    case "192QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_192QAM;
                        break;
                    case "224QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_224QAM;
                        break;
                    case "256QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_256QAM;
                        break;
                    case "320QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_320QAM;
                        break;
                    case "384QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_384QAM;
                        break;
                    case "448QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_448QAM;
                        break;
                    case "512QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_512QAM;
                        break;
                    case "640QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_640QAM;
                        break;
                    case "64QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_64QAM;
                        break;
                    case "768QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_768QAM;
                        break;
                    case "80QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_80QAM;
                        break;
                    case "896QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_896QAM;
                        break;
                    case "8VSB":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_8VSB;
                        break;
                    case "96QAM":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_96QAM;
                        break;
                    case "AMPLITUDE":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_ANALOG_AMPLITUDE;
                        break;
                    case "FREQUENCY":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_ANALOG_FREQUENCY;
                        break;
                    case "BPSK":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_BPSK;
                        break;
                    case "OQPSK":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_OQPSK;
                        break;
                    case "QPSK":
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_QPSK;
                        break;
                    default:
                        dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_NOT_SET;
                        break;
                }
                dvbcChannels[count].symbolrate = Int32.Parse(tpdata[2]);
                count += 1;
              }
              catch
              {
                Log.WriteFile(Log.LogType.Capture, "dvbc-scan:Error in line:{0}", LineNr);
              }
            }
          }
        }
      } while (!(line == null));
      tin.Close();


      Log.WriteFile(Log.LogType.Capture, "dvbc-scan:loaded:{0} dvbcChannels", count);
      this.timer1 = new System.Windows.Forms.Timer();
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      return;
    }

    public void Next()
    {
      if (currentIndex + 1 >= count) return;
      currentIndex++;

      UpdateStatus();
      ScanDVBCChannel();

      captureCard.Process();
      if (captureCard.SignalPresent())
      {
        ScanChannels();
      }
    }
    public void Previous()
    {
      if (currentIndex >= 1)
      {
        currentIndex--;
        UpdateStatus();
        ScanDVBCChannel();

        captureCard.Process();
        if (captureCard.SignalPresent())
        {
          ScanChannels();
        }
      }
    }

    public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback callback)
    {
      // TODO:  Add DVBCTuning.AutoTuneRadio implementation
    }

    public void Continue()
    {
      // TODO:  Add DVBCTuning.Continue implementation
    }

    public int MapToChannel(string channel)
    {
      // TODO:  Add DVBCTuning.MapToChannel implementation
      return 0;
    }
    void UpdateStatus()
    {
      int index = currentIndex;
      if (index < 0) index = 0;
      float percent = ((float)index) / ((float)count);
      percent *= 100.0f;
      callback.OnProgress((int)percent);
    }

    private void timer1_Tick(object sender, System.EventArgs e)
    {
      timer1.Enabled = false;
      try
      {
        if (currentIndex >= count)
        {
          callback.OnProgress(100);
          callback.OnStatus("Finished");
          callback.OnEnded();
          return;
        }

        UpdateStatus();
        ScanNextDVBCChannel();
        captureCard.Process();
        if (captureCard.SignalPresent())
        {
          ScanChannels();
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      timer1.Enabled = true;
    }

    void ScanChannels()
    {
      DVBCList dvbcChan = dvbcChannels[currentIndex];
      string chanDesc = String.Format("freq:{0} Khz, Mod:{1} SR:{2}", dvbcChan.frequency, dvbcChan.modstr, dvbcChan.symbolrate);
      string description = String.Format("Found signal for channel:{0} {1}, Scanning channels", currentIndex, chanDesc);
      callback.OnStatus(description);
      System.Threading.Thread.Sleep(400);
      captureCard.Process();
      callback.OnSignal(captureCard.SignalQuality, captureCard.SignalStrength);

      callback.OnStatus2(String.Format("new tv:{0} new radio:{1}", newChannels, newRadioChannels));
      captureCard.StoreTunedChannels(false, true, ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
      callback.OnStatus2(String.Format("new tv:{0} new radio:{1}", newChannels, newRadioChannels));
      callback.UpdateList();
      return;
    }

    void ScanNextDVBCChannel()
    {
      currentIndex++;
      ScanDVBCChannel();
      System.Windows.Forms.Application.DoEvents();
    }

    void ScanDVBCChannel()
    {
      if (currentIndex < 0 || currentIndex >= count)
      {
        callback.OnProgress(100);
        callback.OnStatus("Finished");
        callback.OnEnded();
        captureCard.DeleteGraph();
        return;
      }
      string chanDesc = String.Format("freq:{0} Khz, Mod:{1} SR:{2}",
                        dvbcChannels[currentIndex].frequency, dvbcChannels[currentIndex].modstr, dvbcChannels[currentIndex].symbolrate);
      string description = String.Format("Channel:{0}/{1} {2}", currentIndex, count, chanDesc);
      callback.OnStatus(description);

      Log.WriteFile(Log.LogType.Capture, "dvbc-scan:tune dvbcChannel:{0}/{1} {2}", currentIndex, count, chanDesc);

      DVBChannel newchan = new DVBChannel();
      newchan.NetworkID = -1;
      newchan.TransportStreamID = -1;
      newchan.ProgramNumber = -1;

      newchan.Modulation = dvbcChannels[currentIndex].modulation;
      newchan.Symbolrate = (dvbcChannels[currentIndex].symbolrate) / 1000;
      newchan.FEC = (int)FECMethod.MethodNotSet;
      newchan.Frequency = dvbcChannels[currentIndex].frequency;
      captureCard.Tune(newchan, 0);
      captureCard.Process();
      System.Threading.Thread.Sleep(400);

      captureCard.Process();
      if (captureCard.SignalQuality < 40)
        System.Threading.Thread.Sleep(400);
      captureCard.Process();
      callback.OnSignal(captureCard.SignalQuality, captureCard.SignalStrength);
    }
    #endregion
  }
}
