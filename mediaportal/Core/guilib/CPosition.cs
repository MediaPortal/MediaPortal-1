using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// A datastructure for keeping track of the position of a control in a window.
	/// </summary>
	public class CPosition
	{
		private GUIControl m_control=null;
		private int  m_iPosX=0;
		private int  m_iPosY=0;

		/// <summary>
		/// The (empty) constructor of the CPosition class.
		/// </summary>
		public CPosition()
		{	
		}

		/// <summary>
		/// Constructs a CPosition class.
		/// </summary>
		/// <param name="control">The control of which the position is kept.</param>
		/// <param name="x">The X coordinate.</param>
		/// <param name="y">The Y coordinate.</param>
		public CPosition(ref GUIControl control, int x, int y)
		{	
			m_control=control;
			m_iPosX=x;
			m_iPosY=y;
		}

		/// <summary>
		/// Gets the X coordintate.
		/// </summary>
		public int XPos
		{
			get {return m_iPosX;}
		}

		/// <summary>
		/// Gets the Y coordinate.
		/// </summary>
		public int YPos
		{
			get {return m_iPosY;}
    }

		/// <summary>
		/// Gets the control.
		/// </summary>
    public GUIControl control
    {
      get {return m_control;}
    }
	}
}
