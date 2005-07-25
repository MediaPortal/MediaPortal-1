using System;
using System.IO;
using System.Xml;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Settings.Wizard;
namespace WindowPlugins.GUISettings.Epg
{
	/// <summary>
	/// Summary description for GUIWizardEpgSelect.
	/// </summary>
	public class GUIWizardEpgSelect : GUIWindow
	{
		[SkinControlAttribute(24)]			protected GUIListControl listGrabbers=null;
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
				OnGrabberSelected(listGrabbers.SelectedListItem);
			}
			base.OnClicked (controlId, control, actionType);
		}

		void OnGrabberSelected(GUIListItem item)
		{
			if (epgGrabberSelected) 
			{
				OnMap();
				return;
			}
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
					ch.Label=String.Format("{0}. ",listGrabbers.Count+1, nodeName.Value);
					listGrabbers.Add(ch);
				}
			}


			epgGrabberSelected=true;
		}
		void OnMap()
		{
			GUIPropertyManager.SetProperty("#Wizard.EPG.Done","yes");
		  GUIWizardCardsDetected.ScanNextCardType();
		}
	}
}
