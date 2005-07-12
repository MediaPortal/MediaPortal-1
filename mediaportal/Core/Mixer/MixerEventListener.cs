using System;
using System.Windows.Forms;

namespace MediaPortal.Mixer
{
	internal class MixerEventListener : NativeWindow
	{
		#region Events

		public event MixerEventHandler	LineChanged;
		public event MixerEventHandler	ControlChanged;

		#endregion Events

		#region Methods

		public void Start()
		{
			CreateParams createParams = new CreateParams();

			createParams.ExStyle = 0x80;
			createParams.Style = unchecked((int)0x80000000);
			
			CreateHandle(createParams);
		}

		protected override void WndProc(ref Message m)
		{
			if(m.Msg == (int)MixerMessages.LineChanged && LineChanged != null)
				LineChanged(this, new MixerEventArgs(m.WParam, m.LParam.ToInt32()));

			if(m.Msg == (int)MixerMessages.ControlChanged && ControlChanged != null)
				ControlChanged(this, new MixerEventArgs(m.WParam, m.LParam.ToInt32()));

			base.WndProc(ref m);
		}

		#endregion Methods
	}
}
