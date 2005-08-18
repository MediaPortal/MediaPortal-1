/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.Windows.Forms;

using MediaPortal.GUI.Library;

namespace MediaPortal.Remotes
{
	public sealed class InputDevices
	{
		#region Constructors

		private InputDevices()
		{
		}

		#endregion Constructors

		#region Methods

		public static void Init(/* SplashScreen splashScreen */)
		{
			MCE2005Remote.Init(GUIGraphicsContext.ActiveForm);
			FireDTVRemote.Init(GUIGraphicsContext.ActiveForm);
  		HCWRemote.Init(GUIGraphicsContext.ActiveForm);
		}

		public static void Stop()
		{
			MCE2005Remote.DeInit();
			HCWRemote.DeInit();
			FireDTVRemote.DeInit();
      diRemote.Stop();
		}

		public static bool WndProc(ref Message msg, out Action action, out char key, out Keys keyCode)
		{
			action = null;
			key = (char)0;
			keyCode = Keys.A;

			HCWRemote.WndProc(msg);

			if(HIDListener.WndProc(ref msg, out action, out key, out keyCode))
				return true;

			if(MCE2005Remote.WndProc(ref msg, out action, out key, out keyCode))
				return true;

			if(FireDTVRemote.WndProc(ref msg, out action, out  key, out keyCode))
				return true;

			return false;
		}

		#endregion Methods

		#region Properties

		internal static AppCommands LastHidRequest
		{
			get { return _lastHidRequest; }
			set { _lastHidRequest = value; _lastHidRequestTick = Environment.TickCount; }
		}	

		internal static int LastHidRequestTick
		{
			get { return _lastHidRequestTick; }
		}	

		#endregion Properties

		#region Fields

		static MCE2005Remote MCE2005Remote = new MCE2005Remote();
		static HCWRemote HCWRemote = new HCWRemote();
		static DirectInputHandler diRemote = new DirectInputHandler();
		static MediaPortal.RemoteControls.FireDTVRemote FireDTVRemote = new MediaPortal.RemoteControls.FireDTVRemote();
		static AppCommands			_lastHidRequest;
		static int					_lastHidRequestTick;

		#endregion Fields
	}
}
