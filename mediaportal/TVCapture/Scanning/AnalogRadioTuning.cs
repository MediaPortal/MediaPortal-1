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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using DirectShowLib;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Scanning
{
	/// <summary>
	/// Class which can search & find all tv channels for an analog capture card
	/// </summary>
	public class AnalogRadioTuning : ITuning
	{
		AutoTuneCallback callback = null;
		private System.Windows.Forms.Timer  timer1;
		TVCaptureDevice	captureCard;
		int currentStationFreq=0;
        int maxStationFreq;
        int minStationFreq;
        bool stopped = true;
        public AnalogRadioTuning()
		{
		}
		#region ITuning Members

		public void Start()
		{
		}
		public void Next()
		{
		}
		public void Previous()
		{
		}
		public void Stop()
		{
            stopped = true;
            timer1.Enabled = false;
			if (captureCard!=null)
			{
				captureCard.DeleteGraph();
			}
            callback.OnSignal(0, 0);
            callback.OnProgress(100);
            callback.OnEnded();
		}
		public bool AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
            captureCard = card;
            card.RadioChannelMinMax(out minStationFreq, out maxStationFreq);
            if (minStationFreq == -1)
            {
                minStationFreq = 87500000;
            }
            else
            {
                minStationFreq = (int)(Math.Floor(((double)minStationFreq / 100000d))) * 100000;
            }
            if (maxStationFreq == -1)
            {
                maxStationFreq = 108000000;
            }
            currentStationFreq = minStationFreq;
            callback = statusCallback;
            callback.OnSignal(0, 0);
            callback.OnProgress(0);
            stopped = false;
            this.timer1 = new System.Windows.Forms.Timer();
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            timer1.Interval = 100;
            timer1.Enabled = true;
            return true;
        }

		public bool AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
            callback.OnEnded();
            return false;
		}
		public void Continue()
		{
            currentStationFreq+=100000;
            if (!stopped)
            {
                timer1.Enabled = true;
            }
        }


		private void timer1_Tick(object sender, System.EventArgs e)
		{
			timer1.Enabled=false;
            if (currentStationFreq <= maxStationFreq)
            {
                float percent = ((float)currentStationFreq - (float)minStationFreq) / ((float)maxStationFreq -(float)minStationFreq);
                percent *= 100.0f;
                callback.OnProgress((int)percent);
                float frequency = ((float)currentStationFreq) / 1000000f;
                string description = String.Format("Radio Station: frequency:{0:###.##} MHz.", frequency);
                callback.OnStatus(description);
                TuneStation();
                int strength = SignalStrength(captureCard.RadioSensitivity);
                callback.OnSignal(strength, strength);
                if (strength == 100)
                {
                    callback.OnNewChannel();
                    return;
                }
                Continue();
            }
            else
            {
                callback.OnSignal(0, 0);
                callback.OnProgress(100);
                callback.OnEnded();
                captureCard.DeleteGraph();
                stopped = true;
                return;
            }
		}
        int SignalStrength(int sensitivity)
        {
            int i = 0;
            for (i = 0; i < sensitivity * 2; i++)
            {
                if (!captureCard.SignalPresent())
                {
                    break;
                }
                System.Threading.Thread.Sleep(50);
            }
            return ((i * 50) / sensitivity);
        }
        void TuneStation()
		{
            captureCard.TuneRadioFrequency(currentStationFreq);
        }
		
		public int MapToChannel(string channelName)
		{
			RadioStation station;
            RadioDatabase.GetStation(channelName, out station);
            station.Frequency=currentStationFreq;
            station.Scrambled = false;
            RadioDatabase.UpdateStation(station);
			RadioDatabase.MapChannelToCard(station.ID,captureCard.ID);
			return station.Channel;
		}

		#endregion
	}
}
