using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace GUIExplorer
{
	/// <summary>
	/// Summary description for SetupForm.
	/// </summary>
  public class SetupForm : System.Windows.Forms.Form, ISetupForm 
  {
 
	public SetupForm()
	{
	  //
	  // Required for Windows Form Designer support
	  //
		//
	  // TODO: Add any constructor code after InitializeComponent call
	  //
	}

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
	  if( disposing )
	  {
	  }
	  base.Dispose( disposing );
	}

	#region Windows Form Designer generated code
	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		// 
		// SetupForm
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(184, 54);
		this.Name = "SetupForm";
		this.Text = "SetupForm";

	}
	#endregion

	#region plugin vars

	public string PluginName() 
	{
	  return "My Explorer";
	}

	public string Description() 
	{
	  return "A File Explorer plugin for Media Portal";
	}

	public string Author() 
	{
	  return "Gucky62";
	}

	public void ShowPlugin() 
	{
	  ShowDialog();
	}

	public bool DefaultEnabled() 
	{
	  return false;
	}

	public bool CanEnable() 
	{
	  return true;
	}

	public bool HasSetup() 
	{
	  return false;
	}

	public int GetWindowId() 
	{
	  return 770;
	}

	/// <summary>
	/// If the plugin should have its own button on the home screen then it
	/// should return true to this method, otherwise if it should not be on home
	/// it should return false
	/// </summary>
	/// <param name="strButtonText">text the button should have</param>
	/// <param name="strButtonImage">image for the button, or empty for default</param>
	/// <param name="strButtonImageFocus">image for the button, or empty for default</param>
	/// <param name="strPictureImage">subpicture for the button or empty for none</param>
	/// <returns>true  : plugin needs its own button on home
	///          false : plugin does not need its own button on home</returns>
	public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage) 
	{

	  strButtonText = GUILocalizeStrings.Get(2200);
	  strButtonImage = "";
	  strButtonImageFocus = "";
	  strPictureImage = "";
	  return true;
	}
	#endregion

  }
}
