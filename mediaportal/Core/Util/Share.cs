using System;

namespace MediaPortal.Util
{
	/// <summary>
	/// 
	/// </summary>
	public class Share
	{
    private string m_strPath="";
    private string m_strName="";
    private bool   m_bDefault=false;		
    private int    m_iPincode=-1;
    public Share()
		{
		}

    public Share(string strName, string strPath)
    {
      m_strName=strName;
      m_strPath=Utils.RemoveTrailingSlash(strPath);
    }


    public Share(string strName, string strPath, int iPincode)
    {
      m_strName=strName;
      m_strPath=Utils.RemoveTrailingSlash(strPath);
      m_iPincode=iPincode;
    }

    public int Pincode
    {
      get { return m_iPincode;}
      set { m_iPincode=value;}
    }

    public string Name
    {
      get { return m_strName;}
      set { m_strName=value;}
    }
    
    public string Path
    {
      get { return m_strPath;}
      set { 
        m_strPath=Utils.RemoveTrailingSlash(value);
        
      }
    }

    public bool Default
    {
      get { return m_bDefault;}
      set { m_bDefault=value;}
    }
	}
}
