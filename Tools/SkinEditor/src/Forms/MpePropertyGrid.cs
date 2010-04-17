#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

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
