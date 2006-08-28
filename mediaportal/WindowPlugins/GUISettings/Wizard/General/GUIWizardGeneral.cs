using System;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.GUI.Settings.Wizard
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class  GUIWizardGeneral : GUIWindow
	{
		[SkinControlAttribute(4)]		protected GUICheckMarkControl cmInternetYes = null;
		[SkinControlAttribute(5)]		protected GUICheckMarkControl cmInternetNo = null;
		[SkinControlAttribute(6)]		protected GUICheckMarkControl cmDeadicatedPC = null;
        [SkinControlAttribute(7)]       protected GUICheckMarkControl cmSharedPC = null;
				//[SkinControlAttribute(8)]       protected GUICheckMarkControl cmAutoStartYes = null;
				//[SkinControlAttribute(9)]       protected GUICheckMarkControl cmAutoStartNo = null;
        //[SkinControlAttribute(6)]       protected GUICheckMarkControl cmDeadicatedYes = null;
        //[SkinControlAttribute(7)]       protected GUICheckMarkControl cmDeadicatedNo = null;
		[SkinControlAttribute(26)]		protected GUIButtonControl    btnNext = null;
        [SkinControlAttribute(25)]      protected GUIButtonControl    btnBack = null;
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
						if (cmDeadicatedPC == control) OnUsageType(true);
						if (cmSharedPC == control) OnUsageType(false);
			      if (btnNext == control) OnNextPage();
            if (btnBack == control) GUIWindowManager.ShowPreviousWindow();
			      base.OnClicked (controlId, control, actionType);
		}

        void OnUsageType(bool deadicated)
		{
			if (deadicated)
            {
							  cmDeadicatedPC.Selected = true;
								cmSharedPC.Selected = false;
								GUIControl.FocusControl(GetID, cmDeadicatedPC.GetID);
            }
            else
            {
							cmDeadicatedPC.Selected = false;
							cmSharedPC.Selected = true;
							GUIControl.FocusControl(GetID, cmSharedPC.GetID);
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
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
			{
                OnInternetAccess(xmlreader.GetValueAsBool("general", "internetaccess", true));
								OnUsageType(true);
								//OnFullScreen(xmlreader.GetValueAsBool("general", "startfullscreen", true));
								//OnAutoStart(xmlreader.GetValueAsBool("general", "autostart", false));
			}
		}

		void OnNextPage()
		{
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("general", "internetaccess", cmInternetYes.Selected);

				// general defaults
        bool startfullscreen = false;
        bool mousesupport = true;
        bool autohidemouse = false;
        bool autostart = false;
        bool baloontips = false;
        bool hidetaskbar = false;
        bool dblclickasrightclick = false;
        bool alwaysontop = false;
        bool exclusivemode = true;
        bool useVMR9ZapOSD = false;
        bool enableguisounds = true;
        bool screensaver = false;

				if (cmDeadicatedPC.Selected)
				{
					startfullscreen = true;
					autostart = true;
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
