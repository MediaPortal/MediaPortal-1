using System;
using System.Collections.Generic;
using System.Text;

namespace WindowPlugins.VideoEditor
{
  /// <summary>
  /// Holds a Timedomain with the starttime, the endtime and the duration
  /// </summary>
  class TimeDomain
  {
    double startTime, endTime, duration;
		TimeSpan startTimeSp, endTimeSp, durationSp;

    public TimeDomain(double startTime, double endTime)
    {
      this.startTime = startTime;
      this.endTime = endTime;
			startTimeSp = new TimeSpan((long)(startTime * 10e6));
			endTimeSp = new TimeSpan((long)(endTime * 10e6));
			durationSp = new TimeSpan();
			if (endTime > startTime)
			{
				duration = endTime - startTime;
				durationSp = new TimeSpan(endTimeSp.Ticks - startTimeSp.Ticks);
			}
			else
			{
				duration = -1;
				//durationSp. = -1;
			}
    }

    public void SetBoth(double startTime, double endTime)
    {
      this.startTime = startTime;
      this.endTime = endTime;
			startTimeSp = new TimeSpan((long)(startTime * 10e6));
			endTimeSp = new TimeSpan((long)(endTime * 10e6));
			durationSp = new TimeSpan();
			if (endTime > startTime)
			{
				duration = endTime - startTime;
				durationSp = new TimeSpan(endTimeSp.Ticks - startTimeSp.Ticks);
			}
			else
			{
				duration = -1;
			}
    }

		public TimeSpan StartTimeSp
		{
			get
			{
				return startTimeSp;
			}
		}

		public TimeSpan EndTimeSp
		{
			get
			{
				return endTimeSp;
			}
		}

		public TimeSpan DurationSp
		{
			get
			{
				return durationSp;
			}
		}

    public double StartTime
    {
      get
      {
        return startTime;
      }
      set
      {
        startTime = value;
      }
    }

    public double EndTime
    {
      get
      {
        return endTime;
      }
      set
      {
        endTime = value;
      }
    }

    /// <summary>
    /// Gets the difference between the starttime and the endtime.
    /// Is the endtime lower than the starttime it gets -1.
    /// </summary>
    public double Duration
    {
      get
      {
        if (endTime > startTime)
          duration = endTime - startTime;
        else
          duration = -1;
        return duration;
      }
    }
  }
}
