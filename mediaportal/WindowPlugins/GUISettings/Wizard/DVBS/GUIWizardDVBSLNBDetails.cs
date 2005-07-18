using System;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;

namespace WindowPlugins.GUISettings.Wizard.DVBS
{
	/// <summary>
	/// Summary description for GUIWizardDVBSLNBDetails.
	/// </summary>
	public class GUIWizardDVBSLNBDetails : GUIWindow
	{
		[SkinControlAttribute(5)]			protected GUIButtonControl btnNext=null;
		[SkinControlAttribute(25)]		protected GUIButtonControl btnBack=null;
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
		
		
		
		int LNBNumber=1;
		int maxLNBs=1;
		int card=0;

		public GUIWizardDVBSLNBDetails()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_DVBS_SELECT_DETAILS;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_dvbs_LNB2.xml");
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			card = Int32.Parse( GUIPropertyManager.GetProperty("#WizardCard"));
			maxLNBs=Int32.Parse(GUIPropertyManager.GetProperty("#WizardsDVBSLNB"));
			Update();
		}
		void Update()
		{	
			LoadSettings();
			lblLNB.Label=String.Format("Please specify the details for LNB:{0}", LNBNumber);
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==btnNext) OnNextPage();
			if (control==btnBack) OnPreviousPage();
			if (controlId >=cmDisEqcNone.GetID && controlId <=cmDisEqcLevel1BB.GetID) OnDisEqC(control);
			if (controlId >=cmLnb0Khz.GetID && controlId <=cmLnb44Khz.GetID) OnLNBKhz(control);
			if (controlId >=cmLnbBandKU.GetID && controlId <=cmLnbBandCircular.GetID) Onband(control);
			base.OnClicked (controlId, control, actionType);
		}

		void Onband(GUIControl control)
		{
			cmLnbBandKU.Selected=false;
			cmLnbBandC.Selected=false;
			cmLnbBandCircular.Selected=false;
			if (control==cmLnbBandKU) cmLnbBandKU.Selected=true;
			if (control==cmLnbBandC) cmLnbBandC.Selected=true;
			if (control==cmLnbBandCircular) cmLnbBandCircular.Selected=true;
		}
		void OnLNBKhz(GUIControl control)
		{
			cmLnb0Khz.Selected=false;
			cmLnb22Khz.Selected=false;
			cmLnb33Khz.Selected=false;
			cmLnb44Khz.Selected=false;
			if (control==cmLnb0Khz) cmLnb0Khz.Selected=true;
			if (control==cmLnb22Khz) cmLnb22Khz.Selected=true;
			if (control==cmLnb33Khz) cmLnb33Khz.Selected=true;
			if (control==cmLnb44Khz) cmLnb44Khz.Selected=true;

		}
		void OnDisEqC(GUIControl control)
		{
			cmDisEqcNone.Selected=false;
			cmDisEqcSimpleA.Selected=false;
			cmDisEqcSimpleB.Selected=false;
			cmDisEqcLevel1AA.Selected=false;
			cmDisEqcLevel1BA.Selected=false;
			cmDisEqcLevel1AB.Selected=false;
			cmDisEqcLevel1BB.Selected=false;
			if (control==cmDisEqcNone) cmDisEqcNone.Selected=true;
			if (control==cmDisEqcSimpleA) cmDisEqcSimpleA.Selected=true;
			if (control==cmDisEqcSimpleB) cmDisEqcSimpleB.Selected=true;
			if (control==cmDisEqcLevel1AA) cmDisEqcLevel1AA.Selected=true;
			if (control==cmDisEqcLevel1BA) cmDisEqcLevel1BA.Selected=true;
			if (control==cmDisEqcLevel1AB) cmDisEqcLevel1AB.Selected=true;
			if (control==cmDisEqcLevel1BB) cmDisEqcLevel1BB.Selected=true;
		}

		void OnNextPage()
		{
			if (LNBNumber < maxLNBs)
			{
				SaveSettings();
				LNBNumber++;
				Update();
			}
		}

		void OnPreviousPage()
		{
			SaveSettings();
			if (LNBNumber>1) LNBNumber--;
			Update();
		}
		void LoadSettings()
		{
			cmLnb0Khz.Selected=false;
			cmLnb22Khz.Selected=false;
			cmLnb33Khz.Selected=false;
			cmLnb44Khz.Selected=false;
			cmDisEqcNone.Selected=false;
			cmDisEqcSimpleA.Selected=false;
			cmDisEqcSimpleB.Selected=false;
			cmDisEqcLevel1AA.Selected=false;
			cmDisEqcLevel1BA.Selected=false;
			cmDisEqcLevel1AB.Selected=false;
			cmDisEqcLevel1BB.Selected=false;
			cmLnbBandKU.Selected=false;
			cmLnbBandC.Selected=false;
			cmLnbBandCircular.Selected=false;

			TVCaptureDevice captureCard= Recorder.Get(card);
			string filename=String.Format(@"database\card_{0}.xml",captureCard.FriendlyName);
			
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml(filename))
			{
				string lnbKey=String.Format("lnb{0}",LNBNumber);
				if (LNBNumber==1) lnbKey="lnb";
				int lnbKhz=xmlreader.GetValueAsInt("dvbs",lnbKey,44);
				switch (lnbKhz)
				{
					case 0: cmLnb0Khz.Selected=true;break;
					case 22: cmLnb22Khz.Selected=true;break;
					case 33: cmLnb33Khz.Selected=true;break;
					case 44: cmLnb44Khz.Selected=true;break;
				}
				
				lnbKey=String.Format("lnbKind{0}",LNBNumber);
				if (LNBNumber==1) lnbKey="lnbKind";
				int lnbKind=xmlreader.GetValueAsInt("dvbs",lnbKey,0);
				switch (lnbKind)
				{
					case 0: cmLnbBandKU.Selected=true;break;
					case 1: cmLnbBandC.Selected=true;break;
					case 2: cmLnbBandCircular.Selected=true;break;
				}

				lnbKey=String.Format("diseqc{0}",LNBNumber);
				if (LNBNumber==1) lnbKey="diseqc";
				int diseqc=xmlreader.GetValueAsInt("dvbs",lnbKey,0);
				switch (diseqc)
				{
					case 0: break;
					case 1: cmDisEqcSimpleA.Selected=true;break;
					case 2: cmDisEqcSimpleB.Selected=true;break;
					case 3: cmDisEqcLevel1AA.Selected=true;break;
					case 4: cmDisEqcLevel1BA.Selected=true;break;
					case 5: cmDisEqcLevel1AB.Selected=true;break;
					case 6: cmDisEqcLevel1BB.Selected=true;break;
				}
			}
		}

		void SaveSettings()
		{
/*
			TVCaptureDevice captureCard= Recorder.Get(card);
			string filename=String.Format(@"database\card_{0}.xml",captureCard.FriendlyName);
			using(MediaPortal.Profile.Xml   xmlwriter=new MediaPortal.Profile.Xml(filename))
			{
				if(diseqca.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","diseqc",diseqca.SelectedIndex); 
				}
				if(diseqcb.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","diseqc2",diseqcb.SelectedIndex); 
				}
				if(diseqcc.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","diseqc3",diseqcc.SelectedIndex); 
				}
				if(diseqcd.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","diseqc4",diseqcd.SelectedIndex); 
				}
				if (lnbconfig1.SelectedIndex>=0)
				{
					switch (lnbconfig1.SelectedIndex)
					{
						case 0: xmlwriter.SetValue("dvbs","lnb",0); break;
						case 1: xmlwriter.SetValue("dvbs","lnb",22); break;
						case 2: xmlwriter.SetValue("dvbs","lnb",33); break;
						case 3: xmlwriter.SetValue("dvbs","lnb",44); break;
					}
				}
				if (lnbconfig2.SelectedIndex>=0)
				{
					switch (lnbconfig2.SelectedIndex)
					{
						case 0: xmlwriter.SetValue("dvbs","lnb2",0); break;
						case 1: xmlwriter.SetValue("dvbs","lnb2",22); break;
						case 2: xmlwriter.SetValue("dvbs","lnb2",33); break;
						case 3: xmlwriter.SetValue("dvbs","lnb2",44); break;
					}
				}
				if (lnbconfig3.SelectedIndex>=0)
				{
					switch (lnbconfig3.SelectedIndex)
					{
						case 0: xmlwriter.SetValue("dvbs","lnb3",0); break;
						case 1: xmlwriter.SetValue("dvbs","lnb3",22); break;
						case 2: xmlwriter.SetValue("dvbs","lnb3",33); break;
						case 3: xmlwriter.SetValue("dvbs","lnb3",44); break;
					}
				}
				if (lnbconfig4.SelectedIndex>=0)
				{
					switch (lnbconfig4.SelectedIndex)
					{
						case 0: xmlwriter.SetValue("dvbs","lnb4",0); break;
						case 1: xmlwriter.SetValue("dvbs","lnb4",22); break;
						case 2: xmlwriter.SetValue("dvbs","lnb4",33); break;
						case 3: xmlwriter.SetValue("dvbs","lnb4",44); break;
					}
				}

				if (lnbkind1.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","lnbKind",lnbkind1.SelectedIndex); 
				}
				if (lnbkind2.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","lnbKind2",lnbkind2.SelectedIndex); 
				}
				if (lnbkind3.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","lnbKind3",lnbkind3.SelectedIndex); 
				}
				if (lnbkind4.SelectedIndex>=0)
				{
					xmlwriter.SetValue("dvbs","lnbKind4",lnbkind4.SelectedIndex); 
				}
			}
*/
		}
	}
}
