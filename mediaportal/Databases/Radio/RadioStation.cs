using System;

namespace MediaPortal.Radio.Database

{
	/// <summary>
	/// 
	/// </summary>
	public class RadioStation
	{
    int    m_ID=0;
    string m_strName="";
    int    m_iChannel=0;
    long   m_lFrequency=0;
    string m_strURL="";
    string m_strGenre=""; 
    int    m_iBitRate=0;
		bool   m_bScrambled=false;
		public RadioStation()
		{
		}

    public int ID
    {
      get { return m_ID;}
      set { m_ID=value;}
    }
    public string Name
    {
      get { return m_strName;}
      set { m_strName=value;}
    }
    public int Channel
    {
      get { return m_iChannel;}
      set { m_iChannel=value;}
    }
    public long Frequency
    {
      get { return m_lFrequency;}
      set { m_lFrequency=value;}
    }
    public string URL
    {
      get { return m_strURL;}
      set { m_strURL=value;}
    }
    public string Genre
    {
      get { return m_strGenre;}
      set { m_strGenre=value;}
    }
    public int BitRate
    {
      get { return m_iBitRate;}
      set { m_iBitRate=value;}
    }
		public bool Scrambled
		{
			get { return m_bScrambled;}
			set { m_bScrambled=value;}
		}

	}
}
