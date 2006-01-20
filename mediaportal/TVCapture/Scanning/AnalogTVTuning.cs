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
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
namespace MediaPortal.TV.Scanning
{
	/// <summary>
	/// Class which can search & find all tv channels for an analog capture card
	/// </summary>
	public class AnalogTVTuning : ITuning
	{
		const int MaxChannelNo=400;
		int	currentChannel=0,maxChannel=0,minChannel=0;
		AutoTuneCallback callback = null;
		private System.Windows.Forms.Timer  timer1;
		TVCaptureDevice	captureCard;
		float lastFrequency=-1f;
        bool stopped = true;
		public AnalogTVTuning()
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
			timer1.Enabled=false;
			if (captureCard!=null)
			{
				captureCard.DeleteGraph();
			}
            callback.OnSignal(0, 0);
            callback.OnProgress(100);
            callback.OnEnded();
        }
		public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
			callback.OnEnded();
		}

		public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
			lastFrequency=-1f;
			captureCard=card;
            card.TVChannelMinMax(out minChannel, out maxChannel);
            if (minChannel == -1)
            {
                minChannel = 1;
            }
            if (maxChannel == -1)
            {
                maxChannel = MaxChannelNo;
            }
            currentChannel = minChannel;
			callback=statusCallback;
            stopped = false;
			this.timer1 = new System.Windows.Forms.Timer();
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			timer1.Interval=100;
			timer1.Enabled=true;
            callback.OnSignal(0, 0);
            callback.OnProgress(0);
		}
		public void Continue()
		{
            if (!stopped)
            {
                timer1.Enabled = true;
            }
            currentChannel++;
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			timer1.Enabled=false;
            if (currentChannel <= maxChannel)
            {
                float percent = (((float)currentChannel)-(float)minChannel) / ((float)maxChannel-(float)minChannel);
                percent *= 100.0f;
                callback.OnProgress((int)percent);
                float frequency = (float)captureCard.VideoFrequency();
                frequency /= 1000000f;
                string description = String.Format("channel:{0} frequency:{1:###.##} MHz.", currentChannel, frequency);
                callback.OnStatus(description);
                TuneChannel();
                callback.OnSignal(captureCard.SignalQuality, captureCard.SignalStrength);
                if (captureCard.SignalPresent())
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
		void TuneChannel()
		{

			TVChannel chan = new TVChannel();
			chan.Number=currentChannel;
			chan.Country=captureCard.DefaultCountryCode;
			chan.TVStandard=AnalogVideoStandard.None;
			if (!captureCard.ViewChannel(chan) )
			{
                callback.OnSignal(0, 0);
                callback.OnProgress(100);
				callback.OnEnded();
				captureCard.DeleteGraph();
                stopped = true;
				return;
			}
		}
		
		public int MapToChannel(string channelName)
		{
      List<TVChannel> channels = new List<TVChannel>();
			TVDatabase.GetChannels(ref channels);
			for (int i=0; i < channels.Count;++i)
			{
				TVChannel chan = channels[i];
				if (chan.Name == channelName)
				{
                    chan.Number = currentChannel;
                    chan.Frequency = captureCard.VideoFrequency();
                    chan.Country = captureCard.DefaultCountryCode;
                    TVDatabase.UpdateChannel(chan, chan.Sort);

                    TVDatabase.MapChannelToCard(chan.ID,captureCard.ID);

					TVGroup group = new TVGroup();
					group.GroupName="Analog";
					int groupid=TVDatabase.AddGroup(group);
					group.ID=groupid;
					TVDatabase.MapChannelToGroup(group,chan);

				}
			}
			return currentChannel;
		}

		#endregion
	}
}
