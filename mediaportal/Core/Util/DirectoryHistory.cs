using System;
using System.Collections;

namespace MediaPortal.Util
{
	/// <summary>
	/// 
	/// </summary>
	public class DirectoryHistory
	{
    class DirectoryItem
    {
      string m_strItem=String.Empty;
      string m_strDir=String.Empty;
      public DirectoryItem()
      {
      }
      public DirectoryItem(string strItem, string strDir)
      {
				if (strItem==null || strDir==null) return;
        m_strItem=strItem;
        m_strDir=strDir;
      }
      public string Item
      {
        get { return m_strItem;}
        set {
					if (value==null) return;
					m_strItem=value;
				}
      }
      public string Dir
      {
        get { return m_strDir;}
				set 
				{ 
					if (value==null) return;
					m_strDir=value;
				}
      }
    }

    ArrayList m_history = new ArrayList ();
		public DirectoryHistory()
		{
		}
    
    public string Get(string strDir)
    {
			if (strDir==null) return String.Empty;
      foreach (DirectoryItem item in m_history)
      {
        if (item.Dir==strDir)
        {
          return item.Item;
        }
      }
      return String.Empty;
    }

    public void Set(string strItem, string strDir)
		{
			if (strItem==null) return ;
			if (strDir==null) return ;
      foreach (DirectoryItem item in m_history)
      {
        if (item.Dir==strDir)
        {
          item.Item=strItem;
          return;
        }
      }
      DirectoryItem newItem= new DirectoryItem(strItem,strDir);
      m_history.Add(newItem);
    }
	}
}
