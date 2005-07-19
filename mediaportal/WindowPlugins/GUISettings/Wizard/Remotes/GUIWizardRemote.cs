using System;
using MediaPortal.GUI.Library;
namespace MediaPortal.GUI.Settings.Wizard
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class  GUIWizardRemote : GUIWindow
	{
		[SkinControlAttribute(4)]			protected GUICheckMarkControl cmMicrosoftUSA=null;
		[SkinControlAttribute(5)]			protected GUICheckMarkControl cmMicrosoftEU=null;
		[SkinControlAttribute(6)]			protected GUICheckMarkControl cmHauppauge=null;
		[SkinControlAttribute(7)]			protected GUICheckMarkControl cmFireDTV=null;
		[SkinControlAttribute(8)]			protected GUICheckMarkControl cmOther=null;
		[SkinControlAttribute(26)]		protected GUIButtonControl		btnNext=null;
		[SkinControlAttribute(10)]		protected GUIImage						imgRemote=null;

		public  GUIWizardRemote()
		{
			
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_REMOTE;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_remote_control.xml");
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			OnMicrosoftEU();
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (cmMicrosoftUSA==control) OnMicrosoftUSA();
			if (cmMicrosoftEU==control) OnMicrosoftEU();
			if (cmHauppauge==control) OnHauppauge();
			if (cmFireDTV==control) OnFireDTV();
			if (cmOther==control) OnOther();
			if (btnNext==control) OnNextPage();
			base.OnClicked (controlId, control, actionType);
		}

		void OnMicrosoftUSA()
		{
			cmMicrosoftUSA.Selected=true;
			cmMicrosoftEU.Selected=false;
			cmHauppauge.Selected=false;
			cmFireDTV.Selected=false;
			cmOther.Selected=false;
			imgRemote.SetFileName(@"Wizards\MCEUSA.jpg");
		}
		void OnMicrosoftEU()
		{
			cmMicrosoftUSA.Selected=false;
			cmMicrosoftEU.Selected=true;
			cmHauppauge.Selected=false;
			cmFireDTV.Selected=false;
			cmOther.Selected=false;
			imgRemote.SetFileName(@"Wizards\MCEEU.JPG");
		}

		void OnHauppauge()
		{
			cmMicrosoftUSA.Selected=false;
			cmMicrosoftEU.Selected=false;
			cmHauppauge.Selected=true;
			cmFireDTV.Selected=false;
			cmOther.Selected=false;
			imgRemote.SetFileName(@"Wizards\hauppauge.jpg");
		}
		
		void OnFireDTV()
		{
			cmMicrosoftUSA.Selected=false;
			cmMicrosoftEU.Selected=false;

			cmHauppauge.Selected=false;
			cmFireDTV.Selected=true;
			cmOther.Selected=false;
			imgRemote.SetFileName(@"Wizards\firedtv.png");
		}
		
		void OnOther()
		{
			cmMicrosoftUSA.Selected=false;
			cmMicrosoftEU.Selected=false;

			cmHauppauge.Selected=false;
			cmFireDTV.Selected=false;
			cmOther.Selected=true;
			imgRemote.SetFileName("");
		}
		void OnNextPage()
		{

			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("remote", "mce2005", (cmMicrosoftUSA.Selected||cmMicrosoftEU.Selected));
				xmlwriter.SetValueAsBool("remote", "USAModel", cmMicrosoftUSA.Selected);
				xmlwriter.SetValueAsBool("remote", "HCW", cmHauppauge.Selected);
				xmlwriter.SetValueAsBool("remote", "FireDTV", cmFireDTV.Selected);

			}
			GUIPropertyManager.SetProperty("#Wizard.Remote.Done","yes");
			GUIWizardCardsDetected.ScanNextCardType();
		}
	}
}
