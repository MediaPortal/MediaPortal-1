using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Key
	{

    private int      m_iChar=0;
    private int      m_iCode=0;
		public Key()
		{
		}

    public Key(Key key)
    {
      m_iChar=key.KeyChar;
      m_iCode=key.KeyCode;
    }

    public Key(int iChar, int iCode)
    {
      m_iChar=iChar;
      m_iCode=iCode;
    }

    public int KeyChar
    {
      get { return m_iChar;}
    }
    
    public int KeyCode
    {
      get { return m_iCode;}
    }
	}
}
