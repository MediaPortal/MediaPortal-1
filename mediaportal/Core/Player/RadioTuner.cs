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
using System.Diagnostics;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
//using DirectX.Capture;

namespace MediaPortal.Player
{
  /// <summary>
  /// 
  /// </summary>
  public class RadioTuner : IPlayer
  {
    private RadioGraph m_capture = null;
    private string m_strFile;
    private bool m_bInternal = false;
    private Process m_player = null;
    private DateTime m_dtTime = new DateTime();
    private string m_strRadioDevice = "";
    private string m_strAudioDevice = "";
    private string m_strLineInput = "";

    public RadioTuner()
    {
    }

    public override bool Play(string strFile)
    {
      int iChannel = Convert.ToInt32(Path.GetFileNameWithoutExtension(strFile));

      Log.Info("Radiotuner: tune to channel:{0}", iChannel);
      try
      {
        string strPlayerFile = "";
        string strPlayerArgs = "";

        int iTunerCountry = 31;
        string strTunerType = "Antenna";
        using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          m_bInternal = xmlreader.GetValueAsBool("radio", "internal", true);
          strPlayerFile = xmlreader.GetValueAsString("radio", "player", "");
          strPlayerArgs = xmlreader.GetValueAsString("radio", "args", "f %MHZ%");
          strTunerType = xmlreader.GetValueAsString("radio", "tuner", "Antenna");
          iTunerCountry = xmlreader.GetValueAsInt("capture", "country", 31);
          m_strRadioDevice = xmlreader.GetValueAsString("radio", "device", "");
          m_strAudioDevice = xmlreader.GetValueAsString("radio", "audiodevice", "");
          m_strLineInput = xmlreader.GetValueAsString("radio", "lineinput", "");
        }

        m_dtTime = DateTime.Now;
        if (!m_bInternal)
        {
          double dFreq = (double) iChannel;
          dFreq /= 1000000d;
          string strFreq = dFreq.ToString();

          strFreq = strFreq.Replace(",", ".");
          strPlayerArgs = strPlayerArgs.Replace("%MHZ%", strFreq);

          strFreq = strFreq.Replace(".", ",");
          strPlayerArgs = strPlayerArgs.Replace("%mhz%", strFreq);

          strFreq = iChannel.ToString();
          strPlayerArgs = strPlayerArgs.Replace("%HZ%", strFreq);

          m_player = Util.Utils.StartProcess(strPlayerFile, strPlayerArgs, false, true);
          if (m_player == null)
          {
            Log.Info("Unable to start external radio player:{0} {1}", strPlayerFile, strPlayerArgs);
            return false;
          }

          m_strFile = strFile;
          return true;
        }

        // alloc card.
        AllocCard(m_strRadioDevice);
        bool bAntenna = false;
        if (strTunerType.Equals("Antenna"))
        {
          bAntenna = true;
        }

        m_capture = new RadioGraph(m_strRadioDevice, m_strAudioDevice, m_strLineInput);
        if (!m_capture.Create(!bAntenna, 0, iTunerCountry))
        {
          Log.Info("RadioTuner:failed to Tune to channel:{0}", iChannel);
          m_capture.DeleteGraph();
          m_capture = null;
          FreeCard(m_strRadioDevice);
          return false;
        }
        m_capture.Tune(iChannel);

        Log.Info("RadioTuner:Frequency:{0} Hz tuned to:{1} Hz", m_capture.Channel, m_capture.AudioFrequency);
      }
      catch (Exception ex)
      {
        if (m_strRadioDevice != "B2C2 MPEG-2 Source")
        {
          Log.Info("RadioTuner:failed to Tune to channel:{0} {1} {2}", iChannel, ex.Message, ex.StackTrace);
          m_capture.DeleteGraph();
          m_capture = null;
          FreeCard(m_strRadioDevice);
          return false;
        }
      }
      m_strFile = strFile;
      return true;
    }

    public override bool Playing
    {
      get
      {
        if (m_bInternal)
        {
          if (m_capture != null)
          {
            return true;
          }
          return false; //TEST
        }
        else
        {
          return (m_player != null);
        }
      }
    }

    public override void Stop()
    {
      if (m_bInternal)
      {
        if (m_capture != null)
        {
          m_capture.DeleteGraph();
        }
        m_capture = null;
        FreeCard(m_strRadioDevice);
      }
      else
      {
        if (m_player != null)
        {
          try
          {
            m_player.CloseMainWindow();
          }
          catch (Exception)
          {
          }
          if (!m_player.HasExited)
          {
            try
            {
              m_player.Kill();
            }
            catch (Exception)
            {
            }
          }
          m_player = null;
        }
      }
    }


    public override string CurrentFile
    {
      get { return m_strFile; }
    }

    public override bool IsRadio
    {
      get { return true; }
    }

    public override double CurrentPosition
    {
      get
      {
        TimeSpan ts = DateTime.Now - m_dtTime;
        return (double) ts.TotalSeconds;
      }
    }

    public override void Process()
    {
      if (!m_bInternal)
      {
        TimeSpan ts = DateTime.Now - m_dtTime;
        if (ts.TotalSeconds < 5)
        {
          GUIGraphicsContext.form.Activate();
        }
      }
    }

    private void AllocCard(string strDevice)
    {
      if (strDevice == null)
      {
        return;
      }
      if (strDevice.Length == 0)
      {
        return;
      }
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_ALLOC_CARD, 0, 0, 0, 0, 0, null);
      msg.Label = strDevice;
      GUIWindowManager.SendMessage(msg);
      GC.Collect();
      GC.Collect();
      GC.Collect();
    }

    private void FreeCard(string strDevice)
    {
      if (strDevice == null)
      {
        return;
      }
      if (strDevice.Length == 0)
      {
        return;
      }
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_FREE_CARD, 0, 0, 0, 0, 0, null);
      msg.Label = strDevice;
      GUIWindowManager.SendMessage(msg);
    }
  }
}