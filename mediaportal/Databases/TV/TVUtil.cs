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

    /// <summary>
    /// Constructor. 
    /// The constructor will load all programs from the TVDatabase
    /// </summary>
    public TVUtil()
		{
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
			TVDatabase.GetPrograms(Utils.datetolong(dtNow),Utils.datetolong(dtNow.AddDays(1)),ref m_programs);
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
	}
}
