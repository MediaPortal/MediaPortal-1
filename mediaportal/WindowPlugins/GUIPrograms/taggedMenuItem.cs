using System;

namespace WindowPlugins.GUIPrograms
{
	/// <summary>
	/// Summary description for taggedMenuItem.
	/// </summary>
	public class taggedMenuItem: System.Windows.Forms.MenuItem
	{
		public taggedMenuItem(string text): base(text)
		{
		}

		int mTag = 0;

		public int Tag
		{
			get{ return mTag; }
			set{ mTag = value;}
		}

	}
}
