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
      string m_strItem;
      string m_strDir;
      public DirectoryItem()
      {
      }
      public DirectoryItem(string strItem, string strDir)
      {
        m_strItem=strItem;
        m_strDir=strDir;
      }
      public string Item
      {
        get { return m_strItem;}
        set { m_strItem=value;}
      }
      public string Dir
      {
        get { return m_strDir;}
        set { m_strDir=value;}
      }
    }

    ArrayList m_history = new ArrayList ();
		public DirectoryHistory()
		{
		}
    
    public string Get(string strDir)
    {
      foreach (DirectoryItem item in m_history)
      {
        if (item.Dir==strDir)
        {
          return item.Item;
        }
      }
      return "";
    }

    public void Set(string strItem, string strDir)
    {
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
