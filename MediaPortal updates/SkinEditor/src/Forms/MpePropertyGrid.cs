using System;
using System.Windows.Forms;

using Crownwood.Magic.Win32;

namespace SkinEditor.Forms
{
	/// <summary>
	/// Summary description for MpePropertyGrid.
	/// </summary>
	public class MpePropertyGrid : System.Windows.Forms.PropertyGrid {

		public MpePropertyGrid() {
			MpeLog.Info("MpePropertyGrid()");
			SetStyle(ControlStyles.EnableNotifyMessage, true);
		}
		protected override void OnNotifyMessage(Message m) {
			if (m.Msg == (int)Msgs.WM_CONTEXTMENU) {
				short x = (short)(m.LParam.ToInt32());
				short y = (short)(m.LParam.ToInt32() >> 16);
				MpeLog.Info(x.ToString() + ", " + y.ToString());
			}
			base.OnNotifyMessage(m);
		}


	}
}
