using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Interface which is used for VMR9 players to render the GUI
	/// </summary>
	public interface IRender
	{
		void RenderFrame(long timePassed);
	}
}
