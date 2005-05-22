using System;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using System.Collections;
using SQLite.NET;
using DShowNET;
namespace MediaPortal.TV.Database
{
	public struct SpecialChannelsStruct
	{
		public string Name;
		public long Frequency;
		public SpecialChannelsStruct(string name, long frequency)
		{
			Name=name;
			Frequency=frequency;
		}
	}

	public enum ExternalInputs:int
	{
		svhs =50000,
		cvbs1=50001,
		cvbs2=50002,
		rgb=50002
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

		public static SpecialChannelsStruct[] SpecialChannels = 
		{
			new SpecialChannelsStruct("K2",48250000L),
			new SpecialChannelsStruct("K3",55250000L),
			new SpecialChannelsStruct("K4",62250000L),
			new SpecialChannelsStruct("S1",105250000L),
			new SpecialChannelsStruct("S2",112250000L),
			new SpecialChannelsStruct("S3",119250000L),
			new SpecialChannelsStruct("S4",126250000L),
			new SpecialChannelsStruct("S5",133250000L),
			new SpecialChannelsStruct("S6",140250000L),
			new SpecialChannelsStruct("S7",147250000L),
			new SpecialChannelsStruct("S8",154250000L),
			new SpecialChannelsStruct("S9",161250000L),
			new SpecialChannelsStruct("S10",168250000L),
			new SpecialChannelsStruct("K5",175250000L),
			new SpecialChannelsStruct("K6",182250000L),
			new SpecialChannelsStruct("K7",189250000L),
			new SpecialChannelsStruct("K8",196250000L),
			new SpecialChannelsStruct("K9",203250000L),
			new SpecialChannelsStruct("K10",210250000L),
			new SpecialChannelsStruct("K11",217250000L),
			new SpecialChannelsStruct("K12",224250000L),
			new SpecialChannelsStruct("S11",231250000L),
			new SpecialChannelsStruct("S12",238250000L),
			new SpecialChannelsStruct("S13",245250000L),
			new SpecialChannelsStruct("S14",252250000L),
			new SpecialChannelsStruct("S15",259250000L),
			new SpecialChannelsStruct("S16",266250000L),
			new SpecialChannelsStruct("S17",273250000L),
			new SpecialChannelsStruct("S18",280250000L),
			new SpecialChannelsStruct("S19",287250000L),
			new SpecialChannelsStruct("S20",294250000L),
			new SpecialChannelsStruct("S21",303250000L),
			new SpecialChannelsStruct("S22",311250000L),
			new SpecialChannelsStruct("S23",319250000L),
			new SpecialChannelsStruct("S24",327250000L),
			new SpecialChannelsStruct("S25",335250000L),
			new SpecialChannelsStruct("S26",343250000L),
			new SpecialChannelsStruct("S27",351250000L),
			new SpecialChannelsStruct("S28",359250000L),
			new SpecialChannelsStruct("S29",367250000L),
			new SpecialChannelsStruct("S30",375250000L),
			new SpecialChannelsStruct("S31",383250000L),
			new SpecialChannelsStruct("S32",391250000L),
			new SpecialChannelsStruct("S33",399250000L),
			new SpecialChannelsStruct("S34",407250000L),
			new SpecialChannelsStruct("S35",415250000L),
			new SpecialChannelsStruct("S36",423250000L),
			new SpecialChannelsStruct("S37",431250000L),
			new SpecialChannelsStruct("S38",439250000L),
			new SpecialChannelsStruct("S39",447250000L),
			new SpecialChannelsStruct("S40",455250000L),
			new SpecialChannelsStruct("S41",463250000L),
			new SpecialChannelsStruct("K21",471250000L),
			new SpecialChannelsStruct("K22",479250000L),
			new SpecialChannelsStruct("K23",487250000L),
			new SpecialChannelsStruct("K24",495250000L),
			new SpecialChannelsStruct("K25",503250000L),
			new SpecialChannelsStruct("K26",511250000L),
			new SpecialChannelsStruct("K27",519250000L),
			new SpecialChannelsStruct("K28",527250000L),
			new SpecialChannelsStruct("K29",535250000L),
			new SpecialChannelsStruct("K30",543250000L),
			new SpecialChannelsStruct("K31",551250000L),
			new SpecialChannelsStruct("K32",559250000L),
			new SpecialChannelsStruct("K33",567250000L),
			new SpecialChannelsStruct("K34",575250000L),
			new SpecialChannelsStruct("K35",583250000L),
			new SpecialChannelsStruct("K36",591250000L),
			new SpecialChannelsStruct("K37",599250000L),
			new SpecialChannelsStruct("K38",607250000L),
			new SpecialChannelsStruct("K39",615250000L),
			new SpecialChannelsStruct("K40",623250000L),
			new SpecialChannelsStruct("K41",631250000L),
			new SpecialChannelsStruct("K42",639250000L),
			new SpecialChannelsStruct("K43",647250000L),
			new SpecialChannelsStruct("K44",655250000L),
			new SpecialChannelsStruct("K45",663250000L),
			new SpecialChannelsStruct("K46",671250000L),
			new SpecialChannelsStruct("K47",679250000L),
			new SpecialChannelsStruct("K48",687250000L),
			new SpecialChannelsStruct("K49",695250000L),
			new SpecialChannelsStruct("K50",703250000L),
			new SpecialChannelsStruct("K51",711250000L),
			new SpecialChannelsStruct("K52",719250000L),
			new SpecialChannelsStruct("K53",727250000L),
			new SpecialChannelsStruct("K54",735250000L),
			new SpecialChannelsStruct("K55",743250000L),
			new SpecialChannelsStruct("K56",751250000L),
			new SpecialChannelsStruct("K57",759250000L),
			new SpecialChannelsStruct("K58",767250000L),
			new SpecialChannelsStruct("K59",775250000L),
			new SpecialChannelsStruct("K60",783250000L),
			new SpecialChannelsStruct("K61",791250000L),
			new SpecialChannelsStruct("K62",799250000L),
			new SpecialChannelsStruct("K63",807250000L),
			new SpecialChannelsStruct("K64",815250000L),
			new SpecialChannelsStruct("K65",823250000L),
			new SpecialChannelsStruct("K66",831250000L),
			new SpecialChannelsStruct("K67",839250000L),
			new SpecialChannelsStruct("K68",847250000L),
			new SpecialChannelsStruct("K69",855250000L),
		};
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
		private TVProgram previousProgram=null;
		private TVProgram nextProgram=null;

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

				Update();
				return currentProgram;
			}
		}

		private void Update()
		{
			DateTime dt = DateTime.Now;
			previousProgram=null;
			currentProgram=null;
			nextProgram=null;
			long lNow=Utils.datetolong(dt);
			ArrayList progs = new ArrayList();
			long starttime=Utils.datetolong( dt.AddDays(-2) );
			long endtime  =Utils.datetolong( dt.AddDays(2));
			TVDatabase.GetProgramsPerChannel(Name,starttime,endtime,ref progs);
			for (int i=0; i < progs.Count;++i)
			{
				TVProgram prog = (TVProgram) progs[i];
				if (prog.Start <= lNow && prog.End >= lNow)
				{
					currentProgram=prog;
					if (i-1 >=0 )
						previousProgram=progs[i-1] as TVProgram;
					if (i+1 < progs.Count)
						nextProgram=progs[i+1] as TVProgram;
					break;
				}
			}
		}

		public TVProgram GetProgramAt(DateTime dt)
		{	
			long lNow=Utils.datetolong(DateTime.Now);
			
			if (currentProgram==null) 
				Update();
			
			if (currentProgram!=null)
			{
				if (currentProgram.End < lNow) 
					Update();
			}
				
			lNow=Utils.datetolong(dt);
			if (previousProgram!=null)
			{
				if (previousProgram.Start <= lNow && previousProgram.End >= lNow) 
					return previousProgram;
			}
			if (nextProgram!=null)
			{
				if (nextProgram.Start <= lNow && nextProgram.End >= lNow) 
					return nextProgram;
			}
			if (currentProgram!=null)
			{
				if (currentProgram.Start <= lNow && currentProgram.End >= lNow) 
					return currentProgram;
			}

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