using System;

namespace MediaPortal.UserInterface.Controls
{
	/// <summary>
	/// Summary description for ListView.
	/// </summary>
	public class MPListView : System.Windows.Forms.ListView
	{
		protected override void WndProc(ref System.Windows.Forms.Message m)
		{
			const int WM_PAINT = 0xf ;

			switch(m.Msg)
			{
				case WM_PAINT:
					if(this.View == System.Windows.Forms.View.Details && this.Columns.Count > 0)
					{
						this.Columns[this.Columns.Count - 1].Width = -2 ;
					}
					break ;
			}

			base.WndProc (ref m);
		}
	}
}
