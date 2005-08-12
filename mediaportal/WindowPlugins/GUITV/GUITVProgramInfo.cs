using System;
using System.Collections;
using System.Globalization;
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
		[SkinControlAttribute(17)]			  protected GUILabelControl				 lblProgramGenre=null;
		[SkinControlAttribute(15)]			  protected GUITextScrollUpControl lblProgramDescription=null;
		[SkinControlAttribute(14)]			  protected GUILabelControl lblProgramTime=null;
		[SkinControlAttribute(13)]			  protected GUIFadeLabel		lblProgramTitle=null;
		[SkinControlAttribute(2)]			  protected GUIButtonControl	btnRecord=null;
		[SkinControlAttribute(3)]			  protected GUIButtonControl	btnAdvancedRecord=null;

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
				if (record.IsRecordingProgram(currentProgram,true) ) 
				{
					bRecording=true;
					break;
				}
			}

			if (bRecording)
			{
				btnRecord.Label=GUILocalizeStrings.Get(1039);//dont record
			}
			else
			{
				btnRecord.Label=GUILocalizeStrings.Get(264);//record
			}
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==btnRecord) 
				OnRecord();
			if (control==btnAdvancedRecord) 
				OnAdvancedRecord();
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
				if (record.IsRecordingProgram(currentProgram,true) ) 
				{
					bRecording=true;
					rec=record;
					break;
				}
			}

			if (!bRecording)
			{
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
						//delete specific series
						rec.CanceledSeries.Add(currentProgram.Start);
						TVDatabase.AddCanceledSerie(rec,currentProgram.Start);
					}
					else
					{
						//cancel recording
						rec.Canceled=Utils.datetolong(DateTime.Now);
						TVDatabase.UpdateRecording(rec,TVDatabase.RecordingChange.Canceled);
					}
					Recorder.StopRecording(rec);
				}
			}
			Update();
		}

		void OnAdvancedRecord()
		{
		}


	}
}
