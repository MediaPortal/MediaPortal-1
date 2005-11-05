using System;
using System.Collections;
using System.Globalization;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;

namespace MediaPortal.GUI.TV
{
	/// <summary>
	/// Summary description for GUITVProgramInfo.
	/// </summary>
	public class GUITVProgramInfo : GUIWindow
	{
		[SkinControlAttribute(17)]			  protected GUILabelControl					lblProgramGenre=null;
		[SkinControlAttribute(15)]			  protected GUITextScrollUpControl	lblProgramDescription=null;
		[SkinControlAttribute(14)]			  protected GUILabelControl					lblProgramTime=null;
		[SkinControlAttribute(13)]			  protected GUIFadeLabel						lblProgramTitle=null;
		[SkinControlAttribute(2)]					protected GUIButtonControl				btnRecord=null;
		[SkinControlAttribute(3)]					protected GUIButtonControl				btnAdvancedRecord=null;
		[SkinControlAttribute(4)]					protected GUIButtonControl				btnKeep=null;
		[SkinControlAttribute(5)]					protected GUIToggleButtonControl	btnNotify=null;
		[SkinControlAttribute(10)]				protected GUIListControl					lstUpcomingEpsiodes=null;
		[SkinControlAttribute(6)]					protected GUIButtonControl				btnQuality=null;
		[SkinControlAttribute(7)]					protected GUIButtonControl				btnEpisodes=null;
		[SkinControlAttribute(8)]					protected GUIButtonControl				btnPreRecord=null;
		[SkinControlAttribute(9)]					protected GUIButtonControl				btnPostRecord=null;

		static TVProgram currentProgram=null;

		public GUITVProgramInfo()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO;//748
		}
		public override bool Init()
		{
			bool bResult=Load (GUIGraphicsContext.Skin+@"\mytvprogram.xml");
			return bResult;
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			Update();
		}

		static public TVProgram CurrentProgram
		{
			get { return currentProgram;}
			set { currentProgram=value;}
		}
    static public TVRecording CurrentRecording
    {
      set
      {
        ArrayList programs = new ArrayList();
        TVDatabase.GetProgramTitles(value.Start,Utils.datetolong(DateTime.Now.AddMonths(1)), ref programs);
        foreach (TVProgram prog in programs)
        {
          if (value.IsRecordingProgram(prog,false))
          {
            CurrentProgram=prog;
            break;
          }
        }
      }
    }

		void UpdateProgramDescription(TVRecording rec)
		{
			if (rec==null) return;
			ArrayList progs = new ArrayList();
			TVDatabase.GetProgramsPerChannel(rec.Channel,rec.Start,rec.End,ref progs);
			if (progs.Count>0)
			{
				foreach (TVProgram prog in progs)
				{
					if (prog.Start==rec.Start && prog.End==rec.End && prog.Channel==rec.Channel)
					{
						currentProgram=prog;
						break;
					}
				}
			}
			if (currentProgram!=null)
			{
				string strTime=String.Format("{0} {1} - {2}", 
					Utils.GetShortDayString(currentProgram.StartTime) , 
					currentProgram.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
					currentProgram.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));

				lblProgramGenre.Label=currentProgram.Genre;
				lblProgramTime.Label=strTime;
				lblProgramDescription.Label=currentProgram.Description;
				lblProgramTitle.Label=currentProgram.Title;
			}
		}

		void Update()
		{
			lstUpcomingEpsiodes.Clear();
			if (currentProgram==null) return;

			string strTime=String.Format("{0} {1} - {2}", 
				Utils.GetShortDayString(currentProgram.StartTime) , 
				currentProgram.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
				currentProgram.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));

			lblProgramGenre.Label=currentProgram.Genre;
			lblProgramTime.Label=strTime;
			lblProgramDescription.Label=currentProgram.Description;
			lblProgramTitle.Label=currentProgram.Title;


			ArrayList recordings = new ArrayList();
			TVDatabase.GetRecordings(ref recordings);
			bool bRecording=false;
			bool bSeries=false;
			foreach (TVRecording record in recordings)
			{
				if (record.Canceled>0) continue;
				if (record.IsRecordingProgram(currentProgram,true) ) 
				{
					if (!record.IsSerieIsCanceled(currentProgram.StartTime))
					{
						if (record.RecType!=TVRecording.RecordingType.Once)
							bSeries=true;
						bRecording=true;
						break;
					}
				}
			}

			if (bRecording)
			{
				btnRecord.Label=GUILocalizeStrings.Get(1039);//dont record
				btnAdvancedRecord.Disabled=true;
				btnKeep.Disabled=false;
				btnQuality.Disabled=false;
				btnEpisodes.Disabled=!bSeries;
				btnPreRecord.Disabled=false;	
				btnPostRecord.Disabled=false;
			}
			else
			{
				btnRecord.Label=GUILocalizeStrings.Get(264);//record
				btnAdvancedRecord.Disabled=false;
				btnKeep.Disabled=true;		
				btnQuality.Disabled=true;
				btnEpisodes.Disabled=true;	
				btnPreRecord.Disabled=true;	
				btnPostRecord.Disabled=true;
			}
			ArrayList notifies = new ArrayList();
			TVDatabase.GetNotifies(notifies,false);
			bool showNotify=false;
			foreach (TVNotify notify in notifies)
			{
				if (notify.Program.ID==currentProgram.ID)
				{
					showNotify=true;
					break;
				}
			}
			if (showNotify)
				btnNotify.Selected=true;
			else
				btnNotify.Selected=false;



			lstUpcomingEpsiodes.Clear();
			TVRecording recTmp = new TVRecording();
			recTmp.Channel = currentProgram.Channel;
			recTmp.Title		=currentProgram.Title;
			recTmp.Start   = currentProgram.Start;
			recTmp.End			= currentProgram.End;
			recTmp.RecType = TVRecording.RecordingType.EveryTimeOnEveryChannel;
			ArrayList recs = ConflictManager.Util.GetRecordingTimes(recTmp);
			foreach (TVRecording recSeries in recs)
			{
				GUIListItem item=new GUIListItem();
				item.Label=recSeries.Title;
				item.TVTag=recSeries;
				item.MusicTag=null;
				item.OnItemSelected+=new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
				string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,recSeries.Channel);
				if (!System.IO.File.Exists(strLogo))
				{
					strLogo="defaultVideoBig.png";
				}
				TVRecording recOrg;
				if (IsRecordingSchedule(recSeries, out recOrg,true))
				{
					item.PinImage=Thumbs.TvRecordingIcon;
					item.MusicTag=recOrg;
				}

				item.ThumbnailImage=strLogo;
				item.IconImageBig=strLogo;
				item.IconImage=strLogo;
				item.Label2=String.Format("{0} {1} - {2}", 
																	Utils.GetShortDayString(recSeries.StartTime) , 
																	recSeries.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
																	recSeries.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));;
				lstUpcomingEpsiodes.Add(item);
			}
		}
		
		bool IsRecordingSchedule(TVRecording rec, out TVRecording recOrg, bool filterOutCanceled)
		{
			recOrg=null;
			ArrayList recordings = new ArrayList();
			TVDatabase.GetRecordings(ref recordings);
			foreach (TVRecording record in recordings)
			{
				if (record.Canceled>0) continue;
				ArrayList recs = ConflictManager.Util.GetRecordingTimes(record);
				foreach (TVRecording recSeries in recs)
				{
					if (!record.IsSerieIsCanceled(recSeries.StartTime) || (filterOutCanceled==false) )
					{
						if (rec.Channel==recSeries.Channel &&
							rec.Title  ==recSeries.Title &&
							rec.Start==recSeries.Start)
						{
							recOrg=record;
							return true;
						}
					}
				}
			}
			return false;
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==btnPreRecord)
				OnPreRecordInterval();
			if (control==btnPostRecord)
				OnPostRecordInterval();
			if (control==btnEpisodes)
				OnSetEpisodes();
			if (control==btnQuality)
				OnSetQuality();
			if (control==btnKeep)
				OnKeep();
			if (control==btnRecord) 
				OnRecordProgram(currentProgram);
			if (control==btnAdvancedRecord) 
				OnAdvancedRecord();
			if (control==btnNotify)
				OnNotify();
			if (control==lstUpcomingEpsiodes)
			{
				GUIListItem item = lstUpcomingEpsiodes.SelectedListItem;
				if (item!=null)
				{
					TVRecording recSeries = item.TVTag as TVRecording;
					TVRecording recOrg    = item.MusicTag as TVRecording;
					OnRecordRecording(recSeries,recOrg);
				}
			}
			base.OnClicked (controlId, control, actionType);
		}
		void OnPreRecordInterval()
		{
			bool bRecording=false;
			TVRecording rec=null;
			ArrayList recordings = new ArrayList();
			TVDatabase.GetRecordings(ref recordings);

			foreach (TVRecording record in recordings)
			{
				if (record.Canceled>0) continue;
				if (record.IsRecordingProgram(currentProgram,true) ) 
				{
					if (!record.IsSerieIsCanceled(currentProgram.StartTime))
					{
						bRecording=true;
						rec=record;
						break;
					}
				}
			}
			if (!bRecording) return;
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.ShowQuickNumbers=false;
				dlg.SetHeading(GUILocalizeStrings.Get(1444));//pre-record
				dlg.Add(GUILocalizeStrings.Get(886));//default
				for (int minute=0; minute < 20; minute++)
				{
					dlg.Add( String.Format("{0} {1}",minute,GUILocalizeStrings.Get(3004)) );
				}
				if (rec.PaddingFront<0) dlg.SelectedLabel=0;
				else dlg.SelectedLabel=rec.PaddingFront+1;
				dlg.DoModal(GetID);
				if (dlg.SelectedLabel<0) return;
				rec.PaddingFront=dlg.SelectedLabel-1;
				TVDatabase.UpdateRecording(rec,TVDatabase.RecordingChange.Modified);
			}
			Update();
		}

		void OnPostRecordInterval()
		{
			bool bRecording=false;
			TVRecording rec=null;
			ArrayList recordings = new ArrayList();
			TVDatabase.GetRecordings(ref recordings);

			foreach (TVRecording record in recordings)
			{
				if (record.Canceled>0) continue;
				if (record.IsRecordingProgram(currentProgram,true) ) 
				{
					if (!record.IsSerieIsCanceled(currentProgram.StartTime))
					{
						bRecording=true;
						rec=record;
						break;
					}
				}
			}
			if (!bRecording) return;
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.ShowQuickNumbers=false;
				dlg.SetHeading(GUILocalizeStrings.Get(1445));//pre-record
				dlg.Add(GUILocalizeStrings.Get(886));//default
				for (int minute=0; minute < 20; minute++)
				{
					dlg.Add(String.Format("{0} {1}",minute,GUILocalizeStrings.Get(3004)));
				}
				if (rec.PaddingEnd<0) dlg.SelectedLabel=0;
				else dlg.SelectedLabel=rec.PaddingEnd+1;
				dlg.DoModal(GetID);
				if (dlg.SelectedLabel<0) return;
				rec.PaddingEnd=dlg.SelectedLabel-1;
				TVDatabase.UpdateRecording(rec,TVDatabase.RecordingChange.Modified);
			}
			Update();
		}

		void OnSetQuality()
		{
			bool bRecording=false;
			TVRecording rec=null;
			ArrayList recordings = new ArrayList();
			TVDatabase.GetRecordings(ref recordings);

			foreach (TVRecording record in recordings)
			{
				if (record.Canceled>0) continue;
				if (record.IsRecordingProgram(currentProgram,true) ) 
				{
					if (!record.IsSerieIsCanceled(currentProgram.StartTime))
					{
						bRecording=true;
						rec=record;
						break;
					}
				}
			}
			if (!bRecording) return;
			GUITVPriorities.OnSetQuality(rec);
			Update();
		}
		void OnSetEpisodes()
		{
			bool bRecording=false;
			TVRecording rec=null;
			ArrayList recordings = new ArrayList();
			TVDatabase.GetRecordings(ref recordings);

			foreach (TVRecording record in recordings)
			{
				if (record.Canceled>0) continue;
				if (record.IsRecordingProgram(currentProgram,true) ) 
				{
					if (!record.IsSerieIsCanceled(currentProgram.StartTime))
					{
						bRecording=true;
						rec=record;
						break;
					}
				}
			}
			if (!bRecording) return;
			GUITVPriorities.OnSetEpisodesToKeep(rec);
			Update();
		}

		void OnRecordRecording(TVRecording recSeries, TVRecording rec)
		{
			if (rec==null)
			{
				//not recording yet.
				TVRecording recOrg;
				if (IsRecordingSchedule(recSeries, out recOrg, false))
				{
					recOrg.UnCancelSerie(recSeries.StartTime);
					TVDatabase.UpdateRecording(recOrg,TVDatabase.RecordingChange.Modified);
				}
				else
				{
					recSeries.RecType=TVRecording.RecordingType.Once;
					Recorder.AddRecording(ref recSeries);
				}																	
				Update();
				return;										 
			}
			else
			{
				if (rec.RecType != TVRecording.RecordingType.Once)
				{
					GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
					if (dlg==null) return;
					dlg.Reset();
					dlg.SetHeading(rec.Title);
					dlg.AddLocalizedString( 981);//Delete this recording
					dlg.AddLocalizedString( 982);//Delete series recording
					dlg.DoModal( GetID);
					if (dlg.SelectedLabel==-1) return;
					switch (dlg.SelectedId)
					{
						case 981: //Delete this recording only
						{
							if (CheckIfRecording(rec))
							{
								//delete specific series
								rec.CanceledSeries.Add(recSeries.Start);
								TVDatabase.AddCanceledSerie(rec,recSeries.Start);
								Recorder.StopRecording(rec);
							}
						}
							break;
						case 982: //Delete entire recording
						{
							if (CheckIfRecording(rec))
							{
								//cancel recording
								TVDatabase.RemoveRecording(rec);
								Recorder.StopRecording(rec);
							}
						}
							break;
					}
				}
				else
				{
					if (CheckIfRecording(rec))
					{
						//cancel recording2
						TVDatabase.RemoveRecording(rec);
						Recorder.StopRecording(rec);
					}
				}
			}
			Update();
		}

		void OnRecordProgram(TVProgram program)
		{
			bool bRecording=false;
			TVRecording rec=null;
			ArrayList recordings = new ArrayList();
			TVDatabase.GetRecordings(ref recordings);

			foreach (TVRecording record in recordings)
			{
				if (record.Canceled>0) continue;
				if (record.IsRecordingProgram(program,true) ) 
				{
					if (!record.IsSerieIsCanceled(program.StartTime))
					{
						bRecording=true;
						rec=record;
						break;
					}
				}
			}

			if (!bRecording)
			{
				foreach (TVRecording record in recordings)
				{
					if (record.IsRecordingProgram(program,false) ) 
					{
						if (record.Canceled>0) 
						{
							record.RecType=TVRecording.RecordingType.Once;
							record.Canceled=0;
							TVDatabase.UpdateRecording(record,TVDatabase.RecordingChange.Modified);
						}
						else if (record.IsSerieIsCanceled(program.StartTime))
						{
							record.UnCancelSerie(program.StartTime);
							TVDatabase.UpdateRecording(record,TVDatabase.RecordingChange.Modified);
						}
						Update();
						return;
					}		
				}
				rec=new TVRecording();
				rec.Title=program.Title;
				rec.Channel=program.Channel;
				rec.Start=program.Start;
				rec.End=program.End;
				rec.RecType=TVRecording.RecordingType.Once;
				Recorder.AddRecording(ref rec);
			}
			else
			{
				if (rec.IsRecordingProgram(program,true))
				{
					if (rec.RecType != TVRecording.RecordingType.Once)
					{
						GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
						if (dlg==null) return;
						dlg.Reset();
						dlg.SetHeading(rec.Title);
						dlg.AddLocalizedString( 981);//Delete this recording
						dlg.AddLocalizedString( 982);//Delete series recording
						dlg.DoModal( GetID);
						if (dlg.SelectedLabel==-1) return;
						switch (dlg.SelectedId)
						{
							case 981: //Delete this recording only
							{
								if (CheckIfRecording(rec))
								{
									//delete specific series
									rec.CanceledSeries.Add(program.Start);
									TVDatabase.AddCanceledSerie(rec,program.Start);
									Recorder.StopRecording(rec);
								}
							}
							break;
							case 982: //Delete entire recording
							{
								if (CheckIfRecording(rec))
								{
									//cancel recording
									TVDatabase.RemoveRecording(rec);
									Recorder.StopRecording(rec);
								}
							}
							break;
						}
					}
					else
					{
						if (CheckIfRecording(rec))
						{
							//cancel recording2
							TVDatabase.RemoveRecording(rec);
							Recorder.StopRecording(rec);
						}
					}
				}
			}
			Update();
		}

		void OnAdvancedRecord()
		{
			if (currentProgram==null) return;

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(616));//616=Select Recording type
				//610=None
				//611=Record once
				//612=Record everytime on this channel
				//613=Record everytime on every channel
				//614=Record every week at this time
				//615=Record every day at this time
				for (int i=611; i <= 615; ++i)
				{
					dlg.AddLocalizedString(i);
				}
				dlg.AddLocalizedString( 672);// 672=Record Mon-Fri
				
				dlg.DoModal( GetID);
				if (dlg.SelectedLabel==-1) return;

				TVRecording rec=new TVRecording();
				rec.Title=currentProgram.Title;
				rec.Channel=currentProgram.Channel;
				rec.Start=currentProgram.Start;
				rec.End=currentProgram.End;
				switch (dlg.SelectedId)
				{
					case 611://once
						rec.RecType=TVRecording.RecordingType.Once;
						break;
					case 612://everytime, this channel
						rec.RecType=TVRecording.RecordingType.EveryTimeOnThisChannel;
						break;
					case 613://everytime, all channels
						rec.RecType=TVRecording.RecordingType.EveryTimeOnEveryChannel;
						break;
					case 614://weekly
						rec.RecType=TVRecording.RecordingType.Weekly;
						break;
					case 615://daily
						rec.RecType=TVRecording.RecordingType.Daily;
						break;
					case 672://Mo-Fi
						rec.RecType=TVRecording.RecordingType.WeekDays;
						break;
				}
				Recorder.AddRecording(ref rec);

				//check if this program is interrupted (for example by a news bulletin)
				//ifso ask the user if he wants to record the 2nd part also
				ArrayList programs=new ArrayList();
				DateTime dtStart=rec.EndTime.AddMinutes(1);
				DateTime dtEnd  =dtStart.AddHours(3);
				long iStart=Utils.datetolong(dtStart);
				long iEnd=Utils.datetolong(dtEnd);
				TVDatabase.GetProgramsPerChannel(rec.Channel,iStart,iEnd,ref programs);
				if (programs.Count>=2)
				{
					TVProgram next=programs[0] as TVProgram;
					TVProgram nextNext=programs[1] as TVProgram;
					if (nextNext.Title==rec.Title)
					{
						TimeSpan ts=next.EndTime-next.StartTime;
						if (ts.TotalMinutes<=40)
						{
							//
							GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
							dlgYesNo.SetHeading(1012);//This program will be interrupted by
							dlgYesNo.SetLine(1,next.Title);
							dlgYesNo.SetLine(2,1013);//Would you like to record the second part also?
							dlgYesNo.DoModal(GetID);
							if (dlgYesNo.IsConfirmed) 
							{
								rec.Start=nextNext.Start;
								rec.End=nextNext.End;
								rec.ID=-1;
								Recorder.AddRecording(ref rec);
							}
						}
					}
				}
				
			}
			Update();
		}
		
		bool CheckIfRecording(TVRecording rec)
		{
			int card;
			if (!Recorder.IsRecordingSchedule(rec, out card)) return true;
			GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
			if (null == dlgYesNo) return true;
			dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));//Delete this recording?
			dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730));//This schedule is recording. If you delete
			dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731));//the schedule then the recording is stopped.
			dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732));//are you sure
			dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

			if (dlgYesNo.IsConfirmed) 
			{
				return true;
			}
			return false;
		}

		void OnNotify()
		{
			TVNotify notification=null;
			ArrayList notifies = new ArrayList();
			TVDatabase.GetNotifies(notifies,false);
			bool showNotify=false;
			foreach (TVNotify notify in notifies)
			{
				if (notify.Program.ID==currentProgram.ID)
				{
					showNotify=true;
					notification=notify;
					break;
				}
			}
			if (showNotify)
				TVDatabase.DeleteNotify(notification);
			else
			{
				TVNotify notify = new TVNotify();
				notify.Program=currentProgram;
				TVDatabase.AddNotify(notify);
			}
			Update();
		}

		void OnKeep()
		{
			bool bRecording=false;
			TVRecording rec=null;
			ArrayList recordings = new ArrayList();
			TVDatabase.GetRecordings(ref recordings);

			foreach (TVRecording record in recordings)
			{
				if (record.Canceled>0) continue;
				if (record.IsRecordingProgram(currentProgram,true) ) 
				{
					if (!record.IsSerieIsCanceled(currentProgram.StartTime))
					{
						bRecording=true;
						rec=record;
						break;
					}
				}
			}

			if (!bRecording)
			{
				return;
			}

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
			dlg.Reset();
			dlg.SetHeading(1042);
			dlg.AddLocalizedString( 1043);//Until watched
			dlg.AddLocalizedString( 1044);//Until space needed
			dlg.AddLocalizedString( 1045);//Until date
			dlg.AddLocalizedString( 1046);//Always
			switch (rec.KeepRecordingMethod)
			{
				case TVRecorded.KeepMethod.UntilWatched: 
					dlg.SelectedLabel=0;
					break;
				case TVRecorded.KeepMethod.UntilSpaceNeeded: 
					dlg.SelectedLabel=1;
					break;
				case TVRecorded.KeepMethod.TillDate: 
					dlg.SelectedLabel=2;
					break;
				case TVRecorded.KeepMethod.Always: 
					dlg.SelectedLabel=3;
					break;
			}
			dlg.DoModal( GetID);
			if (dlg.SelectedLabel==-1) return;
			switch (dlg.SelectedId)
			{
				case 1043: 
					rec.KeepRecordingMethod=TVRecorded.KeepMethod.UntilWatched;
					break;
				case 1044: 
					rec.KeepRecordingMethod=TVRecorded.KeepMethod.UntilSpaceNeeded;
				
					break;
				case 1045: 
					rec.KeepRecordingMethod=TVRecorded.KeepMethod.TillDate;
					dlg.Reset();
					dlg.ShowQuickNumbers=false;
					dlg.SetHeading(1045);
					for (int iDay=1; iDay <= 100; iDay++)
					{
						DateTime dt=currentProgram.StartTime.AddDays(iDay);
						dlg.Add(dt.ToLongDateString());
					}
					TimeSpan ts=(rec.KeepRecordingTill-currentProgram.StartTime);
					int days=(int)ts.TotalDays;
					if (days >=100) days=30;
					dlg.SelectedLabel=days-1;
					dlg.DoModal(GetID);
					if (dlg.SelectedLabel<0) return;
					rec.KeepRecordingTill=currentProgram.StartTime.AddDays(dlg.SelectedLabel+1);
					break;
				case 1046: 
					rec.KeepRecordingMethod=TVRecorded.KeepMethod.Always;
					break;
			}
			TVDatabase.UpdateRecording(rec,TVDatabase.RecordingChange.Modified);
			
		}

		private void item_OnItemSelected(GUIListItem item, GUIControl parent)
		{
			UpdateProgramDescription(item.TVTag as TVRecording);
		}
	}
}
