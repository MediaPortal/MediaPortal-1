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
    TVProgram		m_currentProgram=null;
    ArrayList		m_programs = new ArrayList();
    string			m_strChannel="";
		DateTime   	m_lastDateTime=DateTime.Now;
		int         m_iDays=1;
    /// <summary>
    /// Constructor. 
    /// The constructor will load all programs from the TVDatabase
    /// </summary>
    ///
		public TVUtil()
		{
			m_iDays=1;
			OnProgramsChanged();
			TVDatabase.OnProgramsChanged +=new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(this.OnProgramsChanged);
		}

    public TVUtil(int days)
		{
			m_iDays=days;
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
        if(m_currentProgram==null) return false;
        if (m_currentProgram.IsRunningAt(DateTime.Now)) return true;
        return false;
      }
    }
		
    /// <summary>
    /// Callback we get from the TVDatabase when a program has been added,changed,deleted
    /// </summary>
		void OnProgramsChanged()
		{
			m_programs.Clear();
			DateTime dtNow=new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,0,0,0,0);
			TVDatabase.GetPrograms(Utils.datetolong(dtNow),Utils.datetolong(dtNow.AddDays(m_iDays)),ref m_programs);
			m_lastDateTime=DateTime.Now;
		}

    /// <summary>
    /// Reloads the program list every 4 hours
    /// </summary>
		void CheckForProgramsUpdate()
		{
			// refresh the program list every 4 hours
			TimeSpan ts=DateTime.Now - m_lastDateTime;
			if ( ts.TotalHours > 4 ||
					!DateTime.Now.Date.Equals(m_lastDateTime.Date) ) 
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

      foreach (TVProgram program in m_programs)
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
      if ( !strChannel.Equals(m_strChannel) )
      {
        m_currentProgram=null;
        m_strChannel=strChannel;
      }

      if (IsRunning) return m_currentProgram;
			m_currentProgram=null;

      return GetProgramAt(m_strChannel,DateTime.Now);
		}
		

		#region IDisposable Members

		public void Dispose()
		{
			TVDatabase.OnProgramsChanged -=new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(this.OnProgramsChanged);
			m_currentProgram=null;
			m_programs.Clear();
			m_programs=null;
		}

		#endregion

	

		public ArrayList GetRecordingTimes(TVRecording rec)
		{
			ArrayList recordings = new ArrayList();
			
			DateTime dtDay=DateTime.Now;
			if (rec.RecType==TVRecording.RecordingType.Once)
			{
				recordings.Add(rec);
				return recordings;
			}

			if (rec.RecType==TVRecording.RecordingType.Daily)
			{
				for (int i=0; i < m_iDays;++i)
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
				for (int i=0; i < m_iDays;++i)
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
			
			if (rec.RecType==TVRecording.RecordingType.Weekly)
			{
				for (int i=0; i < m_iDays;++i)
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

			ArrayList programs=new ArrayList();
			if (rec.RecType==TVRecording.RecordingType.EveryTimeOnThisChannel)
				TVDatabase.SearchMinimalPrograms(Utils.datetolong(dtDay),Utils.datetolong(dtDay.AddDays(m_iDays)),ref programs,3,rec.Title,rec.Channel);
			else
				TVDatabase.SearchMinimalPrograms(Utils.datetolong(dtDay),Utils.datetolong(dtDay.AddDays(m_iDays)),ref programs,3,rec.Title,String.Empty);
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
