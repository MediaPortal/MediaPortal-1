using System;
using MediaPortal.GUI.Library;

namespace WindowPlugins.GUISettings.Wizard.DVBS
{
	/// <summary>
	/// Summary description for GUIWizardDVBSSelectLNBs.
	/// </summary>
	public class GUIWizardDVBSSelectLNBs: GUIWindow
	{
		[SkinControlAttribute(4)]			protected GUICheckMarkControl cmLNB1=null;
		[SkinControlAttribute(5)]			protected GUICheckMarkControl cmLNB2=null;
		[SkinControlAttribute(6)]			protected GUICheckMarkControl cmLNB3=null;
		[SkinControlAttribute(7)]			protected GUICheckMarkControl cmLNB4=null;
		[SkinControlAttribute(26)]		protected GUIButtonControl btnNext=null;

		public GUIWizardDVBSSelectLNBs()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_DVBS_SELECT_LNB;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_dvbs_LNB1.xml");
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			cmLNB1.Selected=true;
			cmLNB2.Selected=false;
			cmLNB3.Selected=false;
			cmLNB4.Selected=false;
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==cmLNB1)
			{
				cmLNB1.Selected=true;
				cmLNB2.Selected=false;
				cmLNB3.Selected=false;
				cmLNB4.Selected=false;
				return;
			}
			
			if (control==cmLNB2)
			{
				cmLNB1.Selected=false;
				cmLNB2.Selected=true;
				cmLNB3.Selected=false;
				cmLNB4.Selected=false;
				return;
			}
			
			if (control==cmLNB3)
			{
				cmLNB1.Selected=false;
				cmLNB2.Selected=false;
				cmLNB3.Selected=true;
				cmLNB4.Selected=false;
				return;
			}
			
			if (control==cmLNB4)
			{
				cmLNB1.Selected=false;
				cmLNB2.Selected=false;
				cmLNB3.Selected=false;
				cmLNB4.Selected=true;
				return;
			}
			if (control==btnNext) 
			{
				OnNextPage();
				return;
			}
			base.OnClicked (controlId, control, actionType);
		}

		void OnNextPage()
		{
			if (cmLNB1.Selected) GUIPropertyManager.SetProperty("#WizardsDVBSLNB","1");
			if (cmLNB2.Selected) GUIPropertyManager.SetProperty("#WizardsDVBSLNB","2");
			if (cmLNB3.Selected) GUIPropertyManager.SetProperty("#WizardsDVBSLNB","3");
			if (cmLNB4.Selected) GUIPropertyManager.SetProperty("#WizardsDVBSLNB","4");
			GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_DVBS_SELECT_DETAILS);
		}
	}
}
