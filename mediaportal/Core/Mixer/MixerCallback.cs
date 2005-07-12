using System;

namespace MediaPortal.Mixer
{
	public delegate void MixerCallback(IntPtr handle, short msg, IntPtr reserved, IntPtr WParam, IntPtr LParam);
//	typedef void (CALLBACK DRVCALLBACK)(HDRVR hdrvr, UINT uMsg, DWORD_PTR dwUser, DWORD_PTR dw1, DWORD_PTR dw2);
}
