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
	public class GUITVPriorities :GUIWindow
	{
		public class PriorityComparer: IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				TVRecording rec1=x as TVRecording;
				TVRecording rec2=y as TVRecording;
				if (rec1.Priority>rec2.Priority) return -1;
				if (rec1.Priority<rec2.Priority) return 1;
				return 0;
			}

			#endregion
		};
		#region variables, ctor/dtor
		[SkinControlAttribute(10)]			protected GUIListControl listSchedules=null;

		int								m_iSelectedItem=0;
		TVUtil						util =null;

		public  GUITVPriorities()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TV_SCHEDULER_PRIORITIES;
		}
		~GUITVPriorities()
		{
		}
    
		#endregion


		#region overrides
		public override bool Init()
		{
			bool bResult=Load (GUIGraphicsContext.Skin+@"\mytvpriorities.xml");
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
			if (util==null)
			{
				util = new TVUtil(31);
			}
					
			LoadDirectory();
					
			while (m_iSelectedItem>=GetItemCount() && m_iSelectedItem>0) m_iSelectedItem--;
			GUIControl.SelectItemControl(GetID,listSchedules.GetID,m_iSelectedItem);

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
			if (control == listSchedules)
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

		protected override void OnClickedUp( int controlId, GUIControl control, Action.ActionType actionType) 
		{
			if (control == listSchedules)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,control.GetID,0,0,null);
				OnMessage(msg);         
				int iItem=(int)msg.Param1;
				OnMoveUp(iItem);
			}
		}
		protected override void OnClickedDown( int controlId, GUIControl control, Action.ActionType actionType) 
		{
			if (control == listSchedules)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,control.GetID,0,0,null);
				OnMessage(msg);         
				int iItem=(int)msg.Param1;
				OnMoveDown(iItem);
			}
		}

		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{
				case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
					UpdateDescription();
					break;
			}
			return base.OnMessage(message);
		}
		protected override void OnShowContextMenu()
		{
			OnClick(GetSelectedItemNo());
		}


		#endregion

		#region list management 
		GUIListItem GetSelectedItem()
		{
			return listSchedules.SelectedListItem;
		}

		GUIListItem GetItem(int index)
		{
			if (index < 0 || index >=listSchedules.Count) return null;
			return listSchedules[index];
		}

		int GetSelectedItemNo()
		{
			GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,listSchedules.GetID,0,0,null);
			OnMessage(msg);         
			int iItem=(int)msg.Param1;
			return iItem;
		}
		int GetItemCount()
		{
			return listSchedules.Count;
		}
		#endregion


		#region scheduled tv methods
		void LoadDirectory()
		{
			GUIControl.ClearControl(GetID,listSchedules.GetID);

			ArrayList itemlist = new ArrayList();
			TVDatabase.GetRecordings(ref itemlist);
			itemlist.Sort(new PriorityComparer());
			int total=0;
			foreach (TVRecording rec in itemlist)
			{
				GUIListItem item= new GUIListItem();
				item.Label		  = rec.Title;
				item.TVTag			= rec;
				string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,rec.Channel);
				if (!System.IO.File.Exists(strLogo))
				{
					strLogo="defaultVideoBig.png";
				}
				int card;
				if (Recorder.IsRecordingSchedule(rec,out card))
				{
					if (rec.RecType !=TVRecording.RecordingType.Once)
						item.PinImage="tvguide_recordserie_button.png";
					else
						item.PinImage="tvguide_record_button.png";
				}
				item.ThumbnailImage=strLogo;
				item.IconImageBig=strLogo;
				item.IconImage=strLogo;
				listSchedules.Add(item);
				total++;
			}
      
			string strObjects=String.Format("{0} {1}", total, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
			GUIControl.SelectItemControl(GetID,listSchedules.GetID,m_iSelectedItem);

		}
		void SetLabels()
		{

			for (int i=0; i < GetItemCount();++i)
			{
				GUIListItem item=GetItem(i);
				TVRecording rec=(TVRecording)item.TVTag;
        
				switch (rec.Status)
				{
					case TVRecording.RecordingStatus.Waiting:
						item.Label3=GUILocalizeStrings.Get(681);//waiting
						break;
					case TVRecording.RecordingStatus.Finished:
						item.Label3=GUILocalizeStrings.Get(683);//Finished
						break;
					case TVRecording.RecordingStatus.Canceled:
						item.Label3=GUILocalizeStrings.Get(684);//Canceled
						break;
				}

				// check with recorder.
				int card;
				if (Recorder.IsRecordingSchedule(rec, out card))
				{
					item.Label3=GUILocalizeStrings.Get(682);//Recording
					if (rec.RecType !=TVRecording.RecordingType.Once)
						item.PinImage="tvguide_recordserie_button.png";
					else
						item.PinImage="tvguide_record_button.png";
				}
				else 
				{
					item.PinImage=String.Empty;
				}
        
				string strType=String.Empty;
				item.Label=rec.Title;
				string strTime=String.Format("{0} {1} - {2}", 
					rec.StartTime.ToShortDateString() , 
					rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
					rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
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
			TVRecording rec=(TVRecording)item.TVTag;

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
      
			dlg.Reset();
			dlg.SetHeading(rec.Title);
			
			if (rec.Series==false)
			{
				dlg.AddLocalizedString( 618);//delete
			}
			else
			{
				dlg.AddLocalizedString( 981);//Delete this recording
				dlg.AddLocalizedString( 982);//Delete series recording
			}
			int card;
			if (Recorder.IsRecordingSchedule(rec,out card))
			{
				dlg.AddLocalizedString( 979); //Play recording from beginning
				dlg.AddLocalizedString( 980); //Play recording from live point
			}

			dlg.DoModal( GetID);
			if (dlg.SelectedLabel==-1) return;
			switch (dlg.SelectedId)
			{
				case 981: //Delete this recording only
				{
					if (Recorder.IsRecordingSchedule(rec, out card))
					{
						GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
						if (null != dlgYesNo)
						{
							dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));//Delete this recording?
							dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730));//This schedule is recording. If you delete
							dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731));//the schedule then the recording is stopped.
							dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732));//are you sure
							dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

							if (dlgYesNo.IsConfirmed) 
							{
								Recorder.StopRecording(rec);
								rec.CanceledSeries.Add(Utils.datetolong(rec.StartTime));
								TVDatabase.AddCanceledSerie(rec,Utils.datetolong(rec.StartTime));
							}
						}
					}
					else
					{
						rec.CanceledSeries.Add(Utils.datetolong(rec.StartTime));
						TVDatabase.AddCanceledSerie(rec,Utils.datetolong(rec.StartTime));
					}
					LoadDirectory();
				}
					break;

				case 982: //Delete series recording
					goto case 618;

				case 618: // delete entire recording
				{
					if (Recorder.IsRecordingSchedule(rec, out card))
					{
						GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
						if (null != dlgYesNo)
						{
							dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));//Delete this recording?
							dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730));//This schedule is recording. If you delete
							dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731));//the schedule then the recording is stopped.
							dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732));//are you sure
							dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

							if (dlgYesNo.IsConfirmed) 
							{
								Recorder.StopRecording(rec);
								TVDatabase.RemoveRecording(rec);
							}
						}
					}
					else
					{
						TVDatabase.RemoveRecording(rec);
					}
					LoadDirectory();
				}
					break;

				case 979: // Play recording from beginning
					if (g_Player.Playing && g_Player.IsTVRecording)
					{
						g_Player.Stop();
					}
					GUITVHome.IsTVOn=true;
					GUITVHome.ViewChannel(rec.Channel);
					g_Player.SeekAbsolute(0);
					if (Recorder.IsViewing())
					{
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
						return;
					}
					break;

				case 980: // Play recording from live point
					GUITVHome.IsTVOn=true;
					GUITVHome.ViewChannel(rec.Channel);
					if (Recorder.IsViewing())
					{
						if (g_Player.Playing)
						{
							g_Player.SeekAsolutePercentage(99);
						}
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
						return;
					}
					break;
			}
			while (m_iSelectedItem>=GetItemCount() && m_iSelectedItem>0) m_iSelectedItem--;
			GUIControl.SelectItemControl(GetID,listSchedules.GetID,m_iSelectedItem);
		}
    
		void ChangeType(TVRecording rec)
		{
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(616));//616=Select Recording type
				//611=Record once
				//612=Record everytime on this channel
				//613=Record everytime on every channel
				//614=Record every week at this time
				//615=Record every day at this time
				for (int i=611; i <= 615; ++i)
				{
					dlg.Add( GUILocalizeStrings.Get(i));
				}
				dlg.Add( GUILocalizeStrings.Get(672));// 672=Record Mon-Fri
				switch (rec.RecType)
				{
					case TVRecording.RecordingType.Once:
						dlg.SelectedLabel=0;
						break;
					case TVRecording.RecordingType.EveryTimeOnThisChannel:
						dlg.SelectedLabel=1;
						break;
					case TVRecording.RecordingType.EveryTimeOnEveryChannel:
						dlg.SelectedLabel=2;
						break;
					case TVRecording.RecordingType.Weekly:
						dlg.SelectedLabel=3;
						break;
					case TVRecording.RecordingType.Daily:
						dlg.SelectedLabel=4;
						break;
					case TVRecording.RecordingType.WeekDays:
						dlg.SelectedLabel=5;
						break;
				}
				dlg.DoModal( GetID);
				if (dlg.SelectedLabel==-1) return;
				switch (dlg.SelectedLabel)
				{
					case 0://once
						rec.RecType=TVRecording.RecordingType.Once;
						rec.Canceled=0;
						break;
					case 1://everytime, this channel
						rec.RecType=TVRecording.RecordingType.EveryTimeOnThisChannel;
						rec.Canceled=0;
						break;
					case 2://everytime, all channels
						rec.RecType=TVRecording.RecordingType.EveryTimeOnEveryChannel;
						rec.Canceled=0;
						break;
					case 3://weekly
						rec.RecType=TVRecording.RecordingType.Weekly;
						rec.Canceled=0;
						break;
					case 4://daily
						rec.RecType=TVRecording.RecordingType.Daily;
						rec.Canceled=0;
						break;
					case 5://Mo-Fi
						rec.RecType=TVRecording.RecordingType.WeekDays;
						rec.Canceled=0;
						break;
				}
				TVDatabase.UpdateRecording(rec);
				LoadDirectory();

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

		void OnMoveDown(int item)
		{

			if (item==GetItemCount()-1) return;
			m_iSelectedItem=item+1;
			GUIListItem pItem=GetItem( GetSelectedItemNo() );
			if (pItem==null)
			{
				return;
			}
			TVRecording rec=pItem.TVTag as TVRecording;
			if (rec==null) return;
			GUIListItem tmpItem;
			TVRecording tmprec;
			//0
			//1
			//2 ---->3
			//3 ----
			//4
			//5

			for (int i=0; i < item;++i)
			{
				tmpItem=GetItem( i );
				tmprec=tmpItem.TVTag as TVRecording;
				tmprec.Priority=TVRecording.HighestPriority-i;
				TVDatabase.UpdateRecording(tmprec);
			}
			tmpItem=GetItem( item+1 );
			tmprec=tmpItem.TVTag as TVRecording;
			tmprec.Priority=TVRecording.HighestPriority-item;
			TVDatabase.UpdateRecording(tmprec);
			for (int i=item+2; i < GetItemCount();++i)
			{
				tmpItem=GetItem( i );
				tmprec=tmpItem.TVTag as TVRecording;
				tmprec.Priority=TVRecording.HighestPriority-i;
				TVDatabase.UpdateRecording(tmprec);
			}
			
			rec.Priority=TVRecording.HighestPriority-item-1;
			TVDatabase.UpdateRecording(rec);
			LoadDirectory();
		}

		void OnMoveUp(int item)
		{
			if (item==0) return;
			m_iSelectedItem=item-1;
			GUIListItem pItem=GetItem( GetSelectedItemNo() );
			if (pItem==null)
			{
				return;
			}
			TVRecording rec=pItem.TVTag as TVRecording;
			if (rec==null) return;
			GUIListItem tmpItem;
			TVRecording tmprec;

			for (int i=0; i < item-1;++i)
			{
				tmpItem=GetItem( i );
				tmprec=tmpItem.TVTag as TVRecording;
				tmprec.Priority=TVRecording.HighestPriority-i;
				TVDatabase.UpdateRecording(tmprec);
			}
			for (int i=item-1; i < GetItemCount();++i)
			{
				if (item==i) continue;
				tmpItem=GetItem( i );
				tmprec=tmpItem.TVTag as TVRecording;
				tmprec.Priority=TVRecording.HighestPriority-i-1;
				TVDatabase.UpdateRecording(tmprec);
			}
			
			rec.Priority=TVRecording.HighestPriority-item+1;
			TVDatabase.UpdateRecording(rec);
			LoadDirectory();
		}

		void UpdateDescription()
		{
			TVRecording rec = new TVRecording();
			rec.SetProperties(null);
			GUIListItem pItem=GetItem( GetSelectedItemNo() );
			if (pItem==null)
			{
				return;
			}
			rec=pItem.TVTag as TVRecording;
			if (rec==null) return;
			TVProgram prog=util.GetProgramAt(rec.Channel,rec.StartTime);
			rec.SetProperties(prog);
		}
		#endregion
	}
}
