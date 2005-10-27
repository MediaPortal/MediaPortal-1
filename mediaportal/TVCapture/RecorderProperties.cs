/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
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
		static long					 programStart=-1;
		static public void Init()
		{
			m_TVChannels.Clear();
			TVDatabase.GetChannels(ref m_TVChannels);

			Recorder.OnTvChannelChanged +=new MediaPortal.TV.Recording.Recorder.OnTvChannelChangeHandler(Recorder_OnTvChannelChanged);
			Recorder.OnTvRecordingChanged+=new MediaPortal.TV.Recording.Recorder.OnTvRecordingChangedHandler(Recorder_OnTvRecordingChanged);
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
			if (currentTvChannel!=null)
			{
				TVProgram currentTvProgram=currentTvChannel.CurrentProgram;
				if (currentTvProgram!=null && currentTvProgram.Start!=programStart)
				{
					UpdateTvProgramProperties(currentTvChannel.Name);
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
			GUIPropertyManager.SetProperty("#TV.View.remaining", String.Empty);
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

		
		private static void Recorder_OnTvChannelChanged(string tvChannelName)
		{
			Log.WriteFile(Log.LogType.Recorder,"Recorder: tv channel changed:{0}",tvChannelName);
			UpdateTvProgramProperties(tvChannelName);
		}
		static void UpdateTvProgramProperties(string tvChannelName)
		{
			programStart=-1;
			// for each tv-channel
			for (int i=0; i < m_TVChannels.Count;++i)
			{
				TVChannel chan =(TVChannel)m_TVChannels[i];
				if (chan.Name.Equals(tvChannelName))
				{
					currentTvChannel=chan;
					break;
				}
			}

			GUIPropertyManager.SetProperty("#TV.View.start",String.Empty);
			GUIPropertyManager.SetProperty("#TV.View.stop" ,String.Empty);
			GUIPropertyManager.SetProperty("#TV.View.remaining" ,String.Empty);
			GUIPropertyManager.SetProperty("#TV.View.genre",String.Empty);
			GUIPropertyManager.SetProperty("#TV.View.title", tvChannelName);
			GUIPropertyManager.SetProperty("#TV.View.description",String.Empty);
			

			if (currentTvChannel!=null)
			{
				TVProgram prog=currentTvChannel.CurrentProgram;
				if (prog!=null)
				{
					programStart=prog.Start;
					GUIPropertyManager.SetProperty("#TV.View.start",prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
					GUIPropertyManager.SetProperty("#TV.View.stop",prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
					GUIPropertyManager.SetProperty("#TV.View.remaining", Utils.SecondsToHMSString(prog.EndTime - prog.StartTime));
					GUIPropertyManager.SetProperty("#TV.View.genre",prog.Genre);
					GUIPropertyManager.SetProperty("#TV.View.title",prog.Title);
					GUIPropertyManager.SetProperty("#TV.View.description",prog.Description);
				}
			}//if (currentTvChannel!=null)

			string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,tvChannelName);
			if (!System.IO.File.Exists(strLogo))
			{
				strLogo="defaultVideoBig.png";
			}
			GUIPropertyManager.SetProperty("#TV.View.channel",tvChannelName);
			GUIPropertyManager.SetProperty("#TV.View.thumb",strLogo);
		}

		private static void Recorder_OnTvRecordingChanged()
		{
			Log.WriteFile(Log.LogType.Recorder,"Recorder: recording state changed");
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
		}
	}
}
