using System;
using System.Runtime.InteropServices;
using System.Collections;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// All things related to the skyStar2 specific things go in here
	/// </summary>
	public class DVBSkyStar2Helper
	{
		public DVBSkyStar2Helper()
		{
			//
			// TODO: Fügen Sie hier die Konstruktorlogik hinzu
			//
			m_dvbSections=new DVBSections();
			m_transponder=new DVBSections.TPList[200];

		}
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool ClearAllUp();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool PauseSITable();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool RunSITable();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool IsTunerLocked();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool AddPidToTS(int pid);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool Tune(int fre, int symrate, int fec, int pol, int lnbkhz, int Diseq, int LNBfreq);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool DVBInit(string adapter);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern void ClearAllTsPids();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool StopSITable();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool DeleteAllPIDsI();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool SetupB2C2Graph();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool GetSectionDataI(int pid, int tid, ref int secCount,int tableSection,int timeout);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool AddTSPid(int pid);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern void ResetDevice();

		DVBSections							m_dvbSections;
		ArrayList							m_eitList;
		ArrayList							m_transponderList;
		int									m_diseqc=0;
		int									m_lnb0=0;
		int									m_lnb1=0;
		int									m_lnbsw=0;
		int									m_lnbkhz=0;
		System.Windows.Forms.ProgressBar	m_progress=null;
		System.Windows.Forms.TextBox		m_textBox=null;
		int									m_lnbfreq=0;
		int									m_selKhz=0;
		bool								m_breakScan=false;
		System.Windows.Forms.TreeView		m_transponderTV=null;
		const DShowNET.IBaseFilter			m_mpeg2Data=null;
		DVBSections.TPList[]				m_transponder;


		public DShowNET.IBaseFilter Mpeg2DataFilter
		{
			get {return m_mpeg2Data;}
		}

		public void ClearPids()
		{
			DeleteAllPIDsI();
		}
		public bool TuneChannel(int fre, int symrate, int fec, int pol, int lnbkhz, int Diseq, int LNBfreq)
		{
			return Tune( fre,  symrate, fec, pol,lnbkhz,  Diseq, LNBfreq);
		}
		public void OpenTPLFile (ref DVBSections.Transponder[] list,int diseqc,int lnbkhz,int lnb0,int lnb1,int lnbsw,System.Windows.Forms.ProgressBar progBar,System.Windows.Forms.TextBox feedbackText,System.Windows.Forms.TreeView transponderTreeView)
		{
			string[] tpdata;
			string line="";
			int count = 0;
			// set diseq & lnb
			
			m_diseqc=diseqc;
			m_lnb0=lnb0;
			m_lnb1=lnb1;
			m_lnbsw=lnbsw;
			m_lnbkhz=lnbkhz;

			// set dialog feedback
			m_progress=progBar;
			m_textBox=feedbackText;
			m_transponderTV=transponderTreeView;
			// load transponder list and start scan
			foreach(System.Windows.Forms.TreeNode tn in transponderTreeView.Nodes)
			{
				line = null;
				line =(string) tn.Tag;
				if(line!=null)
					if (line.Length > 0 && tn.Checked==true)
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
						
								m_transponder[count].TPfreq = Convert.ToInt16(tpdata[0]) * 1000;
								switch (tpdata[1].ToLower())
								{
									case "v":
									
										m_transponder[count].TPpol = 1;
										break;
									case "h":
									
										m_transponder[count].TPpol = 0;
										break;
									default:
									
										m_transponder[count].TPpol = 0;
										break;
								}
								m_transponder[count].TPsymb = Convert.ToInt16(tpdata[2]);
								count += 1;
							}
							catch
							{}
						}
					}
			}
			
			count -= 1;
			StartScan(count,ref list);
		}
		private void StartScan (int count, ref DVBSections.Transponder[] transplist)
		{
			// to handle you own scan, you must write an loop
			// in which you 1. tune to your channel and 2. parse the
			// data you get back from stream.
			//

			int t;
			int lnbFreq;
			int servCount=0;
			int lnbkhz=0;

			m_transponderList = new ArrayList();
			transplist = new DVBSections.Transponder[count + 1];
			for (t = 0; t <= count; t++)
			{
				if(m_breakScan==true)
					break;
				DeleteAllPIDsI();
				AddTSPid(0x10);
				AddTSPid(17);
				AddTSPid(0);
				// feedback
				if(m_progress!=null)
				{
					m_progress.Minimum = 0;
					m_progress.Maximum = count;
					m_progress.Value = t;
				}
				// set the tranponder data
				transplist[t].channels = new ArrayList();
				transplist[t].PMTTable = new ArrayList();
				// first get pmt
				if(m_lnb1!=-1 && m_lnbsw!=-1)
				{
					if (m_transponder[t].TPfreq >= m_lnbsw*1000)
					{
						lnbFreq = m_lnb1;
						lnbkhz=m_lnbkhz;
					}
					else
					{
						lnbFreq = m_lnb0;
						lnbkhz=0;
					}
				}
				else
				{
					lnbFreq = m_lnb0;
					lnbkhz=0;
				}
				m_lnbfreq=lnbFreq;
				m_selKhz=lnbkhz;
				m_dvbSections.SetLNBParams(m_diseqc,m_lnb0,m_lnb1,m_lnbsw,m_lnbkhz,lnbkhz,lnbFreq);
				// feedback
				if(m_textBox!=null)
				{
					m_textBox.Text=Convert.ToString(m_transponder[t].TPfreq/1000)+" MHz...("+Convert.ToString(servCount)+" services found yet)";
				}
				Tune(m_transponder[t].TPfreq, m_transponder[t].TPsymb, 6, m_transponder[t].TPpol, m_selKhz, m_diseqc, lnbFreq);
				//System.Threading.Thread.Sleep(500);
					
				if (IsTunerLocked())
				{
					
					m_dvbSections.ProcessPATSections(m_mpeg2Data,0,0,0,5000,m_transponder[t],ref transplist[t]);
					if(m_textBox!=null)
					{
						servCount+=transplist[t].channels.Count;
					}
				}

			}
		}

		public void InterruptScan()
		{
			m_breakScan=true;
			m_textBox.Text="(Stopping scan...please wait a moment!)";

		}

		public void CleanUp ()
		{
			ClearAllUp();
			m_breakScan=false;
		}

		public bool Run()
		{
			bool flag=false;
			flag=SetupB2C2Graph();
			if (flag == true)
			{
				flag = RunSITable();
			}
			if (flag == false)
			{
				return false;
			}
			DeleteAllPIDsI();
			AddTSPid(18);
			AddTSPid(16);
			m_eitList = new ArrayList();
			
			return true;
		}
	}// class
}//namespace
