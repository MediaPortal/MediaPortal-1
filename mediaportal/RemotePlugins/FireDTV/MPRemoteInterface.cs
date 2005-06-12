using System;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using System.Windows.Forms;

namespace MediaPortal.RemoteControls
{
	interface IRemoteControlInterface
	{
		void Init(IntPtr hwnd);
		void DeInit();
		bool WndProc(ref Message msg, out Action action,out char key, out Keys keyCode);
		System.Guid RCGuid
		{
			get;
		}
	}
}
