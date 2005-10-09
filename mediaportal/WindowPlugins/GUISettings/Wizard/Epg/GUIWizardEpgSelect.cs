using System;
using System.Collections;
using System.IO;
using System.Xml;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Settings.Wizard;
using MediaPortal.TV.Database;
using MediaPortal.EPG.config;

namespace WindowPlugins.GUISettings.Epg
{
	/// <summary>
	/// Summary description for GUIWizardEpgSelect.
	/// </summary>
	public class GUIWizardEpgSelect : GUIWindow, IComparer
	{
		[SkinControlAttribute(24)]			protected GUIListControl listGrabbers=null;
		[SkinControlAttribute(2)]			protected GUILabelControl lblLine1=null;
		[SkinControlAttribute(3)]			protected GUILabelControl lblLine2=null;
		[SkinControlAttribute(5)]			protected GUIButtonControl btnNext=null;
		[SkinControlAttribute(25)]			protected GUIButtonControl btnBack=null;
		bool epgGrabberSelected=false;
		ChannelsList _channelList;
		ChannelInfo[] _epgChannels;

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

			_channelList = new ChannelsList(@"webepg");

			string[] country = _channelList.GetCountries();

			for(int i=0; i < country.Length; i++)
			{
				GUIListItem item = new GUIListItem();
				System.Globalization.RegionInfo rInfo = new System.Globalization.RegionInfo( country[i] );
				item.Label = rInfo.DisplayName;
				item.Path = country[i];
				listGrabbers.Add( item );
			}


//			string[] folders = System.IO.Directory.GetDirectories(@"webepg\grabbers");			
//			foreach (string folder in folders)
//			{
//				if (folder.IndexOf("..")>=0) continue;
//				string[] files = System.IO.Directory.GetFiles(folder);
//				foreach (string file in files)
//				{
//					string ext=System.IO.Path.GetExtension(file).ToLower();
//					if (ext==".xml")
//					{
//						GUIListItem item = new GUIListItem();
//						item.Label=System.IO.Path.GetFileNameWithoutExtension(folder);
//						item.Path=file;
//						listGrabbers.Add( item );
//						break;
//					}
//				}
//			}

			GUIListItem manualItem = new GUIListItem();
			manualItem.Label=GUILocalizeStrings.Get(200004);//Manual supplied tvguide.xml
			manualItem.Path="";
			listGrabbers.Add( manualItem );
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
			listGrabbers.Clear();
			if (item.Path==String.Empty)
			{
				string _strTVGuideFile;
				using(MediaPortal.Profile.Xml  xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					_strTVGuideFile=xmlreader.GetValueAsString("xmltv","folder","xmltv");
					_strTVGuideFile=Utils.RemoveTrailingSlash(_strTVGuideFile);
					_strTVGuideFile+=@"\tvguide.xml";
				}
				if (!System.IO.File.Exists(_strTVGuideFile)) 
				{
					ShowError("Unable to open tvguide.xml from",_strTVGuideFile);
					LoadGrabbers();
					return;
				}
				//load tvguide.xml
				XmlDocument xml=new XmlDocument();
				xml.Load(_strTVGuideFile);
				if (xml.DocumentElement==null)
				{
					ShowError("Unable to open tvguide.xml from",_strTVGuideFile);
					LoadGrabbers();
					return;
				}
				XmlNodeList channelList=xml.DocumentElement.SelectNodes("/tv/channel");
				if (channelList==null)
				{
					ShowError("Invalid xmltv file","no tv channels found");
					LoadGrabbers();
					return;
				}
				if (channelList.Count==0)
				{
					ShowError("Invalid xmltv file","no tv channels found");
					LoadGrabbers();
					return;
				}
				foreach (XmlNode nodeChannel in channelList)
				{
					if (nodeChannel.Attributes!=null)
					{
						XmlNode nodeId=nodeChannel.Attributes.GetNamedItem("id");
						XmlNode nodeName=nodeChannel.SelectSingleNode("display-name");
						if (nodeName==null)
							nodeName=nodeChannel.SelectSingleNode("Display-Name");
						if (nodeName!=null && nodeName.InnerText!=null)
						{
							GUIListItem ch = new GUIListItem();
							ch.Label=nodeName.InnerText;
							ch.Path=nodeId.InnerText;
							int idChannel;
							string strTvChannel;
							if ( TVDatabase.GetEPGMapping(ch.Path, out idChannel, out strTvChannel))
							{
								ch.Label2=strTvChannel;
								ch.ItemId=idChannel;
							}
							listGrabbers.Add(ch);
						}
					}
				}
			}
			else
			{
				// Channel List
				_epgChannels = _channelList.GetChannelArray(item.Path);

				ArrayList channels = new ArrayList();
				TVDatabase.GetChannels(ref channels);
				channels.Sort (this);
				
				foreach (TVChannel chan in channels)
				{
					GUIListItem ch = new GUIListItem();
					ch.Label=chan.Name;
					ch.Path="";
//					int idChannel;
//					string strTvChannel;
//					if ( TVDatabase.GetEPGMapping(ch.Label, out idChannel, out strTvChannel))
//					{
//						ch.Label2=strTvChannel;
//						ch.ItemId=idChannel;
//					}
					listGrabbers.Add(ch);
				}

				//setup and import epg...
//				try
//				{
//					Utils.FileDelete(@"webepg\WebEPG.xml");
//					System.IO.File.Copy(item.Path,@"webepg\WebEPG.xml");
//				}
//				catch(Exception){}
//
//				XmlDocument doc = new XmlDocument();
//				doc.Load(@"webepg\WebEPG.xml");
//				XmlNodeList sections=doc.DocumentElement.SelectNodes("/profile/section");
//				foreach (XmlNode section in sections)
//				{
//					if (section.Attributes==null) return;
//					XmlNode nodeName=section.Attributes.GetNamedItem("name");
//					if (nodeName==null) continue;
//					if (nodeName.Value==null) continue;
//					if (nodeName.Value!="ChannelList") continue; 
//					XmlNodeList entries = section.SelectNodes("entry");
//					foreach (XmlNode entry in entries)
//					{
//						nodeName=entry.Attributes.GetNamedItem("name");
//						if (nodeName==null) continue;
//						if (nodeName.Value==null) continue;
//						GUIListItem ch = new GUIListItem();
//						ch.Label=nodeName.Value;
//						ch.Path=nodeName.Value;
//						int idChannel;
//						string strTvChannel;
//						if ( TVDatabase.GetEPGMapping(ch.Path, out idChannel, out strTvChannel))
//						{
//							ch.Label2=strTvChannel;
//							ch.ItemId=idChannel;
//						}
//						listGrabbers.Add(ch);
//					}
//				}
			}
			lblLine1.Label=GUILocalizeStrings.Get(200002);
			lblLine2.Label=GUILocalizeStrings.Get(200003);
			epgGrabberSelected=true;
		}

		void OnNext()
		{
			MapChannels();
			GUIPropertyManager.SetProperty("#Wizard.EPG.Done","yes");
		    GUIWizardCardsDetected.ScanNextCardType();
		}
		
		void MapChannels()
		{
			if (epgGrabberSelected==false) return;

			EPGConfig config = new EPGConfig( @"webepg" );

			config.MaxGrab = 7;

			for (int i=0; i < listGrabbers.Count;++i)
			{
				GUIListItem item = listGrabbers[i];
				if (item.Label.Length > 0 && item.Label2.Length > 0)
				{
					
					EPGConfigData data = new EPGConfigData();
					data.DisplayName = item.Label;
					data.ChannelID = item.Path;
					ChannelInfo selChannel = _epgChannels[item.ItemId];
					GrabberInfo gInfo = (GrabberInfo) selChannel.GrabberList.GetByIndex(0);
					data.PrimaryGrabberID = gInfo.GrabberID;
					config.Add(data);
//					string xmlId=item.Path;
//					string tvChannelName=item.Label2;
//					int    channelId=item.ItemId;
//					TVDatabase.MapEPGChannel(channelId,tvChannelName,xmlId);
				}
			}
			config.Save();
		}

		void OnMap()
		{
			GUIListItem item=listGrabbers.SelectedListItem;
			if (item==null) return;

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			dlg.Reset();
			dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
			dlg.ShowQuickNumbers=false;

			int selected=0;
			int count=0;
			foreach( ChannelInfo chan in _epgChannels )
			{
				dlg.Add(chan.FullName);
				if (chan.FullName == item.Label2) selected = count;
				count++;
			}

			dlg.SelectedLabel=selected;
			dlg.ShowQuickNumbers=false;
			dlg.DoModal(GetID);
			if (dlg.SelectedLabel < 0 || dlg.SelectedLabel >= _epgChannels.Length) return;
			ChannelInfo selChannel = _epgChannels[dlg.SelectedLabel];
			item.Label2=selChannel.FullName;
			item.Path=selChannel.ChannelID;
			item.ItemId = dlg.SelectedLabel;

		//			ArrayList channels = new ArrayList();
		//			TVDatabase.GetChannels(ref channels);
		//			channels.Sort (this);
		//			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
		//			dlg.Reset();
		//			dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
		//			dlg.ShowQuickNumbers=false;
		//			int selected=0;
		//			int count=0;
		//			foreach (TVChannel chan in channels)
		//			{
		//				bool add=true;
		//				/*
		//				for (int i=0; i < listGrabbers.Count;++i)
		//				{
		//					if (listGrabbers[i].Label2==chan.Name) add=false;
		//				}*/
		//				if (add)
		//				{
		//					dlg.Add(chan.Name);
		//					if (chan.Name==item.Label2) selected=count;
		//				}
		//				count++;
		//			}
		//			dlg.SelectedLabel=selected;
		//			dlg.ShowQuickNumbers=false;
		//			dlg.DoModal(GetID);
		//			if (dlg.SelectedLabel<0 || dlg.SelectedLabel>=channels.Count) return;
		//			TVChannel selChannel =(TVChannel)channels[dlg.SelectedLabel];
		//			item.Label2=selChannel.Name;
		//			item.ItemId=selChannel.ID;
	}
		void ShowError(string line1, string line2)
		{
			GUIDialogOK pDlgOK	= (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
			pDlgOK.SetHeading(608);
			pDlgOK.SetLine(1,line1);
			pDlgOK.SetLine(2,line2);
			pDlgOK.DoModal(GetID);
			return;
		}
		#region IComparer Members

		public int Compare(object x, object y)
		{
			TVChannel ch1=(TVChannel)x;
			TVChannel ch2=(TVChannel)y;
			return String.Compare(ch1.Name,ch2.Name,true);
		}

		#endregion
	}
}
