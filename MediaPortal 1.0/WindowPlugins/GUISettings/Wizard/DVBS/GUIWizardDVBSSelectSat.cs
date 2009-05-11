#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;
using MediaPortal.TV.Recording;

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
		int card=0;
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
			card = Int32.Parse( GUIPropertyManager.GetProperty("#WizardCard"));
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
			if (control==btnNext) OnNextPage();
			if (control==btnBack) OnPreviousPage();
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
      string[] files = System.IO.Directory.GetFiles(Config.GetSubFolder(Config.Dir.Base, "Tuningparameters"));
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
						dlg.Add(item);
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

		void OnNextPage()
		{
			TVCaptureDevice captureCard= Recorder.Get(card);
			if (captureCard!=null) 
			{
        string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", captureCard.FriendlyName));

				using(MediaPortal.Profile.Settings   xmlwriter=new MediaPortal.Profile.Settings(filename))
				{
					if (lnbConfig[1]!=null)
						xmlwriter.SetValue("dvbs","sat1",lnbConfig[1].FileName);
					if (lnbConfig[2]!=null)
						xmlwriter.SetValue("dvbs","sat2",lnbConfig[2].FileName);
					if (lnbConfig[3]!=null)
						xmlwriter.SetValue("dvbs","sat3",lnbConfig[3].FileName);
					if (lnbConfig[4]!=null)
						xmlwriter.SetValue("dvbs","sat4",lnbConfig[4].FileName);
				}
			}
			GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_DVBS_SCAN);
		}
		
		void OnPreviousPage()
		{
			GUIWindowManager.ShowPreviousWindow();
		}
	}
}
