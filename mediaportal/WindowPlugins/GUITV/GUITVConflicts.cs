using System;
using System.Collections;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Player;

namespace MediaPortal.GUI.TV
{
	/// <summary>
	/// 
	/// </summary>
	public class GUITVConflicts :GUIWindow
	{
		#region variables, ctor/dtor

		[SkinControlAttribute(10)]			protected GUIListControl listConflicts=null;
		int								m_iSelectedItem=0;
		TVRecording				currentShow=null;

		public  GUITVConflicts()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TV_CONFLICTS;
		}
		~GUITVConflicts()
		{
		}
    
		#endregion

		#region overrides
		public override bool Init()
		{
			bool bResult=Load (GUIGraphicsContext.Skin+@"\mytvconflicts.xml");
			return bResult;
		}


		public override void OnAction(Action action)
		{
			switch (action.wID)
			{
				case Action.ActionType.ACTION_SHOW_GUI:
					if ( !g_Player.Playing && Recorder.IsViewing())
					{
						//if we're watching tv
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
					}
					else if (g_Player.Playing  && g_Player.IsTV && !g_Player.IsTVRecording)
					{
						//if we're watching a tv recording
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
					}
					else if (g_Player.Playing&&g_Player.HasVideo)
					{
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
					}
					break;
			}
			base.OnAction(action);
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
					
			LoadDirectory();
					
			while (m_iSelectedItem>=GetItemCount() && m_iSelectedItem>0) m_iSelectedItem--;
			GUIControl.SelectItemControl(GetID,listConflicts.GetID,m_iSelectedItem);

		}
		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
			m_iSelectedItem=GetSelectedItemNo();
					
			if ( !GUITVHome.IsTVWindow(newWindowId) )
			{
				if (! g_Player.Playing)
				{
					if (GUIGraphicsContext.ShowBackground)
					{
						// stop timeshifting & viewing... 
	              
						Recorder.StopViewing();
					}
				}
			}
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control == listConflicts)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,control.GetID,0,0,null);
				OnMessage(msg);         
				int iItem=(int)msg.Param1;
				if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
				{
					OnClick(iItem);
				}
			}

		}

		protected override void OnShowContextMenu()
		{
			OnClick(GetSelectedItemNo());
		}


		#endregion

		#region list management 
		GUIListItem GetSelectedItem()
		{
			return listConflicts.SelectedListItem;
		}

		GUIListItem GetItem(int index)
		{
			if (index < 0 || index >=listConflicts.Count) return null;
			return listConflicts[index];
		}

		int GetSelectedItemNo()
		{
			GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,listConflicts.GetID,0,0,null);
			OnMessage(msg);         
			int iItem=(int)msg.Param1;
			return iItem;
		}
		int GetItemCount()
		{
			return listConflicts.Count;
		}
		#endregion

		#region scheduled tv methods
		void LoadDirectory()
		{
			GUIControl.ClearControl(GetID,listConflicts.GetID);
			GUIControl cntlLabel ;

			string strObjects;
			int total=0;
			if (currentShow!=null)
			{
				GUIListItem item= new GUIListItem();
				item.Label		  = "..";
				item.IsFolder=true;
				Utils.SetDefaultIcons(item);
				listConflicts.Add(item);
				TVRecording[] conflicts=ConflictManager.GetConflictingRecordings(currentShow);
				for (int i=0; i < conflicts.Length;++i)
				{
					item=new GUIListItem();
					item.Label=conflicts[i].Title;
					item.TVTag=conflicts[i];
					string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,conflicts[i].Channel);
					if (!System.IO.File.Exists(strLogo))
					{
						strLogo="defaultVideoBig.png";
					}
					item.PinImage=Thumbs.TvConflictRecordingIcon;
					item.ThumbnailImage=strLogo;
					item.IconImageBig=strLogo;
					item.IconImage=strLogo;
					listConflicts.Add(item);
					total++;
				}
				strObjects=String.Format("{0} {1}", total, GUILocalizeStrings.Get(632));
				GUIPropertyManager.SetProperty("#itemcount",strObjects);
				cntlLabel = GetControl(12);
				if (cntlLabel!=null)
					cntlLabel.YPosition = listConflicts.SpinY;

				SetLabels();
				return;
			}
			ArrayList itemlist = new ArrayList();
			TVDatabase.GetRecordings(ref itemlist);
			foreach (TVRecording rec in itemlist)
			{
				if (!ConflictManager.IsConflict(rec)) continue;
				ArrayList recs = ConflictManager.Util.GetRecordingTimes(rec);
				if (recs.Count>= 1)
				{
					for (int x=0; x < recs.Count;++x)
					{
						TVRecording recSeries=(TVRecording )recs[x];
						if (DateTime.Now > recSeries.EndTime) continue;
						if (recSeries.Canceled!=0) continue;						
					
						GUIListItem item=new GUIListItem();
						item.Label=recSeries.Title;
						item.TVTag=recSeries;
						string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,recSeries.Channel);
						if (!System.IO.File.Exists(strLogo))
						{
							strLogo="defaultVideoBig.png";
						}
								item.PinImage=Thumbs.TvConflictRecordingIcon;
						item.ThumbnailImage=strLogo;
						item.IconImageBig=strLogo;
						item.IconImage=strLogo;
						item.IsFolder=true;
						listConflicts.Add(item);
						total++;
					}
				}
			}
      
			strObjects=String.Format("{0} {1}", total, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
			cntlLabel = GetControl(12);
			if (cntlLabel!=null)
				cntlLabel.YPosition = listConflicts.SpinY;

			SetLabels();
		}


		void SetLabels()
		{
			for (int i=0; i < GetItemCount();++i)
			{
				GUIListItem item=GetItem(i);
				if (item.IsFolder && item.Label.Equals("..")) continue;
				TVRecording rec=(TVRecording)item.TVTag;
				item.Label=rec.Title;
				string strTime=String.Format("{0} {1} - {2}", 
					rec.StartTime.ToShortDateString() , 
					rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
					rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
				string strType="";
				switch (rec.RecType)
				{
					case TVRecording.RecordingType.Once:
						item.Label2=String.Format("{0} {1} - {2}", 
							Utils.GetShortDayString(rec.StartTime) , 
							rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
							rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));;
						break;
					case TVRecording.RecordingType.Daily:
						strTime=String.Format("{0}-{1}", 
							rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
							rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
						strType=GUILocalizeStrings.Get(648) ;
						item.Label2=String.Format("{0} {1}",strType,strTime);
						break;

					case TVRecording.RecordingType.WeekDays:
						strTime=String.Format("{0}-{1} {2}-{3}",
							GUILocalizeStrings.Get(657),//657=Mon
							GUILocalizeStrings.Get(661),//661=Fri
							rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
							rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
						strType=GUILocalizeStrings.Get(648) ;
						item.Label2=String.Format("{0} {1}",strType,strTime);
						break;

					case TVRecording.RecordingType.Weekly:
						string day;
						switch (rec.StartTime.DayOfWeek)
						{
							case DayOfWeek.Monday :	day = GUILocalizeStrings.Get(11);	break;
							case DayOfWeek.Tuesday :	day = GUILocalizeStrings.Get(12);	break;
							case DayOfWeek.Wednesday :	day = GUILocalizeStrings.Get(13);	break;
							case DayOfWeek.Thursday :	day = GUILocalizeStrings.Get(14);	break;
							case DayOfWeek.Friday :	day = GUILocalizeStrings.Get(15);	break;
							case DayOfWeek.Saturday :	day = GUILocalizeStrings.Get(16);	break;
							default:	day = GUILocalizeStrings.Get(17);	break;
						}

						strTime=String.Format("{0}-{1}", 
							rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
							rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
						strType=GUILocalizeStrings.Get(649) ;
						item.Label2=String.Format("{0} {1} {2}",strType,day,strTime);
						break;
					case TVRecording.RecordingType.EveryTimeOnThisChannel:
						item.Label=rec.Title;
						item.Label2=GUILocalizeStrings.Get(650);
						break;
					case TVRecording.RecordingType.EveryTimeOnEveryChannel:
						item.Label=rec.Title;
						item.Label2=GUILocalizeStrings.Get(651);
						break;
				}
			}
		}

		void OnClick(int iItem)
		{
			m_iSelectedItem=GetSelectedItemNo();
			GUIListItem item = GetItem(iItem);
			if (item==null) return;
			TVRecording rec=item.TVTag as TVRecording;
			if (item.IsFolder)
			{
				if (item.Label=="..")
				{
					currentShow=null;
					LoadDirectory();
					return;
				}
				currentShow=rec;
				LoadDirectory();
				return;
			}
		}
    
		string GetRecType(TVRecording.RecordingType recType)
		{
			string strType=String.Empty;
			switch(recType)
			{
				case TVRecording.RecordingType.Daily:
					strType=GUILocalizeStrings.Get(648);//daily
					break;
				case TVRecording.RecordingType.EveryTimeOnEveryChannel:
					strType=GUILocalizeStrings.Get(651);//Everytime on any channel
					break;
				case TVRecording.RecordingType.EveryTimeOnThisChannel:
					strType=GUILocalizeStrings.Get(650);//Everytime on this channel
					break;
				case TVRecording.RecordingType.Once:
					strType=GUILocalizeStrings.Get(647);//Once
					break;
				case TVRecording.RecordingType.WeekDays:
					strType=GUILocalizeStrings.Get(680);//Mon-Fri
					break;
				case TVRecording.RecordingType.Weekly:
					strType=GUILocalizeStrings.Get(679);//Weekly
					break;
			}
			return strType;
		}
		#endregion
	}
}
