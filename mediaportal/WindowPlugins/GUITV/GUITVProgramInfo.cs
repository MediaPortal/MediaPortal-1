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
		[SkinControlAttribute(4)]					protected GUIToggleButtonControl	btnNotify=null;
		[SkinControlAttribute(10)]				protected GUIListControl					lstUpcomingEpsiodes=null;

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

		void Update()
		{
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
			foreach (TVRecording record in recordings)
			{
				if (record.Canceled>0) continue;
				if (record.IsRecordingProgram(currentProgram,true) ) 
				{
					if (!record.IsSerieIsCanceled(currentProgram.StartTime))
					{
						bRecording=true;
						break;
					}
				}
			}

			if (bRecording)
			{
				btnRecord.Label=GUILocalizeStrings.Get(1039);//dont record
				btnAdvancedRecord.Disabled=true;
			}
			else
			{
				btnRecord.Label=GUILocalizeStrings.Get(264);//record
				btnAdvancedRecord.Disabled=false;
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
				string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,recSeries.Channel);
				if (!System.IO.File.Exists(strLogo))
				{
					strLogo="defaultVideoBig.png";
				}
				int card;
				if (Recorder.IsRecordingSchedule(recSeries,out card))
				{
					item.PinImage=Thumbs.TvRecordingIcon;
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

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==btnRecord) 
				OnRecord();
			if (control==btnAdvancedRecord) 
				OnAdvancedRecord();
			if (control==btnNotify)
				OnNotify();
			base.OnClicked (controlId, control, actionType);
		}
		void OnRecord()
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
				foreach (TVRecording record in recordings)
				{
					if (record.IsRecordingProgram(currentProgram,false) ) 
					{
						if (record.Canceled>0) 
						{
							record.RecType=TVRecording.RecordingType.Once;
							record.Canceled=0;
						}
						else if (record.IsSerieIsCanceled(currentProgram.StartTime))
						{
							record.UnCancelSerie(currentProgram.StartTime);
						}
						TVDatabase.UpdateRecording(record,TVDatabase.RecordingChange.Modified);
						Update();
						return;
					}		
				}
				rec=new TVRecording();
				rec.Title=currentProgram.Title;
				rec.Channel=currentProgram.Channel;
				rec.Start=currentProgram.Start;
				rec.End=currentProgram.End;
				rec.RecType=TVRecording.RecordingType.Once;
				Recorder.AddRecording(ref rec);
			}
			else
			{
				if (rec.IsRecordingProgram(currentProgram,true))
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
									rec.CanceledSeries.Add(currentProgram.Start);
									TVDatabase.AddCanceledSerie(rec,currentProgram.Start);
									Recorder.StopRecording(rec);
								}
							}
							break;
							case 982: //Delete entire recording
							{
								if (CheckIfRecording(rec))
								{
									//cancel recording
									rec.Canceled=Utils.datetolong(DateTime.Now);
									TVDatabase.UpdateRecording(rec,TVDatabase.RecordingChange.Canceled);
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
							rec.Canceled=Utils.datetolong(DateTime.Now);
							TVDatabase.UpdateRecording(rec,TVDatabase.RecordingChange.Canceled);
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

	}
}
