using System;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.TV.Database
{
	/// <summary>
	/// Class which contains all information about a scheduled recording
	/// </summary>
	public class TVRecording
	{
    /// <summary>
    /// Type of recording
    /// </summary>
		public enum RecordingType
		{
			Once,
			EveryTimeOnThisChannel,
			EveryTimeOnEveryChannel,
			Daily,
			Weekly,
      WeekDays
		};

    /// <summary>
    /// Current recording status
    /// </summary>
    public enum RecordingStatus
    {
      Waiting,
      Finished,
      Canceled
    };

		long				m_iStartTime;
		long				m_iEndTime;
    long				m_iCancelTime=0;
    string			m_strTitle;
		string      m_strChannel;
		RecordingType m_eType;
		int				  m_iRecordId;
		bool				m_bContentRecording=true;

    /// <summary>
    /// Constructor.
    /// </summary>
		public TVRecording()
		{
			m_bContentRecording=true;
		}

    /// <summary>
    /// Property to get/set the recording type
    /// </summary>
		public RecordingType RecType
		{
			get { return m_eType;}
			set { m_eType=value;}
		}

    /// <summary>
    /// Property to get/set whether this recording is a content or reference recording
    /// </summary>
    /// <seealso cref="MediaPortal.TV.Recording.IGraph"/>
		public bool IsContentRecording
		{
			get { return m_bContentRecording;}
			set { m_bContentRecording=value;}
		}

    /// <summary>
    /// Property to get/set the TV channel name on which the recording should be done
    /// </summary>
		public string Channel
		{
			get { return m_strChannel;}
			set { m_strChannel=value;}
		}

    /// <summary>
    /// Property to get/set the title of the program to record
    /// </summary>
		public string Title
		{
			get { return m_strTitle;}
			set { m_strTitle=value;}
		}

    /// <summary>
    /// Property to get/set the starttime of the program to record in xmltv format: yyyymmddhhmmss
    /// </summary>
		public long Start
		{
			get { return m_iStartTime;}
			set { m_iStartTime=value;}
		}

    /// <summary>
    /// Property to get/set the endtime of the program to record in xmltv format: yyyymmddhhmmss
    /// </summary>
		public long End
		{
			get { return m_iEndTime;}
			set { m_iEndTime=value;}
		}

    /// <summary>
    /// Property to get/set the database id of this recording 
    /// </summary>
		public int ID
		{
			get { return m_iRecordId;}
			set { m_iRecordId=value;}
		}

    /// <summary>
    /// Property to get the  starttime of the program to record
    /// </summary>
		public DateTime StartTime
		{
			get { return Utils.longtodate(m_iStartTime);}
		}

    /// <summary>
    /// Property to get the endtime of the program to record
    /// </summary>
		public DateTime EndTime
		{
			get { return Utils.longtodate(m_iEndTime);}
		}
	
    /// <summary>
    /// Checks whether this recording is finished and can be deleted
    /// 
    /// </summary>
    /// <returns>true:Recording is finished can be deleted
    ///          false:Recording is not done yet, or needs to be done multiple times
    /// </returns>
		public bool IsDone()
		{
			if (m_eType != RecordingType.Once) return false;
			if (DateTime.Now > EndTime) return true;
			return false;
		}

    /// <summary>
    /// Check is the recording should be started/running between the specified start/end time
    /// </summary>
    /// <param name="tStartTime">starttime</param>
    /// <param name="tEndTime">endtime</param>
    /// <returns>true if the recording should be running between starttime-endtime</returns>
		public bool RunningAt(DateTime tStartTime, DateTime tEndTime)
		{
			DateTime dtStart=StartTime;
			DateTime dtEnd=EndTime;

			bool bRunningAt=false;
			if (dtEnd>=tStartTime && dtEnd <= tEndTime) bRunningAt=true;
			if (dtStart >=tStartTime && dtStart <= tEndTime) bRunningAt=true;
			if (dtStart <=tStartTime && dtEnd>=tEndTime) bRunningAt=true;
			return bRunningAt;
		}

    /// <summary>
    /// Checks if the recording should record the specified tvprogram
    /// </summary>
    /// <param name="program">TVProgram to check</param>
    /// <returns>true if the specified tvprogram should be recorded</returns>
    /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
		public bool IsRecordingProgram(TVProgram program)
		{
			switch (m_eType)
			{
				case RecordingType.Once:
				{
					if (program.Start==Start && program.End==End && program.Channel==Channel) return true;
				}
					break;
				case RecordingType.EveryTimeOnEveryChannel:
					if (program.Title==Title) return true;
					break;
				case RecordingType.EveryTimeOnThisChannel:
					if (program.Title==Title && program.Channel==Channel) return true;
					break;
				case RecordingType.Daily:
					if (program.Channel==Channel)
					{
						int iHourProg=program.StartTime.Hour;
						int iMinProg=program.StartTime.Minute;
						if (iHourProg==StartTime.Hour && iMinProg==StartTime.Minute)
						{
							iHourProg=program.EndTime.Hour;
							iMinProg=program.EndTime.Minute;
							if (iHourProg==EndTime.Hour && iMinProg==EndTime.Minute) return true;
						}
					}
          break;
        case RecordingType.WeekDays:
          if (program.StartTime.DayOfWeek>=DayOfWeek.Monday && program.StartTime.DayOfWeek <= DayOfWeek.Friday)
          {
            if (program.Channel==Channel)
            {
              int iHourProg=program.StartTime.Hour;
              int iMinProg=program.StartTime.Minute;
              if (iHourProg==StartTime.Hour && iMinProg==StartTime.Minute)
              {
                iHourProg=program.EndTime.Hour;
                iMinProg=program.EndTime.Minute;
                if (iHourProg==EndTime.Hour && iMinProg==EndTime.Minute) return true;
              }
            }
          }
          break;				

          case RecordingType.Weekly:
					if (program.Channel==Channel)
					{
						int iHourProg=program.StartTime.Hour;
						int iMinProg=program.StartTime.Minute;
						if (iHourProg==StartTime.Hour && iMinProg==StartTime.Minute)
						{
							iHourProg=program.EndTime.Hour;
							iMinProg=program.EndTime.Minute;
							if (iHourProg==EndTime.Hour && iMinProg==EndTime.Minute) 
							{
								if (StartTime.DayOfWeek==program.StartTime.DayOfWeek)
								{
									return true;
								}
							}
						}
					}
					break;
			}
			return false;
		}

    /// <summary>
    /// Checks whether the recording should be recording at the specified time including the pre/post intervals
    /// </summary>
    /// <param name="dtTime">time</param>
    /// <param name="TVProgram">TVProgram</param>
    /// <param name="iPreInterval">pre record interval</param>
    /// <param name="iPostInterval">post record interval</param>
    /// <returns>true if the recording should record</returns>
    /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
    public bool IsRecordingAtTime(DateTime dtTime,TVProgram currentProgram,  int iPreInterval, int iPostInterval)
    {
      DateTime dtStart;
      DateTime dtEnd;
      switch (RecType)
      {
          // record program just once
        case RecordingType.Once:
          if (dtTime >= this.StartTime.AddMinutes(-iPreInterval) && dtTime <= this.EndTime.AddMinutes(iPostInterval) ) 
          {
            if (Canceled>0)
            {
              return false;
            }
            return true;
          }
          break;
				
          // record program daily at same time & channel
        case RecordingType.Daily:
          // check if recording start/date time is correct
          dtStart=new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,this.StartTime.Hour,StartTime.Minute,0);
          dtEnd  =new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,this.EndTime.Hour  ,EndTime.Minute  ,0);
          if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval) ) 
          {
            // not canceled?
            if (Canceled>0)
            {
              if (CanceledTime.Day==dtTime.Day && CanceledTime.Month==dtTime.Month && CanceledTime.Year==dtTime.Year)
              {
                return false;
              }
            }
            return true;
          }
          break;

          // record program daily at same time & channel
        case RecordingType.WeekDays:
          // check if recording start/date time is correct
          dtStart=new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,this.StartTime.Hour,StartTime.Minute,0);
          dtEnd  =new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,this.EndTime.Hour  ,EndTime.Minute  ,0);
          if (dtStart.DayOfWeek>=DayOfWeek.Monday && dtStart.DayOfWeek<=DayOfWeek.Friday)
          {
            if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval) ) 
            {
              // not canceled?
              if (Canceled>0)
              {
                if (CanceledTime.Day==dtTime.Day && CanceledTime.Month==dtTime.Month && CanceledTime.Year==dtTime.Year)
                {
                  return false;
                }
              }
              return true;
            }
          }
          break;

          // record program weekly at same time & channel
        case RecordingType.Weekly:
          // check if day of week of recording matches 
          if (this.StartTime.DayOfWeek == dtTime.DayOfWeek)
          {
            // check if start/end time of recording is correct
            dtStart=new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,this.StartTime.Hour,this.StartTime.Minute,0);
            dtEnd  =new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,this.EndTime.Hour  ,this.EndTime.Minute  ,0);
            if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval) ) 
            {
              // not canceled?
              if (Canceled>0)
              {
                if (CanceledTime.Day==dtTime.Day && CanceledTime.Month==dtTime.Month && CanceledTime.Year==dtTime.Year)
                {
                  return false;
                }
              }
              return true;
            }
          }
          break;
				
          //record program everywhere
        case RecordingType.EveryTimeOnEveryChannel:
          if (currentProgram==null) return false; // we need a program
          if (currentProgram.Title==this.Title)  // check title
          {
            // check program time
            if (dtTime >= currentProgram.StartTime.AddMinutes(-iPreInterval) && 
                dtTime <= currentProgram.EndTime.AddMinutes(iPostInterval) ) 
            {
              // not canceled?
              if (Canceled>0)
              {
                if (CanceledTime.Day==dtTime.Day && CanceledTime.Month==dtTime.Month && CanceledTime.Year==dtTime.Year)
                {
                  if (CanceledTime >=currentProgram.StartTime.AddMinutes(-iPreInterval) && CanceledTime <=currentProgram.EndTime.AddMinutes(+iPostInterval))
                  {
                    return false;
                  }
                }
              }
              return true;
            }
          }
          break;
				
          //record program on this channel
        case RecordingType.EveryTimeOnThisChannel:
          if (currentProgram==null) return false; // we need a channel
          
          // check channel & title
          if (currentProgram.Title==this.Title && currentProgram.Channel==this.Channel) 
          {
            // check time
            if (dtTime >= currentProgram.StartTime.AddMinutes(-iPreInterval) && 
               dtTime <= currentProgram.EndTime.AddMinutes(iPostInterval) ) 
            {

              // not canceled?
              if (Canceled>0)
              {
                if (CanceledTime >=currentProgram.StartTime.AddMinutes(-iPreInterval) && CanceledTime <=currentProgram.EndTime.AddMinutes(+iPostInterval))
                {
                  return false;
                }
              }
              return true;
            }
          }
          break;
      }
      return false;
    }

    /// <summary>
    /// Checks whether the recording should record the specified TVProgram
    /// at the specified time including the pre/post intervals
    /// </summary>
    /// <param name="dtTime">time</param>
    /// <param name="TVProgram">TVProgram</param>
    /// <param name="iPreInterval">pre record interval</param>
    /// <param name="iPostInterval">post record interval</param>
    /// <returns>true if the recording should record</returns>
    /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
		public bool IsRecordingProgramAtTime(DateTime dtTime,TVProgram currentProgram, int iPreInterval, int iPostInterval)
		{
      DateTime dtStart;
      DateTime dtEnd;
			switch (RecType)
			{
        // record program just once
				case RecordingType.Once:
					if (dtTime >= this.StartTime.AddMinutes(-iPreInterval) && dtTime <= this.EndTime.AddMinutes(iPostInterval) ) 
					{
            if (Canceled>0)
            {
              return false;
            }
          
            if (currentProgram!=null) 
            {
              if (currentProgram.Channel!=this.Channel)  return false;
              if (dtTime >= currentProgram.StartTime.AddMinutes(-iPreInterval) && dtTime <= currentProgram.EndTime.AddMinutes(iPostInterval) ) 
              {
                return true;
              }
              return false;
            }
            string strManual=GUILocalizeStrings.Get(413);
						if (this.Title.Length==0 || String.Compare(this.Title,strManual,true)==0) return true;
						
						strManual=GUILocalizeStrings.Get(736);
						if (this.Title.Length==0 || String.Compare(this.Title,strManual,true)==0) return true;
            return false;
					}
				break;
				
        // record program daily at same time & channel
				case RecordingType.WeekDays:
          if (currentProgram==null) return false;   //we need a program
          if (currentProgram.Channel==this.Channel) //check channel is correct
          {
            // check if program start/date time is correct
            dtStart=new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,currentProgram.StartTime.Hour,StartTime.Minute,0);
            dtEnd  =new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,currentProgram.EndTime.Hour  ,EndTime.Minute  ,0);
            if (dtStart.DayOfWeek>=DayOfWeek.Monday && dtStart.DayOfWeek<=DayOfWeek.Friday)
            {
              if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval) ) 
              {
                // check if recording start/date time is correct
                dtStart=new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,this.StartTime.Hour,StartTime.Minute,0);
                dtEnd  =new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,this.EndTime.Hour  ,EndTime.Minute  ,0);
                if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval) ) 
                {
                  // not canceled?
                  if (Canceled>0)
                  {
                    if (CanceledTime.Day==dtTime.Day && CanceledTime.Month==dtTime.Month && CanceledTime.Year==dtTime.Year)
                    {
                      return false;
                    }
                  }
                  return true;
                }
              }
            }
          }
				break;

        // record program daily at same time & channel
				case RecordingType.Daily:
          if (currentProgram==null) return false;   //we need a program
          if (currentProgram.Channel==this.Channel) //check channel is correct
          {
            // check if program start/date time is correct
            dtStart=new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,currentProgram.StartTime.Hour,StartTime.Minute,0);
            dtEnd  =new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,currentProgram.EndTime.Hour  ,EndTime.Minute  ,0);
            if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval) ) 
            {
              // check if recording start/date time is correct
              dtStart=new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,this.StartTime.Hour,StartTime.Minute,0);
              dtEnd  =new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,this.EndTime.Hour  ,EndTime.Minute  ,0);
              if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval) ) 
              {
                // not canceled?
                if (Canceled>0)
                {
                  if (CanceledTime.Day==dtTime.Day && CanceledTime.Month==dtTime.Month && CanceledTime.Year==dtTime.Year)
                  {
                    return false;
                  }
                }
                return true;
              }
            }
          }
				break;
				
        // record program weekly at same time & channel
				case RecordingType.Weekly:
          if (currentProgram==null) return false;   // we need a program
          if (currentProgram.Channel==this.Channel) // check if channel is correct
          {
            // check if day of week of program matches 
            if (currentProgram.StartTime.DayOfWeek== dtTime.DayOfWeek)
            {

              // check if start/end time of program is correct
              dtStart=new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,currentProgram.StartTime.Hour,currentProgram.StartTime.Minute,0);
              dtEnd  =new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,currentProgram.EndTime.Hour  ,currentProgram.EndTime.Minute  ,0);

              if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval) ) 
              {
                
                // check if day of week of recording matches 
                if (this.StartTime.DayOfWeek == dtTime.DayOfWeek)
                {
                  // check if start/end time of recording is correct
                  dtStart=new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,this.StartTime.Hour,this.StartTime.Minute,0);
                  dtEnd  =new DateTime(dtTime.Year,dtTime.Month,dtTime.Day,this.EndTime.Hour  ,this.EndTime.Minute  ,0);
                  if (dtTime >= dtStart.AddMinutes(-iPreInterval) && dtTime <= dtEnd.AddMinutes(iPostInterval) ) 
                  {
                    // not canceled?
                    if (Canceled>0)
                    {
                      if (CanceledTime.Day==dtTime.Day && CanceledTime.Month==dtTime.Month && CanceledTime.Year==dtTime.Year)
                      {
                        return false;
                      }
                    }
                    return true;
                  }
                }
              }
            }
          }
				break;
				
        //record program everywhere
				case RecordingType.EveryTimeOnEveryChannel:
          if (currentProgram==null) return false; // we need a program
          if (currentProgram.Title==this.Title)  // check title
          {
            // check program time
            if (dtTime >= currentProgram.StartTime.AddMinutes(-iPreInterval) && 
                dtTime <= currentProgram.EndTime.AddMinutes(iPostInterval) ) 
            {
              // not canceled?
              if (Canceled>0)
              {
                if (CanceledTime.Day==dtTime.Day && CanceledTime.Month==dtTime.Month && CanceledTime.Year==dtTime.Year)
                {
                  if (CanceledTime >=currentProgram.StartTime.AddMinutes(-iPreInterval) && CanceledTime <=currentProgram.EndTime.AddMinutes(+iPostInterval))
                  {
                    return false;
                  }
                }
              }
              return true;
            }
          }
				break;
				
          //record program on this channel
				case RecordingType.EveryTimeOnThisChannel:
          if (currentProgram==null) return false; // we need a channel
          
          // check channel & title
          if (currentProgram.Title==this.Title && currentProgram.Channel==this.Channel) 
          {
            // check time
            if (dtTime >= currentProgram.StartTime.AddMinutes(-iPreInterval) && 
                dtTime <= currentProgram.EndTime.AddMinutes(iPostInterval) ) 
            {

              // not canceled?
              if (Canceled>0)
              {
                if (CanceledTime >=currentProgram.StartTime.AddMinutes(-iPreInterval) && CanceledTime <=currentProgram.EndTime.AddMinutes(+iPostInterval))
                {
                  return false;
                }
              }
              return true;
            }
          }
				break;
			}
			return false;
		
    }

    /// <summary>
    /// Returns a string describing the recording
    /// </summary>
    /// <returns>Returns a string describing the recording</returns>
    public override string ToString()
    {
      string strLine="";
      switch (RecType)
      {
        case RecordingType.Once:
          strLine=String.Format("Record once {0} on {1} from {2} {3} - {4} {5}", 
                            this.Title,this.Channel, 
                            StartTime.ToShortDateString(),
                            StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
                            EndTime.ToShortDateString(),
                            EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
          break;

        case RecordingType.WeekDays:
          strLine=String.Format("{0} on {1} {2}-{3} from {4} - {5}", 
            this.Title,this.Channel, 
            GUILocalizeStrings.Get(657),//657=Mon
            GUILocalizeStrings.Get(661),//661=Fri
            StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
            EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
          break;

        case RecordingType.Daily:
          strLine=String.Format("Record daily {0} on {1} from {2} - {3}", 
                                        this.Title,this.Channel, 
                                        StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
                                        EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
          break;
        case RecordingType.Weekly:
          string day;
          switch (StartTime.DayOfWeek)
          {
            case DayOfWeek.Monday :	day = GUILocalizeStrings.Get(11);	break;
            case DayOfWeek.Tuesday :	day = GUILocalizeStrings.Get(12);	break;
            case DayOfWeek.Wednesday :	day = GUILocalizeStrings.Get(13);	break;
            case DayOfWeek.Thursday :	day = GUILocalizeStrings.Get(14);	break;
            case DayOfWeek.Friday :	day = GUILocalizeStrings.Get(15);	break;
            case DayOfWeek.Saturday :	day = GUILocalizeStrings.Get(16);	break;
            default:	day = GUILocalizeStrings.Get(17);	break;
          }
          strLine=String.Format("Record {0} on {1} every {2} from {3} - {4}", 
                                  this.Title,this.Channel, 
                                  day,
                                  StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
                                  EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
          break;

        case RecordingType.EveryTimeOnEveryChannel:
          strLine=String.Format("Record {0} every time on any channel", Title);
        break;

        case RecordingType.EveryTimeOnThisChannel:
          strLine=String.Format("Record {0} every time on {1}", Title, Channel);
          break;
      }      
      return strLine;
    }

    /// <summary>
    /// Property to get/set the time the recording was canceled in xmltv format:yyyymmddhhmmss
    /// </summary>
    public long Canceled
    {
      get { return m_iCancelTime;}
      set { m_iCancelTime=value;}      
    }

    /// <summary>
    /// Property to get the time the recording was canceled 
    /// </summary>
    public DateTime CanceledTime
    {
      get { return Utils.longtodate(m_iCancelTime);}
    }

    /// <summary>
    /// Property to get current status about the recording
    /// </summary>
    public RecordingStatus Status
    {
      get 
      {
        if ( IsDone() ) return RecordingStatus.Finished;
        if (Canceled>0) return RecordingStatus.Canceled;
        return RecordingStatus.Waiting;        
      }
    }
	}
}
