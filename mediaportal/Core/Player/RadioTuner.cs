using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectX.Capture;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.Util;


using MediaPortal.GUI.Library;
using DShowNET;
namespace MediaPortal.Player 
{
	/// <summary>
	/// 
	/// </summary>
	public class RadioTuner: IPlayer
	{
    RadioGraph m_capture=null;
		DVBRadioGraph	m_dvbCapture=null;
    string		m_strFile;
		bool			m_bInternal=false;
		Process		m_player=null;
		DateTime  m_dtTime=new DateTime();
		string		m_strRadioDevice="";
    string    m_strAudioDevice="";
    string    m_strLineInput="";

		public RadioTuner()
		{
		}

    public override bool Play(string strFile)
    {
      int iChannel = Convert.ToInt32( System.IO.Path.GetFileNameWithoutExtension(strFile) );

      Log.Write("Radiotuner: tune to channel:{0}", iChannel);
      try
      {
				string strPlayerFile="";
				string strPlayerArgs="";
        
        int iTunerCountry=31;
        string strTunerType="Antenna";
        using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
        {
					m_bInternal   =xmlreader.GetValueAsBool("radio","internal",true);
					strPlayerFile =xmlreader.GetValueAsString("radio","player","");
					strPlayerArgs =xmlreader.GetValueAsString("radio","args","f %MHZ%");
          strTunerType  =xmlreader.GetValueAsString("radio","tuner","Antenna");
          iTunerCountry =xmlreader.GetValueAsInt("capture","country",31);
          m_strRadioDevice=xmlreader.GetValueAsString("radio","device","");
          m_strAudioDevice=xmlreader.GetValueAsString("radio","audiodevice","");
          m_strLineInput=xmlreader.GetValueAsString("radio","lineinput","");
				}

				m_dtTime=DateTime.Now;
				if (!m_bInternal)
				{
					double dFreq=(double)iChannel;
					dFreq/=1000000d;
					string strFreq=dFreq.ToString();

					strFreq=strFreq.Replace(",", ".");
					strPlayerArgs=strPlayerArgs.Replace("%MHZ%",strFreq);
					
					strFreq=strFreq.Replace(".", ",");
					strPlayerArgs=strPlayerArgs.Replace("%mhz%",strFreq);
					
					strFreq=iChannel.ToString();
					strPlayerArgs=strPlayerArgs.Replace("%HZ%",strFreq);
					
					m_player=Utils.StartProcess(strPlayerFile,strPlayerArgs,false,true);
					if (m_player==null) 
					{
						Log.Write("Unable to start external radio player:{0} {1}", strPlayerFile, strPlayerArgs);
						return false;
					}
					
					m_strFile=strFile;
					return true;
				}
				
				// alloc card.
				AllocCard(m_strRadioDevice);
        bool bAntenna=false;
        if (strTunerType.Equals("Antenna"))
          bAntenna=true;
        if(m_strRadioDevice=="B2C2 MPEG-2 Source")
        {
          m_dvbCapture=new DVBRadioGraph(m_strRadioDevice,m_strAudioDevice,m_strLineInput);
          if (!m_dvbCapture.Create())
          {
            Log.Write("RadioTuner:failed to Tune to channel:{0}",iChannel);
            m_dvbCapture.DeleteGraph();
            m_dvbCapture=null;
            FreeCard(m_strRadioDevice);
            return false;
          }
			if(m_dvbCapture.Tune(iChannel)==false)
			{
				m_dvbCapture.DeleteGraph();
				return false;
			}
          Log.Write("RadioTuner:Frequency:{0} Hz tuned to:{1} Hz",  m_dvbCapture.Channel);
        }
        else
        {
          m_capture = new RadioGraph(m_strRadioDevice,m_strAudioDevice,m_strLineInput);
          if (!m_capture.Create(!bAntenna, 0, iTunerCountry))
          {
            Log.Write("RadioTuner:failed to Tune to channel:{0}",iChannel);
            m_capture.DeleteGraph();
            m_capture=null;
            FreeCard(m_strRadioDevice);
            return false;
          }
          m_capture.Tune(iChannel);

          Log.Write("RadioTuner:Frequency:{0} Hz tuned to:{1} Hz",  m_capture.Channel, m_capture.AudioFrequency);
        }
      }
      catch(Exception ex)
      {
				if(m_strRadioDevice!="B2C2 MPEG-2 Source")
				{
					Log.Write("RadioTuner:failed to Tune to channel:{0} {1} {2}",iChannel,ex.Message,ex.StackTrace);
					m_capture.DeleteGraph();
					m_capture=null;
					FreeCard(m_strRadioDevice);
					return false;
				}
				else
				{
					Log.Write("RadioTuner:failed to Tune to channel:{0} {1} {2}",iChannel,ex.Message,ex.StackTrace);
					m_dvbCapture.DeleteGraph();
					m_dvbCapture=null;
					FreeCard(m_strRadioDevice);
					return false;
				}
      }
      m_strFile=strFile;
      return true;
    }

    public override bool Playing
    {
      get
      {
				if (m_bInternal)
				{
					if(m_capture!=null || m_dvbCapture!=null)
						return true;
					return false; //TEST
				}
				else
				{
					return (m_player!=null);
				}
      }
    }

    public override void Stop()
    {
			if (m_bInternal)
			{
				if (m_dvbCapture!=null) m_dvbCapture.DeleteGraph();
				if (m_capture!=null) m_capture.DeleteGraph();
				m_dvbCapture=null;
				m_capture=null;
				FreeCard(m_strRadioDevice);
			}
			else
			{
				if (m_player!=null)
				{
						try 
						{
							m_player.CloseMainWindow();
						}
						catch (Exception){}
						if (!m_player.HasExited)
						{
							try
							{
								m_player.Kill();
							}
							catch (Exception){}

						}
						m_player=null;
				}
			}
    }


    public override string CurrentFile
    {
      get { return m_strFile;}
    }
    public override bool IsRadio
    {
      get 
      {
        return true;
      }
    }
		public override double CurrentPosition
		{
			get
			{
				TimeSpan ts = DateTime.Now-m_dtTime;
				return (double)ts.TotalSeconds;
			}
		}

		public override void Process()
		{
			if (!m_bInternal)
			{
				TimeSpan ts = DateTime.Now-m_dtTime;
				if (ts.TotalSeconds<5)
				{
					GUIGraphicsContext.form.Activate();
				}
			}
		}

		void AllocCard(string strDevice)
		{
			if (strDevice==null) return;
			if (strDevice.Length==0) return;
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_ALLOC_CARD,0,0,0,0,0,null);
			msg.Label=strDevice;
			GUIWindowManager.SendMessage(msg);
			GC.Collect();GC.Collect();GC.Collect();
		}

		void FreeCard(string strDevice)
		{
			if (strDevice==null) return;
			if (strDevice.Length==0) return;
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_FREE_CARD,0,0,0,0,0,null);
			msg.Label=strDevice;
			GUIWindowManager.SendMessage(msg);
		}
	}
}
