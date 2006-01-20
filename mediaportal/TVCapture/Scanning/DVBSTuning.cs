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


namespace MediaPortal.TV.Scanning
{

  /// <summary>
  /// Summary description for DVBSTuning.
  /// </summary>
  public class DVBSTuning : ITuning
  {
    struct TPList
    {
      public int TPfreq; // frequency
      public int TPpol;  // polarisation 0=hori, 1=vert
      public int TPsymb; // symbol rate
    }
    TVCaptureDevice captureCard;
    AutoTuneCallback callback = null;
    int currentIndex = -1;
    private System.Windows.Forms.Timer timer1;
    TPList[] transp = new TPList[800];
    int count = 0;
    string[] tplFiles;
    int newChannels, updatedChannels;
    int newRadioChannels, updatedRadioChannels;
    int m_diseqcLoops = 1;
    int m_currentDiseqc = 1;
    bool reentrant = false;

    public DVBSTuning()
    {
    }
    #region ITuning Members

    public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
    {
      newRadioChannels = 0;
      updatedRadioChannels = 0;
      newChannels = 0;
      updatedChannels = 0;

      captureCard = card;
      callback = statusCallback;
      string filename = String.Format(@"database\card_{0}.xml", card.FriendlyName);
      //
      // load card settings to check diseqc
      m_diseqcLoops = 1;
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(filename))
      {
        if (xmlreader.GetValueAsBool("dvbs", "useLNB2", false) == true)
          m_diseqcLoops++;
        if (xmlreader.GetValueAsBool("dvbs", "useLNB3", false) == true)
          m_diseqcLoops++;
        if (xmlreader.GetValueAsBool("dvbs", "useLNB4", false) == true)
          m_diseqcLoops++;
      }

      DVBSSelectTPLForm dlgSelectTPL = new DVBSSelectTPLForm();
      dlgSelectTPL.NumberOfLNBs = m_diseqcLoops;
      dlgSelectTPL.ShowDialog();
      tplFiles = dlgSelectTPL.TransponderFiles;

      m_currentDiseqc = 1;

    }

    void LoadFrequencies()
    {
      currentIndex = -1;

      string fileName = tplFiles[m_currentDiseqc - 1];
      if (fileName == String.Empty)
      {
        currentIndex = count + 1;
        return;
      }
      count = 0;
      string line;
      string[] tpdata;
      Log.WriteFile(Log.LogType.Capture, "dvbs-scan:Opening {0}", fileName);
      // load transponder list and start scan
      System.IO.TextReader tin = System.IO.File.OpenText(fileName);

      do
      {
        line = null;
        line = tin.ReadLine();
        if (line != null)
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

                transp[count].TPfreq = Int32.Parse(tpdata[0]) * 1000;
                switch (tpdata[1].ToLower())
                {
                  case "v":

                    transp[count].TPpol = 1;
                    break;
                  case "h":

                    transp[count].TPpol = 0;
                    break;
                  default:

                    transp[count].TPpol = 0;
                    break;
                }
                transp[count].TPsymb = Int32.Parse(tpdata[2]);
                count += 1;
              }
              catch
              { }
            }
          }
      } while (!(line == null));
      tin.Close();


      Log.WriteFile(Log.LogType.Capture, "dvbs-scan:loaded:{0} transponders", count);
      return;
    }
    public void Start()
    {
      m_currentDiseqc = 1;
      currentIndex = -1;
      LoadFrequencies();
      this.timer1 = new System.Windows.Forms.Timer();
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      timer1.Interval = 100;
      timer1.Enabled = true;
      callback.OnProgress(0);
    }

    public void Next()
    {
      if (currentIndex + 1 >= count) return;
      currentIndex++;
      UpdateStatus();
      ScanTransponder();
      captureCard.Process();
      if (captureCard.SignalPresent())
      {
        ScanChannels();
      }
    }
    public void Previous()
    {
      if (currentIndex > 1)
      {
        currentIndex -= 2;
        UpdateStatus();

        ScanTransponder();
        captureCard.Process();
        if (captureCard.SignalPresent())
        {
          ScanChannels();
        }
      }
    }
    public void Stop()
    {
      if (timer1 != null)
      {
        count = 0;
        currentIndex = 1;
        m_currentDiseqc = 10;
        timer1.Enabled = false;
      }
      captureCard.DeleteGraph();
    }
    public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback callback)
    {
      // TODO:  Add DVBSTuning.AutoTuneRadio implementation
    }

    public void Continue()
    {
      // TODO:  Add DVBSTuning.Continue implementation
    }

    public int MapToChannel(string channel)
    {
      // TODO:  Add DVBSTuning.MapToChannel implementation
      return 0;
    }

    void UpdateStatus()
    {
      int index = currentIndex;
      if (index < 0) index = 0;

      float percent = ((float)index) / ((float)count);
      percent *= 100.0f;
      callback.OnProgress((int)percent);
      TPList transponder = transp[index];
      string chanDesc = String.Format("freq:{0} Khz, Pol:{1} SR:{2}",
        transponder.TPfreq, transponder.TPpol, transponder.TPsymb);
      string description = String.Format("Transponder:{0}/{1} {2}", index, count, chanDesc);
      callback.OnStatus(description);
    }
    private void timer1_Tick(object sender, System.EventArgs e)
    {
      try
      {
        if (reentrant) return;
        reentrant = true;

        timer1.Enabled = false;
        if (currentIndex >= count)
        {
          if (m_currentDiseqc >= m_diseqcLoops)
          {
            Log.WriteFile(Log.LogType.Capture, "dvbs-scan:finished");
            callback.OnProgress(100);
            callback.OnStatus("Finished");
            callback.OnEnded();
            captureCard.DeleteGraph();
            return;
          }
        }
        UpdateStatus();
        ScanNextTransponder();
        captureCard.Process();
        if (captureCard.SignalPresent())
        {
          Log.WriteFile(Log.LogType.Capture, "dvbs-scan:Found signal for transponder:{0}", currentIndex);
          ScanChannels();
        }
      }
      catch (Exception ex)
      {
        Log.Write("Exception:{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
      finally
      {
        reentrant = false;
        timer1.Enabled = true;
      }
    }

    void ScanChannels()
    {
      string description = String.Format("Found signal for transponder:{0}, Scanning channels", currentIndex);
      callback.OnStatus(description);
      System.Threading.Thread.Sleep(400);
      captureCard.Process();
      callback.OnSignal(captureCard.SignalQuality, captureCard.SignalStrength);

      callback.OnStatus2(String.Format("new tv:{0} new radio:{1}", newChannels, newRadioChannels));
      captureCard.StoreTunedChannels(false, true, ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
      callback.OnStatus2(String.Format("new tv:{0} new radio:{1}", newChannels, newRadioChannels));

      captureCard.Process();
      callback.UpdateList();
      Log.WriteFile(Log.LogType.Capture, "dvbs-scan:onto next transponder");
    }

    void ScanNextTransponder()
    {
      currentIndex++;
      if (currentIndex >= count)
      {
        if (m_currentDiseqc >= m_diseqcLoops)
        {
          Log.WriteFile(Log.LogType.Capture, "dvbs-scan:finished");
          callback.OnProgress(100);
          callback.OnStatus("Finished");
          callback.OnEnded();
          captureCard.DeleteGraph();
          return;
        }
        else
        {
          m_currentDiseqc++;
          LoadFrequencies();
          currentIndex = 0;
        }
      }
      ScanTransponder();
    }

    void ScanTransponder()
    {
      if (currentIndex < 0 || currentIndex >= count) return;
      DVBChannel newchan = new DVBChannel();
      newchan.NetworkID = -1;
      newchan.TransportStreamID = -1;
      newchan.ProgramNumber = -1;

      newchan.Polarity = transp[currentIndex].TPpol;
      newchan.Symbolrate = transp[currentIndex].TPsymb;
      newchan.FEC = (int)TunerLib.FECMethod.BDA_FEC_METHOD_NOT_DEFINED;
      newchan.Frequency = transp[currentIndex].TPfreq;




      Log.WriteFile(Log.LogType.Capture, "dvbs-scan:tune transponder:{0} freq:{1} KHz symbolrate:{2} polarisation:{3}", currentIndex,
                  newchan.Frequency, newchan.Symbolrate, newchan.Polarity);
      captureCard.Tune(newchan, m_currentDiseqc);
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
