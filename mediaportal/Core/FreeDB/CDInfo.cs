using System;
using System.Text;

namespace MediaPortal.Freedb
{
	/// <summary>
	/// Summary description for CDInfo.
	/// </summary>
	public class CDInfo
	{
    private string m_category;
    private string m_discid;
    private string m_title;

		public CDInfo()
		{
		}

    public CDInfo(string discid, string category, string title)
    {
      m_discid = discid;
      m_category = category;
      m_title = title;
    }

    public string Category
    {
      get
      {
        return m_category;
      }
      set
      {
        m_category = value;
      }
    }

    public string DiscId
    {
      get
      {
        return m_discid;
      }
      set
      {
        m_discid = value;
      }
    }

    public string Title
    {
      get
      {
        return m_title;
      }
      set
      {
        m_title = value;
      }
    }

    public override string ToString()
    {
      StringBuilder buff = new StringBuilder(100);
      buff.Append("DiscId: ");
      buff.Append(m_discid);
      buff.Append("; Category: ");
      buff.Append(m_category);
      buff.Append("; Title: ");
      buff.Append(m_title);
      return buff.ToString();
    }

	}
}
