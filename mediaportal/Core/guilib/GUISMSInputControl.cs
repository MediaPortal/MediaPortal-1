using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for GUISMSInputControl.
	/// </summary>
	public class GUISMSInputControl:GUIControl
	{
		public GUISMSInputControl()
		{
		}
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction ();
    }
    public override void AllocResources()
    {
      base.AllocResources ();
    }
    public override void FreeResources()
    {
      base.FreeResources ();
    }
    public override bool OnMessage(GUIMessage message)
    {
      return base.OnMessage (message);
    }
    public override void OnAction(Action action)
    {
      base.OnAction (action);
    }
    public override void Render()
    {

    }

	}
}
