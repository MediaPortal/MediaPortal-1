using System;

namespace MediaPortal.Util
{
	/// <summary>
	/// Helper class which contains a single share
	/// a share has a 
	/// - name
	/// - drive/path
	/// - pincode
	/// and can be the default share or not. WHen a share is default it will
	/// be shown when the user first selects the share
	/// </summary>
	public class Share
	{
    private string m_strPath=String.Empty;
    private string m_strName=String.Empty;
    private bool   m_bDefault=false;		
    private int    m_iPincode=-1;

		/// <summary>
		/// empty constructor
		/// </summary>
    public Share()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="strName">share name</param>
		/// <param name="strPath">share folder</param>
    public Share(string strName, string strPath)
    {
			if (strName==null || strPath==null) return;
			if (strName==String.Empty || strPath==String.Empty) return;
      m_strName=strName;
      m_strPath=Utils.RemoveTrailingSlash(strPath);
    }



		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="strName">share name</param>
		/// <param name="strPath">share folder</param>
		/// <param name="iPincode">pincode for folder (-1 = no pincode)</param>
    public Share(string strName, string strPath, int iPincode)
		{
			if (strName==null || strPath==null) return;
			if (strName==String.Empty || strPath==String.Empty) return;
      m_strName=strName;
      m_strPath=Utils.RemoveTrailingSlash(strPath);
      m_iPincode=iPincode;
    }

		/// <summary>
		/// property to get/set the pincode for the share
		/// (-1 means no pincode)
		/// </summary>
    public int Pincode
    {
      get { return m_iPincode;}
      set { m_iPincode=value;}
    }

		/// <summary>
		/// Property to get/set the share name
		/// </summary>
    public string Name
    {
      get { return m_strName;}
      set { 
				if (value==null) return;
				m_strName=value;
			}
    }
    
		/// <summary>
		/// Property to get/set the share folder
		/// </summary>
    public string Path
    {
      get { return m_strPath;}
			set 
			{ 
				if (value==null) return;
        m_strPath=Utils.RemoveTrailingSlash(value);
        
      }
    }

		/// <summary>
		/// Property to get/set this share as the default share
		/// </summary>
    public bool Default
    {
      get { return m_bDefault;}
      set { m_bDefault=value;}
    }
	}
}
