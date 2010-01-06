#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#endregion

namespace System.Windows.Media.Animation
{
  public class ParallelTimeline : TimelineGroup
  {
    #region Constructors

    public ParallelTimeline() {}

    public ParallelTimeline(Nullable<TimeSpan> beginTime) : base(beginTime) {}

    public ParallelTimeline(Nullable<TimeSpan> beginTime, Duration duration) : base(beginTime, duration) {}

    public ParallelTimeline(Nullable<TimeSpan> beginTime, Duration duration, RepeatBehavior repeatBehavior)
      : base(beginTime, duration, repeatBehavior) {}

    #endregion Constructors

    #region Methods

    public new ParallelTimeline Copy()
    {
      return (ParallelTimeline)base.Copy();
    }

    protected override Freezable CreateInstanceCore()
    {
      return new ParallelTimeline();
    }

    protected override Duration GetNaturalDurationCore(Clock clock)
    {
      throw new NotImplementedException();
    }

    #endregion Methods
  }
}