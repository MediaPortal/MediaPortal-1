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

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Class which can search & find all tv channels for an analog capture card
	/// </summary>
	public class AnalogTVTuning : ITuning
	{
		const int MaxChannelNo=400;
		int																	currentChannel=0;
		AutoTuneCallback										callback = null;
		private System.Windows.Forms.Timer  timer1;
		TVCaptureDevice											captureCard;
		float                               lastFrequency=-1f;

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
			timer1.Enabled=false;
			if (captureCard!=null)
			{
				captureCard.DeleteGraph();
			}
		}
		public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
			callback.OnEnded();
		}

		public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
			lastFrequency=-1f;
			captureCard=card;
			callback=statusCallback;
			this.timer1 = new System.Windows.Forms.Timer();
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			timer1.Interval=100;
			timer1.Enabled=true;
			callback.OnProgress(0);
		}
		public void Continue()
		{
			timer1.Enabled=true;
			NextChannel();
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			timer1.Enabled=false;
			float percent = ((float)currentChannel) / ((float)MaxChannelNo);
			percent *= 100.0f;
			callback.OnProgress((int)percent);
			float frequency=(float)captureCard.VideoFrequency();
			frequency/=1000000f;
			string description=String.Format("channel:{0} frequency:{1:###.##} MHz.", currentChannel, frequency);
			callback.OnStatus(description);

			if (captureCard.SignalPresent())
			{
				callback.OnNewChannel();
				return;
			}
			NextChannel();
			float freq=captureCard.VideoFrequency();
			if ((int)freq == (int)lastFrequency && freq >100f) 
			{
				callback.OnProgress(100);
				callback.OnEnded();
				captureCard.DeleteGraph();
				return;
			}
			lastFrequency=freq;
			timer1.Enabled=true;
		}
		void NextChannel()
		{
			
			currentChannel++;
			if (currentChannel>=MaxChannelNo)
			{
				callback.OnProgress(100);
				callback.OnEnded();
				captureCard.DeleteGraph();
				return;
			}

			TVChannel chan = new TVChannel();
			chan.Number=currentChannel;
			chan.Country=captureCard.DefaultCountryCode;
			chan.TVStandard=AnalogVideoStandard.None;
			if (!captureCard.ViewChannel(chan) )
			{
				callback.OnProgress(100);
				callback.OnEnded();
				captureCard.DeleteGraph();
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
					TVDatabase.SetChannelNumber(chan.Name,currentChannel);
					TVDatabase.SetChannelFrequency(chan.Name,captureCard.VideoFrequency().ToString());
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
