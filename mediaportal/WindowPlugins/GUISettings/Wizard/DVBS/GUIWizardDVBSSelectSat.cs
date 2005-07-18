using System;
using System.Collections;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;

namespace WindowPlugins.GUISettings.Wizard.DVBS
{
	/// <summary>
	/// Summary description for GUIWizardDVBSSelectSat.
	/// </summary>
	public class GUIWizardDVBSSelectSat : GUIWindow
	{
		class Transponder
		{
			public string SatName;
			public string FileName;
			public override string ToString()
			{
				return SatName;
			}
		}

		[SkinControlAttribute(10)]		protected GUILabelControl lblLNB1=null;
		[SkinControlAttribute(11)]		protected GUILabelControl lblLNB2=null;
		[SkinControlAttribute(12)]		protected GUILabelControl lblLNB3=null;
		[SkinControlAttribute(13)]		protected GUILabelControl lblLNB4=null;

		
		[SkinControlAttribute(50)]		protected GUIButtonControl btnLNB1=null;
		[SkinControlAttribute(51)]		protected GUIButtonControl btnLNB2=null;
		[SkinControlAttribute(52)]		protected GUIButtonControl btnLNB3=null;
		[SkinControlAttribute(53)]		protected GUIButtonControl btnLNB4=null;

		
		[SkinControlAttribute(25)]		protected GUIButtonControl btnBack=null;
		[SkinControlAttribute(26)]		protected GUIButtonControl btnNext=null;

		int maxLNBs=1;
		Transponder[] lnbConfig= new Transponder[5];
		public GUIWizardDVBSSelectSat()
		{
			
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_DVBS_SELECT_TRANSPONDER;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_dvbs_LNB3.xml");
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			maxLNBs=Int32.Parse(GUIPropertyManager.GetProperty("#WizardsDVBSLNB"));
			Update();
		}

		void Update()
		{
			lblLNB1.Label="-";
			lblLNB2.Label="-";
			lblLNB3.Label="-";
			lblLNB4.Label="-";
			btnLNB1.Disabled=!(maxLNBs>=1);
			btnLNB2.Disabled=!(maxLNBs>=2);
			btnLNB3.Disabled=!(maxLNBs>=3);
			btnLNB4.Disabled=!(maxLNBs>=4);
			if (lnbConfig[1]!=null) lblLNB1.Label=lnbConfig[1].SatName;
			if (lnbConfig[2]!=null) lblLNB2.Label=lnbConfig[2].SatName;
			if (lnbConfig[3]!=null) lblLNB3.Label=lnbConfig[3].SatName;
			if (lnbConfig[4]!=null) lblLNB4.Label=lnbConfig[4].SatName;
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==btnLNB1) OnLNB(1);
			if (control==btnLNB2) OnLNB(2);
			if (control==btnLNB3) OnLNB(3);
			if (control==btnLNB4) OnLNB(4);
			base.OnClicked (controlId, control, actionType);
		}

		Transponder LoadTransponder(string file)
		{
			System.IO.TextReader tin = System.IO.File.OpenText(file);
			Transponder ts = new Transponder();
			ts.FileName=file;
			string line=null;
			do
			{
				line = null;
				line = tin.ReadLine();
				if(line!=null)
				{
					if (line.Length > 0)
					{
						if(line.StartsWith(";"))
							continue;
						int pos=line.IndexOf("satname=");
						if (pos>=0)
						{
							ts.SatName=line.Substring(pos+"satname=".Length);
							tin.Close();
							return ts;
						}
					}
				}
			} while (!(line == null));
			tin.Close();
			return null;
		}

		void OnLNB(int lnb)
		{
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			dlg.Reset();
			dlg.SetHeading("Select transponder");//Menu
			string [] files = System.IO.Directory.GetFiles( System.IO.Directory.GetCurrentDirectory()+@"\Tuningparameters");
			ArrayList items = new ArrayList();
			foreach (string file in files)
			{
				if (file.ToLower().IndexOf(".tpl") >=0)
				{
					Transponder ts = LoadTransponder(file);
					if (ts!=null)
					{
						GUIListItem item = new GUIListItem(ts.SatName);
						item.MusicTag =(object)ts;
						items.Add(ts);
					}
				}
			}
			dlg.DoModal(GetID);
			int itemNo=dlg.SelectedLabel;
			if (itemNo<0) return;
			lnbConfig[lnb]= (Transponder)items[itemNo];
			Update();
		}
	}
}
