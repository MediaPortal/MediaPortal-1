using System;

namespace MediaPortal.Mixer
{
	public class MixerEventArgs : EventArgs
	{
		#region Constructors

		public MixerEventArgs(IntPtr handle, int id)
		{
			_handle = handle;
			_id = id;
		}

		#endregion Constructors

		#region Properties

		public IntPtr Handle
		{
			get { return _handle; }
		}

		public int Id
		{
			get { return _id; }
		}

		#endregion Properties

		#region Fields

		IntPtr						_handle;
		int							_id;

		#endregion Fields
	}
}
