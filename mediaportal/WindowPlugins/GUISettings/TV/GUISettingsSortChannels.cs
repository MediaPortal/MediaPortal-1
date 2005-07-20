using System;
using MediaPortal.Dialogs;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.Util;

namespace WindowPlugins.GUISettings.TV
{
	/// <summary>
	/// Summary description for GUISettingsSortChannels.
	/// </summary>
	public class GUISettingsSortChannels : GUIWindow, IComparer
	{
		[SkinControlAttribute(24)]		protected GUIButtonControl btnTvGroup=null;
		[SkinControlAttribute(10)]		protected GUIUpDownListControl listChannels=null;

		TVGroup currentGroup=null;
		public GUISettingsSortChannels()
		{
			GetID=(int)GUIWindow.Window.WINDOW_SETTINGS_SORT_CHANNELS;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\settings_tvSort.xml");
		}
		protected override void OnPageLoad()
		{
			currentGroup=null;
			base.OnPageLoad ();
			UpdateList();
		}

		void UpdateList()
		{
			ArrayList tvChannels = new ArrayList();
			listChannels.Clear();
			if (currentGroup==null)
			{
				TVDatabase.GetChannels(ref tvChannels);
				int count=0;
				foreach (TVChannel chan in tvChannels)
				{
					if (chan.Sort!=count)
					{
						chan.Sort=count;
						TVDatabase.SetChannelSort(chan.Name,chan.Sort);
					}
					GUIListItem item = new GUIListItem();
					item.Label=chan.Name;
					item.MusicTag=chan;
					string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,chan.Name);
					if (!System.IO.File.Exists(strLogo))
					{
						strLogo="defaultVideoBig.png";
					}
					item.ThumbnailImage=strLogo;
					item.IconImage=strLogo;
					item.IconImageBig=strLogo;
					listChannels.Add(item);
					count++;
				}
			}
			else
			{
				int count=0;
				foreach (TVChannel chan in currentGroup.tvChannels)
				{
					chan.Sort=count;
					GUIListItem item = new GUIListItem();
					item.Label=chan.Name;
					item.MusicTag=chan;
					string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,chan.Name);
					if (!System.IO.File.Exists(strLogo))
					{
						strLogo="defaultVideoBig.png";
					}
					item.ThumbnailImage=strLogo;
					item.IconImage=strLogo;
					item.IconImageBig=strLogo;
					listChannels.Add(item);
					count++;
				}
			}
		}


		protected override void OnClickedUp( int controlId, GUIControl control, Action.ActionType actionType) 
		{
			if (control == listChannels)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,control.GetID,0,0,null);
				OnMessage(msg);         
				int iItem=(int)msg.Param1;
				OnMoveUp(iItem);
			}
		}
		protected override void OnClickedDown( int controlId, GUIControl control, Action.ActionType actionType) 
		{
			if (control == listChannels)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,control.GetID,0,0,null);
				OnMessage(msg);         
				int iItem=(int)msg.Param1;
				OnMoveDown(iItem);
			}
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==btnTvGroup) OnTvGroup();
			base.OnClicked (controlId, control, actionType);
		}


		void OnMoveDown(int item)
		{
			if (item+1 >= listChannels.Count) return;
			GUIListItem item1 = listChannels[item];
			TVChannel chan1 = (TVChannel)item1.MusicTag;

			GUIListItem item2 = listChannels[item+1];
			TVChannel chan2 = (TVChannel)item2.MusicTag;

			int prio=chan1.Sort;
			chan1.Sort=chan2.Sort;
			chan2.Sort=prio;
			if (currentGroup==null)
			{
				TVDatabase.SetChannelSort(chan1.Name,chan1.Sort);
				TVDatabase.SetChannelSort(chan2.Name,chan2.Sort);
			}
			else 
			{
				foreach (TVChannel ch in currentGroup.tvChannels)
				{
					if (ch.Name==chan1.Name) ch.Sort=chan1.Sort;
					if (ch.Name==chan2.Name) ch.Sort=chan2.Sort;
				}
				SaveGroup();
			}
			UpdateList();
			listChannels.SelectedListItemIndex=item+1;
		}
		
		void OnMoveUp(int item)
		{
			if (item < 1) return;
			GUIListItem item1 = listChannels[item];
			TVChannel chan1 = (TVChannel)item1.MusicTag;

			GUIListItem item2 = listChannels[item-1];
			TVChannel chan2 = (TVChannel)item2.MusicTag;

			int prio=chan1.Sort;
			chan1.Sort=chan2.Sort;
			chan2.Sort=prio;
			if (currentGroup==null)
			{
				TVDatabase.SetChannelSort(chan1.Name,chan1.Sort);
				TVDatabase.SetChannelSort(chan2.Name,chan2.Sort);
			}
			else 
			{
				foreach (TVChannel ch in currentGroup.tvChannels)
				{
					if (ch.Name==chan1.Name) ch.Sort=chan1.Sort;
					if (ch.Name==chan2.Name) ch.Sort=chan2.Sort;
				}
				SaveGroup();
			}
			UpdateList();
			listChannels.SelectedListItemIndex=item-1;
		}

		void OnTvGroup()
		{
			ArrayList tvGroups = new ArrayList();
			TVDatabase.GetGroups(ref tvGroups);
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
				dlg.SelectedLabel=0;
				dlg.Add("All channels");
				for (int i=0; i < tvGroups.Count;++i)
				{
					TVGroup group=(TVGroup)tvGroups[i];
					dlg.Add(group.GroupName);
					if (currentGroup!=null)
					{
						if (group.GroupName==currentGroup.GroupName)
						{
							dlg.SelectedLabel=i+1;
						}
					}
				}
				dlg.DoModal(GetID);
				if (dlg.SelectedLabel==0)
				{
					currentGroup=null;
					UpdateList();
					return;
				}

				if (dlg.SelectedLabel>0)
				{
					
					currentGroup=(TVGroup)tvGroups[dlg.SelectedLabel-1];
					UpdateList();
				}
			}
		}
		void SaveGroup()
		{
			if (currentGroup==null) return;
			foreach (TVChannel ch in currentGroup.tvChannels)
			{
				TVDatabase.UnmapChannelFromGroup(currentGroup,ch);
			}
			currentGroup.tvChannels.Sort(this);
			foreach (TVChannel ch in currentGroup.tvChannels)
			{
				TVDatabase.MapChannelToGroup(currentGroup,ch);
			}
		}
		#region IComparer Members

		public int Compare(object x, object y)
		{
			TVChannel ch1=(TVChannel)x;
			TVChannel ch2=(TVChannel)y;
			if (ch1.Sort<ch2.Sort) return -1;
			if (ch1.Sort>ch2.Sort) return 1;
			return 0;
		}

		#endregion
	}
}
