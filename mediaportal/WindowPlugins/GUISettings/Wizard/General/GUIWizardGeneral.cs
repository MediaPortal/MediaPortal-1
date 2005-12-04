using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Settings.Wizard
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class  GUIWizardGeneral : GUIWindow
	{
		[SkinControlAttribute(4)]		protected GUICheckMarkControl cmInternetYes = null;
		[SkinControlAttribute(5)]		protected GUICheckMarkControl cmInternetNo = null;
		[SkinControlAttribute(6)]		protected GUICheckMarkControl cmFullScreenYes = null;
        [SkinControlAttribute(7)]       protected GUICheckMarkControl cmFullScreenNo = null;
        [SkinControlAttribute(8)]       protected GUICheckMarkControl cmAutoStartYes = null;
        [SkinControlAttribute(9)]       protected GUICheckMarkControl cmAutoStartNo = null;
        //[SkinControlAttribute(6)]       protected GUICheckMarkControl cmDeadicatedYes = null;
        //[SkinControlAttribute(7)]       protected GUICheckMarkControl cmDeadicatedNo = null;
		[SkinControlAttribute(26)]		protected GUIButtonControl    btnNext=null;
		//[SkinControlAttribute(10)]		protected GUIImage				imgHTPC=null;

		public  GUIWizardGeneral()
		{
			
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_GENERAL;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_general.xml");
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadSettings();
            GUIControl.FocusControl(GetID, btnNext.GetID);
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
            if (cmInternetYes == control)   OnInternetAccess(true);
            if (cmInternetNo == control)    OnInternetAccess(false);
            if (cmFullScreenYes == control) OnFullScreen(true);
            if (cmFullScreenNo == control)  OnFullScreen(false);
			if (btnNext==control) OnNextPage();
			base.OnClicked (controlId, control, actionType);
		}

        void OnFullScreen(bool yes)
		{
            if (yes)
            {
                cmFullScreenYes.Selected = true;
                cmFullScreenNo.Selected = false;
                GUIControl.FocusControl(GetID, cmFullScreenYes.GetID);
            }
            else
            {
                cmFullScreenYes.Selected = false;
                cmFullScreenNo.Selected = true;
                GUIControl.FocusControl(GetID, cmFullScreenNo.GetID);
            }
		}

        void OnAutoStart(bool yes)
        {
            if (yes)
            {
                cmAutoStartYes.Selected = true;
                cmAutoStartNo.Selected = false;
                GUIControl.FocusControl(GetID, cmAutoStartYes.GetID);
            }
            else
            {
                cmAutoStartYes.Selected = false;
                cmAutoStartNo.Selected = true;
                GUIControl.FocusControl(GetID, cmAutoStartNo.GetID);
            }
        }

        void OnInternetAccess(bool yes)
        {
            if (yes)
            {
                cmInternetYes.Selected = true;
                cmInternetNo.Selected = false;
                GUIControl.FocusControl(GetID, cmInternetYes.GetID);
            }
            else
            {
                cmInternetYes.Selected = false;
                cmInternetNo.Selected = true;
                GUIControl.FocusControl(GetID, cmInternetNo.GetID);
            }
        }

		void LoadSettings()
		{
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
                OnInternetAccess(xmlreader.GetValueAsBool("general", "internetaccess", true));
                OnFullScreen(xmlreader.GetValueAsBool("general", "startfullscreen", true));
                OnAutoStart(xmlreader.GetValueAsBool("general", "autostart", false));
			}
		}

		void OnNextPage()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("general", "internetaccess", cmInternetYes.Selected);

                // general defaults
                bool startfullscreen = cmFullScreenYes.Selected;
                bool mousesupport = true;
                bool autohidemouse = false;
                bool autostart = cmAutoStartYes.Selected;
                bool baloontips = false;
                bool hidetaskbar = false;
                bool dblclickasrightclick = false;
                bool alwaysontop = false;
                bool exclusivemode = false;
                bool useVMR9ZapOSD = false;
                bool enableguisounds = false;
                bool screensaver = false;

                if (startfullscreen)
                {
                    autohidemouse = true;
                    baloontips = true;
                    hidetaskbar = true;
                    alwaysontop = true;
                    enableguisounds = true;
                }

                xmlwriter.SetValueAsBool("general", "startfullscreen", startfullscreen);
                xmlwriter.SetValueAsBool("general", "mousesupport", mousesupport);
                xmlwriter.SetValueAsBool("general", "autohidemouse", autohidemouse);
                xmlwriter.SetValueAsBool("general", "autostart", autostart);
                xmlwriter.SetValueAsBool("general", "alwaysontop", alwaysontop);
                xmlwriter.SetValueAsBool("general", "baloontips", baloontips);
                xmlwriter.SetValueAsBool("general", "hidetaskbar", hidetaskbar);
                xmlwriter.SetValueAsBool("general", "dblclickasrightclick", dblclickasrightclick);
                xmlwriter.SetValueAsBool("general", "useVMR9ZapOSD", useVMR9ZapOSD);
                xmlwriter.SetValueAsBool("general", "enableguisounds", enableguisounds);
                xmlwriter.SetValueAsBool("general", "screensaver", screensaver);
                xmlwriter.SetValueAsBool("general", "exclusivemode", exclusivemode);

			}

			GUIPropertyManager.SetProperty("#Wizard.General.Done", "yes");
            GUIPropertyManager.SetProperty("#InternetAccess", cmInternetYes.Selected.ToString());
			
			GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_REMOTE);
		}
	}
}
