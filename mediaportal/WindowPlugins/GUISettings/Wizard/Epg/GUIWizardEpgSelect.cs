using System;
using System.Collections;
using System.IO;
using System.Xml;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Settings.Wizard;
using MediaPortal.TV.Database;

namespace WindowPlugins.GUISettings.Epg
{
	/// <summary>
	/// Summary description for GUIWizardEpgSelect.
	/// </summary>
	public class GUIWizardEpgSelect : GUIWindow
	{
		[SkinControlAttribute(24)]			protected GUIListControl listGrabbers=null;
		[SkinControlAttribute(2)]				protected GUILabelControl lblLine1=null;
		[SkinControlAttribute(3)]				protected GUILabelControl lblLine2=null;
		[SkinControlAttribute(5)]			  protected GUIButtonControl btnNext=null;
		[SkinControlAttribute(25)]			protected GUIButtonControl btnBack=null;
		bool epgGrabberSelected=false;
		public GUIWizardEpgSelect()
		{
			
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_EPG_SELECT;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_epg_select.xml");
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadGrabbers();
		}
		void LoadGrabbers()
		{
			lblLine1.Label=GUILocalizeStrings.Get(200000);
			lblLine2.Label=GUILocalizeStrings.Get(200001);
			epgGrabberSelected=false;
			listGrabbers.Clear();
			string[] folders = System.IO.Directory.GetDirectories(@"webepg\grabbers");			
			foreach (string folder in folders)
			{
				if (folder.IndexOf("..")>=0) continue;
				string[] files = System.IO.Directory.GetFiles(folder);
				foreach (string file in files)
				{
					string ext=System.IO.Path.GetExtension(file).ToLower();
					if (ext==".xml")
					{
						GUIListItem item = new GUIListItem();
						item.Label=System.IO.Path.GetFileNameWithoutExtension(folder);
						item.Path=file;
						listGrabbers.Add( item );
						break;
					}
				}
			}
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==listGrabbers)
			{
				if (!epgGrabberSelected)
					OnGrabberSelected(listGrabbers.SelectedListItem);
				else 
					OnMap();
			}
			if (control==btnNext)
			{
				OnNext();
			}
			if (control==btnBack)
			{
				OnBack();
			}
			base.OnClicked (controlId, control, actionType);
		}

		void OnBack()
		{
			if (epgGrabberSelected)
			{
				epgGrabberSelected=false;
				LoadGrabbers();
				return;
			}
			GUIWindowManager.ShowPreviousWindow();
		}

		void OnGrabberSelected(GUIListItem item)
		{
			if (item==null) return;
			GUIPropertyManager.SetProperty("#WizardGrabber",item.Path);

			//setup and import epg...
			try
			{
				Utils.FileDelete(@"webepg\WebEPG.xml");
				System.IO.File.Copy(item.Path,@"webepg\WebEPG.xml");
			}
			catch(Exception){}

			listGrabbers.Clear();
			XmlDocument doc = new XmlDocument();
			doc.Load(@"webepg\WebEPG.xml");
			XmlNodeList sections=doc.DocumentElement.SelectNodes("/profile/section");
			foreach (XmlNode section in sections)
			{
				if (section.Attributes==null) return;
				XmlNode nodeName=section.Attributes.GetNamedItem("name");
				if (nodeName==null) continue;
				if (nodeName.Value==null) continue;
				if (nodeName.Value!="ChannelList") continue; 
				XmlNodeList entries = section.SelectNodes("entry");
				foreach (XmlNode entry in entries)
				{
					nodeName=entry.Attributes.GetNamedItem("name");
					if (nodeName==null) continue;
					if (nodeName.Value==null) continue;
					GUIListItem ch = new GUIListItem();
					ch.Label=String.Format("{0}. {1}",listGrabbers.Count+1, nodeName.Value);
					listGrabbers.Add(ch);
				}
			}

			lblLine1.Label=GUILocalizeStrings.Get(200002);
			lblLine2.Label=GUILocalizeStrings.Get(200003);
			epgGrabberSelected=true;
		}
		void OnNext()
		{
			GUIPropertyManager.SetProperty("#Wizard.EPG.Done","yes");
		  GUIWizardCardsDetected.ScanNextCardType();
		}
		void OnMap()
		{
			GUIListItem item=listGrabbers.SelectedListItem;
			if (item==null) return;

			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			dlg.Reset();
			dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
			int selected=0;
			int count=0;
			foreach (TVChannel chan in channels)
			{
				dlg.Add(chan.Name);
				if (chan.Name==item.Label2) selected=count;
				count++;
			}
			dlg.SelectedLabel=selected;
			dlg.DoModal(GetID);
			if (dlg.SelectedLabel<0 || dlg.SelectedLabel>=channels.Count) return;
			TVChannel selChannel =(TVChannel)channels[dlg.SelectedLabel];
			item.Label2=selChannel.Name;
		}
	}
}
