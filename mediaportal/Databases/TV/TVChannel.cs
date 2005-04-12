using System;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using System.Collections;
using SQLite.NET;
using DShowNET;
namespace MediaPortal.TV.Database
{
	public enum ExternalInputs:int
	{
		svhs =50000,
		cvbs1=50001,
		cvbs2=50002,
	}

	public class TVGroup
	{
		public int    ID;
		public string GroupName;
		public int    Sort;
		public int    Pincode;
		public ArrayList     tvChannels = new ArrayList();
		public override string ToString()
		{
			return GroupName;
		}

	}

  /// <summary>
  /// Class which holds all information about a tv channel
  /// </summary>
	public class TVChannel
	{
		string m_strName;
		int    m_iNumber;
		int    m_iID;
		long   m_lFrequency;
		string m_strXMLId;
		bool   m_bExternal=false;
		string m_strExternalTunerChannel="";
		bool   m_bVisibleInGuide=true;
		int    m_iCountry=-1;
		string m_strProviderName="";
		public bool   m_scrambled=false;
		public int    m_iSort=-1;
		private TVProgram currentProgram=null;

		AnalogVideoStandard _TVStandard;
		/// <summary> 
		/// Property to indicate if this channel is scrambled or not
		/// </summary>
		public bool Scrambled
		{
			get { return m_scrambled;}
			set {m_scrambled=value;}
		}

		/// <summary> 
		/// Property to indicate if this is an internal or external (USB-UIRT) channel
		/// </summary>
		public bool External
		{
			get { return m_bExternal;}
			set {m_bExternal=value;}
		}
		public int Sort
		{
			get 
			{
				return m_iSort;
			}
			set { m_iSort=value;}
		}

		/// <summary>
		/// Property to specify the TV standard
		/// </summary>
		public AnalogVideoStandard TVStandard
		{
			get { return _TVStandard;}
			set { _TVStandard =value;}
		}

		/// <summary>
		/// Property that indicates if this channel should be visible in the EPG or not.
		/// </summary>
		public bool VisibleInGuide
		{
			get { return m_bVisibleInGuide; }
			set { m_bVisibleInGuide = value; }
		}

		/// <summary> 
		/// Property to get/set the external tuner channel
		/// </summary>
		public string ExternalTunerChannel
		{
			get { return m_strExternalTunerChannel;}
			set 
			{
				m_strExternalTunerChannel=value;
				if (m_strExternalTunerChannel.Equals("unknown") ) m_strExternalTunerChannel="";
			}
		}

		/// <summary> 
		/// Property to get/set the ID the tv channel has in the XMLTV file
		/// </summary>
		public string XMLId
		{
			get { return m_strXMLId;}
			set {m_strXMLId=value;}
		}

		/// <summary>
		/// Property to get/set the ID the tvchannel has in the tv database
		/// </summary>
		public int ID
		{
			get { return m_iID;}
			set {m_iID=value;}
		}
 
		/// <summary>
		/// Property to get/set the name of the tvchannel
		/// </summary>
		public string Name
		{
			get { return m_strName;}
			set {m_strName=value;}
		}
 
		/// <summary>
		/// Property to get/set the name of the tvchannel
		/// </summary>
		public string ProviderName
		{
			get { return m_strProviderName;}
			set {m_strProviderName=value;}
		}
 
		/// <summary>
		/// Property to get/set the the tvchannel number
		/// </summary>
		public int Number
		{
			get { return m_iNumber;}
			set {m_iNumber=value;}
		}

		
		/// <summary>
		/// Property to get/set the the tvchannel country
		/// </summary>
		public int Country
		{
			get { return m_iCountry;}
			set {m_iCountry=value;}
		}

		/// <summary>
		/// Property to get/set the the frequency of the tvchannel (0=use default)
		/// </summary>
		public long Frequency
		{
			get { return m_lFrequency;}
			set {m_lFrequency=value;}
		}

		//return the current running program for this channel
		public TVProgram CurrentProgram
		{
			get
			{				
				DateTime dtNow=DateTime.Now;
				long lNow=Utils.datetolong(dtNow);
				if (currentProgram!=null)
				{
					if (currentProgram.Start <= lNow && currentProgram.End >= lNow) return currentProgram;
					currentProgram=null;
				}

				currentProgram=GetProgramAt(DateTime.Now);
				return currentProgram;
			}
		}

		public TVProgram GetProgramAt(DateTime dt)
		{	
			long lNow=Utils.datetolong(dt);
			ArrayList progs = new ArrayList();
			long starttime=Utils.datetolong( dt.AddDays(-2) );
			long endtime  =Utils.datetolong( dt.AddDays(2));
			TVDatabase.GetProgramsPerChannel(Name,starttime,endtime,ref progs);
			foreach (TVProgram prog in progs)
			{
				if (prog.Start <= lNow && prog.End >= lNow)
				{
					return prog;
				}
			}
			return null;
		}
  }
}