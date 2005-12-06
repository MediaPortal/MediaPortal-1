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
using System.Collections.Generic;
using MediaPortal.Util;

namespace MediaPortal.TV.Database
{
	/// <summary>
	/// Helper class which can be used to determine which tv program is
	/// running at a specific time and date
	/// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
	/// </summary>
	public class TVUtil : IDisposable
	{
    TVProgram		_currentProgram=null;
    List<TVProgram> _listPrograms = new List<TVProgram>();
    string			_currentChannel="";
		DateTime   	_lastDateTime=DateTime.Now;
		int         _days=1;
    /// <summary>
    /// Constructor. 
    /// The constructor will load all programs from the TVDatabase
    /// </summary>
    ///
		public TVUtil()
		{
			_days=1;
			OnProgramsChanged();
			TVDatabase.OnProgramsChanged +=new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(this.OnProgramsChanged);
		}

    public TVUtil(int days)
		{
			_days=days;
			OnProgramsChanged();
			TVDatabase.OnProgramsChanged +=new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(this.OnProgramsChanged);
    }

    /// <summary>
    /// Checks whether the current tv program is still running
    /// </summary>
    /// <returns>true if current program is still running, false is it has ended</returns>
    public bool IsRunning
    {
      get
      {
        if(_currentProgram==null) return false;
        if (_currentProgram.IsRunningAt(DateTime.Now)) return true;
        return false;
      }
    }
		
    /// <summary>
    /// Callback we get from the TVDatabase when a program has been added,changed,deleted
    /// </summary>
		void OnProgramsChanged()
		{
			_listPrograms.Clear();
			DateTime dtNow=new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,0,0,0,0);
			TVDatabase.GetPrograms(Utils.datetolong(dtNow),Utils.datetolong(dtNow.AddDays(_days)),ref _listPrograms);
			_lastDateTime=DateTime.Now;
		}

    /// <summary>
    /// Reloads the program list every 4 hours
    /// </summary>
		void CheckForProgramsUpdate()
		{
			// refresh the program list every 4 hours
			TimeSpan ts=DateTime.Now - _lastDateTime;
			if ( ts.TotalHours > 4 ||
					!DateTime.Now.Date.Equals(_lastDateTime.Date) ) 
			{
				OnProgramsChanged();
			}
		}

    /// <summary>
    /// Returns the TVProgram running on the specified tv channel at the specified date/time
    /// </summary>
    /// <param name="strChannel">Name of the TV channel</param>
    /// <param name="dtDateTime">Date and Time</param>
    /// <returns>TVProgram running on the specified TV channel at the specified date/time
    /// or null if theres no information</returns>
    /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
    public TVProgram GetProgramAt(string strChannel, DateTime dtDateTime)
    {
			CheckForProgramsUpdate();

      foreach (TVProgram program in _listPrograms)
      {
				if (program.Channel.Equals(strChannel))
				{
					if (program.IsRunningAt(dtDateTime) )
					{
						return program;
					}
				}
      }
      return null;
    }

    /// <summary>
    /// Returns the current running program for the specified TV channel
    /// </summary>
    /// <param name="strChannel">Name of the tv channel</param>
    /// <returns>Current program running or null if no information</returns>
    /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
    public TVProgram GetCurrentProgram(string strChannel)
    {
			CheckForProgramsUpdate();

      if (strChannel==null) return null;
      if ( !strChannel.Equals(_currentChannel) )
      {
        _currentProgram=null;
        _currentChannel=strChannel;
      }

      if (IsRunning) return _currentProgram;
			_currentProgram=null;

      return GetProgramAt(_currentChannel,DateTime.Now);
		}
		

		#region IDisposable Members

		public void Dispose()
		{
			TVDatabase.OnProgramsChanged -=new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(this.OnProgramsChanged);
			_currentProgram=null;
			_listPrograms.Clear();
			_listPrograms=null;
		}

		#endregion



    public List<TVRecording> GetRecordingTimes(TVRecording rec)
		{
      List<TVRecording> recordings = new List<TVRecording>();
			
			DateTime dtDay=DateTime.Now;
			if (rec.RecType==TVRecording.RecordingType.Once)
			{
				recordings.Add(rec);
				return recordings;
			}

			if (rec.RecType==TVRecording.RecordingType.Daily)
			{
				for (int i=0; i < _days;++i)
				{
					TVRecording recNew= new TVRecording(rec);
					recNew.RecType=TVRecording.RecordingType.Once;
					recNew.Start=Utils.datetolong(new DateTime(dtDay.Year,dtDay.Month,dtDay.Day, rec.StartTime.Hour,rec.StartTime.Minute,0));
					recNew.End  =Utils.datetolong(new DateTime(dtDay.Year,dtDay.Month,dtDay.Day, rec.EndTime.Hour,rec.EndTime.Minute,0));
					recNew.Series=true;
					if (recNew.StartTime>=DateTime.Now)
					{
						if (rec.IsSerieIsCanceled(recNew.StartTime))
							recNew.Canceled=recNew.Start;
						recordings.Add(recNew);
					}
					dtDay=dtDay.AddDays(1);
				}
				return recordings;
			}

			if (rec.RecType==TVRecording.RecordingType.WeekDays)
			{
				for (int i=0; i < _days;++i)
				{
					if (dtDay.DayOfWeek != DayOfWeek.Saturday && dtDay.DayOfWeek != DayOfWeek.Sunday)
					{
						TVRecording recNew= new TVRecording(rec);
						recNew.RecType=TVRecording.RecordingType.Once;
						recNew.Start=Utils.datetolong(new DateTime(dtDay.Year,dtDay.Month,dtDay.Day, rec.StartTime.Hour,rec.StartTime.Minute,0));
						recNew.End  =Utils.datetolong(new DateTime(dtDay.Year,dtDay.Month,dtDay.Day, rec.EndTime.Hour,rec.EndTime.Minute,0));
						recNew.Series=true;
						if (rec.IsSerieIsCanceled(recNew.StartTime))
							recNew.Canceled=recNew.Start;
						if (recNew.StartTime>=DateTime.Now)
						{
							recordings.Add(recNew);
						}
					}
					dtDay=dtDay.AddDays(1);
				}
				return recordings;
			}

            if (rec.RecType == TVRecording.RecordingType.WeekEnds)
            {
                List<TVProgram> progList = new List<TVProgram>();
			 	TVDatabase.SearchMinimalPrograms(Utils.datetolong(dtDay),Utils.datetolong(dtDay.AddDays(_days)),ref progList,3,rec.Title,rec.Channel);
			
			    foreach (TVProgram prog in progList)
			    {
				    if ((rec.IsRecordingProgram(prog,false)) && 
                        (prog.StartTime.DayOfWeek == DayOfWeek.Saturday || prog.StartTime.DayOfWeek == DayOfWeek.Sunday))
                    {
                        TVRecording recNew = new TVRecording(rec);
                        recNew.RecType = TVRecording.RecordingType.Once;
                        recNew.Start = Utils.datetolong(prog.StartTime);
                        recNew.End = Utils.datetolong(prog.EndTime);
                        recNew.Series = true;

                        if (rec.IsSerieIsCanceled(recNew.StartTime))
                            recNew.Canceled = recNew.Start;
                        recordings.Add(recNew);
                    }
                    
                }
                return recordings;
            }
			if (rec.RecType==TVRecording.RecordingType.Weekly)
			{
				for (int i=0; i < _days;++i)
				{
					if (dtDay.DayOfWeek ==rec.StartTime.DayOfWeek)
					{
						TVRecording recNew= new TVRecording(rec);
						recNew.RecType=TVRecording.RecordingType.Once;
						recNew.Start=Utils.datetolong(new DateTime(dtDay.Year,dtDay.Month,dtDay.Day, rec.StartTime.Hour,rec.StartTime.Minute,0));
						recNew.End  =Utils.datetolong(new DateTime(dtDay.Year,dtDay.Month,dtDay.Day, rec.EndTime.Hour,rec.EndTime.Minute,0));
						recNew.Series=true;
						if (rec.IsSerieIsCanceled(recNew.StartTime))
							recNew.Canceled=recNew.Start;
						if (recNew.StartTime>=DateTime.Now)
						{
							recordings.Add(recNew);
						}
					}
					dtDay=dtDay.AddDays(1);
				}
				return recordings;
			}

      List<TVProgram> programs = new List<TVProgram>();
			if (rec.RecType==TVRecording.RecordingType.EveryTimeOnThisChannel)
				TVDatabase.SearchMinimalPrograms(Utils.datetolong(dtDay),Utils.datetolong(dtDay.AddDays(_days)),ref programs,3,rec.Title,rec.Channel);
			else
				TVDatabase.SearchMinimalPrograms(Utils.datetolong(dtDay),Utils.datetolong(dtDay.AddDays(_days)),ref programs,3,rec.Title,String.Empty);
			foreach (TVProgram prog in programs)
			{
				if (rec.IsRecordingProgram(prog,false))
				{
					TVRecording recNew= new TVRecording(rec);
					recNew.RecType=TVRecording.RecordingType.Once;
					recNew.Channel=prog.Channel;
					recNew.Start=Utils.datetolong(prog.StartTime);
					recNew.End=Utils.datetolong(prog.EndTime);
					recNew.Series=true;
					if (rec.IsSerieIsCanceled(recNew.StartTime))
						recNew.Canceled=recNew.Start;
					recordings.Add(recNew);
				}
			}
			return recordings;
			
		}	
	}
}
