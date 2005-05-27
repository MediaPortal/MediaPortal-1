using System;
using System.Collections;
using System.Globalization;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;


namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Summary description for RecorderProperties.
	/// </summary>
	public class RecorderProperties
	{
		static TVChannel     currentTvChannel=null;
		static TVRecording	 lastTvRecording=null;
		static TVProgram     lastProgramRecording=null;
		static ArrayList     m_TVChannels=new ArrayList();

		static public void Init()
		{
			m_TVChannels.Clear();
			TVDatabase.GetChannels(ref m_TVChannels);

			Clean();
		}

		/// <summary>
		/// Updates the TV tags for the skin bases on the current tv recording...
		/// </summary>
		/// <remarks>
		/// Tags updated are:
		/// #TV.Record.channel, #TV.Record.start,#TV.Record.stop, #TV.Record.genre, #TV.Record.title, #TV.Record.description, #TV.Record.thumb
		/// </remarks>
		static public void UpdateRecordingProperties()
		{
			// handle properties...
			if (Recorder.IsRecording())
			{
				if (lastTvRecording!=Recorder.CurrentTVRecording || lastProgramRecording!=Recorder.ProgramRecording)
				{
					lastTvRecording=Recorder.CurrentTVRecording;
					lastProgramRecording=Recorder.ProgramRecording;
					if (lastProgramRecording==null)
					{
						string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,lastTvRecording.Channel);
						if (!System.IO.File.Exists(strLogo))
						{
							strLogo="defaultVideoBig.png";
						}
						GUIPropertyManager.SetProperty("#TV.Record.thumb",strLogo);
						GUIPropertyManager.SetProperty("#TV.Record.start",lastTvRecording.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
						GUIPropertyManager.SetProperty("#TV.Record.stop",lastTvRecording.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
						GUIPropertyManager.SetProperty("#TV.Record.genre",String.Empty);
						GUIPropertyManager.SetProperty("#TV.Record.title",lastTvRecording.Title);
						GUIPropertyManager.SetProperty("#TV.Record.description",String.Empty);
					}
					else
					{
						string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,lastProgramRecording.Channel);
						if (!System.IO.File.Exists(strLogo))
						{
							strLogo="defaultVideoBig.png";
						}
						GUIPropertyManager.SetProperty("#TV.Record.thumb",strLogo);
						GUIPropertyManager.SetProperty("#TV.Record.channel",lastProgramRecording.Channel);
						GUIPropertyManager.SetProperty("#TV.Record.start",lastProgramRecording.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
						GUIPropertyManager.SetProperty("#TV.Record.stop" ,lastProgramRecording.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
						GUIPropertyManager.SetProperty("#TV.Record.genre",lastProgramRecording.Genre);
						GUIPropertyManager.SetProperty("#TV.Record.title",lastProgramRecording.Title);
						GUIPropertyManager.SetProperty("#TV.Record.description",lastProgramRecording.Description);
					}
				}
			}
			else // not recording.
			{
				if (lastTvRecording!=null)
				{
					lastTvRecording=null;
					lastProgramRecording=null;
					GUIPropertyManager.SetProperty("#TV.Record.channel",String.Empty);
					GUIPropertyManager.SetProperty("#TV.Record.start",String.Empty);
					GUIPropertyManager.SetProperty("#TV.Record.stop" ,String.Empty);
					GUIPropertyManager.SetProperty("#TV.Record.genre",String.Empty);
					GUIPropertyManager.SetProperty("#TV.Record.title",String.Empty);
					GUIPropertyManager.SetProperty("#TV.Record.description",String.Empty);
					GUIPropertyManager.SetProperty("#TV.Record.thumb"  ,String.Empty);
				}
			}

			if (Recorder.IsRecording())
			{
				DateTime dtStart,dtEnd,dtStarted;
				if (lastProgramRecording !=null)
				{
					dtStart=lastProgramRecording.StartTime;
					dtEnd=lastProgramRecording.EndTime;
					dtStarted=Recorder.TimeRecordingStarted;
					if (dtStarted<dtStart) dtStarted=dtStart;
					SetProgressBarProperties(dtStart,dtStarted,dtEnd);
				}
				else 
				{
					if (lastTvRecording!=null)
					{
						dtStart=lastTvRecording.StartTime;
						dtEnd=lastTvRecording.EndTime;
						dtStarted=Recorder.TimeRecordingStarted;
						if (dtStarted<dtStart) dtStarted=dtStart;
						SetProgressBarProperties(dtStart,dtStarted,dtEnd);
					}
				}
			}
			else if (Recorder.View && currentTvChannel!=null)
			{
				if (currentTvChannel.CurrentProgram!=null)
				{
					DateTime dtStart,dtEnd,dtStarted;
					dtStart=currentTvChannel.CurrentProgram.StartTime;
					dtEnd=currentTvChannel.CurrentProgram.EndTime;
					dtStarted=Recorder.TimeTimeshiftingStarted;
					if (dtStarted<dtStart) dtStarted=dtStart;
					SetProgressBarProperties(dtStart,dtStarted,dtEnd);
				}
				else
				{
					// we dont have any tvguide data. 
					// so just suppose program started when timeshifting started and ends 2 hours after that
					DateTime dtStart,dtEnd,dtStarted;
					dtStart=Recorder.TimeTimeshiftingStarted;

					dtEnd=dtStart;
					dtEnd=dtEnd.AddHours(2);

					dtStarted=Recorder.TimeTimeshiftingStarted;
					if (dtStarted<dtStart) dtStarted=dtStart;
					SetProgressBarProperties(dtStart,dtStarted,dtEnd);
				}
			}
		}

		/// <summary>
		/// Empties/clears all tv related skin tags. Gets called during startup en shutdown of
		/// the scheduler
		/// </summary>
		static public void Clean()
		{
			currentTvChannel=null;
			lastTvRecording=null;
			lastProgramRecording=null;

			GUIPropertyManager.SetProperty("#TV.View.channel",String.Empty);
			GUIPropertyManager.SetProperty("#TV.View.thumb",  String.Empty);
			GUIPropertyManager.SetProperty("#TV.View.start",String.Empty);
			GUIPropertyManager.SetProperty("#TV.View.stop", String.Empty);
			GUIPropertyManager.SetProperty("#TV.View.genre",String.Empty);
			GUIPropertyManager.SetProperty("#TV.View.title",String.Empty);
			GUIPropertyManager.SetProperty("#TV.View.description",String.Empty);

			GUIPropertyManager.SetProperty("#TV.Record.channel",String.Empty);
			GUIPropertyManager.SetProperty("#TV.Record.start",String.Empty);
			GUIPropertyManager.SetProperty("#TV.Record.stop", String.Empty);
			GUIPropertyManager.SetProperty("#TV.Record.genre",String.Empty);
			GUIPropertyManager.SetProperty("#TV.Record.title",String.Empty);
			GUIPropertyManager.SetProperty("#TV.Record.description",String.Empty);
			GUIPropertyManager.SetProperty("#TV.Record.thumb",  String.Empty);

			GUIPropertyManager.SetProperty("#TV.Record.percent1",String.Empty);
			GUIPropertyManager.SetProperty("#TV.Record.percent2",String.Empty);
			GUIPropertyManager.SetProperty("#TV.Record.percent3",String.Empty);
			GUIPropertyManager.SetProperty("#TV.Record.duration",String.Empty);
			GUIPropertyManager.SetProperty("#TV.Record.current",String.Empty);
		}//static void CleanProperties()
		
		
		/// <summary>
		/// this method will update all tags for the tv progress bar
		/// </summary>
		static void SetProgressBarProperties(DateTime MovieStartTime,DateTime RecordingStarted, DateTime MovieEndTime)
		{
			TimeSpan tsMovieDuration = (MovieEndTime-MovieStartTime);
			float fMovieDurationInSecs=(float)tsMovieDuration.TotalSeconds;

			GUIPropertyManager.SetProperty("#TV.Record.duration",Utils.SecondsToShortHMSString((int)fMovieDurationInSecs));
      
			// get point where we started timeshifting/recording relative to the start of movie
			TimeSpan tsRecordingStart= (RecordingStarted-MovieStartTime)+new TimeSpan(0,0,0,(int)g_Player.ContentStart,0);
			float fRelativeRecordingStart=(float)tsRecordingStart.TotalSeconds;
			float percentRecStart = (fRelativeRecordingStart/fMovieDurationInSecs)*100.00f;
			int iPercentRecStart=(int)Math.Floor(percentRecStart);
			GUIPropertyManager.SetProperty("#TV.Record.percent1",iPercentRecStart.ToString());

			// get the point we're currently watching relative to the start of movie
			if (g_Player.Playing && g_Player.IsTV)
			{
				float fRelativeViewPoint=(float)g_Player.CurrentPosition+ fRelativeRecordingStart;
				float fPercentViewPoint = (fRelativeViewPoint / fMovieDurationInSecs)*100.00f;
				int iPercentViewPoint=(int)Math.Floor(fPercentViewPoint);
				GUIPropertyManager.SetProperty("#TV.Record.percent2",iPercentViewPoint.ToString());
				GUIPropertyManager.SetProperty("#TV.Record.current",Utils.SecondsToShortHMSString((int)fRelativeViewPoint));
			} 
			else
			{
				GUIPropertyManager.SetProperty("#TV.Record.percent2",iPercentRecStart.ToString());
				GUIPropertyManager.SetProperty("#TV.Record.current",Utils.SecondsToShortHMSString((int)fRelativeRecordingStart));
			}

			// get point the live program is now
			TimeSpan tsRelativeLivePoint= (DateTime.Now-MovieStartTime);
			float   fRelativeLiveSec=(float)tsRelativeLivePoint.TotalSeconds;
			float percentLive = (fRelativeLiveSec/fMovieDurationInSecs)*100.00f;
			int   iPercentLive=(int)Math.Floor(percentLive);
			GUIPropertyManager.SetProperty("#TV.Record.percent3",iPercentLive.ToString());
		}//static void SetProgressBarProperties(DateTime MovieStartTime,DateTime RecordingStarted, DateTime MovieEndTime)

		
		/// <summary>
		/// Updates the TV tags for the skin bases on the current tv channel
		/// </summary>
		/// <remarks>
		/// Tags updated are:
		/// #TV.View.channel, #TV.View.start,#TV.View.stop, #TV.View.genre, #TV.View.title, #TV.View.description
		/// </remarks>
		static public void UpdateViewProperties()
		{
			// for each tv-channel

			if (currentTvChannel==null || !currentTvChannel.Name.Equals(Recorder.TVChannelName))
			{
				for (int i=0; i < m_TVChannels.Count;++i)
				{
					TVChannel chan =(TVChannel)m_TVChannels[i];
					if (chan.Name.Equals(Recorder.TVChannelName))
					{
						currentTvChannel=chan;
						OnTVChannelChanged();
						break;
					}
				}
				if (currentTvChannel==null)
				{
					GUIPropertyManager.SetProperty("#TV.View.start",String.Empty);
					GUIPropertyManager.SetProperty("#TV.View.stop" ,String.Empty);
					GUIPropertyManager.SetProperty("#TV.View.genre",String.Empty);
					GUIPropertyManager.SetProperty("#TV.View.title", "");
					GUIPropertyManager.SetProperty("#TV.View.description",String.Empty);
				}
			}
			if (currentTvChannel!=null)
			{
				TVProgram prog=currentTvChannel.CurrentProgram;
				if (prog!=null)
				{
					GUIPropertyManager.SetProperty("#TV.View.start",prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
					GUIPropertyManager.SetProperty("#TV.View.stop",prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
					GUIPropertyManager.SetProperty("#TV.View.genre",prog.Genre);
					GUIPropertyManager.SetProperty("#TV.View.title",prog.Title);
					GUIPropertyManager.SetProperty("#TV.View.description",prog.Description);

				}
				else
				{
					GUIPropertyManager.SetProperty("#TV.View.start",String.Empty);
					GUIPropertyManager.SetProperty("#TV.View.stop" ,String.Empty);
					GUIPropertyManager.SetProperty("#TV.View.genre",String.Empty);
					GUIPropertyManager.SetProperty("#TV.View.title", Recorder.TVChannelName);
					GUIPropertyManager.SetProperty("#TV.View.description",String.Empty);
				}
			}//for (int i=0; i < m_TVChannels.Count;++i)			
		}//static void SetProperties()

		
		/// <summary>
		/// Sets the current tv channel tags. This function gets called when the current
		/// tv channel gets changed. It will update the corresponding skin tags 
		/// </summary>
		/// <remarks>
		/// Sets the current tags:
		/// #TV.View.channel,  #TV.View.thumb
		/// </remarks>
		static void OnTVChannelChanged()
		{
			string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,Recorder.TVChannelName);
			if (!System.IO.File.Exists(strLogo))
			{
				strLogo="defaultVideoBig.png";
			}
			GUIPropertyManager.SetProperty("#TV.View.channel",Recorder.TVChannelName);
			GUIPropertyManager.SetProperty("#TV.View.thumb",strLogo);
		}//static void OnTVChannelChanged()

	}
}
