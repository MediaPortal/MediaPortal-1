using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Class which hold information about a key press
	/// </summary>
	public class Key
	{

    private int      m_iChar=0;	// character 
    private int      m_iCode=0;	// character code 

		/// <summary>
		/// empty constructor
		/// </summary>
		public Key()
		{
		}

		/// <summary>
		/// copy constructor
		/// </summary>
		/// <param name="key"></param>
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
