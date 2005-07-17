using System;
using MediaPortal.GUI.Library;

namespace WindowPlugins.GUISettings.Wizard.DVBS
{
	/// <summary>
	/// Summary description for GUIWizardDVBSLNBDetails.
	/// </summary>
	public class GUIWizardDVBSLNBDetails : GUIWindow
	{
		[SkinControlAttribute(5)]			protected GUIButtonControl btnNext=null;
		[SkinControlAttribute(100)]		protected GUILabelControl lblLNB=null;
		[SkinControlAttribute(40)]		protected GUICheckMarkControl cmDisEqcNone=null;
		[SkinControlAttribute(41)]		protected GUICheckMarkControl cmDisEqcSimpleA=null;
		[SkinControlAttribute(42)]		protected GUICheckMarkControl cmDisEqcSimpleB=null;
		[SkinControlAttribute(43)]		protected GUICheckMarkControl cmDisEqcLevel1AA=null;
		[SkinControlAttribute(44)]		protected GUICheckMarkControl cmDisEqcLevel1BA=null;
		[SkinControlAttribute(45)]		protected GUICheckMarkControl cmDisEqcLevel1AB=null;
		[SkinControlAttribute(46)]		protected GUICheckMarkControl cmDisEqcLevel1BB=null;


		[SkinControlAttribute(50)]		protected GUICheckMarkControl cmLnb0Khz=null;
		[SkinControlAttribute(51)]		protected GUICheckMarkControl cmLnb22Khz=null;
		[SkinControlAttribute(52)]		protected GUICheckMarkControl cmLnb33Khz=null;
		[SkinControlAttribute(53)]		protected GUICheckMarkControl cmLnb44Khz=null;
		
		[SkinControlAttribute(60)]		protected GUICheckMarkControl cmLnbBandKU=null;
		[SkinControlAttribute(61)]		protected GUICheckMarkControl cmLnbBandC=null;
		[SkinControlAttribute(62)]		protected GUICheckMarkControl cmLnbBandCircular=null;

		public GUIWizardDVBSLNBDetails()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_DVBS_SELECT_DETAILS;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_dvbs_LNB2.xml");
		}
	}
}
