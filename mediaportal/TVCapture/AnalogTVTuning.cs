using System;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Class which can search & find all tv channels for an analog capture card
	/// </summary>
	public class AnalogTVTuning : ITuning
	{
		int																	currentChannel=1;
		AutoTuneCallback										callback = null;
		private System.Windows.Forms.Timer  timer1;
		TVCaptureDevice											captureCard;

		public AnalogTVTuning()
		{
		}
		#region ITuning Members

		public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
			callback.OnEnded();
		}

		public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
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
			float percent = ((float)currentChannel) / 256.0f;
			percent *= 100.0f;
			callback.OnProgress((int)percent);
			float frequency=(float)captureCard.VideoFrequency();
			frequency/=1000000f;
			string description=String.Format("channel:{0} frequency:{1:###.##} MHz.", currentChannel, frequency);
			callback.OnStatus(description);

			if (captureCard.SignalPresent())
			{
				timer1.Enabled=false;
				callback.OnNewChannel();
				return;
			}
			NextChannel();
		}
		void NextChannel()
		{
			
			currentChannel++;
			if (currentChannel>=256)
			{
				timer1.Enabled=false;
				callback.OnProgress(100);
				callback.OnEnded();
				captureCard.DeleteGraph();
				return;
			}

			if (!captureCard.ViewChannel(currentChannel,captureCard.CountryCode,AnalogVideoStandard.None) )
			{
				timer1.Enabled=false;
				callback.OnProgress(100);
				callback.OnEnded();
				captureCard.DeleteGraph();
				return;
			}
		}
		
		public int MapToChannel(string channelName)
		{
			ArrayList channels=new ArrayList();
			TVDatabase.GetChannels(ref channels);
			for (int i=0; i < channels.Count;++i)
			{
				TVChannel chan = (TVChannel)channels[i];
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
