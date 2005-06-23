using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using System.Xml;


namespace MediaPortal.TV.Recording
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
		enum State
		{
			ScanStart,
			ScanTransponders,
			ScanChannels
		}
		TVCaptureDevice											captureCard;
		AutoTuneCallback										callback = null;
		int                                 currentIndex=-1;
		private System.Windows.Forms.Timer  timer1;
		State                               currentState;
		TPList[]														transp=new TPList[800];
		int																	count = 0;
		
		int newChannels, updatedChannels;
		int																	newRadioChannels, updatedRadioChannels;
		int m_diseqcLoops=1;
		int m_currentDiseqc=1;

		public DVBSTuning()
		{
		}
		#region ITuning Members

		public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
			newRadioChannels=0;
			updatedRadioChannels=0;
			newChannels=0;
			updatedChannels=0;
			
			captureCard=card;
			callback=statusCallback;
			string filename=String.Format(@"database\card_{0}.xml",card.FriendlyName);
			//
			// load card settings to check diseqc
			m_diseqcLoops=1;
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml(filename))
			{
				if(xmlreader.GetValueAsBool("dvbs","useLNB2",false)==true)
					m_diseqcLoops++;
				if(xmlreader.GetValueAsBool("dvbs","useLNB3",false)==true)
					m_diseqcLoops++;
				if(xmlreader.GetValueAsBool("dvbs","useLNB4",false)==true)
					m_diseqcLoops++;
			}

			currentState=State.ScanStart;
			currentIndex=-1;

			OpenFileDialog ofd =new OpenFileDialog();
      ofd.RestoreDirectory = true;
			ofd.Filter = "Transponder-Listings (*.tpl)|*.tpl";
			ofd.Title = "Choose Transponder-Listing Files";
			DialogResult res=ofd.ShowDialog();
			if(res!=DialogResult.OK)
			{
				Stop();
				callback.OnProgress(100);
				callback.OnStatus("Finished");
				callback.OnEnded();
				return;
			}
			
			count = 0;
			string line;
			string[] tpdata;
			Log.WriteFile(Log.LogType.Capture,"Opening {0}",ofd.FileName);
			// load transponder list and start scan
			System.IO.TextReader tin = System.IO.File.OpenText(ofd.FileName);
			
			do
			{
				line = null;
				line = tin.ReadLine();
				if(line!=null)
					if (line.Length > 0)
					{
						if(line.StartsWith(";"))
							continue;
						tpdata = line.Split(new char[]{','});
						if(tpdata.Length!=3)
							tpdata = line.Split(new char[]{';'});
						if (tpdata.Length == 3)
						{
							try
							{
			
								transp[count].TPfreq = Int32.Parse(tpdata[0]) *1000;
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
							{}
						}
					}
			} while (!(line == null));
			tin.Close();
			

			Log.WriteFile(Log.LogType.Capture,"loaded:{0} transponders", count);
			this.timer1 = new System.Windows.Forms.Timer();
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			timer1.Interval=100;
			timer1.Enabled=true;
			callback.OnProgress(0);
			return;
		}
		public void Stop()
		{
			if(timer1!=null)
			{
				timer1.Enabled=false;
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

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			if (currentIndex >= count)
			{
				timer1.Enabled=false;
				callback.OnProgress(100);
				callback.OnStatus("Finished");
				callback.OnEnded();
				return;
			}
			int index=currentIndex;
			if (index<0) index=0;
			
			float percent = ((float)index) / ((float)count);
			percent *= 100.0f;
			callback.OnProgress((int)percent);
			TPList transponder=transp[index];
			string chanDesc=String.Format("freq:{0} Khz, Pol:{1} SR:{2}",
						transponder.TPfreq, transponder.TPpol, transponder.TPsymb );
			string description=String.Format("Transponder:{0}/{1} {2}", index,count,chanDesc);

			if (currentState==State.ScanTransponders)
			{
				if (captureCard.SignalPresent())
				{
					
					//if (captureCard.SignalQuality>40)
					{
						Log.WriteFile(Log.LogType.Capture,"Found signal for transponder:{0} {1}",currentIndex,chanDesc);
						currentState=State.ScanChannels;
					}
				}
			}

			if (currentState==State.ScanTransponders || currentState==State.ScanStart)
			{
				currentState=State.ScanTransponders ;
				callback.OnStatus(description);
				ScanNextTransponder();
			}

			if (currentState==State.ScanChannels)
			{
				description=String.Format("Found signal for transponder:{0} {1}, Scanning channels", currentIndex,chanDesc);
				callback.OnStatus(description);
				ScanChannels();
			}
			
		}

		void ScanChannels()
		{
			timer1.Enabled=false;
			callback.OnStatus2( String.Format("new tv:{0} new radio:{1}", newChannels,newRadioChannels) );
			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
			callback.OnStatus2( String.Format("new tv:{0} new radio:{1}", newChannels,newRadioChannels) );

			callback.UpdateList();
			Log.WriteFile(Log.LogType.Capture,"timeout, goto scanning transponders");
			currentState=State.ScanTransponders;
			ScanNextTransponder();
			timer1.Enabled=true;
			return;
		}

		void ScanNextTransponder()
		{
			currentIndex++;
			if (currentIndex>=count)
			{
				if(m_currentDiseqc>=m_diseqcLoops)
				{
					timer1.Enabled=false;
					callback.OnProgress(100);
					callback.OnStatus("Finished");
					callback.OnEnded();
					captureCard.DeleteGraph();
				}
				else
				{
					m_currentDiseqc++;
					AutoTuneTV(captureCard,callback);
				}
				return;
			}
			timer1.Enabled=false;
			DVBChannel newchan = new DVBChannel();
			newchan.NetworkID=-1;
			newchan.TransportStreamID=-1;
			newchan.ProgramNumber=-1;

			newchan.Polarity=transp[currentIndex].TPpol;
			newchan.Symbolrate=transp[currentIndex].TPsymb;
			newchan.FEC=(int)TunerLib.FECMethod.BDA_FEC_METHOD_NOT_DEFINED;
			newchan.Frequency=transp[currentIndex].TPfreq;

			
			

			Log.WriteFile(Log.LogType.Capture,"tune transponder:{0} freq:{1} KHz symbolrate:{2} polarisation:{3}",currentIndex,
									newchan.Frequency,newchan.Symbolrate,newchan.Polarity);
			Application.DoEvents();
			Application.DoEvents();
			captureCard.Tune(newchan,m_currentDiseqc);
			Application.DoEvents();
			timer1.Enabled=true;
		}

		#endregion
	}
}
