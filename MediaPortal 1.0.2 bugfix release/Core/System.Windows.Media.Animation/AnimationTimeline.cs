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
  public abstract class AnimationTimeline : Timeline
  {
    #region Constructors

    static AnimationTimeline()
    {
      IsAdditiveProperty = DependencyProperty.Register("IsAdditive", typeof (bool), typeof (AnimationTimeline),
                                                       new PropertyMetadata(false));
      IsCumulativeProperty = DependencyProperty.Register("IsCumulative", typeof (bool), typeof (AnimationTimeline),
                                                         new PropertyMetadata(false));
    }

    protected AnimationTimeline()
    {
    }

    #endregion Constructors

    #region Methods

    protected internal override Clock AllocateClock()
    {
      return CreateClock();
    }

    public new AnimationClock CreateClock()
    {
      return new AnimationClock(this);
    }

    protected override Duration GetNaturalDurationCore(Clock clock)
    {
      // default duration for an animation is 1 second
      return new Duration(1000);
    }

    #endregion Methods

    #region Properties

    public virtual bool IsDestinationDefault
    {
      get { throw new NotImplementedException(); }
    }

    public abstract Type TargetPropertyType { get; }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty IsAdditiveProperty;
    public static readonly DependencyProperty IsCumulativeProperty;

    #endregion Properties (Dependency)
  }
}