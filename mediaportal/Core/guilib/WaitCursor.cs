using System;

namespace MediaPortal.GUI.Library
{
	public class WaitCursor : IDisposable
	{
		#region Constructors

		public WaitCursor()
		{
			GUIWaitCursor.Instance.Show();
		}

		#endregion Constructors

		#region Methods

		public void Dispose()
		{
			GUIWaitCursor.Instance.Hide();
		}

		#endregion Methods
	}
}
