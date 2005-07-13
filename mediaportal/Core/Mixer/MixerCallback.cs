using System;

namespace MediaPortal.Mixer
{
	public delegate void MixerCallback(IntPtr handle, short msg, IntPtr reserved, IntPtr WParam, IntPtr LParam);
}
